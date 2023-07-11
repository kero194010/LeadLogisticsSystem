using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using Renci.SshNet;
using Renci.SshNet.Common;


namespace LLII_Systems.Models
{
    [XmlRoot("PurchaseOrder")]
    public class PurchaseOrdercs
    {
        [Key]
        [XmlElement("PurchaseOrderNumber")]
        public int PurchaseOrderNumber { get; set; }

        [XmlElement("PurchaseOrderDate")]
        public DateTime PurchaseOrderDate { get; set; }

        [XmlElement("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [XmlElement("RequiredDate")]
        public DateTime RequiredDate { get; set; }

        [XmlElement("StoreNumber")]
        public string StoreNumber { get; set; }

        [XmlElement("StoreGLN")]
        public string StoreGLN { get; set; }

        [XmlElement("StoreInterchangeID")]
        public string StoreInterchangeID { get; set; }

        [XmlElement("StoreVendorAccountCode")]
        public string StoreVendorAccountCode { get; set; }

        [XmlElement("VendorCode")]
        public string VendorCode { get; set; }

        [XmlElement("InvoiceAmount")]
        public decimal InvoiceAmount { get; set; }

        [XmlElement("VendorGLN")]
        public string VendorGLN { get; set; }

        [XmlElement("VendorInterchangeID")]
        public string VendorInterchangeID { get; set; }

        [XmlElement("VendorName")]
        public string VendorName { get; set; }

        [XmlElement("VendorAddress1")]
        public string VendorAddress1 { get; set; }

        [XmlElement("VendorAddress2")]
        public string VendorAddress2 { get; set; }

        [XmlElement("VendorAddress3")]
        public string VendorAddress3 { get; set; }

        [XmlElement("ShipToAttention")]
        public string ShipToAttention { get; set; }

        [XmlElement("ShipToName")]
        public string ShipToName { get; set; }

        [XmlElement("ShipToAddress1")]
        public string ShipToAddress1 { get; set; }

        [XmlElement("ShipToAddress2")]
        public string ShipToAddress2 { get; set; }

        [XmlElement("ShipToAddress3")]
        public string ShipToAddress3 { get; set; }

        [XmlElement("Comment")]
        public string Comment { get; set; }

        [XmlElement("PurchaseOrderLineItem")]
        public List<PurchaseOrderLineItem> PurchaseOrderLineItems { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StoreName { get; set; }
    }

    public class PurchaseOrderLineItem
    {
        [Key]
        [XmlElement("LineItemNumber")]
        public int LineItemNumber { get; set; }

        [XmlElement("VendorItemCode")]
        public string VendorItemCode { get; set; }

        [XmlElement("ItemCode")]
        public string ItemCode { get; set; }

        [XmlElement("OrderQuantity")]
        public int OrderQuantity { get; set; }

        [XmlElement("PurchaseUnit")]
        public string PurchaseUnit { get; set; }

        [XmlElement("PurchaseUnitPrice")]
        public decimal PurchaseUnitPrice { get; set; }

        public string Item_name { get; set; }
        public decimal Total_unit_price { get; set; }
    }

    public class XmlData
    {
        public int XmlDataId { get; set; }
        public string FileName { get; set; }
        public PurchaseOrdercs PurchaseOrder { get; set; }
        // Add more properties as needed
    }

    [Table("tbl_purchaseOrderLogs")]
    public class PurchaseOrderLogs
    {
        [Key]
        public int LogId { get; set; }

        [XmlElement("LogDate")]
        public DateTime LogDate { get; set; }

        [XmlElement("LogMessage")]
        public string LogMessage { get; set; }

        // Foreign key to PurchaseOrderHdr
        [ForeignKey("PurchaseOrderHdr")]
        public int PurchaseOrderNumber { get; set; }
        public PurchaseOrdercs PurchaseOrderHdr { get; set; }
    }
    public class PurchaseOrderDetailsViewModel
    {
        public PurchaseOrdercs PurchaseOrderHdr { get; set; }
        public List<PurchaseOrderLineItem> PurchaseOrderLineItems { get; set; }
        public List<PurchaseOrderLogs> PurchaseOrderLogs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

    }

}


