using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class HouseholdViewModel
    {
        [Display(Name="Household Name")]
        public string Name { get; set; }

        [Display(Name = "Invitation Code")]
        public string Code { get; set; }
    }
}