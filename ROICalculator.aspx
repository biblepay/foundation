<%@ Page Title="ROI Calculator" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ROICalculator.aspx.cs" Inherits="Saved.ROICalculator" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h3>ROI Calculator</h3>
    <br />
    <div id="divroi">

        <%=GetChartOfROI() %>
        
    </div>
    <br />
    <br />
    <br />

    <table>
       
    <tr><td> Current DWU %:&nbsp;<td>    <asp:TextBox ID="txtDWUPercent" width="500px" runat="server" ></asp:TextBox></tr>
    <tr><td> Expected Global Inflation %:&nbsp;<td>    <asp:TextBox ID="txtInflation" width="500px" runat="server" ></asp:TextBox></tr>


    <tr><td> Portfolio Value $:&nbsp;<td>    <asp:TextBox ID="txtValueUSD" width="500px" runat="server" ></asp:TextBox></tr>
    <tr><td> Duration (Years):&nbsp;<td>    <asp:TextBox ID="txtDuration" width="500px" runat="server" ></asp:TextBox></tr>

    </table>
    <asp:Button ID="btnCalculate" runat="server" Text="Calculate" OnClick="btnCalculate_Click" />

</asp:Content>
