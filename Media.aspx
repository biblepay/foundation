<%@ Page Title="Media" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Media.aspx.cs" Inherits="Saved.Media" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        function getParameterByName(name, url) {
            if (!url) url = window.location.href;
            name = name.replace(/[\[\]]/g, '\\$&');
            var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, ' '));
        }

        CT = 0;

        function beacon() {
            queryString = window.location.search;
            var mediaid = getParameterByName('mediaid');

            CT++;
            if (CT > 256)
                return;


            if (mediaid == null)
                return;

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

    <h4>Media:</h4>

    <br />
    <%=GetMedia() %>
  
</asp:Content>
