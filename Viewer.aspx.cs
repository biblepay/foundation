using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
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
                string sql = "select * from sponsoredOrphan "
                    + " where active=1 order by Charity,Name";
                DataTable dt = gData.GetDataTable2(sql);
                string sHTML = "<table><tr>";
                int iTD = 0;
                string sErr = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // Each Orphan should be a div with their picture in it
                    string sMyBIO = dt.Rows[i]["BioURL"].ToString();
                    string sName = dt.Rows[i]["ChildID"].ToString() + " - " + dt.Rows[i]["Charity"].ToString();
                    string sBioImg = dt.Rows[i]["BioPicture"].ToString();
                    if (sBioImg != "")
                    {
                        string sMyOrphan = "<td style='padding:7px;border:1px solid lightgrey' cellpadding=7 cellspacing=7><a href='" + sMyBIO + "'>" + sName
                            + "<br><img style='width:300px;height:250px' src='" + sBioImg + "'></a><br></td>";
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
            else if (sURL != "")
            {
                sURL = sURL.Replace("javascript", "");
                sURL = sURL.Replace("script:", "");
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                string sCleansed = rgx.Replace(sURL.ToLower(), "");
                bool fOK = sURL.StartsWith("https://minexmr.com/dashboard") || sURL.StartsWith("https://www.freebibleimages.org/") || sURL.StartsWith("https://wiki.biblepay.org/");
                if (sURL.ToLower().Contains("script") || sURL.ToLower().Contains("javascript") || sURL.ToLower().Contains("(") || sURL.Contains(")"))
                    fOK = false;
                if (sURL.ToLower().Contains("javas	cript"))
                    fOK = false;
                if (sCleansed.Contains("java") || sCleansed.Contains("confirm"))
                    fOK = false;

                if (fOK)
                {
                    string sDec = System.Web.HttpUtility.UrlDecode(sURL);
                    sDec = Server.HtmlEncode(sDec);
                    string sIframe = "<iframe width=95% style='height: 80vh;' src='" + sDec + "'></iframe>";
                    return sIframe;
                }
                else
                {
                    MsgBox("Security Error", "Sorry, this domain is restricted.", this);
                }
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