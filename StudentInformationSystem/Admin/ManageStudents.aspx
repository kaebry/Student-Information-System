<%@ Page Title="Manage Students" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ManageStudents.aspx.vb" Inherits="StudentInformationSystem.ManageStudents" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- 
    =============================================================================
    MANAGE STUDENTS PAGE - ASP.NET WEB FORMS PAGE
    =============================================================================
    Purpose: Provides a comprehensive interface for CRUD operations on student records
    Features: Add, Edit, Delete, and View students with form validation and error handling

    Key Components:
    - Student input form with validation
    - GridView for displaying student records  
    - Message system for user feedback
    - Responsive Bootstrap design
    - Client-side form validation and UX enhancements
    =============================================================================
    -->

    <!-- MAIN CONTAINER: Bootstrap fluid container for full-width layout -->
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <!-- PAGE HEADER: Title with icon for visual appeal -->
                <h2 class="text-center mb-4">
                    <i class="fas fa-user-graduate"></i> Manage Students
                </h2>

                <!-- MESSAGE DISPLAY AREA: Shows success/error messages from server operations -->
                <!-- Server control updates this dynamically based on operation results -->
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="False" />

                <!-- ==================================================================== -->
                <!-- STUDENT INPUT FORM SECTION -->
                <!-- ==================================================================== -->
                <!-- Card layout provides visual separation and professional appearance -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">
                            <i class="fas fa-plus-circle"></i> Student Information
                        </h5>
                    </div>
                    <div class="card-body">
                        <!-- FORM FIELDS ROW: 4-column responsive layout -->
                        <!-- Bootstrap grid system ensures fields stack vertically on mobile -->
                        <div class="row">
                            
                            <!-- FIRST NAME FIELD -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtFirstName.ClientID %>" class="form-label">
                                        First Name <span class="text-danger">*</span>
                                    </label>
                                    <!-- TextBox with Bootstrap styling and validation attributes -->
                                    <asp:TextBox ID="txtFirstName" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter first name" 
                                        MaxLength="50" />
                                    <!-- Server-side validation: Ensures field is not empty -->
                                    <!-- ValidationGroup prevents validation during Delete/Clear operations -->
                                    <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" 
                                        ControlToValidate="txtFirstName" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="First name is required." 
                                        Display="Dynamic" 
                                        ValidationGroup="StudentForm" />
                                </div>
                            </div>
                            
                            <!-- LAST NAME FIELD -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtLastName.ClientID %>" class="form-label">
                                        Last Name <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtLastName" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter last name" 
                                        MaxLength="50" />
                                    <!-- Validation ensures last name is provided -->
                                    <asp:RequiredFieldValidator ID="rfvLastName" runat="server" 
                                        ControlToValidate="txtLastName" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Last name is required." 
                                        Display="Dynamic" 
                                        ValidationGroup="StudentForm" />
                                </div>
                            </div>
                            
                            <!-- EMAIL FIELD -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtEmail.ClientID %>" class="form-label">
                                        Email <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtEmail" runat="server" 
                                        CssClass="form-control" 
                                        placeholder="Enter email address" />
                                    <!-- DUAL VALIDATION: Required field + Email format validation -->
                                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                                        ControlToValidate="txtEmail" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Email is required." 
                                        Display="Dynamic" 
                                        ValidationGroup="StudentForm" />
                                    <!-- Regular expression validator ensures proper email format -->
                                    <asp:RegularExpressionValidator ID="revEmail" runat="server" 
                                        ControlToValidate="txtEmail" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Please enter a valid email address." 
                                        ValidationExpression="^[\w\.-]+@[\w\.-]+\.\w+$" 
                                        Display="Dynamic" 
                                        ValidationGroup="StudentForm" />
                                </div>
                            </div>
                            
                            <!-- ENROLLMENT DATE FIELD -->
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtEnrollmentDate.ClientID %>" class="form-label">
                                        Enrollment Date <span class="text-danger">*</span>
                                    </label>
                                    <!-- HTML5 Date input provides native date picker -->
                                    <asp:TextBox ID="txtEnrollmentDate" runat="server" 
                                        TextMode="Date" 
                                        CssClass="form-control" />
                                    <asp:RequiredFieldValidator ID="rfvEnrollmentDate" runat="server" 
                                        ControlToValidate="txtEnrollmentDate" 
                                        CssClass="text-danger small" 
                                        ErrorMessage="Enrollment date is required." 
                                        Display="Dynamic" 
                                        ValidationGroup="StudentForm" />
                                </div>
                            </div>
                        </div>

                        <!-- ==================================================================== -->
                        <!-- ACTION BUTTONS SECTION -->
                        <!-- ==================================================================== -->
                        <!-- Button row with different validation behaviors for different operations -->
                        <div class="row">
                            <div class="col-md-12">
                                <!-- ADD BUTTON: Triggers validation and creates new student -->
                                <asp:Button ID="btnAdd" runat="server" 
                                    Text="Add Student" 
                                    CssClass="btn btn-primary me-2" 
                                    OnClick="btnCreate_Click" 
                                    ValidationGroup="StudentForm" 
                                    OnClientClick="return confirm('Are you sure you want to add this student?');" />
                                
                                <!-- UPDATE BUTTON: Only enabled when student is selected -->
                                <!-- Triggers validation and updates existing student record -->
                                <asp:Button ID="btnUpdate" runat="server" 
                                    Text="Update Student" 
                                    CssClass="btn btn-warning me-2" 
                                    OnClick="btnUpdate_Click" 
                                    ValidationGroup="StudentForm" 
                                    Enabled="False" 
                                    OnClientClick="return confirm('Are you sure you want to update this student?');" />
                                
                                <!-- DELETE BUTTON: No validation (CausesValidation="False") -->
                                <!-- Allows deletion even with invalid form data for cleanup scenarios -->
                                <asp:Button ID="btnDelete" runat="server" 
                                    Text="Delete Student" 
                                    CssClass="btn btn-danger me-2" 
                                    OnClick="btnDelete_Click" 
                                    CausesValidation="False" 
                                    Enabled="False" 
                                    OnClientClick="return confirm('Are you sure you want to delete this student? This action cannot be undone!');" />
                                
                                <!-- CLEAR BUTTON: Resets form without validation -->
                                <asp:Button ID="btnClear" runat="server" 
                                    Text="Clear Form" 
                                    CssClass="btn btn-secondary" 
                                    OnClick="btnClear_Click" 
                                    CausesValidation="False" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ==================================================================== -->
                <!-- STUDENTS LIST SECTION -->
                <!-- ==================================================================== -->
                <!-- Card container for the students data grid -->
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-list"></i> Students List</h5>
                        <!-- DYNAMIC COUNTER: JavaScript updates this to show current record count -->
                        <span class="badge bg-info" id="studentCount">Loading...</span>
                    </div>
                    <div class="card-body">
                        <!-- RESPONSIVE TABLE WRAPPER: Enables horizontal scrolling on small screens -->
                        <div class="table-responsive">
                            <!-- ==================================================================== -->
                            <!-- GRIDVIEW CONTROL: Core data display component -->
                            <!-- ==================================================================== -->
                            <!-- 
                            Key Features:
                            - AutoGenerateColumns="False": Gives full control over column layout
                            - DataKeyNames="id": Enables row selection and data key access
                            - OnSelectedIndexChanged: Handles row selection for edit operations
                            - EmptyDataText: User-friendly message when no data exists
                            - GridLines="None": Clean, modern appearance without grid lines
                            -->
                            <!-- 
                            GRIDVIEW COLUMNS DEFINITION:
                            - ID Column: Read-only identifier with center alignment
                            - Name Columns: Bold text for emphasis and easy scanning  
                            - Email Column: Fixed width to prevent layout issues
                            - Date Column: Formatted display (dd/MM/yyyy) with center alignment
                            - Action Column: Template field with Select button
                            -->
                            <asp:GridView ID="gvStudents" runat="server" 
                                AutoGenerateColumns="False" 
                                CssClass="table table-striped table-hover"
                                DataKeyNames="id" 
                                OnSelectedIndexChanged="gvStudents_SelectedIndexChanged" 
                                EmptyDataText="No students found. Add your first student using the form above."
                                GridLines="None">
                                <Columns>
                                    <asp:BoundField DataField="id" HeaderText="ID" ReadOnly="True" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="60px" />
                                    
                                    <asp:BoundField DataField="first_name" HeaderText="First Name" 
                                        ItemStyle-CssClass="fw-bold" 
                                        HeaderStyle-CssClass="text-start" />
                                    
                                    <asp:BoundField DataField="last_name" HeaderText="Last Name" 
                                        ItemStyle-CssClass="fw-bold" 
                                        HeaderStyle-CssClass="text-start" />
                                    
                                    <asp:BoundField DataField="email" HeaderText="Email" 
                                        HeaderStyle-CssClass="text-start" 
                                        ItemStyle-Width="250px" />
                                    
                                    <asp:BoundField DataField="enrollment_date" HeaderText="Enrollment Date" 
                                        DataFormatString="{0:dd/MM/yyyy}" 
                                        ItemStyle-CssClass="text-center" 
                                        HeaderStyle-CssClass="text-center" 
                                        ItemStyle-Width="120px" />
                                    
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <%-- SELECT BUTTON: Triggers row selection event --%>
                                            <%-- CausesValidation="false" prevents form validation during selection --%>
                                            <asp:LinkButton ID="lnkSelect" runat="server" 
                                                Text="Select" 
                                                CommandName="Select" 
                                                CssClass="btn btn-sm btn-outline-primary" 
                                                CausesValidation="false"/>
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                </Columns>
                                <%-- 
                                GRIDVIEW STYLING: Bootstrap classes for professional appearance
                                - HeaderStyle: Dark theme for headers
                                - RowStyle: Vertical alignment for content
                                - SelectedRowStyle: Highlight selected rows
                                - EmptyDataRowStyle: Styling for "no data" message
                                --%>
                                <HeaderStyle CssClass="table-dark" />
                                <RowStyle CssClass="align-middle" />
                                <SelectedRowStyle CssClass="table-warning" />
                                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- ==================================================================== -->
    <!-- LOADING OVERLAY: Shows during form submissions -->
    <!-- ==================================================================== -->
    <!-- Hidden by default, shown via JavaScript during operations -->
    <div id="loadingOverlay" style="display: none;">
        <div class="d-flex justify-content-center align-items-center h-100">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    </div>

    <!-- ==================================================================== -->
    <!-- CUSTOM CSS STYLING -->
    <!-- ==================================================================== -->
    <!-- 
    CSS Purpose:
    - Enhances Bootstrap styles with custom animations and effects
    - Provides responsive design adjustments
    - Creates professional hover effects and transitions
    - Ensures consistent visual appearance across devices
    -->
    <style>
        /* CARD STYLING: Enhanced shadows and hover effects */
        .card {
            border: none;
            border-radius: 0.75rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
            transition: box-shadow 0.15s ease-in-out;
        }
        
        .card:hover {
            box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
        }
        
        /* TABLE STYLING: Clean, modern appearance */
        .table th {
            border-top: none;
            font-weight: 600;
            font-size: 0.9em;
            vertical-align: middle;
        }
        
        .table td {
            font-size: 0.9em;
            vertical-align: middle;
            border-color: rgba(0,0,0,0.1);
        }
        
        /* UTILITY CLASSES: Font weights and spacing */
        .fw-bold {
            font-weight: 600 !important;
        }
        
        .btn-sm {
            padding: 0.25rem 0.5rem;
            font-size: 0.8rem;
        }
        
        .card-header h5 {
            margin-bottom: 0;
        }
        
        .form-label {
            font-weight: 500;
            margin-bottom: 0.5rem;
        }
        
        .text-danger {
            color: #dc3545 !important;
        }
        
        .me-2 {
            margin-right: 0.5rem !important;
        }
        
        /* INTERACTIVE EFFECTS: Hover states for better UX */
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.05);
        }
        
        /* FORM FOCUS STYLING: Visual feedback for active fields */
        .form-control:focus {
            border-color: #0d6efd;
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
        }
        
        /* BUTTON ANIMATIONS: Subtle movement on hover */
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
        
        /* SELECTED ROW HIGHLIGHTING: Visual indicator for selected records */
        .table-warning {
            background-color: rgba(255, 193, 7, 0.1) !important;
            border-left: 4px solid #ffc107;
        }
        
        /* LOADING OVERLAY: Full-screen loading indicator */
        #loadingOverlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(255, 255, 255, 0.8);
            z-index: 9999;
        }
        
        /* ALERT STYLING: Consistent message appearance */
        .alert {
            border-radius: 0.5rem;
            border: none;
            margin-bottom: 1rem;
        }
        
        /* RESPONSIVE DESIGN: Mobile-friendly adjustments */
        @media (max-width: 768px) {
            .btn-sm {
                font-size: 0.7rem;
                padding: 0.2rem 0.4rem;
            }
            
            .table {
                font-size: 0.8rem;
            }
            
            .card-body {
                padding: 1rem;
            }
        }
        
        /* FORM VALIDATION STYLING: Visual feedback for valid/invalid fields */
        .is-valid {
            border-color: #198754;
        }
        
        .is-invalid {
            border-color: #dc3545;
        }
    </style>

    <!-- ==================================================================== -->
    <!-- CLIENT-SIDE JAVASCRIPT -->
    <!-- ==================================================================== -->
    <!-- 
    JavaScript Functions:
    - Enhances user experience with dynamic updates
    - Handles form validation and visual feedback
    - Manages loading states and UI interactions
    - Provides responsive behavior and accessibility
    -->
    <script>
        // PAGE INITIALIZATION: Executes when DOM is fully loaded
        document.addEventListener('DOMContentLoaded', function() {
            // Update the student count badge in the header
            updateStudentCount();
            
            // BUTTON STATE MANAGEMENT: Re-enable buttons after page load
            // Prevents disabled state from persisting after postbacks
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function(button) {
                button.disabled = false;
            });

            // VALIDATION BYPASS SETUP: Special handling for Delete and Clear buttons
            // These operations should not trigger form validation
            var deleteBtn = document.getElementById('<%= btnDelete.ClientID %>');
            var clearBtn = document.getElementById('<%= btnClear.ClientID %>');
            
            // DELETE BUTTON: Temporarily disable HTML5 validation
            if (deleteBtn) {
                deleteBtn.addEventListener('click', function(e) {
                    var form = this.closest('form');
                    if (form) {
                        form.setAttribute('novalidate', 'true');
                        // Re-enable validation after postback processing
                        setTimeout(function() {
                            form.removeAttribute('novalidate');
                        }, 100);
                    }
                });
            }

            // CLEAR BUTTON: Same validation bypass as delete
            if (clearBtn) {
                clearBtn.addEventListener('click', function(e) {
                    var form = this.closest('form');
                    if (form) {
                        form.setAttribute('novalidate', 'true');
                        setTimeout(function() {
                            form.removeAttribute('novalidate');
                        }, 100);
                    }
                });
            }
        });

        // STUDENT COUNT UPDATE: Dynamically counts and displays record total
        function updateStudentCount() {
            try {
                var table = document.querySelector('#<%= gvStudents.ClientID %>');
                if (table) {
                    var rows = table.querySelectorAll('tbody tr');
                    var count = 0;

                    // Count only actual data rows (not empty data message rows)
                    rows.forEach(function (row) {
                        // Skip rows with colspan (empty data messages)
                        if (!row.querySelector('td[colspan]')) {
                            count++;
                        }
                    });

                    // Update the badge with current count
                    var badge = document.getElementById('studentCount');
                    if (badge) {
                        badge.textContent = 'Total: ' + count;
                    }
                }
            } catch (e) {
                console.log('Count update error:', e);
            }
        }

        // LOADING STATE MANAGEMENT: Visual feedback during operations
        function showLoading() {
            document.getElementById('loadingOverlay').style.display = 'block';
        }

        function hideLoading() {
            document.getElementById('loadingOverlay').style.display = 'none';
        }

        // POST-LOAD CLEANUP: Ensures UI is ready after page fully loads
        window.addEventListener('load', function () {
            // Brief delay to ensure all elements are rendered
            setTimeout(hideLoading, 500);
            updateStudentCount();
        });

        // FORM VALIDATION FEEDBACK: Visual indicators for field validation
        function validateForm() {
            var isValid = true;
            var requiredFields = document.querySelectorAll('input[data-val-required]');

            requiredFields.forEach(function (field) {
                if (field.value.trim() === '') {
                    field.classList.add('is-invalid');
                    isValid = false;
                } else {
                    field.classList.remove('is-invalid');
                    field.classList.add('is-valid');
                }
            });

            return isValid;
        }
    </script>

<!-- 
=============================================================================
END OF MANAGESTUDENTS.ASPX PAGE
=============================================================================

PAGE ARCHITECTURE SUMMARY:
1. Form Section: Input fields with comprehensive validation
2. Action Buttons: CRUD operations with appropriate validation groups
3. Data Grid: Professional display of student records with selection
4. Styling: Modern, responsive design with Bootstrap and custom CSS
5. JavaScript: Enhanced UX with dynamic updates and visual feedback

VALIDATION STRATEGY:
- ValidationGroup="StudentForm" for Add/Update operations
- CausesValidation="False" for Delete/Clear operations
- Client-side validation for immediate feedback
- Server-side validation for security and data integrity

RESPONSIVE DESIGN:
- Bootstrap grid system for mobile-friendly layout
- Responsive tables with horizontal scrolling
- Scalable typography and spacing
- Touch-friendly button sizes on mobile devices

ACCESSIBILITY FEATURES:
- Semantic HTML structure with proper labels
- ARIA attributes for screen readers
- Keyboard navigation support
- High contrast color schemes
- Clear visual feedback for interactions
=============================================================================
-->
</asp:Content>
