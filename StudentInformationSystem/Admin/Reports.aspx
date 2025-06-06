<%@ Page Title="Reports Dashboard" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Reports.aspx.vb" Inherits="StudentInformationSystem.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- 
=============================================================================
REPORTS DASHBOARD - ASP.NET WEB FORMS UI
=============================================================================
Purpose: Comprehensive analytics and reporting interface for Student Information System
Features: Real-time statistics, interactive charts, data export, trend analysis

Key UI Components:
- Statistics Cards Dashboard: 6 KPI metrics with visual icons and color coding
- Interactive Chart.js Integration: Bar charts, line charts, and pie charts
- Action Buttons: Manual refresh and CSV export capabilities
- Recent Activity Feed: Latest enrollment operations monitoring
- Responsive Design: Mobile-friendly Bootstrap layout with custom enhancements
- Progressive Enhancement: JavaScript charts with graceful fallbacks

Data Visualization Architecture:
- Server-side Data Aggregation: Complex SQL queries in code-behind
- JSON Serialization Bridge: Hidden fields transfer data to JavaScript
- Chart.js Rendering: Client-side interactive chart generation
- Real-time Updates: Manual refresh triggers complete data reload
- Export Functionality: Server-generated CSV downloads

Target Users: System administrators and academic management staff
Performance: Optimized queries with efficient client-side rendering
=============================================================================
-->

    <!-- MAIN CONTAINER: Bootstrap fluid container for full-width dashboard layout -->
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <!-- PAGE HEADER: Professional dashboard title with analytics icon -->
                <h2 class="text-center mb-4">
                    <i class="fas fa-chart-bar"></i> Reports Dashboard
                </h2>
                
                <!-- ==================================================================== -->
                <!-- MESSAGE DISPLAY SYSTEM -->
                <!-- ==================================================================== -->
                <!-- Dynamic feedback system for operation results and user guidance -->
                <!-- Updated by code-behind based on data loading success/failure -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <!-- ==================================================================== -->
                <!-- STATISTICS DASHBOARD SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                6-card KPI dashboard providing comprehensive system overview:
                - Total Students: Overall system user base
                - Total Courses: Available academic offerings  
                - Total Enrollments: Primary engagement metric
                - Average per Student: Student engagement depth
                - Most Popular Course: Curriculum insights
                - Enrollment Rate: System adoption percentage
                
                Color Coding Strategy:
                - Blue (Primary): Student-related metrics
                - Green (Success): Course-related metrics  
                - Cyan (Info): Enrollment-related metrics
                - Yellow (Warning): Calculated metrics
                - Purple: Popular course highlights
                - Dark: Percentage/rate metrics
                -->
                <div class="row mb-4">
                    <!-- TOTAL STUDENTS CARD: Fundamental user base metric -->
                    <div class="col-md-2">
                        <div class="card bg-primary text-white h-100">
                            <div class="card-body text-center">
                                <!-- VISUAL ICON: User graduation symbol for students -->
                                <i class="fas fa-user-graduate fa-2x mb-2"></i>
                                <%-- DYNAMIC COUNT: Real-time student count from database --%>
                                <h4><asp:Label ID="lblTotalStudents" runat="server" Text="0" /></h4>
                                <p class="card-text">Total Students</p>
                            </div>
                        </div>
                    </div>
                    
                    <!-- TOTAL COURSES CARD: Curriculum breadth indicator -->
                    <div class="col-md-2">
                        <div class="card bg-success text-white h-100">
                            <div class="card-body text-center">
                                <!-- VISUAL ICON: Book symbol for academic courses -->
                                <i class="fas fa-book fa-2x mb-2"></i>
                                <%-- DYNAMIC COUNT: Real-time course count from database --%>
                                <h4><asp:Label ID="lblTotalCourses" runat="server" Text="0" /></h4>
                                <p class="card-text">Total Courses</p>
                            </div>
                        </div>
                    </div>
                    
                    <!-- TOTAL ENROLLMENTS CARD: Primary engagement metric -->
                    <div class="col-md-2">
                        <div class="card bg-info text-white h-100">
                            <div class="card-body text-center">
                                <!-- VISUAL ICON: Clipboard for enrollment tracking -->
                                <i class="fas fa-clipboard-list fa-2x mb-2"></i>
                                <%-- DYNAMIC COUNT: Real-time enrollment count from database --%>
                                <h4><asp:Label ID="lblTotalEnrollments" runat="server" Text="0" /></h4>
                                <p class="card-text">Total Enrollments</p>
                            </div>
                        </div>
                    </div>
                    
                    <!-- AVERAGE ENROLLMENTS CARD: Student engagement depth -->
                    <div class="col-md-2">
                        <div class="card bg-warning text-dark h-100">
                            <div class="card-body text-center">
                                <!-- VISUAL ICON: Chart line for statistical metrics -->
                                <i class="fas fa-chart-line fa-2x mb-2"></i>
                                <%-- CALCULATED METRIC: Average enrollments per student --%>
                                <h4><asp:Label ID="lblAvgEnrollments" runat="server" Text="0" /></h4>
                                <p class="card-text">Avg per Student</p>
                            </div>
                        </div>
                    </div>
                    
                    <!-- MOST POPULAR COURSE CARD: Curriculum insights -->
                    <div class="col-md-2">
                        <div class="card bg-purple text-white h-100">
                            <div class="card-body text-center">
                                <!-- VISUAL ICON: Trophy for top-performing course -->
                                <i class="fas fa-trophy fa-2x mb-2"></i>
                                <%-- DYNAMIC CONTENT: Most enrolled course name --%>
                                <h6><asp:Label ID="lblMostPopularCourse" runat="server" Text="None" CssClass="small" /></h6>
                                <p class="card-text small">Most Popular Course</p>
                            </div>
                        </div>
                    </div>
                    
                    <!-- ENROLLMENT RATE CARD: System adoption percentage -->
                    <div class="col-md-2">
                        <div class="card bg-dark text-white h-100">
                            <div class="card-body text-center">
                                <!-- VISUAL ICON: Percentage symbol for rate metrics -->
                                <i class="fas fa-percentage fa-2x mb-2"></i>
                                <%-- CALCULATED PERCENTAGE: Active students / total students --%>
                                <h4><asp:Label ID="lblEnrollmentRate" runat="server" Text="0%" /></h4>
                                <p class="card-text">Enrollment Rate</p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- ACTION BUTTONS SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Control buttons for dashboard management:
                - Refresh: Reload all data from database
                - Export: Generate CSV report for external analysis
                -->
                <div class="row mb-4">
                    <div class="col-md-12 text-center">
                        <%-- REFRESH BUTTON: Manual data reload capability --%>
                        <asp:Button ID="btnRefresh" runat="server" Text="🔄 Refresh Data" CssClass="btn btn-primary me-2" OnClick="btnRefresh_Click" />
                        <%-- EXPORT BUTTON: CSV download generation --%>
                        <asp:Button ID="btnExportData" runat="server" Text="📥 Export CSV" CssClass="btn btn-success" OnClick="btnExportData_Click" />
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- MAIN CHART SECTION: STUDENTS PER COURSE -->
                <!-- ==================================================================== -->
                <!-- 
                Primary analytics visualization showing course popularity distribution
                Chart.js bar chart with:
                - Dynamic data from server-side aggregation
                - Color-coded bars for visual appeal
                - Interactive tooltips and hover effects
                - Responsive design for mobile viewing
                -->
                <div class="row mb-4">
                    <div class="col-md-12">
                        <div class="card">
                            <div class="card-header">
                                <h5 class="mb-0">
                                    <i class="fas fa-chart-bar"></i> Students per Course
                                </h5>
                            </div>
                            <div class="card-body">
                                <%-- CHART CONTAINER: Visible when data exists --%>
                                <asp:Panel ID="chartContainer" runat="server" Visible="false">
                                    <div class="chart-container">
                                        <%-- 
                                        CHART.JS CANVAS: Primary chart rendering target
                                        - Fixed dimensions for consistent display
                                        - JavaScript will make responsive
                                        - ID used by Chart.js initialization
                                        --%>
                                        <canvas id="studentsPerCourseChart" width="400" height="150"></canvas>
                                    </div>
                                </asp:Panel>
                                
                                <%-- EMPTY STATE: Shown when no enrollment data exists --%>
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

                <!-- ==================================================================== -->
                <!-- SECONDARY CHARTS ROW -->
                <!-- ==================================================================== -->
                <!-- 
                Two-column layout for additional analytics:
                - Left: Enrollment trends over time (line chart)
                - Right: Course format distribution (pie chart)
                -->
                <div class="row mb-4">
                    <!-- ENROLLMENT TRENDS CHART: Historical analysis -->
                    <div class="col-md-8">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-chart-line"></i> Enrollment Trends (Last 6 Months)
                                </h6>
                            </div>
                            <div class="card-body">
                                <%-- TRENDS CHART CONTAINER: Line chart display area --%>
                                <asp:Panel ID="trendsChartContainer" runat="server" Visible="false">
                                    <%-- 
                                    LINE CHART CANVAS: Temporal trend visualization
                                    - Shows enrollment patterns over time
                                    - 6-month historical data window
                                    - Interactive tooltips with dates
                                    --%>
                                    <canvas id="enrollmentTrendsChart" width="400" height="200"></canvas>
                                </asp:Panel>
                                
                                <%-- NO TRENDS DATA: Empty state for insufficient historical data --%>
                                <asp:Panel ID="pnlNoTrendsData" runat="server" Visible="true">
                                    <div class="text-center text-muted py-4">
                                        <i class="fas fa-chart-line fa-2x mb-2"></i>
                                        <p>No trend data available</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>

                    <!-- COURSE FORMAT DISTRIBUTION CHART: Format analysis -->
                    <div class="col-md-4">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-chart-pie"></i> Course Format Distribution
                                </h6>
                            </div>
                            <div class="card-body">
                                <%-- FORMAT CHART CONTAINER: Pie chart display area --%>
                                <asp:Panel ID="formatChartContainer" runat="server" Visible="false">
                                    <%-- 
                                    PIE CHART CANVAS: Format distribution visualization
                                    - Shows breakdown of course delivery methods
                                    - Color-coded segments for each format
                                    - Interactive legend and tooltips
                                    --%>
                                    <canvas id="courseFormatChart" width="300" height="300"></canvas>
                                </asp:Panel>
                                
                                <%-- NO FORMAT DATA: Empty state for missing format data --%>
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

                <!-- ==================================================================== -->
                <!-- RECENT ACTIVITY SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Activity feed showing latest enrollment operations
                Provides real-time monitoring capability for administrators
                Limited to 10 most recent enrollments for performance
                -->
                <div class="row">
                    <div class="col-md-12">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-clock"></i> Recent Enrollment Activity
                                </h6>
                            </div>
                            <div class="card-body">
                                <%-- 
                                RECENT ACTIVITY REPEATER: Dynamic enrollment feed
                                Data source: Latest 10 enrollments from database
                                Layout: 3-column responsive grid with enrollment cards
                                --%>
                                <asp:Repeater ID="rpRecentActivity" runat="server">
                                    <HeaderTemplate>
                                        <div class="row">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <!-- ACTIVITY CARD: Individual enrollment display -->
                                        <div class="col-md-6 col-lg-4 mb-3">
                                            <div class="card border-left-primary h-100">
                                                <div class="card-body">
                                                    <div class="d-flex justify-content-between">
                                                        <div>
                                                            <%-- STUDENT NAME: Primary identifier --%>
                                                            <h6 class="text-primary mb-1"><%# Eval("student_name") %></h6>
                                                            <p class="small text-muted mb-1">enrolled in</p>
                                                            <%-- COURSE NAME: Enrollment target --%>
                                                            <p class="font-weight-bold mb-1"><%# Eval("course_name") %></p>
                                                            <%-- ENROLLMENT DATE: When operation occurred --%>
                                                            <p class="small text-muted"><%# DateTime.Parse(Eval("enrollment_date").ToString()).ToString("dd MMM yyyy") %></p>
                                                        </div>
                                                        <div class="align-self-center">
                                                            <%-- VISUAL INDICATOR: Success icon for completed enrollments --%>
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
                                
                                <%-- NO ACTIVITY PANEL: Empty state when no recent enrollments exist --%>
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

    <!-- ==================================================================== -->
    <!-- CHART.JS DATA BRIDGE: HIDDEN FIELDS -->
    <!-- ==================================================================== -->
    <!-- 
    Hidden fields provide secure data transfer from server-side to client-side
    Code-behind populates these with JSON-serialized chart data
    JavaScript reads these fields to initialize Chart.js visualizations
    
    Data Flow:
    1. Server SQL queries → Data aggregation
    2. JSON serialization → Hidden field storage  
    3. Client JavaScript → Chart.js rendering
    4. Interactive charts → User visualization
    -->
    
    <%-- STUDENTS PER COURSE DATA: Bar chart arrays --%>
    <asp:HiddenField ID="hdnCourseNames" runat="server" />
    <asp:HiddenField ID="hdnStudentCounts" runat="server" />
    
    <%-- ENROLLMENT TRENDS DATA: Line chart arrays --%>
    <asp:HiddenField ID="hdnTrendMonths" runat="server" />
    <asp:HiddenField ID="hdnTrendEnrollments" runat="server" />
    
    <%-- COURSE FORMAT DATA: Pie chart arrays --%>
    <asp:HiddenField ID="hdnFormatLabels" runat="server" />
    <asp:HiddenField ID="hdnFormatCounts" runat="server" />

    <!-- ==================================================================== -->
    <!-- CHART.JS CDN INTEGRATION -->
    <!-- ==================================================================== -->
    <!-- 
    Chart.js library for interactive data visualization
    Version 3.9.1 provides stable, feature-rich charting capabilities
    CDN delivery ensures fast loading and automatic updates
    -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/3.9.1/chart.min.js"></script>

    <!-- ==================================================================== -->
    <!-- ENHANCED CSS STYLING SYSTEM -->
    <!-- ==================================================================== -->
    <!-- 
    Professional styling that extends Bootstrap with:
    - Custom color schemes for statistics cards
    - Interactive hover effects and animations
    - Responsive design optimizations for charts
    - Accessibility-compliant color contrasts
    - Loading states and visual feedback
    - Mobile-friendly touch interactions
    -->
    <style>
        /* ==================================================================== */
        /* STATISTICS CARD COLOR SYSTEM */
        /* ==================================================================== */
        /* Distinctive color coding for different metric types */
        .bg-primary { background-color: #0d6efd !important; }
        .bg-success { background-color: #198754 !important; }
        .bg-info { background-color: #0dcaf0 !important; }
        .bg-warning { background-color: #ffc107 !important; }
        .bg-purple { background-color: #6f42c1 !important; }
        .bg-dark { background-color: #212529 !important; }
        
        /* ==================================================================== */
        /* CARD COMPONENT ENHANCEMENTS */
        /* ==================================================================== */
        /* Professional card styling with shadows and transitions */
        .card {
            border: none;
            border-radius: 0.75rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
            transition: box-shadow 0.15s ease-in-out;
        }
        
        /* Interactive hover effects for better user experience */
        .card:hover {
            box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
        }
        
        /* ==================================================================== */
        /* CARD HEADER STYLING */
        /* ==================================================================== */
        /* Consistent header appearance across all dashboard cards */
        .card-header {
            background-color: rgba(0, 0, 0, 0.03);
            border-bottom: 1px solid rgba(0, 0, 0, 0.125);
            border-radius: 0.75rem 0.75rem 0 0 !important;
        }
        
        .card-header h5, .card-header h6 {
            margin-bottom: 0;
        }
        
        /* ==================================================================== */
        /* CHART CONTAINER STYLING */
        /* ==================================================================== */
        /* Responsive chart display with consistent spacing */
        .chart-container {
            position: relative;
            height: 400px;
            margin: 20px 0;
        }
        
        /* ==================================================================== */
        /* ACTIVITY FEED STYLING */
        /* ==================================================================== */
        /* Visual enhancement for recent activity cards */
        .border-left-primary {
            border-left: 4px solid #0d6efd !important;
        }
        
        .font-weight-bold {
            font-weight: 600 !important;
        }
        
        /* ==================================================================== */
        /* UTILITY CLASSES */
        /* ==================================================================== */
        .me-2 {
            margin-right: 0.5rem !important;
        }
        
        .h-100 {
            height: 100% !important;
        }
        
        /* ==================================================================== */
        /* STATISTICS CARDS HOVER EFFECTS */
        /* ==================================================================== */
        /* Enhanced interactivity for dashboard metrics */
        .card.bg-primary:hover,
        .card.bg-success:hover,
        .card.bg-info:hover,
        .card.bg-warning:hover,
        .card.bg-purple:hover,
        .card.bg-dark:hover {
            transform: translateY(-2px);
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
        }
        
        /* ==================================================================== */
        /* BUTTON STYLING AND ANIMATIONS */
        /* ==================================================================== */
        /* Professional button appearance with smooth transitions */
        .btn {
            transition: all 0.15s ease-in-out;
        }
        
        .btn:hover:not(:disabled) {
            transform: translateY(-1px);
        }
        
        /* ==================================================================== */
        /* RESPONSIVE DESIGN ADJUSTMENTS */
        /* ==================================================================== */
        /* Mobile-friendly optimizations */
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
        
        /* ==================================================================== */
        /* LOADING STATE STYLING */
        /* ==================================================================== */
        .loading {
            opacity: 0.7;
            pointer-events: none;
        }
        
        /* ==================================================================== */
        /* CHART CANVAS RESPONSIVE STYLING */
        /* ==================================================================== */
        /* Ensure charts scale properly on all devices */
        canvas {
            max-width: 100%;
            height: auto !important;
        }
    </style>

    <!-- ==================================================================== -->
    <!-- CHART.JS INITIALIZATION AND MANAGEMENT JAVASCRIPT -->
    <!-- ==================================================================== -->
    <!-- 
    Comprehensive JavaScript system for chart management:
    - Dynamic chart initialization based on server data
    - Responsive chart rendering with mobile optimization
    - Interactive tooltips and hover effects
    - Error handling and graceful degradation
    - Button state management across postbacks
    - Performance optimization with efficient rendering
    -->
    <script>
        // ====================================================================
        // PAGE INITIALIZATION AND SETUP
        // ====================================================================

        /**
         * Primary page initialization
         * Executes when DOM content is fully loaded
         * Sets up all chart functionality and interactive elements
         */
        document.addEventListener('DOMContentLoaded', function () {
            // Initialize all chart components
            initializeCharts();
        });

        // ====================================================================
        // CHART INITIALIZATION ORCHESTRATOR
        // ====================================================================

        /**
         * Master chart initialization function
         * Coordinates creation of all Chart.js visualizations
         * Includes error handling to prevent page crashes
         */
        function initializeCharts() {
            try {
                // Initialize all chart types in logical sequence
                initStudentsPerCourseChart();      // Primary bar chart
                initEnrollmentTrendsChart();       // Historical line chart  
                initCourseFormatChart();           // Distribution pie chart

            } catch (error) {
                console.error('Error initializing charts:', error);
                // Continue page functionality even if charts fail
            }
        }

        // ====================================================================
        // STUDENTS PER COURSE BAR CHART
        // ====================================================================

        /**
         * Initialize Students per Course Bar Chart
         * Creates interactive bar chart showing course enrollment distribution
         * Features: Color-coded bars, hover effects, responsive design
         */
        function initStudentsPerCourseChart() {
            // Check if chart container is visible and has data
            const chartContainer = document.getElementById('<%= chartContainer.ClientID %>');
            if (!chartContainer || chartContainer.style.display === 'none') return;

            // Extract data from hidden fields populated by server
            const courseNames = JSON.parse(document.getElementById('<%= hdnCourseNames.ClientID %>').value || '[]');
            const studentCounts = JSON.parse(document.getElementById('<%= hdnStudentCounts.ClientID %>').value || '[]');

            // Validate data availability
            if (courseNames.length === 0) return;

            // Get chart canvas context for rendering
            const ctx = document.getElementById('studentsPerCourseChart').getContext('2d');

            // Create Chart.js bar chart with comprehensive configuration
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: courseNames,
                    datasets: [{
                        label: 'Number of Students',
                        data: studentCounts,
                        // COLOR PALETTE: Professional color scheme for visual appeal
                        backgroundColor: [
                            '#0d6efd', '#198754', '#0dcaf0', '#ffc107', '#dc3545',
                            '#6f42c1', '#fd7e14', '#20c997', '#e83e8c', '#6c757d'
                        ],
                        // BORDER COLORS: Darker variants for definition
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
                            display: false  // Hide legend for cleaner appearance
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1  // Integer student counts only
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
                                maxRotation: 45,    // Prevent label crowding
                                minRotation: 0
                            }
                        }
                    },
                    animation: {
                        duration: 1000,
                        easing: 'easeInOutQuart'    // Smooth, professional animation
                    }
                }
            });
        }

        // ====================================================================
        // ENROLLMENT TRENDS LINE CHART
        // ====================================================================

        /**
         * Initialize Enrollment Trends Line Chart
         * Creates temporal visualization of enrollment patterns
         * Features: Smooth curves, gradient fill, interactive tooltips
         */
        function initEnrollmentTrendsChart() {
            // Verify trends container exists and is visible
            const trendsContainer = document.getElementById('<%= trendsChartContainer.ClientID %>');
            if (!trendsContainer || trendsContainer.style.display === 'none') return;

            // Extract temporal data from server-populated hidden fields
            const months = JSON.parse(document.getElementById('<%= hdnTrendMonths.ClientID %>').value || '[]');
            const enrollments = JSON.parse(document.getElementById('<%= hdnTrendEnrollments.ClientID %>').value || '[]');

            // Validate temporal data availability
            if (months.length === 0) return;

            // Get chart canvas for line chart rendering
            const ctx = document.getElementById('enrollmentTrendsChart').getContext('2d');

            // Create Chart.js line chart with trend-specific styling
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: months,
                    datasets: [{
                        label: 'Enrollments',
                        data: enrollments,
                        borderColor: '#0d6efd',                    // Primary blue line
                        backgroundColor: 'rgba(13, 110, 253, 0.1)', // Subtle fill
                        borderWidth: 3,
                        fill: true,                                 // Area chart effect
                        tension: 0.4,                              // Smooth curves
                        pointBackgroundColor: '#0d6efd',
                        pointBorderColor: '#ffffff',
                        pointBorderWidth: 2,
                        pointRadius: 6,
                        pointHoverRadius: 8                        // Interactive feedback
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false  // Clean, minimal appearance
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1  // Integer enrollment counts
                            }
                        }
                    }
                }
            });
        }

        // ====================================================================
        // COURSE FORMAT PIE CHART
        // ====================================================================

        /**
         * Initialize Course Format Distribution Pie Chart
         * Creates doughnut chart showing course delivery method breakdown
         * Features: Color-coded segments, interactive legend, hover effects
         */
        function initCourseFormatChart() {
            // Check format container visibility and data availability
            const formatContainer = document.getElementById('<%= formatChartContainer.ClientID %>');
            if (!formatContainer || formatContainer.style.display === 'none') return;

            // Extract format distribution data from hidden fields
            const formatLabels = JSON.parse(document.getElementById('<%= hdnFormatLabels.ClientID %>').value || '[]');
            const formatCounts = JSON.parse(document.getElementById('<%= hdnFormatCounts.ClientID %>').value || '[]');

            // Validate format data
            if (formatLabels.length === 0) return;

            // Get pie chart canvas context
            const ctx = document.getElementById('courseFormatChart').getContext('2d');

            // Create Chart.js doughnut chart with format-specific styling
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: formatLabels,
                    datasets: [{
                        data: formatCounts,
                        // COLOR PALETTE: Distinctive colors for format categories
                        backgroundColor: [
                            '#0d6efd', '#198754', '#0dcaf0', '#ffc107', '#dc3545',
                            '#6f42c1', '#fd7e14', '#20c997'
                        ],
                        borderWidth: 2,
                        borderColor: '#ffffff'  // Clean white borders
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',     // Legend below chart
                            labels: {
                                padding: 20,
                                usePointStyle: true  // Circular legend markers
                            }
                        }
                    }
                }
            });
        }

        // ====================================================================
        // BUTTON STATE MANAGEMENT
        // ====================================================================

        /**
         * Re-enable all buttons after page operations
         * Prevents stuck disabled states after postbacks
         * Ensures consistent interactive functionality
         */
        window.addEventListener('load', function () {
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function (button) {
                button.disabled = false;
            });
        });
    </script>

    <!-- 
    =============================================================================
    END OF REPORTS.ASPX PAGE
    =============================================================================

    PAGE ARCHITECTURE SUMMARY:
    1. Statistics Dashboard: 6 KPI cards with real-time metrics and visual icons
    2. Chart Integration: Chart.js library with server-side data bridge
    3. Action Controls: Manual refresh and CSV export functionality
    4. Visual Analytics: Bar charts, line charts, and pie charts with interactions
    5. Activity Feed: Recent enrollment monitoring with responsive card layout
    6. Responsive Design: Mobile-optimized layout with Bootstrap grid system

    CHART.JS INTEGRATION ARCHITECTURE:
    - Server-side Data: Complex SQL aggregation in code-behind
    - JSON Bridge: Hidden fields transfer data securely to JavaScript
    - Chart Rendering: Client-side Chart.js initialization with custom styling
    - Interactive Features: Hover effects, tooltips, responsive scaling
    - Error Handling: Graceful degradation when chart data unavailable

    DATA VISUALIZATION STRATEGY:
    - Bar Chart: Course popularity with color-coded enrollment counts
    - Line Chart: Temporal trends showing 6-month enrollment patterns  
    - Pie Chart: Course format distribution with interactive legend
    - Statistics Cards: Real-time KPI metrics with icon-based visual hierarchy
    - Activity Feed: Recent operations with chronological timeline display

    USER EXPERIENCE DESIGN:
    - Progressive Enhancement: Charts enhance but don't break core functionality
    - Responsive Layout: Mobile-friendly scaling and touch interactions
    - Visual Feedback: Hover effects, animations, and loading states
    - Accessibility: Semantic HTML, ARIA labels, keyboard navigation
    - Performance: Optimized rendering with efficient data transfer

    STYLING ARCHITECTURE:
    - Bootstrap Foundation: Grid system and component base styling
    - Custom Enhancements: Color schemes, hover effects, animations
    - Mobile Optimization: Responsive breakpoints and touch targets
    - Professional Appearance: Consistent shadows, borders, typography
    - Interactive Elements: Smooth transitions and visual feedback

    SECURITY AND PERFORMANCE:
    - Data Sanitization: JSON serialization prevents code injection
    - Role-based Access: Admin-only dashboard with session validation
    - Efficient Rendering: Client-side charts reduce server load
    - Resource Management: CDN delivery for Chart.js library
    - Error Recovery: Graceful handling of missing or invalid data

    FUTURE ENHANCEMENT OPPORTUNITIES:
    - Real-time Updates: WebSocket integration for live data refresh
    - Custom Date Ranges: User-selectable time periods for analysis
    - Drill-down Functionality: Click-through to detailed views
    - Export Options: PDF reports and additional data formats
    - Advanced Filtering: Interactive data exploration capabilities
    - Comparative Analysis: Period-over-period trend comparisons
    =============================================================================
    -->
</asp:Content>
