<%@ Page Title="MiningCalculator" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MiningCalculator.aspx.cs" Inherits="Saved.MiningCalculator" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h3>Mining Calculator</h3>
    <br />

    <table>
    <tr><td> Your Miner HPS:&nbsp;<td>    <asp:TextBox ID="txtHPS" width="500px" runat="server" ></asp:TextBox><font color="red">* Assumption</font></tr>
    <tr><td> Electric Cost per KWH:&nbsp;<td>    <asp:TextBox ID="txtElectricCost" width="500px" runat="server" ></asp:TextBox><font color="red">* Assumption</font></tr>
    <tr><td> Your Miner Wattage Consumption:&nbsp;<td>    <asp:TextBox ID="txtWatts" width="500px" runat="server" ></asp:TextBox><font color="red">* Assumption</font></tr>
    
   
    <tr><td> XMR Blocks Found Per Day:&nbsp;<td>    <asp:TextBox ID="txtXMRBlocksFound" width="500px" runat="server" ></asp:TextBox></tr>
    <tr><td> XMR Price (USD):&nbsp;<td>    <asp:TextBox ID="txtXMRPrice" width="500px" runat="server" ></asp:TextBox></tr>
    <tr><td> XMR Pool MH/S:&nbsp;<td>    <asp:TextBox ID="txtXMRMHS" width="500px" runat="server" ></asp:TextBox></tr>
   
    <tr><td> BBP Blocks Found Per Day:&nbsp;<td>    <asp:TextBox ID="txtBBPBlocksFound" width="500px" runat="server" ></asp:TextBox></tr>
    <tr><td> BBP Price (USD):&nbsp;<td>    <asp:TextBox ID="txtBBPPrice" width="500px" runat="server" ></asp:TextBox></tr>
    <tr><td> BBP Pool MH/S:&nbsp;<td>    <asp:TextBox ID="txtBBPMHS" width="500px" runat="server" ></asp:TextBox></tr>

    <tr><td> XMR Monthly Revenue:&nbsp;<td>    <asp:TextBox ID="txtXMRRevenue" width="500px" runat="server" readonly ></asp:TextBox></tr>
    <tr><td> BBP Monthly Revenue:&nbsp;<td>    <asp:TextBox ID="txtBBPRevenue" width="500px" runat="server" readonly ></asp:TextBox></tr>

    <tr><td> Total Monthly Cost:&nbsp;<td>    <asp:TextBox ID="txtCost" width="500px" runat="server" readonly></asp:TextBox></tr>

    <tr><td> Net Monthly Profit:&nbsp;<td>    <asp:TextBox ID="txtNET" width="500px" runat="server" readonly></asp:TextBox></tr>
   
    </table>
    <asp:Button ID="btnCalculate" runat="server" Text="Calculate" OnClick="btnCalculate_Click" />

    <h2>Calculation Details:</h2>
    <asp:TextBox ID="txtCalc" runat="server" TextMode="MultiLine"  Rows="12" style="width: 1000px" />

</asp:Content>
