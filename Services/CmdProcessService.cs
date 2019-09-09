using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;

namespace AppMaker.Server.Services
{
    public class CmdProcessService : IDisposable
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private const string CmdPath = @"C:\Program Files\nodejs\node.exe";

        private readonly List<Process> _processes = new List<Process>();
        public readonly AutoResetEvent SignalEvent = new AutoResetEvent(false);
        public readonly Queue<string> CmdLog = new Queue<string>();

        public CmdProcessService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        private Process CreateProcess(string arguments)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = CmdPath,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    Arguments = $"{_hostingEnvironment.WebRootPath}\\simple\\node_modules\\@angular\\cli\\bin\\ng " + arguments,
                    WorkingDirectory = $"{_hostingEnvironment.WebRootPath}\\simple"
                }
            };

            process.OutputDataReceived += CaptureOutput;
            process.ErrorDataReceived += CaptureOutput;

            return process;
        }

        private void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            CmdLog.Enqueue($"{e.Data}");
            SignalEvent.Set();
        }

        public void StopProcess()
        {
            lock (_processes)
            {
                _processes.ToList().ForEach(x =>
                {
                    if (x.HasExited) return;

                    x.Kill();
                    x.Close();
                    x.Dispose();

                    _processes.Remove(x);
                });
            }
        }

        public void WriteInput([NotNull] string input)
        {
            var process = CreateProcess(input);
            process.Start();
            process.BeginOutputReadLine();

            Task.Run(() => { process.WaitForExit(); });

            _processes.Add(process);
        }

        public void Dispose()
        {
            StopProcess();
        }
    }
}
