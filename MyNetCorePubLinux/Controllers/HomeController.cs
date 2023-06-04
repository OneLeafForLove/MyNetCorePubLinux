using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNetCorePubLinux.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MyUntil;

namespace MyNetCorePubLinux.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Helper _helper;

        public HomeController(ILogger<HomeController> logger, Helper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public IActionResult Index()
        {
            _logger.LogError("LogDebug Index");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Ping()
        {
            var list = _helper.PingSegmentIp();
            return Json(list);
        }

        public FileContentResult GetFile(string path)
        {
            using (var sw = new FileStream("E://项目//FileImag//20191217//11233aa89256ce39f4e378f58f3bd6e3ac537.jpg", FileMode.Open))
            {
                var contenttype = GetContentTypeForFileName("E://项目//FileImag//20191217//11233aa89256ce39f4e378f58f3bd6e3ac537.jpg");
                var bytes = new byte[sw.Length];
                sw.Read(bytes, 0, bytes.Length);
                sw.Close();
                return new FileContentResult(bytes, contenttype);
            }
        }
        /// <summary>
        /// 注册表获取文件类型
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetContentTypeForFileName(string fileName)
        {
            //获取文件后缀
            string ext = Path.GetExtension(fileName);
            using (Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext))
            {
                if (registryKey == null)
                    return null;
                var value = registryKey.GetValue("Content Type");
                return value?.ToString();
            }
        }
        //直接返回文件
        public IActionResult ShowImage()
        {
            return PhysicalFile(@"c:\404.jpg", "image/jpeg");
        }
    }
}
