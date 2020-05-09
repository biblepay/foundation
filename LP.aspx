<%@ Page Title="LandingPage" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="LP.aspx.cs" Inherits="Saved.LP" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Welcome to BiblePay!</h2>



    We are elated to have you here.  We hope you will consider joining our community, and help make BiblePay the CryptoCurrency for Christians!
    <br />
    <br />

    To take advantage of this limited time offer, please create a fractional sanctuary by following these steps:

    <ul>
        <li>Create a free forum account <a href="https://forum.biblepay.org">here.</a>  </li>
        <li>Click <a href='FractionalSanctuaries'>on Fractional Sanctuary</a>.  </li>
        <li>Click Add Fractional Sanctuary</li>
        <li>Learn more about Biblepay <a href=https://www.biblepay.org>here</a></li>
        <li>Feel free to e-mail the founder, <a href=mailto:Rob@biblepay.org> if you have additional questions or need help.</a></li>


    </ul>


        <%=GetROIGauge() %>



    <br />
    Thank you for using BiblePay!



</asp:Content>
