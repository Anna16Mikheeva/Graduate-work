using Microsoft.AspNetCore.Mvc;
using Search_for_a_medicine_by_the_photo_of_its_packaging.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Tesseract;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

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
                return "Ошибка";
            }
        }

        //public async Task<IActionResult> Privacy()
        //{
        //    Image image = new Image();
        //    image.AvailabilityOfInformation = true;
        //    string json;
        //    string token = "aII4Hhj1EaeQ";
        //    using var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Add("X-Token", token);
        //    json = await httpClient.GetStringAsync(@"http://www.vidal.ru/api/rest/v1/product/list?filter[name]=Цитрамон");
        //    var jsonString = System.IO.File.ReadAllText("D://Аня//Диплом//Graduate work//Search for a medicine by the photo of its packaging//Product.json");
        //    var company1 = JsonConvert.DeserializeObject<FileJson.Company1>(jsonString);
        //    var company = JsonConvert.DeserializeObject<FileJson.Company>(jsonString);
        //    var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
        //    var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);
        //    image.DD = rootobject.products[1].companies[0].company.name.ToString();
        //    return View(image);
        //}
    }
}
