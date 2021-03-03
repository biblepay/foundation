<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="PoolAbout.aspx.cs" Inherits="Saved.PoolAbout" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3>About - BiblePay Pool</h3>

    <p>            </p>
    <p>&nbsp;</p>


    <table><tr><td rowspan="10" width="35%">  <img src="<%=GetImgSource()%>" height="300px" /></tr>
        <tr><td><td width="50%">&#8227;&nbsp;Welcome to the Future of Orphan-Mining</td></tr>

        <tr><td><td>&#8227;&nbsp;Orphan-Charity - Fulfilling James 1:27</tr>

        <tr><td><td>&#8227;&nbsp;RandomX Mining with Dual Hash Rewards (BBP + XMR)</tr>
        <tr><td><td>&#8227;&nbsp;User Friendly</tr>
        <tr><td><td>&#8227;&nbsp;Partnering with the highest efficiency orphan charities</tr>
    </table>

    <br />
    <p>
        <span><font color="red">
            <br /><%=Saved.Code.PoolCommon.PoolBonusNarrative() %>

              </font></span>
    </p>
    
    <hr />
    <%=GetPoolAboutMetrics() %>
    <br />
    <table>
        <tr><td width="80%">
            <img src="Images/workers.png" width="95%"/>
            </td>
        </tr>
        <tr>
            <td>
            <img src="Images/hashrate.png" width="95%"/>
            </td>
        </tr>
        <tr>
            <td>
            <img src="Images/blockssolved.png" width="95%"/>
            </td>
        </tr>


    </table>




</asp:Content>
