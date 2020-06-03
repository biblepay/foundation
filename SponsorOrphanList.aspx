<%@ Page Title="Sponsor an Orphan - List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="SponsorOrphanList.aspx.cs" Inherits="Saved.SponsorOrphanList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Sponsor an Orphan (Monthly Sponsorships)</h2>

    <%=GetSponsoredOrphanList() %>
       

</asp:Content>
