using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Models
{
    public class FileJson
    {
        //+
        public class Rootobject
        {
            public Product[] products { get; set; }//+
        }

        //+
        public class Product
        {
            public Atccode[] atcCodes { get; set; }//+
            public Moleculename[] moleculeNames { get; set; }//+
            public string rusName { get; set; }//+
            public string engName { get; set; }//+
            public string zipInfo { get; set; }//+
            public Document document { get; set; }//+
        }

        public class Document
        {
            public string phInfluence { get; set; }//+
            public string dosage { get; set; }//+
            public string interaction { get; set; }//+
            public string lactation { get; set; }//+
            public string sideEffects { get; set; }//+
            public string indication { get; set; }//+
            public string contraIndication { get; set; }//+
            public string specialInstruction { get; set; }//+
            public string renalInsuf { get; set; }//+
            public string hepatoInsuf { get; set; }//+
            public string childInsuf { get; set; }//+
        }

        //+
        public class Atccode
        {
            public string code { get; set; }//+
            public string rusName { get; set; }//+
        }

        //+
        public class Moleculename
        {
            public Molecule molecule { get; set; }//+
        }

        //+
        public class Molecule
        {
            public string latName { get; set; }//+
            public string rusName { get; set; }//+
            public Gnparent GNParent { get; set; }//+
        }

        //+
        public class Gnparent
        {
            public string GNParent { get; set; }//+
            public string description { get; set; }//+
        }
    }
}
