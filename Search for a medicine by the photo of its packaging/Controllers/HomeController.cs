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

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //private Image image = new Image();

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
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

        [HttpPost]
        public string TextRecognising(Image image)
        {
            //string fileName = null;
            //if (image.PackingImage != null)
            //{
            //    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "Img");
            //    fileName = Guid.NewGuid().ToString() + "-" + image.PackingImage.FileName;
            //    string filePath = Path.Combine(uploadDir, fileName);
            //    using (var fileStream = new FileStream(filePath, FileMode.Create))
            //    {
            //        image.PackingImage.CopyTo(fileStream);
            //    }
            //}
            var img = Tes(image);
            return img;
        }

        public string Tes(Image image)
        {
            var ocrengine = new TesseractEngine(@".\tessdata", "rus+eng", EngineMode.Default);
            var img = Pix.LoadFromFile(image.PackingImage.FileName);
            var res = ocrengine.Process(img);
            image.DD = res.GetText();
            return image.DD;
        }
    }
}
