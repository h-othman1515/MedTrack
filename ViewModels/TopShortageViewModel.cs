using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class TopShortageViewModel
        {
            public string DrugName { get; set; }
            public string GenericName { get; set; }
            public string Category { get; set; }
            public int PharmaciesShort { get; set; }
            public int GovernoratesAffected { get; set; }
            public int DaysSinceFirstReported { get; set; }
        }
}
