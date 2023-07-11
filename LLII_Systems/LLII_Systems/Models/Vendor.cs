using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LLII_Systems.Models
{
    public class Vendor
    {
        public List<Vendor> vendorlist { get; set; }
        public int Id { get; set; }
        public string vendor_code { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int CreatedBy { get; set; }
        public int EditedBy { get; set; }
        public string TinNumber { get; set; }
        public string owner_type { get; set; }
        public string primary_subsidiary { get; set; }
        public string sap_subsidiary { get; set; }
        public string acct_code { get; set; }
        public string sub_acct_code { get; set; }
        public decimal vendor_price { get; set; }

    }
    public class VendorDetailViewModel
    {
        public Vendor VendorDetail { get; set; }
        public List<Item> itemVendors { get; set; }
    }
}