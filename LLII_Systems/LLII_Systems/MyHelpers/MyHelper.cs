using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LLII_Systems.Helpers
{
    public class MyHelper
    {
        public MySession session = MySession.Current;
        public static string stringConnection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        static string _emailSender_Account_Email = ConfigurationManager.AppSettings["EmailSender_Account_Email"];
        static string _emailSender_Account_Password = ConfigurationManager.AppSettings["EmailSender_Account_Password"];
        static string _emailReceiver_Account_Email = ConfigurationManager.AppSettings["EmailReceiver_Account_Email"];
       
        public static string EmailSenderAccountPassword
        {
            get { return _emailSender_Account_Password; }
        }
        public static string EmailSenderAccountEmail
        {
            get { return _emailSender_Account_Email; }
        }
        public static string EmailReceiverAccountEmail
        {
            get { return _emailReceiver_Account_Email; }
        }
    }
}