using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Models
{
    public class Login_ViewModel
    {
        [Required]
        [DisplayName("Username")]
        public string UserID { get; set; }
        [Required]
        [DisplayName("Password")]
        public string Password { get; set; }
    }
}