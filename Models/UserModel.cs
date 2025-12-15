using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StayEasePG.Models
{
    public class UserModel
    {
        public int UserID { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public string Gender { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string IDProofType { get; set; }
        public string IDProofNumber { get; set; }
        public string Occupation { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }
    }
}