<%@ Page Title="Proposals - List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="ProposalsList.aspx.cs" Inherits="Saved.ProposalsList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Proposals - List</h2>

    <%=GetProposalsList() %>

    
</asp:Content>
