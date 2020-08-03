<%@ Page Title="Account Edit" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccountEdit.aspx.cs" Inherits="Saved.AccountEdit" %>



<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Account Edit</h2>
    <hr />
    <br />
    <br />
    <!-- upload control -->
    <table>
        <tr>
            <td width="25%"> Current User Photo: 
            <td> 
                
                   <div><%=_picture %></div>
                    
                </td> 
            <td>
                <div>
                   <asp:Image ID="imgQrCode" runat="server" Width="220" Height="220" /><br />
                   <br /><asp:Label ID="lblQR" runat="server" Text=""></asp:Label>                    
                </div>
             
            </td>
        </tr>

       
        <tr><td>&nbsp;</td></tr>
        <tr><td>RandomX BBP Mining Address:</asp:Label></td>
           <td><asp:TextBox ID="txtRandomXAddress" width="500px" runat="server" ></asp:TextBox></td></tr>



       <tr><td>User Name:</td></td>
           <td><asp:TextBox ID="txtUserName" readonly runat="server"></asp:TextBox></td></tr>

        <tr><td>Account Balance:</td></td>
           <td><asp:TextBox ID="txtMyBalance" readonly runat="server"></asp:TextBox></td></tr>
 
        <tr><td>Unsubscribe from E-mails:</td>
            <td><asp:CheckBox ID="chkUnsubscribe" runat="server" /></asp:CheckBox>
            <font color="red"><small>Note: If you unsubscribe from our e-mails, you will no longer be eligibile for video rewards, bonuses, promotions, giveaways, tweet rewards, gospel rewards, or randomx bonsuses and you will be out of the loop on emergency notifications.</font></small>
                </td>
        </tr>
        <tr><td>Account 2FA Enabled:</asp:Label></td>
           <td><asp:TextBox ID="txtTwoFactorEnabled" readonly runat="server"></asp:TextBox></td></tr>
 

       <tr><td>2FA Code:</asp:Label></td>
           <td><asp:TextBox ID="txttwofactorcode" runat="server"></asp:TextBox></td></tr>
 
        <tr><td>&nbsp;</td></tr>
        <tr><td>&nbsp;</td></tr>


       <tr><td colspan="3">
       <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
           <!--            <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" OnClick="btnChangePassword_Click" />-->


            <asp:Button ID="btnSetTwoFactor"  onclientclick="alert('Once you see the QR code, click Add New Site in your authenticator app.  You may scan the code or add it manually.  Then, test it by viewing your next 2fa pin, and pasting it in the two-factor pin box, and then click Validate 2FA.  ');" runat="server" Text="Set Up 2FA" OnClick="btnSetTwoFactor_Click" />
            <asp:Button ID="btnCheckTwoFactor" runat="server" Text="Test 2FA" OnClick="btnValidateTwoFactor_Click" />

           </td></td></tr>


          </table>

</asp:Content>
