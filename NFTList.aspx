<%@ Page Title="NFT List - My NFTs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="NFTList.aspx.cs" Inherits="Saved.NFTList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>My NFTs</h2>

    <%=GetMyNFTs(this) %>


</asp:Content>
