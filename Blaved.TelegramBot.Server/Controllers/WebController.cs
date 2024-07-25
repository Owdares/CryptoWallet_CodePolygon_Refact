using Blaved.Core.Objects.Models.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bleved.TelegramBot.Server.Controllers
{
    public class WebController : Controller
    {
        private readonly AppConfig _appConfig;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public WebController(IOptions<AppConfig> appConfig, IWebHostEnvironment hostingEnvironment) 
        {
            _appConfig = appConfig.Value;
            _hostingEnvironment = hostingEnvironment;

        }
        //[HttpGet]
        //[Route("MainLogo.jpg")]
        //public async Task<IActionResult> ViewMainLogo()
        //{
        //    byte[] bytesMainLogo = await ImageFileManager.ReadBytesAsync(_appConfig.PathConfiguration.MainLogo);

        //    return File(bytesMainLogo, "image/jpeg");

        //}

        //[HttpGet]
        //[Route("CheckLogo.jpg")]
        //public async Task<IActionResult> ViewCheckLogo()
        //{
        //    byte[] bytesMainLogo = await ImageFileManager.ReadBytesAsync(_appConfig.PathConfiguration.CheckLogo);

        //    return File(bytesMainLogo, "image/jpeg");

        //}

        //[HttpGet]
        //[Route("{*path}")]
        //public async Task<IActionResult> StartPage()
        //{
        //    byte[] bytesMainLogo = await ImageFileManager.ReadBytesAsync(_appConfig.PathConfiguration.MainLogo);

        //    return File(bytesMainLogo, "image/jpeg");
        //}
    }
}
