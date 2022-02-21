<%@ Page Title="AddExpense" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AddExpense.aspx.cs" Inherits="Saved.AddExpense" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <br />
    <h3>Add Expense</h3>

    <hr />

    <table>
        <tr><td>    Expense ADDED Date:             <td>    <asp:TextBox ID="txtAdded" width="200px"  runat="server" ></asp:TextBox></tr>
        <tr><td>    Expense Amount:                  <td>    <asp:TextBox ID="txtExpenseAmount" width="200px"  runat="server" ></asp:TextBox></tr>
        <tr><td>    Current Balance:                 <td>    <asp:TextBox ID="txtBalance" width="200px"  runat="server" ></asp:TextBox></tr>

        <tr><td>    Child ID (or Charity Name):  <td>    <asp:TextBox ID="txtChildID" width="400px"  runat="server" ></asp:TextBox></tr>
        <tr><td>    Expense Narrative:               <td>    <asp:TextBox ID="txtNotes" width="400px"  runat="server" ></asp:TextBox></tr>
    </table>

    <h3>Add Monthly Pool Expense+Income record:</h3>
    <table>
        <tr><td>    XMR Raised from Foundation:                  <td>    <asp:TextBox ID="txtFoundation" width="200px"  runat="server" ></asp:TextBox></tr>
        <tr><td>    XMR Raised from Song Pool:                 <td>    <asp:TextBox ID="txtSong" width="200px"  runat="server" ></asp:TextBox></tr>
        <tr><td>    Bitcoin Value:  <td>    <asp:TextBox ID="txtBitcoin" width="400px"  runat="server" ></asp:TextBox></tr>
        <tr><td>    XMR Value:  <td>    <asp:TextBox ID="txtXMRRate" width="400px"  runat="server" ></asp:TextBox></tr>

    </table>


    <asp:Button ID="btnAddSingleExpense" runat="server" Text="Add Expense" OnClick="btnAddExpense_Click" />


    <asp:Button ID="btnAddExpenseRecord" runat="server" Text="Add Monthly Expense Record + Income Record" OnClick="btnAddExpenseRecord_Click" />

</asp:Content>
