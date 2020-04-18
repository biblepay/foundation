<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HowTo.aspx.cs" Inherits="Saved.HowTo" ValidateRequest="false"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <script>
    document.onkeydown = function(evt) {
    evt = evt || window.event;
        if (evt.keyCode == 32)
        {
             //alert("spacebar");
            var oNext = $("#MainContent_btnNext")[0];
            if (oNext) {
                oNext.click();
            }

        }
    };
    </script>


    <h2><%=Title1 %></h2>
    <br />

<br />
    <div>

        <div style="font-size:18px">
        <%=Body %>
        </div>


    </div>
    <!--Buttons-->
    <asp:Button ID="btnPrevious" runat="server" Text="Previous Point" OnClick="btnPrevious_Click" />

    <asp:Button ID="btnNext" runat="server" Text="Next Point" OnClick="btnNext_Click" />

  
</asp:Content>
