<%@ Page Title="Manage Coupons" Language="C#" MasterPageFile="~/Site1.Master"
    AutoEventWireup="true" CodeBehind="AdminDiscounts.aspx.cs"
    Inherits="project01.WebForm23" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            background-color: #f4f6f9;
            font-family: 'Segoe UI', sans-serif;
            color: #212529;
        }
        .container {
            max-width: 900px;
            margin: 40px auto;
            background: #fff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.1);
        }
        h2 {
            text-align: center;
            color: #007bff;
            margin-bottom: 25px;
        }
        label {
            font-weight: 600;
            display: block;
            margin-top: 10px;
        }
        input, textarea {
            width: 100%;
            padding: 8px;
            border-radius: 6px;
            border: 1px solid #ccc;
            margin-bottom: 10px;
        }
        .btn {
            background: linear-gradient(90deg,#0072ff,#00c6ff);
            color: white;
            border: none;
            padding: 8px 16px;
            border-radius: 6px;
            cursor: pointer;
            margin: 5px;
            transition: 0.3s;
        }
        .btn:hover { transform: scale(1.05); }
        .message { text-align: center; color: #28a745; font-weight: bold; margin-top: 15px; }
        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            font-size: 14px;
        }
        .table th, .table td {
            border: 1px solid #dee2e6;
            padding: 10px;
            text-align: center;
        }
        .table th {
            background: #007bff;
            color: white;
        }
        .expired { color: #dc3545; font-weight: bold; }
        .active { color: #28a745; font-weight: bold; }
    </style>

    <div class="container">
        <h2>🎟 Manage Coupons / Promo Codes</h2>

        <!-- Add/Edit Discount Form -->
        <asp:HiddenField ID="hfDiscountID" runat="server" />

        <label>Coupon / Promo Code</label>
        <asp:TextBox ID="txtCode" runat="server" placeholder="E.g. FLY2025" />

        <label>Description</label>
        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="2" placeholder="E.g. 10% off on domestic flights"></asp:TextBox>

        <label>Discount Percentage (%)</label>
        <asp:TextBox ID="txtPercentage" runat="server" placeholder="E.g. 10"></asp:TextBox>

        <label>Valid Till</label>
        <asp:TextBox ID="txtValidTill" runat="server" TextMode="Date"></asp:TextBox>

        <div style="text-align:center;">
            <asp:Button ID="btnSave" runat="server" Text="💾 Save" CssClass="btn" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="🧹 Clear" CssClass="btn" OnClick="btnClear_Click" />
        </div>

        <hr />

        <!-- Coupon Table -->
        <asp:GridView ID="gvDiscounts" runat="server" CssClass="table" AutoGenerateColumns="False" DataKeyNames="DiscountID"
            OnRowEditing="gvDiscounts_RowEditing"
            OnRowUpdating="gvDiscounts_RowUpdating"
            OnRowCancelingEdit="gvDiscounts_RowCancelingEdit"
            OnRowDeleting="gvDiscounts_RowDeleting">
            <Columns>
                <asp:BoundField DataField="DiscountID" HeaderText="ID" ReadOnly="true" />
                <asp:TemplateField HeaderText="Code">
                    <ItemTemplate><%# Eval("Code") %></ItemTemplate>
                    <EditItemTemplate><asp:TextBox ID="txtEditCode" runat="server" Text='<%# Bind("Code") %>' /></EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description">
                    <ItemTemplate><%# Eval("Description") %></ItemTemplate>
                    <EditItemTemplate><asp:TextBox ID="txtEditDescription" runat="server" Text='<%# Bind("Description") %>' /></EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Discount (%)">
                    <ItemTemplate><%# Eval("Percentage") %></ItemTemplate>
                    <EditItemTemplate><asp:TextBox ID="txtEditPercentage" runat="server" Text='<%# Bind("Percentage") %>' /></EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Valid Till">
                    <ItemTemplate><%# Eval("ValidTill", "{0:dd-MMM-yyyy}") %></ItemTemplate>
                    <EditItemTemplate><asp:TextBox ID="txtEditValidTill" runat="server" Text='<%# Bind("ValidTill", "{0:yyyy-MM-dd}") %>' TextMode="Date" /></EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <%# Convert.ToDateTime(Eval("ValidTill")) < DateTime.Now 
                            ? "<span class='expired'>Expired</span>" 
                            : "<span class='active'>Active</span>" %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:CommandField ShowEditButton="True" EditText="✏ Edit" UpdateText="💾 Save" CancelText="❌ Cancel" />
                <asp:CommandField ShowDeleteButton="True" DeleteText="🗑 Delete" />
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
    </div>
</asp:Content>
