using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace LLII_Systems.Models
{
    public class order
    {      
        public int PurchaseOrderNumber { get; set; }
        public DateTime PurchaseOrderDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public string StoreNumber { get; set; }
        public string StoreGLN { get; set; }
        public string StoreInterchangeID { get; set; }
        public string StoreVendorAccountCode { get; set; }
        public string VendorCode { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string VendorGLN { get; set; }
        public string VendorInterchangeID { get; set; }
        public string VendorName { get; set; }
        public string VendorAddress1 { get; set; }
        public string VendorAddress2 { get; set; }
        public string VendorAddress3 { get; set; }
        public string ShipToAttention { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAddress1 { get; set; }
        public string ShipToAddress2 { get; set; }
        public string ShipToAddress3 { get; set; }
        public string Comment { get; set; }
        public List<PurchaseOrderLineItem> PurchaseOrderLineItems { get; set; }
        public string StoreName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


    public class OrderLogs
    {
     
        public int LogId { get; set; }

        public DateTime LogDate { get; set; }
        public string LogMessage { get; set; }

        public int PurchaseOrderNumber { get; set; }
        public order PurchaseOrderHdr { get; set; }
    }

    public class orderdetailsViewModel
    {
        public order PurchaseOrderHdr { get; set; }
        public List<PurchaseOrderLineItem> PurchaseOrderLineItems { get; set; }
        public List<OrderLogs> OrderLog { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

    }
}