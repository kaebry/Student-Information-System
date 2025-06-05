<%@ Page Title="Available Courses" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="AvailableCourses.aspx.vb" Inherits="StudentInformationSystem.AvailableCourses" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <h2 class="text-center mb-4">
                    <i class="fas fa-search"></i> Available Courses
                </h2>
                
                <!-- Messages -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <!-- Student Info -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">
                            <i class="fas fa-user-graduate"></i> Student Information
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6">
                                <p><strong>Name:</strong> <asp:Label ID="lblStudentName" runat="server" CssClass="text-primary" /></p>
                                <p><strong>Email:</strong> <asp:Label ID="lblStudentEmail" runat="server" CssClass="text-muted" /></p>
                            </div>
                            <div class="col-md-6">
                                <p><strong>Current Enrollments:</strong> <asp:Label ID="lblCurrentEnrollments" runat="server" CssClass="badge badge-info text-dark" /></p>
                                <p><strong>Total ECTS Enrolled:</strong> <asp:Label ID="lblTotalECTS" runat="server" CssClass="badge badge-success text-dark" /></p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Search and Filter -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-filter"></i> Search & Filter Courses</h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label for="<%= txtSearchName.ClientID %>" class="form-label">Search by Course Name</label>
                                    <asp:TextBox ID="txtSearchName" runat="server" CssClass="form-control" placeholder="Enter course name..." />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= ddlFilterFormat.ClientID %>" class="form-label">Filter by Format</label>
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
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= ddlFilterECTS.ClientID %>" class="form-label">Filter by ECTS</label>
                                    <asp:DropDownList ID="ddlFilterECTS" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="All ECTS" Value="" />
                                        <asp:ListItem Text="1-3 ECTS" Value="1-3" />
                                        <asp:ListItem Text="4-6 ECTS" Value="4-6" />
                                        <asp:ListItem Text="7-9 ECTS" Value="7-9" />
                                        <asp:ListItem Text="10+ ECTS" Value="10+" />
                                    </asp:DropDownList>
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

                <!-- Available Courses -->
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-book-open"></i> Available Courses</h5>
                        <div>
                            <asp:Label ID="lblTotalCourses" runat="server" CssClass="badge badge-info me-2" Text="Total: 0" />
                            <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-sm btn-outline-primary" OnClick="btnRefresh_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <asp:GridView ID="gvCourses" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-hover"
                                DataKeyNames="course_id" OnRowDataBound="gvCourses_RowDataBound" EmptyDataText="No courses found.">
                                <Columns>
                                    <asp:BoundField DataField="course_id" HeaderText="ID" ReadOnly="True" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="60px" />
                                    <asp:BoundField DataField="course_name" HeaderText="Course Name" 
                                        ItemStyle-CssClass="fw-bold" HeaderStyle-CssClass="text-start" />
                                    <asp:BoundField DataField="ects" HeaderText="ECTS" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="80px" />
                                    <asp:BoundField DataField="hours" HeaderText="Hours" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="80px" />
                                    <asp:BoundField DataField="format" HeaderText="Format" 
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="120px" />
                                    <asp:BoundField DataField="instructor" HeaderText="Instructor" 
                                        HeaderStyle-CssClass="text-start" ItemStyle-Width="150px" />
                                    <asp:TemplateField HeaderText="Enrollment Status" ItemStyle-Width="140px">
                                        <ItemTemplate>
                                            <asp:Label ID="lblEnrollmentStatus" runat="server" />
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="120px">
                                        <ItemTemplate>
                                            <asp:Button ID="btnEnroll" runat="server" Text="Enroll" 
                                                CssClass="btn btn-sm btn-success" 
                                                CommandName="Enroll" 
                                                CommandArgument='<%# Eval("course_id") %>'
                                                OnCommand="btnEnroll_Command"
                                                OnClientClick="return confirm('Are you sure you want to enroll in this course?');" />
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
                                <HeaderStyle CssClass="table-dark" />
                                <RowStyle CssClass="align-middle" />
                            </asp:GridView>
                        </div>
                    </div>
                </div>

                <!-- My Enrollments Summary -->
                <div class="card mt-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-clipboard-list"></i> My Current Enrollments</h5>
                    </div>
                    <div class="card-body">
                        <asp:Repeater ID="rpMyEnrollments" runat="server">
                            <HeaderTemplate>
                                <div class="row">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class="col-md-6 col-lg-4 mb-3">
                                    <div class="card border-success">
                                        <div class="card-body">
                                            <h6 class="card-title text-success"><%# Eval("course_name") %></h6>
                                            <p class="card-text small">
                                                <strong>ECTS:</strong> <%# Eval("ects") %> | 
                                                <strong>Hours:</strong> <%# Eval("hours") %><br/>
                                                <strong>Format:</strong> <%# Eval("format") %><br/>
                                                <strong>Instructor:</strong> <%# Eval("instructor") %><br/>
                                                <strong>Enrolled:</strong> <%# DateTime.Parse(Eval("enrollment_date").ToString()).ToString("dd/MM/yyyy") %>
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                </div>
                            </FooterTemplate>
                        </asp:Repeater>
                        
                        <asp:Panel ID="pnlNoEnrollments" runat="server" Visible="false">
                            <div class="text-center text-muted py-4">
                                <i class="fas fa-info-circle fa-2x mb-2"></i>
                                <p>You are not currently enrolled in any courses.</p>
                                <p class="small">Browse available courses above and click "Enroll" to get started!</p>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <style>
        .badge {
            font-size: 0.75em;
            padding: 0.35em 0.65em;
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
        
        .card-header h5 {
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
        
        /* Hover effects */
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.05);
        }
        
        /* Focus styles */
        .form-control:focus, .form-select:focus {
            border-color: #0d6efd;
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
        }
        
        /* Card hover effects */
        .card {
            transition: box-shadow 0.15s ease-in-out;
        }
        
        .card:hover {
            box-shadow: 0 0.125rem 0.5rem rgba(0, 0, 0, 0.075);
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
        
        /* Enrollment cards */
        .border-success {
            border-color: #198754 !important;
        }
        
        .text-success {
            color: #198754 !important;
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
            
            .badge {
                font-size: 0.65em;
            }
        }
        
        /* Loading states */
        .loading {
            opacity: 0.7;
            pointer-events: none;
        }
        
        /* Status badges */
        .status-enrolled {
            background-color: #198754 !important;
            color: white !important;
        }
        
        .status-not-enrolled {
            background-color: #6c757d !important;
            color: white !important;
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

        // Add loading state to enrollment buttons
        function setLoadingState(button) {
            button.disabled = true;
            button.textContent = 'Processing...';
            button.classList.add('loading');
        }
    </script>
</asp:Content>
