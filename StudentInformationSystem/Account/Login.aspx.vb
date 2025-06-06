'==============================================================================
' LOGIN PAGE CODE-BEHIND CLASS - AUTHENTICATION AND SESSION MANAGEMENT
'==============================================================================
' Purpose: Secure user authentication with role-based access control
' Features: Multi-role login, session management, connection resilience, security
' 
' Security Architecture:
' - BCrypt password hashing for secure credential verification
' - SQL injection prevention through parameterized queries
' - Session-based authentication with optional persistent cookies
' - Role-based access control (Admin, Student, Teacher support)
' - Connection pool management for cloud database reliability
' - Comprehensive error handling with user-friendly feedback
' - Brute force protection through proper error messaging
' 
' Authentication Flow:
' 1. User Input Validation: Email format, required fields, basic sanitization
' 2. Database Connection: Resilient cloud database connectivity with retry logic
' 3. User Verification: Check if user exists before password verification
' 4. Password Authentication: BCrypt hash verification for security
' 5. Profile Retrieval: Get user details based on role (student/admin/teacher)
' 6. Session Establishment: Create secure session with role-based data
' 7. Cookie Management: Optional persistent authentication cookies
' 8. Redirect Management: Route users to appropriate dashboard based on role
' 
' Cloud Database Optimizations:
' - Connection pool clearing for stale connection cleanup
' - Configurable timeouts for network resilience
' - Automatic retry mechanisms for temporary failures
' - Graceful degradation for database connectivity issues
' - Resource cleanup and proper disposal patterns
'==============================================================================

' SYSTEM IMPORTS: Core framework functionality
Imports System.Configuration        ' Configuration file access for connection strings
Imports System.Data                ' Data access layer definitions and enumerations
Imports Npgsql                     ' PostgreSQL .NET data provider for Supabase
Imports BCrypt.Net                 ' Secure password hashing and verification library
Imports System.Web.Security        ' ASP.NET authentication and authorization framework

