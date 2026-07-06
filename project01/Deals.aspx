<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Deals.aspx.cs" Inherits="project01.WebForm28" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container my-5">
        <h2 class="text-center mb-4">🔥 Exclusive Hot Deals</h2>
        
        <div class="row">
            <asp:Repeater ID="rptDeals" runat="server">
                <ItemTemplate>
                    <div class="col-md-4 col-sm-6 mb-4">
                        <div class="card deal-card h-100 shadow-sm">
                            <div class="image-container">
                                <img src='<%# ResolveUrl(Eval("ImageUrl").ToString()) %>' class="card-img-top" alt="Deal Image">
                                <span class="badge bg-danger price-badge">
                                    Only ₹<%# Eval("Price") %>
                                </span>
                            </div>

                            <div class="card-body d-flex flex-column">
                                <h5 class="card-title text-primary">
                                    <i class="fas fa-plane"></i> <%# Eval("Route") %>
                                </h5>
                                
                                <p class="card-text text-muted">
                                    <i class="far fa-calendar-alt"></i> <%# Eval("TravelDates") %>
                                </p>
                                
                                <div class="mt-auto">
                                    <a href="Flights.aspx?DealID=<%# Eval("DealID") %>" class="btn btn-outline-primary w-100">
                                        Book This Deal
                                    </a>
                                </div>
                            </div>
                            
                            <div class="card-footer text-muted small">
                                Posted: <%# Eval("CreatedAt", "{0:MMM dd, yyyy}") %>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Label ID="lblNoDeals" runat="server" Text="No hot deals available right now." Visible="false" CssClass="alert alert-warning text-center w-100 d-block"></asp:Label>
    </div>

    <style>
        .deal-card {
            transition: transform 0.3s ease, box-shadow 0.3s ease;
            border: none;
            overflow: hidden;
        }
        .deal-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.15) !important;
        }
        .image-container {
            position: relative;
            height: 200px; /* Fixed height for uniformity */
            overflow: hidden;
        }
        .image-container img {
            width: 100%;
            height: 100%;
            object-fit: cover; /* Ensures image fills box without stretching */
        }
        .price-badge {
            position: absolute;
            top: 10px;
            right: 10px;
            font-size: 1rem;
            padding: 8px 12px;
            border-radius: 20px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        }
    </style>

</asp:Content>