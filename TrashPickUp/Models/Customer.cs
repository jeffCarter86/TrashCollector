using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TrashPickUp.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Collection Day")]
        public DayOfWeek? CollectionDay { get; set; }

        [Display(Name = "One-Time Extra Pickup Day")]
        public DayOfWeek? ExtraCollectionDay { get; set; }

        [Display(Name = "Street Address")]
        [Required(ErrorMessage = "Address is Required")]
        public string Address { get; set; }

        [Display(Name = "Zip Code")]
        //[RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip")]
        [Required(ErrorMessage = "Zip Code is Required")]
        public string ZipCode { get; set; }

        [Display(Name = "Suspend Pickup Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Suspend Pickup End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        [Display(Name = "Pickup Confirmation Date")]
        [DataType(DataType.Date)]
        public DateTime? PickupConfirmationDate { get; set; }

        [Display(Name = "One-Time Pickup Confirmation")]
        [DataType(DataType.Date)]
        public DateTime? ExtraCollectionDayConfirmation { get; set; }



        [ForeignKey("IdentityUser")]
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }


    }
}
