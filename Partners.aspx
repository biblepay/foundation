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
    
    
</asp:Content>
