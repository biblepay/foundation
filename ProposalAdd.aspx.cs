using Saved.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.DataOps;

namespace Saved
{
    public partial class ProposalAdd : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                CoerceUser(Session);

            if (!IsPostBack)
            {
                ddCharity.Items.Clear();
                ddCharity.Items.Add("Charity");
                ddCharity.Items.Add("PR");
                ddCharity.Items.Add("P2P");
                ddCharity.Items.Add("IT");
                ddCharity.Items.Add("XSPORK");
            }
        }

        protected void btnSubmitProposal_Click(object sender, EventArgs e)
        {
            string sError = "";
            if (txtName.Text.Length < 5)
                sError = "Proposal name too short.";
            if (txtAddress.Text.Length < 24)
                sError = "Address must be valid.";
            if (GetDouble(txtAmount.Text) <= 0)
                sError = "Amount must be populated.";
            if (!gUser(this).LoggedIn)
                sError = "You must be logged in.";

            bool fValid = PoolCommon.ValidateBiblepayAddress(IsTestNet(this), txtAddress.Text);
            if (!fValid)
            {
                sError = "Address is not valid for this chain.";
            }

            if (GetDouble(txtAmount.Text) > 2600000)
            {
                sError = "Amount is too high (over superblock limit).";
            }

            double nMyBal = DataOps.GetUserBalance(this);
            if (nMyBal < 2501)
                sError = "Balance too low.";

            if (sError != "")
            {
                MsgBox("Error", sError, this);
            }
            // Submit
            
            DataOps.AdjBalance(-1 * 2500, gUser(this).UserId, "Proposal Fee - " + Left(txtURL.Text, 100));

            Code.Proposals.gobject_serialize(IsTestNet(this), gUser(this).UserId, gUser(this).UserName,txtName.Text, txtAddress.Text, txtAmount.Text, 
                txtURL.Text, ddCharity.SelectedValue);
            MsgBox("Success", "Thank you.  Your proposal will be submitted in six blocks.", this);
        }
    }
}