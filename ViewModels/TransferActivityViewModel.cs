using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class TransferActivityViewModel
        {
            public string Month { get; set; }
            public int TransfersCompleted { get; set; }
            public decimal ValueTransferred { get; set; }
            public int ItemsSaved { get; set; }
        }
}
