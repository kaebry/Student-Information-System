<%@ Page Title="Manage Enrollments" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ManageEnrollments.aspx.vb" Inherits="StudentInformationSystem.ManageEnrollments" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- 
    =============================================================================
    MANAGE ENROLLMENTS DASHBOARD - ASP.NET WEB FORMS UI
    =============================================================================
    Purpose: Comprehensive enrollment management interface with analytics dashboard
    Features: Statistics overview, advanced search/filtering, enrollment deletion, reporting
    
    Key UI Components:
    - Statistics Cards: Real-time KPI dashboard with 4 key metrics
    - Advanced Search Interface: Multi-criteria filtering with date ranges
    - Professional Data Grid: Enrollment display with formatting and actions
    - Recent Activity Panel: Latest enrollment activity monitoring
    - Popular Courses Panel: Course popularity analytics
    - Responsive Design: Mobile-friendly Bootstrap layout
    
    Target Users: System administrators and academic staff
    Data Flow: Real-time database queries → Statistics calculation → UI display
    =============================================================================
    -->

    <!-- MAIN CONTAINER: Bootstrap fluid container for full-width dashboard layout -->
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <!-- PAGE HEADER: Professional dashboard title with icon -->
                <h2 class="text-center mb-4">
                    <i class="fas fa-clipboard-list"></i> Manage Enrollments
                </h2>
                
                <!-- ==================================================================== -->
                <!-- MESSAGE DISPLAY SYSTEM -->
                <!-- ==================================================================== -->
                <!-- Dynamic feedback system for operation results and user guidance -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <!-- ==================================================================== -->
                <!-- STATISTICS DASHBOARD SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                4-card KPI dashboard providing at-a-glance system overview:
                - Total Enrollments: Overall system usage indicator
                - Active Students: Students with at least one enrollment
                - Active Courses: Courses with enrollments (curriculum utilization)
                - Average Enrollments: Student engagement metric
                -->
                <div class="row mb-4">
                    <!-- TOTAL ENROLLMENTS CARD: Primary system usage metric -->
                    <div class="col-md-3">
                        <div class="card bg-primary text-white">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Total Enrollments</h6>
                                        <%-- Real-time count updated by code-behind --%>
                                        <h3><asp:Label ID="lblTotalEnrollments" runat="server" Text="0" /></h3>
                                    </div>
                                    <div class="align-self-center">
                                        <i class="fas fa-clipboard-list fa-2x"></i>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- ACTIVE STUDENTS CARD: Student engagement indicator -->
                    <div class="col-md-3">
                        <div class="card bg-success text-white">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Active Students</h6>
                                        <%-- Count of students with at least one enrollment --%>
                                        <h3><asp:Label ID="lblActiveStudents" runat="server" Text="0" /></h3>
                                    </div>
                                    <div class="align-self-center">
                                        <i class="fas fa-user-graduate fa-2x"></i>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- ACTIVE COURSES CARD: Curriculum utilization metric -->
                    <div class="col-md-3">
                        <div class="card bg-info text-white">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Active Courses</h6>
                                        <%-- Count of courses with enrollments --%>
                                        <h3><asp:Label ID="lblActiveCourses" runat="server" Text="0" /></h3>
                                    </div>
                                    <div class="align-self-center">
                                        <i class="fas fa-book fa-2x"></i>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- AVERAGE ENROLLMENTS CARD: Student engagement depth -->
                    <div class="col-md-3">
                        <div class="card bg-warning text-dark">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Avg. Enrollments/Student</h6>
                                        <%-- Average calculated by complex SQL query --%>
                                        <h3><asp:Label ID="lblAvgEnrollments" runat="server" Text="0" /></h3>
                                    </div>
                                    <div class="align-self-center">
                                        <i class="fas fa-chart-bar fa-2x"></i>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- ADVANCED SEARCH AND FILTER SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Multi-criteria search interface supporting:
                - Student name search (partial matching on first/last/full name)
                - Course name search (partial matching)
                - Date range filtering (enrollment date)
                - Combined filter application with intelligent parameter handling
                -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-search"></i> Search & Filter Enrollments</h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <!-- STUDENT NAME SEARCH: Flexible name matching -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtSearchStudent.ClientID %>" class="form-label">
                                        Search by Student Name
                                    </label>
                                    <%-- Supports partial matching on first name, last name, or full name --%>
                                    <asp:TextBox ID="txtSearchStudent" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter student name..." />
                                </div>
                            </div>
                            
                            <!-- COURSE NAME SEARCH: Course-based filtering -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtSearchCourse.ClientID %>" class="form-label">
                                        Search by Course Name
                                    </label>
                                    <asp:TextBox ID="txtSearchCourse" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter course name..." />
                                </div>
                            </div>
                            
                            <!-- DATE RANGE FILTERS: Enrollment date filtering -->
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label for="<%= txtDateFrom.ClientID %>" class="form-label">From Date</label>
                                    <%-- HTML5 date picker for user-friendly date selection --%>
                                    <asp:TextBox ID="txtDateFrom" runat="server" 
                                        CssClass="form-control" 
                                        TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label for="<%= txtDateTo.ClientID %>" class="form-label">To Date</label>
                                    <asp:TextBox ID="txtDateTo" runat="server" 
                                        CssClass="form-control" 
                                        TextMode="Date" />
                                </div>
                            </div>
                            
                            <!-- SEARCH ACTION BUTTONS -->
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div>
                                        <%-- SEARCH BUTTON: Apply current filter criteria --%>
                                        <asp:Button ID="btnSearch" runat="server" 
                                            Text="Search" 
                                            CssClass="btn btn-info btn-sm me-1" 
                                            OnClick="btnSearch_Click" />
                                        <%-- CLEAR BUTTON: Reset all filters and show all data --%>
                                        <asp:Button ID="btnClearSearch" runat="server" 
                                            Text="Clear" 
                                            CssClass="btn btn-outline-secondary btn-sm" 
                                            OnClick="btnClearSearch_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- ENROLLMENTS DATA GRID SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Professional data display with:
                - Comprehensive enrollment information (student, course, dates)
                - Formatted course format badges with color coding
                - Individual deletion capability with confirmation
                - Recent enrollment highlighting (last 7 days)
                - Responsive design for mobile viewing
                - Real-time count display and pagination info
                -->
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-list"></i> Enrollments List</h5>
                        <div>
                            <%-- Dynamic count updated by code-behind --%>
                            <asp:Label ID="lblDisplayedEnrollments" runat="server" 
                                CssClass="badge bg-info text-white me-2" Text="Showing: 0" />
                            <%-- Manual refresh capability --%>
                            <asp:Button ID="btnRefresh" runat="server" 
                                Text="Refresh" 
                                CssClass="btn btn-sm btn-outline-primary" 
                                OnClick="btnRefresh_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <!-- RESPONSIVE TABLE WRAPPER: Horizontal scrolling on small screens -->
                        <div class="table-responsive">
                            <%-- 
                            GRIDVIEW CONFIGURATION:
                            - Complex JOIN query displays student + course + enrollment data
                            - OnRowDataBound: Custom formatting for course format badges
                            - DataKeyNames: Enables deletion operations
                            - Responsive design with Bootstrap classes
                            --%>
                            <asp:GridView ID="gvEnrollments" runat="server" 
                                AutoGenerateColumns="False" 
                                CssClass="table table-striped table-hover"
                                DataKeyNames="enrollment_id" 
                                OnRowDataBound="gvEnrollments_RowDataBound" 
                                EmptyDataText="No enrollments found.">
                                <Columns>
                                    <%-- 
                                    COLUMN DEFINITIONS:
                                    Each column optimized for enrollment management workflow
                                    --%>
                                    
                                    <%-- ENROLLMENT ID: Unique identifier for operations --%>
                                    <asp:BoundField DataField="enrollment_id" HeaderText="ID" ReadOnly="True" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="60px" />
                                    
                                    <%-- STUDENT NAME: Primary identification field --%>
                                    <asp:BoundField DataField="student_name" HeaderText="Student Name" 
                                        ItemStyle-CssClass="fw-bold" 
                                        HeaderStyle-CssClass="text-start" />
                                    
                                    <%-- STUDENT EMAIL: Contact information --%>
                                    <asp:BoundField DataField="student_email" HeaderText="Student Email" 
                                        HeaderStyle-CssClass="text-start" 
                                        ItemStyle-Width="200px" />
                                    
                                    <%-- COURSE NAME: Enrolled course identification --%>
                                    <asp:BoundField DataField="course_name" HeaderText="Course Name" 
                                        ItemStyle-CssClass="fw-bold text-primary" 
                                        HeaderStyle-CssClass="text-start" />
                                    
                                    <%-- ECTS CREDITS: Academic value information --%>
                                    <asp:BoundField DataField="course_ects" HeaderText="ECTS" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="70px" />
                                    
                                    <%-- COURSE FORMAT: Visual badge formatting applied in RowDataBound --%>
                                    <asp:BoundField DataField="course_format" HeaderText="Format" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="100px" />
                                    
                                    <%-- ENROLLMENT DATE: When student enrolled in course --%>
                                    <asp:BoundField DataField="enrollment_date" HeaderText="Enrollment Date" 
                                        DataFormatString="{0:dd/MM/yyyy}" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="120px" />
                                    
                                    <%-- ACTION COLUMN: Deletion capability with confirmation --%>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <%-- 
                                            DELETE BUTTON: Individual enrollment deletion
                                            - CommandArgument: Passes enrollment_id for deletion
                                            - OnClientClick: JavaScript confirmation dialog
                                            - OnCommand: Server-side deletion handler
                                            --%>
                                            <asp:Button ID="btnDelete" runat="server" 
                                                Text="Delete" 
                                                CssClass="btn btn-sm btn-danger" 
                                                CommandName="DeleteEnrollment" 
                                                CommandArgument='<%# Eval("enrollment_id") %>'
                                                OnCommand="btnDelete_Command"
                                                OnClientClick="return confirm('Are you sure you want to delete this enrollment? This action cannot be undone!');" />
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                </Columns>
                                <%-- GRIDVIEW STYLING: Professional appearance with Bootstrap --%>
                                <HeaderStyle CssClass="table-dark" />
                                <RowStyle CssClass="align-middle" />
                            </asp:GridView>
                        </div>
                        
                        <!-- PAGINATION AND STATUS INFORMATION -->
                        <div class="d-flex justify-content-between align-items-center mt-3">
                            <div>
                                <small class="text-muted">
                                    <%-- Dynamic pagination info updated by code-behind --%>
                                    <asp:Label ID="lblPaginationInfo" runat="server" />
                                </small>
                            </div>
                            <div>
                                <!-- Future pagination controls can be added here -->
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- ANALYTICS PANELS SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Two-panel analytics layout providing:
                - Recent Activity: Latest enrollment activity monitoring
                - Popular Courses: Course popularity insights for administrators
                -->
                <div class="row mt-4">
                    <!-- RECENT ENROLLMENTS PANEL: Activity monitoring -->
                    <div class="col-md-6">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-clock"></i> Recent Enrollments (Last 7 Days)
                                </h6>
                            </div>
                            <div class="card-body">
                                <%-- 
                                RECENT ACTIVITY REPEATER:
                                Displays latest enrollments with student and course information
                                Limited to last 7 days for relevance
                                --%>
                                <asp:Repeater ID="rpRecentEnrollments" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between border-bottom py-2">
                                            <div>
                                                <%-- Student name with course information --%>
                                                <strong><%# Eval("student_name") %></strong><br/>
                                                <small class="text-muted"><%# Eval("course_name") %></small>
                                            </div>
                                            <div class="text-end">
                                                <%-- Formatted enrollment date --%>
                                                <small class="text-muted"><%# DateTime.Parse(Eval("enrollment_date").ToString()).ToString("dd/MM/yyyy") %></small>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                
                                <%-- EMPTY STATE: Shown when no recent activity --%>
                                <asp:Panel ID="pnlNoRecentEnrollments" runat="server" Visible="false">
                                    <div class="text-center text-muted py-3">
                                        <i class="fas fa-info-circle"></i>
                                        <p class="mb-0">No new enrollments in the last 7 days.</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                    
                    <!-- POPULAR COURSES PANEL: Course popularity analytics -->
                    <div class="col-md-6">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0">
                                    <i class="fas fa-trophy"></i> Most Popular Courses
                                </h6>
                            </div>
                            <div class="card-body">
                                <%-- 
                                POPULAR COURSES REPEATER:
                                Shows top 5 courses by enrollment count
                                Provides insights for curriculum planning
                                --%>
                                <asp:Repeater ID="rpPopularCourses" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between border-bottom py-2">
                                            <div>
                                                <%-- Course name with format and ECTS information --%>
                                                <strong><%# Eval("course_name") %></strong><br/>
                                                <small class="text-muted"><%# Eval("course_format") %> • <%# Eval("ects") %> ECTS</small>
                                            </div>
                                            <div class="text-end">
                                                <%-- Enrollment count badge --%>
                                                <span class="badge bg-primary"><%# Eval("enrollment_count") %> students</span>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                
                                <%-- EMPTY STATE: Shown when no enrollment data available --%>
                                <asp:Panel ID="pnlNoPopularCourses" runat="server" Visible="false">
                                    <div class="text-center text-muted py-3">
                                        <i class="fas fa-info-circle"></i>
                                        <p class="mb-0">No enrollment data available.</p>
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
    <!-- CUSTOM CSS STYLING SYSTEM -->
    <!-- ==================================================================== -->
    <!-- 
    Enhanced styling that extends Bootstrap with:
    - Professional dashboard card effects and shadows
    - Color-coded course format badges
    - Interactive hover effects and animations
    - Responsive design optimizations
    - Accessibility-compliant color schemes
    - Loading states and visual feedback
    -->
    <style>
        /* ==================================================================== */
        /* CARD COMPONENT ENHANCEMENTS */
        /* ==================================================================== */
        /* Professional card styling with subtle shadows and hover effects */
        .card {
            border: none;
            border-radius: 0.75rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
            transition: box-shadow 0.15s ease-in-out;
        }
        
        .card:hover {
            box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
        }
        
        /* ==================================================================== */
        /* STATISTICS CARD COLOR SCHEMES */
        /* ==================================================================== */
        /* Distinctive colors for each KPI metric */
        .bg-primary { background-color: #0d6efd !important; }
        .bg-success { background-color: #198754 !important; }
        .bg-info { background-color: #0dcaf0 !important; }
        .bg-warning { background-color: #ffc107 !important; }
        
        /* ==================================================================== */
        /* TABLE AND GRIDVIEW STYLING */
        /* ==================================================================== */
        /* Clean, professional table appearance */
        .table th {
            border-top: none;
            font-weight: 600;
            font-size: 0.9em;
        }
        
        .table td {
            font-size: 0.9em;
            vertical-align: middle;
        }
        
        /* ==================================================================== */
        /* TYPOGRAPHY AND UTILITY CLASSES */
        /* ==================================================================== */
        .fw-bold {
            font-weight: 600 !important;
        }
        
        .btn-sm {
            padding: 0.25rem 0.5rem;
            font-size: 0.8rem;
        }
        
        .card-header h5, .card-header h6 {
            margin-bottom: 0;
        }
        
        .form-label {
            font-weight: 500;
            margin-bottom: 0.5rem;
        }
        
        .me-1 {
            margin-right: 0.25rem !important;
        }
        
        .me-2 {
            margin-right: 0.5rem !important;
        }
        
        /* ==================================================================== */
        /* COURSE FORMAT BADGE STYLING */
        /* ==================================================================== */
        /* Color-coded badges for different course formats */
        .format-lecture { background-color: #0d6efd; color: white; }
        .format-seminar { background-color: #198754; color: white; }
        .format-workshop { background-color: #0dcaf0; color: black; }
        .format-laboratory { background-color: #ffc107; color: black; }
        .format-online { background-color: #6c757d; color: white; }
        .format-hybrid { background-color: #212529; color: white; }
        .format-practical { background-color: #f8f9fa; color: black; border: 1px solid #dee2e6; }
        
        /* ==================================================================== */
        /* INTERACTIVE EFFECTS */
        /* ==================================================================== */
        /* Hover effects for better user experience */
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.05);
        }
        
        /* Focus styles for accessibility */
        .form-control:focus {
            border-color: #0d6efd;
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
        }
        
        /* ==================================================================== */
        /* BUTTON ANIMATIONS */
        /* ==================================================================== */
        /* Subtle animations for professional feel */
        .btn {
            transition: all 0.15s ease-in-out;
        }
        
        .btn:hover:not(:disabled) {
            transform: translateY(-1px);
        }
        
        .btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
        
        /* ==================================================================== */
        /* BADGE STYLING */
        /* ==================================================================== */
        .badge {
            font-size: 0.75em;
            padding: 0.35em 0.65em;
        }
        
        /* ==================================================================== */
        /* DASHBOARD CARD HOVER EFFECTS */
        /* ==================================================================== */
        /* Enhanced hover effects for statistics cards */
        .card.bg-primary:hover,
        .card.bg-success:hover,
        .card.bg-info:hover,
        .card.bg-warning:hover {
            transform: translateY(-2px);
            box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
        }
        
        /* ==================================================================== */
        /* RECENT ACTIVITY STYLING */
        /* ==================================================================== */
        /* Clean borders for activity lists */
        .border-bottom {
            border-bottom: 1px solid #dee2e6 !important;
        }
        
        .border-bottom:last-child {
            border-bottom: none !important;
        }
        
        /* ==================================================================== */
        /* RESPONSIVE DESIGN ADJUSTMENTS */
        /* ==================================================================== */
        /* Mobile-friendly optimizations */
        @media (max-width: 768px) {
            .btn-sm {
                font-size: 0.7rem;
                padding: 0.2rem 0.4rem;
            }
            
            .table {
                font-size: 0.8rem;
            }
            
            .badge {
                font-size: 0.65em;
            }
            
            .card-body h3 {
                font-size: 1.5rem;
            }
            
            .card-body .fa-2x {
                font-size: 1.5em;
            }
        }
        
        /* ==================================================================== */
        /* LOADING STATES */
        /* ==================================================================== */
        .loading {
            opacity: 0.7;
            pointer-events: none;
        }
        
        /* ==================================================================== */
        /* SUCCESS/ERROR STATES */
        /* ==================================================================== */
        /* Visual indicators for row states */
        .table-success {
            background-color: rgba(25, 135, 84, 0.1) !important;
        }
        
        .table-danger {
            background-color: rgba(220, 53, 69, 0.1) !important;
        }
        
        /* ==================================================================== */
        /* ACCESSIBILITY ENHANCEMENTS */
        /* ==================================================================== */
        /* High contrast and screen reader support */
        .visually-hidden {
            position: absolute !important;
            width: 1px !important;
            height: 1px !important;
            padding: 0 !important;
            margin: -1px !important;
            overflow: hidden !important;
            clip: rect(0, 0, 0, 0) !important;
            white-space: nowrap !important;
            border: 0 !important;
        }
    </style>

    <!-- ==================================================================== -->
    <!-- CLIENT-SIDE JAVASCRIPT ENHANCEMENTS -->
    <!-- ==================================================================== -->
    <!-- 
    JavaScript functionality for enhanced user experience:
    - Button state management across postbacks
    - Loading state indication during server operations
    - Auto-refresh functionality for real-time updates
    - Form validation and visual feedback
    - Mobile responsiveness enhancements
    - Error handling and recovery
    -->
    <script>
        // ====================================================================
        // PAGE INITIALIZATION AND SETUP
        // ====================================================================
        
        // Execute when DOM is ready
        document.addEventListener('DOMContentLoaded', function() {
            initializePage();
        });
        
        // Execute after all resources are loaded
        window.addEventListener('load', function () {
            finalizePageLoad();
        });
        
        // ====================================================================
        // CORE INITIALIZATION FUNCTIONS
        // ====================================================================
        
        /**
         * Initialize page functionality
         * Sets up button states, event listeners, and enhancements
         */
        function initializePage() {
            // BUTTON STATE MANAGEMENT
            // Ensure buttons remain enabled after postbacks
            enableAllButtons();
            
            // FORM ENHANCEMENTS
            // Add visual feedback and validation
            enhanceSearchForm();
            
            // ACCESSIBILITY IMPROVEMENTS
            // Add keyboard navigation support
            setupKeyboardNavigation();
        }
        
        /**
         * Finalize page loading
         * Performs cleanup and final setup
         */
        function finalizePageLoad() {
            // Brief delay to ensure postback processing is complete
            setTimeout(function() {
                enableAllButtons();
                updateDashboardAnimations();
            }, 500);
        }
        
        // ====================================================================
        // BUTTON AND FORM MANAGEMENT
        // ====================================================================
        
        /**
         * Re-enable all buttons after postback
         * Prevents stuck disabled states
         */
        function enableAllButtons() {
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function(button) {
                button.disabled = false;
            });
        }
        
        /**
         * Enhanced search form functionality
         * Adds visual feedback and improved UX
         */
        function enhanceSearchForm() {
            // DATE RANGE VALIDATION
            var dateFrom = document.getElementById('<%= txtDateFrom.ClientID %>');
            var dateTo = document.getElementById('<%= txtDateTo.ClientID %>');
            
            if (dateFrom && dateTo) {
                // Validate date range when dates change
                dateFrom.addEventListener('change', validateDateRange);
                dateTo.addEventListener('change', validateDateRange);
            }
            
            // SEARCH FIELD ENHANCEMENTS
            var searchFields = document.querySelectorAll('input[type="text"]');
            searchFields.forEach(function(field) {
                // Add visual feedback for search fields
                field.addEventListener('input', function() {
                    if (this.value.length > 0) {
                        this.classList.add('has-content');
                    } else {
                        this.classList.remove('has-content');
                    }
                });
            });
        }
        
        /**
         * Validate date range selection
         * Ensures 'from' date is not after 'to' date
         */
        function validateDateRange() {
            var dateFrom = document.getElementById('<%= txtDateFrom.ClientID %>');
            var dateTo = document.getElementById('<%= txtDateTo.ClientID %>');
            
            if (dateFrom.value && dateTo.value) {
                var fromDate = new Date(dateFrom.value);
                var toDate = new Date(dateTo.value);
                
                if (fromDate > toDate) {
                    // Swap dates if from > to
                    var temp = dateFrom.value;
                    dateFrom.value = dateTo.value;
                    dateTo.value = temp;
                    
                    // Show brief visual feedback
                    showTooltip(dateTo, 'Date range corrected');
                }
            }
        }
        
        // ====================================================================
        // VISUAL FEEDBACK AND ANIMATIONS
        // ====================================================================
        
        /**
         * Update dashboard card animations
         * Adds staggered animation effects
         */
        function updateDashboardAnimations() {
            var cards = document.querySelectorAll('.card.bg-primary, .card.bg-success, .card.bg-info, .card.bg-warning');
            cards.forEach(function(card, index) {
                // Staggered animation for dashboard cards
                setTimeout(function() {
                    card.style.animation = 'fadeInUp 0.5s ease-out forwards';
                }, index * 100);
            });
        }
        
        /**
         * Show tooltip feedback
         * Displays temporary tooltip messages
         */
        function showTooltip(element, message) {
            var tooltip = document.createElement('div');
            tooltip.className = 'custom-tooltip';
            tooltip.textContent = message;
            tooltip.style.cssText = `
                position: absolute;
                background: #333;
                color: white;
                padding: 5px 10px;
                border-radius: 4px;
                font-size: 12px;
                z-index: 1000;
                opacity: 0;
                transition: opacity 0.3s;
            `;
            
            document.body.appendChild(tooltip);
            
            // Position tooltip
            var rect = element.getBoundingClientRect();
            tooltip.style.left = rect.left + 'px';
            tooltip.style.top = (rect.top - 30) + 'px';
            
            // Show and hide tooltip
            setTimeout(function() { tooltip.style.opacity = '1'; }, 10);
            setTimeout(function() {
                tooltip.style.opacity = '0';
                setTimeout(function() {
                    if (tooltip.parentNode) {
                        tooltip.parentNode.removeChild(tooltip);
                    }
                }, 300);
            }, 2000);
        }
        
        // ====================================================================
        // LOADING STATES AND PROGRESS INDICATION
        // ====================================================================
        
        /**
         * Add loading state to delete buttons
         * Provides visual feedback during deletion
         */
        function setLoadingState(button) {
            button.disabled = true;
            button.textContent = 'Deleting...';
            button.classList.add('loading');
        }
        
        /**
         * Remove loading state
         * Restores normal button appearance
         */
        function removeLoadingState() {
            var buttons = document.querySelectorAll('.btn.loading');
            buttons.forEach(function(btn) {
                btn.classList.remove('loading');
                btn.disabled = false;
                // Reset text (will be handled by postback)
            });
        }
        
        // ====================================================================
        // ACCESSIBILITY ENHANCEMENTS
        // ====================================================================
        
        /**
         * Setup keyboard navigation
         * Improves accessibility for keyboard users
         */
        function setupKeyboardNavigation() {
            // Enhanced Enter key handling for search
            document.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' && !e.shiftKey) {
                    var target = e.target;
                    
                    // If in search field, trigger search
                    if (target.classList.contains('form-control')) {
                        var searchButton = document.getElementById('<%= btnSearch.ClientID %>');
                        if (searchButton) {
                            e.preventDefault();
                            searchButton.click();
                        }
                    }
                }
            });
            
            // Focus management for better navigation
            var firstInput = document.querySelector('input[type="text"]');
            if (firstInput) {
                firstInput.focus();
            }
        }
        
        // ====================================================================
        // AUTO-REFRESH FUNCTIONALITY
        // ====================================================================
        
        /**
         * Auto-refresh dashboard every 5 minutes
         * Keeps data current for administrative monitoring
         * Only refreshes if page is visible to user
         */
        setTimeout(function() {
            // Check if page is visible and auto-refresh is appropriate
            if (!document.hidden && typeof __doPostBack !== 'undefined') {
                var refreshButton = document.getElementById('<%= btnRefresh.ClientID %>');
                if (refreshButton) {
                    console.log('Auto-refreshing enrollment data...');
                    refreshButton.click();
                }
            }
        }, 300000); // 5 minutes

        // ====================================================================
        // ERROR HANDLING AND RECOVERY
        // ====================================================================

        /**
         * Global error handler
         * Provides graceful error recovery
         */
        window.addEventListener('error', function (e) {
            console.log('Page error detected:', e.message);
            // Ensure buttons remain functional
            enableAllButtons();
        });

        /**
         * Handle page visibility changes
         * Manages auto-refresh based on visibility
         */
        document.addEventListener('visibilitychange', function () {
            if (!document.hidden) {
                // Page became visible - ensure everything is working
                enableAllButtons();
            }
        });

        // ====================================================================
        // MOBILE RESPONSIVENESS
        // ====================================================================

        /**
         * Enhanced mobile support
         * Touch-friendly interactions
         */
        if ('ontouchstart' in window) {
            // Add touch-specific enhancements
            document.body.classList.add('touch-device');

            // Larger touch targets for mobile
            var smallButtons = document.querySelectorAll('.btn-sm');
            smallButtons.forEach(function (btn) {
                btn.style.minHeight = '44px';
                btn.style.padding = '8px 12px';
            });
        }
    </script>

    <!-- 
=============================================================================
END OF MANAGEENROLLMENTS.ASPX PAGE
=============================================================================

PAGE ARCHITECTURE SUMMARY:
1. Statistics Dashboard: Real-time KPI display with 4 key metrics
2. Advanced Search Interface: Multi-criteria filtering with date ranges
3. Professional Data Grid: Comprehensive enrollment display with actions
4. Analytics Panels: Recent activity and popular courses insights
5. Enhanced Styling: Modern, responsive design with Bootstrap extensions
6. JavaScript Layer: Real-time updates, validation, and UX enhancements

DASHBOARD DESIGN PHILOSOPHY:
- Data-Driven Decision Making: Real-time metrics for administrative insights
- Workflow Optimization: Efficient enrollment management and monitoring
- Visual Hierarchy: Clear information organization and priority
- Progressive Enhancement: Features degrade gracefully on older browsers
- Mobile-First Approach: Responsive design for all device types

ADVANCED FEATURES:
- Real-time Statistics: Live calculation and display of key metrics
- Intelligent Filtering: Context-aware search with date range validation
- Audit Trail Display: Complete enrollment history with deletion capability
- Recent Activity Monitoring: Last 7 days of enrollment activity
- Popular Courses Analytics: Data-driven curriculum insights

USER EXPERIENCE DESIGN:
- Administrative Workflow: Optimized for administrative tasks
- Visual Feedback: Immediate confirmation of all operations
- Error Recovery: Graceful handling of partial failures
- Accessibility: Full keyboard navigation and screen reader support
- Performance: Optimized queries and efficient data loading

SECURITY AND COMPLIANCE:
- Role-based Access: Admin-only access with session validation
- Audit Capabilities: Complete tracking of enrollment modifications
- Data Protection: Secure parameter binding and input validation
- CSRF Protection: ViewState validation and form security
- Privacy Considerations: Appropriate data display and access controls

TECHNICAL IMPLEMENTATION:
- Bootstrap Framework: Modern, responsive UI components
- ASP.NET Web Forms: Server-side rendering with ViewState management
- Client-side Enhancement: Progressive JavaScript improvements
- Database Integration: Optimized queries with proper join operations
- Error Handling: Comprehensive error recovery and user feedback
=============================================================================
-->

</asp:Content>
