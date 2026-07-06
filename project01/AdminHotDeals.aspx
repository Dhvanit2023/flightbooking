<%@ Page Title="Manage Hot Deals" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true"
    CodeBehind="AdminHotDeals.aspx.cs" Inherits="project01.WebForm24" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
.container {
    max-width: 900px; margin: 40px auto; background: #fff; padding: 25px;
    border-radius: 10px; box-shadow: 0 4px 10px rgba(0,0,0,0.1);
}
h2 { text-align: center; color: #007bff; margin-bottom: 20px; }
.table th { background: #007bff; color: white; }
</style>

<div class="container">

    <h2>🔥 Manage Hot Flight Deals</h2>

    <asp:HiddenField ID="hfDealID" runat="server" />

    <!-- ROUTE -->
    <label>Route</label>
    <asp:TextBox ID="txtRoute" runat="server" CssClass="form-control"
        Placeholder="E.g. Mumbai → London"></asp:TextBox>

    <!-- PRICE -->
    <label>Price (₹)</label>
    <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control"
        Placeholder="E.g. 25000"></asp:TextBox>

    <!-- FROM DATE -->
    <label>From Date</label>
    <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control"
        Placeholder="Select Start Date"></asp:TextBox>

    <asp:Calendar ID="calFrom" runat="server" Visible="false"
        OnSelectionChanged="calFrom_SelectionChanged"></asp:Calendar>

    <asp:Button ID="btnShowFrom" runat="server" Text="📅 Select From Date"
        CssClass="btn btn-info mt-2" OnClick="btnShowFrom_Click" />

    <!-- TO DATE -->
    <label>To Date</label>
    <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control"
        Placeholder="Select End Date"></asp:TextBox>

    <asp:Calendar ID="calTo" runat="server" Visible="false"
        OnSelectionChanged="calTo_SelectionChanged"></asp:Calendar>

    <asp:Button ID="btnShowTo" runat="server" Text="📅 Select To Date"
        CssClass="btn btn-info mt-2" OnClick="btnShowTo_Click" />

    <!-- IMAGE -->
    <label>Image URL</label>
    <asp:TextBox ID="txtImage" runat="server" CssClass="form-control"
        Placeholder="Photos/flight1.jpg"></asp:TextBox>


    <div style="text-align:center; margin-top:15px;">
        <asp:Button ID="btnSave" runat="server" Text="💾 Save"
            CssClass="btn btn-primary" OnClick="btnSave_Click" />

        <asp:Button ID="btnClear" runat="server" Text="🧹 Clear"
            CssClass="btn btn-secondary" OnClick="btnClear_Click" />
    </div>

    <hr />

    <!-- GRIDVIEW -->
    <asp:GridView ID="gvDeals" runat="server" AutoGenerateColumns="False"
        CssClass="table table-bordered" DataKeyNames="DealID"
        OnRowEditing="gvDeals_RowEditing"
        OnRowDeleting="gvDeals_RowDeleting"
        OnRowCancelingEdit="gvDeals_RowCancelingEdit"
        OnRowUpdating="gvDeals_RowUpdating">

        <Columns>

            <asp:BoundField DataField="DealID" HeaderText="ID" ReadOnly="true" />

            <asp:TemplateField HeaderText="Route">
                <ItemTemplate><%# Eval("Route") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditRoute" runat="server"
                        Text='<%# Bind("Route") %>' CssClass="form-control" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Price (₹)">
                <ItemTemplate><%# Eval("Price") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditPrice" runat="server"
                        Text='<%# Bind("Price") %>' CssClass="form-control" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Travel Dates">
                <ItemTemplate><%# Eval("TravelDates") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditDates" runat="server"
                        Text='<%# Bind("TravelDates") %>' CssClass="form-control" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Image">
                <ItemTemplate>
                    <img src='<%# Eval("ImageUrl") %>' width="100" />
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditImage" runat="server"
                        Text='<%# Bind("ImageUrl") %>' CssClass="form-control" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />

        </Columns>

    </asp:GridView>

    <asp:Label ID="lblMsg" runat="server" CssClass="text-success fw-bold"></asp:Label>

</div>

</asp:Content>
