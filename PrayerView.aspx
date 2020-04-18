<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PrayerView.aspx.cs" Inherits="Saved.PrayerView" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Prayer - View</h2>

<%=GetPrayer() %>

</asp:Content>
