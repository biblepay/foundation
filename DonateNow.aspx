<%@ Page Title="Donate Now - Single Donation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="DonateNow.aspx.cs" Inherits="Saved.DonateNow" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Donate Now (One Time Donation)</h2>

       
   <asp:Button ID="btnDonateNow" runat="server" onclick="btnDonateNow_Click"  Text="Make One Time Donation" style="width:85px" />

</asp:Content>
