<%@ Page Title="Prayer Add" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PrayerAdd.aspx.cs" Inherits="Saved.PrayerAdd" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Prayer Request - Add New Prayer Request</h2>
    

        <asp:Label ID="Label3" runat="server" Text="Prayer Body"></asp:Label>
        <br />

       <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine"  Rows="30" style="width: 1200px">
        </asp:TextBox>
        <br />


        <asp:Label ID="Label1" runat="server" Text="Prayer Subject - (Type a short description of what the prayer request entails): " ></asp:Label>
    <br />
        <asp:TextBox ID="txtSubject" width="400px" runat="server"></asp:TextBox>
        <asp:Button ID="btnSave" runat="server" Text="Save Prayer" OnClick="btnSave_Click" />
    

</asp:Content>
