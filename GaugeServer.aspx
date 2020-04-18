<%@ Page Title="GaugeServer" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GaugeServer.aspx.cs" Inherits="Saved.GaugeServer" %>


   
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
        
    
    <div id="chart_div">
        <%=RenderChart() %>
    </div>
    <br />
    <style>font-family:Old English text MT;</style>
    Oh noooo, An error has occurred.  
    <br />

    
    <br />
    
    Please try again.  If the problem persists, please contact rob@biblepay.org.

    <br />
    <br />

    Thank you for using BiblePay.


</asp:Content>
