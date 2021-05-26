using Saved.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.PoolCommon;

namespace Saved
{
    public partial class MiningCalculator : Page
    {
        double GetAvgHashRate()
        {
            string sql = "select avg(hashRate) hr from hashrate where added > getdate()-1";
            double nHash = gData.GetScalarDouble(sql, "hr");
            return nHash;
        }

        double GetAvgBlocksFound()
        {
            string sql = "select avg(solvedCount) ct from hashrate where added > getdate()-1";
            double nCt = gData.GetScalarDouble(sql, "ct");
            return nCt;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {

            }
            else
            {
                // These 3 are assumptions, the miner can type these in
                txtHPS.Text = "10000";
                txtWatts.Text = "400";
                txtElectricCost.Text = ".08";
                txtXMRMHS.Text = "842";
                txtXMRBlocksFound.Text = "200";
                // These are pulled from the pool
                txtBBPBlocksFound.Text = GetAvgBlocksFound().ToString();
                txtBBPMHS.Text = (GetAvgHashRate() / 1000000).ToString();
                txtBBPPrice.Text = Code.BMS.GetPriceQuote("BBP/USD").ToString();
                txtXMRPrice.Text = Code.BMS.GetPriceQuote("XMR/USD").ToString();
                txtCost.Text = "0";
               // BMS.GetMoneroHashRate(out nMoneroBlocks, out nMoneroHashRate);
               // txtXMRMHS.Text = nMoneroHashRate.ToString();
               // txtXMRBlocksFound.Text = nMoneroBlocks.ToString();
            }
            btnCalculate_Click(this, null);

        }

        string PrintDouble(double n)
        {
            return String.Format("  {0:F20}", Math.Round(n, 10));
        }
        protected void btnCalculate_Click (object sender, EventArgs e)
        {
            txtCalc.Text = "";
            double nXMRReward = 1.75;
            double nLastSubsidy = 4000;
            double nXMRRevPerDay = nXMRReward * GetDouble(txtXMRPrice.Text) * GetDouble(txtXMRBlocksFound.Text);
            double nXMRPPH = nXMRRevPerDay / (GetDouble(txtXMRMHS.Text) + .001) / 1000000 * .90;
            double nXMRMonthlyRev = nXMRPPH * GetDouble(txtHPS.Text) * 31 * 1;
            double nBonus = GetDouble(GetBMSConfigurationKeyValue("PoolBlockBonus"));
            double nBBPReward = nLastSubsidy + nBonus;
            double nBBPRevPerDay = GetDouble(txtBBPPrice.Text) * GetDouble(txtBBPBlocksFound.Text) * nBBPReward;
            double nBBPPPH = nBBPRevPerDay / (GetDouble(txtBBPMHS.Text) + .001) / 1000000;
            double nBBPMonthlyRev = nBBPPPH * GetDouble(txtHPS.Text) * 31;
            txtCalc.Text = "1. XMR Revenue Per Day: (XMRPrice=" + txtXMRPrice.Text + ") * XMR Blocks Per Day=" + txtXMRBlocksFound.Text + " * XMRReward=" + nXMRReward.ToString() 
                + ") = " + nXMRRevPerDay.ToString() + "\r\n";
            txtCalc.Text += "2. XMR Payment Per Hash: (XMRRevenuePerDay=" + nXMRRevPerDay.ToString() + "/XMR Pool MH/S=" + txtXMRMHS.Text + " * .90 (XMR Net Revenue after Tithe))=" + PrintDouble(nXMRPPH) + "\r\n";
            txtCalc.Text += "3. XMR Revenue Per Month: (XMRPPH=" + PrintDouble(nXMRPPH) + " * YourHashPerSecond=" + txtHPS.Text + ") = " + nXMRMonthlyRev.ToString() + "\r\n";
            
            txtCalc.Text += "4. BBP Revenue Per Day: (BBPPrice=" + txtBBPPrice.Text + ") * BBP Blocks Per Day=" + txtBBPBlocksFound.Text + " * Reward " + nBBPReward.ToString() + ") = " + nBBPRevPerDay.ToString() + "\r\n";
            txtCalc.Text += "5. BBP Payment Per Hash: (BBPRevPerDay=" + nBBPRevPerDay.ToString() + "/BBP Pool MH/S=" + txtBBPMHS.Text + ")=" + PrintDouble(nBBPPPH) + "\r\n";
            txtCalc.Text += "6. BBP Revenue Per Month: (BBPPPH=" + PrintDouble(nBBPPPH) + " * YourHashPerSecond=" + txtHPS.Text + ") = " + nBBPMonthlyRev.ToString() + "\r\n";
            double nRevenue = nBBPMonthlyRev + nXMRMonthlyRev;
            txtCalc.Text += "7. Monthly Revenue: " + nRevenue.ToString() + "\r\n";
            double nTotalCosts = GetDouble(txtWatts.Text) / 1000 * 24 * 31 * GetDouble(txtElectricCost.Text);
            txtCalc.Text += "8. Monthly Costs: " + nTotalCosts.ToString() + "\r\n";
            double nProfit = nRevenue - nTotalCosts;
            txtCalc.Text += "9. Net Profit: " + nProfit.ToString() + "\r\n";
            txtBBPRevenue.Text = nBBPMonthlyRev.ToString();
            txtXMRRevenue.Text = nXMRMonthlyRev.ToString();
            txtCost.Text = nTotalCosts.ToString();
            txtNET.Text = nProfit.ToString();
        }
    }
}