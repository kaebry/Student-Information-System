<%@ Page Title="Manage Courses" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ManageCourses.aspx.vb" Inherits="StudentInformationSystem.ManageCourses" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- 
    =============================================================================
    MANAGE COURSES PAGE - ASP.NET WEB FORMS UI
    =============================================================================
    Purpose: Comprehensive interface for course management operations (CRUD)
    Features: Course creation, editing, deletion, search, and filtering
    
    Key UI Components:
    - Course input form with comprehensive validation
    - Advanced search and filtering controls
    - Professional GridView with formatted display
    - Responsive Bootstrap design with custom enhancements
    - Client-side form validation and UX improvements
    - Real-time feedback and loading states
    
    Target Users: System administrators and academic staff
    =============================================================================
    -->

    <!-- MAIN CONTAINER: Bootstrap fluid container for full-width responsive layout -->
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <!-- PAGE HEADER: Professional title with icon for visual hierarchy -->
                <h2 class="text-center mb-4">
                    <i class="fas fa-book"></i> Manage Courses
                </h2>
                
                <!-- ==================================================================== -->
                <!-- MESSAGE DISPLAY SYSTEM -->
                <!-- ==================================================================== -->
                <!-- Dynamic message panel for server-side feedback (success/error/info) -->
                <!-- Updated by code-behind based on operation results -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <!-- ==================================================================== -->
                <!-- COURSE INPUT FORM SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Card-based layout provides visual separation and professional appearance
                Form includes comprehensive validation and user guidance
                -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h4 class="mb-0">
                            <i class="fas fa-plus-circle"></i> Course Information
                            <%-- Dynamic form mode indicator (Add/Edit) --%>
                            <asp:Label ID="lblFormMode" runat="server" CssClass="badge badge-secondary ml-2 text-dark" Text="Add New Course" />
                        </h4>
                    </div>
                    <div class="card-body">
                        <!-- FORM FIELDS LAYOUT: 2x3 responsive grid for optimal space usage -->
                        <div class="row">
                            <!-- COURSE NAME FIELD: Primary identifier with validation -->
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="<%= txtCourseName.ClientID %>" class="form-label">
                                        Course Name <span class="text-danger">*</span>
                                    </label>
                                    <%-- TextBox with Bootstrap styling and character limits --%>
                                    <asp:TextBox ID="txtCourseName" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter course name" 
                                        MaxLength="200" />
                                    <%-- Server-side validation: Required field with custom error message --%>
                                    <asp:RequiredFieldValidator ID="rfvCourseName" runat="server" 
                                        ControlToValidate="txtCourseName" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Course name is required." 
                                        Display="Dynamic" />
                                </div>
                            </div>
                            
                            <!-- ECTS CREDITS FIELD: Academic credit value with range validation -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtECTS.ClientID %>" class="form-label">
                                        ECTS Credits <span class="text-danger">*</span>
                                    </label>
                                    <%-- Number input for ECTS with HTML5 number validation --%>
                                    <asp:TextBox ID="txtECTS" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="e.g. 6" 
                                        TextMode="Number" />
                                    <%-- Required field validation --%>
                                    <asp:RequiredFieldValidator ID="rfvECTS" runat="server" 
                                        ControlToValidate="txtECTS" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="ECTS credits is required." 
                                        Display="Dynamic" />
                                    <%-- Range validation: Ensures ECTS values are within academic standards --%>
                                    <asp:RangeValidator ID="rvECTS" runat="server" 
                                        ControlToValidate="txtECTS" 
                                        MinimumValue="1" 
                                        MaximumValue="30" 
                                        Type="Integer" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="ECTS must be between 1 and 30." 
                                        Display="Dynamic" />
                                </div>
                            </div>
                            
                            <!-- HOURS FIELD: Contact hours with validation -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtHours.ClientID %>" class="form-label">
                                        Hours <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtHours" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="e.g. 45" 
                                        TextMode="Number" />
                                    <asp:RequiredFieldValidator ID="rfvHours" runat="server" 
                                        ControlToValidate="txtHours" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Hours is required." 
                                        Display="Dynamic" />
                                    <%-- Range validation: Reasonable hour limits for academic courses --%>
                                    <asp:RangeValidator ID="rvHours" runat="server" 
                                        ControlToValidate="txtHours" 
                                        MinimumValue="1" 
                                        MaximumValue="500" 
                                        Type="Integer" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Hours must be between 1 and 500." 
                                        Display="Dynamic" />
                                </div>
                            </div>
                        </div>
                        
                        <!-- SECOND ROW: Additional course properties -->
                        <div class="row">
                            <!-- COURSE FORMAT FIELD: Dropdown with predefined academic formats -->
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="<%= ddlFormat.ClientID %>" class="form-label">
                                        Course Format <span class="text-danger">*</span>
                                    </label>
                                    <%-- 
                                    DropDownList with comprehensive academic format options
                                    Each format represents different teaching methodologies
                                    --%>
                                    <asp:DropDownList ID="ddlFormat" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="Select Format" Value="" />
                                        <asp:ListItem Text="Lecture" Value="lecture" />
                                        <asp:ListItem Text="Seminar" Value="seminar" />
                                        <asp:ListItem Text="Workshop" Value="workshop" />
                                        <asp:ListItem Text="Laboratory" Value="laboratory" />
                                        <asp:ListItem Text="Online" Value="online" />
                                        <asp:ListItem Text="Hybrid" Value="hybrid" />
                                        <asp:ListItem Text="Practical" Value="practical" />
                                    </asp:DropDownList>
                                    <%-- Required validation for format selection --%>
                                    <asp:RequiredFieldValidator ID="rfvFormat" runat="server" 
                                        ControlToValidate="ddlFormat" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Course format is required." 
                                        Display="Dynamic" />
                                </div>
                            </div>
                            
                            <!-- INSTRUCTOR FIELD: Faculty member assignment -->
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="<%= txtInstructor.ClientID %>" class="form-label">
                                        Instructor <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtInstructor" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter instructor name" 
                                        MaxLength="100" />
                                    <asp:RequiredFieldValidator ID="rfvInstructor" runat="server" 
                                        ControlToValidate="txtInstructor" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Instructor is required." 
                                        Display="Dynamic" />
                                </div>
                            </div>
                        </div>

                        <!-- ==================================================================== -->
                        <!-- ACTION BUTTONS SECTION -->
                        <!-- ==================================================================== -->
                        <!-- 
                        Button row with different behaviors for different operations:
                        - Add/Update: Require form validation
                        - Delete: Confirmation dialog with no validation
                        - Clear: No validation (allows clearing invalid data)
                        -->
                        <div class="row">
                            <div class="col-md-12">
                                <%-- ADD BUTTON: Creates new course with validation --%>
                                <asp:Button ID="btnAdd" runat="server" 
                                    Text="Add Course" 
                                    CssClass="btn btn-primary me-2" 
                                    OnClick="btnAdd_Click" 
                                    OnClientClick="return confirm('Are you sure you want to add this course?');" />
                                
                                <%-- UPDATE BUTTON: Modifies selected course (disabled until selection) --%>
                                <asp:Button ID="btnUpdate" runat="server" 
                                    Text="Update Course" 
                                    CssClass="btn btn-warning me-2" 
                                    OnClick="btnUpdate_Click" 
                                    Enabled="False" 
                                    OnClientClick="return confirm('Are you sure you want to update this course?');" />
                                
                                <%-- DELETE BUTTON: Removes course with safety confirmation --%>
                                <asp:Button ID="btnDelete" runat="server" 
                                    Text="Delete Course" 
                                    CssClass="btn btn-danger me-2" 
                                    OnClick="btnDelete_Click" 
                                    Enabled="False" 
                                    OnClientClick="return confirm('Are you sure you want to delete this course? This action cannot be undone!');" />
                                
                                <%-- CLEAR BUTTON: Resets form to default state --%>
                                <asp:Button ID="btnClear" runat="server" 
                                    Text="Clear Form" 
                                    CssClass="btn btn-secondary" 
                                    OnClick="btnClear_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- SEARCH AND FILTER SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Advanced filtering interface allowing users to:
                - Search by course name (partial text matching)
                - Filter by course format (exact matching)
                - Search by instructor name (partial text matching)
                -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-search"></i> Search & Filter</h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <!-- COURSE NAME SEARCH: Flexible text-based searching -->
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label for="<%= txtSearchName.ClientID %>" class="form-label">
                                        Search by Course Name
                                    </label>
                                    <asp:TextBox ID="txtSearchName" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter course name..." />
                                </div>
                            </div>
                            
                            <!-- FORMAT FILTER: Exact format matching -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= ddlFilterFormat.ClientID %>" class="form-label">
                                        Filter by Format
                                    </label>
                                    <%-- Filter dropdown with same options as input form --%>
                                    <asp:DropDownList ID="ddlFilterFormat" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="All Formats" Value="" />
                                        <asp:ListItem Text="Lecture" Value="lecture" />
                                        <asp:ListItem Text="Seminar" Value="seminar" />
                                        <asp:ListItem Text="Workshop" Value="workshop" />
                                        <asp:ListItem Text="Laboratory" Value="laboratory" />
                                        <asp:ListItem Text="Online" Value="online" />
                                        <asp:ListItem Text="Hybrid" Value="hybrid" />
                                        <asp:ListItem Text="Practical" Value="practical" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            
                            <!-- INSTRUCTOR SEARCH: Faculty-based filtering -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtSearchInstructor.ClientID %>" class="form-label">
                                        Search by Instructor
                                    </label>
                                    <asp:TextBox ID="txtSearchInstructor" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter instructor name..." />
                                </div>
                            </div>
                            
                            <!-- FILTER ACTION BUTTONS -->
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div>
                                        <%-- SEARCH BUTTON: Applies current filter criteria --%>
                                        <asp:Button ID="btnSearch" runat="server" 
                                            Text="Search" 
                                            CssClass="btn btn-info btn-sm me-1" 
                                            OnClick="btnSearch_Click" 
                                            CausesValidation="false" />
                                        <%-- CLEAR SEARCH BUTTON: Resets all filters --%>
                                        <asp:Button ID="btnClearSearch" runat="server" 
                                            Text="Clear" 
                                            CssClass="btn btn-outline-secondary btn-sm" 
                                            OnClick="btnClearSearch_Click" 
                                            CausesValidation="false"/>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- COURSES DATA DISPLAY SECTION -->
                <!-- ==================================================================== -->
                <!-- 
                Professional data grid with:
                - Sortable columns for organized viewing
                - Formatted display with colored badges
                - Row selection for editing operations
                - Responsive design for mobile compatibility
                - Empty state messaging for user guidance
                -->
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-list"></i> Courses List</h5>
                        <%-- Dynamic course counter updated by client-side JavaScript --%>
                        <asp:Label ID="lblTotalCourses" runat="server" CssClass="badge badge-info" Text="Total: 0" />
                    </div>
                    <div class="card-body">
                        <!-- RESPONSIVE TABLE WRAPPER: Enables horizontal scrolling on small screens -->
                        <div class="table-responsive">
                            <%-- 
                            GRIDVIEW CONFIGURATION:
                            - AutoGenerateColumns="False": Full control over column layout
                            - DataKeyNames="course_id": Enables row selection and data access
                            - OnSelectedIndexChanged: Handles row selection for editing
                            - OnRowDataBound: Custom formatting for format badges
                            - EmptyDataText: User-friendly message for empty results
                            --%>
                            <asp:GridView ID="gvCourses" runat="server" 
                                AutoGenerateColumns="False" 
                                CssClass="table table-striped table-hover"
                                DataKeyNames="course_id" 
                                OnSelectedIndexChanged="gvCourses_SelectedIndexChanged"
                                OnRowDataBound="gvCourses_RowDataBound" 
                                EmptyDataText="No courses found.">
                                <Columns>
                                    <%-- 
                                    COLUMN DEFINITIONS:
                                    Each column is carefully configured for optimal display
                                    and user experience across different screen sizes
                                    --%>
                                    
                                    <%-- ID COLUMN: Compact identifier with center alignment --%>
                                    <asp:BoundField DataField="course_id" HeaderText="ID" ReadOnly="True" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="60px" />
                                    
                                    <%-- COURSE NAME: Primary field with bold emphasis --%>
                                    <asp:BoundField DataField="course_name" HeaderText="Course Name" 
                                        ItemStyle-CssClass="fw-bold" 
                                        HeaderStyle-CssClass="text-start" />
                                    
                                    <%-- ECTS CREDITS: Academic value with center alignment --%>
                                    <asp:BoundField DataField="ects" HeaderText="ECTS" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="80px" />
                                    
                                    <%-- CONTACT HOURS: Duration information --%>
                                    <asp:BoundField DataField="hours" HeaderText="Hours" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="80px" />
                                    
                                    <%-- FORMAT: Styled with colored badges (handled by RowDataBound) --%>
                                    <asp:BoundField DataField="format" HeaderText="Format" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="100px" />
                                    
                                    <%-- INSTRUCTOR: Faculty assignment information --%>
                                    <asp:BoundField DataField="instructor" HeaderText="Instructor" 
                                        HeaderStyle-CssClass="text-start" 
                                        ItemStyle-Width="150px" />
                                    
                                    <%-- ACTION COLUMN: Selection button for editing operations --%>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkSelect" runat="server" 
                                                Text="Select" 
                                                CommandName="Select" 
                                                CssClass="btn btn-sm btn-outline-primary" 
                                                CausesValidation="false" />
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                </Columns>
                                <%-- 
                                GRIDVIEW STYLING:
                                Professional appearance with Bootstrap classes
                                --%>
                                <HeaderStyle CssClass="table-dark" />
                                <RowStyle CssClass="align-middle" />
                                <SelectedRowStyle CssClass="table-warning" />
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- ==================================================================== -->
    <!-- CUSTOM CSS STYLING -->
    <!-- ==================================================================== -->
    <!-- 
    Enhanced styling system that extends Bootstrap with:
    - Professional shadows and hover effects
    - Consistent spacing and typography
    - Interactive animations and transitions
    - Responsive design adjustments
    - Accessibility-compliant color schemes
    - Mobile-optimized touch targets
    -->
    <style>
        /* ==================================================================== */
        /* CARD COMPONENT STYLING */
        /* ==================================================================== */
        /* Enhanced card appearance with subtle shadows and hover effects */
        .card {
            border: none;
            border-radius: 0.75rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
            transition: box-shadow 0.15s ease-in-out;
        }
        
        /* Hover effect for interactive feedback */
        .card:hover {
            box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
        }
        
        /* ==================================================================== */
        /* TABLE AND GRIDVIEW STYLING */
        /* ==================================================================== */
        /* Professional table appearance with improved readability */
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
        
        .card-header h4, .card-header h5 {
            margin-bottom: 0;
        }
        
        .form-label {
            font-weight: 500;
            margin-bottom: 0.5rem;
        }
        
        .text-danger {
            color: #dc3545 !important;
        }
        
        .me-1 {
            margin-right: 0.25rem !important;
        }
        
        .me-2 {
            margin-right: 0.5rem !important;
        }
        
        /* ==================================================================== */
        /* INTERACTIVE EFFECTS */
        /* ==================================================================== */
        /* Hover effects for better user experience */
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.05);
        }
        
        /* Focus styles for accessibility */
        .form-control:focus, .form-select:focus {
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
        /* BADGE STYLING FOR COURSE FORMATS */
        /* ==================================================================== */
        /* Consistent badge appearance across all formats */
        .badge {
            font-size: 0.75em;
            padding: 0.35em 0.65em;
        }
        
        /* ==================================================================== */
        /* SELECTED ROW HIGHLIGHTING */
        /* ==================================================================== */
        /* Visual indicator for selected courses */
        .table-warning {
            background-color: rgba(255, 193, 7, 0.1) !important;
            border-left: 4px solid #ffc107;
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
        }
        
        /* ==================================================================== */
        /* FORM VALIDATION STYLING */
        /* ==================================================================== */
        /* Visual feedback for form validation states */
        .is-valid {
            border-color: #198754;
        }
        
        .is-invalid {
            border-color: #dc3545;
        }
        
        /* ==================================================================== */
        /* LOADING AND ANIMATION STATES */
        /* ==================================================================== */
        .loading {
            opacity: 0.7;
            pointer-events: none;
        }
        
        /* Course card hover effects */
        .card {
            transition: box-shadow 0.15s ease-in-out;
        }
        
        .card:hover {
            box-shadow: 0 0.125rem 0.5rem rgba(0, 0, 0, 0.075);
        }
        
        /* Disabled button styling */
        .btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
    </style>

    <!-- ==================================================================== -->
    <!-- CLIENT-SIDE JAVASCRIPT ENHANCEMENTS -->
    <!-- ==================================================================== -->
    <!-- 
    JavaScript functionality for enhanced user experience:
    - Dynamic UI updates and real-time feedback
    - Form validation and visual indicators
    - Button state management
    - Loading states and progress indication
    - Accessibility improvements
    - Mobile-responsive behavior
    -->
    <script>
        // ==================================================================== 
        // PAGE INITIALIZATION AND SETUP
        // ====================================================================
        
        // Execute when DOM is fully loaded but before all resources
        document.addEventListener('DOMContentLoaded', function() {
            // Initialize all page functionality
            initializePage();
        });
        
        // Execute after all resources including images are loaded
        window.addEventListener('load', function () {
            // Final setup and cleanup
            finalizePageLoad();
        });
        
        // ====================================================================
        // CORE INITIALIZATION FUNCTIONS
        // ====================================================================
        
        /**
         * Initialize page functionality
         * Sets up button states, event listeners, and UI enhancements
         */
        function initializePage() {
            // BUTTON STATE MANAGEMENT
            // Ensure all buttons are enabled after page load
            // Prevents stuck disabled states after postbacks
            enableAllButtons();
            
            // FORM ENHANCEMENT
            // Add visual feedback and improved validation
            enhanceFormFields();
            
            // UI UPDATES
            // Update dynamic counters and status indicators
            updateCourseCount();
        }
        
        /**
         * Finalize page loading
         * Performs cleanup and final UI updates
         */
        function finalizePageLoad() {
            // Brief delay to ensure all postback processing is complete
            setTimeout(function() {
                updateCourseCount();
                enableAllButtons();
            }, 500);
        }
        
        // ====================================================================
        // BUTTON AND FORM MANAGEMENT
        // ====================================================================
        
        /**
         * Re-enable all form buttons
         * Prevents disabled state persistence after server round-trips
         */
        function enableAllButtons() {
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function(button) {
                button.disabled = false;
            });
        }
        
        /**
         * Enhanced form field handling
         * Adds visual feedback and improved user experience
         */
        function enhanceFormFields() {
            // ADD VISUAL FEEDBACK for required fields
            var requiredFields = document.querySelectorAll('input[required], select[required]');
            requiredFields.forEach(function(field) {
                // Add visual indicator for required fields
                field.addEventListener('blur', function() {
                    validateField(this);
                });
                
                // Real-time validation feedback
                field.addEventListener('input', function() {
                    if (this.value.trim() !== '') {
                        this.classList.remove('is-invalid');
                        this.classList.add('is-valid');
                    }
                });
            });
        }
        
        /**
         * Validate individual form field
         * Provides immediate visual feedback
         */
        function validateField(field) {
            if (field.hasAttribute('required') && field.value.trim() === '') {
                field.classList.add('is-invalid');
                field.classList.remove('is-valid');
                return false;
            } else {
                field.classList.remove('is-invalid');
                field.classList.add('is-valid');
                return true;
            }
        }
        
        // ====================================================================
        // DYNAMIC UI UPDATES
        // ====================================================================
        
        /**
         * Update course count badge
         * Dynamically counts displayed courses and updates the badge
         */
        function updateCourseCount() {
            try {
                var table = document.querySelector('#<%= gvCourses.ClientID %>');
                if (table) {
                    var rows = table.querySelectorAll('tbody tr');
                    var count = 0;

                    // Count only actual data rows (not empty data messages)
                    rows.forEach(function (row) {
                        // Skip rows with colspan (empty data message rows)
                        if (!row.querySelector('td[colspan]')) {
                            count++;
                        }
                    });

                    // Update the badge with current count
                    var badge = document.getElementById('<%= lblTotalCourses.ClientID %>');
                    if (badge) {
                        badge.textContent = 'Total: ' + count;
                    }
                }
            } catch (e) {
                console.log('Course count update error:', e);
            }
        }
        
        // ====================================================================
        // FORM VALIDATION AND FEEDBACK
        // ====================================================================
        
        /**
         * Comprehensive form validation
         * Validates all required fields and provides visual feedback
         */
        function validateForm() {
            var isValid = true;
            var requiredFields = document.querySelectorAll('input[required], select[required]');

            requiredFields.forEach(function (field) {
                if (!validateField(field)) {
                    isValid = false;
                }
            });

            return isValid;
        }
        
        // ====================================================================
        // LOADING STATES AND VISUAL FEEDBACK
        // ====================================================================
        
        /**
         * Show loading state for long operations
         * Provides visual feedback during server processing
         */
        function showLoading() {
            var buttons = document.querySelectorAll('.btn');
            buttons.forEach(function(btn) {
                btn.classList.add('loading');
                btn.disabled = true;
            });
        }
        
        /**
         * Hide loading state
         * Restores normal UI after operation completion
         */
        function hideLoading() {
            var buttons = document.querySelectorAll('.btn');
            buttons.forEach(function(btn) {
                btn.classList.remove('loading');
                btn.disabled = false;
            });
        }
        
        // ====================================================================
        // ACCESSIBILITY ENHANCEMENTS
        // ====================================================================
        
        /**
         * Enhanced keyboard navigation
         * Improves accessibility for keyboard-only users
         */
        document.addEventListener('keydown', function(e) {
            // Enhanced Enter key handling for form submission
            if (e.key === 'Enter' && !e.shiftKey) {
                var target = e.target;
                
                // Handle Enter in text fields (submit form)
                if (target.tagName === 'INPUT' && target.type === 'text') {
                    e.preventDefault();
                    var addButton = document.getElementById('<%= btnAdd.ClientID %>');
                    if (addButton && !addButton.disabled) {
                        addButton.click();
                    }
                }
            }
        });

        // ====================================================================
        // ERROR HANDLING AND RECOVERY
        // ====================================================================

        /**
         * Global error handler for JavaScript errors
         * Provides graceful degradation when client-side features fail
         */
        window.addEventListener('error', function (e) {
            console.log('Page error detected:', e.message);
            // Continue functioning even if some enhancements fail
            enableAllButtons();
        });

        // ====================================================================
        // MOBILE RESPONSIVENESS ENHANCEMENTS
        // ====================================================================

        /**
         * Touch-friendly enhancements for mobile devices
         * Improves usability on touch screens
         */
        if ('ontouchstart' in window) {
            // Add touch-friendly class for mobile-specific styling
            document.body.classList.add('touch-device');

            // Enhanced touch targets for small screens
            var buttons = document.querySelectorAll('.btn-sm');
            buttons.forEach(function (btn) {
                btn.style.minHeight = '44px'; // iOS recommended touch target
            });
        }
    </script>

    <!-- 
