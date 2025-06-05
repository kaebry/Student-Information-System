<%@ Page Title="Manage Students" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ManageStudents.aspx.vb" Inherits="StudentInformationSystem.ManageStudents" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <h2 class="text-center mb-4">
                    <i class="fas fa-user-graduate"></i> Manage Students
                </h2>

                <!-- Messages -->
                <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="False" />

                <!-- Student Form -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">
                            <i class="fas fa-plus-circle"></i> Student Information
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtFirstName.ClientID %>" class="form-label">First Name <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" placeholder="Enter first name" MaxLength="50" />
                                    <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName" 
                                        CssClass="text-danger small" ErrorMessage="First name is required." Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtLastName.ClientID %>" class="form-label">Last Name <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" placeholder="Enter last name" MaxLength="50" />
                                    <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName" 
                                        CssClass="text-danger small" ErrorMessage="Last name is required." Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtEmail.ClientID %>" class="form-label">Email <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter email address" />
                                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                                        CssClass="text-danger small" ErrorMessage="Email is required." Display="Dynamic" />
                                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" 
                                        CssClass="text-danger small" ErrorMessage="Please enter a valid email address." 
                                        ValidationExpression="^[\w\.-]+@[\w\.-]+\.\w+$" Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtEnrollmentDate.ClientID %>" class="form-label">Enrollment Date <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtEnrollmentDate" runat="server" TextMode="Date" CssClass="form-control" />
                                    <asp:RequiredFieldValidator ID="rfvEnrollmentDate" runat="server" ControlToValidate="txtEnrollmentDate" 
                                        CssClass="text-danger small" ErrorMessage="Enrollment date is required." Display="Dynamic" />
                                </div>
                            </div>
                        </div>

                        <!-- Action Buttons -->
                        <div class="row">
                            <div class="col-md-12">
                                <asp:Button ID="btnAdd" runat="server" Text="Add Student" CssClass="btn btn-primary me-2" OnClick="btnCreate_Click" 
                                    OnClientClick="return confirm('Are you sure you want to add this student?');" />
                                <asp:Button ID="btnUpdate" runat="server" Text="Update Student" CssClass="btn btn-warning me-2" OnClick="btnUpdate_Click" 
                                    Enabled="False" OnClientClick="return confirm('Are you sure you want to update this student?');" />
                                <asp:Button ID="btnDelete" runat="server" Text="Delete Student" CssClass="btn btn-danger me-2" OnClick="btnDelete_Click" 
                                    Enabled="False" OnClientClick="return confirm('Are you sure you want to delete this student? This action cannot be undone!');" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear Form" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Students List -->
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-list"></i> Students List</h5>
                        <span class="badge bg-info" id="studentCount">Loading...</span>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <asp:GridView ID="gvStudents" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-hover"
                                DataKeyNames="id" OnSelectedIndexChanged="gvStudents_SelectedIndexChanged" 
                                EmptyDataText="No students found. Add your first student using the form above."
                                GridLines="None">
                                <Columns>
                                    <asp:BoundField DataField="id" HeaderText="ID" ReadOnly="True" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="60px" />
                                    <asp:BoundField DataField="first_name" HeaderText="First Name" 
                                        ItemStyle-CssClass="fw-bold" HeaderStyle-CssClass="text-start" />
                                    <asp:BoundField DataField="last_name" HeaderText="Last Name" 
                                        ItemStyle-CssClass="fw-bold" HeaderStyle-CssClass="text-start" />
                                    <asp:BoundField DataField="email" HeaderText="Email" 
                                        HeaderStyle-CssClass="text-start" ItemStyle-Width="250px" />
                                    <asp:BoundField DataField="enrollment_date" HeaderText="Enrollment Date" 
                                        DataFormatString="{0:dd/MM/yyyy}" ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="120px" />
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkSelect" runat="server" Text="Select" CommandName="Select" 
                                                CssClass="btn btn-sm btn-outline-primary" CausesValidation="false"/>
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                </Columns>
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

    <!-- Loading Overlay -->
    <div id="loadingOverlay" style="display: none;">
        <div class="d-flex justify-content-center align-items-center h-100">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    </div>

    <style>
        .card {
            border: none;
            border-radius: 0.75rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
            transition: box-shadow 0.15s ease-in-out;
        }
        
        .card:hover {
            box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
        }
        
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
        
        /* Hover effects */
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.05);
        }
        
        /* Focus styles */
        .form-control:focus {
            border-color: #0d6efd;
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
        }
        
        /* Button animations */
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
        
        /* Selected row highlighting */
        .table-warning {
            background-color: rgba(255, 193, 7, 0.1) !important;
            border-left: 4px solid #ffc107;
        }
        
        /* Loading overlay */
        #loadingOverlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(255, 255, 255, 0.8);
            z-index: 9999;
        }
        
        /* Alert styling */
        .alert {
            border-radius: 0.5rem;
            border: none;
            margin-bottom: 1rem;
        }
        
        /* Responsive adjustments */
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
        
        /* Success/error states for form fields */
        .is-valid {
            border-color: #198754;
        }
        
        .is-invalid {
            border-color: #dc3545;
        }
    </style>

    <script>
        // Page load improvements
        document.addEventListener('DOMContentLoaded', function() {
            // Update student count badge
            updateStudentCount();
            
            // Re-enable buttons on page load
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function(button) {
                button.disabled = false;
            });
        });

        // Update student count in badge
        function updateStudentCount() {
            try {
                var table = document.querySelector('#<%= gvStudents.ClientID %>');
                if (table) {
                    var rows = table.querySelectorAll('tbody tr');
                    var count = 0;
                    
                    // Count only data rows (not empty data row)
                    rows.forEach(function(row) {
                        if (!row.querySelector('td[colspan]')) {
                            count++;
                        }
                    });
                    
                    var badge = document.getElementById('studentCount');
                    if (badge) {
                        badge.textContent = 'Total: ' + count;
                    }
                }
            } catch (e) {
                console.log('Count update error:', e);
            }
        }

        // Show loading for form submissions
        function showLoading() {
            document.getElementById('loadingOverlay').style.display = 'block';
        }

        // Hide loading
        function hideLoading() {
            document.getElementById('loadingOverlay').style.display = 'none';
        }

        // Auto-hide loading after page load
        window.addEventListener('load', function() {
            setTimeout(hideLoading, 500);
            updateStudentCount();
        });

        // Form validation feedback
        function validateForm() {
            var isValid = true;
            var requiredFields = document.querySelectorAll('input[data-val-required]');
            
            requiredFields.forEach(function(field) {
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
</asp:Content>
