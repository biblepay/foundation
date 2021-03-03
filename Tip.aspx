<%@ Page Title="Tip" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tip.aspx.cs" Inherits="Saved.Tip" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <br />
    <h3>Tip another BiblePay User</h3>

    <hr />

    <table>
    <tr><td>    BBP Receive Address:&nbsp;<td>    <asp:TextBox ID="txtAddress" width="500px" runat="server" ></asp:TextBox></tr>

    <tr><td>    Amount of Tip:      <td>    <asp:TextBox ID="txtAmount" width="500px" runat="server" ></asp:TextBox></tr>
    </table>
    <br />
    <br />

    <font color="red"></font>
    <br />
    <asp:Button ID="btnTip" runat="server" Text="Tip User Now" OnClick="btnTip_Click" />

    <br />

</asp:Content>
