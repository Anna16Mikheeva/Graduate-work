using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search_for_a_medicine_by_the_photo_of_its_packaging.Models
{
    /// <summary>
    /// Данные из json файла
    /// </summary>
    public class DataNames
    {
        public DataNames()
        {

        }

        /// <summary>
        /// Название препарата на русском
        /// </summary>
        public string NameDrugsRus { get; set; }
        /// <summary>
        /// Название препарата на английском
        /// </summary>
        public string NameDrugsEng { get; set; }

        /// <summary>
        /// Название вещества
        /// </summary>
        public string SubstanceName { get; set; }

        /// <summary>
        /// АТХ код
        /// </summary>
        public string AtcCode { get; set; }

        /// <summary>
        /// Название активных веществ на русском
        /// </summary>
        public string[] NameOfActiveSubstancesRus { get; set; }

        /// <summary>
        /// Название активных веществ на английском
        /// </summary>
        public string[] NameOfActiveSubstancesEng { get; set; }

        /// <summary>
        /// Название кампании на русском
        /// </summary>
        public string[] NameCompanyRus { get; set; }

        /// <summary>
        /// Название кампании на английском
        /// </summary>
        public string[] NameCompanyEng { get; set; }

        /// <summary>
        /// Фармакологическое действие
        /// </summary>
        public string pPharmachologicEffect { get; set; }

        /// <summary>
        /// Показания активных веществ препарата
        /// </summary>
        public string IndicationsOfTheActiveSubstancesOfTheDrug { get; set; }

        /// <summary>
        /// Режим дозирования
        /// </summary>
        public string DosingRegimen { get; set; }

        /// <summary>
        /// Передозировка
        /// </summary>
        public string OverDosage { get; set; }

        /// <summary>
        /// Побочное действие
        /// </summary>
        public string SideEffect { get; set; }

        /// <summary>
        /// Противопоказания к применению
        /// </summary>
        public string ContraindicationsForUse { get; set; }

        /// <summary>
        /// Применение при беременности и кормлении грудью
        /// </summary>
        public string UseDuringPregnancyAndLactation { get; set; }

        /// <summary>
        /// Применение при нарушениях функции печени
        /// </summary>
        public string ApplicationForViolationsOfLiverFunction { get; set; }

        /// <summary>
        /// Применение при нарушениях функции почек
        /// </summary>
        public string ApplicationForViolationsOfKidneyFunction { get; set; }

        /// <summary>
        /// Применение у детей
        /// </summary>
        public string UseInChildren { get; set; }

        /// <summary>
        /// Особые указания
        /// </summary>
        public string SpecialInstructions { get; set; }

        /// <summary>
        /// Лекарственное взаимодействие
        /// </summary>
        public string DrugInteraction { get; set; }
    }
}
