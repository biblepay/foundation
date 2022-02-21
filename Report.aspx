<%@ Page Title="BiblePay Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="Report.aspx.cs" Inherits="Saved.Report" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%=Session["ReportName"]%></h2>

    <%=GetReport() %>
    

</asp:Content>
