<%@ Page Title="NFT Browser" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" validateRequest="false" CodeBehind="NFTBrowse.aspx.cs" Inherits="Saved.NFTBrowse" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <h3> NFT Marketplace - Digital Goods </h3>

    <hr />

    <div>
    <table id="tbl1">
        <tr><td>
    <asp:CheckBox ID="chkDigital" runat="server" Text="Digital Goods (Audio, Video, MP3, MP4, PDF, E-Books)" AutoPostBack="true"     OnCheckedChanged="chkDigital_Changed" />
            </td></tr>
        <tr><td>
    <asp:CheckBox ID="chkSocial" runat="server" Text="Social Media (Tweets, Posts, URLs)"                    AutoPostBack="true"     OnCheckedChanged="chkTweet_Changed" />
            </td></tr>
        </table>
    </div>
    <hr />


    <div style="font-family:Arial;">
        <%=Saved.Code.Common.GetNFTDisplayList(false, this) %>
    </div>
</asp:Content>
