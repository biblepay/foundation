<%@ Page Title="FractionalSanctuaries" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FractionalSanctuaries.aspx.cs" Inherits="Saved.FractionalSanctuaries" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <br />
    <h3>Active Fractional Sanctuaries</h3>

    <hr />

    <%=_report %>

    <br />

     
    <%=GetROIGauge() %>
        
    <h3>Network:</h3>
    
    <table>

        <tr><td>    Global Sanctuary Investment Amount:          <td>    <asp:TextBox ID="txtGlobalSancInvestments" width="200px" readonly runat="server" ></asp:TextBox></tr>
        <tr><td>    Global Sanctuary Investment Count:          <td>    <asp:TextBox ID="txtGlobalSancInvestmentCount" width="200px" readonly runat="server" ></asp:TextBox></tr>
        <tr><td>    HODL %:          <td>    <asp:TextBox ID="txtHODLPercent" width="200px" readonly runat="server" ></asp:TextBox> 
            &nbsp;[<%=GetNonCompounded()%>% non-compounded]&nbsp;<small><font color="red">* Based on actual chain data over 48 hours</font></small>
        <tr><td colspan="2">Note:  To receive compounded ROI on your HODL, you must transfer your earned BBP rewards into your fractional sanctuary once per month. </td></tr>
    
    </table>
    <hr />

    <h3>My Account:</h3>

    <table>
        <tr><td>    My Fractional Sanctuary Investments:          <td>    <asp:TextBox ID="txtFractionalSancBalance" width="200px" readonly runat="server" ></asp:TextBox></tr>
        <tr><td>    My Sanctuary Rewards Received:  <td>    <asp:TextBox ID="txtRewards" width="200px" readonly runat="server" ></asp:TextBox></tr>
        <tr><td>    My BBP Balance:  <td>    <asp:TextBox ID="txtBalance" width="200px" readonly runat="server" ></asp:TextBox></tr>
    </table>
    <asp:Button ID="btnFracReport" runat="server" Text="Run Fractional Sanctuary Report" OnClick="btnFracReport_Click" />

    <br />
    <hr />

    <h3>Add Fractional Sanctuary</h3>
    <br />
    Note:  If you add to your fractional sanctuary balance, we will deduct this amount from your BiblePay foundation balance, and transfer it into the Active Fractional Sanctuary HODL farm.
    <br />
    If you already have an active fractional sanc, we will add to its size automatically.
    <br />
    Once per day, we will compute the total sanctuary rewards in the farm, and split these according to your size.
    <br />
    <br />

    Example: 
    If you maintain a 1MM BBP fractional sanctuary balance here, you will receive a reward equal to 22% of a full sanctuary, credited to your account once per day.
    <br />

    You will see the detailed transactions in the Fractional Sanctuary report, available here.
    <br />
    
    You may add up to: <%=GetBalance() %>.
    
    <br />
    <table>


    <tr><td>Add Fractional Sanctuary Amount: &nbsp;<td>    <asp:TextBox ID="txtAmount" width="200px" runat="server" ></asp:TextBox></tr>
    </table>
    <asp:Button ID="btnAddFractionalSanctuary" runat="server" Text="Add Fractional Sanctuary" OnClick="btnAddFractionalSanctuary_Click" />



    <hr />

    <h3>Liquidate Fractional Sanctuary</h3>
    <br />
    Note: This option allows you to cash out, or liquidate, part of or all of your fractional sanctuary.  This will transfer the fractional balance back to your free BBP balance.
    But it will also cause you to stop earning rewards.  Note that you should not try to switch in and out of fractional sanctuaries, as we may put a hold on your balance for 24 hours.
    <br />
    You may remove up to: <%=GetTotalSancInvestment() %>.

    <br />

    <table>

    <tr><td>Remove Fractional Sanctuary Amount: &nbsp;<td>    <asp:TextBox ID="txtRemoveFractionalAmount" width="200px" runat="server" ></asp:TextBox></tr>
    </table>
    <asp:Button ID="btnRemove" runat="server" Text="Remove Fractional Sanctuary" OnClick="btnRemoveFractionalSanctuary_Click" />



</asp:Content>
