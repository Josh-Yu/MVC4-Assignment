using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Assignment2Basic.Models
{
    public class EditProfile_ViewModel
    {
        public int CustomerID { get; set; }


        [Required(ErrorMessage = "A Customer Name is required")]
        [StringLength(50, ErrorMessage = "Customer name too Long (Max 50 Characters)")]
        [RegularExpression(@"^[A-z]+\s[A-z]+$", ErrorMessage="Name format must be first name and last name (Seperated by a space)")]
        public string CustomerName { get; set; }

        
        [RegularExpression(@"\d{9}$",ErrorMessage="TFN Number must be 9 Digits")]
        public string TFN { get; set; }

        
        [StringLength(50, ErrorMessage = "Address too long (Max 50 Characters)")]
        public string Address { get; set; }

        
        [StringLength(40, ErrorMessage = "City name too long (Max 40 Character")]
        public string City { get; set; }

        
        //[StringLength(20, ErrorMessage = "String too long")]
        [RegularExpression(@"^(NT|WA|SA|VIC|ACT|NSW|QLD|TAS)$",ErrorMessage="Must be a valid 2-3 Letter State Code (uppercase)")]
        public string State { get; set; }

       
        [RegularExpression(@"^\d{4}$",ErrorMessage= "Must be a 4 digit number")]
        public string PostCode { get; set; }

        [Required(ErrorMessage = "A Phone Number is required to be entered")]
        [RegularExpression(@"^\(61\)-\s\d{8}$", ErrorMessage = "Phone number must be of format (61)- XXXXXXXX")]
        public string Phone { get; set; }
    }
}