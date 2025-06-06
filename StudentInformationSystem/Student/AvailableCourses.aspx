<%--
===================================================================================
AVAILABLE COURSES - STUDENT PORTAL PAGE (ASPX MARKUP)
===================================================================================
Purpose: Front-end interface for student course discovery and enrollment management
Features: Course browsing, search/filtering, enrollment operations, personal dashboard

Student Portal Capabilities:
- Interactive course catalog with real-time search and filtering
- One-click enrollment and unenrollment with confirmations
- Personal enrollment dashboard with ECTS tracking
- Responsive design optimized for mobile and desktop access
- Visual status indicators for enrollment states
- Bootstrap-based responsive UI with modern styling

Page Structure Overview:
1. Student Information Dashboard - Personal academic overview
2. Search & Filter Controls - Multi-criteria course discovery
3. Available Courses Grid - Interactive course catalog with actions
4. Current Enrollments Display - Personal enrollment summary
5. Custom Styling - Enhanced visual design and responsiveness
6. Client-side Scripts - UI enhancements and loading states
===================================================================================
--%>

<%-- 
PAGE DIRECTIVE CONFIGURATION
Defines the page structure, inheritance, and code-behind linkage
--%>
<%@ Page Title="Available Courses" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="AvailableCourses.aspx.vb" Inherits="StudentInformationSystem.AvailableCourses" %>

