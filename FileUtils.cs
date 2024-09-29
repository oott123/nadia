using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace nadia;

public static class FileUtils
{
    public static void TakeFileAccess(string path)
    {
        if (!File.Exists(path))
        {
            Log.Debug($"{path} is not exists.");
            return;
        }

        var identity = WindowsIdentity.GetCurrent().User;
        var fileInfo = new FileInfo(path);
        var fileSecurity = new FileSecurity();

        fileSecurity.SetOwner(identity);
        fileInfo.SetAccessControl(fileSecurity);
        fileInfo.Attributes = FileAttributes.Normal;

        var fullControlRule = new FileSystemAccessRule(
            identity,
            FileSystemRights.FullControl,
            AccessControlType.Allow
        );
        fileSecurity.AddAccessRule(fullControlRule);
        fileInfo.SetAccessControl(fileSecurity);
    }

    public static void TakeDirectoryTreeAccess(string path, bool includeFile = false)
    {
        var identity = WindowsIdentity.GetCurrent().User;

        var dirInfo = new DirectoryInfo(path);
        var dirSecurity = new DirectorySecurity();

        dirSecurity.SetOwner(identity);
        dirInfo.SetAccessControl(dirSecurity);

        var fullControlRule = new FileSystemAccessRule(
            identity,
            FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
            PropagationFlags.None,
            AccessControlType.Allow
        );
        dirSecurity.AddAccessRule(fullControlRule);
        dirInfo.SetAccessControl(dirSecurity);
        dirInfo.Attributes = FileAttributes.Normal;

        var children = Directory.EnumerateDirectories(path);
        foreach (var child in children)
        {
            if (Directory.Exists(child))
            {
                TakeDirectoryTreeAccess(child, includeFile);
            }
        }

        if (includeFile)
        {
            var files = Directory.EnumerateFiles(path);
            foreach (var child in files)
            {
                TakeFileAccess(child);
            }
        }
    }

    public static void TakeOwnAndDelete(string dir)
    {
        if (Directory.Exists(dir))
        {
            TakeDirectoryTreeAccess(dir, true);
            Directory.Delete(dir, true);
        }
        else if (File.Exists(dir))
        {
            TakeFileAccess(dir);
            File.Delete(dir);
        }
        else
        {
            Log.Debug($"{dir} is not exists");
        }
    }
}
