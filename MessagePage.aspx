<%@ Page Title="Message Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MessagePage.aspx.cs" Inherits="Saved.MessagePage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%=GetMessagePage() %>
 </asp:Content>
