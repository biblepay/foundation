<%@ Page Title="Viewer" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Viewer.aspx.cs" Inherits="Saved.Viewer" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <p>
    <div style="font-family:Arial;">
        <%=GetArticle() %>
    </div>


</asp:Content>
