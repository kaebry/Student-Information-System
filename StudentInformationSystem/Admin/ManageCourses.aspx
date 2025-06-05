<%@ Page Title="Manage Courses" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ManageCourses.aspx.vb" Inherits="StudentInformationSystem.ManageCourses" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-md-12">
                <h2 class="text-center mb-4">
                    <i class="fas fa-book"></i> Manage Courses
                </h2>
                
                <!-- Messages -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>

                <!-- Course Form -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h4 class="mb-0">
                            <i class="fas fa-plus-circle"></i> Course Information
                            <asp:Label ID="lblFormMode" runat="server" CssClass="badge badge-secondary ml-2 text-dark" Text="Add New Course" />
                        </h4>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="<%= txtCourseName.ClientID %>" class="form-label">Course Name <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtCourseName" runat="server" CssClass="form-control" placeholder="Enter course name" MaxLength="200" />
                                    <asp:RequiredFieldValidator ID="rfvCourseName" runat="server" ControlToValidate="txtCourseName" 
                                        CssClass="text-danger small" ErrorMessage="Course name is required." Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtECTS.ClientID %>" class="form-label">ECTS Credits <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtECTS" runat="server" CssClass="form-control" placeholder="e.g. 6" TextMode="Number" />
                                    <asp:RequiredFieldValidator ID="rfvECTS" runat="server" ControlToValidate="txtECTS" 
                                        CssClass="text-danger small" ErrorMessage="ECTS credits is required." Display="Dynamic" />
                                    <asp:RangeValidator ID="rvECTS" runat="server" ControlToValidate="txtECTS" 
                                        MinimumValue="1" MaximumValue="30" Type="Integer" 
                                        CssClass="text-danger small" ErrorMessage="ECTS must be between 1 and 30." Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="<%= txtHours.ClientID %>" class="form-label">Hours <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtHours" runat="server" CssClass="form-control" placeholder="e.g. 45" TextMode="Number" />
                                    <asp:RequiredFieldValidator ID="rfvHours" runat="server" ControlToValidate="txtHours" 
                                        CssClass="text-danger small" ErrorMessage="Hours is required." Display="Dynamic" />
                                    <asp:RangeValidator ID="rvHours" runat="server" ControlToValidate="txtHours" 
                                        MinimumValue="1" MaximumValue="500" Type="Integer" 
                                        CssClass="text-danger small" ErrorMessage="Hours must be between 1 and 500." Display="Dynamic" />
                                </div>
                            </div>
                        </div>
                        
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="<%= ddlFormat.ClientID %>" class="form-label">Course Format <span class="text-danger">*</span></label>
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
                                    <asp:RequiredFieldValidator ID="rfvFormat" runat="server" ControlToValidate="ddlFormat" 
                                        CssClass="text-danger small" ErrorMessage="Course format is required." Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="<%= txtInstructor.ClientID %>" class="form-label">Instructor <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtInstructor" runat="server" CssClass="form-control" placeholder="Enter instructor name" MaxLength="100" />
                                    <asp:RequiredFieldValidator ID="rfvInstructor" runat="server" ControlToValidate="txtInstructor" 
                                        CssClass="text-danger small" ErrorMessage="Instructor is required." Display="Dynamic" />
                                </div>
                            </div>
                        </div>

                        <!-- Action Buttons -->
                        <div class="row">
                            <div class="col-md-12">
                                <asp:Button ID="btnAdd" runat="server" Text="Add Course" CssClass="btn btn-primary me-2" OnClick="btnAdd_Click" 
                                    OnClientClick="return confirm('Are you sure you want to add this course?');" />
                                <asp:Button ID="btnUpdate" runat="server" Text="Update Course" CssClass="btn btn-warning me-2" OnClick="btnUpdate_Click" 
                                    Enabled="False" OnClientClick="return confirm('Are you sure you want to update this course?');" />
                                <asp:Button ID="btnDelete" runat="server" Text="Delete Course" CssClass="btn btn-danger me-2" OnClick="btnDelete_Click" 
                                    Enabled="False" OnClientClick="return confirm('Are you sure you want to delete this course? This action cannot be undone!');" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear Form" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Search and Filter -->
                <div class="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="fas fa-search"></i> Search & Filter</h5>
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
                                    <label for="<%= txtSearchInstructor.ClientID %>" class="form-label">Search by Instructor</label>
                                    <asp:TextBox ID="txtSearchInstructor" runat="server" CssClass="form-control" placeholder="Enter instructor name..." />
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div>
                                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-info btn-sm me-1" OnClick="btnSearch_Click" CausesValidation="false" />
                                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClearSearch_Click" CausesValidation="false"/>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                      
                    </div>
                </div>

                <!-- Courses List -->
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="mb-0"><i class="fas fa-list"></i> Courses List</h5>
                        <asp:Label ID="lblTotalCourses" runat="server" CssClass="badge badge-info" Text="Total: 0" />
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <asp:GridView ID="gvCourses" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-hover"
                                DataKeyNames="course_id" OnSelectedIndexChanged="gvCourses_SelectedIndexChanged"
                                OnRowDataBound="gvCourses_RowDataBound" EmptyDataText="No courses found.">
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
                                        ItemStyle-CssClass="text-center" HeaderStyle-CssClass="text-center" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="instructor" HeaderText="Instructor" 
                                        HeaderStyle-CssClass="text-start" ItemStyle-Width="150px" />
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkSelect" runat="server" Text="Select" CommandName="Select" 
                                                CssClass="btn btn-sm btn-outline-primary" CausesValidation="false" />
                                        </ItemTemplate>
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                </Columns>
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
        
        /* Hover effects for better UX */
        .table-hover tbody tr:hover {
            background-color: rgba(0,0,0,.05);
        }
        
        /* Better spacing for form elements */
        .form-control:focus, .form-select:focus {
            border-color: #0d6efd;
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
        }
        
        /* Responsive improvements */
        @media (max-width: 768px) {
            .btn-sm {
                font-size: 0.7rem;
                padding: 0.2rem 0.4rem;
            }
            
            .table {
                font-size: 0.8rem;
            }
        }
        
        /* Loading animation for better UX */
        .card {
            transition: box-shadow 0.15s ease-in-out;
        }
        
        .card:hover {
            box-shadow: 0 0.125rem 0.5rem rgba(0, 0, 0, 0.075);
        }
        
        /* Better button styles */
        .btn {
            transition: all 0.15s ease-in-out;
        }
        
        .btn:hover {
            transform: translateY(-1px);
        }
        
        /* Selected row highlighting */
        .table-warning {
            background-color: rgba(255, 193, 7, 0.1) !important;
            border-left: 4px solid #ffc107;
        }
        
        /* Disabled button style */
        .btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
    </style>

    <script>
        // Re-enable buttons on page load (in case of page refresh)
        window.addEventListener('load', function () {
            var buttons = document.querySelectorAll('input[type="submit"], input[type="button"]');
            buttons.forEach(function (button) {
                button.disabled = false;
            });
        });
    </script>
</asp:Content>
