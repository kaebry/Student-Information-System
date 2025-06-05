<%@ Page Title="Register" Language="vb" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="Register.aspx.vb" Inherits="StudentInformationSystem.Register" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <main aria-labelledby="title">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-md-8">
                    <h2 id="title" class="text-center mb-4">Create New Account</h2>
                    
                    <!-- Messages -->
                    <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                        <asp:Literal ID="MessageLiteral" runat="server" />
                    </asp:Panel>
                    
                    <div class="card">
                        <div class="card-header">
                            <h4 class="mb-0">Registration Form</h4>
                        </div>
                        <div class="card-body">
                            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-warning" DisplayMode="BulletList" />
                            
                            <!-- Personal Information -->
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <label for="<%= txtFirstName.ClientID %>" class="form-label">First Name <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" placeholder="Enter first name" MaxLength="50" />
                                    <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName" 
                                        CssClass="text-danger small" ErrorMessage="First name is required." Display="Dynamic" />
                                </div>
                                <div class="col-md-6">
                                    <label for="<%= txtLastName.ClientID %>" class="form-label">Last Name <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" placeholder="Enter last name" MaxLength="50" />
                                    <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName" 
                                        CssClass="text-danger small" ErrorMessage="Last name is required." Display="Dynamic" />
                                </div>
                            </div>

                            <!-- Role Selection -->
                            <div class="row mb-3">
                                <div class="col-md-12">
                                    <label for="<%= ddlUserRole.ClientID %>" class="form-label">I am registering as <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlUserRole" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlUserRole_SelectedIndexChanged">
                                        <asp:ListItem Text="Select Role" Value="" />
                                        <asp:ListItem Text="Student" Value="student" />
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvUserRole" runat="server" ControlToValidate="ddlUserRole" 
                                        CssClass="text-danger small" ErrorMessage="Please select your role." Display="Dynamic" />
                                </div>
                            </div>

                            <!-- Student-specific fields -->
                            <asp:Panel ID="pnlStudentFields" runat="server" Visible="false">
                                <div class="alert alert-info">
                                    <i class="fas fa-info-circle"></i> Student Information
                                </div>
                                
                                <div class="row mb-3">
                                    <div class="col-md-6">
                                        <label for="<%= txtDateOfBirth.ClientID %>" class="form-label">Date of Birth</label>
                                        <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="form-control" TextMode="Date" />
                                    </div>
                                    <div class="col-md-6">
                                        <label for="<%= ddlYearLevel.ClientID %>" class="form-label">Year Level</label>
                                        <asp:DropDownList ID="ddlYearLevel" runat="server" CssClass="form-select">
                                            <asp:ListItem Text="Select Year" Value="" />
                                            <asp:ListItem Text="1st Year" Value="1" />
                                            <asp:ListItem Text="2nd Year" Value="2" />
                                            <asp:ListItem Text="3rd Year" Value="3" />
                                            <asp:ListItem Text="4th Year" Value="4" />
                                            <asp:ListItem Text="5th Year" Value="5" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                
                                <div class="row mb-3">
                                    <div class="col-md-12">
                                        <label for="<%= ddlProgram.ClientID %>" class="form-label">Program/Course</label>
                                        <asp:DropDownList ID="ddlProgram" runat="server" CssClass="form-select">
                                            <asp:ListItem Text="Select Program" Value="" />
                                            <asp:ListItem Text="Computer Science" Value="computer_science" />
                                            <asp:ListItem Text="Information Technology" Value="information_technology" />
                                            <asp:ListItem Text="Business Administration" Value="business_admin" />
                                            <asp:ListItem Text="Engineering" Value="engineering" />
                                            <asp:ListItem Text="Mathematics" Value="mathematics" />
                                            <asp:ListItem Text="Physics" Value="physics" />
                                            <asp:ListItem Text="Chemistry" Value="chemistry" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                            </asp:Panel>

                          

                            <!-- Account Information -->
                            <div class="alert alert-primary">
                                <i class="fas fa-user-lock"></i> Account Information
                            </div>
                            
                            <div class="row mb-3">
                                <div class="col-md-12">
                                    <label for="<%= txtEmail.ClientID %>" class="form-label">Email Address <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter email address" />
                                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                                        CssClass="text-danger small" ErrorMessage="Email is required." Display="Dynamic" />
                                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" 
                                        CssClass="text-danger small" ErrorMessage="Please enter a valid email address." 
                                        ValidationExpression="^[\w\.-]+@[\w\.-]+\.\w+$" Display="Dynamic" />
                                </div>
                            </div>
                            
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <label for="<%= txtPassword.ClientID %>" class="form-label">Password <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter password" />
                                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" 
                                        CssClass="text-danger small" ErrorMessage="Password is required." Display="Dynamic" />
                                    <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword" 
                                        CssClass="text-danger small" ErrorMessage="Password must be at least 6 characters long." 
                                        ValidationExpression=".{6,}" Display="Dynamic" />
                                    <small class="form-text text-muted">Password must be at least 6 characters long.</small>
                                </div>
                                <div class="col-md-6">
                                    <label for="<%= txtConfirmPassword.ClientID %>" class="form-label">Confirm Password <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Confirm password" />
                                    <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" 
                                        CssClass="text-danger small" ErrorMessage="Password confirmation is required." Display="Dynamic" />
                                    <asp:CompareValidator ID="cvPassword" runat="server" ControlToCompare="txtPassword" ControlToValidate="txtConfirmPassword" 
                                        CssClass="text-danger small" ErrorMessage="Passwords do not match." Display="Dynamic" />
                                </div>
                            </div>
                            
                            <!-- Submit Button -->
                            <div class="row">
                                <div class="col-md-12 text-center">
                                    <asp:Button ID="btnRegister" runat="server" Text="Create Account" 
                                        CssClass="btn btn-primary btn-lg px-5" OnClick="btnRegister_Click" />
                                    <div class="mt-3">
                                        <span class="text-muted">Already have an account? </span>
                                        <a href="Login.aspx" class="text-decoration-none">Sign in here</a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>

    <style>
        .card {
            border: none;
            border-radius: 0.75rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
        }
        
        .form-label {
            font-weight: 500;
            margin-bottom: 0.5rem;
        }
        
        .text-danger {
            color: #dc3545 !important;
        }
        
        .alert {
            border-left: 4px solid;
            border-radius: 0.5rem;
        }
        
        .alert-info {
            border-left-color: #0dcaf0;
        }
        
        .alert-success {
            border-left-color: #198754;
        }
        
        .alert-primary {
            border-left-color: #0d6efd;
        }
        
        .btn-primary {
            background: linear-gradient(135deg, #0d6efd 0%, #0056b3 100%);
            border: none;
            font-weight: 500;
        }
        
        .btn-primary:hover {
            background: linear-gradient(135deg, #0056b3 0%, #004085 100%);
            transform: translateY(-1px);
            box-shadow: 0 4px 8px rgba(0,0,0,0.15);
        }

        .form-control:focus {
            border-color: #0d6efd;
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
        }

        .card-header {
            background-color: rgba(0, 0, 0, 0.03);
            border-bottom: 1px solid rgba(0, 0, 0, 0.125);
            border-radius: 0.75rem 0.75rem 0 0 !important;
        }
    </style>
</asp:Content>

