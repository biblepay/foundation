<%@ Page Title="Accountability" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Accountability.aspx.cs" Inherits="Saved.Accountability" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <p>
        <%=GetPDFList() %>
</asp:Content>
