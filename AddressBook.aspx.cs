using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.EntityHelper;

namespace Saved
{
    public partial class AddressBook : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private string GOV(string data)
        {
            //Get obfuscated value
            string c = "<span style='font-family:OCR A'><small>" + Left(data, 8) + "...</small></span>";
            return c;
        }

        private string ENC(string sData)
        {
            string e = BiblePayCommon.Encryption.EncryptAES256(sData, GetBMSConfigurationKeyValue("QEV2"));
            return e;
        }
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (gUser(this).LoggedIn == false)
            {
                MsgBox("Log In Error", "Sorry, you must be logged in first.", this);
                return;
            }

            try
            {
                string sql = "Insert Into AddressBook (id, Added, LastName, FirstName, AddressLine1, AddressLine2, City, State, PostalCode, Country, UserId) "
                    + " values (newid(), getdate(), @lastname, @firstname, @addressline1, @addressline2, @city, @state, @postalcode, @country, @userid)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@lastname", ENC(txtLastName.Text));
                command.Parameters.AddWithValue("@firstname", ENC(txtFirstName.Text));
                command.Parameters.AddWithValue("@addressline1", ENC(txtAddressLine1.Text));
                command.Parameters.AddWithValue("@addressline2", ENC(txtAddressLine2.Text));
                command.Parameters.AddWithValue("@city", ENC(txtCity.Text));
                command.Parameters.AddWithValue("@state", ENC(txtState.Text));
                command.Parameters.AddWithValue("@postalcode", ENC(txtPostalCode.Text));
                command.Parameters.AddWithValue("@country", txtCountry.Text);
                command.Parameters.AddWithValue("@userid", gUser(this).UserId.ToString());

                gData.ExecCmd(command, false, false, true);
            }catch(Exception ex)
            {
                MsgBox("Error", "An error occurred while saving the address book entry. ", this);
                Log("addr book add: " + ex.Message);

            }


        }
        protected string GetAddressBookList()
        {
            string sql = "Select * from AddressBook where userid='" + BMS.PurifySQL(gUser(this).UserId.ToString(),20) + "' order by LastName";
            DataTable dt = gData.GetDataTable2(sql);
            string html = "<table class=saved><tr><th width=20%>Last Name</th><th>First Name<th>Address Line 1<th>Address Line 2<th>City<th>State<th>Postal Code<th>Added</tr>";
            for (int y = 0; y < dt.Rows.Count; y++)
            {

                string div = "<tr>"
                    + "<td>" + GOV(dt.Rows[y]["LastName"].ToString())
                    + "<td>" + GOV(dt.Rows[y]["FirstName"].ToString())
                    + "<td>" + GOV(dt.Rows[y]["AddressLine1"].ToString())
                    + "<td>" + GOV(dt.Rows[y]["AddressLine2"].ToString())
                    + "<td>" + GOV(dt.Rows[y]["City"].ToString())
                    + "<td>" + GOV(dt.Rows[y]["State"].ToString())
                    + "<td>" + GOV(dt.Rows[y]["PostalCode"].ToString())
                    + "<td>" + dt.Rows[y]["Added"].ToString()
                    + "</tr>";
                html += div + "\r\n";
            }
            html += "</table>";
            return html;
        }
    }
}