<%@ Page Title="Contact View" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ContactView.aspx.cs" Inherits="Saved.ContactView" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>View Contact</h2>

    <br />
    <br />
    <br />

   <table>

       <tr><td width="35%" nowrap>Contact First Name:</td>
           <td>    <asp:TextBox width="500px" ID="txtFirstName" runat="server"></asp:TextBox></td></tr>

       <tr><td width="35%">    <asp:Label ID="Label3" runat="server" Text="Contact Last Name:"></asp:Label></td>
           <td>    <asp:TextBox width="500px" ID="txtLastName" runat="server"></asp:TextBox></td></tr>
       <tr><td width="35%">    <asp:Label ID="Label4" runat="server" Text="Contact E-Mail Address (OPTIONAL):"></asp:Label></td>
           <td>    <asp:TextBox width="500px" ID="txtEmailAddress" runat="server"></asp:TextBox></td></tr>

       <tr><td>Salvation Status:</td><td>
       <!-- Salvation Statuses -->
       <asp:dropdownlist runat="server" id="ddlStatus"> 
           <asp:listitem text="Atheist" value="Atheist"></asp:listitem>
           <asp:listitem text="Agnostic" value="Agnostic"></asp:listitem>
           <asp:listitem text="Buddhist" value="Buddhist"></asp:listitem>
           <asp:listitem text="Muslim" value="Muslim"></asp:listitem>
           <asp:listitem text="Indian/Hindu" value="Indian/Hindu"></asp:listitem>
           <asp:listitem text="Other/See Notes" value="Other/See Notes"></asp:listitem>
           <asp:listitem text="Lukewarm/Backslider" value="Lukewarm/Backslider"></asp:listitem>
           
           <asp:listitem text="Saved" value="Saved"></asp:listitem>

       </asp:dropdownlist>
           </td></tr>

       <tr><td colspan="2">
       <asp:Button ID="btnSave" runat="server" Text="Update Contact" OnClick="btnUpdate_Click" />
           </td></td></tr>

       <!-- Salvation History -->
       <tr><td></td><td>In this section, you can enter notes about what recent steps you took to help save this person.  For example, enter subject 'Met with Joe for coffee', 
           <p> and for the body, spoke about Jesus and shared scriptures from Matthew, etc. <p> These notes will accumulate over time giving you an idea of where you left off.
           </td></tr>

        <tr><td> Salvation Notes Subject:</td>
           <td>    <asp:TextBox width="500px" ID="txtNotesSubject" runat="server"></asp:TextBox></td></tr>

       <tr><td>Salvation Notes:</td><td>
          <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine"  Rows="30" style="width: 1200px">
          </asp:TextBox>
           </td></tr>


       
       <tr><td colspan="2">
       <asp:Button ID="btnSaveNotes" runat="server" Text="Save Notes to History" OnClick="btnSaveNotes_Click" />
           </td></td></tr>
    

   </table>
    <!-- Historical Notes area -->
    <br />
        <p>Historical Salvation Notes:</p>
          </p><p></p>
    <%=GetHistoricalNotes() %>
</asp:Content>
