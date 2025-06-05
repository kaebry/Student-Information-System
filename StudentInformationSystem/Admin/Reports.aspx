<%@ Page Title="Reports Dashboard" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Reports.aspx.vb" Inherits="StudentInformationSystem.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <h2 class="text-center mb-4">
                    <i class="fas fa-chart-bar"></i> Reports Dashboard
                </h2>
                
                <!-- Messages -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <!-- Statistics Cards -->
                <div class="row mb-4">
                    <div class="col-md-2">
                        <div class="card bg-primary text-white h-100">
                            <div class="card-body text-center">
                                <i class="fas fa-user-graduate fa-2x mb-2"></i>
                                <h4><asp:Label ID="lblTotalStudents" runat="server" Text="0" /></h4>
                                <p class="card-text">Total Students</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="card bg-success text-white h-100">
                            <div class="card-body text-center">
                                <i class="fas fa-book fa-2x mb-2"></i>
                                <h4><asp:Label ID="lblTotalCourses" runat="server" Text="0" /></h4>
                                <p class="card-text">Total Courses</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="card bg-info text-white h-100">
                            <div class="card-body text-center">
                                <i class="fas fa-clipboard-list fa-2x mb-2"></i>
                                <h4><asp:Label ID="lblTotalEnrollments" runat="server" Text="0" /></h4>
                                <p class="card-text">Total Enrollments</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="card bg-warning text-dark h-100">
                            <div class="card-body text-center">
                                <i class="fas fa-chart-line fa-2x mb-2"></i>
                                <h4><asp:Label ID="lblAvgEnrollments" runat="server" Text="0" /></h4>
                                <p class="card-text">Avg per Student</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="card bg-purple text-white h-100">
                            <div class="card-body text-center">
                                <i class="fas fa-trophy fa-2x mb-2"></i>
                                <h6><asp:Label ID="lblMostPopularCourse" runat="server" Text="None" CssClass="small" /></h6>
                                <p class="card-text small">Most Popular Course</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="card bg-dark text-white h-100">
                            <div class="card-body text-center">
                                <i class="fas fa-percentage fa-2x mb-2"></i>
                                <h4><asp:Label ID="lblEnrollmentRate" runat="server" Text="0%" /></h4>
                                <p class="card-text">Enrollment Rate</p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Action Buttons -->
                <div class="row mb-4">
                    <div class="col-md-12 text-center">
                        <asp:Button ID="btnRefresh" runat="server" Text="🔄 Refresh Data" CssClass="btn btn-primary me-2" OnClick="btnRefresh_Click" />
                        <asp:Button ID="btnExportData" runat="server" Text="📥 Export CSV" CssClass="btn btn-success" OnClick="btnExportData_Click" />
                    </div>
                </div>

                <!-- Charts Row -->
                <div class="row mb-4">
                    <!-- Students per Course Chart -->
                    <div class="col-md-12">
                        <div class="card">
                            <div class="card-header">
                                <h5 class="mb-0">
                                    <i class="fas fa-chart-bar"></i> Students per Course
                                </h5>
                            </div>
                            <div class="card-body">
                                <asp:Panel ID="chartContainer" runat="server" Visible="false">
                                    <div class="chart-container">
                                        <canvas id="studentsPerCourseChart" width="400" height="150"></canvas>
                                    </div>
                                </asp:Panel>
                                <asp:Panel ID="noDataMessage" runat="server" Visible="true">
                                    <div class="text-center text-muted py-5">
                                        <i class="fas fa-chart-bar fa-3x mb-3"></i>
                                        <h5>No enrollment data available</h5>
                                        <p>Once students start enrolling in courses, the chart will appear here.</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Additional Charts Row -->
                <div class="row mb-4">
                    <!-- Enrollment Trends -->
                    <div class="col-md-8">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-chart-line"></i> Enrollment Trends (Last 6 Months)
                                </h6>
                            </div>
                            <div class="card-body">
                                <asp:Panel ID="trendsChartContainer" runat="server" Visible="false">
                                    <canvas id="enrollmentTrendsChart" width="400" height="200"></canvas>
                                </asp:Panel>
                                <asp:Panel ID="pnlNoTrendsData" runat="server" Visible="true">
                                    <div class="text-center text-muted py-4">
                                        <i class="fas fa-chart-line fa-2x mb-2"></i>
                                        <p>No trend data available</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>

                    <!-- Course Format Distribution -->
                    <div class="col-md-4">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-chart-pie"></i> Course Format Distribution
                                </h6>
                            </div>
                            <div class="card-body">
                                <asp:Panel ID="formatChartContainer" runat="server" Visible="false">
                                    <canvas id="courseFormatChart" width="300" height="300"></canvas>
                                </asp:Panel>
                                <asp:Panel ID="pnlNoFormatData" runat="server" Visible="true">
                                    <div class="text-center text-muted py-4">
                                        <i class="fas fa-chart-pie fa-2x mb-2"></i>
                                        <p>No format data</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Recent Activity -->
                <div class="row">
                    <div class="col-md-12">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-clock"></i> Recent Enrollment Activity
                                </h6>
                            </div>
                            <div class="card-body">
                                <asp:Repeater ID="rpRecentActivity" runat="server">
                                    <HeaderTemplate>
                                        <div class="row">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="col-md-6 col-lg-4 mb-3">
                                            <div class="card border-left-primary h-100">
                                                <div class="card-body">
                                                    <div class="d-flex justify-content-between">
                                                        <div>
                                                            <h6 class="text-primary mb-1"><%# Eval("student_name") %></h6>
                                                            <p class="small text-muted mb-1">enrolled in</p>
                                                            <p class="font-weight-bold mb-1"><%# Eval("course_name") %></p>
                                                            <p class="small text-muted"><%# DateTime.Parse(Eval("enrollment_date").ToString()).ToString("dd MMM yyyy") %></p>
                                                        </div>
                                                        <div class="align-self-center">
                                                            <i class="fas fa-user-check text-success fa-lg"></i>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </div>
                                    </FooterTemplate>
                                </asp:Repeater>
                                
                                <asp:Panel ID="pnlNoActivity" runat="server" Visible="false">
                                    <div class="text-center text-muted py-4">
                                        <i class="fas fa-info-circle fa-2x mb-2"></i>
                                        <h6>No recent enrollment activity</h6>
                                        <p>Recent enrollments will appear here.</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Hidden fields for Chart.js data -->
    <asp:HiddenField ID="hdnCourseNames" runat="server" />
    <asp:HiddenField ID="hdnStudentCounts" runat="server" />
    <asp:HiddenField ID="hdnTrendMonths" runat="server" />
    <asp:HiddenField ID="hdnTrendEnrollments" runat="server" />
    <asp:HiddenField ID="hdnFormatLabels" runat="server" />
    <asp:HiddenField ID="hdnFormatCounts" runat="server" />

    <!-- Chart.js CDN -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/3.9.1/chart.min.js"></script>

    <style>
        .bg-primary { background-color: #0d6efd !important; }
        .bg-success { background-color: #198754 !important; }
        .bg-info { background-color: #0dcaf0 !important; }
        .bg-warning { background-color: #ffc107 !important; }
        .bg-purple { background-color: #6f42c1 !important; }
        .bg-dark { background-color: #212529 !important; }
        
        .card {
            border: none;
            border-radius: 0.75rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
            transition: box-shadow 0.15s ease-in-out;
        }
        
        .card:hover {
            box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
        }
        
        .card-header {
            background-color: rgba(0, 0, 0, 0.03);
            border-bottom: 1px solid rgba(0, 0, 0, 0.125);
            border-radius: 0.75rem 0.75rem 0 0 !important;
        }
        
        .card-header h5, .card-header h6 {
            margin-bottom: 0;
        }
        
        .chart-container {
            position: relative;
            height: 400px;
            margin: 20px 0;
        }
        
        .border-left-primary {
            border-left: 4px solid #0d6efd !important;
        }
        
        .font-weight-bold {
            font-weight: 600 !important;
        }
        
        .me-2 {
            margin-right: 0.5rem !important;
        }
        
        .h-100 {
            height: 100% !important;
        }
        
        /* Statistics cards hover effect */
        .card.bg-primary:hover,
        .card.bg-success:hover,
        .card.bg-info:hover,
        .card.bg-warning:hover,
        .card.bg-purple:hover,
        .card.bg-dark:hover {
            transform: translateY(-2px);
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
        }
        
        /* Button styles */
        .btn {
            transition: all 0.15s ease-in-out;
        }
        
        .btn:hover:not(:disabled) {
            transform: translateY(-1px);
        }
        
        /* Responsive adjustments */
        @media (max-width: 768px) {
            .chart-container {
                height: 300px;
            }
            
            .card-body h4 {
                font-size: 1.5rem;
            }
            
            .card-body .fa-2x {
                font-size: 1.5em;
            }
        }
        
        /* Loading state */
        .loading {
            opacity: 0.7;
            pointer-events: none;
        }
        
        /* Chart canvas responsive */
        canvas {
            max-width: 100%;
            height: auto !important;
        }
    </style>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            // Initialize charts when page loads
            initializeCharts();
        });

        function initializeCharts() {
            try {
                // Students per Course Chart
                initStudentsPerCourseChart();

                // Enrollment Trends Chart
                initEnrollmentTrendsChart();

                // Course Format Distribution Chart
                initCourseFormatChart();

            } catch (error) {
                console.error('Error initializing charts:', error);
            }
        }

        function initStudentsPerCourseChart() {
            const chartContainer = document.getElementById('<%= chartContainer.ClientID %>');
            if (!chartContainer || chartContainer.style.display === 'none') return;

            const courseNames = JSON.parse(document.getElementById('<%= hdnCourseNames.ClientID %>').value || '[]');
            const studentCounts = JSON.parse(document.getElementById('<%= hdnStudentCounts.ClientID %>').value || '[]');

            if (courseNames.length === 0) return;

            const ctx = document.getElementById('studentsPerCourseChart').getContext('2d');
            
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: courseNames,
                    datasets: [{
                        label: 'Number of Students',
                        data: studentCounts,
                        backgroundColor: [
                            '#0d6efd', '#198754', '#0dcaf0', '#ffc107', '#dc3545', 
                            '#6f42c1', '#fd7e14', '#20c997', '#e83e8c', '#6c757d'
                        ],
                        borderColor: [
                            '#0a58ca', '#146c43', '#0aa2c0', '#cc9a06', '#b02a37',
                            '#59359a', '#ca6510', '#1a9a7b', '#c2185b', '#565e64'
                        ],
                        borderWidth: 2,
                        borderRadius: 5,
                        borderSkipped: false,
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        title: {
                            display: true,
                            text: 'Student Enrollment by Course',
                            font: {
                                size: 16,
                                weight: 'bold'
                            }
                        },
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            },
                            title: {
                                display: true,
                                text: 'Number of Students'
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Courses'
                            },
                            ticks: {
                                maxRotation: 45,
                                minRotation: 0
                            }
                        }
                    },
                    animation: {
                        duration: 1000,
                        easing: 'easeInOutQuart'
                    }
                }
            });
        }

        function initEnrollmentTrendsChart() {
            const trendsContainer = document.getElementById('<%= trendsChartContainer.ClientID %>');
            if (!trendsContainer || trendsContainer.style.display === 'none') return;

            const months = JSON.parse(document.getElementById('<%= hdnTrendMonths.ClientID %>').value || '[]');
            const enrollments = JSON.parse(document.getElementById('<%= hdnTrendEnrollments.ClientID %>').value || '[]');

            if (months.length === 0) return;

            const ctx = document.getElementById('enrollmentTrendsChart').getContext('2d');
            
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: months,
                    datasets: [{
                        label: 'Enrollments',
                        data: enrollments,
                        borderColor: '#0d6efd',
                        backgroundColor: 'rgba(13, 110, 253, 0.1)',
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointBackgroundColor: '#0d6efd',
                        pointBorderColor: '#ffffff',
                        pointBorderWidth: 2,
                        pointRadius: 6,
                        pointHoverRadius: 8
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    }
                }
            });
        }

        function initCourseFormatChart() {
            const formatContainer = document.getElementById('<%= formatChartContainer.ClientID %>');
            if (!formatContainer || formatContainer.style.display === 'none') return;

            const formatLabels = JSON.parse(document.getElementById('<%= hdnFormatLabels.ClientID %>').value || '[]');
            const formatCounts = JSON.parse(document.getElementById('<%= hdnFormatCounts.ClientID %>').value || '[]');

            if (formatLabels.length === 0) return;

            const ctx = document.getElementById('courseFormatChart').getContext('2d');

            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: formatLabels,
                    datasets: [{
                        data: formatCounts,
                        backgroundColor: [
                            '#0d6efd', '#198754', '#0dcaf0', '#ffc107', '#dc3545',
                            '#6f42c1', '#fd7e14', '#20c997'
                        ],
                        borderWidth: 2,
                        borderColor: '#ffffff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                padding: 20,
                                usePointStyle: true
                            }
                        }
                    }
                }
            });
        }

        // Re-enable buttons on page load
        window.addEventListener('load', function () {
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function (button) {
                button.disabled = false;
            });
        });
    </script>
</asp:Content>
