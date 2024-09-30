using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Nadia
{
    public class Aria2Downloader
    {
        public class Aria2Status
        {
            public string status;
            public long totalLength;
            public long completedLength;
            public int errorCode;
            public string errorMessage;
            public long verifiedLength;
            public int downloadSpeed;
            public bool verifyIntegrityPending;
        }

        private static readonly Lazy<Aria2Downloader> _lazy = new Lazy<Aria2Downloader>(
            () => new Aria2Downloader()
        );
        public static Aria2Downloader Instance => _lazy.Value;

        private int _port;
        private Process? _process;
        private string _secret;

        Aria2Downloader()
        {
            _port = FreeTcpPort();
            _secret = Guid.NewGuid().ToString();
            StartProcess();
        }

        ~Aria2Downloader()
        {
            KillProcess();
        }

        private void StartProcess()
        {
            var aria2 = Path.Combine(
                Assembly.GetExecutingAssembly().Location,
                "..",
                "Tools",
                "aria2c.exe"
            );
            var startInfo = new ProcessStartInfo
            {
                Arguments =
                    $"--enable-rpc --rpc-listen-port={_port} --rpc-secret={_secret} -j 10 -x 10 -s 10 --stop-with-process={Environment.ProcessId} --seed-time=5",
                FileName = aria2,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };
            _process = Process.Start(startInfo);
            try
            {
                ChildProcessTracker.AddProcess(_process);
            }
            catch
            {
                // do nothing
            }
        }

        public void EnsureProcess()
        {
            if (_process == null || _process.HasExited)
            {
                StartProcess();
            }
        }

        public void KillProcess()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
                _process = null;
            }
        }

        public async Task<string> AddUrl(
            string[] url,
            string dir,
            string filename,
            string hash,
            int connections
        )
        {
            EnsureProcess();

            var args = new Dictionary<string, object>();
            args["file-allocation"] = "falloc";
            args["continue"] = "true";
            args["max-concurrent-downloads"] = 10;
            args["max-connection-per-server"] = connections;
            args["split"] = 100;
            args["min-split-size"] = "1M";
            args["async-dns"] = "false";
            args["lowest-speed-limit"] = "1K";

            try
            {
                var myRequest = WebRequest.CreateHttp(url[0]);
                var proxy = myRequest.Proxy;
                if (proxy != null)
                {
                    var myUrl = new Uri(url[0]);
                    var proxyUrl = proxy.GetProxy(myUrl);
                    if (proxyUrl != myUrl && proxyUrl != null)
                    {
                        var proxyString = proxyUrl.ToString();
                        Log.Debug("setting proxy for downloader {proxyUrl}", proxyUrl);
                        args["all-proxy"] = proxyUrl;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "failed to set download proxy");
            }

            args["out"] = filename;
            args["dir"] = dir;
            args["checksum"] = $"sha-256={hash}";
            args["check-integrity"] = "true";

            var result = await JsonRpc<JObject>("aria2.addUri", url, args);
            return result["result"].ToString();
        }

        public async Task<Aria2Status> GetStatus(string gid)
        {
            var result = await JsonRpc<JObject>("aria2.tellStatus", gid);
            return result["result"].ToObject<Aria2Status>();
        }

        public async Task<T> JsonRpc<T>(string method, params object[] paras)
        {
            var list = new List<object>(paras);
            list.Insert(0, $"token:{_secret}");

            var payload = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method,
                @params = list,
            };

            using var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var jSettings = new JsonSerializerSettings()
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
            };
            var json = JsonConvert.SerializeObject(payload, jSettings);
            try
            {
                client.Headers.Add("Content-Type", "application/json");
                var result = await client.UploadStringTaskAsync(
                    $"http://127.0.0.1:{_port}/jsonrpc",
                    "POST",
                    json
                );
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Failed while connect to aria2");
                }
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var sw = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        var result = sw.ReadToEnd();
                        Log.Error(result);
                    }
                }
                throw;
            }
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        /// <summary>
        /// Allows processes to be automatically killed if this parent process unexpectedly quits.
        /// This feature requires Windows 8 or greater. On Windows 7, nothing is done.</summary>
        /// <remarks>References:
        ///  https://stackoverflow.com/a/4657392/386091
        ///  https://stackoverflow.com/a/9164742/386091 </remarks>
        public static class ChildProcessTracker
        {
            /// <summary>
            /// Add the process to be tracked. If our current process is killed, the child processes
            /// that we are tracking will be automatically killed, too. If the child process terminates
            /// first, that's fine, too.</summary>
            /// <param name="process"></param>
            public static void AddProcess(Process process)
            {
                if (s_jobHandle != IntPtr.Zero)
                {
                    bool success = AssignProcessToJobObject(s_jobHandle, process.Handle);
                    if (!success && !process.HasExited)
                        throw new Win32Exception();
                }
            }

            static ChildProcessTracker()
            {
                // This feature requires Windows 8 or later. To support Windows 7 requires
                //  registry settings to be added if you are using Visual Studio plus an
                //  app.manifest change.
                //  https://stackoverflow.com/a/4232259/386091
                //  https://stackoverflow.com/a/9507862/386091
                //if (Environment.OSVersion.Version < new Version(6, 2))
                //    return;

                // The job name is optional (and can be null) but it helps with diagnostics.
                //  If it's not null, it has to be unique. Use SysInternals' Handle command-line
                //  utility: handle -a ChildProcessTracker
                string jobName = "ChildProcessTracker" + Process.GetCurrentProcess().Id;
                s_jobHandle = CreateJobObject(IntPtr.Zero, jobName);

                var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION();

                // This is the key flag. When our process is killed, Windows will automatically
                //  close the job handle, and when that happens, we want the child processes to
                //  be killed, too.
                info.LimitFlags = JOBOBJECTLIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

                var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                extendedInfo.BasicLimitInformation = info;

                int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
                try
                {
                    Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                    if (
                        !SetInformationJobObject(
                            s_jobHandle,
                            JobObjectInfoType.ExtendedLimitInformation,
                            extendedInfoPtr,
                            (uint)length
                        )
                    )
                    {
                        throw new Win32Exception();
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(extendedInfoPtr);
                }
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string name);

            [DllImport("kernel32.dll")]
            static extern bool SetInformationJobObject(
                IntPtr job,
                JobObjectInfoType infoType,
                IntPtr lpJobObjectInfo,
                uint cbJobObjectInfoLength
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

            // Windows will automatically close any open job handles when our process terminates.
            //  This can be verified by using SysInternals' Handle utility. When the job handle
            //  is closed, the child processes will be killed.
            private static readonly IntPtr s_jobHandle;
        }

        public enum JobObjectInfoType
        {
            AssociateCompletionPortInformation = 7,
            BasicLimitInformation = 2,
            BasicUIRestrictions = 4,
            EndOfJobTimeInformation = 6,
            ExtendedLimitInformation = 9,
            SecurityLimitInformation = 5,
            GroupInformation = 11,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public Int64 PerProcessUserTimeLimit;
            public Int64 PerJobUserTimeLimit;
            public JOBOBJECTLIMIT LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public UInt32 ActiveProcessLimit;
            public Int64 Affinity;
            public UInt32 PriorityClass;
            public UInt32 SchedulingClass;
        }

        [Flags]
        public enum JOBOBJECTLIMIT : uint
        {
            JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IO_COUNTERS
        {
            public UInt64 ReadOperationCount;
            public UInt64 WriteOperationCount;
            public UInt64 OtherOperationCount;
            public UInt64 ReadTransferCount;
            public UInt64 WriteTransferCount;
            public UInt64 OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }
    }
}
