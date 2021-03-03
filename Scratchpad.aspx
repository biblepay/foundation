<%@ Page Title="Faucet" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Scratchpad.aspx.cs" Inherits="Saved.Scratchpad" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <br />
    <h3>ScratchPad</h3>

    <hr />

    <br />
    <br />
    <pre>
        <%=Saved.Code.Common.sScratchpad %>
    </pre>


    <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine"  Rows="20" style="width: 1100px" />

    <br />
    <br />

    <br />
    <asp:Button ID="btnScratchpad" runat="server" Text="Scratchpad" OnClick="btnScratchpad_Click" />

    <br />

</asp:Content>