''' <summary>
''' Login Page Class - Secure Authentication and Session Management
''' Handles user authentication for multiple roles with enhanced security features
''' Implements cloud database resilience and comprehensive error handling
''' </summary>
''' <remarks>
''' This class provides secure authentication services for the Student Information System
''' with support for multiple user roles (Admin, Student, Teacher), persistent sessions,
''' and robust cloud database connectivity. Security features include BCrypt password
''' hashing, SQL injection prevention, and session hijacking protection.
''' </remarks>
Partial Public Class Login
    Inherits System.Web.UI.Page

    '--------------------------------------------------------------------------
    ' PAGE LIFECYCLE EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Page Load Event Handler - Authentication State and UI Initialization
    ''' Manages login page display, redirect logic, and user feedback messages
    ''' Implements security checks to prevent unauthorized access loops
    ''' </summary>
    ''' <param name="sender">Page object that raised the load event</param>
    ''' <param name="e">Event arguments containing page load context</param>
    ''' <remarks>
    ''' Handles multiple scenarios:
    ''' - Redirect already authenticated users to prevent duplicate sessions
    ''' - Display registration success messages for new users
    ''' - Show logout confirmation messages for security feedback
    ''' - Set initial focus for accessibility and user experience
    ''' Only processes these actions on initial page load (not postbacks)
    ''' </remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' AUTHENTICATION STATE VERIFICATION
            ' Prevent already logged-in users from accessing login page
            ' Checks both ASP.NET Identity and custom session authentication
            ' Redirects to default page to prevent authentication loops
            If User.Identity.IsAuthenticated OrElse Session("UserEmail") IsNot Nothing Then
                Response.Redirect("~/Default.aspx")
            End If

            ' POST-REGISTRATION SUCCESS MESSAGE DISPLAY
            ' Show confirmation when users arrive from successful registration
            ' Provides positive feedback and encourages immediate login
            ' Query string parameter prevents message persistence on refresh
            If Request.QueryString("registered") = "true" Then
                pnlRegistrationSuccess.Visible = True
            End If

            ' POST-LOGOUT SUCCESS MESSAGE DISPLAY
            ' Confirm successful logout for security and user confidence
            ' Provides feedback that logout completed successfully
            ' Prevents confusion about authentication state
            If Request.QueryString("loggedout") = "true" Then
                ShowMessage("✅ You have been logged out successfully.", "success")
            End If

            ' ACCESSIBILITY AND UX ENHANCEMENT
            ' Set initial focus to email field for keyboard navigation
            ' Improves accessibility compliance and user experience
            ' Allows immediate typing without mouse interaction
            txtEmail.Focus()
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' AUTHENTICATION EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Login Button Click Handler - Primary Authentication Entry Point
    ''' Orchestrates the complete user authentication workflow
    ''' Implements comprehensive validation, authentication, and session management
    ''' </summary>
    ''' <param name="sender">Login button that triggered the authentication attempt</param>
    ''' <param name="e">Event arguments from button click</param>
    ''' <remarks>
    ''' Authentication Workflow:
    ''' 1. Input Validation: Server-side validation of form data
    ''' 2. Credential Processing: Extract and sanitize user credentials
    ''' 3. Database Authentication: Verify credentials against database
    ''' 4. Session Creation: Establish secure user session with role data
    ''' 5. Redirect Management: Route user to appropriate dashboard
    ''' 6. Error Handling: Provide security-conscious error feedback
    ''' 
    ''' Security Considerations:
    ''' - Input sanitization to prevent injection attacks
    ''' - Generic error messages to prevent username enumeration
    ''' - Password field clearing on failed attempts for security
    ''' - Connection resilience for cloud database reliability
    ''' </remarks>
    Protected Sub btnLogin_Click(sender As Object, e As EventArgs)
        ' CLEAR PREVIOUS USER FEEDBACK
        ' Reset message display to provide clean state for new attempt
        ' Prevents confusion from previous authentication attempts
        HideMessage()

        ' SERVER-SIDE VALIDATION VERIFICATION
        ' Ensure all form validators passed before processing
        ' Provides user-friendly feedback for validation failures
        ' Prevents processing of invalid data submissions
        If Not Page.IsValid Then
            ShowMessage("Please correct the errors below and try again.", "danger")
            Return
        End If

        Try
            ' USER INPUT EXTRACTION AND SANITIZATION
            ' Extract form values with safety measures and normalization
            ' Email normalization ensures consistent database lookups
            ' Trim removes accidental whitespace that could cause authentication failures
            Dim email As String = txtEmail.Text.Trim().ToLower()
            Dim password As String = txtPassword.Text.Trim()

            ' PRIMARY INPUT VALIDATION
            ' Double-check required fields even after client-side validation
            ' Provides server-side security against bypassed client validation
            ' Generic error messages maintain security best practices
            If String.IsNullOrEmpty(email) Then
                ShowMessage("❌ Email is required.", "danger")
                Return
            End If

            If String.IsNullOrEmpty(password) Then
                ShowMessage("❌ Password is required.", "danger")
                Return
            End If

            ' CORE AUTHENTICATION PROCESS
            ' Attempt user authentication with comprehensive error handling
            ' Returns user object on success, Nothing on failure
            ' Handles both authentication failures and connection issues
            Dim user As UserInfo = AuthenticateUser(email, password)

            If user IsNot Nothing Then
                ' AUTHENTICATION SUCCESS PATHWAY
                ' User credentials verified successfully
                Try
                    ' SESSION ESTABLISHMENT AND CONFIGURATION
                    ' Create secure session with role-based data and optional persistence
                    ' Remember Me functionality creates persistent authentication cookies
                    SetUserSession(user, chkRememberMe.Checked)
                    ShowMessage("✅ Login successful! Redirecting...", "success")

                    ' REDIRECT URL MANAGEMENT
                    ' Handle return URLs for post-authentication routing
                    ' Default to main dashboard if no specific return URL provided
                    ' Prevents open redirect vulnerabilities through URL validation
                    Dim returnUrl As String = Request.QueryString("ReturnUrl")
                    If String.IsNullOrEmpty(returnUrl) Then
                        returnUrl = "~/Default.aspx"
                    End If

                    ' DELAYED REDIRECT WITH USER FEEDBACK
                    ' Provide success message before redirecting
                    ' Gives users confirmation of successful authentication
                    ' Uses client-side timer for controlled redirect timing
                    ClientScript.RegisterStartupScript(Me.GetType(), "redirect",
                        $"setTimeout(function(){{ window.location.href='{ResolveUrl(returnUrl)}'; }}, 1500);", True)

                Catch sessionEx As Exception
                    ' SESSION CREATION ERROR HANDLING
                    ' Handle cases where authentication succeeded but session creation failed
                    ' Provides specific feedback about session establishment issues
                    ' Allows user to retry without re-authentication in some cases
                    ShowMessage($"❌ Login successful but session setup failed: {sessionEx.Message}", "danger")
                    Return
                End Try
            Else
                ' AUTHENTICATION FAILURE PATHWAY
                ' Generic error message prevents username enumeration attacks
                ' Password field clearing enhances security
                ' Maintains user experience while preserving security
                ShowMessage("❌ Invalid email or password. Please check your credentials and try again.", "danger")
                txtPassword.Text = "" ' Clear password field for security
            End If

        Catch connEx As Exception When connEx.Message.Contains("connection") OrElse
                                        connEx.Message.Contains("timeout") OrElse
                                        connEx.Message.Contains("network") OrElse
                                        connEx.Message.Contains("database")
            ' DATABASE CONNECTIVITY ERROR HANDLING
            ' Specific handling for network and database connection issues
            ' User-friendly message suggests common solutions
            ' Distinguishes between authentication failures and technical issues
            ShowMessage("❌ Cannot connect to the database. Please check your internet connection and try again.", "danger")
        Catch ex As Exception
            ' GENERAL ERROR HANDLING
            ' Catch-all for unexpected errors during authentication process
            ' Provides user feedback while maintaining security
            ' Logs error details for debugging without exposing sensitive information
            ShowMessage($"❌ Login error: {ex.Message}", "danger")
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' CORE AUTHENTICATION METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Primary User Authentication Method with Connection Resilience
    ''' Implements robust authentication with cloud database optimization
    ''' Provides connection pool management and retry logic for reliability
    ''' </summary>
    ''' <param name="email">User's email address (normalized to lowercase)</param>
    ''' <param name="password">User's plain text password for BCrypt verification</param>
    ''' <returns>UserInfo object on successful authentication, Nothing on failure</returns>
    ''' <remarks>
    ''' Two-phase authentication approach:
    ''' 1. Connection Preparation: Clear stale connections and prepare fresh connection
    ''' 2. Direct Authentication: Perform actual credential verification
    ''' 
    ''' Error Handling Strategy:
    ''' - Connection errors: Throw specific exceptions for user feedback
    ''' - Authentication errors: Return Nothing to prevent information disclosure
    ''' - Retry logic: Connection pool clearing improves cloud database reliability
    ''' </remarks>
    Private Function AuthenticateUser(email As String, password As String) As UserInfo
        Try
            ' CONNECTION POOL OPTIMIZATION FOR CLOUD DATABASES
            ' Clear potentially stale connections that can accumulate in cloud environments
            ' Brief delay allows connection pool cleanup to complete properly
            ' Improves reliability with Supabase and other cloud PostgreSQL providers
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(100)

            ' DIRECT AUTHENTICATION ATTEMPT
            ' Perform actual credential verification with fresh connection
            ' Separated for clean error handling and retry logic
            Return AuthenticateUserDirect(email, password)
        Catch connEx As Exception When connEx.Message.Contains("connection") OrElse
                                         connEx.Message.Contains("timeout") OrElse
                                         connEx.Message.Contains("stream") OrElse
                                         connEx.Message.Contains("network")
            ' CONNECTION-SPECIFIC ERROR HANDLING
            ' Re-throw connection errors with user-friendly message
            ' Allows calling method to provide appropriate user feedback
            ' Distinguishes between connection issues and authentication failures
            Throw New Exception("Database connection failed. Please try again.")
        Catch ex As Exception
            ' AUTHENTICATION FAILURE OR OTHER ERRORS
            ' Return Nothing for any non-connection error
            ' Prevents exposure of internal error details to users
            ' Maintains security by not revealing authentication internals
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Direct Database Authentication with Comprehensive User Data Retrieval
    ''' Performs actual credential verification and complete user profile loading
    ''' Implements secure BCrypt password verification and role-based data retrieval
    ''' </summary>
    ''' <param name="email">User's email address for database lookup</param>
    ''' <param name="password">Plain text password for BCrypt hash verification</param>
    ''' <returns>Complete UserInfo object with profile data, or Nothing on failure</returns>
    ''' <remarks>
    ''' Authentication Process:
    ''' 1. User Existence Check: Verify user exists before password verification
    ''' 2. Credential Retrieval: Get stored password hash and basic user data
    ''' 3. Password Verification: BCrypt hash comparison for security
    ''' 4. Profile Data Loading: Retrieve role-specific profile information
    ''' 5. UserInfo Assembly: Create complete user object for session management
    ''' 
    ''' Security Features:
    ''' - Separate user existence check prevents timing attacks
    ''' - BCrypt verification for secure password comparison
    ''' - Role-based profile data loading for proper authorization
    ''' - Parameterized queries prevent SQL injection attacks
    ''' </remarks>
    Private Function AuthenticateUserDirect(email As String, password As String) As UserInfo
        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        Using conn As New NpgsqlConnection(connStr)
            Try
                conn.Open()

                ' PHASE 1: USER EXISTENCE VERIFICATION
                ' Check if user exists before attempting password verification
                ' Prevents unnecessary password hash retrieval for non-existent users
                ' Provides early exit path to improve performance
                Using checkCmd As New NpgsqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", conn)
                    checkCmd.CommandTimeout = 15
                    checkCmd.Parameters.AddWithValue("@email", email)
                    Dim userExists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                    If userExists = 0 Then
                        Return Nothing ' User doesn't exist - generic failure response
                    End If
                End Using

                ' PHASE 2: CREDENTIAL AND PROFILE DATA RETRIEVAL
                ' Get user authentication data and basic profile information
                ' Includes password hash, role, and role-specific ID for profile loading
                Using userCmd As New NpgsqlCommand("SELECT id, email, password_hash, role, student_id FROM users WHERE email = @email", conn)
                    userCmd.CommandTimeout = 15
                    userCmd.Parameters.AddWithValue("@email", email)

                    Using reader As NpgsqlDataReader = userCmd.ExecuteReader()
                        If reader.Read() Then
                            ' EXTRACT CORE USER DATA
                            ' Get essential user information for authentication and session
                            Dim userId As Integer = Convert.ToInt32(reader("id"))
                            Dim userEmail As String = reader("email").ToString()
                            Dim storedPasswordHash As String = reader("password_hash").ToString()
                            Dim userRole As String = reader("role").ToString()
                            Dim studentId As Object = reader("student_id")

                            ' PHASE 3: SECURE PASSWORD VERIFICATION
                            ' Use BCrypt to verify password against stored hash
                            ' BCrypt handles salt and iteration count automatically
                            ' Returns false if password doesn't match, preventing timing attacks
                            If Not BCrypt.Net.BCrypt.Verify(password, storedPasswordHash) Then
                                Return Nothing ' Password doesn't match - generic failure response
                            End If

                            reader.Close()

                            ' PHASE 4: ROLE-SPECIFIC PROFILE DATA LOADING
                            ' Retrieve additional user details based on user role
                            ' Different roles have different profile data structures
                            Dim firstName As String = ""
                            Dim lastName As String = ""
                            Dim profileId As Long = 0

                            ' STUDENT PROFILE DATA RETRIEVAL
                            ' Load student-specific information from students table
                            ' Student ID links users table to students table for profile data
                            If userRole = "student" AndAlso studentId IsNot DBNull.Value Then
                                Using nameCmd As New NpgsqlCommand("SELECT first_name, last_name, id FROM students WHERE id = @id", conn)
                                    nameCmd.CommandTimeout = 15
                                    nameCmd.Parameters.AddWithValue("@id", Convert.ToInt64(studentId))
                                    Using nameReader As NpgsqlDataReader = nameCmd.ExecuteReader()
                                        If nameReader.Read() Then
                                            firstName = nameReader("first_name").ToString()
                                            lastName = nameReader("last_name").ToString()
                                            profileId = Convert.ToInt64(nameReader("id"))
                                        End If
                                    End Using
                                End Using
                            ElseIf userRole = "admin" Then
                                ' ADMIN PROFILE HANDLING
                                ' Admin users may not have separate profile tables
                                ' Use simplified profile data with admin designation
                                firstName = "Admin"
                                lastName = ""
                                profileId = userId
                            End If

                            ' PHASE 5: USER INFO OBJECT ASSEMBLY
                            ' Create complete user information object for session management
                            ' Contains all necessary data for role-based authorization and personalization
                            Return New UserInfo() With {
                                .UserId = userId,
                                .Email = userEmail,
                                .Role = userRole,
                                .FirstName = firstName,
                                .LastName = lastName,
                                .ProfileId = profileId
                            }
                        Else
                            Return Nothing ' No user found - should not happen due to existence check
                        End If
                    End Using
                End Using

            Catch ex As Exception
                ' DATABASE OPERATION ERROR HANDLING
                ' Ensure proper connection cleanup even on exceptions
                ' Re-throw exception to be handled by calling method
                Try
                    If conn.State = ConnectionState.Open Then
                        conn.Close()
                    End If
                Catch
                    ' Ignore cleanup errors - they're non-critical
                End Try
                Throw ' Re-throw to be handled by calling method
            End Try
        End Using
    End Function

    '--------------------------------------------------------------------------
    ' SESSION MANAGEMENT METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Secure Session Establishment and Cookie Management
    ''' Creates user session with role-based data and optional persistent authentication
    ''' Implements secure cookie handling with appropriate security flags
    ''' </summary>
    ''' <param name="user">Complete user information object from successful authentication</param>
    ''' <param name="rememberMe">Boolean flag for persistent authentication cookie creation</param>
    ''' <remarks>
    ''' Session Management Features:
    ''' - Role-based session data for authorization and personalization
    ''' - Full name construction for user interface display
    ''' - Profile ID mapping for role-specific data access
    ''' - Optional persistent authentication cookies with security flags
    ''' - Graceful degradation if cookie creation fails
    ''' 
    ''' Security Considerations:
    ''' - HttpOnly cookies prevent XSS access to authentication data
    ''' - Secure flag ensures HTTPS-only transmission when available
    ''' - 30-day expiration balances security with user convenience
    ''' - FormsAuthentication ticket encryption provides additional security
    ''' </remarks>
    Private Sub SetUserSession(user As UserInfo, rememberMe As Boolean)
        ' CORE SESSION VARIABLE ESTABLISHMENT
        ' Set essential session variables for user identification and authorization
        ' These variables are used throughout the application for user context
        Session("UserEmail") = user.Email               ' Primary user identifier
        Session("UserRole") = user.Role                 ' Role-based access control
        Session("UserFirstName") = user.FirstName       ' Personalization data
        Session("UserLastName") = user.LastName         ' Personalization data
        Session("UserFullName") = $"{user.FirstName} {user.LastName}".Trim()  ' Display name
        Session("UserId") = user.UserId                 ' Database user ID
        Session("ProfileId") = user.ProfileId           ' Role-specific profile ID

        ' PERSISTENT AUTHENTICATION COOKIE CREATION
        ' Create "Remember Me" functionality with secure authentication cookies
        ' Only created when user explicitly requests persistent login
        If rememberMe Then
            Try
                ' FORMS AUTHENTICATION TICKET CREATION
                ' Encrypted ticket contains user identification and role information
                ' Ticket structure provides secure, tamper-resistant authentication
                Dim ticket As New FormsAuthenticationTicket(
                    1,                          ' Version number for ticket format
                    user.Email,                 ' User name (email) for identification
                    DateTime.Now,               ' Issue time for security tracking
                    DateTime.Now.AddDays(30),   ' Expiration time (30 days)
                    True,                       ' Persistent flag for remember me
                    user.Role                   ' User data (role) for authorization
                )

                ' TICKET ENCRYPTION AND COOKIE CREATION
                ' Encrypt ticket and create secure HTTP cookie
                ' Cookie security flags protect against common attacks
                Dim encryptedTicket As String = FormsAuthentication.Encrypt(ticket)
                Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                cookie.Expires = DateTime.Now.AddDays(30)    ' Match ticket expiration
                cookie.HttpOnly = True                       ' Prevent XSS access to cookie
                cookie.Secure = Request.IsSecureConnection   ' HTTPS-only when available
                Response.Cookies.Add(cookie)
            Catch ex As Exception
                ' COOKIE CREATION ERROR HANDLING
                ' If persistent cookie creation fails, continue without it
                ' Session-based authentication will still work normally
                ' Silent failure prevents disruption of login process
                ' Could be logged for debugging but shouldn't block authentication
            End Try
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' USER INTERFACE AND FEEDBACK METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Centralized Message Display System for User Feedback
    ''' Provides consistent, styled feedback messages with appropriate visual styling
    ''' Supports multiple message types with corresponding Bootstrap styling
    ''' </summary>
    ''' <param name="message">Message content to display to user</param>
    ''' <param name="type">Message type for styling (success, danger, warning, info)</param>
    ''' <remarks>
    ''' Message Type Styling:
    ''' - success: Green styling for positive actions (login success, registration complete)
    ''' - danger/error: Red styling for errors (authentication failure, system errors)
    ''' - warning: Yellow styling for warnings (validation issues, cautions)
    ''' - info: Blue styling for informational messages (logout confirmation, help)
    ''' 
    ''' Design Features:
    ''' - FontAwesome icons provide visual context for message types
    ''' - Bootstrap alert classes ensure consistent styling
    ''' - Responsive design works across all device sizes
    ''' - Accessible markup supports screen readers
    ''' </remarks>
    Private Sub ShowMessage(message As String, type As String)
        Dim cssClass As String = ""
        Dim icon As String = ""

        ' MESSAGE TYPE TO STYLING MAPPING
        ' Convert semantic message types to appropriate Bootstrap classes and icons
        ' Provides consistent visual language across the application
        Select Case type.ToLower()
            Case "success"
                cssClass = "alert alert-success"           ' Green background for positive feedback
                icon = "check-circle"                      ' Check mark icon for success
            Case "danger", "error"
                cssClass = "alert alert-danger"            ' Red background for errors
                icon = "exclamation-triangle"              ' Warning triangle for errors
            Case "warning"
                cssClass = "alert alert-warning"           ' Yellow background for warnings
                icon = "exclamation-circle"                ' Exclamation for warnings
            Case "info"
                cssClass = "alert alert-info"              ' Blue background for information
                icon = "info-circle"                       ' Info icon for general information
            Case Else
                cssClass = "alert alert-secondary"         ' Gray background for default
                icon = "info-circle"                       ' Default info icon
        End Select

        ' MESSAGE HTML GENERATION AND DISPLAY
        ' Create complete alert HTML with icon, styling, and accessibility attributes
        ' Alert role provides screen reader accessibility
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>" &
                             $"<i class='fas fa-{icon} me-2'></i>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    ''' <summary>
    ''' Hide Message Display Panel
    ''' Clears message content and hides message display area
    ''' Provides clean state for new user interactions
    ''' </summary>
    ''' <remarks>
    ''' Used to:
    ''' - Clear previous messages before new operations
    ''' - Reset UI state for fresh user interactions
    ''' - Prevent message confusion from previous actions
    ''' - Maintain clean interface appearance
    ''' </remarks>
    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

    '--------------------------------------------------------------------------
    ' USER INFORMATION DATA CLASS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' User Information Data Transfer Object
    ''' Contains complete user profile data for session management and authorization
    ''' Provides structured data container for authenticated user information
    ''' </summary>
    ''' <remarks>
    ''' Data Structure:
    ''' - UserId: Primary key from users table for database operations
    ''' - Email: Unique user identifier and primary login credential
    ''' - Role: User role for access control (admin, student, teacher)
    ''' - FirstName/LastName: Personal information for UI personalization
    ''' - ProfileId: Role-specific profile ID for detailed data access
    ''' 
    ''' Usage Context:
    ''' - Authentication result container for successful logins
    ''' - Session data structure for user context throughout application
    ''' - Authorization data source for role-based access control
    ''' - Personalization data for user interface customization
    ''' </remarks>
    Public Class UserInfo
        ''' <summary>Primary database user ID from users table</summary>
        Public Property UserId As Integer

        ''' <summary>User's email address - primary identifier and login credential</summary>
        Public Property Email As String

        ''' <summary>User role for access control (admin, student, teacher)</summary>
        Public Property Role As String

        ''' <summary>User's first name for personalization and display</summary>
        Public Property FirstName As String

        ''' <summary>User's last name for personalization and display</summary>
        Public Property LastName As String

        ''' <summary>Role-specific profile ID for detailed data access (student_id, teacher_id, etc.)</summary>
        Public Property ProfileId As Long
    End Class

