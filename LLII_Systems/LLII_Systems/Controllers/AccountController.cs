using LLII_Systems.Helpers;
using LLII_Systems.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace LLII_Systems.Controllers
{

    [RoutePrefix("account")]
    public class AccountController : Controller
    {
        private MySession session = MySession.Current;
        // GET: Account
        [AllowAnonymous]
        [Route("login")]
        public ActionResult Login()
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return View();
            }
            return View();
        }
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult> Login(User model) //Login
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await Task.Run(() => IsAccountValid(model));
            if (response == true)
            {
                session.UserID = GetUserID(model.UserName);
                session.Username = model.UserName;

                //if true, session continues after closing the page
                //if false, session cuts after closing the page
                FormsAuthentication.SetAuthCookie(model.UserName, true);
                TempData["successMessage"] = "Welcome! LLII Systems " + session.Username;

                return RedirectToAction("", ""); //Go to home index
            }
            else
            {
                TempData["errorMessage"] = "Invalid Username and/or Password";
                //ModelState.AddModelError("", "Invalid Username and/or Password");
                return View();
            }


        }

        [HttpGet]
        public int GetUserID(string logon)
        {
            int response = 0;
            SqlConnection connection = new SqlConnection(MyHelper.stringConnection);
            try
            {
                connection.Open();

                string query = @"SELECT id
                FROM tbl_user WHERE username = @Logon OR emp_id = @Logon";

                SqlCommand command = new SqlCommand(query, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@Logon", logon);
                response = Convert.ToInt32(command.ExecuteScalar());

                command.Dispose();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("login", "account"); //redirect to index
        }

        public bool IsAccountValid(User user)
        {
            bool response = false;

            SqlConnection connection = new SqlConnection(MyHelper.stringConnection);
            try
            {
                Encryptionv2.Encryptionv2 encryptv2 = new Encryptionv2.Encryptionv2();
                string encryptedPassword = encryptv2.EncryptPassword(user.Password);

                connection.Open();

                string query = @"Select * FROM tbl_user U 
                                WHERE U.Password = @Password 
                                AND (U.username = @Logon 
                                OR U.emp_id = @Logon)";
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@Password", encryptedPassword);
                command.Parameters.AddWithValue("@Logon", user.UserName);
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read() == true)
                {
                    response = true;
                }
                dataReader.Close();
                command.Dispose();
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "An error has occured. " + ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return response;
        }


        // View: Add User
        public ActionResult AddUser()
        {

            return View();
        }

    }
}