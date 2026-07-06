<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="project01.WebForm3" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">

        <!-- 🔹 Offers -->
        <div class="offer-ticker bg-primary text-white py-2 px-3 rounded mb-4">
            <marquee behavior="scroll" direction="left" scrollamount="6" onmouseover="this.stop();" onmouseout="this.start();">
                🔥 Mumbai → London ₹25,000 | Delhi → Paris ₹28,500 | Bangalore → Singapore ₹18,500 | Dubai → Tokyo ₹30,000
           
            </marquee>
        </div>

        <!-- 🔹 Hero Section -->
        <div class="hero-section position-relative mb-5 rounded overflow-hidden shadow-sm">
            <div class="hero-slider">
                <img src="Photos/Flight_Photo/flight01.jpg" class="hero-img active" />
                <img src="Photos/Flight_Photo/flight02.jpg" class="hero-img" />
                <img src="Photos/Flight_Photo/flight03.jpg" class="hero-img" />
                <img src="Photos/Flight_Photo/flight05.jpg" class="hero-img" />
                <img src="Photos/Flight_Photo/flight06.jpg" class="hero-img" />
            </div>
            <div class="hero-text position-absolute top-50 start-50 translate-middle text-center text-white">
                <h1 class="display-4 fw-bold">Book Flights, Hotels & Cars Easily!</h1>
                <p class="lead">Exclusive deals, best prices, and instant booking.</p>
                <a href="Flights.aspx" class="btn btn-warning btn-lg mt-3">Start Booking</a>
            </div>
        </div>

        <!-- 🔹 Dynamic Statistics -->
        <div class="row text-center mb-5">
            <div class="col-md-3 mb-3">
                <div class="stat-box bg-light shadow rounded py-4">
                    <h2 class="text-primary">
                        <asp:Label ID="lblTotalFlights" runat="server" Text="0"></asp:Label>+</h2>
                    <p>Total Flights Booked</p>
                </div>
            </div>
            <div class="col-md-3 mb-3">
                <div class="stat-box bg-light shadow rounded py-4">
                    <h2 class="text-success">
                        <asp:Label ID="lblTotalPassengers" runat="server" Text="0"></asp:Label>+</h2>
                    <p>Total Passengers Flown</p>
                </div>
            </div>
            <div class="col-md-3 mb-3">
                <div class="stat-box bg-light shadow rounded py-4">
                    <h2 class="text-warning">
                        <asp:Label ID="lblFamilies" runat="server" Text="0"></asp:Label>+</h2>
                    <p>Families Travelled</p>
                </div>
            </div>
            <div class="col-md-3 mb-3">
                <div class="stat-box bg-light shadow rounded py-4">
                    <h2 class="text-danger">
                        <asp:Label ID="lblHappyCustomers" runat="server" Text="0"></asp:Label>+</h2>
                    <p>Happy Customers</p>
                </div>
            </div>
                <div class="stat-box bg-light shadow rounded py-4">
                    <h2 class="text-danger">
                        <asp:Label ID="Label1" runat="server" Text="0"></asp:Label>+</h2>
                    <p>Visited Customer</p>
                </div>
            
        </div>

        <!-- 🔹 Hot Flight Deals Section -->
        <h3 class="mb-4">🔥 Hot Flight Deals</h3>
        <div class="row g-4">
            <asp:Repeater ID="rptHotDeals" runat="server">
                <ItemTemplate>
                    <div class="col-md-4">
                        <div class="card shadow-sm h-100 hover-card">
                            <img src='<%# Eval("ImageUrl") %>' class="card-img-top" alt="Deal Image">
                            <div class="card-body">
                                <h5 class="card-title"><%# Eval("Route") %></h5>
                                <p class="card-text">Special fare: ₹<%# Eval("Price") %>. Travel dates: <%# Eval("TravelDates") %>.</p>
                                <a href="Flights.aspx" class="btn btn-primary w-100">Book Now</a>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <!-- 🔹 Travel News Section -->
        <h3 class="mt-5 mb-4">📰 Travel News & Notes</h3>
        <div class="row g-4">
            <asp:Repeater ID="rptTravelNews" runat="server">
                <ItemTemplate>
                    <div class="col-md-4">
                        <div class="p-3 bg-light border rounded shadow-sm h-100 hover-news">
                            <h5><%# Eval("Title") %></h5>
                            <p><%# Eval("Description") %></p>
                            <a href='<%# Eval("Link") %>' target="_blank" class="text-primary text-decoration-none">Read More →</a>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <!-- 🔹 Feedback Section -->
        <div class="feedback-section mt-5 p-4 bg-light rounded shadow-sm">
            <h3 class="mb-3 text-center">⭐ Rate & Share Your Experience</h3>

            <div class="text-center mb-3">
                <asp:Label ID="lblAverageRating" runat="server" CssClass="fw-bold fs-5 text-warning"></asp:Label><br />
                <asp:Label ID="lblTotalReviews" runat="server" CssClass="text-muted"></asp:Label>
            </div>

            <asp:Label ID="lblFeedbackMsg" runat="server" CssClass="text-success"></asp:Label>
            <asp:Panel ID="pnlFeedbackForm" runat="server">
                <div class="text-center mb-3 star-rating">
                    <span class="star" data-value="1">&#9733;</span>
                    <span class="star" data-value="2">&#9733;</span>
                    <span class="star" data-value="3">&#9733;</span>
                    <span class="star" data-value="4">&#9733;</span>
                    <span class="star" data-value="5">&#9733;</span>
                </div>

                <asp:HiddenField ID="hfRating" runat="server" Value="0" />

                <div class="mb-3">
                    <asp:TextBox ID="txtFeedback" runat="server" CssClass="form-control"
                        TextMode="MultiLine" Rows="3" Placeholder="Write your feedback..."></asp:TextBox>
                </div>

                <div class="text-center">
                    <asp:Button ID="btnSubmitFeedback" runat="server" Text="Submit Feedback"
                        CssClass="btn btn-primary" OnClick="btnSubmitFeedback_Click" />
                </div>
            </asp:Panel>
        </div>
    </div>

    <!-- 🔹 Styles -->
    <style>
        .hero-section {
            height: 400px;
            position: relative;
            overflow: hidden;
        }

        .hero-img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            position: absolute;
            top: 0;
            left: 0;
            opacity: 0;
            transition: opacity 1.2s ease-in-out;
        }

            .hero-img.active {
                opacity: 1;
            }

        .hover-card:hover {
            transform: scale(1.05);
            transition: all 0.3s ease;
        }

        .hover-news:hover {
            transform: translateY(-5px);
            transition: all 0.3s ease;
        }

        .stat-box:hover {
            transform: scale(1.05);
            transition: all 0.3s ease;
        }

        .star-rating {
            font-size: 2rem;
            cursor: pointer;
        }

        .star {
            color: gray;
            transition: color 0.3s;
            margin: 0 5px;
        }

            .star.selected {
                color: gold;
            }
    </style>

    <!-- 🔹 JS -->
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // Hero slider
            const slides = document.querySelectorAll(".hero-img");
            let current = 0;
            setInterval(() => {
                slides[current].classList.remove("active");
                current = (current + 1) % slides.length;
                slides[current].classList.add("active");
            }, 4000);

            // Rating click
            const stars = document.querySelectorAll('.star');
            const hidden = document.getElementById('<%= hfRating.ClientID %>');
            stars.forEach(star => {
                star.addEventListener('click', function () {
                    const val = this.getAttribute('data-value');
                    hidden.value = val;
                    stars.forEach(s => s.classList.remove('selected'));
                    for (let i = 0; i < val; i++) stars[i].classList.add('selected');
                });
            });

            // Counter Animation
            const counters = document.querySelectorAll(".stat-box h2");
            counters.forEach(counter => {
                const target = +counter.innerText.replace('+', '');
                let count = 0;
                const update = () => {
                    count += Math.ceil(target / 50);
                    counter.innerText = (count >= target ? target : count) + "+";
                    if (count < target) setTimeout(update, 20);
                };
                update();
            });
        });
    </script>
</asp:Content>
