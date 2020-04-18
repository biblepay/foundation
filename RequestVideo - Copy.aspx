<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RequestVideo.aspx.cs" Inherits="Saved.RequestVideo"  ValidateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Request Video</h3>
    

    Add any Notes about this video request:

       <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine"  Rows="30" style="width: 1200px">
        </asp:TextBox>
        <br />


        <asp:Label ID="Label1" runat="server" Text="Video URL"></asp:Label>
        <asp:TextBox ID="txtURL" runat="server"></asp:TextBox>

        

    
    <asp:Label ID="lblStatus" runat="server" Text="Status_________________"></asp:Label>
    <br />    
    
    <asp:Button ID="btnSave" runat="server" Text="Save Markup" OnClick="btnSave_Click" />
    
     
</asp:Content>
