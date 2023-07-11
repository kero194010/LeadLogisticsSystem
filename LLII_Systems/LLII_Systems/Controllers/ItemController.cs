using LLII_Systems.Helpers;
using LLII_Systems.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using System.Text;

namespace LLII_Systems.Controllers
{
    [RoutePrefix("item")]
    [Authorize]
    public class ItemController : Controller
    {
        WebMasterFileController wmf = new WebMasterFileController();
        VendorController vendor = new VendorController();

        #region Item List View
        [Route("list")]
        // GET: Item
        public ActionResult ItemList(int page = 1, int pageSize = 10)
        {
            var itemList = GenerateItemList(page, pageSize);

            // Calculate total number of pages based on the total items and page size
            int totalItems = GetTotalItemCount();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Create the PaginationViewModel to pass to the view
            PaginationViewModel viewModel = new PaginationViewModel
            {
                Items = itemList,
                Page = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        private int GetTotalItemCount()
        {
            int totalItems = 0;

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM tbl_item";
                    SqlCommand command = new SqlCommand(query, connection);
                    totalItems = (int)command.ExecuteScalar();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return totalItems;
        }
        private List<Item> GenerateItemList(int page, int pageSize)
        {
            List<Item> itemList = new List<Item>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();

                    string query = @"
                SELECT *
                FROM (
                    SELECT 
                        i.id,
                        i.item_code,
                        i.item_desc,
                        i.item_source,
                        c.name,
                        sc.name AS scname,
                        i.vat_indicator,
                        i.perishable,
                        i.with_sf,
                        ROW_NUMBER() OVER (ORDER BY i.item_code ASC) AS RowNum
                    FROM 
                        tbl_item AS i 
                        INNER JOIN tbl_category AS c ON i.item_cat = c.cat_code 
                        INNER JOIN tbl_subcategory AS sc ON i.item_sub_cat = sc.cat_code
                ) AS SubQuery
                WHERE RowNum BETWEEN @StartIndex AND @EndIndex";

                    int startIndex = (page - 1) * pageSize + 1;
                    int endIndex = page * pageSize;

                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@EndIndex", endIndex);

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Item item_dtl = new Item
                        {
                            id = Convert.ToInt32(dataReader["id"]),
                            item_code = dataReader["item_code"].ToString(),
                            item_desc = dataReader["item_desc"].ToString(),
                            perishable = Convert.ToBoolean(dataReader["perishable"]),
                            vat_indicator = Convert.ToBoolean(dataReader["vat_indicator"]),
                            with_sf = Convert.ToBoolean(dataReader["with_sf"]),
                            item_source = dataReader["item_source"].ToString(),
                            item_cat = new Category
                            {
                                //Id = Convert.ToInt32(dataReader["category_id"]),
                                Name = dataReader["name"].ToString()
                            },
                            item_sub_cat = new SubCategory
                            {
                                //Id = Convert.ToInt32(dataReader["category_id"]),
                                Name = dataReader["scname"].ToString()
                            }
                        };

                        itemList.Add(item_dtl);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return itemList;
        }
        #endregion

        #region Item Detail View
        [Route("detail")]
        [HttpGet]
        public ActionResult ItemDetail(string itemId, string itemCode)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                // Redirect to the ItemList action when itemId is null or empty
                return RedirectToAction("list", "item");
            }

            if (!IsItemCodeExists(itemCode))
            {
                TempData["errorMessage"] = "Item Code: " + itemCode + " doesn't exist.";
                return RedirectToAction("list", "item");
            }

            // Retrieve the necessary data from the database
            List<Category> categories = wmf.GetCategoryList();
            List<SubCategory> subcategories = wmf.GetSubCategoryList();
            List<Vendor> vendors = vendor.GetVendorList();
            Item itemDetail = GetItemDetail(itemId);
            List<Vendor> itemVendors = GenerateVendorToItemList(itemId);
            List<ItemLog> itemLogs = GetItemLogs(itemId); // Retrieve the item logs

            // Create SelectList objects for categories, subcategories, and vendors with selected values
            SelectList categoryList = new SelectList(categories, "Id", "Name", itemDetail.item_cat.Id);
            SelectList subcategoryList = new SelectList(subcategories, "Id", "Name", itemDetail.item_sub_cat.Id);
            SelectList vendorList = new SelectList(vendors, "Id", "Name", itemDetail.vendor_id.Id);

            // Pass the necessary data to the view
            ViewBag.Categories = categoryList;
            ViewBag.SubCategories = subcategoryList;
            ViewBag.Vendor = vendorList;
            ViewBag.ItemVendors = itemVendors;

            var model = new ItemDetailViewModel
            {
                ItemDetail = itemDetail,
                ItemVendors = itemVendors,
                ItemLogs = itemLogs,
                VendorList = vendors
            };

            // Pass the wrapper model to the view
            return View(model);
        }
        [Route("save")]
        [HttpPost]
        public ActionResult SaveItem(ItemDetailViewModel itemViewModel)
        {
            // Access the Item object from the ItemDetail property of the view model

            Item item = itemViewModel.ItemDetail;

            // Call the update method
            UpdateItem(itemViewModel);

            // Redirect back to the same view
            return RedirectToAction("ItemDetail", new { itemId = item.id, itemCode = item.item_code });
        }
        private void UpdateItem(ItemDetailViewModel itemViewModel)
        {
            Item item = itemViewModel.ItemDetail;
            SqlConnection connection = new SqlConnection(MyHelper.stringConnection);
            SqlTransaction transaction = null;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                string updateQuery = @"UPDATE tbl_item
                              SET item_code = @itemCode,
                                  item_desc = @itemDesc,
                                  item_source = @itemSource,
                                  shelf_life = @shelfLife,
                                  item_cat = @categoryId,
                                  item_sub_cat = @subCategoryId,
                                  vat_indicator = @vatIndicator,
                                  perishable = @perishable,
                                  edit_by = @editby
                              WHERE id = @itemId";

                SqlCommand command = new SqlCommand(updateQuery, connection, transaction);
                command.Parameters.AddWithValue("@itemCode", item.item_code);
                command.Parameters.AddWithValue("@itemDesc", item.item_desc);
                command.Parameters.AddWithValue("@itemSource", item.item_source);
                command.Parameters.AddWithValue("@shelfLife", item.shelf_life);
                command.Parameters.AddWithValue("@categoryId", item.item_cat.Id);
                command.Parameters.AddWithValue("@subCategoryId", item.item_sub_cat.Id);
                command.Parameters.AddWithValue("@vatIndicator", item.vat_indicator);
                command.Parameters.AddWithValue("@perishable", item.perishable);
                command.Parameters.AddWithValue("@itemId", item.id);
                command.Parameters.AddWithValue("@editby", 1);
                command.ExecuteNonQuery();

                string insertItemLogQuery = "INSERT INTO tbl_itemlogs (itemid, logdate, logdescription, logtype, userid) " +
                                            "VALUES (@itemid, GETDATE(), 'Item Update: ' + @item_code, @log_type, @user_id)";

                SqlCommand logCommand = new SqlCommand(insertItemLogQuery, connection, transaction);
                logCommand.Parameters.AddWithValue("@itemid", item.id);
                logCommand.Parameters.AddWithValue("@item_code", item.item_code);
                logCommand.Parameters.AddWithValue("@log_type", "Update");
                logCommand.Parameters.AddWithValue("@user_id", 1); // Replace "UserId" with the actual user ID

                logCommand.ExecuteNonQuery();

                // Commit the transaction
                transaction.Commit();
                TempData["successMessage"] = "Update Successfully";
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an exception
                transaction?.Rollback();
                TempData["errorMessage"] = ex.Message.ToString();
            }
            finally
            {
                transaction?.Dispose();
                connection.Close();
            }
        }
        public Item GetItemDetail(string itemId)
        {
            Item itemDetail = null;

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT
                                i.id,
                                i.item_code,
                                i.item_desc AS item_name,
                                c.id as cat_id,
                                c.name AS category_name,
                                sc.id as scat_id,
                                sc.name AS subcategory_name,                               
                                i.item_uom,
                                i.inner_per_case,
                                i.pieces_per_inner,
                                i.serving_per_case,
                                i.item_source,
                                i.perishable,
                                i.vat_indicator,
                                i.shelf_life,
                                i.activated,
                                Case
								When p.unit_price is null then 0
                                else
								p.unit_price
								end as unit_price
                            FROM
                                tbl_item AS i
                            INNER JOIN
                                tbl_category AS c ON i.item_cat = c.id 
                            INNER JOIN
                                tbl_subcategory AS sc ON i.item_sub_cat = sc.id 
							left JOIN
								 tbl_price AS p ON i.id = p.item_id
                            WHERE i.id = @itemId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemId", itemId);
                        SqlDataReader dataReader = command.ExecuteReader();

                        if (dataReader.Read())
                        {
                            itemDetail = new Item
                            {
                                id = Convert.ToInt32(dataReader["id"]),
                                item_code = dataReader["item_code"].ToString(),
                                item_desc = dataReader["item_name"].ToString(),
                                item_source = dataReader["item_source"].ToString(),
                                shelf_life = Convert.ToInt32(dataReader["shelf_life"]),
                                item_uom = dataReader["item_uom"].ToString(),
                                inner_per_case = Convert.ToInt32(dataReader["inner_per_case"]),
                                pieces_per_inner = Convert.ToInt32(dataReader["pieces_per_inner"]),
                                serving_per_case = Convert.ToInt32(dataReader["serving_per_case"]),
                                perishable = Convert.ToBoolean(dataReader["perishable"]),
                                vat_indicator = Convert.ToBoolean(dataReader["vat_indicator"]),
                                activated = Convert.ToBoolean(dataReader["activated"]),
                                unit_price = Convert.ToDecimal(dataReader["unit_price"]),
                                item_cat = new Category
                                {
                                    Id = Convert.ToInt32(dataReader["cat_id"]),
                                    Name = dataReader["category_name"].ToString()
                                },
                                item_sub_cat = new SubCategory
                                {
                                    Id = Convert.ToInt32(dataReader["scat_id"]),
                                    Name = dataReader["subcategory_name"].ToString()
                                },
                                vendor_id = new Vendor
                                {
                                    Id = Convert.ToInt32(dataReader["id"]),
                                    Name = dataReader["category_name"].ToString()
                                }
                            };
                        }

                        dataReader.Close();
                    }
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return itemDetail;
        }
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
        private List<ItemLog> GetItemLogs(string itemId)
        {
            List<ItemLog> itemLogs = new List<ItemLog>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = @"Select IL.*,U.firstname,U.lastname from tbl_itemlogs as IL
                                inner join tbl_user as U on U.id = IL.userid 
                                WHERE ItemId = @ItemId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ItemId", itemId);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ItemLog itemLog = new ItemLog
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        ItemId = reader["itemid"].ToString(),
                        Action = reader["logtype"].ToString(),
                        Description = reader["logdescription"].ToString(),
                        Date = Convert.ToDateTime(reader["logdate"]),
                        User = new User
                        {
                            FirstName = reader["firstname"].ToString(),
                            LastName = reader["lastname"].ToString()
                        }
                    };

                    itemLogs.Add(itemLog);
                }

                reader.Close();
            }

            return itemLogs;
        }
        public List<Vendor> GenerateVendorToItemList(string ItemId)
        {
            List<Vendor> vendorList = new List<Vendor>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = @"Select v.vendor_name,v.tin_no,vi.* from tbl_vendor_item as VI
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
                            TinNumber = dataReader["tin_no"].ToString(),
                            vendor_price = Convert.ToDecimal(dataReader["vendor_price"]),
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

        #region Add Item View
        [Route("add")]
        public ActionResult AddItem()
        {

            List<Category> categories = wmf.GetCategoryList();
            List<SubCategory> subcategories = wmf.GetSubCategoryList();
            //List<Vendor> vendors = GetVendorList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            //ViewBag.Vendor = new SelectList(vendors, "Id", "Name");
            ViewBag.SubCategories = new SelectList(subcategories, "Id", "Name");
            return View();
        }


        [Route("vendoritem")]
        public ActionResult ItemToVendor(string itemId, string vendorid, string itemCode)
        {
            try
            {
                if (IsItemVendorExist(itemId, vendorid))
                {
                    TempData["errorMessage"] = "Vendor already exists";
                    return RedirectToAction("ItemDetail", new { itemId = itemId, itemCode = itemCode });
                }

                using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {


                        string AddVendorIDandItemID = "Insert into tbl_vendor_item(vendor_id,item_id)" +
                                                    "VALUES (@vendorid, @itemid)";


                        using (SqlCommand addVendortoItem = new SqlCommand(AddVendorIDandItemID, connection, transaction))
                        {
                            addVendortoItem.Parameters.AddWithValue("@itemid", itemId);
                            addVendortoItem.Parameters.AddWithValue("@vendorid", vendorid);
                            addVendortoItem.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }


                TempData["successMessage"] = "Vendor was successfully added.";
                return RedirectToAction("ItemDetail", new { itemId = itemId, itemCode = itemCode });
            }
            catch (Exception ex)
            {
                // Handle the exception or log the error

                TempData["errorMessage"] = ex.Message.ToString();
                return RedirectToAction("ItemDetail", new { itemId = itemId, itemCode = itemCode });
            }
        }

        private bool IsItemVendorExist(string itemId, string vendorId)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM tbl_vendor_item WHERE item_id = @item_id and vendor_id = @vendor_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@item_id", itemId);
                    command.Parameters.AddWithValue("@vendor_id", vendorId);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        #endregion

        #region Item Pricelist
        [Route("pricelist")]
        public ActionResult ItemPricelist(int page = 1, int pageSize = 10)
        {
            var itemList = GetPricelist(page, pageSize);

            // Calculate total number of pages based on the total items and page size
            int totalItems = GetTotalItemPricelistCount();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Create the PaginationViewModel to pass to the view
            PaginationViewModel viewModel = new PaginationViewModel
            {
                Items = itemList,
                Page = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        public List<Item> GetPricelist(int page, int pageSize)
        {
            List<Item> itemList = new List<Item>();

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();

                    string query = @"SELECT * FROM (
                    SELECT 
                        i.id,
                        i.item_code,
                        i.item_desc,
						case
						when p.unit_price  is null then 0
						else p.unit_price
						End as unit_price
                        ,
						Case
						when p.effective_date  is null then getdate()
						else p.effective_date
						End as effective_date
						,
                        ROW_NUMBER() OVER (ORDER BY i.item_code ASC) AS RowNum
                    FROM 
                        tbl_item AS i 
                        INNER JOIN tbl_category AS c ON i.item_cat = c.cat_code 
                        INNER JOIN tbl_subcategory AS sc ON i.item_sub_cat = sc.cat_code
						left join tbl_price as p on p.item_id = i.id
                ) AS SubQuery
                WHERE RowNum BETWEEN @StartIndex AND @EndIndex"
                    ;

                    int startIndex = (page - 1) * pageSize + 1;
                    int endIndex = page * pageSize;

                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@EndIndex", endIndex);

                    SqlDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Item item_dtl = new Item
                        {
                            id = Convert.ToInt32(dataReader["id"]),
                            item_code = dataReader["item_code"].ToString(),
                            item_desc = dataReader["item_desc"].ToString(),
                            unit_price = Convert.ToDecimal(dataReader["unit_price"]),
                            effective_date = Convert.ToDateTime(dataReader["effective_date"]),
                        };

                        itemList.Add(item_dtl);
                    }

                    dataReader.Close();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return itemList;
        }
        private int GetTotalItemPricelistCount()
        {
            int totalItems = 0;

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM tbl_price";
                    SqlCommand command = new SqlCommand(query, connection);
                    totalItems = (int)command.ExecuteScalar();
                    command.Dispose();
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = ex.Message.ToString();
                }
            }

            return totalItems;
        }
        #endregion

        #region Insert Item
        [HttpPost]
        public async Task<ActionResult> InsertItem(Item item)
        {
            try
            {
                if (IsItemCodeExists(item.item_code))
                {
                    TempData["errorMessage"] = "Item Code: " + item.item_code + " already exists.";
                    return RedirectToAction("add", "item");
                }

                await InsertSingleItemAsync(item);
                await LogItemInsertionAsync(item);

                bool isEmailSent = await SendEmailNotificationAsync(item.item_code, item.item_desc, User.Identity.Name);

                if (isEmailSent)
                {
                    TempData["successMessage"] = "Item Code: " + item.item_code + " - " + item.item_desc + " was successfully added and email notification sent.";
                }
                else
                {
                    TempData["errorMessage"] = "Item Code: " + item.item_code + " - " + item.item_desc + " was successfully added, but failed to send email notification.";
                }
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message.ToString();
            }

            return RedirectToAction("add", "item");
        }

        [HttpPost]
        public async Task<ActionResult> InsertItems(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                List<Item> insertedItems = new List<Item>();

                using (var reader = new StreamReader(file.InputStream))
                {
                    reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        var item = new Item
                        {
                            item_code = values[0],
                            item_desc = values[1],
                            perishable = Convert.ToBoolean(values[2]),
                            item_source = values[3],
                            shelf_life = Convert.ToInt32(values[4]),
                            item_cat = new Category { Id = Convert.ToInt32(values[5]) },
                            item_sub_cat = new SubCategory { Id = Convert.ToInt32(values[6]) }
                            // Add other properties as necessary
                        };

                        await InsertSingleItemAsync(item);
                        await LogItemInsertionAsync(item);
                        insertedItems.Add(item);
                    }
                }

                if (insertedItems.Count > 0)
                {
                    foreach (var item in insertedItems)
                    {
                        bool isEmailSent = await SendEmailNotificationAsync(item.item_code, item.item_desc, User.Identity.Name);

                        if (isEmailSent)
                        {
                            TempData["successMessage"] += "Item Code: " + item.item_code + " - " + item.item_desc + " was successfully added and email notification sent.\n";
                        }
                        else
                        {
                            TempData["errorMessage"] += "Item Code: " + item.item_code + " - " + item.item_desc + " was successfully added, but failed to send email notification.\n";
                        }
                    }
                }

                TempData["successMessage"] += "Items were successfully added.";
            }
            else
            {
                TempData["errorMessage"] = "No file uploaded.";
            }

            return RedirectToAction("add", "item");
        }

        private async Task InsertSingleItemAsync(Item item)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    string insertItemQuery = "INSERT INTO tbl_item (item_code, item_desc, perishable, item_source, shelf_life, item_cat, item_sub_cat, created_by,activated) " +
                                                "VALUES (@item_code, @item_desc, @perishable, @item_source, @shelf_life, @item_cat, @item_sub_cat, @created_by, 1); " +
                                                "SELECT SCOPE_IDENTITY();";

                    using (SqlCommand itemCommand = new SqlCommand(insertItemQuery, connection, transaction))
                    {
                        itemCommand.Parameters.AddWithValue("@item_code", item.item_code);
                        itemCommand.Parameters.AddWithValue("@item_desc", item.item_desc);
                        itemCommand.Parameters.AddWithValue("@perishable", item.perishable);
                        itemCommand.Parameters.AddWithValue("@item_source", item.item_source);
                        itemCommand.Parameters.AddWithValue("@shelf_life", item.shelf_life);
                        itemCommand.Parameters.AddWithValue("@item_cat", item.item_cat.Id);
                        itemCommand.Parameters.AddWithValue("@item_sub_cat", item.item_sub_cat.Id);
                        itemCommand.Parameters.AddWithValue("@created_by", MySession.Current.UserID);

                        int itemId = Convert.ToInt32(await itemCommand.ExecuteScalarAsync());
                        item.id = itemId;
                    }

                    await Task.Run(() => transaction.Commit());
                }
            }
        }

        private async Task LogItemInsertionAsync(Item item)
        {
            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    string insertItemLogQuery = "INSERT INTO tbl_itemlogs (itemid, logdate, logdescription, logtype, userid) " +
                                                "VALUES (@itemid, GETDATE(), 'Item added: ' + @item_code, @log_type, @user_id)";

                    using (SqlCommand itemLogCommand = new SqlCommand(insertItemLogQuery, connection, transaction))
                    {
                        itemLogCommand.Parameters.AddWithValue("@itemid", item.id);
                        itemLogCommand.Parameters.AddWithValue("@item_code", item.item_code);
                        itemLogCommand.Parameters.AddWithValue("@log_type", "New Item: " + item.item_code);
                        itemLogCommand.Parameters.AddWithValue("@user_id", MySession.Current.UserID);

                        await itemLogCommand.ExecuteNonQueryAsync();
                    }

                    await Task.Run(() => transaction.Commit());
                }
            }
        }

        #endregion
              
        #region AUTO EMAIL
        // Method to send email notification
        [ValidateAntiForgeryToken]
        private async Task<bool> SendEmailNotificationAsync(string itemCode, string itemDescription, string fullName)
        {
            try
            {
                MailAddress from = new MailAddress(MyHelper.EmailSenderAccountEmail);
                string[] receiverEmails = GetReceiverEmails();
                MailMessage message = new MailMessage();
                message.From = from;
                foreach (string receiverEmail in receiverEmails)
                {
                    message.To.Add(receiverEmail);
                }
                message.Subject = "Item Added Notification";
                string headTag = "<html lang=\"en\">" +
                          "<head>" +
                          "<meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\">" +
                          "<style type=\"text/css\">" +
                          "#backgroundColor{" +
                          "background-color: #ECF0F1;" +
                          "padding: 15px 0;" +
                          "}" +
                          ".detailsContainer{" +
                          "margin: 0 15px;" +
                          "}" +
                          "#moreDetailsContainer, #subjectAndMessageContainer{" +
                          "font-size: 15px;" +
                          "}" +
                          "#moreDetailsContainer, #subjectAndMessageContainer{" +
                          "background-color: #fff;" +
                          "color: #000;" +
                          "padding: 15px;" +
                          "}" +
                          "#messageSection{" +
                          "text-align: justify;" +
                          "text-justify: inter-character;" +
                          "margin: 0 30px;" +
                          "padding: 20px 40px;" +
                          "}" +
                          "</style>" +
                          "</head>";

                string bodyTag = $@"
                    <body>
                        <div id=""backgroundColor"">
                            <div class=""detailsContainer"">
                                <div id=""moreDetailsContainer"">
                                    <center><img src=""https://www.leadlogistics.com.ph/ews/wp-content/uploads/2022/03/logo-200x154.jpg"" class=""img-responsive"" /></center><br>
                                    Dear Team, <br> A message was sent in our Web Portal. Please see details below:<br><br>
                                    <b>Item Code: </b> {itemCode} <br>                
                                    <b>Description: </b> {itemDescription} <br>
                                    <b>Date: </b> {DateTime.Now.Date.ToString("MM/dd/yyyy")} <br>
                                    <b>Created by: </b> {fullName} <br>
                                </div>
                                <br>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                string body = headTag + bodyTag;


                message.IsBodyHtml = true;
                message.Body = body;
                // Send the email
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential(MyHelper.EmailSenderAccountEmail, MyHelper.EmailSenderAccountPassword);
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = networkCredential;
                await smtpClient.SendMailAsync(message);

                return true; // Email sent successfully
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.ToString();
                // Handle the exception or log the error
                return false; // Failed to send email
            }
        }

        [ValidateAntiForgeryToken]
        public static string[] GetReceiverEmails()
        {
            string value = ConfigurationManager.AppSettings["EmailReceiver_Account_Email"];
            string[] receiverEmails = value.Split(';');

            return receiverEmails;
        }

        #endregion

        #region ITEM PRICE 

        [HttpGet]    
        [Route("priceupdate")]
        public ActionResult ItemPriceUpdate()
        {
            return View();
        }
        public ActionResult DownloadTemplate()
        {
            // Retrieve data from the database using ADO.NET
            DataTable dataTable = RetrieveDataFromDatabase();

            // Generate the CSV content
            string csvContent = GenerateCsvContent(dataTable);

            // Set the response headers
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=PricelistTemplate.csv");

            // Write the CSV content to the response
            Response.Write(csvContent);
            Response.End();

            return null; // Important: Returning null to prevent further processing of the request
        }
        private DataTable RetrieveDataFromDatabase()
        {
            // Your ADO.NET code to retrieve data from the database
           
            string query = "SELECT item_id,item_code,new_value,effective_date FROM tbl_price_update";

            using (SqlConnection connection = new SqlConnection(MyHelper.stringConnection))
            {
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                return dataTable;
            }
        }
        private string GenerateCsvContent(DataTable dataTable)
        {
            StringBuilder csvContent = new StringBuilder();

            // Append the column headers to the CSV content
            csvContent.AppendLine(string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName)));

            // Append the data rows to the CSV content
            foreach (DataRow row in dataTable.Rows)
            {
                // Format the "item_code" column as text by adding an apostrophe before the value
                string itemCode = $"'{row["item_code"]}'";
                row["item_code"] = itemCode;

                csvContent.AppendLine(string.Join(",", row.ItemArray));
            }

            return csvContent.ToString();
        }
        #endregion
    
    }

}