<%@ Page Title="Unchained Main Menu" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="UnchainedMainMenu.aspx.cs" Inherits="Saved.UnchainedMainMenu" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Welcome to BiblePay Unchained - Main Menu</h2>


    <a href="UnchainedPayPerView">Unchained Pay Per View Demo - Requires BBP-Chrome + BBP Payment to view video</a>
    <a href="UnchainedPayPerByte">Unchained Pay Per Byte Demo - Required BBP-Chrome + BBP Payment per byte to view video</a>
    <a href="UnchainedAssetProtection">Unchained - Asset Protection Demo (Expiring Link, but Public Access)</a>
    <a href="UnchainedAuthentication">Unchained - Authentication Demo (Decentralized Authenticated access by CPK only, no charges, CPK+Nickname demo)</a>

    <!--  Temporary placeholder for general cryptocurrency videos -->


    <script src="https://apps.elfsight.com/p/platform.js" defer></script>
    <div class="elfsight-app-79ce3a39-2705-4a4b-a5c4-55d43a32e3f2"></div>


</asp:Content>
