<%@ Page Title="Address Book" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="AddressBook.aspx.cs" Inherits="Saved.AddressBook" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Address Book</h2>

    <small><font color="red">** For your convenience, your address book entries will be encrypted with your user-guid so that no one can see them.  
        The address will be decrypted on the fly when you buy something through the API, and only visible to Amazon. **</font></small>

    <fieldset>
    <legend>Add a new Address Book Entry:</legend>

        <label for="txtFirstName">First Name:</label>
        <br/>
        <asp:TextBox ID="txtFirstName" width="250px" runat="server" ></asp:TextBox>
        <br />

        <label for="txtLastName">Last Name:</label>
        <br/>
        <asp:TextBox ID="txtLastName" width="250px" runat="server" ></asp:TextBox>
        <br />

        <label for="txtAddressLine1">Address Line 1:</label>
        <br/>
        <asp:TextBox ID="txtAddressLine1" width="250px" runat="server" ></asp:TextBox>
        <br />

        <label for="txtAddressLine2">Address Line 2:</label>
        <br/>
        <asp:TextBox ID="txtAddressLine2" width="250px" runat="server" ></asp:TextBox>
        <br />

        <label for="txtCity">City:</label>
        <br/>
        <asp:TextBox ID="txtCity" width="250px" runat="server" ></asp:TextBox>
        <br />

        <label for="txtState">State:</label>
        <br/>
        <asp:TextBox ID="txtState" MaxLength="2" width="50px" runat="server" ></asp:TextBox>
        <br />

        <label for="txtPostalCode">Postal Code:</label>
        <br/>
        <asp:TextBox ID="txtPostalCode" maxlength="5" width="120px" runat="server" ></asp:TextBox>
        <br />


        <label for="txtCountry">Country:</label>
        <br/>
        <asp:TextBox ID="txtCountry" width="250px" ReadOnly="true" value="US" runat="server" ></asp:TextBox>
        <br />
        <br />

        <asp:Button class='button' ID="btnAdd" runat="server" Text="Add to Address Book" OnClick="btnAdd_Click" />


  </fieldset>


    
    <hr />

    <h2>My Address Book Contents:</h2>

    <%=GetAddressBookList() %>





</asp:Content>
