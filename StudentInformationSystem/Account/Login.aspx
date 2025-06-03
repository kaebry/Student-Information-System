<%@ Page Title="Login" Language="vb" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="Login.aspx.vb" Inherits="StudentInformationSystem.Login" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <main aria-labelledby="title">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-md-6">
                    <h2 id="title" class="text-center mb-4">Welcome Back</h2>
                    
                    <!-- Messages -->
                    <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                        <asp:Literal ID="MessageLiteral" runat="server" />
                    </asp:Panel>
                    
                    <div class="card">
                        <div class="card-header">
                            <h4 class="mb-0">Sign In to Your Account</h4>
                        </div>
                        <div class="card-body">
                            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-warning" DisplayMode="BulletList" />
                            
                            <!-- Login Form -->
                            <div class="mb-3">
                                <label for="<%= txtEmail.ClientID %>" class="form-label">Email Address <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter your email address" />
                                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                                    CssClass="text-danger small" ErrorMessage="Email is required." Display="Dynamic" />
                                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" 
                                    CssClass="text-danger small" ErrorMessage="Please enter a valid email address." 
                                    ValidationExpression="^[\w\.-]+@[\w\.-]+\.\w+$" Display="Dynamic" />
                            </div>
                            
                            <div class="mb-3">
                                <label for="<%= txtPassword.ClientID %>" class="form-label">Password <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter your password" />
                                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" 
                                    CssClass="text-danger small" ErrorMessage="Password is required." Display="Dynamic" />
                            </div>
                            
                            <div class="mb-3 form-check">
                                <asp:CheckBox ID="chkRememberMe" runat="server" CssClass="form-check-input" />
                                <label for="<%= chkRememberMe.ClientID %>" class="form-check-label">Remember me</label>
                            </div>
                            
                            <!-- Login Button -->
                            <div class="d-grid">
                                <asp:Button ID="btnLogin" runat="server" Text="Sign In" 
                                    CssClass="btn btn-primary btn-lg" OnClick="btnLogin_Click" />
                            </div>
                            
                            <!-- Links -->
                            <div class="text-center mt-3">
                                <div class="mb-2">
                                    <a href="Forgot.aspx" class="text-decoration-none">Forgot your password?</a>
                                </div>
                                <div>
                                    <span class="text-muted">Don't have an account? </span>
                                    <a href="Register.aspx" class="text-decoration-none">Create one here</a>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    
                    <!-- Registration Success Message -->
                    <asp:Panel ID="pnlRegistrationSuccess" runat="server" Visible="false" CssClass="mt-3">
                        <div class="alert alert-success">
                            <i class="fas fa-check-circle"></i> Registration completed successfully! You can now sign in with your credentials.
                        </div>
                    </asp:Panel>
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

        .form-check-input:checked {
            background-color: #0d6efd;
            border-color: #0d6efd;
        }

        .alert {
            border-left: 4px solid;
            border-radius: 0.5rem;
        }
        
        .alert-success {
            border-left-color: #198754;
        }
        
        .alert-danger {
            border-left-color: #dc3545;
        }
        
        .alert-warning {
            border-left-color: #ffc107;
        }
    </style>
</asp:Content>
