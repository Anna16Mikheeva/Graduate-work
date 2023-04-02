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
using Newtonsoft.Json;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Image image = new Image();

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

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
                var loadFromFile = Pix.LoadFromFile(image.PackingImage.FileName);
                var process = ocrengine.Process(loadFromFile);
                var textGhoto = process.GetText();
                return textGhoto;
            }
            else
            {
                return "Ошибка!";
            }
        }

        public async Task<string> Privacy()
        {
            image.AvailabilityOfInformation = true;
            string json;
            string token = "aII4Hhj1EaeQ";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Token", token);
            //json = await httpClient.GetStringAsync(@"http://www.vidal.ru/api/rest/v1/product/list?filter[name]=Цитрамон");
            var jsonString = System.IO.File.ReadAllText("D://Аня//Диплом//Graduate work//Search for a medicine by the photo of its packaging//Product.json");
            var country = JsonConvert.DeserializeObject<FileJson.Country>(jsonString);
            var company1 = JsonConvert.DeserializeObject<FileJson.Company1>(jsonString);
            var company = JsonConvert.DeserializeObject<FileJson.Company>(jsonString);
            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);

            var rusName = rootobject.products[0].companies[0].company.name.ToString();//нужно ли?
            var engName = rootobject.products[0].companies[0].company.GDDBName.ToString();//нужно ли?
            var countryRusName = rootobject.products[1].companies[0].company.country.rusName.ToString();//нужно ли?
            DrugName(jsonString);
            CodeATC(jsonString);
            ActiveSubstances(jsonString);
            DosageForm(jsonString);
            PharmachologicEffect(jsonString);
            return jsonString;
        }

        public void DrugName(string jsonString)
        {
            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);

            var rusName = rootobject.products[0].rusName;
            var engName = rootobject.products[0].engName;
        }

        public void CodeATC(string jsonString)
        {
            var atcCode = JsonConvert.DeserializeObject<FileJson.Atccode>(jsonString);
            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);

            var rusName = rootobject.products[0].atcCodes[0].rusName;
            var code = rootobject.products[0].atcCodes[0].code;
        }

        public void ActiveSubstances(string jsonString)
        {
            var moleculeName = JsonConvert.DeserializeObject<FileJson.Moleculename>(jsonString);
            var gnparent = JsonConvert.DeserializeObject<FileJson.Gnparent>(jsonString);
            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);

            var count = rootobject.products[0].moleculeNames.Length;
            string[] latName = new string[count];
            string[] rusName = new string[count];
            string[] GNParent = new string[count];
            string[] description = new string[count];

            for (int i = 0; i < count; i++)
            {
                latName[i] = rootobject.products[0].moleculeNames[i].molecule.latName;
                rusName[i] = rootobject.products[0].moleculeNames[i].molecule.rusName;
                GNParent[i] = rootobject.products[0].moleculeNames[i].molecule.GNParent.GNParent;
                description[i] = rootobject.products[0].moleculeNames[i].molecule.GNParent.description;
            }
        }

        public void DosageForm(string jsonString)
        {
            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);

            var zipInfo = rootobject.products[0].zipInfo;
            var registrationNumber = rootobject.products[0].registrationNumber;//надо ли?
            var registrationDate = rootobject.products[0].registrationDate;//надо ли?
        }

        public void PharmachologicEffect(string jsonString)
        {
            var document = JsonConvert.DeserializeObject<FileJson.Document>(jsonString);
            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);

            var phInfluence = rootobject.products[0].document.phInfluence;//Фармакологическое действие
            var indication = rootobject.products[0].document.indication;//Показания активных веществ препарата 
            var dosage = rootobject.products[0].document.dosage;//Режим дозирования
            var sideEffects = rootobject.products[0].document.sideEffects;//Побочное действие
            var contraIndication = rootobject.products[0].document.contraIndication;//Противопоказания к применению
            var lactation = rootobject.products[0].document.lactation;//Применение при беременности и кормлении грудью
            var hepatoInsuf = rootobject.products[0].document.hepatoInsuf;//Применение при нарушениях функции печени
            var renalInsuf = rootobject.products[0].document.renalInsuf;//Применение при нарушениях функции почек
            var childInsuf = rootobject.products[0].document.childInsuf;//Применение у детей
            var specialInstruction = rootobject.products[0].document.specialInstruction;//Особые указания
            var interaction = rootobject.products[0].document.interaction;//Лекарственное взаимодействие
        }
        }
    }
