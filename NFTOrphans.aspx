<%@ Page Title="NFT Orphans" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" validateRequest="false" CodeBehind="NFTOrphans.aspx.cs" Inherits="Saved.NFTOrphans" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h3>Children Available to be Sponsored through NFT Technology</h3>
            


    <div style="font-family:Arial;">
        <%=Saved.Code.Common.GetNFTDisplayList(true,this) %>
    </div>
</asp:Content>
