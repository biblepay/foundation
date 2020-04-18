<%@ Page Title="Corruption Report" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="Corruption.aspx.cs" Inherits="Saved.Corruption" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Corruption Report (14 days)</h3>

    <%=GetCorruption() %>





</asp:Content>
