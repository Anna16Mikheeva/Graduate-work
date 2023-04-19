using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Search_for_a_medicine_by_the_photo_of_its_packaging.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Tesseract;
using ZXing.Windows.Compatibility;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

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
        
        private readonly IWebHostEnvironment _environment;

        private static IFormFile _camera = null;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Capture()
        {
            return View();
        }

        public void SharpenPhoto(string fileName)
        {
            IFilter filter = new Grayscale(0.2125, 0.7154, 0.0721);
            var path = "D://Аня//Диплом//Graduate work//" +
                       "Search for a medicine by the photo of its packaging" +
                       "//wwwroot//CameraPhotos//webcam.jpg";
            Bitmap image = (Bitmap)System.Drawing.Image.FromFile(path);
            //Bitmap newImage = filter.Apply(image);
            //newImage = g.Apply(newImage);
            Bitmap newImage = filter.Apply(image);
            image.Dispose();
            image = null;
            System.IO.File.Delete(path);
            newImage.Save("D://Аня//Диплом//Graduate work//" +
                          "Search for a medicine by the photo of its packaging" +
                          "//wwwroot//CameraPhotos//webcam.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        [HttpPost]
        public IActionResult Capture(string name)
        {

            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = file.FileName;
                            var myUniqueFileName = "webcam";
                            var fileExtension = Path.GetExtension(fileName);
                            var newFileName = string.Concat(myUniqueFileName, fileExtension);
                            var filePath = Path.Combine(_environment.WebRootPath, "CameraPhotos") + $@"\{newFileName}";
                            
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                StoreInFolder(file, filePath);
                            }

                            var imageBytes = System.IO.File.ReadAllBytes(filePath);
                            SharpenPhoto(filePath);
                        }

                        _camera = file;
                        
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return View("Index"); 
        }

        private void StoreInFolder(IFormFile file, string fileName)
        {
            FileStream fs;
            if (System.IO.File.Exists(fileName) == false)
            {
                fs = System.IO.File.Create(fileName);
                file.CopyTo(fs);
                fs.Flush();
                fs.Close();
            }
            else
            {
                System.IO.File.Delete(fileName);
                fs = System.IO.File.Create(fileName);
                file.CopyTo(fs);
                fs.Flush();
                fs.Close();
            }
        }

        /// <summary>
        /// Распознает штрих-код
        /// </summary>
        /// <param name="formCollection"></param>
        /// <returns></returns>
        [HttpPost]
        public string ReadBarCode(PhotoProcessing photo)
        {
            string webRoothPath = _environment.WebRootPath;
            var path = webRoothPath + "\\CameraPhotos\\" + photo.PackingImage.FileName;
            Bitmap image = (Bitmap)System.Drawing.Image.FromFile(path);
            BarcodeReader reader = new BarcodeReader();
            var result = reader.Decode(image);
            image.Dispose();
            image = null;
            if (result == null)
            {
                return "null";
            }
            return result.Text;
        }

        /// <summary>
        /// Возвращает главную страницу Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
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
            var loadFromFile =
                Pix.LoadFromFile(_environment.WebRootPath + "\\CameraPhotos\\" + image.PackingImage.FileName);
            var process = ocrengine.Process(loadFromFile);
            var textPhoto = process.GetText();
            loadFromFile.Dispose();
            loadFromFile = null;
            loadFromFile = null;
            textPhoto = textPhoto.Replace("\n", " ");
            string[] words = textPhoto.Split(' ');
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
            if (_camera != null)
            {
                viewModel.PhotoProcessingView.PackingImage = _camera;
                _camera = null;
            }

            if (!string.IsNullOrEmpty(viewModel.PhotoProcessingView.SearchLine) &&
                viewModel.PhotoProcessingView.SearchLine != " ")
            {
                try
                {
                    json = await httpClient.GetStringAsync(
                        "http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" +
                        viewModel.PhotoProcessingView.SearchLine);
                    search = viewModel.PhotoProcessingView.SearchLine;
                    StreamWriter writer1 = new StreamWriter(path, false);
                    await writer1.WriteLineAsync(json);
                    writer1.Close();
                    var jsonString = await System.IO.File.ReadAllTextAsync(path);
                    var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
                    var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);
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
                var f = ReadBarCode(viewModel.PhotoProcessingView);
                if (ReadBarCode(viewModel.PhotoProcessingView) == "null")
                {
                    var s = TextRecognising(viewModel.PhotoProcessingView);
                    for (var i = 0; i < s.Length; i++)
                    {
                        try
                        {
                            json = await httpClient.GetStringAsync(
                                "http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + s[i]);
                            StreamWriter writer1 = new StreamWriter(path, false);
                            await writer1.WriteLineAsync(json);
                            writer1.Close();
                            var jsonString = await System.IO.File.ReadAllTextAsync(path);
                            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
                            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);
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
                else
                {
                    try
                    {
                        var s = ReadBarCode(viewModel.PhotoProcessingView);
                        json = await httpClient.GetStringAsync(
                            "http://www.vidal.ru/api/rest/v1/product/list?filter[barCode]=" + s);
                        search = s;
                        StreamWriter writer1 = new StreamWriter(path, false);
                        await writer1.WriteLineAsync(json);
                        writer1.Close();
                        var jsonString = await System.IO.File.ReadAllTextAsync(path);
                        var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
                        var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);
                        if (rootobject.products.Length != 0)
                        {
                            search = s;
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
            }

            if (search == null)
            {
                viewModel.PhotoProcessingView.Error = "Препарат не найден";

            }

            if (viewModel.PhotoProcessingView.Error != "Препарат не найден")
            {
                var jsonString = await System.IO.File.ReadAllTextAsync(path);
                viewModel.DataNamesView = DescriptionOfTheDrug(jsonString, _dataNames);

                return View("Privacy", viewModel);
            }

            return View("Privacy", viewModel);
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
            dataNames.PharmachologicEffect = rootobject.products[0].document.phInfluence;
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
