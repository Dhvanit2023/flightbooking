<%@ Page Title="OTP Verification" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Verify.aspx.cs" Inherits="project01.WebForm7" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">



    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="container">
                <div id="otpOverlay" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.7); z-index: 9999; display: flex; justify-content: center; align-items: center;">
                    <div style="background: #fff; padding: 30px; border-radius: 8px; text-align: center;">
                        <h2>Enter OTP</h2>
                        <asp:TextBox ID="txtOTP" runat="server" placeholder="Enter OTP" CssClass="form-control" />
                        <asp:Button ID="btnVerifyOTP" runat="server" Text="Verify OTP" CssClass="btn btn-primary" OnClick="btnVerifyOTP_Click" />
                        <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
        </Triggers>
    </asp:UpdatePanel>

    <asp:Timer ID="Timer1" runat="server" Interval="3000" OnTick="Timer1_Tick" />

</asp:Content>
