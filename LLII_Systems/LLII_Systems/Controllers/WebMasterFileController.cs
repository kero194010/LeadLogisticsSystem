using LLII_Systems.Models;
using LLII_Systems.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using OfficeOpenXml;
using System.Drawing.Printing;
using System.IO;
using System.Web.UI;
using System.Web;
using System.Xml.Linq;
using System.Web.UI.WebControls;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.tool.xml;
using System.Linq;
using System.Windows.Documents;
using System.Text;
using System.Threading.Tasks;

namespace LLII_Systems.Controllers
{
    public class WebMasterFileController : Controller
    {
        // GET: WebMasterFile
        public ActionResult Index()
        {
            return View();
        }
        #region Item

     
        

        


        private bool IsItemCodeExists(string itemCode)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM tbl_item WHERE item_code = @item_code";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@item_code", itemCode);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }



        public List<Category> GetCategoryList()
        {
            List<Category> categorylist = new List<Category>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, name FROM tbl_category order by id asc";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.Text;

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Category category = new Category
                        {
                            Id = Convert.ToInt32(dataReader["id"]),
                            Name = dataReader["name"].ToString()
                        };

                        categorylist.Add(category);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return categorylist;
        }
        public List<SubCategory> GetSubCategoryList()
        {
            List<SubCategory> subcategorylist = new List<SubCategory>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, name FROM tbl_subcategory";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.Text;

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        SubCategory category = new SubCategory
                        {
                            Id = Convert.ToInt32(dataReader["id"]),
                            Name = dataReader["name"].ToString()
                        };

                        subcategorylist.Add(category);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return subcategorylist;
        }
        
        private void SaveItemToDatabase(Item item)
        {
            // Replace this with your actual database saving logic
            // Here, you can access the item's properties and save them to the database
            // For example: dbContext.Items.Add(item);
        }

        #endregion
        #region  Vendor
        
       
        
        public ActionResult AddVendor()
        {
            return View();
        }
        public ActionResult InsertVendor(Vendor vendor)
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        string query = "INSERT INTO tbl_vendor (vendor_name, contact_person, contact_number, email, address, city, state, country, postal_code, status, date_created, date_modified, created_by, edited_by) " +
                           "VALUES (@Name, @ContactPerson, @ContactNumber, @Email, @Address, @City, @State, @Country, @PostalCode, @Status, getdate(), getdate(), @CreatedBy, @EditedBy)";

                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {

                            command.Parameters.AddWithValue("@Name", vendor.Name);
                            command.Parameters.AddWithValue("@ContactPerson", vendor.ContactPerson);
                            command.Parameters.AddWithValue("@ContactNumber", vendor.ContactNumber);
                            command.Parameters.AddWithValue("@Email", vendor.Email);
                            command.Parameters.AddWithValue("@Address", vendor.Address);
                            command.Parameters.AddWithValue("@City", vendor.City);
                            command.Parameters.AddWithValue("@State", vendor.State);
                            command.Parameters.AddWithValue("@Country", vendor.Country);
                            command.Parameters.AddWithValue("@PostalCode", vendor.PostalCode);
                            command.Parameters.AddWithValue("@Status", 0);

                            command.Parameters.AddWithValue("@CreatedBy", 1);
                            command.Parameters.AddWithValue("@EditedBy", 1);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                TempData["successMessage"] = "Company Name: " + vendor.Name + " was successfully added.";
                return RedirectToAction("AddVendor", "WebMasterFile");
            }
            catch (Exception ex)
            {
                // Handle the exception or log the error
                TempData["errorMessage"] = ex.Message.ToString();
                return RedirectToAction("AddVendor", "WebMasterFile");
            }
        }

        #endregion
        #region Store
        public async Task<ActionResult> StoreList()
        {
            var storeList = await GenerateStoreListAsync();
            return View(storeList);
        }

        private async Task<List<Store>> GenerateStoreListAsync()
        {
            List<Store> storeList = new List<Store>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = @"SELECT * FROM tbl_store";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.Text;

                    SqlDataReader dataReader = await command.ExecuteReaderAsync();
                    while (await dataReader.ReadAsync())
                    {
                        Store store_dtl = new Store
                        {
                            Id = Convert.ToInt32(dataReader["id"]),
                            StoreNumber = dataReader["store_number"].ToString(),
                            StoreName = dataReader["store_name"].ToString(),
                            CompanyName = dataReader["company_name"].ToString(),
                        };

                        storeList.Add(store_dtl);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return storeList;
        }
        public ActionResult StoreDetail(string storeId)
        {

            if (string.IsNullOrEmpty(storeId))
            {
                // Redirect to another action when itemCode is null or empty
                return RedirectToAction("StoreList", "WebMasterFile");
            }
            else
            {
                if (IsStoreCodeExists(storeId) == false)
                {
                    TempData["errorMessage"] = "Store ID: " + storeId + " doesnt exists.";
                    return RedirectToAction("StoreList", "WebMasterFile");
                }
                else
                {

                    // Retrieve the item detail from the database
                    Store storeDetail = GetStoreDetail(storeId);
                    return View(storeDetail);
                }

            }
        }
        private Store GetStoreDetail(string storeId)
        {
            Store storeDetail = null;

            SqlConnection connection = new SqlConnection(MyHelper.stringConnection);
            try
            {
                connection.Open();
                string query = @"SELECT
                                   *
                                FROM
                                  tbl_store
                                WHERE id = @storeID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@storeID", storeId);
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    storeDetail = new Store();
                    storeDetail.Id = Convert.ToInt32(dataReader["Id"].ToString());
                    storeDetail.StoreNumber = dataReader["store_number"].ToString();
                    storeDetail.StoreName = dataReader["store_name"].ToString();
                    storeDetail.OwnerName = dataReader["owner_name"].ToString();
                    storeDetail.StoreManager = dataReader["store_manager"].ToString();
                    storeDetail.OrderingManager = dataReader["ordering_manager"].ToString();
                    storeDetail.OC_BC = dataReader["OC_BC"].ToString();
                    storeDetail.OM_FSM = dataReader["OM_FSM"].ToString();
                    storeDetail.StoreOwnership = dataReader["store_ownership"].ToString();
                    storeDetail.StoreOperatingHours = dataReader["store_operating_hours"].ToString();
                    storeDetail.BrandExtension = dataReader["brand_extension"].ToString();
                    storeDetail.ContactNumber = dataReader["contact_number"].ToString();
                    storeDetail.Email = dataReader["email"].ToString();
                    storeDetail.Address = dataReader["address"].ToString();
                    storeDetail.City = dataReader["city"].ToString();
                    storeDetail.State = dataReader["state"].ToString();
                    storeDetail.Country = dataReader["country"].ToString();
                    storeDetail.PostalCode = dataReader["postal_code"].ToString();
                    storeDetail.TinNo = dataReader["tin_no"].ToString();
                    storeDetail.Status = Convert.ToInt32(dataReader["status"].ToString());
                }

                dataReader.Close();
                command.Dispose();
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message.ToString();
            }
            finally
            {
                connection.Close();
            }

            return storeDetail;

        }
        private bool IsStoreCodeExists(string storeId)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM tbl_store WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", storeId);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public ActionResult AddStore()
        {
            return View();
        }
        public ActionResult InsertStore(Store store)
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        string query = @"INSERT INTO tbl_store 
                        (store_number, temp_store_number, store_name, company_name, owner_name, store_manager, ordering_manager,
                                 OC_BC, OM_FSM, store_ownership, store_operating_hours, brand_extension, email, contact_number,
                                 alt_phone_no, address, city, state, country, postal_code, tin_no, status, created_by, edited_by,
                                 date_created, date_modified)
                        VALUES
                        (@StoreNumber, @TempStoreNumber, @StoreName, @CompanyName, @OwnerName, @StoreManager, @OrderingManager,
                         @OC_BC, @OM_FSM, @StoreOwnership, @StoreOperatingHours, @BrandExtension, @Email, @ContactNumber,
                         @AltPhoneNo, @Address, @City, @State, @Country, @PostalCode, @TinNo, @Status, @CreatedBy, @EditedBy,
                         getdate(), getdate())";
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {

                            command.Parameters.AddWithValue("@StoreNumber", store.StoreNumber);
                            command.Parameters.AddWithValue("@TempStoreNumber", "TMP123");
                            command.Parameters.AddWithValue("@StoreName", store.StoreName);
                            command.Parameters.AddWithValue("@CompanyName", "McDonalds");
                            command.Parameters.AddWithValue("@OwnerName", store.OwnerName);
                            command.Parameters.AddWithValue("@StoreManager", "Kervin");
                            command.Parameters.AddWithValue("@OrderingManager", "Kervin");
                            command.Parameters.AddWithValue("@OC_BC", "OC_BC");
                            command.Parameters.AddWithValue("@OM_FSM", "OM_FSM");
                            command.Parameters.AddWithValue("@StoreOwnership", "Kervin");
                            command.Parameters.AddWithValue("@StoreOperatingHours", "8 AM - 6 PM");
                            command.Parameters.AddWithValue("@BrandExtension", "Brand Ext 1");
                            command.Parameters.AddWithValue("@Email", store.Email);
                            command.Parameters.AddWithValue("@ContactNumber", store.ContactNumber);
                            command.Parameters.AddWithValue("@AltPhoneNo", 123123123);
                            command.Parameters.AddWithValue("@Address", store.Address);
                            command.Parameters.AddWithValue("@City", store.City);
                            command.Parameters.AddWithValue("@State", store.State);
                            command.Parameters.AddWithValue("@Country", store.Country);
                            command.Parameters.AddWithValue("@PostalCode", store.PostalCode);
                            command.Parameters.AddWithValue("@TinNo", store.TinNo);
                            command.Parameters.AddWithValue("@Status", 1);
                            command.Parameters.AddWithValue("@CreatedBy", 1001);
                            command.Parameters.AddWithValue("@EditedBy", 1001);


                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                TempData["successMessage"] = "Store Number: " + store.StoreNumber + " was successfully added.";
                return RedirectToAction("AddStore", "WebMasterFile");
            }
            catch (Exception ex)
            {
                // Handle the exception or log the error

                TempData["errorMessage"] = ex.Message.ToString();
                return RedirectToAction("AddStore", "WebMasterFile");
            }
        }
        #endregion



        #region Vendor to Item or Item to Vendor
        public List<Vendor> GenerateVendorToItemList(string ItemId)
        {
            List<Vendor> vendorList = new List<Vendor>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = @"Select v.vendor_name,vi.* from tbl_vendor_item as VI
                                        inner join tbl_vendor as V
                                        on V.id = Vi.vendor_id
                                        where item_id = @itemId
                                ";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@itemId", ItemId);
                    command.CommandType = CommandType.Text;

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Vendor vendor_dtl = new Vendor
                        {
                            Id = Convert.ToInt32(dataReader["vendor_id"]),
                            Name = dataReader["vendor_name"].ToString(),
                        };

                        vendorList.Add(vendor_dtl);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return vendorList;
        }

    

        #endregion

        #region Audit Logs


        #endregion

    }



}