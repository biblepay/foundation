<%@ Page Title="Leaderboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="RokuLeaderboard.aspx.cs" Inherits="Saved.RokuLeaderboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Roku Leaderboard</h2>

    <%=GetLeaderboard() %>





</asp:Content>
