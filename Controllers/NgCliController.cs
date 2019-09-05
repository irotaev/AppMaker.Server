using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppMaker.Server.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("create-component")]
        public async Task<ActionResult> CreateComponent([NotNull] [FromBody] string componentName)
        {
            await _cmdProcessService.WriteInputAsync($"ng generate component {componentName}");

            return Ok();
        }

        [HttpGet("serve")]
        public async Task<ActionResult> Serve()
        {
            _cmdProcessService.ReloadProcess();

            await _cmdProcessService.WriteInputAsync("ng serve");

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

        [HttpGet("ng-console")]
        public async Task NgConsole()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest) return;

            var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            while (socket.State == WebSocketState.Open)
            {
                var outgoing = new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello world!"));
                await socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);

                Task.Delay(5000).Wait();
            }
        }

        public class SaveFileRequest
        {
            public string FileName { get; set; }
            public string FileText { get; set; }
        }
    }
}
