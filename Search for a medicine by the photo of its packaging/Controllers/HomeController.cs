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
        
        private static IWebHostEnvironment _environment;

        private static IFormFile _camera = null;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }
        
        /// <summary>
        /// Возвращает главную страницу Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        public IActionResult Capture()
        {
            return View();
        }

        /// <summary>
        /// Наложение черно-белого фильтра на фото
        /// </summary>
        /// <param name="fileName"></param>
        public void ApplyFilter(string fileName)
        {
            IFilter filter = new Grayscale(0.2125, 0.7154, 0.0721);
            var path = _environment.WebRootPath + "//CameraPhotos//" + fileName;
            Bitmap image = (Bitmap)System.Drawing.Image.FromFile(path);
            //Bitmap newImage = filter.Apply(image);
            //newImage = g.Apply(newImage);
            Bitmap newImage = filter.Apply(image);
            image.Dispose();
            image = null;
            System.IO.File.Delete(path);
            newImage.Save(_environment.WebRootPath + "//CameraPhotos//" + fileName,
                System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public void Capture(string name)
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
                            var filePath =_environment.WebRootPath + "\\CameraPhotos" + $@"\{newFileName}";
                            
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                StoreInFolder(file, filePath);
                            }

                            var imageBytes = System.IO.File.ReadAllBytes(filePath);
                            ApplyFilter(fileName);
                        }

                        _camera = file;
                        
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Сохранение фотографии с камеры
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
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
        /// Распознание штрих-кода
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
            string[] wordsArrayString = textPhoto.Split(' ');
            foreach (var word in wordsArrayString)
            {
                if (word != "" && word != " " && word.Length != 0)
                {
                    toArray.Add(word);
                }
            }
            var wordsArray = toArray.ToArray();

            return wordsArrayString;
        }

        public async Task<string> WriteToFileJson(string path, HttpClient httpClient, ViewModel viewModel)
        {
            var json = await httpClient.GetStringAsync(
                "http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" +
                viewModel.PhotoProcessingView.SearchLine);
            StreamWriter writer = new StreamWriter(path, false);
            await writer.WriteLineAsync(json);
            writer.Close();
            var jsonString = await System.IO.File.ReadAllTextAsync(path);
            return jsonString;
        }

        /// <summary>
        /// Обрабатывает запрос и выводит информацию о лекарстве
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<ActionResult> ApiVidal(ViewModel viewModel)
        {
            var httpClient = new HttpClient();
            var token = "axLGF8mt0kkw";
            var path = "Product.json";
            string jsonString;
            string productName = null;
            httpClient.DefaultRequestHeaders.Add("X-Token", token);

            if (_camera != null)
            {
                viewModel.PhotoProcessingView.PackingImage = _camera;
            }

            if (!string.IsNullOrEmpty(viewModel.PhotoProcessingView.SearchLine) &&
                viewModel.PhotoProcessingView.SearchLine != " ")
            {
                try
                {
                    //json = await httpClient.GetStringAsync(
                    //    "http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" +
                    //    viewModel.PhotoProcessingView.SearchLine);
                    //productName = viewModel.PhotoProcessingView.SearchLine;
                    //StreamWriter writer = new StreamWriter(path, false);
                    //await writer.WriteLineAsync(json);
                    //writer.Close();
                    //var jsonString = await System.IO.File.ReadAllTextAsync(path);
                    productName = viewModel.PhotoProcessingView.SearchLine;
                    jsonString = await WriteToFileJson(path, httpClient, viewModel);
                    var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
                    var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);
                    if (rootobject != null && rootobject.products.Length != 0)
                    {
                        productName = viewModel.PhotoProcessingView.SearchLine;
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
                var file = viewModel.PhotoProcessingView.PackingImage;
                var fileName = file.FileName;

                if (System.IO.File.Exists(fileName) != false)
                {
                    string filePath = Path.Combine(_environment.WebRootPath + "\\CameraPhotos\\", fileName);
                    var fileStream = System.IO.File.Create(filePath);
                    viewModel.PhotoProcessingView.PackingImage.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }

                var barCode = ReadBarCode(viewModel.PhotoProcessingView);
                if (barCode == "null")
                {
                    var textRecognising = TextRecognising(viewModel.PhotoProcessingView);
                    foreach (var search in textRecognising)
                    {
                        try
                        {
                            //json = await httpClient.GetStringAsync(
                            //    "http://www.vidal.ru/api/rest/v1/product/list?filter[name]=" + search);
                            //StreamWriter writer = new StreamWriter(path, false);
                            //await writer.WriteLineAsync(json);
                            //writer.Close();
                            //var jsonString = await System.IO.File.ReadAllTextAsync(path);
                            jsonString = await WriteToFileJson(path, httpClient, viewModel);
                            var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
                            var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);
                            if (rootobject != null && rootobject.products.Length != 0)
                            {
                                productName = search;
                                break;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                else
                {
                    try
                    {
                        var searchBarCode = ReadBarCode(viewModel.PhotoProcessingView);
                        //json = await httpClient.GetStringAsync(
                        //    "http://www.vidal.ru/api/rest/v1/product/list?filter[barCode]=" + s);
                        //productName = s;
                        //StreamWriter writer = new StreamWriter(path, false);
                        //await writer.WriteLineAsync(json);
                        //writer.Close();
                        //var jsonString = await System.IO.File.ReadAllTextAsync(path);
                        jsonString = await WriteToFileJson(path, httpClient, viewModel);
                        var product = JsonConvert.DeserializeObject<FileJson.Product>(jsonString);
                        var rootobject = JsonConvert.DeserializeObject<FileJson.Rootobject>(jsonString);
                        if (rootobject != null && rootobject.products.Length != 0)
                        {
                            productName = searchBarCode;
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

            if (productName == null)
            {
                viewModel.PhotoProcessingView.Error = "Препарат не найден";
            }

            if (viewModel.PhotoProcessingView.Error != "Препарат не найден")
            {
                jsonString = await System.IO.File.ReadAllTextAsync(path);
                viewModel.DataNamesView = DescriptionOfTheDrug(jsonString, _dataNames);

                return View("Index", viewModel);
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
            if (rootobject != null)
            {
                dataNames.NameDrugsRus = rootobject.products[0].rusName;
                //Название препарата на английском
                dataNames.NameDrugsEng = rootobject.products[0].engName;
                if (rootobject.products[0].atcCodes.Length != 0)
                {
                    //Название вещества
                    dataNames.SubstanceName = rootobject.products[0].atcCodes[0].rusName;
                    //Название АТХ кода
                    dataNames.AtcCode = rootobject.products[0].atcCodes[0].code;
                }

                //Количество активных веществ
                var count = rootobject.products[0].moleculeNames.Length;

                //Массив с названиями активных веществ на английском
                dataNames.NameOfActiveSubstancesEng = new string[count];
                //Массив с названиями активных веществ на русском
                dataNames.NameOfActiveSubstancesRus = new string[count];
                //Название кампании(или что это)
                dataNames.NameCompanyRus = new string[count];
                //Объяснение кампании(или что это) на русском
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
            }

            return dataNames;
        }
    }
}
