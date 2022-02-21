<%@ Page Title="Storefront" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="Storefront.aspx.cs" Inherits="Saved.Storefront" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Amazon Storefront &nbsp;&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;  <%=GetSaleNarrative() %>  </h2>

    <span><font color="purple">Attention Users:  Our Amazon storefront only works in the United States currently!  We are looking at the possibility of more Countries.  Thank you for your patience.</font></span>
    <br />

    <small><font color="red">And I say to you, make friends for yourselves by unrighteous mammon, that when you fail, they may receive you into an everlasting home.  (Luke 16:9)</font></small>
    <br />
    Add Item to Store: <asp:TextBox ID="txtAdd" width="600px" runat="server" ></asp:TextBox>
    <asp:Button ID="btnAdd" runat="server" Text="Add Item to Store" OnClick="btnAdd_Click" />
    <br />
    <br />


    <%=GetStorefront() %>


</asp:Content>
