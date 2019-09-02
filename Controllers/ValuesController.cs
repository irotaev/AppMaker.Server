using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace AppMakerServer.Controllers
{
    [EnableCors("AllowCors")]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ValuesController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("generate")]
        public ActionResult<string> Get(int id)
        {
            System.Diagnostics.ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo();
            proc.FileName = @"C:\windows\system32\cmd.exe";
            proc.Arguments = $@"""cd {Directory.GetCurrentDirectory()}""";
            System.Diagnostics.Process.Start(proc);

            return "OK!";
        }

        // POST api/values
        [HttpGet("get-file")]
        public ActionResult<string> GetFile()
        {
            var text = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "simple\\src\\app\\app.component.html"));

            return Ok(text);
        }

        // POST api/values
        [HttpPost("save-file")]
        public ActionResult SaveFile([FromBody] string text)
        {
            System.IO.File.WriteAllText(Path.Combine(_hostingEnvironment.WebRootPath, "simple\\src\\app\\app.component.html"), text);

            return Ok();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
