using System;
using System.Data;
using System.Text;
using System.Web.UI;
using static Saved.Code.Common;

namespace Saved
{
    public partial class Viewer : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        
        public string GetArticle()
        {

            string sURL = Request.QueryString["target"] ?? "";

            if (sURL == "collage")
            {
                string sql = "Select * from Orphans order by charity, childid";
                DataTable dt = gData.GetDataTable(sql);
                string sHTML = "<table cellpadding=7><tr>";
                int iTD = 0;
                string sErr = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // Each Orphan should be a div with their picture in it
                    string sMyBIO = dt.Rows[i]["URL"].ToString();
                    string sName = dt.Rows[i]["ChildID"].ToString() + " - " + dt.Rows[i]["Charity"].ToString();
                    string sImg = ScrapeImage(sMyBIO, dt.Rows[i]["Charity"].ToString(), sName);

                    if (sImg != "")
                    {
                        string sMyOrphan = "<td style='border=1px solid black'><a href='" + sMyBIO + "'>" + sName
                            + "<br><img style='width:300px;height:250px' src='" + sImg + "'></a></td>";

                        sHTML += sMyOrphan;
                        iTD++;
                        if (iTD > 2)
                        {
                            iTD = 0;
                            sHTML += "<td width=30%>&nbsp;</td></tr><tr>";
                        }
                    }
                    else
                    {
                        sErr += "<a href='" + sMyBIO + "'>Missing</a>";
                    }
                }
                sHTML += "</TR></TABLE>";
                if (gUser(this).Admin)
                {
                    sHTML += sErr;
                }
                return sHTML;

            }
            else             if (sURL != "")
            {
                string sDec = System.Web.HttpUtility.UrlDecode(sURL);
                string sIframe = "<iframe width=95% style='height: 80vh;' src='" + sDec + "'></iframe>";
                return sIframe;
            }

            string sArticle = Request.QueryString["ref"];
            if (sArticle != "")
            {
                string sPath = Server.MapPath("JesusChrist/" + sArticle + ".htm");

                if (System.IO.File.Exists(sPath))
                {
                    string sData = System.IO.File.ReadAllText(sPath, Encoding.Default);
                    // Remove legacy Logos data (Logos is the verse popup reference), and use our intrinsic javascript popup we reference in Site.Master
                    string s1 = "<SCRIPT" + ExtractXML(sData, "<SCRIPT", "</SCRIPT>").ToString() + "</SCRIPT>";
                    sData = sData.Replace(s1, "");
                    s1 = "<SCRIPT>" + ExtractXML(sData, "<SCRIPT>", "</SCRIPT>").ToString() + "</SCRIPT>";
                    sData = sData.Replace(s1, "");
                    return sData;
                }
            }

            return "n/a";

        }
    }
}