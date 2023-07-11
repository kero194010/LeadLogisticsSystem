using LLII_Systems.Helpers;
using LLII_Systems.Models;
using LLII_Systems.Controllers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Org.BouncyCastle.Asn1.Mozilla;

namespace LLII_Systems.Controllers
{

    [RoutePrefix("vendor")]
    public class VendorController : Controller
    {

        [Route("list")]
        [Authorize]
        public ActionResult VendorList()
        {
            var vendorlist = GenerateVendorList();
            return View(vendorlist);
        }

        private List<Vendor> GenerateVendorList()
        {
            List<Vendor> vendorList = new List<Vendor>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = @"Select * from tbl_vendor
                                ";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.Text;

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Vendor vendor_dtl = new Vendor
                        {
                            Id = Convert.ToInt32(dataReader["id"]),
                            Name = dataReader["vendor_name"].ToString(),
                            TinNumber = dataReader["tin_no"].ToString(),
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
        [Route("detail")]
        [Authorize]
        public ActionResult VendorDetail(string vendorId)
        {

            if (string.IsNullOrEmpty(vendorId))
            {
                // Redirect to another action when itemCode is null or empty
                return RedirectToAction("list", "vendor");
            }
            else
            {
                if (IsVendorCodeExists(vendorId) == false)
                {
                    TempData["errorMessage"] = "Vendor ID: " + vendorId + " doesnt exists.";
                    return RedirectToAction("list", "vendor");
                }
                else
                {

                    List<Vendor> vendors = GetVendorList();

                    // Retrieve the item detail from the database
                    Vendor vendorDetail = GetVendorDetail(vendorId);
                    List<Item> itemVendors = GetItemtoVendor(vendorId);
                    var model = new VendorDetailViewModel
                    {
                        VendorDetail = vendorDetail,
                        itemVendors = itemVendors,
                    };

                    return View(model);
                }

            }
        }
        private Vendor GetVendorDetail(string vendorId)
        {
            Vendor vendorDetail = null;

            SqlConnection connection = new SqlConnection(MyHelper.stringConnection);
            try
            {
                connection.Open();
                string query = @"SELECT
                                   *
                                FROM
                                  tbl_vendor
                                WHERE id = @vendorID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@vendorID", vendorId);
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    vendorDetail = new Vendor();
                    vendorDetail.Id = Convert.ToInt32(dataReader["Id"].ToString());
                    vendorDetail.vendor_code = dataReader["vendor_code"].ToString();
                    vendorDetail.Name = dataReader["vendor_name"].ToString();
                    vendorDetail.ContactPerson = dataReader["contact_person"].ToString();
                    vendorDetail.ContactNumber = dataReader["contact_number"].ToString();
                    vendorDetail.Email = dataReader["email"].ToString();
                    vendorDetail.Address = dataReader["address"].ToString();
                    vendorDetail.City = dataReader["city"].ToString();
                    vendorDetail.State = dataReader["state"].ToString();
                    vendorDetail.Country = dataReader["country"].ToString();
                    vendorDetail.PostalCode = dataReader["postal_code"].ToString();
                    vendorDetail.Status = Convert.ToInt32(dataReader["status"].ToString());
                    vendorDetail.TinNumber = dataReader["tin_no"].ToString();
                    //vendorDetail.vendor_price = Convert.ToDecimal(dataReader[""]).ToString());
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

            return vendorDetail;

        }
        private bool IsVendorCodeExists(string vendorId)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM tbl_vendor WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", vendorId);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public List<Vendor> GetVendorList()
        {
            List<Vendor> vendorlist = new List<Vendor>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id,vendor_name FROM tbl_vendor";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.Text;

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Vendor vendor = new Vendor
                        {
                            Id = Convert.ToInt32(dataReader["id"]),
                            Name = dataReader["vendor_name"].ToString()
                        };

                        vendorlist.Add(vendor);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return vendorlist;
        }
        public List<Item> GetItemtoVendor(string vendorId)
        {
            List<Item> itemlist = new List<Item>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = @"Select i.item_code,i.item_desc,vi.*
                                    from tbl_vendor_item as VI
                                    inner join tbl_item as i
                                    on i.id = Vi.item_id
                                    where Vi.vendor_id = @vendorId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@vendorId", vendorId);
                    command.CommandType = CommandType.Text;

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Item item_detail = new Item
                        {
                            id = Convert.ToInt32(dataReader["item_id"]),
                            item_code = dataReader["item_code"].ToString(),
                            item_desc = dataReader["item_desc"].ToString(),
                            //tinnumber = dataReader["tin_no"].ToString(),
                            vendor_price = Convert.ToDecimal(dataReader["vendor_price"]),
                        };

                        itemlist.Add(item_detail);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return itemlist;
        }

        public void UpdatePrice(string vendorId, string itemId, decimal newPrice, string editedBy)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    string query = "UPDATE [LeadLogistics_db].[dbo].[tbl_vendor_item] " +
                                   "SET [vendor_price] = @NewPrice, [edit_by] = @EditedBy, [date_edited] = GETDATE() " +
                                   "WHERE [vendor_id] = @VendorId AND [item_id] = @ItemId";

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@NewPrice", newPrice);
                        command.Parameters.AddWithValue("@EditedBy", 1);
                        command.Parameters.AddWithValue("@VendorId", vendorId);
                        command.Parameters.AddWithValue("@ItemId", itemId);

                        command.ExecuteNonQuery();
                    }
                    TempData["successMessage"] = "Price Update Successfully";
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Handle the exception and rollback the transaction if necessary
                    transaction.Rollback();
                    TempData["errorMessage"] = ex.Message.ToString();
                    // Log or throw the exception as needed
                    throw;
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }

        [Route("add")]
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
                        string query = "INSERT INTO tbl_vendor (vendor_name, contact_person, contact_number, email, address, city, state, country, postal_code, status, date_created, date_modified, created_by) " +
                           "VALUES (@Name, @ContactPerson, @ContactNumber, @Email, @Address, @City, @State, @Country, @PostalCode, @Status, getdate(), getdate(), @CreatedBy)";

                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@VendorCode", vendor.vendor_code);
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

    }
}