<%@ Page Title="Request Video" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RequestVideo.aspx.cs" Inherits="Saved.RequestVideo"  ValidateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Share Video with the Community</h3>
    
    From this page, you can paste the URL of a youtube video.
    <br />
    <br />
    
    We will then convert it to the resilient format (IE striving to be available during the tribulation) and then make it available in our video catalog.
    
    <br />
    <ul>Guidelines

         <li>Please do not submit copyrighted videos - they must be public domain (like rapture dreams etc).
         <li>Do not post any questionable material.
         <li>The video URL must be the actual playable URL (copy it from the play screen) - not the URL from the search list.  Example: https://www.youtube.com/watch?v=Vqy33JV3LZw</li>

    <br />

 

    </ul>
    <br />

    <br />

    <asp:Label ID="Label1" runat="server" Text="Video URL:"></asp:Label>
    <asp:TextBox ID="txtURL" runat="server" Width="500px"></asp:TextBox>
    <br />
    <br />

    <asp:Button ID="btnSave" runat="server" Text="Submit Request" OnClick="btnSave_Click" />
    
     
</asp:Content>
