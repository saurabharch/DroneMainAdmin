using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DroneMainAdmin.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public string ConfirmPassword { get; set; } 
    }
    public class UserMetadata
    {
        
        [Display(Name = "First Name")]
        [RegularExpression(@"^[a-zA-Z'.\s]{1,40}$", ErrorMessage = "Special Characters not allowed")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name Required")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [RegularExpression(@"^[a-zA-Z'.\s]{1,40}$", ErrorMessage = "Special Characters not allowed")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [RegularExpression(@"^[a-zA-Z'.\s]{1,40}$", ErrorMessage = "Special Characters not allowed")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name Required")]
        public string LastName { get; set; }

        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID Required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 Character Required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm Password and Password Do Not Match")]
        public string ConfirmPassword { get; set; }

        [Display(Name ="Contact No")]
        [DataType(DataType.PhoneNumber)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Contact Number Required")]
        public string ContactNo { get; set; }
        [Display(Name = "Address")]
        [DataType(DataType.MultilineText)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Address Required")]
        public string AddresL { get; set; }
        [Display(Name = "Street")]
        [DataType(DataType.Text)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Street Name Required")]
        public string Street { get; set; }
        [Display(Name = "City/Distric")]
        [DataType(DataType.Text)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "City Name Required")]
        public string CityName { get; set; }
        [Display(Name = "State/Provinance")]
        [DataType(DataType.Text)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "State/Provinance Name Required")]
        public string Provinance { get; set; }
        [Display(Name = "Country")]
        [DataType(DataType.Text)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Country Name Required")]
        public string CountryName { get; set; }
        public bool IsEmailVerified { get; set; }
        [Display(Name = "Accept T&C")]
      
        public bool TermsAccepted { get; set; }
        [Display(Name = "Subscribe Alert & Messages")]
        
        public bool SubEmail { get; set; }
        public System.Guid ActivationCode { get; set; }

        public System.Guid GlobalID { get; set; }
        public bool Visachk { get; set; }
        public bool Nocchk { get; set; }
        public bool AdminType { get; set; }
        [Display(Name = "Team Name/Company")]
        [RegularExpression(@"^[a-zA-Z'.\s]{1,40}$", ErrorMessage = "Special Characters not allowed")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Team Name/Company Name Required")]
        public string TeamName { get; set; }
        [Display(Name = "Description/Extra Message")]
        [DataType(DataType.MultilineText)]
        public string Decscript { get; set; }
        [Display(Name = "Pincode/ZIP")]
        [DataType(DataType.PostalCode)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Zip/Pincode Number Required")]
        public string Pincode { get; set; }


    }
    public partial class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        public string Abbr { get; set; }
    }
    public partial class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int StateId { get; set; }
    }
}