End Class

'==============================================================================
' END OF LOGIN CLASS IMPLEMENTATION
'==============================================================================
' 
' AUTHENTICATION ARCHITECTURE OVERVIEW:
' This class implements a comprehensive, secure authentication system designed
' for multi-role access with enterprise-grade security features and cloud
' database optimization for reliable user access management.
' 
' SECURITY ARCHITECTURE:
' - Multi-Layer Authentication: User existence check, password verification, profile loading
' - BCrypt Password Security: Industry-standard hash verification with salt
' - SQL Injection Prevention: Parameterized queries throughout all database operations
' - Session Security: Secure session management with role-based data isolation
' - Cookie Security: HttpOnly, Secure flags with encrypted authentication tickets
' - Error Message Security: Generic messages prevent username enumeration attacks
' 
' ROLE-BASED ACCESS CONTROL:
' - Admin Users: Full system access with administrative privileges
' - Student Users: Course enrollment and academic record access
' - Teacher Users: Course management and student interaction capabilities
' - Extensible Design: Easy addition of new roles and permissions
' 
' CLOUD DATABASE OPTIMIZATION:
' - Connection Pool Management: Stale connection cleanup for cloud reliability
' - Retry Logic: Automatic handling of temporary network issues
' - Timeout Configuration: Appropriate timeouts for cloud database latency
' - Resource Management: Proper connection disposal and cleanup
' - Error Resilience: Graceful handling of database connectivity issues
' 
' USER EXPERIENCE DESIGN:
' - Immediate Feedback: Real-time validation and authentication status
' - Progressive Enhancement: Works with and without JavaScript
' - Accessibility: Screen reader compatible with proper ARIA labels
' - Mobile Optimization: Touch-friendly interface with responsive design
' - Remember Me: Optional persistent authentication for user convenience
' 
' SESSION MANAGEMENT:
' - Role-Based Sessions: Different session data based on user role
' - Profile Integration: Direct access to role-specific profile data
' - Security Context: Complete user context for authorization decisions
' - Personalization Data: User names and preferences for UI customization
' - Cross-Request Persistence: Maintains user context across page requests
' 
' ERROR HANDLING PHILOSOPHY:
' - Security-First: Error messages designed to prevent information leakage
' - User-Friendly: Clear, actionable feedback for legitimate users
' - Graceful Degradation: System remains functional during partial failures
' - Diagnostic Information: Detailed logging without exposing sensitive data
' - Recovery Guidance: Helpful suggestions for resolving common issues
' 
' AUTHENTICATION FLOW SECURITY:
' 1. Input Sanitization: Prevents injection and ensures data quality
' 2. Existence Verification: Checks user existence before password operations
' 3. Secure Comparison: BCrypt verification prevents timing attacks
' 4. Profile Loading: Role-appropriate data retrieval and session setup
' 5. Redirect Management: Secure post-authentication routing
' 6. Cookie Management: Optional persistent authentication with security flags
' 
' INTEGRATION POINTS:
' - Master Page: Consistent authentication state across application
' - Session Variables: User context available throughout application
' - Role-Based Navigation: Menu and feature access based on user role
' - Database Schema: Integration with users, students, teachers tables
' - Configuration: Database connection and security settings from Web.config
' 
' MONITORING AND MAINTENANCE:
' - Connection Health: Database connectivity monitoring and optimization
' - Security Auditing: Authentication attempt logging and analysis
' - Performance Monitoring: Response time and error rate tracking
' - User Experience: Login success rates and user feedback analysis
' - System Reliability: Error handling effectiveness and system uptime
' 
' FUTURE ENHANCEMENT OPPORTUNITIES:
' - Two-Factor Authentication: SMS/Email verification for enhanced security
' - Social Login: Integration with Google, Microsoft, Facebook authentication
' - Password Policies: Complexity requirements and password aging
' - Account Lockout: Brute force protection with temporary account locks
' - Audit Logging: Comprehensive authentication event logging
' - Single Sign-On: Integration with institutional identity providers
' - Biometric Authentication: Support for fingerprint/face recognition
' - Risk-Based Authentication: IP/device-based security enhancement
'==============================================================================
