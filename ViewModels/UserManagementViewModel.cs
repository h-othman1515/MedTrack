using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class UserManagementViewModel
        {
            public List<UserListViewModel> Users { get; set; } = new();
        }
}
