Imports System.Configuration
Imports System.Data
Imports Npgsql
Imports BCrypt.Net
Imports System.Web.Security

Partial Public Class Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' Check if user is already logged in
            If User.Identity.IsAuthenticated OrElse Session("UserEmail") IsNot Nothing Then
                Response.Redirect("~/Default.aspx")
            End If

            ' Check if coming from registration page
            If Request.QueryString("registered") = "true" Then
                pnlRegistrationSuccess.Visible = True
            End If

            ' Check if logged out
            If Request.QueryString("loggedout") = "true" Then
                ShowMessage("✅ You have been logged out successfully.", "success")
            End If

            ' Focus on email field
            txtEmail.Focus()
        End If
    End Sub

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs)
        ' Clear any previous messages
        HideMessage()

        ' Validate the page first
        If Not Page.IsValid Then
            ShowMessage("Please correct the errors below and try again.", "danger")
            Return
        End If

        Try
            ' Get form values
            Dim email As String = txtEmail.Text.Trim().ToLower()
            Dim password As String = txtPassword.Text.Trim()

            ' Basic validation
            If String.IsNullOrEmpty(email) Then
                ShowMessage("❌ Email is required.", "danger")
                Return
            End If

            If String.IsNullOrEmpty(password) Then
                ShowMessage("❌ Password is required.", "danger")
                Return
            End If

            ' Authenticate user directly (no pre-connection test)
            Dim user As UserInfo = AuthenticateUser(email, password)

            If user IsNot Nothing Then
                ' Login successful
                Try
                    SetUserSession(user, chkRememberMe.Checked)
                    ShowMessage("✅ Login successful! Redirecting...", "success")

                    ' Check if there's a return URL
                    Dim returnUrl As String = Request.QueryString("ReturnUrl")
                    If String.IsNullOrEmpty(returnUrl) Then
                        returnUrl = "~/Default.aspx"
                    End If

                    ' Redirect after a short delay
                    ClientScript.RegisterStartupScript(Me.GetType(), "redirect",
                        $"setTimeout(function(){{ window.location.href='{ResolveUrl(returnUrl)}'; }}, 1500);", True)

                Catch sessionEx As Exception
                    ShowMessage($"❌ Login successful but session setup failed: {sessionEx.Message}", "danger")
                    Return
                End Try
            Else
                ' Login failed - clear password field and show proper error
                ShowMessage("❌ Invalid email or password. Please check your credentials and try again.", "danger")
                txtPassword.Text = "" ' Clear password field for security
            End If

        Catch connEx As Exception When connEx.Message.Contains("connection") OrElse
                                        connEx.Message.Contains("timeout") OrElse
                                        connEx.Message.Contains("network") OrElse
                                        connEx.Message.Contains("database")
            ShowMessage("❌ Cannot connect to the database. Please check your internet connection and try again.", "danger")
        Catch ex As Exception
            ShowMessage($"❌ Login error: {ex.Message}", "danger")
        End Try
    End Sub

    ' Authenticate user with robust error handling
    Private Function AuthenticateUser(email As String, password As String) As UserInfo
        Try
            ' Clear any stale connections first
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(100)

            Return AuthenticateUserDirect(email, password)
        Catch connEx As Exception When connEx.Message.Contains("connection") OrElse
                                         connEx.Message.Contains("timeout") OrElse
                                         connEx.Message.Contains("stream") OrElse
                                         connEx.Message.Contains("network")
            ' For connection issues, throw to be caught by the main handler
            Throw New Exception("Database connection failed. Please try again.")
        Catch ex As Exception
            ' For any other error (like invalid credentials), just return Nothing
            ' Don't expose internal errors to the user
            Return Nothing
        End Try
    End Function

    ' Direct authentication method with improved connection handling
    Private Function AuthenticateUserDirect(email As String, password As String) As UserInfo
        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        Using conn As New NpgsqlConnection(connStr)
            Try
                conn.Open()

                ' Check if user exists
                Using checkCmd As New NpgsqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", conn)
                    checkCmd.CommandTimeout = 15
                    checkCmd.Parameters.AddWithValue("@email", email)
                    Dim userExists As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                    If userExists = 0 Then
                        Return Nothing ' User doesn't exist
                    End If
                End Using

                ' Get user info and verify password
                Using userCmd As New NpgsqlCommand("SELECT id, email, password_hash, role, student_id FROM users WHERE email = @email", conn)
                    userCmd.CommandTimeout = 15
                    userCmd.Parameters.AddWithValue("@email", email)

                    Using reader As NpgsqlDataReader = userCmd.ExecuteReader()
                        If reader.Read() Then
                            Dim userId As Integer = Convert.ToInt32(reader("id"))
                            Dim userEmail As String = reader("email").ToString()
                            Dim storedPasswordHash As String = reader("password_hash").ToString()
                            Dim userRole As String = reader("role").ToString()
                            Dim studentId As Object = reader("student_id")

                            ' Verify password
                            If Not BCrypt.Net.BCrypt.Verify(password, storedPasswordHash) Then
                                Return Nothing ' Password doesn't match
                            End If

                            reader.Close()

                            ' Get additional user details based on role
                            Dim firstName As String = ""
                            Dim lastName As String = ""
                            Dim profileId As Long = 0

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
                                ' For admin users, use email as display name
                                firstName = "Admin"
                                lastName = ""
                                profileId = userId
                            End If

                            ' Return user info
                            Return New UserInfo() With {
                                .UserId = userId,
                                .Email = userEmail,
                                .Role = userRole,
                                .FirstName = firstName,
                                .LastName = lastName,
                                .ProfileId = profileId
                            }
                        Else
                            Return Nothing ' No user found
                        End If
                    End Using
                End Using

            Catch ex As Exception
                ' Ensure connection is properly cleaned up
                Try
                    If conn.State = ConnectionState.Open Then
                        conn.Close()
                    End If
                Catch
                End Try
                Throw ' Re-throw to be handled by calling method
            End Try
        End Using
    End Function

    Private Sub SetUserSession(user As UserInfo, rememberMe As Boolean)
        ' Set session variables
        Session("UserEmail") = user.Email
        Session("UserRole") = user.Role
        Session("UserFirstName") = user.FirstName
        Session("UserLastName") = user.LastName
        Session("UserFullName") = $"{user.FirstName} {user.LastName}".Trim()
        Session("UserId") = user.UserId
        Session("ProfileId") = user.ProfileId

        ' Set authentication cookie if remember me is checked
        If rememberMe Then
            Try
                Dim ticket As New FormsAuthenticationTicket(
                    1, ' version
                    user.Email, ' name
                    DateTime.Now, ' issue time
                    DateTime.Now.AddDays(30), ' expiration
                    True, ' persistent
                    user.Role ' user data
                )

                Dim encryptedTicket As String = FormsAuthentication.Encrypt(ticket)
                Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                cookie.Expires = DateTime.Now.AddDays(30)
                cookie.HttpOnly = True
                cookie.Secure = Request.IsSecureConnection
                Response.Cookies.Add(cookie)
            Catch ex As Exception
                ' If cookie creation fails, just continue without it
                ' The session will still work
            End Try
        End If
    End Sub

    Private Sub ShowMessage(message As String, type As String)
        Dim cssClass As String = ""
        Dim icon As String = ""

        Select Case type.ToLower()
            Case "success"
                cssClass = "alert alert-success"
                icon = "check-circle"
            Case "danger", "error"
                cssClass = "alert alert-danger"
                icon = "exclamation-triangle"
            Case "warning"
                cssClass = "alert alert-warning"
                icon = "exclamation-circle"
            Case "info"
                cssClass = "alert alert-info"
                icon = "info-circle"
            Case Else
                cssClass = "alert alert-secondary"
                icon = "info-circle"
        End Select

        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>" &
                             $"<i class='fas fa-{icon} me-2'></i>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

    ' User information class
    Public Class UserInfo
        Public Property UserId As Integer
        Public Property Email As String
        Public Property Role As String
        Public Property FirstName As String
        Public Property LastName As String
        Public Property ProfileId As Long
    End Class

End Class
