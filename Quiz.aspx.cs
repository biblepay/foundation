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
using static Saved.Code.EntityHelper;
using static Saved.Code.PoolCommon;

namespace Saved
{
    public partial class Quiz : Page
    {
        // Grab a bible verse from the Core Wallet KJV class
        NBitcoin.Crypto.KJV kjv = new NBitcoin.Crypto.KJV();
        protected void Page_Load(object sender, EventArgs e)
        {
            string sID = Request.QueryString["id"] ?? "";
            string sBBP = Request.QueryString["bbpaddress"] ?? "";
            string sql = "Select count(*) ct from Quiz where Solved is null and id=@quizid";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@quizid", sID);
            double d1 = gData.GetScalarDouble(command, "ct");
            string sIP = (HttpContext.Current.Request.UserHostAddress ?? "").ToString();
            sql = "Select count(*) ct from Quiz where ip='" + sIP + "' and Solved > getdate()-1";
            double d2 = gData.GetScalarDouble(sql, "ct");

            sql = "Select count(*) ct from QuizParticipant where bbpaddress=@bbp";
            command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@bbp", sBBP);
            double dBBP = gData.GetScalarDouble(command, "ct");
            if (d1 < 1 || dBBP < 1 || d2 > 0)
            {
                MsgBox("Solved", "Sorry, this quiz has been solved by someone already or does not exist, or you have solved one within the last frequency range.  ", this);
                return;
            }

            int nVerse = 0;
            if (Session["verseno"] == null)
            {
                nVerse = GetVerseNumber();
                Session["verseno"] = nVerse;
            }
            else
            {
                nVerse = (int)GetDouble(Session["verseno"]);
            }
            string quiz = GetQuiz(nVerse);
            txtBBP.Text = sBBP;

            if (!IsPostBack)
            {
                txtAnswer.Text = quiz;
            }
            else
            {

            }
        }

        string GetUS(int l)
        {
            string d = "";
            for (int i = 0; i < l; i++)
            {
                d += "_";
            }
            return d;
        }
        string GetKnockOut(string v)
        {
            string[] vData = v.Split(" ");
            bool fKnocked = false;
            string outdata = "";
            for (int i  = 0; i < vData.Length; i++)
            {
                string word = vData[i];
                if (!fKnocked && word.Length > 7)
                {
                    word = GetUS(word.Length);
                    fKnocked = true;
                }
                outdata += word + " ";
            }
            outdata = outdata.Trim();
            return outdata;
        }
        string ProcessKnockOuts(string data)
        {
            string[] vData = data.Split("\r\n");
            string par = "";
            for (int i = 0; i < vData.Length; i++)
            {
                string v = vData[i];
                string knockout = GetKnockOut(v);
                par += knockout + "\r\n";
            }
            return par;

        }
        bool CheckAnswer(string sAnswer)
        {
            string sQuestion = Session["question"].ToString();
            Regex rgx = new Regex("[^a-zA-Z -]");
            sAnswer = rgx.Replace(sAnswer, "").ToLower();
            sQuestion = rgx.Replace(sQuestion, "").ToLower();
            sAnswer = sAnswer.Replace(" ", "");
            sQuestion = sQuestion.Replace(" ", "");
            sAnswer = sAnswer.Replace("\r\n ", "");
            sQuestion = sQuestion.Replace("\r\n", "");
            return sAnswer == sQuestion;
        }

