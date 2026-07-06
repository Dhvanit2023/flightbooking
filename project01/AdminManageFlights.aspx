<%@ Page Title="Manage Flights" Language="C#" MasterPageFile="~/Site1.Master"
    AutoEventWireup="true" CodeBehind="AdminManageFlights.aspx.cs"
    Inherits="project01.WebForm18" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body { background-color:#f4f6f9; font-family:'Segoe UI'; }
    .container { max-width:1100px; margin:30px auto; background:white; padding:25px;
                border-radius:10px; box-shadow:0 4px 10px rgba(0,0,0,0.1);}
    h2 { text-align:center; color:#007bff; margin-bottom:20px; }
    .form-section{ display:flex; flex-wrap:wrap; gap:15px; margin-bottom:10px; }
    label{ font-weight:600; }
    input, select{ width:100%; padding:8px; border:1px solid #ccc; border-radius:6px; box-sizing:border-box; }
    .btn{ background:#007bff; color:white; padding:8px 15px; border:none; border-radius:5px; cursor:pointer;}
    .btn:hover{ opacity:.85; }
    .table { width:100%; border-collapse:collapse; margin-top:20px;}
    .table th { background:#007bff; color:white; padding:10px; }
    .table td { padding:8px; border:1px solid #ddd; text-align:center; }
    .message { margin-top:15px; font-weight:bold; color:#28a745; text-align:center; }
    .nested-grid { margin-top:10px; background:#f9f9f9; padding:10px; border-radius:8px; }
    .nested-grid th { background:#17a2b8; }
    .small { font-size:0.9rem; color:#555; }
    .btn-secondary { background:#6c757d; color:white; padding:6px 10px; border-radius:4px; border:0; cursor:pointer;}
</style>

<div class="container">

    <h2>✈ Manage Flights and Classes</h2>

    <!-- Add Flight Form -->
    <div class="form-section">
        <div style="flex:1;">
            <label>Flight Number</label>
            <asp:TextBox ID="txtFlightNumber" runat="server" />
        </div>
        <div style="flex:1;">
            <label>Source Airport</label>
            <asp:DropDownList ID="ddlSource" runat="server" />
        </div>
        <div style="flex:1;">
            <label>Destination Airport</label>
            <asp:DropDownList ID="ddlDestination" runat="server" />
        </div>
    </div>

    <div class="form-section">
        <div style="flex:1;">
            <label>Aircraft</label>
            <asp:DropDownList ID="ddlAircraft" runat="server" />
        </div>
        <div style="flex:1;">
            <label>Departure</label>
            <asp:TextBox ID="txtDeparture" runat="server" TextMode="DateTimeLocal" />
        </div>
        <div style="flex:1;">
            <label>Arrival</label>
            <asp:TextBox ID="txtArrival" runat="server" TextMode="DateTimeLocal" />
        </div>
    </div>

    <!-- NEW: Add multiple classes (dropdown) -->
    <div class="nested-grid" style="margin-top:12px;">
        <h3 style="margin:0 0 8px 0;">Add Flight Classes (choose from dropdown)</h3>

        <table id="tblNewClasses" class="table">
            <thead>
                <tr>
                    <th>Class Name</th>
                    <th>Base Price</th>
                    <th>Price Multiplier</th>
                    <th>Seats</th>
                    <th>Remove</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>
                        <select name="newClassName">
                            <option>Economy</option>
                            <option>Premium Economy</option>
                            <option>Business</option>
                            <option>First Class</option>
                        </select>
                    </td>
                    <td><input name="newBasePrice" type="number" step="0.01" min="0" required /></td>
                    <td><input name="newMultiplier" type="number" step="0.1" min="0.1" value="1" required /></td>
                    <td><input name="newSeats" type="number" min="1" required /></td>
                    <td><button type="button" class="btn-secondary" onclick="removeNewClassRow(this)">Remove</button></td>
                </tr>
            </tbody>
        </table>

        <div style="margin-top:8px;">
            <button type="button" class="btn" onclick="addNewClassRow()">Add Class Row</button>
            <span class="small" style="margin-left:12px;">You can add multiple class rows before saving.</span>
        </div>
    </div>

    <div class="form-section" style="justify-content:center; margin-top:12px;">
        <asp:Button ID="btnSave" runat="server" Text="Add Flight" CssClass="btn" OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
    </div>

    <!-- Existing Flights Grid (your original code preserved) -->
    <asp:GridView ID="gvFlights" runat="server"
        CssClass="table"
        AutoGenerateColumns="False"
        DataKeyNames="FlightID"
        AllowPaging="True"
        PageSize="6"
        OnPageIndexChanging="gvFlights_PageIndexChanging"
        OnRowEditing="gvFlights_RowEditing"
        OnRowCancelingEdit="gvFlights_RowCancelingEdit"
        OnRowUpdating="gvFlights_RowUpdating"
        OnRowDeleting="gvFlights_RowDeleting"
        OnRowDataBound="gvFlights_RowDataBound">

        <Columns>

            <asp:BoundField DataField="FlightID" HeaderText="ID" ReadOnly="true" />

            <asp:TemplateField HeaderText="Flight No">
                <ItemTemplate><%# Eval("FlightNumber") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditFlightNumber" runat="server"
                        Text='<%# Bind("FlightNumber") %>' />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="From">
                <ItemTemplate><%# Eval("SourceAirport") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlEditSource" runat="server" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="To">
                <ItemTemplate><%# Eval("DestinationAirport") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlEditDestination" runat="server" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Departure">
                <ItemTemplate><%# Eval("DepartureTime", "{0:dd-MMM yyyy HH:mm}") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditDeparture" runat="server"
                        Text='<%# Bind("DepartureTime", "{0:yyyy-MM-ddTHH:mm}") %>'
                        TextMode="DateTimeLocal" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Arrival">
                <ItemTemplate><%# Eval("ArrivalTime", "{0:dd-MMM yyyy HH:mm}") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditArrival" runat="server"
                        Text='<%# Bind("ArrivalTime", "{0:yyyy-MM-ddTHH:mm}") %>'
                        TextMode="DateTimeLocal" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Aircraft">
                <ItemTemplate><%# Eval("AircraftName") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlEditAircraft" runat="server" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Flight Classes" SortExpression="">
                <ItemTemplate>
                    <div class="nested-grid">
                        <asp:GridView ID="gvFlightClasses" runat="server"
                            AutoGenerateColumns="False"
                            DataKeyNames="FlightClassID"
                            CssClass="table"
                            ShowFooter="True"
                            OnRowEditing="gvFlightClasses_RowEditing"
                            OnRowCancelingEdit="gvFlightClasses_RowCancelingEdit"
                            OnRowUpdating="gvFlightClasses_RowUpdating"
                            OnRowDeleting="gvFlightClasses_RowDeleting"
                            OnRowCommand="gvFlightClasses_RowCommand"
                            OnRowDataBound="gvFlightClasses_RowDataBound"
                            PageSize="5"
                            AllowPaging="True"
                            OnPageIndexChanging="gvFlightClasses_PageIndexChanging">

                            <Columns>
                                <asp:BoundField DataField="FlightClassID" HeaderText="Class ID" ReadOnly="true" />

                                <asp:TemplateField HeaderText="Class Name">
                                    <ItemTemplate><%# Eval("ClassName") %></ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtClassName" runat="server"
                                            Text='<%# Bind("ClassName") %>' CssClass="form-control" />
                                    </EditItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewClassName" runat="server" CssClass="form-control" />
                                    </FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Base Price">
                                    <ItemTemplate><%# Eval("BasePrice", "{0:C}") %></ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtBasePrice" runat="server"
                                            Text='<%# Bind("BasePrice") %>' CssClass="form-control" />
                                    </EditItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewBasePrice" runat="server" CssClass="form-control" />
                                    </FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Price Multiplier">
                                    <ItemTemplate><%# Eval("PriceMultiplier", "{0:N2}") %></ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtPriceMultiplier" runat="server"
                                            Text='<%# Bind("PriceMultiplier") %>' CssClass="form-control" />
                                    </EditItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewPriceMultiplier" runat="server" CssClass="form-control" />
                                    </FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Seats Available">
                                    <ItemTemplate><%# Eval("SeatsAvailable") %></ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtSeatsAvailable" runat="server"
                                            Text='<%# Bind("SeatsAvailable") %>' CssClass="form-control" />
                                    </EditItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewSeatsAvailable" runat="server" CssClass="form-control" />
                                    </FooterTemplate>
                                </asp:TemplateField>

                                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />

                                <asp:ButtonField CommandName="AddNew" Text="Add Class" ShowHeader="False" ButtonType="Button" Visible="false" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:CommandField ShowEditButton="True" />
            <asp:CommandField ShowDeleteButton="True" />

        </Columns>

    </asp:GridView>

    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

</div>

<script>
    function addNewClassRow() {
        var tbody = document.querySelector('#tblNewClasses tbody');
        var tr = document.createElement('tr');

        tr.innerHTML =
            '<td><select name="newClassName">' +
                '<option>Economy</option>' +
                '<option>Premium Economy</option>' +
                '<option>Business</option>' +
                '<option>First Class</option>' +
            '</select></td>' +
            '<td><input name="newBasePrice" type="number" step="0.01" min="0" required /></td>' +
            '<td><input name="newMultiplier" type="number" step="0.1" min="0.1" value="1" required /></td>' +
            '<td><input name="newSeats" type="number" min="1" required /></td>' +
            '<td><button type="button" class="btn-secondary" onclick="removeNewClassRow(this)">Remove</button></td>';

        tbody.appendChild(tr);
    }

    function removeNewClassRow(btn) {
        var tr = btn.closest('tr');
        var tbody = document.querySelector('#tblNewClasses tbody');
        if (tbody.rows.length > 1) tr.remove();
        else alert('At least one class is required.');
    }
</script>

</asp:Content>
