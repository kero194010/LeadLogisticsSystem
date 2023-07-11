using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LLII_Systems.Helpers
{
    [Serializable]
    public class MySession
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
  

        public static MySession Current
        {
            get
            {
                MySession session =
                  (MySession)HttpContext.Current.Session["__MySession__"];
                if (session == null)
                {
                    session = new MySession();
                    HttpContext.Current.Session["__MySession__"] = session;
                }
                return session;
            }
        }

    }
}