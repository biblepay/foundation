<%@ Page Title="Partners" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Partners.aspx.cs" Inherits="Saved._Partners" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


          <h3>Partners</h3>
    <br />


    <div id="divPartners">

         <b>   BiblePay only partners with the highest efficiency charities, with more than 75% of charitable funding reaching the beneficiary.
             </b>

    </div>


    <br />
    <br />
    <%=GetPartners() %>
    <br />
    <br />
    <h3>Case Studies by our partners:</h3>
    <br />

    <table>
        <tr><td width="20%">SAI<td width="20%">Orphans<td width="50%">&nbsp;<a href=https://sai.ngo/wp-content/uploads/2019/11/SAI-Brochure-1.pdf>PDF</a></tr>
        <tr><td>SAI<td>Animals<td>&nbsp;<a href=https://sai.ngo/wp-content/uploads/2019/11/Animals-SAI-.pdf>PDF</a></tr>
    </table>

    
</asp:Content>
