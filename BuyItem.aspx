<%@ Page Title="BuyItem" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="BuyItem.aspx.cs" Inherits="Saved.BuyItem" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Buy Item</h2>


    Item:
    <%=GetAmzItem(true) %>

    <br />

    Delivery Address: <font color="red">** These addresses are only readable by you, and have been decrypted using your user-guid for your convenience **</font>
    <br />
       <asp:dropdownlist runat="server" id="ddDeliveryAddress" style="font-family:OCR A;"> 
       </asp:dropdownlist>
    

    <br />

    <font color="red"><small>By clicking Buy Item, you agree that we may deduct the amount shown above in BBP from your account.  The item will be delivered to the address above.  
        You will receive status updates continuously until the item is delivered.  Once an order is placed it cannot be cancelled.  
        If the item is not available, or if our supplier fails to the deliver the item, you will automatically be refunded the full purchase price.  
        Shipping is FREE and the price is all inclusive.  

        Any applicable local, state and federal taxes are included in the total cost.  
        </small></font>
    <br />

        
    <asp:Button ID="btnBuy" runat="server" Text="Buy Item" OnClick="btnBuy_Click" />
    <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" />

    <br />



</asp:Content>
