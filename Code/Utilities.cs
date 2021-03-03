using Google.Authenticator;
using Microsoft.VisualBasic;
using MimeKit;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using static Saved.Code.PoolCommon;

namespace Saved.Code
{



    public static class Utils
    {
        public static string CleanseHeading(string sMyHeading)
        {
            int iPos = 0;
            for (int i = 0; i < sMyHeading.Length; i++)
            {
                if (sMyHeading.Substring(i, 1) == "-")
                    iPos = i;
            }
            if (iPos > 1)
            {
                string sOut = sMyHeading.Substring(0, iPos - 1);
                return sOut;
            }
            return sMyHeading;
        }

        public static string GetEle(string data, string delim, int iEle)
        {
            string[] vData = data.Split(delim);
            if (iEle <= vData.Length - 1)
            {
                string d = vData[iEle];
                return d;
            }
            return "";
        }


    }

}
