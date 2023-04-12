using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Search_for_a_medicine_by_the_photo_of_its_packaging.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web;
using System.IO;
using Tesseract;
using System.Collections.Generic;
using ZXing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ZXing.QrCode;
using System.Drawing;
//using System.DrawingCore;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Экземпляр класса PhotoProcessing
        /// </summary>
        private readonly PhotoProcessing _image = new PhotoProcessing();

        /// <summary>
        /// Экземпляр класса ViewModel
        /// </summary>
        private readonly ViewModel _viewModel = new ViewModel();

        /// <summary>
        /// Экземпляр класса DataNames
        /// </summary>
        private readonly DataNames _dataNames = new DataNames();

        //-------------------
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public ActionResult ReadBarCode(IFormCollection formCollection)
        {
            var writer = new QRCodeWriter();
            var resultBit = writer.encode(formCollection["QRCodeString"], BarcodeFormat.QR_CODE, 200, 200);
            var matrix = resultBit;
            var scale = 2;
            Bitmap result = new Bitmap(matrix.Width*scale, matrix.Height*scale);
            for(int i = 0; i < matrix.Height; i++)
            {
                for(int j = 0; j < matrix.Width; j++)
                {
                    Color pixel = matrix[i, j] ? Color.Black : Color.White;
                    for(int x = 0; x < scale; x++)
                    {
                        for (int y = 0; y < scale; y++)
                        {
                            result.SetPixel(i * scale + x, j * scale + y, pixel);
                        }
                    }
                }
            }
            string webRoothPath = _webHostEnvironment.WebRootPath;
            result.Save(webRoothPath + "\\img\\QR.png");
            ViewBag.URL = "\\img\\QR.png";
            return View("BarCode");
        }

        public ActionResult ReadQRCode()
        {
            string webRoothPath = _webHostEnvironment.WebRootPath;
            var path = webRoothPath + "\\img\\Cream qr.jpeg";
            var reader = new BarcodeReaderGeneric();
            Bitmap image = (Bitmap)Image.FromFile(path);
            using(image)
            {
                LuminanceSource source;
                source = new ZXing.Windows.Compatibility.BitmapLuminanceSource(image);
                Result result = reader.Decode(source);
                ViewBag.Text = result.Text;
            }
            return View("BarCode");
        }

        /// <summary>
        /// Возвращает главную страницу Index
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Возвращает старницу Privacy
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Privacy()
        {
            return View();
        }

        /// <summary>
        /// Возвращает страницу BarCode
        /// </summary>
        /// <returns></returns>
        public IActionResult BarCode()
        {
            return View();
        }

        /// <summary>
        /// Возвращает ошибку
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Распознает текст на фотографии
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        [HttpPost]
        public string[] TextRecognising(PhotoProcessing image)
        {
            List<string> toArray = new List<string>();
            var ocrengine = new TesseractEngine(@".\tessdata", "rus+eng", EngineMode.Default);
            var loadFromFile = Pix.LoadFromFile(image.PackingImage.FileName);
            var v = Pix.LoadFromFile(image.PackingImage.FileName);
            var process = ocrengine.Process(loadFromFile);
            var textGhoto = process.GetText();
            textGhoto = textGhoto.Replace("\n", " ");
            string[] words = textGhoto.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] != "" && words[i] != " " && words[i].Length != 0)
                {
                    toArray.Add(words[i]);
                }
            }
            var wordsArray = toArray.ToArray();

            return wordsArray;
        }

        /// <summary>
        /// Обрабатывает запрос и выводит информацию о лекарстве
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<ActionResult> ApiVidal(ViewModel viewModel)
        {
            using var httpClient = new HttpClient();
            string json;
            var token = "aII4Hhj1EaeQ";
            var path = "Product.json";
            string search = null;
            httpClient.DefaultRequestHeaders.Add("X-Token", token);

            if (viewModel.PhotoProcessingView.SearchLine != null && viewModel.PhotoProcessingView.SearchLine != "" && viewModel.PhotoProcessingView.SearchLine != " ")
            {
                try
                {
                    json = await httpClient.GetStringAsync("http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + viewModel.PhotoProcessingView.SearchLine);
                    search = viewModel.PhotoProcessingView.SearchLine;
                    StreamWriter writer1 = new StreamWriter(path, false);
                    writer1.WriteLine(json);
                    writer1.Close();
                    var jsonStrin = System.IO.File.ReadAllText("Product.json");
                    var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonStrin);
                    var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonStrin);
                    if (rootobject.products.Length != 0)
                    {
                        search = viewModel.PhotoProcessingView.SearchLine;
                    }
                    else
                    {
                        viewModel.PhotoProcessingView.Error = "Препарат не найден";
                    }
                }
                catch
                {
                    viewModel.PhotoProcessingView.Error = "Препарат не найден";
                }
            }
            else if (viewModel.PhotoProcessingView.PackingImage != null)
            {
                var s = TextRecognising(viewModel.PhotoProcessingView);
                for (int i = 0; i < s.Length; i++)
                {
                    try
                    {
                        json = await httpClient.GetStringAsync("http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + s[i]);
                        StreamWriter writer1 = new StreamWriter(path, false);
                        writer1.WriteLine(json);
                        writer1.Close();
                        var jsonStrin = System.IO.File.ReadAllText("Product.json");
                        var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonStrin);
                        var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonStrin);
                        if (rootobject.products.Length != 0)
                        {
                            search = s[i];
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            if (search == null)
            {
                viewModel.PhotoProcessingView.Error = "Препарат не найден";
            }

            if (viewModel.PhotoProcessingView.Error != "Препарат не найден")
            {
                json = await httpClient.GetStringAsync("http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + search);
                StreamWriter writer = new StreamWriter(path, false);
                writer.WriteLine(json);
                writer.Close();
                var jsonString = System.IO.File.ReadAllText("Product.json");
                viewModel.DataNamesView = DescriptionOfTheDrug(jsonString, _dataNames);

                return View("Privacy", viewModel);
            }

            return View("Index", viewModel);
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
