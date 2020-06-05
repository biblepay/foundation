<%@ Page Title="Sponsor an Orphan - List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="DonorMatchList.aspx.cs" Inherits="Saved.DonorMatchList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>List of Donations Recevied for Orphan Sponsorship Matches</h2>

    <%=GetDonorMatchList() %>
    

    <ul>About the BiblePay Donor Match Program:

        <li>The above generous donors have gifted our foundation BiblePay that will be allocated toward matching donations to reduce the cost of monthly orphan sponsorships.</li>
        <li>When a donation is made, we deduct it from your foundation balance immediately.</li>
        <li>Then our algorithm allocates these funds towards donor matches (by increasing the Match Percentage in our Sponsor Monthly Orphan page).</li>
        <li>When a user sponsors a monthly orphan, they will receive the quoted rebate (quoted on the sponsor a child page) automatically once per month to help them cover the cost of the monthly sponsorship.</li>
    </ul>


    <table>
    <tr><td>BBP Amount you would like to Donate Now: &nbsp;<td>    <asp:TextBox ID="txtAmount" width="200px" runat="server" ></asp:TextBox>
        <td> <asp:Button ID="btnDonate" runat="server" onclick="btnDonate_Click"  Text="Donate" style="width:200px" />
        </table>

</asp:Content>
