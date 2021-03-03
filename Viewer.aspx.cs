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
                string sql = "select ChildID,Charity,URL,sponsoredOrphan.Added,Name,BIOURL,AboutCharity,users.username  from sponsoredOrphan "
                    + " left join Users on Users.ID = SponsoredOrphan.Userid  Where ChildID != 'VARIOUS-TBD' and childid != 'TBD'  and active=1 order by Charity,Name";
                DataTable dt = gData.GetDataTable(sql);
                string sHTML = "<table><tr>";
                int iTD = 0;
                string sErr = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // Each Orphan should be a div with their picture in it
                    string sMyBIO = dt.Rows[i]["URL"].ToString();
                    string sName = dt.Rows[i]["ChildID"].ToString() + " - " + dt.Rows[i]["Charity"].ToString();
                    string sBioImg = dt.Rows[i]["BioURL"].ToString();
                    string sSponsoredBy = dt.Rows[i]["Username"].ToString() == "" ? "BiblePay" : dt.Rows[i]["Username"].ToString();

                    if (sBioImg != "")
                    {
                        string sMyOrphan = "<td style='padding:7px;border:1px solid lightgrey' cellpadding=7 cellspacing=7><a href='" + sMyBIO + "'>" + sName
                            + "<br>Sponsored by: " + sSponsoredBy + "<br><img style='width:300px;height:250px' src='" + sBioImg + "'></a><br></td>";

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