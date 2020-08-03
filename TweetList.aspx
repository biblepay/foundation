<%@ Page Title="Tweet List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="TweetList.aspx.cs" Inherits="Saved.TweetList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Tweets</h2>

    <%=Saved.Code.Common.GetTweetList(Saved.Code.Common.gUser(this).UserId, 90) %>

    <br />
    <br />

    <ul>FAQ:
        <li>You will be compensated 1 BBP for every distinct tweet you read.</li>
        <li>The tweet advertiser pays 1 BBP for each distinct active subscriber in our system (to send a tweet).</li>
        <li>When you advertise a tweet, we will send an e-mail and an alert to our subscribers.</li>
        <li>Tweets will be useful when you have important information to share with the community (for example, when the great tribulation starts you may be seeking a Safe Haven or Opening a safe haven, 
            or broadcasting important information.)</li>
    </ul>


</asp:Content>
