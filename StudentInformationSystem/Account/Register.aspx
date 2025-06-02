<%@ Page Title="Register" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Register.aspx.vb" Inherits="StudentInformationSystem.Register" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <main aria-labelledby="title">
        <h2 id="title">Register</h2>
        <p class="text-danger">
            <asp:Literal runat="server" ID="ErrorMessage" />
        </p>
        <div id="registrationStatus" class="alert" style="display: none;"></div>
        
        <div>
            <h4>Create a new account</h4>
            <hr />
            <asp:ValidationSummary runat="server" CssClass="text-danger" />
            
            <div class="form-group">
                <label for="FirstName">First Name</label>
                <asp:TextBox ID="FirstName" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="FirstName" CssClass="text-danger" ErrorMessage="First name is required." />
            </div>
            
            <div class="form-group">
                <label for="LastName">Last Name</label>
                <asp:TextBox ID="LastName" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="LastName" CssClass="text-danger" ErrorMessage="Last name is required." />
            </div>
            
            <!-- Student-specific fields -->
            <div class="form-group">
                <label for="DateOfBirth">Date of Birth</label>
                <asp:TextBox ID="DateOfBirth" runat="server" CssClass="form-control" TextMode="Date" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="DateOfBirth" CssClass="text-danger" ErrorMessage="Date of birth is required." />
            </div>
            
            <div class="form-group">
                <label for="Program">Program/Course</label>
                <asp:DropDownList ID="Program" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Select Program" Value="" />
                    <asp:ListItem Text="Computer Science" Value="computer_science" />
                    <asp:ListItem Text="Information Technology" Value="information_technology" />
                    <asp:ListItem Text="Business Administration" Value="business_admin" />
                    <asp:ListItem Text="Engineering" Value="engineering" />
                </asp:DropDownList>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="Program" CssClass="text-danger" ErrorMessage="Program selection is required." />
            </div>
            
            <div class="form-group">
                <label for="YearLevel">Year Level</label>
                <asp:DropDownList ID="YearLevel" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Select Year" Value="" />
                    <asp:ListItem Text="1st Year" Value="1" />
                    <asp:ListItem Text="2nd Year" Value="2" />
                    <asp:ListItem Text="3rd Year" Value="3" />
                    <asp:ListItem Text="4th Year" Value="4" />
                </asp:DropDownList>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="YearLevel" CssClass="text-danger" ErrorMessage="Year level is required." />
            </div>
            
            <div class="form-group">
                <label for="Email">Email</label>
                <asp:TextBox ID="Email" runat="server" CssClass="form-control" TextMode="Email" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="Email" CssClass="text-danger" ErrorMessage="Email is required." />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="Email" CssClass="text-danger" 
                    ErrorMessage="Invalid email format." ValidationExpression="^[\w\.-]+@[\w\.-]+\.\w+$" />
            </div>
            
            <div class="form-group">
                <label for="Password">Password</label>
                <asp:TextBox ID="Password" runat="server" CssClass="form-control" TextMode="Password" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="Password" CssClass="text-danger" ErrorMessage="Password is required." />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="Password" CssClass="text-danger" 
                    ErrorMessage="Password must be at least 6 characters long." ValidationExpression=".{6,}" />
                <small class="form-text text-muted">Password must be at least 6 characters long.</small>
            </div>
            
            <div class="form-group">
                <label for="ConfirmPassword">Confirm Password</label>
                <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ConfirmPassword" CssClass="text-danger" ErrorMessage="Confirmation is required." />
                <asp:CompareValidator runat="server" ControlToCompare="Password" ControlToValidate="ConfirmPassword" CssClass="text-danger" ErrorMessage="Passwords do not match." />
            </div>
            
            <div class="form-group">
                <label for="UserRole">Role</label>
                <asp:DropDownList ID="UserRole" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Student" Value="student" Selected="True" />
                    <asp:ListItem Text="Teacher" Value="teacher" />
                </asp:DropDownList>
            </div>
            
            <div class="form-group mt-3">
                <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn btn-dark" 
                    OnClientClick="return registerWithSupabase();" UseSubmitBehavior="false" />
                <div id="loadingSpinner" style="display: none;" class="mt-2">
                    <div class="spinner-border spinner-border-sm" role="status">
                        <span class="sr-only">Loading...</span>
                    </div>
                    <span class="ml-2">Creating account...</span>
                </div>
            </div>
        </div>
    </main>

    <script type="text/javascript">
        async function registerWithSupabase() {
            // Prevent default form submission
            event.preventDefault();
            
            // Check if page is valid first
            if (!Page_ClientValidate()) {
                return false;
            }
            
            // Show loading spinner
            document.getElementById('loadingSpinner').style.display = 'block';
            document.getElementById('<%= btnRegister.ClientID %>').disabled = true;
            
            // Get form values
            const formData = {
                email: document.getElementById('<%= Email.ClientID %>').value,
                password: document.getElementById('<%= Password.ClientID %>').value,
                firstName: document.getElementById('<%= FirstName.ClientID %>').value,
                lastName: document.getElementById('<%= LastName.ClientID %>').value,
                dateOfBirth: document.getElementById('<%= DateOfBirth.ClientID %>').value,
                program: document.getElementById('<%= Program.ClientID %>').value,
                yearLevel: document.getElementById('<%= YearLevel.ClientID %>').value,
                role: document.getElementById('<%= UserRole.ClientID %>').value
            };
            
            try {
                // Register with Supabase Auth
                const { data, error } = await supabase.auth.signUp({
                    email: formData.email,
                    password: formData.password,
                    options: {
                        data: {
                            first_name: formData.firstName,
                            last_name: formData.lastName,
                            role: formData.role
                        }
                    }
                });
                
                if (error) {
                    showMessage('Registration failed: ' + error.message, 'danger');
                } else {
                    // If successful, create student profile
                    if (data.user) {
                        await createStudentProfile(data.user.id, formData);
                        showMessage('Registration successful! Please check your email to verify your account.', 'success');
                        
                        // Clear form
                        document.querySelector('form').reset();
                    }
                }
                
            } catch (err) {
                showMessage('An error occurred during registration: ' + err.message, 'danger');
            } finally {
                // Hide loading spinner
                document.getElementById('loadingSpinner').style.display = 'none';
                document.getElementById('<%= btnRegister.ClientID %>').disabled = false;
            }
            
            return false; // Prevent form submission
        }
        
        async function createStudentProfile(userId, formData) {
            try {
                // Generate student ID (you can customize this logic)
                const currentYear = new Date().getFullYear();
                const studentId = currentYear + '-' + Math.random().toString(36).substr(2, 6).toUpperCase();
                
                const { data, error } = await supabase
                    .from('student_profiles')
                    .insert([
                        {
                            user_id: userId,
                            student_id: studentId,
                            first_name: formData.firstName,
                            last_name: formData.lastName,
                            date_of_birth: formData.dateOfBirth,
                            program: formData.program,
                            year_level: parseInt(formData.yearLevel),
                            enrollment_status: 'pending'
                        }
                    ]);
                
                if (error) {
                    console.error('Error creating student profile:', error);
                    showMessage('Account created but profile setup failed. Please contact support.', 'warning');
                }
                
            } catch (err) {
                console.error('Error in createStudentProfile:', err);
            }
        }
        
        function showMessage(message, type) {
            const statusDiv = document.getElementById('registrationStatus');
            statusDiv.className = 'alert alert-' + type;
            statusDiv.textContent = message;
            statusDiv.style.display = 'block';
            
            // Scroll to message
            statusDiv.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            
            // Auto-hide success messages after 5 seconds
            if (type === 'success') {
                setTimeout(() => {
                    statusDiv.style.display = 'none';
                }, 5000);
            }
        }
    </script>
</asp:Content>

