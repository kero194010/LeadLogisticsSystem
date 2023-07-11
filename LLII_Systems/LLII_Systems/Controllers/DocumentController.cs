using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.Serialization;
using LLII_Systems.Helpers;
using LLII_Systems.Models;
using Renci.SshNet;



namespace LLII_Systems.Controllers
{
    public class DocumentController : Controller
    {
        public ActionResult ViewXMLFiles()
        {
            string folderPath = "C:\\xampp\\htdocs\\Backup\\";

            var xmlFiles = Directory.GetFiles(folderPath, "*.xml").ToList();

            return View(xmlFiles);
            //return View();
        }

        public FilePathResult OpenXmlFile(string filePath)
        {
            return File(filePath, "application/xml");
        }
        public ActionResult DownloadXmlFile(string filePath)
        {
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(fileBytes, "application/xml", fileName);
        }


        public ActionResult ConvertXmlToHtml(string fileList)
        {
            // Load XML data from file
            string xmlData = System.IO.File.ReadAllText(fileList);

            // Deserialize XML data into PurchaseOrdercs object
            XmlSerializer serializer = new XmlSerializer(typeof(PurchaseOrdercs));
            PurchaseOrdercs purchaseOrder;
            using (StringReader stringReader = new StringReader(xmlData))
            {
                purchaseOrder = (PurchaseOrdercs)serializer.Deserialize(stringReader);
            }

            return View(purchaseOrder);
        }


