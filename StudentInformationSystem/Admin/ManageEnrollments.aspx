<%@ Page Title="Manage Enrollments" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ManageEnrollments.aspx.vb" Inherits="StudentInformationSystem.ManageEnrollments" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <h2 class="text-center mb-4">
                    <i class="fas fa-clipboard-list"></i> Manage Enrollments
                </h2>
                
                <!-- Messages -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <!-- Statistics Cards -->
                <div class="row mb-4">
                    <div class="col-md-3">
                        <div class="card bg-primary text-white">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Total Enrollments</h6>
                                        <h3><asp:Label ID="lblTotalEnrollments" runat="server" Text="0" /></h3>
                                    </div>
                                    <div class="align-self-center">
                                        <i class="fas fa-clipboard-list fa-2x"></i>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card bg-success text-white">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Active Students</h6>
                                        <h3><asp:Label ID="lblActiveStudents" runat="server" Text="0" /></h3>
                                    </div>
                                    <div class="align-self-center">
                                        <i class="fas fa-user-graduate fa-2x"></i>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card bg-info text-white">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Active Courses</h6>
                                        <h3><asp:Label ID="lblActiveCourses" runat="server" Text="0" /></h3>
                                    </div>
                                    <div class="align-self-center">
                                        <i class="fas fa-book fa-2x"></i>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="card bg-warning text-dark">
                            <div class="card-body">
                                <div class="d-flex justify-content-between">
                                    <div>
                                        <h6 class="card-title">Avg. Enrollments/Student</h6>
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

                <!-- Search and Filter -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-search"></i> Search & Filter Enrollments</h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtSearchStudent.ClientID %>" class="form-label">Search by Student Name</label>
                                    <asp:TextBox ID="txtSearchStudent" runat="server" CssClass="form-control" placeholder="Enter student name..." />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtSearchCourse.ClientID %>" class="form-label">Search by Course Name</label>
                                    <asp:TextBox ID="txtSearchCourse" runat="server" CssClass="form-control" placeholder="Enter course name..." />
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label for="<%= txtDateFrom.ClientID %>" class="form-label">From Date</label>
                                    <asp:TextBox ID="txtDateFrom" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label for="<%= txtDateTo.ClientID %>" class="form-label">To Date</label>
                                    <asp:TextBox ID="txtDateTo" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div>
                                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-info btn-sm me-1" OnClick="btnSearch_Click" />
                                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClearSearch_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Enrollments List -->
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-list"></i> Enrollments List</h5>
                        <div>
                            <asp:Label ID="lblDisplayedEnrollments" runat="server" CssClass="badge bg-info text-white me-2" Text="Showing: 0" />
                            <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-sm btn-outline-primary" OnClick="btnRefresh_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <asp:GridView ID="gvEnrollments" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-hover"
                                DataKeyNames="enrollment_id" OnRowDataBound="gvEnrollments_RowDataBound" EmptyDataText="No enrollments found.">
                                <Columns>
                                    <asp:BoundField DataField="enrollment_id" HeaderText="ID" ReadOnly="True" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="60px" />
                                    <asp:BoundField DataField="student_name" HeaderText="Student Name" 
                                        ItemStyle-CssClass="fw-bold" HeaderStyle-CssClass="text-start" />
                                    <asp:BoundField DataField="student_email" HeaderText="Student Email" 
                                        HeaderStyle-CssClass="text-start" ItemStyle-Width="200px" />
                                    <asp:BoundField DataField="course_name" HeaderText="Course Name" 
                                        ItemStyle-CssClass="fw-bold text-primary" HeaderStyle-CssClass="text-start" />
                                    <asp:BoundField DataField="course_ects" HeaderText="ECTS" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="70px" />
                                    <asp:BoundField DataField="course_format" HeaderText="Format" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="enrollment_date" HeaderText="Enrollment Date" 
                                        DataFormatString="{0:dd/MM/yyyy}" ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="120px" />
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:Button ID="btnDelete" runat="server" Text="Delete" 
                                                CssClass="btn btn-sm btn-danger" 
                                                CommandName="DeleteEnrollment" 
                                                CommandArgument='<%# Eval("enrollment_id") %>'
                                                OnCommand="btnDelete_Command"
                                                OnClientClick="return confirm('Are you sure you want to delete this enrollment? This action cannot be undone!');" />
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                </Columns>
                                <HeaderStyle CssClass="table-dark" />
                                <RowStyle CssClass="align-middle" />
                            </asp:GridView>
                        </div>
                        
                        <!-- Pagination (if needed) -->
                        <div class="d-flex justify-content-between align-items-center mt-3">
                            <div>
                                <small class="text-muted">
                                    <asp:Label ID="lblPaginationInfo" runat="server" />
                                </small>
                            </div>
                            <div>
                                <!-- Add pagination controls here if needed -->
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Recent Enrollments Summary -->
                <div class="row mt-4">
                    <div class="col-md-6">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0"><i class="fas fa-clock"></i> Recent Enrollments (Last 7 Days)</h6>
                            </div>
                            <div class="card-body">
                                <asp:Repeater ID="rpRecentEnrollments" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between border-bottom py-2">
                                            <div>
                                                <strong><%# Eval("student_name") %></strong><br/>
                                                <small class="text-muted"><%# Eval("course_name") %></small>
                                            </div>
                                            <div class="text-end">
                                                <small class="text-muted"><%# DateTime.Parse(Eval("enrollment_date").ToString()).ToString("dd/MM/yyyy") %></small>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                
                                <asp:Panel ID="pnlNoRecentEnrollments" runat="server" Visible="false">
                                    <div class="text-center text-muted py-3">
                                        <i class="fas fa-info-circle"></i>
                                        <p class="mb-0">No new enrollments in the last 7 days.</p>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                    
                    <div class="col-md-6">
                        <div class="card">
                            <div class="card-header">
                                <h6 class="mb-0"><i class="fas fa-trophy"></i> Most Popular Courses</h6>
                            </div>
                            <div class="card-body">
                                <asp:Repeater ID="rpPopularCourses" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between border-bottom py-2">
                                            <div>
                                                <strong><%# Eval("course_name") %></strong><br/>
                                                <small class="text-muted"><%# Eval("course_format") %> • <%# Eval("ects") %> ECTS</small>
                                            </div>
                                            <div class="text-end">
                                                <span class="badge bg-primary"><%# Eval("enrollment_count") %> students</span>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                
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
        }
        
        .table td {
            font-size: 0.9em;
            vertical-align: middle;
        }
        
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
        
        /* Statistics cards */
        .bg-primary { background-color: #0d6efd !important; }
        .bg-success { background-color: #198754 !important; }
        .bg-info { background-color: #0dcaf0 !important; }
        .bg-warning { background-color: #ffc107 !important; }
        
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
        
        /* Badge styles */
        .badge {
            font-size: 0.75em;
            padding: 0.35em 0.65em;
        }
        
        /* Format badges */
        .format-lecture { background-color: #0d6efd; color: white; }
        .format-seminar { background-color: #198754; color: white; }
        .format-workshop { background-color: #0dcaf0; color: black; }
        .format-laboratory { background-color: #ffc107; color: black; }
        .format-online { background-color: #6c757d; color: white; }
        .format-hybrid { background-color: #212529; color: white; }
        .format-practical { background-color: #f8f9fa; color: black; border: 1px solid #dee2e6; }
        
        /* Responsive adjustments */
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
        }
        
        /* Recent enrollments and popular courses styling */
        .border-bottom {
            border-bottom: 1px solid #dee2e6 !important;
        }
        
        .border-bottom:last-child {
            border-bottom: none !important;
        }
        
        /* Loading states */
        .loading {
            opacity: 0.7;
            pointer-events: none;
        }
        
        /* Success/danger states for rows */
        .table-success {
            background-color: rgba(25, 135, 84, 0.1) !important;
        }
        
        .table-danger {
            background-color: rgba(220, 53, 69, 0.1) !important;
        }
    </style>

    <script>
        // Re-enable buttons on page load
        window.addEventListener('load', function () {
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function (button) {
                button.disabled = false;
            });
        });

        // Add loading state to delete buttons
        function setLoadingState(button) {
            button.disabled = true;
            button.textContent = 'Deleting...';
            button.classList.add('loading');
        }

        // Auto-refresh every 5 minutes (optional)
        setTimeout(function() {
            if (typeof __doPostBack !== 'undefined') {
                __doPostBack('<%= btnRefresh.UniqueID %>', '');
            }
        }, 300000); // 5 minutes
    </script>
</asp:Content>
