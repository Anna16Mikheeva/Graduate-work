using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Search_for_a_medicine_by_the_photo_of_its_packaging.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Tesseract;
using System.Net.Http;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public string TextRecognising(Image image)
        {
            if (image.PackingImage != null)
            {
                var ocrengine = new TesseractEngine(@".\tessdata", "rus+eng", EngineMode.Default);
                var img = Pix.LoadFromFile(image.PackingImage.FileName);
                var res = ocrengine.Process(img);
                image.DD = res.GetText();
                return image.DD;
            }
            else
            {
                return "Ошибка!";
            }
            Index();
        }

        public async Task<IActionResult> Index()
        {
            string json;
            string token = "aII4Hhj1EaeQ";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Token", token);
                json = await httpClient.GetStringAsync("http://www.vidal.ru/api/rest/v1/product/list?filter[name]=Аспирин&page=1&limit=5");
            }
            return View();
        }
    }
}
