<%@ Page Title="Media" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MediaBlack.aspx.cs" Inherits="Saved.MediaBlack" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body { 
            background-color: #01182a;
               color: gold;


        }

        td {
            color:gold;
        }

    </style>


    <h2><b><%=GetMediaCategory()%></b></h2>

    <%=GetMedia() %>
  
    <br />
</asp:Content>
