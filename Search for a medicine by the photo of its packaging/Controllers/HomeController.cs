using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Search_for_a_medicine_by_the_photo_of_its_packaging.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web;
using System.IO;
using Tesseract;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        //private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PhotoProcessing image = new PhotoProcessing();
        private ViewModel viewModel = new ViewModel();
        private DataNames dataNames = new DataNames();

        //public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        //{
        //    _logger = logger;
        //    _webHostEnvironment = webHostEnvironment;
        //}

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

        public ActionResult GetViewModel()
        {
            var db = new ViewModel();
            var photoProcessing = db.PhotoProcessingView;
            var dataNames = db.DataNamesView;
            var model = new ViewModel { PhotoProcessingView = photoProcessing, DataNamesView = dataNames };
            return View(model);
        }

        [HttpPost]
        public string TextRecognising(PhotoProcessing image)
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
                return "Препарат не найден";
            }
        }

        public void Search(PhotoProcessing searchLine)
        {

        }

        public async Task<ActionResult> ApiVidal(ViewModel viewModel)
        {
            using var httpClient = new HttpClient();
            string json;
            var token = "aII4Hhj1EaeQ";
            var path = "Product.json";
            string search = null;
            if (viewModel.PhotoProcessingView.SearchLine != null)
            {
                search = viewModel.PhotoProcessingView.SearchLine;
            }
            else if(viewModel.PhotoProcessingView.PackingImage !=null)
            {
                search = TextRecognising(viewModel.PhotoProcessingView);
            }
            httpClient.DefaultRequestHeaders.Add("X-Token", token);
            json = await httpClient.GetStringAsync("http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + search);
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(json);
            writer.Close();
            var jsonString = System.IO.File.ReadAllText("Product.json");
            viewModel.DataNamesView = DescriptionOfTheDrug(jsonString, dataNames);
            return View("Privacy",viewModel);

            //DataNames dataNames = new DataNames();
            //ViewModel viewModel = new ViewModel();
            //string search = null;
            //if (viewModel.PhotoProcessingView.PackingImage != null)
            //{
            //    search = TextRecognising(viewModel.PhotoProcessingView);
            //}
            //else if (viewModel.PhotoProcessingView.SearchLine != null)
            //{
            //    search = viewModel.PhotoProcessingView.SearchLine;
            //}
            //string json;
            //string token = "aII4Hhj1EaeQ";
            //string path = "Product.json";
            //using var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Add("X-Token", token);
            //json = await httpClient.GetStringAsync(@"http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + search);
            //StreamWriter writer = new StreamWriter(path, false);
            //writer.WriteLine(json);
            //writer.Close();
            //var jsonString = System.IO.File.ReadAllText("Product.json");
            //dataNames = DescriptionOfTheDrug(jsonString, dataNames);
            //viewModel.DataNamesView = dataNames;

            //return View("Privacy", viewModel);
        }

        public async Task<ActionResult> Privacy()
        {
            //DataNames dataNames = new DataNames();
            //ViewModel viewModel = new ViewModel();
            //string search = null;
            //if (viewModel.PhotoProcessingView.PackingImage != null)
            //{
            //    search = TextRecognising(viewModel.PhotoProcessingView);
            //}
            //else if (viewModel.PhotoProcessingView.SearchLine != null)
            //{
            //    search = viewModel.PhotoProcessingView.SearchLine;
            //}
            //string json;
            //string token = "aII4Hhj1EaeQ";
            //string path = "Product.json";
            //using var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Add("X-Token", token);
            //json = await httpClient.GetStringAsync(@"http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + search);
            //StreamWriter writer = new StreamWriter(path, false);
            //writer.WriteLine(json);
            //writer.Close();
            //var jsonString = System.IO.File.ReadAllText("Product.json");
            //dataNames = DescriptionOfTheDrug(jsonString, dataNames);
            //viewModel.DataNamesView = dataNames;

            //return View(viewModel);

            
            using var httpClient = new HttpClient();
            string json;
            string token = "aII4Hhj1EaeQ";
            string path = "Product.json";
            httpClient.DefaultRequestHeaders.Add("X-Token", token);
            json = await httpClient.GetStringAsync(@"http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + viewModel.PhotoProcessingView.SearchLine);
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(json);
            writer.Close();
            var jsonString = System.IO.File.ReadAllText("Product.json");
            dataNames = DescriptionOfTheDrug(jsonString, dataNames);
            viewModel.DataNamesView = dataNames;
            return View(viewModel);
        }



        /// <summary>
        /// Описание препарата
        /// </summary>
        /// <param name="jsonString"></param>
        public DataNames DescriptionOfTheDrug(string jsonString, DataNames dataNames)
        {
            var atcCode = JsonConvert.DeserializeObject<FileJson.Atccode>(jsonString);
            var moleculeName = JsonConvert.DeserializeObject<FileJson.Moleculename>(jsonString);
            var document = JsonConvert.DeserializeObject<FileJson.Document>(jsonString);
            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);

            //Название препарата на русском
            dataNames.NameDrugsRus = rootobject.products[0].rusName;
            //Название препарата на английском
            dataNames.NameDrugsEng = rootobject.products[0].engName;
            //Название вещества
            dataNames.SubstanceName = rootobject.products[0].atcCodes[0].rusName;
            //Название АТХ кода
            dataNames.AtcCode = rootobject.products[0].atcCodes[0].code;
            //Количество активных веществ
            var count = rootobject.products[0].moleculeNames.Length;

            //Массив с названиями активных веществ на английском
            dataNames.NameOfActiveSubstancesEng = new string[count];
            //Массив с названиями активных веществ на русском
            dataNames.NameOfActiveSubstancesRus = new string[count];
            //Название кампании(или что это)
            dataNames.NameCompanyRus = new string[count];
            //Обьяснение кампании(или что это) на русском
            dataNames.NameCompanyEng = new string[count];

            for (int i = 0; i < count; i++)
            {
                dataNames.NameOfActiveSubstancesEng[i] = rootobject.products[0].moleculeNames[i].molecule.latName;
                dataNames.NameOfActiveSubstancesRus[i] = rootobject.products[0].moleculeNames[i].molecule.rusName;
                dataNames.NameCompanyRus[i] = rootobject.products[0].moleculeNames[i].molecule.GNParent.GNParent;
                dataNames.NameCompanyEng[i] = rootobject.products[0].moleculeNames[i].molecule.GNParent.description;
            }

            //Фармакологическое действие
            dataNames.pPharmachologicEffect = HttpUtility.HtmlEncode(rootobject.products[0].document.phInfluence);
            //Показания активных веществ препарата 
            dataNames.IndicationsOfTheActiveSubstancesOfTheDrug = rootobject.products[0].document.indication;
            //Режим дозирования
            dataNames.DosingRegimen = rootobject.products[0].document.dosage;
            //Побочное действие
            dataNames.SideEffect = rootobject.products[0].document.sideEffects;
            //Противопоказания к применению
            dataNames.ContraindicationsForUse = rootobject.products[0].document.contraIndication;
            //Применение при беременности и кормлении грудью
            dataNames.UseDuringPregnancyAndLactation = rootobject.products[0].document.lactation;
            //Применение при нарушениях функции печени
            dataNames.ApplicationForViolationsOfLiverFunction = rootobject.products[0].document.hepatoInsuf;
            //Применение при нарушениях функции почек
            dataNames.ApplicationForViolationsOfKidneyFunction = rootobject.products[0].document.renalInsuf;
            //Применение у детей
            dataNames.UseInChildren = rootobject.products[0].document.childInsuf;
            //Особые указания
            dataNames.SpecialInstructions = rootobject.products[0].document.specialInstruction;
            //Лекарственное взаимодействие
            dataNames.DrugInteraction = rootobject.products[0].document.interaction;

            return dataNames;
        }
    }
}