<%-- 
MAIN CONTENT AREA
Contains all student portal functionality within the master page framework
Uses Bootstrap container-fluid for full-width responsive layout
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                
                <%-- 
                PAGE HEADER SECTION
                Main page title with search icon for visual context
                --%>
                <h2 class="text-center mb-4">
                    <i class="fas fa-search"></i> Available Courses
                </h2>
                
                <%-- 
                DYNAMIC MESSAGE PANEL
                Displays system feedback, success confirmations, and error messages
                Controlled by code-behind: MessagePanel.Visible and MessageLiteral.Text
                Hidden by default, shown when user actions require feedback
                --%>
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <%-- 
                STUDENT INFORMATION DASHBOARD CARD
                Personal academic overview showing current enrollment status
                Displays: Student name, email, enrollment count, total ECTS credits
                Provides context for enrollment decisions and academic planning
                --%>
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">
                            <i class="fas fa-user-graduate"></i> Student Information
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <%-- Left Column: Basic Student Information --%>
                            <div class="col-md-6">
                                <%-- Student Full Name (populated from database) --%>
                                <p><strong>Name:</strong> <asp:Label ID="lblStudentName" runat="server" CssClass="text-primary" /></p>
                                <%-- Student Email Address (populated from session) --%>
                                <p><strong>Email:</strong> <asp:Label ID="lblStudentEmail" runat="server" CssClass="text-muted" /></p>
                            </div>
                            <%-- Right Column: Academic Statistics --%>
                            <div class="col-md-6">
                                <%-- Current Course Enrollment Count (calculated from enrollments table) --%>
                                <p><strong>Current Enrollments:</strong> <asp:Label ID="lblCurrentEnrollments" runat="server" CssClass="badge badge-info text-dark" /></p>
                                <%-- Total ECTS Credits Enrolled (sum of ECTS from enrolled courses) --%>
                                <p><strong>Total ECTS Enrolled:</strong> <asp:Label ID="lblTotalECTS" runat="server" CssClass="badge badge-success text-dark" /></p>
                            </div>
                        </div>
                    </div>
                </div>

                <%-- 
                COURSE SEARCH AND FILTER CONTROLS CARD
                Multi-criteria course discovery interface
                Enables students to find courses by name, format, and ECTS credits
                Implements responsive design for mobile-friendly filtering
                --%>
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-filter"></i> Search & Filter Courses</h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <%-- Course Name Search Input (40% width on desktop) --%>
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label for="<%= txtSearchName.ClientID %>" class="form-label">Search by Course Name</label>
                                    <%-- Text input for course name search (case-insensitive partial matching) --%>
                                    <asp:TextBox ID="txtSearchName" runat="server" CssClass="form-control" placeholder="Enter course name..." />
                                </div>
                            </div>
                            
                            <%-- Course Format Filter Dropdown (30% width on desktop) --%>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= ddlFilterFormat.ClientID %>" class="form-label">Filter by Format</label>
                                    <%-- Dropdown list for course delivery format filtering --%>
                                    <asp:DropDownList ID="ddlFilterFormat" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="All Formats" Value="" />        <%-- Default: No filter --%>
                                        <asp:ListItem Text="Lecture" Value="lecture" />     <%-- Traditional lecture format --%>
                                        <asp:ListItem Text="Seminar" Value="seminar" />     <%-- Interactive seminar format --%>
                                        <asp:ListItem Text="Workshop" Value="workshop" />   <%-- Hands-on workshop format --%>
                                        <asp:ListItem Text="Laboratory" Value="laboratory" /><%-- Lab-based practical format --%>
                                        <asp:ListItem Text="Online" Value="online" />       <%-- Remote online delivery --%>
                                        <asp:ListItem Text="Hybrid" Value="hybrid" />       <%-- Mixed online/in-person --%>
                                        <asp:ListItem Text="Practical" Value="practical" /> <%-- Applied practical format --%>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            
                            <%-- ECTS Credit Range Filter (30% width on desktop) --%>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= ddlFilterECTS.ClientID %>" class="form-label">Filter by ECTS</label>
                                    <%-- Dropdown for ECTS credit range filtering --%>
                                    <asp:DropDownList ID="ddlFilterECTS" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="All ECTS" Value="" />           <%-- Default: No ECTS filter --%>
                                        <asp:ListItem Text="1-3 ECTS" Value="1-3" />       <%-- Low credit courses --%>
                                        <asp:ListItem Text="4-6 ECTS" Value="4-6" />       <%-- Medium credit courses --%>
                                        <asp:ListItem Text="7-9 ECTS" Value="7-9" />       <%-- High credit courses --%>
                                        <asp:ListItem Text="10+ ECTS" Value="10+" />       <%-- Very high credit courses --%>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            
                            <%-- Action Buttons Column (20% width on desktop) --%>
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label> <%-- Empty label for alignment --%>
                                    <div>
                                        <%-- Search Button: Apply current filter criteria --%>
                                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-info btn-sm me-1" OnClick="btnSearch_Click" />
                                        <%-- Clear Button: Reset all filters to default state --%>
                                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClearSearch_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <%-- 
                AVAILABLE COURSES CATALOG CARD
                Main course discovery and enrollment interface
                Features sortable grid with enrollment actions and status indicators
                --%>
                <div class="card">
                    <%-- Course Catalog Header with Statistics and Manual Refresh --%>
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-book-open"></i> Available Courses</h5>
                        <div>
                            <%-- Dynamic course count display (updated by code-behind) --%>
                            <asp:Label ID="lblTotalCourses" runat="server" CssClass="badge badge-info me-2 text-dark" Text="Total: 0" />
                            <%-- Manual refresh button for real-time data updates --%>
                            <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-sm btn-outline-primary" OnClick="btnRefresh_Click" />
                        </div>
                    </div>
                    
                    <%-- Course Catalog Table Container --%>
                    <div class="card-body">
                        <div class="table-responsive"> <%-- Horizontal scrolling for mobile devices --%>
                            <%-- 
                            MAIN COURSE CATALOG GRIDVIEW
                            Interactive table displaying all available courses with enrollment capabilities
                            Features: Sorting, responsive design, enrollment status, action buttons
                            Data binding: Controlled by code-behind LoadAvailableCourses() method
                            --%>
                            <asp:GridView ID="gvCourses" runat="server" 
                                AutoGenerateColumns="False" 
                                CssClass="table table-striped table-hover"
                                DataKeyNames="course_id" 
                                OnRowDataBound="gvCourses_RowDataBound" 
                                EmptyDataText="No courses found.">
                                
                                <Columns>
                                    <%-- Course ID Column: Unique identifier (read-only, centered, narrow) --%>
                                    <asp:BoundField DataField="course_id" HeaderText="ID" ReadOnly="True" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="60px" />
                                    
                                    <%-- Course Name Column: Primary course identifier (bold, left-aligned, flexible width) --%>
                                    <asp:BoundField DataField="course_name" HeaderText="Course Name" 
                                        ItemStyle-CssClass="fw-bold" HeaderStyle-CssClass="text-start" />
                                    
                                    <%-- ECTS Credits Column: Academic credit value (centered, narrow) --%>
                                    <asp:BoundField DataField="ects" HeaderText="ECTS" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="80px" />
                                    
                                    <%-- Course Hours Column: Total course duration (centered, narrow) --%>
                                    <asp:BoundField DataField="hours" HeaderText="Hours" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="80px" />
                                    
                                    <%-- Course Format Column: Delivery method with custom styling (see RowDataBound event) --%>
                                    <asp:BoundField DataField="format" HeaderText="Format" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="120px" />
                                    
                                    <%-- Instructor Column: Course teacher information (left-aligned, medium width) --%>
                                    <asp:BoundField DataField="instructor" HeaderText="Instructor" 
                                        HeaderStyle-CssClass="text-start" ItemStyle-Width="150px" />
                                    
                                    <%-- 
                                    ENROLLMENT STATUS TEMPLATE COLUMN
                                    Dynamic status display controlled by RowDataBound event
                                    Shows "Enrolled" or "Not Enrolled" with appropriate styling
                                    --%>
                                    <asp:TemplateField HeaderText="Enrollment Status" ItemStyle-Width="140px">
                                        <ItemTemplate>
                                            <%-- Status label populated dynamically by code-behind --%>
                                            <asp:Label ID="lblEnrollmentStatus" runat="server" />
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    
                                    <%-- 
                                    ENROLLMENT ACTIONS TEMPLATE COLUMN
                                    Dynamic button display based on current enrollment status
                                    Contains mutually exclusive Enroll/Unenroll buttons
                                    --%>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="120px">
                                        <ItemTemplate>
                                            <%-- 
                                            ENROLL BUTTON
                                            Visible when student is NOT enrolled in course
                                            Triggers enrollment process with confirmation dialog
                                            --%>
                                            <asp:Button ID="btnEnroll" runat="server" Text="Enroll" 
                                                CssClass="btn btn-sm btn-success" 
                                                CommandName="Enroll" 
                                                CommandArgument='<%# Eval("course_id") %>'
                                                OnCommand="btnEnroll_Command"
                                                OnClientClick="return confirm('Are you sure you want to enroll in this course?');" />
                                            
                                            <%-- 
                                            UNENROLL BUTTON
                                            Visible when student IS enrolled in course
                                            Triggers unenrollment process with confirmation dialog
                                            Hidden by default, shown by RowDataBound event when appropriate
                                            --%>
                                            <asp:Button ID="btnUnenroll" runat="server" Text="Unenroll" 
                                                CssClass="btn btn-sm btn-warning" 
                                                CommandName="Unenroll" 
                                                CommandArgument='<%# Eval("course_id") %>'
                                                OnCommand="btnUnenroll_Command"
                                                OnClientClick="return confirm('Are you sure you want to unenroll from this course?');" 
                                                Visible="false" />
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                </Columns>
                                
                                <%-- Grid Styling Configuration --%>
                                <HeaderStyle CssClass="table-dark" />    <%-- Dark theme header row --%>
                                <RowStyle CssClass="align-middle" />     <%-- Vertical center alignment for all rows --%>
                            </asp:GridView>
                        </div>
                    </div>
                </div>

                <%-- 
                PERSONAL ENROLLMENTS SUMMARY CARD
                Student's current enrollment dashboard with course details
                Displays enrolled courses in responsive card layout
                --%>
                <div class="card mt-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-clipboard-list"></i> My Current Enrollments</h5>
                    </div>
                    <div class="card-body">
                        <%-- 
                        CURRENT ENROLLMENTS REPEATER
                        Displays student's enrolled courses in responsive card grid
                        Data source: Current student's enrollments with joined course details
                        --%>
                        <asp:Repeater ID="rpMyEnrollments" runat="server">
                            <%-- Header: Open responsive row container --%>
                            <HeaderTemplate>
                                <div class="row">
                            </HeaderTemplate>
                            
                            <%-- 
                            INDIVIDUAL ENROLLMENT CARD ITEM
                            Each enrolled course displayed as a bootstrap card
                            Responsive: 2 cards per row on tablet, 3 per row on desktop
                            --%>
                            <ItemTemplate>
                                <div class="col-md-6 col-lg-4 mb-3">
                                    <div class="card border-success"> <%-- Green border indicates active enrollment --%>
                                        <div class="card-body">
                                            <%-- Course Name as Card Title --%>
                                            <h6 class="card-title text-success"><%# Eval("course_name") %></h6>
                                            
                                            <%-- Course Details and Enrollment Information --%>
                                            <p class="card-text small">
                                                <%-- Academic Credits and Hours --%>
                                                <strong>ECTS:</strong> <%# Eval("ects") %> | 
                                                <strong>Hours:</strong> <%# Eval("hours") %><br/>
                                                
                                                <%-- Course Delivery Format --%>
                                                <strong>Format:</strong> <%# Eval("format") %><br/>
                                                
                                                <%-- Course Instructor --%>
                                                <strong>Instructor:</strong> <%# Eval("instructor") %><br/>
                                                
                                                <%-- Enrollment Date (formatted for user-friendly display) --%>
                                                <strong>Enrolled:</strong> <%# DateTime.Parse(Eval("enrollment_date").ToString()).ToString("dd/MM/yyyy") %>
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                            
                            <%-- Footer: Close responsive row container --%>
                            <FooterTemplate>
                                </div>
                            </FooterTemplate>
                        </asp:Repeater>
                        
                        <%-- 
                        EMPTY STATE PANEL
                        Displayed when student has no current enrollments
                        Provides encouragement and guidance for new students
                        Controlled by code-behind: Visible property set based on enrollment data
                        --%>
                        <asp:Panel ID="pnlNoEnrollments" runat="server" Visible="false">
                            <div class="text-center text-muted py-4">
                                <%-- Large info icon for visual emphasis --%>
                                <i class="fas fa-info-circle fa-2x mb-2"></i>
                                <%-- Primary empty state message --%>
                                <p>You are not currently enrolled in any courses.</p>
                                <%-- Helpful guidance for getting started --%>
                                <p class="small">Browse available courses above and click "Enroll" to get started!</p>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- 
    ===================================================================================
    CUSTOM STYLING SECTION
    Enhanced visual design and responsive behavior improvements
    Supplements Bootstrap with application-specific styling
    ===================================================================================
    --%>
    <style>
        /* 
        BADGE COMPONENT STYLING
        Consistent badge appearance across all status indicators
        */
        .badge {
            font-size: 0.75em;              /* Slightly smaller than normal text */
            padding: 0.35em 0.65em;         /* Comfortable padding for readability */
        }
        
        /* 
        TABLE ENHANCEMENT STYLING
        Improved typography and visual hierarchy for course data
        */
        .table th {
            border-top: none;               /* Remove default top border */
            font-weight: 600;               /* Semi-bold headers for emphasis */
            font-size: 0.9em;               /* Slightly smaller header text */
        }
        
        .table td {
            font-size: 0.9em;               /* Consistent smaller text for data */
            vertical-align: middle;         /* Center content vertically in cells */
        }
        
        /* 
        TEXT WEIGHT UTILITY
        Bootstrap-compatible font weight utility class
        */
        .fw-bold {
            font-weight: 600 !important;   /* Semi-bold text emphasis */
        }
        
        /* 
        BUTTON SIZE CUSTOMIZATION
        Optimized small button sizing for grid actions
        */
        .btn-sm {
            padding: 0.25rem 0.5rem;       /* Compact padding for table buttons */
            font-size: 0.8rem;             /* Smaller text for space efficiency */
        }
        
        /* 
        CARD HEADER STYLING
        Consistent card header appearance
        */
        .card-header h5 {
            margin-bottom: 0;              /* Remove default margin for clean alignment */
        }
        
        /* 
        FORM LABEL ENHANCEMENT
        Improved form label visibility and spacing
        */
        .form-label {
            font-weight: 500;              /* Medium weight for form labels */
            margin-bottom: 0.5rem;         /* Consistent label spacing */
        }
        
        /* 
        SPACING UTILITY CLASSES
        Bootstrap-compatible margin utilities
        */
        .me-1 {
            margin-right: 0.25rem !important;  /* Small right margin */
        }
        
        .me-2 {
            margin-right: 0.5rem !important;   /* Medium right margin */
        }
        
        /* 
        INTERACTIVE HOVER EFFECTS
        Enhanced user experience with visual feedback
        */
        
        /* Table row hover effect for better navigation */
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.05); /* Subtle gray highlight on hover */
        }
        
        /* Form control focus styling for accessibility */
        .form-control:focus, .form-select:focus {
            border-color: #0d6efd;             /* Blue border on focus */
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25); /* Blue glow effect */
        }
        
        /* Card hover effects for interactive elements */
        .card {
            transition: box-shadow 0.15s ease-in-out; /* Smooth shadow transition */
        }
        
        .card:hover {
            box-shadow: 0 0.125rem 0.5rem rgba(0, 0, 0, 0.075); /* Elevated shadow on hover */
        }
        
        /* 
        BUTTON ANIMATION EFFECTS
        Subtle animations for better user feedback
        */
        .btn {
            transition: all 0.15s ease-in-out; /* Smooth transition for all properties */
        }
        
        /* Button lift effect on hover (except disabled buttons) */
        .btn:hover:not(:disabled) {
            transform: translateY(-1px);       /* Subtle upward movement */
        }
        
        /* Disabled button styling */
        .btn:disabled {
            opacity: 0.6;                     /* Reduced opacity for disabled state */
            cursor: not-allowed;              /* Not-allowed cursor */
            transform: none;                  /* No hover animation when disabled */
        }
        
        /* 
        ENROLLMENT STATUS STYLING
        Visual indicators for enrollment states
        */
        
        /* Success enrollment cards styling */
        .border-success {
            border-color: #198754 !important; /* Green border for enrolled courses */
        }
        
        .text-success {
            color: #198754 !important;        /* Green text for enrolled courses */
        }
        
        /* 
        RESPONSIVE DESIGN ADAPTATIONS
        Mobile-first responsive enhancements
        */
        @media (max-width: 768px) {
            /* Smaller buttons for mobile screens */
            .btn-sm {
                font-size: 0.7rem;            /* Even smaller text on mobile */
                padding: 0.2rem 0.4rem;       /* Tighter padding for mobile */
            }
            
            /* Condensed table text for mobile readability */
            .table {
                font-size: 0.8rem;            /* Smaller table text on mobile */
            }
            
            /* Smaller badges for mobile screens */
            .badge {
                font-size: 0.65em;            /* Even smaller badges on mobile */
            }
        }
        
        /* 
        LOADING STATE STYLING
        Visual feedback during processing operations
        */
        .loading {
            opacity: 0.7;                     /* Faded appearance during loading */
            pointer-events: none;             /* Disable interactions while loading */
        }
        
        /* 
        ENROLLMENT STATUS BADGE STYLING
        Distinctive colors for enrollment states
        */
        
        /* Enrolled status styling */
        .status-enrolled {
            background-color: #198754 !important; /* Green background for enrolled */
            color: white !important;              /* White text for contrast */
        }
        
        /* Not enrolled status styling */
        .status-not-enrolled {
            background-color: #6c757d !important; /* Gray background for not enrolled */
            color: white !important;              /* White text for contrast */
        }
    </style>

    <%-- 
    ===================================================================================
    CLIENT-SIDE JAVASCRIPT SECTION
    User interface enhancements and interactive functionality
    ===================================================================================
    --%>
    <script>
        /* 
        PAGE LOAD EVENT HANDLER
        Re-enable buttons after page load to prevent disabled state persistence
        Fixes issue where buttons remain disabled after postback operations
        */
        window.addEventListener('load', function () {
            // Find all submit and button input elements
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');

            // Re-enable each button to ensure interactivity
            buttons.forEach(function (button) {
                button.disabled = false;
            });
        });

        /* 
        LOADING STATE UTILITY FUNCTION
        Provides visual feedback during enrollment operations
        Can be called from client-side events to show processing state
        */
        function setLoadingState(button) {
            button.disabled = true;                 // Disable button to prevent double-clicks
            button.textContent = 'Processing...';   // Show processing message
            button.classList.add('loading');       // Apply loading CSS class
        }

        // Additional client-side functionality can be added here:
        // - Real-time form validation
        // - Ajax-based enrollment operations
        // - Dynamic filtering without postback
        // - Keyboard navigation enhancements
        // - Progress indicators for long operations
    </script>

    <%--
