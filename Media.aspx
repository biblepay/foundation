<%@ Page Title="Media" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Media.aspx.cs" Inherits="Saved.Media" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">




    <script>



        function slowPlaySpeed() {
            var vid = document.getElementById("video1");

            vid.playbackRate = 0.5;
        }

        function normalPlaySpeed() {
            var vid = document.getElementById("video1");

            vid.playbackRate = 1;
        }

        function fastPlaySpeed() {
            var vid = document.getElementById("video1");

            vid.playbackRate = 1.75;
        }

        function getParameterByName(name, url) {
            if (!url) url = window.location.href;
            name = name.replace(/[\[\]]/g, '\\$&');
            var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, ' '));
        }

        var CT = 0;
        var interval = 60000;
        var starttime = 0;      
        function beacon() {
            queryString = window.location.search;
            var mediaid = getParameterByName('mediaid');


            if (mediaid == null)
                return;

            setInterval('beacon()', interval);
         
            var elapsed = new Date().getTime() - starttime;

            if (elapsed < interval)
                return;


            if (CT > 65535)
                return;

            CT++;
            starttime = new Date().getTime();

            $.ajax({
                type: 'GET',
                async: false,
                url: 'Media.aspx' + queryString + '&watching=1'
            });


            
        }


        window.onload = beacon();

        </script>

    <h2><b><%=GetMediaCategory()%></b></h2>

    <%=GetMedia() %>
  
    <br />
</asp:Content>
