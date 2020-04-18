<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="PrayerBlog.aspx.cs" Inherits="Saved.PrayerBlog" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Prayer Blog</h2>

    <%=GetPrayerBlogs() %>





</asp:Content>
