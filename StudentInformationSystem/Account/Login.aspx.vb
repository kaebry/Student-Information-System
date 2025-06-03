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

            ' Focus on email field
            txtEmail.Focus()
        End If
    End Sub

    'Protected Sub btnLogin_Click(sender As Object, e As EventArgs)
    '    ' Clear any previous messages
    '    HideMessage()

    '    ' Validate the page
    '    If Not Page.IsValid Then
    '        ShowMessage("Please correct the errors below and try again.", "danger")
    '        Return
    '    End If

    '    ' Test database connection first
    '    If Not TestDatabaseConnection() Then
    '        ShowMessage("Cannot connect to the database. Please check your internet connection and try again.", "danger")
    '        Return
    '    End If

    '    Try
    '        ' Get form values
    '        Dim email As String = txtEmail.Text.Trim().ToLower()
    '        Dim password As String = txtPassword.Text.Trim()
    '        Dim rememberMe As Boolean = chkRememberMe.Checked

    '        ' Basic validation
    '        If String.IsNullOrEmpty(email) Then
    '            ShowMessage("Email is required.", "danger")
    '            Return
    '        End If

    '        If String.IsNullOrEmpty(password) Then
    '            ShowMessage("Password is required.", "danger")
    '            Return
    '        End If

    '        ' Authenticate user
    '        Dim user As UserInfo = AuthenticateUser(email, password)

    '        If user IsNot Nothing Then
    '            ' Login successful
    '            Try
    '                SetUserSession(user, rememberMe)
    '                ShowMessage("Login successful! Redirecting...", "success")

    '                ' Check if there's a return URL
    '                Dim returnUrl As String = Request.QueryString("ReturnUrl")
    '                If String.IsNullOrEmpty(returnUrl) Then
    '                    returnUrl = "~/Default.aspx"
    '                End If

    '                ' Redirect after a short delay to show the success message
    '                ClientScript.RegisterStartupScript(Me.GetType(), "redirect",
    '                    $"setTimeout(function(){{ window.location.href='{ResolveUrl(returnUrl)}'; }}, 1500);", True)

    '            Catch sessionEx As Exception
    '                ShowMessage($"Login successful but session setup failed: {sessionEx.Message}", "warning")
    '                ' Try direct redirect without session
    '                Response.Redirect("~/Default.aspx")
    '            End Try
    '        Else
    '            ' Login failed
    '            ShowMessage("Invalid email or password. Please try again.", "danger")
    '            txtPassword.Text = "" ' Clear password field
    '        End If

    '    Catch ex As Exception
    '        ShowMessage("An error occurred during login: " & ex.Message, "danger")
    '    End Try
    'End Sub
    Protected Sub btnLogin_Click(sender As Object, e As EventArgs)
        ' Clear any previous messages
        HideMessage()

        Try
            ' Get form values
            Dim email As String = txtEmail.Text.Trim().ToLower()
            Dim password As String = txtPassword.Text.Trim()

            ShowMessage($"🔄 Starting login process for: {email}", "info")

            ' Basic validation
            If String.IsNullOrEmpty(email) Then
                ShowMessage("❌ Email is required.", "danger")
                Return
            End If

            If String.IsNullOrEmpty(password) Then
                ShowMessage("❌ Password is required.", "danger")
                Return
            End If

            ShowMessage("✅ Basic validation passed", "success")

            ' Test database connection first
            ShowMessage("🔄 Testing database connection...", "info")
            If Not TestDatabaseConnection() Then
                ShowMessage("❌ Cannot connect to the database.", "danger")
                Return
            End If
            ShowMessage("✅ Database connection successful", "success")

            ' Debug: Show what we're about to call
            ShowMessage($"🔄 Calling AuthenticateUser with email='{email}' and password length={password.Length}", "info")

            ' Authenticate user
            Dim user As UserInfo = Nothing
            Try
                user = AuthenticateUser(email, password)
                ShowMessage($"🔄 AuthenticateUser completed. Result: {If(user Is Nothing, "Nothing", "UserInfo object")}", "info")
            Catch authEx As Exception
                ShowMessage($"❌ AuthenticateUser threw exception: {authEx.Message}", "danger")
                Return
            End Try

            If user IsNot Nothing Then
                ShowMessage($"✅ Authentication successful for user: {user.Email}", "success")

                ' Try to set session
                Try
                    ShowMessage("🔄 Setting user session...", "info")
                    SetUserSession(user, chkRememberMe.Checked)
                    ShowMessage("✅ Session set successfully", "success")

                    ShowMessage("🔄 Login successful! Redirecting...", "success")

                    ' Check if there's a return URL
                    Dim returnUrl As String = Request.QueryString("ReturnUrl")
                    If String.IsNullOrEmpty(returnUrl) Then
                        returnUrl = "~/Default.aspx"
                    End If

                    ' Redirect after a short delay
                    ClientScript.RegisterStartupScript(Me.GetType(), "redirect",
                    $"setTimeout(function(){{ window.location.href='{ResolveUrl(returnUrl)}'; }}, 2000);", True)

                Catch sessionEx As Exception
                    ShowMessage($"❌ Session setup failed: {sessionEx.Message}", "danger")
                    Return
                End Try
            Else
                ShowMessage("❌ AuthenticateUser returned Nothing - Invalid email or password", "danger")
                txtPassword.Text = "" ' Clear password field
            End If

        Catch ex As Exception
            ShowMessage($"❌ Login process error: {ex.Message}<br/>Stack trace: {ex.StackTrace}", "danger")
        End Try
    End Sub


    ' Also improve the TestDatabaseConnection method to not interfere
    Private Function TestDatabaseConnection() As Boolean
        Try
            ' Use a separate connection string instance
            Dim testConnStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

            If Not testConnStr.Contains("Timeout") Then
                testConnStr &= ";Connection Timeout=10;Command Timeout=10;"
            End If

            Using testConn As New NpgsqlConnection(testConnStr)
                testConn.Open()
                Using cmd As New NpgsqlCommand("SELECT 1", testConn)
                    cmd.CommandTimeout = 10
                    cmd.ExecuteScalar()
                End Using
            End Using

            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    ' Replace your AuthenticateUser method with this robust version
    Private Function AuthenticateUser(email As String, password As String) As UserInfo
        Dim maxRetries As Integer = 3
        Dim retryDelay As Integer = 500 ' milliseconds

        For attempt As Integer = 1 To maxRetries
            Try
                Return AuthenticateUserAttempt(email, password, attempt)
            Catch ex As Exception
                If attempt = maxRetries Then
                    ' Last attempt failed, return Nothing
                    Return Nothing
                Else
                    ' Wait before retry
                    System.Threading.Thread.Sleep(retryDelay)
                End If
            End Try
        Next

        Return Nothing
    End Function

    Private Function AuthenticateUserAttempt(email As String, password As String, attempt As Integer) As UserInfo
        ' Create fresh connection string for each attempt
        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        ' Ensure clean connection parameters
        If Not connStr.Contains("Timeout") Then
            connStr &= ";Connection Timeout=30;Command Timeout=30;Connection Idle Lifetime=300;Max Pool Size=50;Min Pool Size=1;"
        End If

        ' Add connection pooling parameters to ensure clean connections
        connStr = connStr.Replace("Pooling=true", "")
        connStr &= ";Pooling=true;Connection Pruning Interval=10;"

        Dim conn As NpgsqlConnection = Nothing
        Try
            conn = New NpgsqlConnection(connStr)
            conn.Open()

            ' Verify connection is actually working
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 10
                testCmd.ExecuteScalar()
            End Using

            ' Simple query first to check if user exists
            Using checkCmd As New NpgsqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", conn)
                checkCmd.CommandTimeout = 30
                checkCmd.Parameters.AddWithValue("@email", email)
                Dim userCount As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                If userCount = 0 Then
                    Return Nothing ' User doesn't exist
                End If
            End Using

            ' Get user basic info first
            Using userCmd As New NpgsqlCommand("SELECT id, email, password_hash, role, student_id, teacher_id FROM users WHERE email = @email", conn)
                userCmd.CommandTimeout = 30
                userCmd.Parameters.AddWithValue("@email", email)

                Using reader As NpgsqlDataReader = userCmd.ExecuteReader()
                    If reader.Read() Then
                        Dim userId As Integer = Convert.ToInt32(reader("id"))
                        Dim userEmail As String = reader("email").ToString()
                        Dim storedPasswordHash As String = reader("password_hash").ToString()
                        Dim userRole As String = reader("role").ToString()
                        Dim studentId As Object = reader("student_id")
                        Dim teacherId As Object = reader("teacher_id")

                        ' Verify password
                        If Not BCrypt.Net.BCrypt.Verify(password, storedPasswordHash) Then
                            Return Nothing ' Password doesn't match
                        End If

                        reader.Close()

                        ' Get name information separately to avoid complex JOIN issues
                        Dim firstName As String = ""
                        Dim lastName As String = ""
                        Dim profileId As Long = 0

                        If userRole = "student" AndAlso studentId IsNot DBNull.Value Then
                            Using nameCmd As New NpgsqlCommand("SELECT first_name, last_name, id FROM students WHERE id = @id", conn)
                                nameCmd.CommandTimeout = 30
                                nameCmd.Parameters.AddWithValue("@id", Convert.ToInt64(studentId))
                                Using nameReader As NpgsqlDataReader = nameCmd.ExecuteReader()
                                    If nameReader.Read() Then
                                        firstName = nameReader("first_name").ToString()
                                        lastName = nameReader("last_name").ToString()
                                        profileId = Convert.ToInt64(nameReader("id"))
                                    End If
                                End Using
                            End Using
                        ElseIf userRole = "teacher" AndAlso teacherId IsNot DBNull.Value Then
                            Using nameCmd As New NpgsqlCommand("SELECT first_name, last_name, id FROM teachers WHERE id = @id", conn)
                                nameCmd.CommandTimeout = 30
                                nameCmd.Parameters.AddWithValue("@id", Convert.ToInt32(teacherId))
                                Using nameReader As NpgsqlDataReader = nameCmd.ExecuteReader()
                                    If nameReader.Read() Then
                                        firstName = nameReader("first_name").ToString()
                                        lastName = nameReader("last_name").ToString()
                                        profileId = Convert.ToInt64(nameReader("id"))
                                    End If
                                End Using
                            End Using
                        End If

                        ' Create and return user info
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
            ' Close connection if it's still open
            If conn IsNot Nothing AndAlso conn.State = ConnectionState.Open Then
                Try
                    conn.Close()
                Catch
                    ' Ignore close errors
                End Try
            End If

            ' Re-throw the exception to be caught by the retry logic
            Throw

        Finally
            ' Ensure connection is properly disposed
            If conn IsNot Nothing Then
                Try
                    conn.Dispose()
                Catch
                    ' Ignore disposal errors
                End Try
            End If
        End Try

        Return Nothing
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
        Public Property ProfileId As Long  ' Changed to Long to handle bigint from students table
    End Class

End Class
