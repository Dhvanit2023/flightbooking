<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AdminTravelNews.aspx.cs" Inherits="project01.WebForm25" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<style>
.container { max-width: 900px; margin: 40px auto; background:#fff; padding:25px; border-radius:10px; box-shadow:0 4px 10px rgba(0,0,0,0.1);}
h2{text-align:center;color:#007bff;margin-bottom:20px;}
.table th{background:#007bff;color:white;}
</style>

<div class="container">
    <h2>📰 Manage Travel News & Notes</h2>
    <asp:HiddenField ID="hfNewsID" runat="server" />

    <label>Title</label>
    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" Placeholder="E.g. New Airline Routes"></asp:TextBox>

    <label>Description</label>
    <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" Placeholder="Write a short description..."></asp:TextBox>

    <label>Link (optional)</label>
    <asp:TextBox ID="txtLink" runat="server" CssClass="form-control" Placeholder="https://example.com"></asp:TextBox>

    <div style="text-align:center;margin-top:15px;">
        <asp:Button ID="btnSave" runat="server" Text="💾 Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="🧹 Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
    </div>

    <hr />

    <asp:GridView ID="gvNews" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered" DataKeyNames="NewsID"
        OnRowEditing="gvNews_RowEditing" OnRowDeleting="gvNews_RowDeleting"
        OnRowCancelingEdit="gvNews_RowCancelingEdit" OnRowUpdating="gvNews_RowUpdating">
        <Columns>
            <asp:BoundField DataField="NewsID" HeaderText="ID" ReadOnly="true" />
            <asp:TemplateField HeaderText="Title">
                <ItemTemplate><%# Eval("Title") %></ItemTemplate>
                <EditItemTemplate><asp:TextBox ID="txtEditTitle" runat="server" Text='<%# Bind("Title") %>' /></EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Description">
                <ItemTemplate><%# Eval("Description") %></ItemTemplate>
                <EditItemTemplate><asp:TextBox ID="txtEditDescription" runat="server" Text='<%# Bind("Description") %>' /></EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Link">
                <ItemTemplate>
                    <a href='<%# Eval("Link") %>' target="_blank" class="text-primary"><%# Eval("Link") %></a>
                </ItemTemplate>
                <EditItemTemplate><asp:TextBox ID="txtEditLink" runat="server" Text='<%# Bind("Link") %>' /></EditItemTemplate>
            </asp:TemplateField>
            <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
        </Columns>
    </asp:GridView>

    <asp:Label ID="lblMsg" runat="server" CssClass="text-success fw-bold"></asp:Label>
</div>
</asp:Content>
