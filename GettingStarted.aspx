<%@ Page Title="Getting Started" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="GettingStarted.aspx.cs" Inherits="Saved.GettingStarted" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <p>

    </p>



    <h3><u>Getting Started as a RandomX Miner:</u></h3>


    <div>
        <h3>1.     Download the XMRig Miner</h3>
        <p>

        </p>
        <p>Download the miner from <a href="https://github.com/xmrig/xmrig/releases">here.</a>
        </p>
        <p></p>
        <p></p>
        <h3>2.     Configure the miner with your mining configuration:</h3>
        <p>

            <table>
                <tr><td>Username:<td>Your Monero Wallet Address                </tr>
                <tr><td>Password:<td>Your BiblePay Wallet Address</tr>
                <tr><td>Algorithm:<td>RandomX</tr>
                <tr><td>URL:<td><%=Saved.Code.Common.GetBMSConfigurationKeyValue("PoolDNS")%>:3001</tr>
                <tr><td>&nbsp;</td></tr>
                <tr><td>Example:<td style="background-color:yellow;">./xmrig -o <%=Saved.Code.Common.GetBMSConfigurationKeyValue("PoolDNS")%>:3001 --user=your_monero_address.your_worker_name --password=your_bbp_address</tr>
                <tr><td>Note:<td>your_worker_name is optional, but if provided it adds your cpu information per worker to xmrig.                    </tr>

                <tr><td></td><td>Monitor your workers and Monero earnings at <a target="_blank" href="https://minexmr.com/dashboard">minexmr.com</a></td></tr>
                <tr><td>&nbsp;</td></tr>
                <tr><td></td><td>Sample Windows batch file with failover pools (replace the parameters on the first 3 lines with your info):<br />
                <pre style="width:60vw;">
                                         set worker=miner00
                                         set xmr=YourXMRAddress
                                         set bbp=YourBBPAddress
                                         START xmrig.exe -o bbp.hanalani.org:3001 -u %xmr%.%worker% -p %bbp% -o foundation.biblepay.org:3001 -u %xmr%.%worker% -p %bbp% --pause-on-battery</pre>
                             </td></tr>


            </table>
        </p>
    </div>

</asp:Content>
