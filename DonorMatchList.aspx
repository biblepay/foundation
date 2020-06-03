<%@ Page Title="Sponsor an Orphan - List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="DonorMatchList.aspx.cs" Inherits="Saved.DonorMatchList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>List of Donations Recevied for Orphan Sponsorship Matches</h2>

    <%=GetDonorMatchList() %>
       
   <asp:Button ID="btnDonate" runat="server" onclick="btnDonate_Click"  Text="Donate Now" style="width:85px" />

</asp:Content>
