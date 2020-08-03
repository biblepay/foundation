<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Saved.Register" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Register</h2>

    <br />
    <br />

   <table>

       <tr><td width="35%">    <asp:Label ID="lbl1" runat="server" Text="E-Mail Address:"></asp:Label></td>
           <td>    <asp:TextBox width="500px" ID="txtEmailAddress" runat="server"></asp:TextBox></td></tr>

       <tr><td>    <asp:Label ID="Label1" runat="server" Text="Password:"></asp:Label></td>
           <td>    <asp:TextBox ID="txtPassword" width="300px" type="password" runat="server"></asp:TextBox></td></tr>

       <tr><td>    <asp:Label ID="Label2" runat="server" Text="Nick Name:"></asp:Label></td>
           <td>    <asp:TextBox ID="txtUserName" runat="server"></asp:TextBox></td></tr>



       <tr><td>
       <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
           </td></td></tr>


          </table>

</asp:Content>
