using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web.UI.DataVisualization.Charting;
using static Saved.Code.Common;

namespace Saved.Code
{
    public static class RokuOperations
    {


        public static string CleanHTML1(string sTitle)
        {
            sTitle = sTitle.Replace("\"", "`");
            sTitle = sTitle.Replace("&", " and ");
            sTitle = sTitle.Replace("\r", " ");
            sTitle = sTitle.Replace("\n", " ... ");
            return sTitle;
        }

        public static void GenerateMediaListXML()
        {

            try
            {
                //mediaplaygrid.xml = The Rapture drill in CATEGORIES
                string sql = "Select * from RaptureCategories order by Category";
                DataTable dt0 = Saved.Code.Common.gData.GetDataTable2(sql);
                for (int k = 0; k < dt0.Rows.Count; k++)
                {
                    string sCategory = dt0.Rows[k]["category"].ToString();

                    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>\r\n";
                    xml += "<Content>\r\n";

                    sql = "Select * from Rapture where category = '" + sCategory + "' order by Title";
                    DataTable dt = Saved.Code.Common.gData.GetDataTable2(sql);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string sTitle = Saved.Code.Common.Left(dt.Rows[i]["title"].ToString(), 50);

                        string sNotes = Saved.Code.Common.Left(dt.Rows[i]["notes"].ToString(), 200);
                        sNotes = CleanHTML1(sNotes);
                        sTitle = CleanHTML1(sTitle);
                        string sVideoURL = dt.Rows[i]["url"].ToString();
                        sVideoURL = sVideoURL.Replace("https://", "http://");
                        string sItem = "<item hdposterurl=\"" + dt.Rows[i]["thumbnail"].ToString() + "\" streamformat=\"mp4\" url=\"" + sVideoURL + "\" "
                            + " title=\"" + sTitle + "\" description=\"" + sNotes + "\" />";

                        xml += sItem + "\r\n";
                    }
                    xml += "</Content>";
                    System.IO.File.WriteAllText(GetFolderUploads("")+ "\\list_" + sCategory + ".xml", xml);

                }

            }
            catch (Exception ex)
            {
                Saved.Code.Common.Log("GMLX " + ex.Message);
            }
        }

        public static void GenerateMediaPlayGrid()
        {
            try
            {
                //mediaplaygrid.xml = The Rapture drill in CATEGORIES
                string sql = "Select * from RaptureCategories order by Category";
                DataTable dt = Saved.Code.Common.gData.GetDataTable2(sql);
                string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>\r\n";
                xml += "<Content>\r\n";
                int x = 0;
                int y = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string sItem = "<item hdgridposterurl=\"" + dt.Rows[i]["url"].ToString() + "\" shortdescriptionline1=\"" + dt.Rows[i]["category"].ToString()
                        + "\" shortdescriptionline2=\"componentVideoList\" x=\"" + x.ToString() + "\" y=\"" + y.ToString() + "\" />";
                    x++;
                    if (x == 2)
                    {
                        y++;
                        x = 0;
                    }

                    xml += sItem + "\r\n";
                }
                xml += "</Content>";
                
                System.IO.File.WriteAllText(GetFolderUploads("") + "\\mediaplaygrid.xml", xml);
            }
            catch (Exception ex)
            {
                Saved.Code.Common.Log("WMPG " + ex.Message);
            }
        }
    }
}
