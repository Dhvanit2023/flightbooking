<%@ Page Title="Select Your Seats" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="selectset.aspx.cs" Inherits="project01.SelectTicket" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <style>
        /* CSS for Seat Selection Page */
        .seat-container { display: flex; max-width: 1000px; margin: 30px auto; background-color: #f0f4f8; border-radius: 15px; box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1); padding: 30px; color: #333; }
        .seat-map-area { flex: 2; padding: 20px; border-right: 1px solid #cdd5e0; }
        .summary-area { flex: 1; padding: 20px; }
        .status-box { background: #e9ecef; padding: 15px; border-radius: 10px; margin-bottom: 20px; border: 1px solid #dee2e6; }
        h2 { color: #0056b3; border-bottom: 2px solid #0056b3; padding-bottom: 10px; margin-bottom: 20px; }
        .row { display: flex; justify-content: center; gap: 5px; margin-bottom: 8px; align-items: center; }
        .seat { width: 35px; height: 35px; border-radius: 5px; background: #90a4ae; color: #fff; display: flex; align-items: center; justify-content: center; font-size: 12px; cursor: pointer; transition: transform 0.2s, background 0.2s; font-weight: bold; }
        .seat:hover:not(.booked) { transform: scale(1.1); background-color: #3b5998; }
        .seat.selected { background: #28a745; transform: scale(1.1); }
        .seat.booked { background: #6c757d; cursor: not-allowed; opacity: 0.6; }
        .aisle-space { width: 30px; height: 35px; }
        .btn-confirm { padding: 12px 25px; background: #007bff; color: #fff; border: none; border-radius: 8px; cursor: pointer; font-size: 18px; font-weight: bold; transition: background 0.3s; width: 100%; margin-top: 15px; }
        .btn-confirm:hover { background: #0056b3; }
        .price-detail strong { color: #dc3545; font-size: 1.2em; }
    </style>

    <div class="seat-container">
        <div class="seat-map-area">
            <h2><asp:Label ID="lblClass" runat="server" Text="Select Class:" /></h2>
            <p class="text-sm mb-4"><asp:Label ID="lblMessage" runat="server" ForeColor="Red" /></p>
            
            <div style="text-align:center; padding: 10px; margin-bottom: 20px; background: #e0e7f1; border-radius: 8px; border: 1px solid #cdd5e0;">
                <p>✈️ <span style="font-weight:bold;">Front of Plane / Cockpit</span> ✈️</p>
            </div>
            
            <div id="seatMap" runat="server" style="display: flex; flex-direction: column; align-items: center;">
                </div>
            
            <div style="text-align:center; padding: 10px; margin-top: 20px; background: #e0e7f1; border-radius: 8px; border: 1px solid #cdd5e0;">
                <p>🚽 <span style="font-weight:bold;">Rear / Washroom Area</span> 🚽</p>
            </div>
        </div>

        <div class="summary-area">
            <h2>Booking Summary</h2>
            <div class="status-box">
                <p>Seats Selected: <strong id="selectedSeatsCountSpan"><asp:Label ID="lblSelectedSeatsCount" runat="server" Text="0" /></strong> / <asp:Label ID="lblPassengerCount" runat="server" /> Passengers</p>
                <p>Selected Seats: <strong id="selectedSeatsListSpan"><asp:Label ID="lblSelectedSeats" runat="server" Text="N/A" /></strong></p>
                <div class="price-detail">
                    <p>Base Fare (per seat): <asp:Label ID="lblBasePriceDisplay" runat="server" Text="₹0" /></p>
                    <p style="font-size: 1.1em; margin-top: 10px;">Total Price: <strong id="totalPriceSpan"><asp:Label ID="lblCurrentPrice" runat="server" Text="₹0" /></strong></p>
                </div>
            </div>

            <div class="status-box">
                <p class="font-bold mb-3">Seat Surcharge Multipliers:</p>
                <ul class="text-sm mt-2" style="list-style: disc; margin-left: 20px;">
                    <li>Window Seat: <strong>1.5x</strong> (15% Extra)</li>
                    <li>Aisle Seat: <strong>1.05x</strong> (5% Extra)</li>
                    <li>Middle Seat: <strong>1.0x</strong> (Base Price)</li>
                </ul>
            </div>
            
            <asp:HiddenField ID="hfFlightID" runat="server" />
            <asp:HiddenField ID="hfFlightClassID" runat="server" />
            <asp:HiddenField ID="hfBasePrice" runat="server" />
            <asp:HiddenField ID="hfPassengerCount" runat="server" />

            <asp:HiddenField ID="hfSelectedSeats" runat="server" />
            <asp:HiddenField ID="hfTotalPrice" runat="server" />
            <asp:HiddenField ID="hfWindowCount" runat="server" Value="0" />
            <asp:HiddenField ID="hfMiddleCount" runat="server" Value="0" />
            <asp:HiddenField ID="hfIntermediateCount" runat="server" Value="0" />

            <asp:Button ID="btnContinue" runat="server" Text="Continue to Confirmation" CssClass="btn-confirm" OnClick="btnContinue_Click" />
        </div>
    </div>
    
    <script type="text/javascript">
        $(document).ready(function () {
            // --- Seat Price Multipliers ---
            const WINDOW_MULTIPLIER = 1.15; // 15% extra
            const INTERMEDIATE_MULTIPLIER = 1.05; // 5% extra
            const MIDDLE_MULTIPLIER = 1.00; // 0% extra

            // --- Initialization ---
            const passengerLimit = parseInt($('#<%= hfPassengerCount.ClientID %>').val());
            const basePrice = parseFloat($('#<%= hfBasePrice.ClientID %>').val());
            let selectedSeats = [];

            function calculatePrice() {
                let totalPrice = 0;
                let windowCount = 0;
                let middleCount = 0;
                let intermediateCount = 0;

                selectedSeats.forEach(seatNum => {
                    // Find the seat element using its text content
                    const seatElement = $(`.seat:contains('${seatNum}'):not(.booked)`);
                    if (seatElement.length === 0) return;

                    const seatType = seatElement.data('seattype');

                    let multiplier = MIDDLE_MULTIPLIER;
                    if (seatType === 'window') {
                        multiplier = WINDOW_MULTIPLIER;
                        windowCount++;
                    } else if (seatType === 'intermediate') {
                        multiplier = INTERMEDIATE_MULTIPLIER;
                        intermediateCount++;
                    } else {
                        middleCount++;
                    }

                    totalPrice += basePrice * multiplier;
                });

                // Update UI and Hidden Fields
                totalPrice = Math.round(totalPrice * 100) / 100;
                $('#selectedSeatsCountSpan').text(selectedSeats.length);
                $('#selectedSeatsListSpan').text(selectedSeats.join(', ') || 'N/A');
                $('#totalPriceSpan').text('₹' + totalPrice.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));

                // Update Hidden Fields for C# processing
                $('#<%= hfSelectedSeats.ClientID %>').val(selectedSeats.join(','));
                $('#<%= hfTotalPrice.ClientID %>').val(totalPrice);
                $('#<%= hfWindowCount.ClientID %>').val(windowCount);
                $('#<%= hfMiddleCount.ClientID %>').val(middleCount);
                $('#<%= hfIntermediateCount.ClientID %>').val(intermediateCount);

                // Enable/Disable Continue button
                const btn = $('#<%= btnContinue.ClientID %>');
                if (selectedSeats.length === passengerLimit) {
                    btn.prop('disabled', false).css('opacity', '1.0');
                    $('#<%= lblMessage.ClientID %>').text('Seats selection complete.').css('color', '#28a745');
                } else {
                    btn.prop('disabled', true).css('opacity', '0.6');
                    const remaining = passengerLimit - selectedSeats.length;
                    $('#<%= lblMessage.ClientID %>').text(`Please select ${remaining} more seat(s) exactly.`).css('color', '#dc3545');
                }
            }

            // --- Default State Loading from C# ---
            const initialSeatsStr = $('#<%= hfSelectedSeats.ClientID %>').val();
            if (initialSeatsStr) {
                // Initialize JS state from C# calculated defaults
                selectedSeats = initialSeatsStr.split(',').filter(s => s.trim() !== '');

                // Highlight default selected seats visually
                selectedSeats.forEach(seatNum => {
                    const seatElement = $(`.seat:contains('${seatNum}'):not(.booked)`);
                    if (seatElement.length > 0) {
                        seatElement.addClass('selected');
                    }
                });
                // The C# already calculated the initial price, but we call this to ensure button state is correct
                calculatePrice();
            }


            // --- Click Handler for Seat Map (User Change) ---
            $('.seat:not(.booked)').on('click', function () {
                const seatNum = $(this).text();
                const index = selectedSeats.indexOf(seatNum);

                if (index > -1) {
                    // Deselect
                    selectedSeats.splice(index, 1);
                    $(this).removeClass('selected');
                } else {
                    // Select
                    if (selectedSeats.length < passengerLimit) {
                        selectedSeats.push(seatNum);
                        $(this).addClass('selected');
                    } else {
                        alert('You can only select ' + passengerLimit + ' seats.');
                    }
                }
                calculatePrice(); // Recalculate price and update UI after user change
            });

            // Final check on load (important if no seats were selected, though C# should handle default)
            if (selectedSeats.length === 0) {
                calculatePrice();
            }
        });
    </script>
</asp:Content>