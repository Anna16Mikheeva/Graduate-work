using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Models
{
    public class Image
    {
        public IFormFile PackingImage { get; set; }
        public string DD { get; set; }
        private bool _availabilityOfInformation;
        public bool AvailabilityOfInformation { get; set; }
    }

    public class FileJson
    {

        public class Rootobject
        {
            public bool success { get; set; }
            public Product[] products { get; set; }
            public Pagination pagination { get; set; }
        }

        public class Pagination
        {
            public int page { get; set; }
            public int limit { get; set; }
            public int pageCount { get; set; }
            public int itemsCount { get; set; }
            public int totalItemsCount { get; set; }
        }

        public class Product
        {
            public object[] infoPages { get; set; }
            public Atccode[] atcCodes { get; set; }
            public Phthgroup[] phthgroups { get; set; }
            public Clphgroup[] ClPhGroups { get; set; }
            public DateTime updatedAt { get; set; }
            public bool isValid { get; set; }
            public string productUrl { get; set; }
            public string[] images { get; set; }
            public Moleculename[] moleculeNames { get; set; }
            public int id { get; set; }
            public string rusName { get; set; }
            public string engName { get; set; }
            public bool nonPrescriptionDrug { get; set; }
            public string registrationDate { get; set; }
            public string dateOfCloseRegistration { get; set; }
            public string registrationNumber { get; set; }
            public string zipInfo { get; set; }
            public string composition { get; set; }
            public string productTypeCode { get; set; }
            public Marketstatus marketStatus { get; set; }
            public string dateOfReRegistration { get; set; }
            public bool gnvls { get; set; }
            public object listPkkn { get; set; }
            public bool strongMeans { get; set; }
            public bool poison { get; set; }
            public Company[] companies { get; set; }
            public Document document { get; set; }
            public object forms { get; set; }
            public object[] productPackages { get; set; }
            public string regPreviousNumber { get; set; }
        }

        public class Marketstatus
        {
            public int id { get; set; }
            public string rusName { get; set; }
        }

        public class Document
        {
            public object storageCondition { get; set; }
            public string storageTime { get; set; }
            public DateTime updatedAt { get; set; }
            public Nozology[] nozologies { get; set; }
            public Clphpointer[] clphPointers { get; set; }
            public int documentId { get; set; }
            public int articleId { get; set; }
            public string yearEdition { get; set; }
            public string phInfluence { get; set; }
            public object phKinetics { get; set; }
            public string dosage { get; set; }
            public object overDosage { get; set; }
            public string interaction { get; set; }
            public string lactation { get; set; }
            public string sideEffects { get; set; }
            public string indication { get; set; }
            public string contraIndication { get; set; }
            public string specialInstruction { get; set; }
            public string pregnancyUsing { get; set; }
            public string nursingUsing { get; set; }
            public string renalInsuf { get; set; }
            public string renalInsufUsing { get; set; }
            public string hepatoInsuf { get; set; }
            public string hepatoInsufUsing { get; set; }
            public object pharmDelivery { get; set; }
            public object elderlyInsuf { get; set; }
            public string elderlyInsufUsing { get; set; }
            public string childInsuf { get; set; }
            public string childInsufUsing { get; set; }
        }

        public class Nozology
        {
            public string code { get; set; }
            public string name { get; set; }
        }

        public class Clphpointer
        {
            public int id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
        }

        public class Atccode
        {
            public string code { get; set; }
            public string rusName { get; set; }
            public string engName { get; set; }
            public string parentCode { get; set; }
        }

        public class Phthgroup
        {
            public string code { get; set; }
        }

        public class Clphgroup
        {
            public string name { get; set; }
        }

        public class Moleculename
        {
            public int id { get; set; }
            public Molecule molecule { get; set; }
            public string rusName { get; set; }
            public string engName { get; set; }
        }

        public class Molecule
        {
            public int id { get; set; }
            public string latName { get; set; }
            public string originalLatName { get; set; }
            public string rusName { get; set; }
            public Gnparent GNParent { get; set; }
            public Document1[] documents { get; set; }
            public Moleculename1[] moleculeNames { get; set; }
        }

        public class Gnparent
        {
            public string GNParent { get; set; }
            public string description { get; set; }
        }

        public class Document1
        {
            public int documentId { get; set; }
            public int moleculeId { get; set; }
        }

        public class Moleculename1
        {
            public int id { get; set; }
            public string rusName { get; set; }
            public string engName { get; set; }
        }

        public class Company
        {
            public bool isRegistrationCertificate { get; set; }
            public bool isManufacturer { get; set; }
            public Company1 company { get; set; }
            public object companyRusNote { get; set; }
        }

        public class Company1
        {
            public string name { get; set; }
            public string GDDBName { get; set; }
            public Country country { get; set; }
        }

        public class Country
        {
            public string code { get; set; }
            public string rusName { get; set; }
        }

    }
}
