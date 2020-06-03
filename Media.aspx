<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Media.aspx.cs" Inherits="Saved.Media" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        
        function beacon() {
            queryString = window.location.search;
           
            $.ajax({
                type: 'GET',
                async: false,
                url: 'Media.aspx' + queryString + '&watching=1'
            });
            setInterval('beacon()', 30000);
        }

        function startIt() {
            beacon();
        }
        window.onload = startIt();

        </script>

    <h3>Media:</h3>

    <br /><br /><br />
    <%=GetMedia() %>
  
</asp:Content>
