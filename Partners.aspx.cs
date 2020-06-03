using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;

namespace Saved
{
    public partial class _Partners : Page
    {

        public string GetPartners()
        {

            //http://san.biblepay.org/Mission/Logos/JesusLogo.jpg
            //http://san.biblepay.org/Mission/Logos/CameroonOneLogo.png
            //http://san.biblepay.org/Mission/Logos/CompassionLogo.png
            //http://san.biblepay.org/Mission/Logos/KairosLogo.png

            string sNarr = "<h2 align=center><br>BIBLEPAY - DECENTRALIZED WEB</h3><br><br>From here, you can view Christian Spaces, Accountability, Orphans, and more.<br><br><br><br><br>";
            string sLogos = "JesusLogo.jpg|CameroonOneLogo.png|CompassionLogo.png|KairosLogo.png|DashPayLogo.png|boinclogo.jpg|SouthXChangeLogo.png";
            string sClicks = "jesus-christ.us/Jesus/JesusChrist.htm|cameroonone.org|compassion.com|kairoschildrensfund.com|dash.org|boinc.berkeley.edu|www.southxchange.com/Market/Book/BBP/BTC";
            string[] vLogos = sLogos.Split(new string[] { "|" }, StringSplitOptions.None);
            string[] vURLS = sClicks.Split(new string[] { "|" }, StringSplitOptions.None);
            string sPartners = "<table border=0><tr><td colspan=5><b>Partners Spotlight:</b></td></tr>"
                + "<tr>";
            int iCols = 4;
            int iColNo = 0;
            for (int i = 0; i < vLogos.Length; i++)
            {

                string sURL = vURLS[i];
                sPartners += "<td><a href=http://" + sURL + "><img width=225 height=100 src=http://san.biblepay.org/Mission/Logos/" + vLogos[i] + " /></a>&nbsp;&nbsp;</td>";
                iColNo++;
                if (iColNo == iCols)
                {
                    iColNo = 0;
                    sPartners += "</tr><tr>";
                }
            }

            sPartners += "</tr></table>";
            return sPartners;

            // Metrics

            //sNarr += "<br>Helping " + nOrphanCount.ToString() + " Orphans globally through POOM and our sponsorships through our partners.";
            //sNarr += "<br><br><br><b>Upgrade News:</b><br><br>POBH 2.0 Being Released:  Please upgrade to 1.4.8.7 by Jan 21st, 2020 to avoid going on a fork. <br> ";
        }


        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}