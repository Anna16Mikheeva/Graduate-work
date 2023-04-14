using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebCamExample.Controllers
{
    public class CameraController : Controller
    {
        public readonly IWebHostEnvironment _environment;

        public CameraController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public IActionResult Capture()
        {
            return View();
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
                            var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                            var fileExtension = Path.GetExtension(fileName);
                            var newFileName = string.Concat(myUniqueFileName, fileExtension);
                            var filePath = Path.Combine("D://Аня//Диплом//Graduate work//" +
                                                        "Search for a medicine by the photo of its " +
                                                        "packaging//Search for a medicine by the photo " +
                                                        "of its packaging.csproj//wwwroot", "CameraPhotos") 
                                           + $@"\{newFileName}";
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                StoreInFolder(file, filePath);
                            }

                            var imageBytes = System.IO.File.ReadAllBytes(filePath);
                            //if (imageBytes != null)
                            //{
                            //    StoreInDatabase(imageBytes);
                            //}
                        }
                    }

                    return Json(true);
                }
                else
                {
                    return Json(true);
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void StoreInFolder(IFormFile file, string fileName)
        {
            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }
    }
}
