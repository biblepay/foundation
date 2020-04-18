<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MediaList.aspx.cs" Inherits="Saved.MediaList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Media List:</h3>

    <br /><br /><br />
    <div><%=GetMediaList()%></div>


  
</asp:Content>
