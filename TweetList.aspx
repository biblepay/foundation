<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="TweetList.aspx.cs" Inherits="Saved.TweetList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Tweets</h2>

    <%=GetTweetList() %>





</asp:Content>
