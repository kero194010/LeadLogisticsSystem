using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LLII_Systems.Models
{
    public class Item
    {
        public List<Item> itemlist { get; set; }

        [Required(ErrorMessage = "The Category field is required.")]

        public Category item_cat { get; set; }

        [Required(ErrorMessage = "The Sub Category field is required.")]
        public SubCategory item_sub_cat { get; set; }

        public Zone zone { get; set; }

        public Vendor vendor_id { get; set; }

        #region Integer Types
        public int id { get; set; }

        public int cases_pallet { get; set; }

        public int shelf_life { get; set; }

        public int inner_per_case { get; set; }

        public int pieces_per_inner { get; set; }
        public int total_unit_per_case { get; set; }
        public int case_yield { get; set; }
        public int serving_per_case { get; set; }

        public int status { get; set; }
        public decimal weight { get; set; }
        public decimal weight_per_inner { get; set; }

        #endregion

        #region Decimal Type
        public decimal case_cube { get; set; }

        public decimal conversion_factor { get; set; }
        public decimal vendor_price { get; set; }
        public decimal unit_price { get; set; }
        #endregion

        #region Boolean Type
        public bool vat_indicator { get; set; }

        public bool perishable { get; set; }

        public bool del_flag { get; set; }

        public bool activated { get; set; }

        public bool with_sf { get; set; }
        #endregion

        #region String Type
        [Required(ErrorMessage = "The Item Code field is required.")]
        public string item_code { get; set; }

        [Required(ErrorMessage = "The Item Description field is required.")]
        public string item_desc { get; set; }

        [Required(ErrorMessage = "The Item Source field is required.")]
        public string item_source { get; set; }

        [Required(ErrorMessage = "The UOM field is required.")]
        public string item_uom { get; set; }
        public string inner_pckg_uom { get; set; }
        public string base_unit { get; set; }

        public string split_code { get; set; }

        public string mother_code { get; set; }

        public string client_code { get; set; }

        public string secondary_desc { get; set; }

        public string abc_class { get; set; }

        public string issuance_uom { get; set; }

        public string purchase_unit { get; set; }
        public string inner_unit { get; set; }

        public string weight_uom { get; set; }
        public string weight_uom_inner { get; set; }

        public string dim_uom { get; set; }

        public string temp_zone { get; set; }

        public string cat1 { get; set; }

        public string cat2 { get; set; }

        public string cat3 { get; set; }

        public string acct_code { get; set; }

        public string sub_acct_code { get; set; }

        public string created_by { get; set; }



        public string primary_subsidiary { get; set; }

        public string sap_subsidiary { get; set; }

        public string length { get; set; }

        public string width { get; set; }

        public string height { get; set; }



        #endregion

        #region DateTime Type
        public DateTime date_created { get; set; }

        public DateTime date_edit { get; set; }

        public DateTime effective_date { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        #endregion
    }

    public class PaginationViewModel
    {
        public List<Item> Items { get; set; }

        public int Page { get; set; }

        public int TotalPages { get; set; }
    }

    public class ItemDetailViewModel
    {
        public Item ItemDetail { get; set; }

        public List<Vendor> ItemVendors { get; set; }

        public List<Vendor> VendorList { get; set; }

        public List<ItemLog> ItemLogs { get; set; }


    }

    public class ItemLog
    {
        public int Id { get; set; }

        public string ItemId { get; set; }

        public string Action { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public User User { get; set; }
    }
    public class Category
    {
        [Required(ErrorMessage = "The Category field is required.")]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class SubCategory
    {
        [Required(ErrorMessage = "The Sub Category field is required.")]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Zone
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
