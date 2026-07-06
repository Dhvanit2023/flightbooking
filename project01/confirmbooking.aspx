<%@ Page Title="Confirm Booking" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="confirmbooking.aspx.cs" Inherits="project01.WebForm11" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Enter Passenger Details</h2>
    <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>

    <asp:HiddenField ID="hdnSeats" runat="server" />

    <asp:Repeater ID="rptPassengers" runat="server">
        <HeaderTemplate>
            <hr />
        </HeaderTemplate>
        <ItemTemplate>
            <div style="border: 1px solid #ccc; padding: 15px; margin-bottom: 20px; border-radius: 5px;">
                <h3>Passenger #<%# Container.ItemIndex + 1 %> (Seat: <%# Eval("SeatNumber") %>)</h3>
                <table class="passenger-table">
                    <tr>
                        <td style="width: 150px;">Name:</td>
                        <td><asp:TextBox ID="txtName" runat="server" MaxLength="100"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>Age:</td>
                        <td><asp:TextBox ID="txtAge" runat="server" TextMode="Number"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>Gender:</td>
                        <td>
                            <asp:DropDownList ID="ddlGender" runat="server">
                                <asp:ListItem Text="Male" Value="Male"></asp:ListItem>
                                <asp:ListItem Text="Female" Value="Female"></asp:ListItem>
                                <asp:ListItem Text="Other" Value="Other"></asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>Aadhaar Number:</td>
                        <td><asp:TextBox ID="txtAadhaar" runat="server" MaxLength="12"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>Address:</td>
                        <td><asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>Occupation:</td>
                        <td><asp:TextBox ID="txtOccupation" runat="server" MaxLength="50"></asp:TextBox></td>
                    </tr>
                    <asp:HiddenField ID="hdnSeatNumber" runat="server" Value='<%# Eval("SeatNumber") %>' />
                </table>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            <hr />
            <asp:Button ID="btnSubmit" runat="server" Text="Proceed to Payment" OnClick="btnSubmit_Click" CssClass="btn-primary" />
        </FooterTemplate>
    </asp:Repeater>

</asp:Content>