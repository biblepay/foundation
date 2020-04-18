<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Media.aspx.cs" Inherits="Saved.Media" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        window.onload = function (evt) {
            var i = 0;

            $("video.connect-bg source").each(function () {
                var sourceFile = $(this).attr("src");
                $(this).attr("src", sourceFile);
                var video = this.parentElement;
                i++;
            });
        }

        </script>

    <h3>Media:</h3>

    <br /><br /><br />
    <%=GetMedia() %>
  
</asp:Content>
