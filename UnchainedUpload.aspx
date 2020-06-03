<%@ Page Title="Unchained-Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UnchainedUpload.aspx.cs" Inherits="Saved.UnchainedUpload"  ValidateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
        function UploadFile(fileUpload) {
            if (fileUpload.value != '') {
                document.getElementById("<%=btnUnchainedSave.ClientID %>").click();
            }
        }
</script>

    <h3>BiblePay Unchained - Upload</h3>
    <br />
    Unchained upload allows you to upload a file to BiblePay's sidechain (BBP Unchained).
    
    <br />
    This will allow the file to be shared with the entire world, on a robust network that is resistant
    to internet outages over multiple geographies in high density storage with high availability and performance.
    <br />
    <p>
    <br />
        <p>
    What can I use Unchained Upload for?  
            <br />
            Many uses including:  Making a file available for a tweet, archiving a file that Christians need to see forever, making a file available for an 
    e-mail, etc.  (See Unchained Tweeting).
    <br />

      <div>
         <h3> Add File to BiblePay Unchained:</h3>
         <br />
         <asp:FileUpload ID="FileUpload1" runat="server" />
         <br /><br />
          <div id="invisible" style="visibility:hidden">
         <asp:Button ID="btnUnchainedSave" runat="server" OnClick="btnUnchainedSave_Click"  Text="Save" style="width:85px" />
              </div>
         <br /><br />
         <asp:Label ID="lblmessage" runat="server" />
      </div>

     
</asp:Content>
