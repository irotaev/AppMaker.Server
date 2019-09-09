using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppMaker.Server.Model;
using AppMaker.Server.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMaker.Server.Controllers
{
    [EnableCors("AllowCors")]
    [Route("ngcli")]
    [ApiController]
    public class NgCliController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly CmdProcessService _cmdProcessService;

        public NgCliController(
            IHostingEnvironment hostingEnvironment,
            CmdProcessService cmdProcessService)
        {
            _hostingEnvironment = hostingEnvironment;
            _cmdProcessService = cmdProcessService;
        }

        [HttpGet("reload")]
        public async Task<ActionResult> Reload()
        {
            _cmdProcessService.StopProcess();

            return Ok();
        }

        [HttpGet("get-file-text")]
        public ActionResult<string> GetFileText([NotNull] [FromQuery] string fileName)
        {
            var text = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, $"simple\\src\\app\\{fileName}"));

            return Ok(text);
        }

        [HttpPost("save-file-text")]
        public ActionResult SaveFileText([NotNull] [FromBody] SaveFileRequest request)
        {
            System.IO.File.WriteAllText(Path.Combine(_hostingEnvironment.WebRootPath, $"simple\\src\\app\\{request.FileName}"), request.FileText);

            return Ok();
        }

        [HttpPost("ng-console-write")]
        public void NgConsoleWrite([NotNull] [FromBody] NgConsoleWriteRequest request)
        {
            _cmdProcessService.WriteInput(request.Command);
        }

        [HttpGet("ng-console-read")]
        public async Task NgConsoleRead()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest) return;

            var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            while (socket.State == WebSocketState.Open)
            {
                _cmdProcessService.SignalEvent.WaitOne();

                while (_cmdProcessService.CmdLog.TryDequeue(out var logStr))
                {
                    if (string.IsNullOrWhiteSpace(logStr)) continue;

                    var outgoing = new ArraySegment<byte>(Encoding.UTF8.GetBytes(logStr));
                    await socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public class SaveFileRequest
        {
            public string FileName { get; set; }
            public string FileText { get; set; }
        }
    }
}
