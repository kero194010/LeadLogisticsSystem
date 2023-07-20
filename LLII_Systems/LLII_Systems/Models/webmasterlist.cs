using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LLII_Systems.Models
{
    public class webmasterlist
    {
    }
    public class item
    {
        public List<item> ItemList { get; set; }

        public string item_code { get; set; }
        public string name { get; set; }
        public string del_flag { get; set; }

        public int item_id { get; set; }
        public decimal cbm { get; set; }
        public decimal weight { get; set; }

        public Category Category { get; set; }
        public string UOM { get; set; }
        public int shelf_life { get; set; }
        public int pcs_per_inner { get; set; }
        public int inner_per_case { get; set; }
        public int pcs_per_case { get; set; }
        public decimal weight_per_case { get; set; }
        public decimal weight_per_inner { get; set; }
        public decimal case_cube { get; set; }
        public decimal length { get; set; }
        public decimal width { get; set; }
        public string unacceptable_condition { get; set; }
        public decimal max_receiving_temp { get; set; }
        public decimal min_receiving_temp { get; set; }
        public int stacking_height_cases { get; set; }
        public int servings_per_case { get; set; }
    }



    
   


    
    public class Store
    {
        public List<Store> storelist { get; set; }
        public int Id { get; set; }
        public string StoreNumber { get; set; }
        public string TempStoreNumber { get; set; }
        public string StoreName { get; set; }
        public string CompanyName { get; set; }
        public string OwnerName { get; set; }
        public string StoreManager { get; set; }
        public string OrderingManager { get; set; }
        public string OC_BC { get; set; }
        public string OM_FSM { get; set; }
        public string StoreOwnership { get; set; }
        public string StoreOperatingHours { get; set; }
        public string BrandExtension { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string AltPhoneNo { get; set; }
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string TinNo { get; set; }
        public int Status { get; set; }
        public int CreatedBy { get; set; }
        public int EditedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
    

}