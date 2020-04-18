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

namespace Saved
{
    public partial class Markup : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string editType = Request.QueryString["type"].ToNonNullString();
            if (!gUser(this).Admin)
            {
                MsgBox("Restricted", "Sorry this page is for admins only.", this);
                return;
            }
            if (editType == "Rapture" && IsPostBack == false)
            {
                string sql = "Select * from Rapture where id=@id";

                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@id", Request.QueryString["id"].ToString());


                txtTitle.Text = Request.QueryString["id"].ToString();
                txtBody.Text = gData.GetScalarString(command, "Notes");
                lblStatus.Text = "Loaded";
            }
        }
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string editType = Request.QueryString["type"].ToString();

            if (editType == "Rapture")
            {
                string sql = "Delete from Rapture Where id = @id";
                SqlCommand command = new SqlCommand(sql);
                command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@id", Request.QueryString["id"].ToString());
                gData.ExecCmd(command);
                lblStatus.Text = "Updated " + DateTime.Now.ToString();
            }
        }

        protected void btnCopy_Click(object sender, EventArgs e)
        {
            string sTerm = txtName.Text;
            Data dSource = new Data(Data.SecurityType.REPLICATOR);
            string sql = "Select * from bms..rapture where notes like '%" + sTerm + "%'";
            string  sCategory = sTerm;

            DataTable dt = dSource.GetDataTable(sql);
            int i = 0;
            for (i = 0; i < dt.Rows.Count; i++)
            {
                sql = "Insert into Rapture (id,added,notes,url,timestamp,filename,category) values ('" + dt.Rows[i]["id"].ToString() + "', getdate(), '" 
                    + dt.Rows[i]["Notes"].ToString() + "','" + dt.Rows[i]["Url"].ToString() + "',0,'" 
                    + dt.Rows[i]["FileName"].ToString() + "','" + sCategory + "')";
                gData.Exec(sql);

            }
            lblStatus.Text = "Updated " + i.ToString() + "rows";

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string editType = Request.QueryString["type"].ToNonNullString();

           if (editType == "Rapture")
            {

                string sql = "Update Rapture Set Notes=@notes where id=@id";
                SqlCommand command = new SqlCommand(sql);
                command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@notes", txtBody.Text);
                command.Parameters.AddWithValue("@id", Request.QueryString["id"].ToString());
                gData.ExecCmd(command);
                lblStatus.Text = "Updated " + DateTime.Now.ToString();
            }
            else
            {
                string body = txtBody.Text;
                string sql = "Delete from Markup where name=@name and stepno=@stepno";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@stepno", txtStepNo.Text);
                command.Parameters.AddWithValue("@name", txtName.Text);
                gData.ExecCmd(command);
                sql = "Insert into Markup(id,body,name,stepno,added,Title) values (newid(),@body,@name,@step,getdate(),@title)";
                command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@body", body);
                command.Parameters.AddWithValue("@name", txtName.Text);
                command.Parameters.AddWithValue("@step", txtStepNo.Text);
                command.Parameters.AddWithValue("@title", txtTitle.Text);
                gData.ExecCmd(command);
                lblStatus.Text = "Updated " + DateTime.Now.ToString();
            }

        }
        protected void btnLoad_Click(object sender, EventArgs e)
        {
            Data d = new Data(Data.SecurityType.REQ_SA);
           if (true)
            {
                string sql = "Select * from markup where stepno=@step and name=@name";

                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@step", txtStepNo.Text);
                command.Parameters.AddWithValue("@name", txtName.Text);

                string body = d.GetScalarString(command, "body");
                txtTitle.Text = d.GetScalarString(command, "title");
                txtBody.Text = body;
                lblStatus.Text = "Loaded";
            }
        }
    }
}