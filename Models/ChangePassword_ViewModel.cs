using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Assignment2Basic.Models
{
    public class ChangePassword_ViewModel
    {
        [Required(ErrorMessage="Password Field cannot be blank")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage="Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword",ErrorMessage="Passwords do not match")]
        public string ConfirmPassword { get; set; }

    }
}