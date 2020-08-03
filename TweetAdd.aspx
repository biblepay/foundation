<%@ Page Title="Tweet Add" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TweetAdd.aspx.cs" Inherits="Saved.TweetAdd" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Tweet - Add</h2>
        <asp:Label ID="Label1" runat="server" Text="Subject (Type a short description of what the tweet is about): " ></asp:Label>
    <br />
        <asp:TextBox ID="txtSubject" width="400px" runat="server"></asp:TextBox>
    <br />
    <br />
        <asp:Label ID="Label3" runat="server" Text="Tweet Body"></asp:Label>
        <br />
        
       <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine"  Rows="20" style="width: 1100px">
        </asp:TextBox>
        <br />
        <font color="red">Note: Click Advertise if you agree to pay <%=GetTweetCost() %> BBP to advertise this tweet.</font>
    <br />
        <asp:Button ID="btnSave" runat="server" Text="Advertise Tweet (Charges Apply)" OnClick="btnSave_Click" />
    
</asp:Content>
