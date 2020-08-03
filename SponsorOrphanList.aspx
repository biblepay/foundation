<%@ Page Title="Sponsor an Orphan - List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="SponsorOrphanList.aspx.cs" Inherits="Saved.SponsorOrphanList" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Sponsor an Orphan (Monthly Sponsorships)</h2>

    <%=GetSponsoredOrphanList() %>
<br />
    <br />


    <ul>FAQs:
        <li>Q: How much BBP is taken if I sponsor now?  
            <br />A: If you sponsor now, we remove the first months payment, and then once every 30 days, we remove the next successive payment automatically.  </li>
        <li>Q:  Can I cancel a child that I have sponsored?  
            <br />A:  Yes, you may cancel a child, but the minimum sponsorship duration is one month, so you will not receive a pro-rated refund, instead you will incur no additional monthly charges.</li>
        <li>Q:  Are these kids sponsored twice?  
            <br />A:  No.  Once you sponsor a child, you will be the primary beneficiary of this child, and we will not sub-sponsor your child to any other sponsors.  Your foundation username will also be seen under the child.</li>
        <li>Q:  Where are these kids coming from?
            <br />A:  Since we transitioned from POOM, the founder of BBP is sustaining the majority of our children, therefore we have some that are available for sponsorship that XMR revenue does not cover.  
            <br />Additionally, the Founder would like to see maximum growth (and maximum net sponsorships), so we are making children available here that are partially subsidized by matches.  </li>
        <li>Q:  Why are the kids from two Charities only?  
            <br />A:  We currently partner with a few vetted charities, (<a href=Partners>see our partners page here</a>), but to expand to new charities we must hold a sanctuary vote.</li>
        <li>Q:  What if I don't understand how to buy BBP on an exchange, or part of this sounds complicated?
            <br />A:  We have a forum at (https://forum.biblepay.org) that can answer some questions.  But feel free to send an e-mail to rob@biblepay.org and the founder will help you get started.
        </li>
        <li>Q:  I don't understand how to open an account here?
            <br />A:  Please register first at https://forum.biblepay.org - then use that username to log into https://foundation.biblepay.org .  If you have any trouble, please e-mail rob@biblepay.org.  Thank You!
        </li>

    </ul>


</asp:Content>
