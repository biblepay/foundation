using OpenHtmlToPdf;
using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;

namespace Saved
{
    public partial class Admin1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnConvert_Click(object sender, EventArgs e)
        {
            // Convert unconverted RequestVideo to Rapture videos
            string sql = "Select * from RequestVideo where status is null";

            DataTable dt = gData.GetDataTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string url = dt.Rows[i]["url"].ToString();
                // Convert this particular youtube URL into a rapture video
                // Then store in the rapture table with an uncategorized category
                GetVideo(url);
                string sPath = GetPathFromTube(url);
                // Convert the path to hash
                string sNewFileName = "700" + sPath.GetHashCode().ToString() + ".mp4";

                System.IO.FileInfo fi = new FileInfo(sPath);
                string sHeading = Chop(fi.Name, 16);

                string notes = sHeading + "\r\n\r\n"+ Chop(GetNotes(sPath), 4000);
                string sNewPath = "s:\\Rapture\\" + sNewFileName;

                fi.CopyTo(sNewPath,true);

                sql = "Insert into Rapture (id,added,Notes,URL,timestamp,FileName,Category) values (newid(), getdate(), @notes, @url, 0, @filename, @category)";
                SqlCommand command = new SqlCommand(sql);
                command.Parameters.AddWithValue("@notes", notes);
                string sNewUrl = "https://video.biblepay.org/" + sNewFileName;
                command.Parameters.AddWithValue("@url", sNewUrl);
                command.Parameters.AddWithValue("@filename", sNewFileName);
                command.Parameters.AddWithValue("@category", "Miscellaneous");

                gData.ExecCmd(command);

                sql = "Update RequestVideo set Status='FILLED' where id = '" + dt.Rows[i]["id"].ToString() + "'";

                gData.Exec(sql);

            }

            lblStatus.Text = "Updated " + DateTime.Now.ToString();

        }

        protected void btnPDF_Click(object sender, EventArgs e)
        {
            string sql = "select id,added as a1, FORMAT (added, 'MMMM yyyy') as Added,'DR' as Type,Amount,Charity, '' as Notes from expense "
                + " union all  select id, added as a1, format(added, 'MMMM yyyy'), 'CR' as Type,Amount, Charity, Notes from Revenue  order by a1 ";
            string html = GetTableHTML(sql);
            var result = Pdf.From(html).Portrait().Content();
            ByteArrayToFile("c:\\test_pdf.pdf", result);
        }
    }
}