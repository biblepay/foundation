﻿<%@ Page Title="Admin" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Admin1.aspx.cs" Inherits="Saved.Admin1"  ValidateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Admin 1</h3>
    
    Background convert all unconverted videos from RequestVideo into Rapture videos with categories:


    <asp:Label ID="Label1" runat="server" Text="Video URL"></asp:Label>
    <asp:TextBox ID="txtURL" runat="server"></asp:TextBox>

    
    <asp:Label ID="lblStatus" runat="server" Text="Status_________________"></asp:Label>
    <br />    
    
    <asp:Button ID="btnConvert" runat="server" Text="Convert Unconverted Videos" OnClick="btnConvert_Click" />
    
    <asp:Button ID="btnPDF" runat="server" OnClick="btnPDF_Click" Text="PDF-Test" />
    

      <div>
         <h3> File Upload:</h3>
         <br />
         <asp:FileUpload ID="FileUpload1" runat="server" />
         <br /><br />
         <asp:Button ID="btnSave" runat="server" onclick="btnSave_Click"  Text="Save" style="width:85px" />
         <br /><br />
         <asp:Label ID="lblmessage" runat="server" />
      </div>

     
</asp:Content>
