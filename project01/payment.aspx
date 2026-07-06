<%@ Page Title="Payment" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true"
    CodeBehind="payment.aspx.cs" Inherits="project01.WebForm9" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div style="max-width:600px;margin:50px auto;padding:25px;border:1px solid #ddd;border-radius:8px;">
        <h2>💳 Payment Details</h2>
        <hr />

        <p><b>Total Amount:</b>
            <asp:Label ID="lblTotalAmount" runat="server" Text="0.00"></asp:Label>
        </p>

        <div style="margin-top:10px;">
            <asp:Label ID="lblDiscountLabel" runat="server" Text="Discount Code:" AssociatedControlID="txtDiscount"></asp:Label>
            <asp:TextBox ID="txtDiscount" runat="server" CssClass="form-control" />
            <asp:Button ID="btnApplyDiscount" runat="server" Text="Apply" CssClass="btn btn-secondary btn-sm"
                OnClick="btnApplyDiscount_Click" />
        </div>

        <br />
        <asp:Label ID="lblDiscountMessage" runat="server" ForeColor="Blue"></asp:Label>

        <p><b>Discount Applied:</b>
            <asp:Label ID="lblDiscountAmount" runat="server" Text="0.00" ForeColor="Red"></asp:Label>
        </p>

        <h3><b>Final Payable:</b>
            <asp:Label ID="lblFinalAmount" runat="server" Text="0.00" ForeColor="Blue"></asp:Label>
        </h3>

        <asp:HiddenField ID="hdnFinalAmount" runat="server" />

        <!-- Pay button that does NOT post back -->
        <asp:Button ID="btnPay" runat="server" Text="Pay Now" CssClass="btn btn-primary btn-lg"
            UseSubmitBehavior="false" OnClientClick="return openRazorpay();" />
    </div>

    <!-- Razorpay Checkout JS -->
    <script src="https://checkout.razorpay.com/v1/checkout.js"></script>

    <script type="text/javascript">
        function openRazorpay() {
            var finalAmount = document.getElementById('<%=hdnFinalAmount.ClientID%>').value;
            if (!finalAmount || isNaN(finalAmount) || parseFloat(finalAmount) <= 0) {
                alert("Invalid amount. Please refresh and try again.");
                return false;
            }

            var bookingId = "<%=Session["CurrentBookingId"] ?? ""%>";

            if (!bookingId) {
                alert("Booking ID missing. Please restart the booking process.");
                return false;
            }

            var options = {
                "key": "rzp_test_RL7wtJsjhGQKdb", // Replace with your Razorpay key
                "amount": parseInt(parseFloat(finalAmount) * 100),
                "currency": "INR",
                "name": "Flight Booking System",
                "description": "Payment for Booking ID: " + bookingId,
                "handler": function (response) {
                    // ✅ Redirect to success page on payment success
                    window.location.href = "PaymentSuccess.aspx?bookingId=" + bookingId +
                        "&payment_id=" + response.razorpay_payment_id +
                        "&amount=" + finalAmount;
                },
                "theme": {
                    "color": "#007bff"
                }
            };

            var rzp = new Razorpay(options);
            rzp.open();

            return false; // prevent form submit
        }
    </script>

</asp:Content>
