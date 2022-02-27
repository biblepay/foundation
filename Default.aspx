<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Saved._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <script>
        //Array of images which you want to show: Use path you want.
        var images = new Array('../Images/hellfire.jpg', '../Images/jc2.png', '../Images/jc3.jpg',
            '../Images/Maranatha.jpg', '../Images/TheChurchAge1.gif', '../Images/bbphoriz.png');
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
        setTimeout(doSlideshow, 12000);
    }
}
</script>


    <div>
          Welcome to <%=Saved.Code.Common.GetLongSiteName(this) %>
      
    </div>
    <br />

    <div class="container global-header" style="min-width:50%;min-height:200px;width:95%;height:500px">
        <br />
    </div>

    <div id="AboutBiblePay1">
        <b>
            Launched in July 2017 with no premine and no ICO, BiblePay describes itself as a decentralized autonomous cryptocurrency that tithes 10% to orphan-charity (with Sanctuary governance).
              The project is passionate about spreading the gospel of Jesus, having the entire KJV bible compiled in its wallet utilizing RandomX.
            <br> BiblePay (BBP) is deflationary, decreasing its emissions by 19.5% per year. <br>
            <br>The project views itself as a utility that provides an alternative method for giving to charity.
             With Generic Smart Contracts, the project seeks to become the go-to wallet for Christians.In the future, the team intends to lease file space on its Sanctuaries and release corporate integration features, such as c# access to the blockchain. 
               The BiblePay platform is a derivative of Dash-Evolution. BiblePay is an international decentralized autonomous organization.  The Team seeks to help orphans globally. <br>
     </div>

    <br />
    <hr />

    <h4><a href="Partners.aspx">Our Partners</a></h4>
    <hr />

    <div id="tweets">
        <br><br><br><div style='width:100%; height:500; text-align:center; overflow:auto;'>
            <a class='twitter-timeline' href='https://twitter.com/BiblePay?ref_src=twsrc%5Etfw'>Tweets on the BiblePay Network</a> 
            <script src='https://platform.twitter.com/widgets.js' charset='utf-8'></script><script>window.onerror = function () { return false; };</script></div>
                     </div>-->
    
</asp:Content>
