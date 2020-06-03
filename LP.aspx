<%@ Page Title="LandingPage" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="LP.aspx.cs" Inherits="Saved.LP" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Welcome to BiblePay!</h3>
    

    We are elated to have you here.  We hope you will consider joining our community, and help make BiblePay the CryptoCurrency for Christians!
    <br />


    <ul>There are many things you can do with BiblePay today, and more coming:
        <li>Send coins to Exchanges, exchanging to Bitcoin, From Bitcoin, or from peer to peer</li>
        <li>Buy things with BBP from bitrefill.com (gift cards, grocery cards)</li>
        <li>Cancer mining with PODC</li>
        <li>RandomX mining (we are the first bitcoin branch of RandomX)</li>
        <li>Enter a prayer request in the core wallet, or, pray for others</li>
        <li>Join our healing campaign and pray for people on the street</li>
        <li>Buy BBP coins to gain cryptocurrency exposure without risking a lot of money</li>
        <li>Our wallet has the KJV bible right in it - use it as a bible reader</li>
        <li><a href="http://foundation.biblepay.org/HowTo?name=sinner">Get Saved</a></li>
        <li><a href="http://foundation.biblepay.org/Study">Theological Studies</a></li>
        <li><a href="http://foundation.biblepay.org/MediaList">Video Lists</a></li>


    </ul>

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
