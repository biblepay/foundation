<%@ Page Title="Unchained Pay Per View" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="UnchainedPayPerByte.aspx.cs" Inherits="Saved.UnchainedPayPerByte" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Pay Per Byte - Video Demo</h2>

    <div>
       <video class='connect-bg' width='400' height='340' style='background-color:black'>
           <source src='<%=GetPlayURL() %>' type='video/mp4'>
       </video>
    </div>

 <ul>Features:
    <li>This video is hosted on the biblepay network, and consumed through our content-delivery-network (50+ POPS, 100MBS, ANTI-DDOS).</li>
     <li>This video is protected from unauthorized download - consumers must originate on your web site, otherwise they receive a 404.  The link also expires after a while.</li>
     <li>This video must be viewed by an authenticated-by-cpk biblepay user (having a signed CPK originating from the bbp-chrome-browser).</li>
     <li>This video will not play unless the pay-per-byte fees are paid first.  If the CPK has insufficient funds, the video will not play.</li>
     <li>If the user has sufficient funds, the user will be debited.  Then the payment will go out during the next collection cycle.</li>
     <li>To view your unchained balance click here.</li>
     <li>NOTE:  Our pay-per-byte technology is DECENTRALIZED, and does not rely on foundation.biblepay.org's user balance!  The balance is paid when the browser signs the coins over to the host.</li>
     <li>To host your own pay-per-byte content, all you need to do is upload your asset file to biblepay network, and host the file in a web server that is capable of generating expiring links.
         To generate an expiring link your php or aspx code just needs to call our sha256 function.  We provide examples <here> how to do this.  </li>
 </ul>
</asp:Content>
