using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class LP : Page
    {

        protected string GetROIGauge()
        {
            try
            {
                string s = RenderGauge(250, "HODL %", (int)(GetEstimatedHODL(true, .10) * 100));
                return s;
            }
            catch (Exception ex)
            {
                return "";
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            // Increment the Counter
            IncSysByFloat("GoogleAd", 1);

        }


        public string GetId()
        {
            string id = Saved.Code.BMS.PurifySQL(Request.QueryString["id"].ToNonNullString(), 50);
            return id;
        }
    }
}