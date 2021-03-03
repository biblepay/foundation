<%@ Page Title="LandingPage" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="LandingPage.aspx.cs" Inherits="Saved.LandingPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Welcome to BiblePay!</h2>



    We are very happy to have you here.  We hope you will consider joining our community, and help make BiblePay the CryptoCurrency for Christians!
    <br />
    <br />

    <h3>To claim the free new-user reward (which is currently <%= Saved.Code.Common.GetRewardMoniker() %>, please follow these instructions:</h3>
    
    <ul>
        <li>
            Download BiblePay Wallet to your PC by clicking <a href="https://www.biblepay.org/wallet/">here.</a>
            <li> Sync the wallet (IE verify the block count synchronized from 1 to the current block, which is about 250000).
                <li> From the console, type 'newuser emailaddress' where emailaddress is your e-mail address.  To get the console, click Tools | Info | Console.  This command will immediately inform you of the free coins and you will receive them instantly into the wallet!
            <li> From the BiblePay wallet, you can take Free college courses to learn about Christianity.  Just click "BiblePay University" to get started.
            <li> You can also sponsor an orphan!  Just click Send Money, check the DAC checkbox and send a donation.  To see our orphan collage, click <a href="http://foundation.biblepay.org/Viewer?target=collage">here.</a>
            <li> You can also create a free forum account <a href="https://forum.biblepay.org">here.</a>  This forum allows you to ask questions or help us.</li>

        
    </ul>

    <br />
    Thank you for using BiblePay!



</asp:Content>
