<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Saved._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <script>
//Array of images which you want to show: Use path you want.
var images=new Array('../Images/GloriousLion.jpg','../Images/RockMarriage.jpg','../Images/GloriousLion.jpg','../Images/Armageddon.jpg');
        var nextimage = 0;

        var slide = 0;
doSlideshow();
      
function doSlideshow()
{
    if(nextimage>=images.length){nextimage=0;}
    $('.global-header').css('background-image', 'url("' + images[nextimage++] + '")');
    slide++;
   
    if (slide == 1) {
        setTimeout(doSlideshow, 100);
    }
    else {
        setTimeout(doSlideshow, 15000);
    }
}
</script>


    <div>
          Welcome to the <%=Saved.Code.Common.GetSiteName() %>
      
    </div>
    <br />

    <div class="container global-header" style="min-width:100%;min-height:600px;width:100%;height:450px">
        <br /><br />

        &nbsp;
    </div>


    
</asp:Content>
