<%@ Page Title="Create Admin User" Language="vb" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="CreateAdmin.aspx.vb" Inherits="StudentInformationSystem.CreateAdmin" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <div class="container">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <h2 class="text-center mb-4">Create Admin User</h2>
                <div class="alert alert-warning">
                    <i class="fas fa-exclamation-triangle"></i>
                    <strong>Security Notice:</strong> This page should be deleted after creating your admin user!
                </div>
                
                <!-- Messages -->
                <asp:Panel ID="MessagePanel" runat="server" Visible="false" CssClass="mb-3">
                    <asp:Literal ID="MessageLiteral" runat="server" />
                </asp:Panel>
                
                <div class="card">
                    <div class="card-header">
                        <h4>Admin Account Information</h4>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label">Email Address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="admin@school.com" />
                        </div>
                        
                        <div class="mb-3">
                            <label class="form-label">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Choose a strong password" />
                        </div>
                        
                        <div class="mb-3">
                            <label class="form-label">Confirm Password</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Confirm password" />
                        </div>
                        
                        <div class="d-grid">
                            <asp:Button ID="btnCreateAdmin" runat="server" Text="Create Admin User" 
                                        CssClass="btn btn-danger btn-lg" OnClick="btnCreateAdmin_Click" 
                                        OnClientClick="return confirm('Are you sure you want to create an admin user?');" />
                        </div>
                        
                        <div class="mt-3 text-center">
                            <asp:Button ID="btnCheckExistingAdmins" runat="server" Text="Check Existing Admins" 
                                        CssClass="btn btn-secondary" OnClick="btnCheckExistingAdmins_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>