        [HttpPost]
        public ActionResult UploadFile(IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var model = ParseXmlFile(file.InputStream);

                            // Insert the model data into the database
                            InsertPurchaseOrderData(model);

                            // Additional code for inserting into PurchaseOrderLogs
                            //InsertPurchaseOrderLogs(model);
                            TempData["successMessage"] = "Files uploaded and data inserted successfully.";
                            ViewBag.Message = "Files uploaded and data inserted successfully.";
                        }
                        else
                        {
                            TempData["errorMessage"] = "No files selected.";
                            ViewBag.ErrorMessage = "No files selected.";
                        }

                    }

                }
                else
                {
                    TempData["errorMessage"] = "No files selected.";
                    ViewBag.ErrorMessage = "No files selected.";
                }
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "An error occurred: " + ex.Message;
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
            }

            return RedirectToAction("ViewXMLFiles");
        }

        private PurchaseOrdercs ParseXmlFile(Stream fileStream)
        {
            var serializer = new XmlSerializer(typeof(PurchaseOrdercs));
            return (PurchaseOrdercs)serializer.Deserialize(fileStream);
        }

        private void InsertPurchaseOrderData(PurchaseOrdercs model)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Insert the data into tbl_purchaseOrderHdr
                    string hdrQuery = "INSERT INTO tbl_purchaseOrderHdr (PurchaseOrderNumber, PurchaseOrderDate, InvoiceDate, RequiredDate, StoreNumber, StoreVendorAccountCode, VendorCode, InvoiceAmount, VendorName, ShipToName) " +
                                      "VALUES (@PurchaseOrderNumber, @PurchaseOrderDate, @InvoiceDate, @RequiredDate, @StoreNumber, @StoreVendorAccountCode, @VendorCode, @InvoiceAmount, @VendorName, @ShipToName)";

                    using (SqlCommand hdrCommand = new SqlCommand(hdrQuery, connection, transaction))
                    {
                        hdrCommand.Parameters.AddWithValue("@PurchaseOrderNumber", model.PurchaseOrderNumber);
                        hdrCommand.Parameters.AddWithValue("@PurchaseOrderDate", model.PurchaseOrderDate);
                        hdrCommand.Parameters.AddWithValue("@InvoiceDate", model.InvoiceDate);
                        hdrCommand.Parameters.AddWithValue("@RequiredDate", model.RequiredDate);
                        hdrCommand.Parameters.AddWithValue("@StoreNumber", model.StoreNumber);
                        hdrCommand.Parameters.AddWithValue("@StoreVendorAccountCode", model.StoreVendorAccountCode);
                        hdrCommand.Parameters.AddWithValue("@VendorCode", model.VendorCode);
                        hdrCommand.Parameters.AddWithValue("@InvoiceAmount", model.InvoiceAmount);
                        hdrCommand.Parameters.AddWithValue("@VendorName", model.VendorName);
                        hdrCommand.Parameters.AddWithValue("@ShipToName", model.ShipToName);

                        hdrCommand.ExecuteNonQuery();
                    }

                    // Insert the data into tbl_purchaseOrderDtl
                    string dtlQuery = "INSERT INTO tbl_purchaseOrderDtl (PurchaseOrderNumber, LineItemNumber, VendorItemCode, ItemCode, OrderQuantity, PurchaseUnit, PurchaseUnitPrice) " +
                                      "VALUES (@PurchaseOrderNumber, @LineItemNumber, @VendorItemCode, @ItemCode, @OrderQuantity, @PurchaseUnit, @PurchaseUnitPrice)";

                    foreach (var lineItem in model.PurchaseOrderLineItems)
                    {
                        using (SqlCommand dtlCommand = new SqlCommand(dtlQuery, connection, transaction))
                        {
                            dtlCommand.Parameters.AddWithValue("@PurchaseOrderNumber", model.PurchaseOrderNumber);
                            dtlCommand.Parameters.AddWithValue("@LineItemNumber", lineItem.LineItemNumber);
                            dtlCommand.Parameters.AddWithValue("@VendorItemCode", lineItem.VendorItemCode);
                            dtlCommand.Parameters.AddWithValue("@ItemCode", lineItem.ItemCode);
                            dtlCommand.Parameters.AddWithValue("@OrderQuantity", lineItem.OrderQuantity);
                            dtlCommand.Parameters.AddWithValue("@PurchaseUnit", lineItem.PurchaseUnit);
                            dtlCommand.Parameters.AddWithValue("@PurchaseUnitPrice", lineItem.PurchaseUnitPrice);

                            dtlCommand.ExecuteNonQuery();
                        }
                    }

                    // Insert a log entry into tbl_purchaseOrderLogs
                    string logQuery = "INSERT INTO tbl_purchaseOrderLogs (PurchaseOrderNumber, LogDate, LogMessage) " +
                                      "VALUES (@PurchaseOrderNumber, @LogDate, @LogMessage)";

                    using (SqlCommand logCommand = new SqlCommand(logQuery, connection, transaction))
                    {
                        logCommand.Parameters.AddWithValue("@PurchaseOrderNumber", model.PurchaseOrderNumber);
                        logCommand.Parameters.AddWithValue("@LogDate", DateTime.Now);
                        logCommand.Parameters.AddWithValue("@LogMessage", "Purchase Order: " + model.PurchaseOrderNumber + " data inserted successfully.");

                        logCommand.ExecuteNonQuery();
                    }

                    // Commit the transaction
                    transaction.Commit();

                    // Any other post-commit logic or operations can be performed here
                }
                catch (Exception ex)
                {
                    // Rollback the transaction in case of an exception
                    transaction.Rollback();

                    // Insert a log entry into tbl_purchaseOrderLogs
                    string logQuery = "INSERT INTO tbl_purchaseOrderLogs (PurchaseOrderNumber, LogDate, LogMessage) " +
                                      "VALUES (@PurchaseOrderNumber, @LogDate, @LogMessage)";

                    using (SqlCommand logCommand = new SqlCommand(logQuery, connection, transaction))
                    {
                        logCommand.Parameters.AddWithValue("@PurchaseOrderNumber", model.PurchaseOrderNumber);
                        logCommand.Parameters.AddWithValue("@LogDate", DateTime.Now);
                        logCommand.Parameters.AddWithValue("@LogMessage", model.PurchaseOrderNumber + " failed to insert. Exception: " + ex.Message);

                        logCommand.ExecuteNonQuery();
                    }

                    // Handle or rethrow the exception as needed
                    throw new Exception("An error occurred while inserting data.", ex);
                }

            }
        }



        #region PO List
        public ActionResult PurchaseOrderHdrVw()
        {
            List<PurchaseOrdercs> purchaseOrderHdrs = GetPurchaseOrderHeader();


            return View(purchaseOrderHdrs);
        }
        private List<PurchaseOrdercs> GetPurchaseOrderHeader()
        {
            List<PurchaseOrdercs> purchaseOrderHdrs = new List<PurchaseOrdercs>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = @"SELECT POH.*,ST.name FROM tbl_purchaseOrderHdr as POH
                        inner join tbl_store as ST on POH.StoreNumber = ST.StoreNumber order by POH.PurchaseOrderDate desc";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PurchaseOrdercs purchaseOrder = new PurchaseOrdercs
                    {
                        PurchaseOrderNumber = (int)reader["PurchaseOrderNumber"],
                        PurchaseOrderDate = (DateTime)reader["PurchaseOrderDate"],
                        VendorName = (string)reader["VendorName"],
                        StoreName = (string)reader["name"]
                        // Set other properties accordingly
                    };

                    purchaseOrderHdrs.Add(purchaseOrder);
                }

                reader.Close();
            }

            return purchaseOrderHdrs;
        }
        #endregion


        #region order Details 
        public ActionResult Details(int? id, int page = 1)
        {
            if (id.HasValue)
            {
                int purchaseOrderId = id.Value;

                // Retrieve the purchase order header
                PurchaseOrdercs purchaseOrderHdr = GetPurchaseOrderHeader(purchaseOrderId);

                if (purchaseOrderHdr == null)
                {
                    // Handle the case when the purchase order header is not found
                    return HttpNotFound();
                }

                int pageSize = 1;

                // Retrieve all purchase order line items
                List<PurchaseOrderLineItem> allLineItems = GetPurchaseOrderLineItems(purchaseOrderId);

                // Calculate the total number of pages
                int totalPages = (int)Math.Ceiling((double)allLineItems.Count / pageSize);

                // Ensure the current page is within valid range
                page = Math.Max(1, Math.Min(page, totalPages));

                // Get the subset of line items for the current page
                List<PurchaseOrderLineItem> lineItems = allLineItems
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Retrieve the purchase order logs
                List<PurchaseOrderLogs> purchaseOrderLogs = GetPurchaseOrderLogs(purchaseOrderId);

                // Create a view model to hold the details
                var viewModel = new PurchaseOrderDetailsViewModel
                {
                    PurchaseOrderHdr = purchaseOrderHdr,
                    PurchaseOrderLineItems = lineItems,
                    PurchaseOrderLogs = purchaseOrderLogs,
                    CurrentPage = page,
                    TotalPages = totalPages
                };
                return View(viewModel);

            }
            else
            {
                return RedirectToAction("PurchaseOrderHdrVw", "Document");
            }

        }



        public PurchaseOrdercs GetPurchaseOrderHeader(int id)
        {
            PurchaseOrdercs purchaseOrderHdr = null;

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = @"SELECT POH.*,ST.name FROM tbl_purchaseOrderHdr as POH
                        inner join tbl_store as ST on POH.StoreNumber = ST.StoreNumber
                        where POH.PurchaseOrderNumber = @Id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    purchaseOrderHdr = new PurchaseOrdercs
                    {
                        PurchaseOrderNumber = (int)reader["PurchaseOrderNumber"],
                        PurchaseOrderDate = (DateTime)reader["PurchaseOrderDate"],
                        StoreName = (string)reader["name"],
                        VendorName = (string)reader["VendorName"],
                        InvoiceAmount = (decimal)reader["InvoiceAmount"]

                    };
                }

                reader.Close();
            }

            return purchaseOrderHdr;
        }
        public List<PurchaseOrderLineItem> GetPurchaseOrderLineItems(int id)
        {
            List<PurchaseOrderLineItem> purchaseOrderLineItems = new List<PurchaseOrderLineItem>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = @"SELECT Pd.*, i.name FROM tbl_purchaseOrderDtl AS PD
                        INNER JOIN tbl_item AS i ON PD.ItemCode = i.item_code
                        WHERE PD.PurchaseOrderNumber = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PurchaseOrderLineItem purchaseOrderLineItem = new PurchaseOrderLineItem
                    {
                        LineItemNumber = Convert.ToInt32(reader["LineItemNumber"]),
                        VendorItemCode = reader["VendorItemCode"].ToString(),
                        ItemCode = reader["ItemCode"].ToString(),
                        OrderQuantity = Convert.ToInt32(reader["OrderQuantity"]),
                        PurchaseUnit = reader["PurchaseUnit"].ToString(),
                        PurchaseUnitPrice = Convert.ToDecimal(reader["PurchaseUnitPrice"]),
                        Item_name = reader["name"].ToString(),
                        Total_unit_price = Convert.ToInt32(reader["OrderQuantity"]) * Convert.ToDecimal(reader["PurchaseUnitPrice"]),
                    };

                    purchaseOrderLineItems.Add(purchaseOrderLineItem);
                }

                reader.Close();
            }

            return purchaseOrderLineItems;
        }
        public List<PurchaseOrderLogs> GetPurchaseOrderLogs(int id)
        {
            List<PurchaseOrderLogs> purchaseOrderLogs = new List<PurchaseOrderLogs>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = "SELECT * FROM tbl_purchaseOrderLogs WHERE PurchaseOrderNumber = @Id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PurchaseOrderLogs purchaseOrderLog = new PurchaseOrderLogs
                    {
                        LogDate = (DateTime)reader["LogDate"],
                        LogMessage = reader["LogMessage"].ToString(),
                    };

                    purchaseOrderLogs.Add(purchaseOrderLog);
                }

                reader.Close();
            }

            return purchaseOrderLogs;
        }

        #endregion



        public ActionResult AccessSFTP()
        {
            var fileList = new List<string>();

            // SFTP server details
            string sftpHost = "13.229.20.149";
            string sftpUsername = "netsuite-sftp";
            int sftpPort = 22;

            try
            {
                var privateKeyFilePath = @"D:\mrmxintegration\kollab AWS\PEM PPK\netsuite-sftp.pem"; // Specify the path to your PEM format private key file

                var privateKeyFile = new PrivateKeyFile(privateKeyFilePath);

                var privateKeyAuth = new PrivateKeyAuthenticationMethod(sftpUsername, privateKeyFile);

                var connectionInfo = new ConnectionInfo(sftpHost, sftpPort, sftpUsername, privateKeyAuth);

                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        // Change the SFTP folder path to the desired folder
                        string sftpFolderPath = "/ns-mrmxintegration/Inbound/Backup";

                        var files = client.ListDirectory(sftpFolderPath);
                        foreach (var file in files)
                        {
                            // Exclude directories from the file list
                            if (!file.IsDirectory)
                            {
                                fileList.Add(file.Name);
                            }
                        }
                    }
                    else
                    {
                        // Failed to connect to the SFTP server
                        ViewBag.Message = "Failed to connect to the SFTP server.";
                    }

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the SFTP operation
                ViewBag.Message = "An error occurred while accessing the SFTP server: " + ex.Message;
            }

            return View(fileList);
        }

        public ActionResult SerializeXml(string fileName)
        {
            string sftpHost = "13.229.20.149";
            string sftpUsername = "netsuite-sftp";
            int sftpPort = 22;
            var privateKeyFilePath = @"D:\mrmxintegration\kollab AWS\PEM PPK\netsuite-sftp.pem"; // Specify the path to your PEM format private key file

            var privateKeyFile = new PrivateKeyFile(privateKeyFilePath);
            var privateKeyAuth = new PrivateKeyAuthenticationMethod(sftpUsername, privateKeyFile);
            var connectionInfo = new ConnectionInfo(sftpHost, sftpPort, sftpUsername, privateKeyAuth);

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    string sftpFolderPath = "/ns-mrmxintegration/Inbound/Backup";
                    string filePath = sftpFolderPath + "/" + fileName;

                    var memoryStream = new MemoryStream();

                    client.DownloadFile(filePath, memoryStream);
                    memoryStream.Position = 0;

                    // Deserialize the XML file
                    var serializer = new XmlSerializer(typeof(PurchaseOrdercs)); 
                    var deserializedData = (PurchaseOrdercs)serializer.Deserialize(memoryStream);
                   
                    //Do something with the deserialized data, such as saving it to a database or passing it to a view

                    return View(deserializedData); // Replace 'Index' with the name of the action you want to redirect to after deserialization
                }
                else
                {
                    // Failed to connect to the SFTP server
                    ViewBag.Message = "Failed to connect to the SFTP server.";
                }

                client.Disconnect();
            }

            return View("AccessSFTP"); // Redirect back to the AccessSFTP view if any error occurs during deserialization
        }



    }
}