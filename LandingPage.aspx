<%@ Page Title="LandingPage" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="LandingPage.aspx.cs" Inherits="Saved.LandingPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Welcome to BiblePay!</h2>



    We are elated to have you here.  We hope you will consider joining our community, and help make BiblePay the CryptoCurrency for Christians!
    <br />
    <br />

    To claim your free reward, please follow these steps:

    <ul>
        <li>Create a free forum account <a href="https://forum.biblepay.org">here.</a>  Note: The reason this is necessary is to establish a user account with the ability to withdraw Foundation coins.</li>
        <li>Come back to this page, while logged in from your new forum account.  </li>
        <li>Click <a href='LandingPage?claim=1&id=<%=GetId()%>'>this link to claim your reward.</a></li>

    </ul>

    <br />
    Thank you for using BiblePay!



</asp:Content>
