<%@ Page Title="Contact Add" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ContactAdd.aspx.cs" Inherits="Saved.ContactAdd" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Add New Contact</h2>

    <br />
    <p>From here, you can add a new contact - a person that you would like to see Saved by Jesus.</p>
    <p>Later you will be able to edit this persons status, and build a history of actions done with this person.</p>
    <br />
    <br />

   <table>

       <tr><td width="35%">    <asp:Label ID="lbl1" runat="server" Text="Contact First Name:"></asp:Label></td>
           <td>    <asp:TextBox width="500px" ID="txtFirstName" runat="server"></asp:TextBox></td></tr>

       <tr><td width="35%">    <asp:Label ID="Label3" runat="server" Text="Contact Last Name:"></asp:Label></td>
           <td>    <asp:TextBox width="500px" ID="txtLastName" runat="server"></asp:TextBox></td></tr>
       <tr><td width="35%">    <asp:Label ID="Label4" runat="server" Text="Contact E-Mail Address (OPTIONAL):"></asp:Label></td>
           <td>    <asp:TextBox width="500px" ID="txtEmailAddress" runat="server"></asp:TextBox></td></tr>



       <tr><td>
       <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
           </td></td></tr>


          </table>

</asp:Content>
