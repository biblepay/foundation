<%@ Page Title="My Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="MyOrders.aspx.cs" Inherits="Saved.MyOrders" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>My Orders</h2>

    <%=GetOrders() %>

    
</asp:Content>
