<%@ Page Title="Study" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Study.aspx.cs" Inherits="Saved.Study" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Theological Studies, provided by our partners for your edification.</h3>

    <p>NOTE: We only post the highest quality content here, after being reviewed by our staff.  </p>
    <p>
        <%=GetArticles() %>
    </p>
    
    <br />

</asp:Content>
