<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="Saved.About" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>About v1.2</h3>
    <p>Use this area to provide additional information.</p>
    <p>
        
    </p>
    <img src="Images/RockMarriage.jpg" />

    <div></div>
    <asp:Label ID="Label1" runat="server" Text="Enter Data"></asp:Label>
    <asp:TextBox ID="txtData" runat="server"></asp:TextBox>
    <asp:Button ID="btnPost" runat="server" Text="Post Entire Page" OnClick="btnPost_Click" />
    

    
    <!-- THIS IS THE CUSTOM MESSAGE BOX HERE -->
     <div class="custompopup" id="divThankYou" runat="server" visible="false">
        <p>
            <asp:Label ID="lblmessage" runat="server"></asp:Label>
        </p>
        <asp:Button ID="btnMessageBox" CssClass="classname leftpadding" runat="server" Text="Ok" OnClick="btnMessageBox_Click" />
    </div>

            


</asp:Content>
