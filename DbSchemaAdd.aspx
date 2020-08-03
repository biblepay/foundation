<%@ Page Title="Schema Add" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DbSchemaAdd.aspx.cs" Inherits="Saved.DbSchemaAdd" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Database Schema - Add</h2>
        <asp:Label ID="Label1" runat="server" Text="Table Name (Please no spaces): " ></asp:Label>
    <br />
        <asp:TextBox ID="txtTableName" width="400px" runat="server"></asp:TextBox>
    <br />
    <br />
        <asp:Label ID="Label3" runat="server" Text="Column Names (separated by commas)"></asp:Label>
        <br />
        
       <asp:TextBox ID="txtColumnNames" runat="server" TextMode="MultiLine"  Rows="7" style="width: 1100px">
        </asp:TextBox>
        <br />

        <asp:Label ID="Label2" runat="server" Text="Data Types (separated by commas) (they must be:  string,float,datetime,guid):"></asp:Label>
        <br />
        
       <asp:TextBox ID="txtDataTypes" runat="server" TextMode="MultiLine"  Rows="7" style="width: 1100px">
        </asp:TextBox>
        <br />


        <font color="red">Note: Click Save to Save this Schema</font>
    <br />
        <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
    
</asp:Content>
