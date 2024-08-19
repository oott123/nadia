using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ShellProgressBar;

namespace nadia
{
    internal class DismProgressBar : IDisposable
    {
        private ProgressBar? progressBar;
        private bool disposedValue;

        public DismProgressBar(string message)
        {
            progressBar = new ProgressBar(
                100,
                message,
                new ProgressBarOptions()
                {
                    ProgressCharacter = '-',
                    BackgroundCharacter = '.',
                    ForegroundColor = ConsoleColor.Green,
                    BackgroundColor = ConsoleColor.Gray,
                    ProgressBarOnBottom = true,
                }
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    progressBar?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Callback(Microsoft.Dism.DismProgress progress)
        {
            if (progressBar == null)
            {
                return;
            }

            progressBar.AsProgress<float>().Report((float)progress.Current / progress.Total);
            if (progress.Current >= progress.Total)
            {
                progressBar.Dispose();
                progressBar = null;
            }
        }
    }
}
