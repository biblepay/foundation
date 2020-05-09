<%@ Page Title="Block History" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="BlockHistory.aspx.cs" Inherits="Saved.BlockHistory" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Block History</h2>

    <%=GetReport()%>
    <table>
        <tr><td>Enter a few characters of your BBP Address:</td><asp:TextBox ID="txtAddress" runat="server"></asp:TextBox></td></tr>
       </table>

    
    <asp:Button ID="btnRun" runat="server" Text="Run Report" OnClick="btnRunBlockHistory_Click" />
   



</asp:Content>