===================================================================================
END OF AVAILABLE COURSES PAGE MARKUP
===================================================================================

SUMMARY OF PAGE FUNCTIONALITY:
This ASPX page provides a comprehensive student portal for course discovery and 
enrollment management within the Student Information System. The interface combines
modern responsive design with practical academic functionality.

KEY FEATURES IMPLEMENTED:
1. Student Dashboard: Personal academic overview with enrollment statistics
2. Course Discovery: Multi-criteria search and filtering capabilities
3. Interactive Catalog: Sortable course grid with real-time enrollment actions
4. Enrollment Management: One-click enroll/unenroll with confirmation dialogs
5. Personal Summary: Current enrollments display with detailed course information
6. Responsive Design: Mobile-optimized layout with touch-friendly interactions
7. Visual Feedback: Comprehensive styling and loading states
8. Accessibility: Screen reader compatible with semantic HTML structure

RESPONSIVE DESIGN APPROACH:
- Mobile-first CSS with progressive enhancement
- Bootstrap grid system for flexible layouts
- Touch-optimized button sizes and spacing
- Horizontal scrolling for complex tables on small screens
- Condensed typography for mobile readability

USER EXPERIENCE DESIGN:
- Clear visual hierarchy with cards and sections
- Immediate feedback for all user actions
- Progressive disclosure of complex information
- Consistent styling and interaction patterns
- Accessibility-compliant color contrast and focus states

INTEGRATION POINTS:
- Code-behind events for all enrollment operations
- Master page integration for consistent site navigation
- Session-based authentication and authorization
- Database-driven content with real-time updates
- Bootstrap framework for responsive UI components

FUTURE ENHANCEMENT OPPORTUNITIES:
- Ajax-based operations for faster enrollment actions
- Real-time course availability updates
- Advanced filtering with date ranges and prerequisites
- Course recommendation engine based on student history
- Integration with academic calendar and scheduling systems
- Enhanced mobile app-like experience with offline capabilities
===================================================================================
--%>

</asp:Content>