        string GetEle(string data, int iEle)
        {
            //Rev|22|16| I
            string[] vData = data.Split("|");
            if (vData.Length >= iEle)
            {
                string d = vData[iEle];
                d = d.Replace("~", "");
                return d.Trim();
            }

            return "";
        }
        bool CheckVerses(int nStart)
        {
            string sMainBook = GetEle(kjv.b[nStart + 1], 0);

            for (int i = 1; i <= 14; i++)
            {
                string v = kjv.b[i + nStart];
                string sBook = GetEle(v, 0);
                if (sBook != sMainBook )
                    return false;
            }
            return true;
        }
        int GetVerseNumber()
        {
            for (int i = 0; i < 299; i++)
            {
                int nRange = 31101 - 23145 - 14;
                Random r = new Random();
                int rInt = r.Next(0, nRange);
                double rDouble = r.NextDouble() * nRange;
                rDouble += 23145;
                //23145 is the beginning of Matthew (The new testament), 31101 is the end of the bible
                bool fCheck = CheckVerses((int)rDouble);
                if (fCheck)
                     return (int)rDouble;
            }
            return 0;
        }

        
        public string _studybook = "";
        protected string GetQuiz(int nVerse)
        {
            string verses = "";
            for (int i = 1; i <= 14; i++)
            {
                string v = GetEle(kjv.b[i + nVerse], 3);

                verses += v + "\r\n";
            }
            Session["question"] = verses;

            _studybook = GetFullBookName(GetEle(kjv.b[nVerse + 1], 0)) + " Chapter " + GetEle(kjv.b[nVerse + 1], 1);
            string knockedout = ProcessKnockOuts(verses);
            return knockedout;
        }

        protected string GetFullBookName(string shortname)
        {
            for (int i = 0; i < 66; i++)
            {
                string[] vData = kjv.book[i].Split("|");
                if (vData[0].ToLower() == shortname.ToLower())
                    return vData[1];
            }
            return "?";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (txtAnswer.Text.Length < 1)
            {
                MsgBox("Answer too short", "Sorry, the answer must be populated.", this);
                return;
            }
            
            bool fPass = CheckAnswer(txtAnswer.Text);
            string sql = "Select count(*) ct from Quiz where Solved is null and id=@quizid";
            SqlCommand command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@quizid", Request.QueryString["id"].ToString());
            double d1 = gData.GetScalarDouble(command, "ct");
            if (d1 < 1)
            {
                MsgBox("Solved", "Sorry, this quiz has been solved by someone already.  ", this);
                return;
            }
            if (!fPass)
            {
                MsgBox("Failed", "Sorry, the Answer provided does not match the KJV bible.  Please click back and continue to try to solve the quiz. ", this);
                return;
            }
            command = new SqlCommand(sql);
            sql = "Select Reward from Quiz where id=@quizid";
            command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@quizid", Request.QueryString["id"].ToString());
            double nReward = gData.GetScalarDouble(command, "Reward");
            bool fValid = ValidateBiblepayAddress(false,txtBBP.Text);
            if (!fValid)
            {
                MsgBox("Failed", "Sorry, the BiblePay address is not valid.", this);
                return;
            }

            List<Payment> p = new List<Payment>();
            Payment p1 = new Payment();
            p1.bbpaddress = txtBBP.Text;
            p1.amount = nReward;
            p.Add(p1);

            string poolAccount = GetBMSConfigurationKeyValue("PoolPayAccount");
            string txid = SendMany(p, poolAccount, "Quiz " + Request.QueryString["id"].ToString());
            string sIP = (HttpContext.Current.Request.UserHostAddress ?? "").ToString();

            sql = "Update Quiz Set Solved=getdate(), IP=@ip, TXID=@txid, Book=@book, bbpaddress=@bbpaddress where id=@quizid";
            command = new SqlCommand(sql);
            command.Parameters.AddWithValue("@txid", txid);
            command.Parameters.AddWithValue("@book", _studybook);
            command.Parameters.AddWithValue("@bbpaddress", txtBBP.Text);
            command.Parameters.AddWithValue("@quizid", Request.QueryString["id"].ToString());
            command.Parameters.AddWithValue("@ip", sIP);

            gData.ExecCmd(command);
            string sNarr = "Congratulations!  You solved the verses of " + _studybook + " first, and won the prize of " + nReward.ToString() + " BBP.  <p>The reward has been transmitted " + txid + ".<p><p>  Thank you for participating! <p><p>And, thank you for learning the Gospel of Jesus Christ!";
            // reset
            Session["verseno"] = null;
            MsgBox("You Won", sNarr, this);
        }
    }
}
