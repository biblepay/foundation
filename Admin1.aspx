<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Admin1.aspx.cs" Inherits="Saved.Admin1"  ValidateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Admin 1</h3>
    
    Background convert all unconverted videos from RequestVideo into Rapture videos with categories:


    <asp:Label ID="Label1" runat="server" Text="Video URL"></asp:Label>
    <asp:TextBox ID="txtURL" runat="server"></asp:TextBox>

    
    
    <asp:Label ID="lblStatus" runat="server" Text="Status_________________"></asp:Label>
    <br />    
    
    <asp:Button ID="btnConvert" runat="server" Text="Convert Unconverted Videos" OnClick="btnConvert_Click" />
    
     
    <asp:Button ID="btnPDF" runat="server" OnClick="btnPDF_Click" Text="PDF-Test" />
    
     
</asp:Content>
