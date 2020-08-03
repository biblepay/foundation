<%@ Page Title="Tweet View" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TweetView.aspx.cs" Inherits="Saved.TweetView" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Tweet - View</h2>

<%=GetTweet() %>

</asp:Content>
