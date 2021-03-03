<%@ Page Title="CampaignReport" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="CampaignReport.aspx.cs" Inherits="Saved.CampaignReport" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Campaign Report </h3>
    
    <small>Shows current Dash Campaign Unclaimed Public Addresses - Each of these addresses have between 1000 and 1MM BiblePay Waiting!<br /></small>
    <br />

    <%=GetCampaignReport() %>





</asp:Content>
