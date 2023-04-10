using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Models
{
    public class PhotoProcessing
    {
        public IFormFile PackingImage { get; set; }
        public string SearchLine { get; set; }
        public string Error { get; set; }
    }
}
