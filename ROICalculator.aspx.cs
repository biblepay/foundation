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
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using static Saved.Code.Common;
using static Saved.Code.PoolCommon;

namespace Saved
{
    public partial class ROICalculator : Page
    {

        protected string GetChartOfROI()
        {
            Chart c = new Chart();
            System.Drawing.Color bg = System.Drawing.Color.White;
            System.Drawing.Color primaryColor = System.Drawing.Color.Blue;
            System.Drawing.Color textColor = System.Drawing.Color.Black;
            c.Width = 1500;

            string sChartName = "Hypothetical ROI Chart and Future Value of Money";
            Series s = new Series("PV in BBP with UTXO Rewards");
            s.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
            s.LabelForeColor = textColor;
            s.Color = primaryColor;
            s.BackSecondaryColor = bg;
            s.BorderWidth = 10;

            Series sDecay = new Series("PV in USD with Inflation");
            sDecay.ChartType = System.Web.UI.DataVisualization.Charting.SeriesChartType.Line;
            sDecay.LabelForeColor = textColor;
            sDecay.Color = System.Drawing.Color.Red;
            sDecay.BackSecondaryColor = bg;
            sDecay.BorderWidth = 10;

            c.ChartAreas.Add("ChartArea");
            c.ChartAreas[0].BorderWidth = 1;
            c.Series.Add(s);
            c.Series.Add(sDecay);

            c.Legends.Add(new System.Web.UI.DataVisualization.Charting.Legend(sChartName));
            c.Legends[sChartName].DockedToChartArea = "ChartArea";
            c.Legends[sChartName].BackColor = bg;
            c.Legends[sChartName].ForeColor = textColor;
            double dDuration = GetDouble(txtDuration.Text) * 12;
            double nPortfolioValue = GetDouble(txtValueUSD.Text);
            double nPortfolioValueDecay = GetDouble(txtValueUSD.Text);

            double nRate = GetDouble(txtDWUPercent.Text) / 100 / 12;
            double nDecayRate = GetDouble(txtInflation.Text) / 100 / 12;

            for (int i = 0; i < dDuration; i++)
            {
                DateTime dt = DateTime.Now.AddDays(i * 30);
                nPortfolioValue = nPortfolioValue + (nPortfolioValue * nRate);
                nPortfolioValueDecay = nPortfolioValueDecay - (nDecayRate * nPortfolioValueDecay);
                s.Points.AddXY(dt, nPortfolioValue);
                sDecay.Points.AddXY(dt, nPortfolioValueDecay);
            }

            c.ChartAreas[0].AxisX.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].AxisY.LabelStyle.ForeColor = textColor;
            c.ChartAreas[0].BackColor = bg;
            c.Titles.Add(sChartName);
            c.Titles[0].ForeColor = textColor;
            c.BackColor = bg;
            c.ForeColor = textColor;
            string sSan = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/roi.png");
            c.SaveImage(sSan);

            string sImage = "<img style='width:100%;height:50%' src='https://foundation.biblepay.org/Images/roi.png?t=" + getrand() + "' />";
            
            return sImage;
        }

        protected string getrand()
        {
            Random r = new Random();
            int rInt = r.Next(0, 20000);
            return rInt.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtInflation.Text = "5";
                txtDWUPercent.Text = "13.5";
                txtDuration.Text = "10";
                txtValueUSD.Text = "100000";
                btnCalculate_Click(this, null);

            }
        }

        string PrintDouble(double n)
        {
            return String.Format("  {0:F20}", Math.Round(n, 10));
        }
        protected void btnCalculate_Click (object sender, EventArgs e)
        {
            //GetChartOfROI();

        }
    }
}