<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Markup.aspx.cs" Inherits="Saved.Markup"  ValidateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Markup Editor</h3>
    

        <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine"  Rows="30" style="width: 1200px">
        </asp:TextBox>
        <br />


        <asp:Label ID="Label1" runat="server" Text="Markup Name"></asp:Label>
        <asp:TextBox ID="txtName" runat="server"></asp:TextBox>

        <asp:Label ID="Label2" runat="server" Text="Step No"></asp:Label>
    
         <asp:TextBox ID="txtStepNo" runat="server"></asp:TextBox>
        
    

        <asp:Label ID="Label3" runat="server" Text="Title"></asp:Label>
    
         <asp:TextBox ID="txtTitle" style='width:400px' runat="server"></asp:TextBox>
    
    
    <asp:Label ID="lblStatus" runat="server" Text="Status_________________"></asp:Label>
<br />    
        <asp:Button ID="btnSave" runat="server" Text="Save Markup" OnClick="btnSave_Click" />
    
        <asp:Button ID="btnLoad" runat="server" Text="Load Markup" OnClick="btnLoad_Click" />
        <asp:Button ID="btnDelete" runat="server" Text="Delete Markup" OnClick="btnDelete_Click" />
        <asp:Button ID="btnCopy" runat="server" Text="Copy Videos from BMS" OnClick="btnCopy_Click" />
     
</asp:Content>
