<%@ Page Title="Flight Search" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Flights.aspx.cs" Inherits="project01.WebForm4" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .booking-container { max-width: 950px; margin: 40px auto; background: rgba(0,0,0,0.6); color:#fff; padding:30px; border-radius:12px; }
        .form-row { display:flex; gap:12px; margin-bottom:12px; }
        .form-col { flex:1; }
        .form-control { width: 100%; padding: 8px; border-radius: 4px; border: 1px solid #ccc; background-color: rgba(255,255,255,0.9); color: #333; }
        .form-col label { display: block; margin-bottom: 4px; font-weight: bold; }
        .btn-search { padding:10px 18px; background:linear-gradient(90deg,#00c6ff,#0072ff); color:#fff; border:none; border-radius:8px; font-size:16px; cursor:pointer; }
        .price-preview { text-align:right; font-size:16px; color:#ffea00; margin-bottom:10px; }
        .recent-searches { margin-top:20px; background:rgba(255,255,255,0.03); padding:12px; border-radius:8px; }
        table.grid { width:100%; border-collapse:collapse; margin-top:14px; }
        table.grid th, table.grid td { padding:8px; border:1px solid rgba(255,255,255,0.06); color:#fff; text-align: left;}
        table.grid th { background-color: rgba(0, 114, 255, 0.4); }
        .no-data { padding:12px; color:#ff6b6b; }
    </style>

    <div class="booking-container">
        <h2>✈️ Search Flights</h2>

        <div class="form-row">
            <div class="form-col">
                <label>From</label>
                <asp:DropDownList ID="ddlFrom" runat="server" CssClass="form-control" />
            </div>
            <div class="form-col">
                <label>To</label>
                <asp:DropDownList ID="ddlTo" runat="server" CssClass="form-control" />
            </div>
            <div class="form-col">
                <label>Class</label>
                <asp:DropDownList ID="ddlClass" runat="server" CssClass="form-control" />
            </div>
        </div>

        <div class="form-row">
            <div class="form-col">
                <label>Departure Date</label>
                <asp:TextBox ID="txtDepartureDate" runat="server" TextMode="Date" CssClass="form-control" />
            </div>
            <div class="form-col">
                <label>Return Date</label>
                <asp:TextBox ID="txtReturnDate" runat="server" TextMode="Date" CssClass="form-control" Enabled="false" />
            </div>
            <div class="form-col">
                <label>Passengers (Adults/Children/Seniors)</label>
                <div style="display:flex; gap:8px;">
                    <asp:DropDownList ID="ddlAdults" runat="server" CssClass="form-control" Width="100px">
                        <asp:ListItem>1</asp:ListItem><asp:ListItem>2</asp:ListItem><asp:ListItem>3</asp:ListItem><asp:ListItem>4</asp:ListItem>
                    </asp:DropDownList>
                    <asp:DropDownList ID="ddlChildren" runat="server" CssClass="form-control" Width="100px">
                        <asp:ListItem>0</asp:ListItem><asp:ListItem>1</asp:ListItem><asp:ListItem>2</asp:ListItem><asp:ListItem>3</asp:ListItem>
                    </asp:DropDownList>
                    <asp:DropDownList ID="ddlSeniors" runat="server" CssClass="form-control" Width="100px">
                        <asp:ListItem>0</asp:ListItem><asp:ListItem>1</asp:ListItem><asp:ListItem>2</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
        </div>

        <div class="price-preview" id="pricePreview2" runat="server">💰 Estimated Price: ₹0</div>

        <div style="text-align:right;">
            <asp:Button ID="btnSearchFlights" runat="server" Text="🔍 Search Flights" CssClass="btn-search" OnClick="btnSearchFlights_Click" />
        </div>

        <asp:GridView ID="gvFlights" runat="server" AutoGenerateColumns="False" CssClass="grid"
            OnRowCommand="gvFlights_RowCommand" DataKeyNames="FlightID,FlightClassID,BasePrice,ClassName">
            <Columns>
                <asp:BoundField DataField="FlightID" HeaderText="ID" />
                <asp:BoundField DataField="FromAirport" HeaderText="From" />
                <asp:BoundField DataField="ToAirport" HeaderText="To" />
                <asp:BoundField DataField="AirlineName" HeaderText="Airline" />
                <asp:BoundField DataField="DepartureTime" HeaderText="Departure" DataFormatString="{0:HH:mm}" />
                <asp:BoundField DataField="ArrivalTime" HeaderText="Arrival" DataFormatString="{0:HH:mm}" />
                <asp:BoundField DataField="ClassName" HeaderText="Class" />
                <asp:BoundField DataField="BasePrice" HeaderText="Base Fare (₹)" DataFormatString="{0:N0}" />
                <asp:TemplateField HeaderText="Action">
                    <ItemTemplate>
                        <asp:Button runat="server" Text="Book Now" CommandName="BookFlight"
                            CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn-search" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <div class="recent-searches">
            <h5>🕓 Recent Bookings</h5>
            <asp:Repeater ID="rptRecentSearches" runat="server">
                <ItemTemplate>
                    <div>
                        <strong><%# Eval("FromAirport") %></strong> → <strong><%# Eval("ToAirport") %></strong> |
                        <%# Eval("ClassName") %> | Booked: <%# Eval("BookingDate", "{0:dd-MMM-yyyy}") %>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>
</asp:Content>