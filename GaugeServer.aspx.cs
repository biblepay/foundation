using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Saved.Code;

namespace Saved
{
    public partial class GaugeServer : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string RenderChart()
        {
             string s = "<script type=text/javascript> google.load( *visualization*, *1*, {packages:[*gauge*]});"
                + "      google.setOnLoadCallback(drawChart);"
                + "      function drawChart() {"
                + "      var data = new google.visualization.DataTable();"
                + "      data.addColumn('string', 'item');"
                + "      data.addColumn('number', 'value');     "
                + "      data.addRows(1);";
            s += "data.setValue(0,0,'Oops');";
            s += "data.setValue(0,1,1000000);";
            s += "var options = {width: 600, height: 300,redFrom: 90, redTo: 100,yellowFrom:75, yellowTo: 90,minorTicks: 5};";
            s += " var chart = new google.visualization.Gauge(document.getElementById('chart_div'));";
            s += "chart.draw(data, options); }";
            s += "</script>";
            s = s.TrimEnd(',').Replace('*', '"');
            return s;
        }
    }
}