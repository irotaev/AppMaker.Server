using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;

namespace AppMaker.Server.Services
{
    public class CmdProcessService : IDisposable
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private const string CmdPath = @"C:\windows\system32\cmd.exe";

        private Process _process;

        public CmdProcessService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            CreateProcess();

            RunProcessTask();
        }

        private Process CreateProcess()
        {
            _process?.Kill();

            _process = new Process
            {
                StartInfo =
                {
                    FileName = CmdPath,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                }
            };

            _process.OutputDataReceived += CaptureOutput;

            return _process;
        }

        private void RunProcessTask()
        {
            if (_process == null) return;

            _process.Start();
            _process.BeginOutputReadLine();

            Task.Run(() =>
            {
                WriteInputAsync($"cd {_hostingEnvironment.WebRootPath}\\simple").Wait();

                _process.WaitForExit();
            });
        }

        private void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Debug.WriteLine($"Received: {e.Data}");
            }
        }

        public void ReloadProcess()
        {
            CreateProcess();

            RunProcessTask();
        }

        public async Task WriteInputAsync([NotNull] string input)
        {
            await _process.StandardInput.WriteLineAsync(input);
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}
