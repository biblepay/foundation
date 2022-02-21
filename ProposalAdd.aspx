<%@ Page Title="Proposal Add" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="ProposalAdd.aspx.cs" Inherits="Saved.ProposalAdd" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
  
    <h2>Welcome to the BiblePay Governance System </h2>
    <h3>Proposal - Add  <small>v1.1</small></h3><font color="red"><small>Note: It costs 2500 BBP to submit a proposal.</small></font>

    <br />

    <table>

            <tr>
                <td><asp:Label ID="lblName" runat="server" Text="Proposal Name:"></asp:Label></td>
                <td><asp:TextBox ID="txtName" runat="server" style="width: 900px"></asp:TextBox></td>
            </tr>

            <tr>
                <td><asp:Label ID="lblAmount" runat="server" Text="Amount:"></asp:Label></td>
                <td><asp:TextBox ID="txtAmount" runat="server" style="width: 250px"></asp:TextBox></td>
            </tr>

            <tr>
                <td><asp:Label ID="lblURL" runat="server" Text="Discussion URL:"></asp:Label></td>
                <td><asp:TextBox ID="txtURL" runat="server" style="width: 900px"></asp:TextBox></td>
            </tr>

            <tr>
                <td><asp:Label ID="lblAddress" runat="server" Text="Funding Receive Address:"></asp:Label></td>
                <td><asp:TextBox ID="txtAddress" runat="server" style="width: 900px"></asp:TextBox></td>
            </tr>

            <tr>
                <td><asp:Label ID="lblCharity" runat="server" Text="Expense Type:"></asp:Label></td>
                <td><asp:dropdownlist AutoPostBack="false" runat="server" id="ddCharity">   </asp:dropdownlist>   
            </tr>

    </table>
    <br />

    <asp:Button ID="btnSubmit" runat="server" Text="Submit Proposal" OnClick="btnSubmitProposal_Click" />




</asp:Content>
