<%@ Page Title="Quiz" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="Quiz.aspx.cs" Inherits="Saved.Quiz" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Gospel Quiz</h2>
    <br />

    
    <div>
        
        <table>

            <tr><td>Please read the following verses, for the book of <%=_studybook %> and fill in the missing words.

                <br />
                <font color="red">
                NOTE: Do not worry about punctuation, capitalization, extra spaces, or extra underscores-- only the text will be checked.
                    </font>
                </td></tr>
            <tr><td>                  
                <asp:TextBox onkeyup="var pos1 = this.selectionStart; var ch = this.value.substr(pos1, 1); if (ch=='_') { var part1 = this.value.substr(0, pos1); var part2 = this.value.substr(pos1+1, this.value.length - pos1); this.value = part1 + part2; this.selectionStart = pos1;this.selectionEnd = pos1;}" ID="txtAnswer" runat="server" TextMode="MultiLine" Rows="27" style="width: 1200px">
                    
                </asp:TextBox>
                </td>
             </tr>
            

            <tr><td>Your Pool BiblePay Address:
                <tr><td><asp:TextBox ID="txtBBP" width="340px" readonly runat="server"></asp:TextBox></td></tr>

             <tr><td>
               <asp:Button ID="btnSave" runat="server" Text="Submit Answer" OnClick="btnSave_Click" />
           </td></td></tr>

       </table>



    </div>
    


</asp:Content>