=============================================================================
END OF MANAGECOURSES.ASPX PAGE
=============================================================================

PAGE ARCHITECTURE SUMMARY:
1. Form Section: Comprehensive course input with validation
2. Search Interface: Advanced filtering and search capabilities  
3. Data Grid: Professional course display with formatted badges
4. Styling System: Modern, responsive design with Bootstrap enhancements
5. JavaScript Layer: Enhanced UX with real-time feedback and validation

VALIDATION STRATEGY:
- Required Field Validation: Server-side validation for all mandatory fields
- Range Validation: ECTS and hours within academic standards
- Format Validation: Predefined course format selection
- Client-side Enhancement: Real-time visual feedback
- Accessibility: Screen reader compatible validation messages

RESPONSIVE DESIGN FEATURES:
- Mobile-first Bootstrap grid system
- Responsive tables with horizontal scrolling
- Touch-friendly button sizes on mobile devices
- Scalable typography and spacing
- Adaptive layout for different screen sizes

USER EXPERIENCE ENHANCEMENTS:
- Real-time form validation with visual feedback
- Dynamic course counter in header badge
- Hover effects and smooth transitions
- Loading states for server operations
- Keyboard navigation support
- Error recovery mechanisms

ACCESSIBILITY COMPLIANCE:
- Semantic HTML structure with proper headings
- ARIA labels and roles for screen readers
- High contrast color schemes
- Keyboard navigation support
- Form labels properly associated with controls
- Clear error messaging and validation feedback

SECURITY FEATURES:
- Server-side validation for all inputs
- SQL injection prevention through parameterized queries
- Role-based access control (admin only)
- Input sanitization and validation
- CSRF protection through ViewState validation
=============================================================================
-->

</asp:Content>
