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
using System.Xml.Serialization;
using LLII_Systems.Helpers;
using LLII_Systems.Models;
using OfficeOpenXml.Core.Worksheet.Fill;

namespace LLII_Systems.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Orders
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SalesOrderHdr()
        {
            return View();
        }

        #region PO Header List
        public ActionResult PurchaseOrderHdr(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
            {
                // Set default value for startDate as today - 7 days
                startDate = DateTime.Today.AddDays(-7);
            }

            if (!endDate.HasValue)
            {
                // Set default value for endDate as today
                endDate = DateTime.Today;
            }

            List<order> poHeaders = GetPurchaseOrderHeader(startDate, endDate);

            return View(poHeaders);
        }
        private List<order> GetPurchaseOrderHeader(DateTime? startDate, DateTime? endDate)
        {
            List<order> poHeaders = new List<order>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = @"SELECT POH.*, ST.name FROM tbl_purchaseOrderHdr as POH
                        INNER JOIN tbl_store as ST on POH.StoreNumber = ST.StoreNumber";

                // Include date filtering if start and end dates are provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    query += " WHERE POH.PurchaseOrderDate BETWEEN @StartDate AND @EndDate";
                }

                SqlCommand command = new SqlCommand(query, connection);

                // Set parameter values for date filtering
                if (startDate.HasValue && endDate.HasValue)
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate.Value);
                }

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    order purchaseOrder = new order
                    {
                        PurchaseOrderNumber = (int)reader["PurchaseOrderNumber"],
                        PurchaseOrderDate = (DateTime)reader["PurchaseOrderDate"],
                        VendorName = (string)reader["VendorName"],
                        StoreName = (string)reader["name"]
                        // Set other properties accordingly
                    };

                    poHeaders.Add(purchaseOrder);
                }

                reader.Close();
            }
            return poHeaders;
        }
        #endregion


        #region Logs
        public ActionResult OrderLogs(DateTime? startDate, DateTime? endDate)
        {
                // Retrieve the purchase order logs
                List<OrderLogs> OrderLogs = GetOrderLogs(startDate, endDate);

                // Create a view model to hold the details
                var viewModel = new orderdetailsViewModel
                {
                    OrderLog = OrderLogs
                };
                return View(viewModel);
        }


        public List<OrderLogs> GetOrderLogs(DateTime? startDate, DateTime? endDate)
        {
            List<OrderLogs> OrderLogs = new List<OrderLogs>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = "SELECT * FROM tbl_purchaseOrderLogs order by LogDate desc";
                SqlCommand command = new SqlCommand(query, connection);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    OrderLogs OrderLog = new OrderLogs
                    {
                        LogDate = (DateTime)reader["LogDate"],
                        LogMessage = reader["LogMessage"].ToString(),
                    };

                    OrderLogs.Add(OrderLog);
                }

                reader.Close();
            }

            return OrderLogs;
        }
        #endregion
    }
}