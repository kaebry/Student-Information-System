Imports System.Configuration
Imports System.Data
Imports Npgsql
Imports BCrypt.Net

Partial Class Register
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' Check if user is already logged in
            If User.Identity.IsAuthenticated Then
                Response.Redirect("~/Default.aspx")
            End If
        End If
    End Sub

    Protected Sub ddlUserRole_SelectedIndexChanged(sender As Object, e As EventArgs)
        ' Show/hide fields based on selected role
        Select Case ddlUserRole.SelectedValue.ToLower()
            Case "student"
                pnlStudentFields.Visible = True
            Case Else
                pnlStudentFields.Visible = False
        End Select
    End Sub

    Protected Sub btnRegister_Click(sender As Object, e As EventArgs)
        ' Clear any previous messages
        HideMessage()

        ' Validate the page
        If Not Page.IsValid Then
            ShowMessage("Please correct the errors below and try again.", "danger")
            Return
        End If

        ' Test database connection first
        If Not TestDatabaseConnection() Then
            ShowMessage("Cannot connect to the database. Please check your internet connection and try again.", "danger")
            Return
        End If

        Try
            ' Get form values
            Dim firstName As String = txtFirstName.Text.Trim()
            Dim lastName As String = txtLastName.Text.Trim()
            Dim email As String = txtEmail.Text.Trim().ToLower()
            Dim password As String = txtPassword.Text.Trim()
            Dim role As String = ddlUserRole.SelectedValue

            ' Basic validation
            If String.IsNullOrEmpty(firstName) Then
                ShowMessage("First name is required.", "danger")
                Return
            End If

            If String.IsNullOrEmpty(lastName) Then
                ShowMessage("Last name is required.", "danger")
                Return
            End If

            If String.IsNullOrEmpty(email) Then
                ShowMessage("Email is required.", "danger")
                Return
            End If

            If String.IsNullOrEmpty(password) Then
                ShowMessage("Password is required.", "danger")
                Return
            End If

            If password.Length < 6 Then
                ShowMessage("Password must be at least 6 characters long.", "danger")
                Return
            End If

            If password <> txtConfirmPassword.Text Then
                ShowMessage("Passwords do not match.", "danger")
                Return
            End If

            If String.IsNullOrEmpty(role) Then
                ShowMessage("Please select your role.", "danger")
                Return
            End If

            ' Check if email already exists
            If EmailExists(email) Then
                ShowMessage("An account with this email already exists. Please use a different email or try logging in.", "danger")
                Return
            End If

            ' Hash the password using BCrypt
            Dim hashedPassword As String = BCrypt.Net.BCrypt.HashPassword(password)

            ' Create user account
            Dim success As Boolean = CreateUserAccount(firstName, lastName, email, hashedPassword, role)

            If success Then
                ' Registration successful
                ShowMessage("Registration successful! You can now log in with your credentials.", "success")
                ClearForm()

                ' Optional: Auto-redirect to login page after a delay
                ClientScript.RegisterStartupScript(Me.GetType(), "redirect",
                    "setTimeout(function(){ window.location.href='Login.aspx?registered=true'; }, 3000);", True)
            Else
                ShowMessage("Registration failed. Please try again.", "danger")
            End If

        Catch ex As Exception
            ShowMessage("An error occurred during registration: " & ex.Message, "danger")
        End Try
    End Sub

    Private Function TestDatabaseConnection() As Boolean
        Try
            Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

            ' Add timeout parameters if not already present
            If Not connStr.Contains("Timeout") Then
                connStr &= ";Timeout=10;Command Timeout=10;"
            End If

            Using conn As New NpgsqlConnection(connStr)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT 1", conn)
                    cmd.CommandTimeout = 10
                    cmd.ExecuteScalar()
                End Using
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function EmailExists(email As String) As Boolean
        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        Try
            Using conn As New NpgsqlConnection(connStr)
                conn.Open()

                ' Check in users table
                Dim query As String = "SELECT COUNT(*) FROM users WHERE email = @email"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@email", email)
                    Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        Catch ex As Exception
            ' Log error if needed, but don't block registration
            Return False
        End Try
    End Function

    Private Function CreateUserAccount(firstName As String, lastName As String, email As String, hashedPassword As String, role As String) As Boolean
        ' Improved connection string with timeout settings
        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        ' Add timeout parameters if not already present
        If Not connStr.Contains("Timeout") Then
            connStr &= ";Timeout=30;Command Timeout=30;Connection Idle Lifetime=300;"
        End If

        Dim transaction As NpgsqlTransaction = Nothing
        Dim conn As NpgsqlConnection = Nothing

        Try
            conn = New NpgsqlConnection(connStr)

            ' Set connection timeout
            conn.Open()

            ' Test connection first
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 30
                testCmd.ExecuteScalar()
            End Using

            ' Start transaction
            transaction = conn.BeginTransaction()

            Dim relatedId As Integer = 0

            ' Insert into appropriate table based on role
            If role = "student" Then
                relatedId = CreateStudentRecord(conn, transaction, firstName, lastName, email)
            End If

            ' Insert into users table
            CreateUserRecord(conn, transaction, email, hashedPassword, role, relatedId)

            ' Commit transaction
            transaction.Commit()

            Return True

        Catch timeoutEx As TimeoutException
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch
                End Try
            End If
            Throw New Exception("Database connection timeout. Please check your internet connection and try again.")

        Catch socketEx As System.Net.Sockets.SocketException
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch
                End Try
            End If
            Throw New Exception("Network connection failed. Please check your internet connection and try again.")

        Catch npgsqlEx As NpgsqlException
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch
                End Try
            End If

            ' Handle specific Npgsql errors
            If npgsqlEx.Message.Contains("timeout") OrElse npgsqlEx.Message.Contains("connexion") Then
                Throw New Exception("Database connection timeout. Please try again in a moment.")
            Else
                Throw New Exception("Database error: " & npgsqlEx.Message)
            End If

        Catch ex As Exception
            If transaction IsNot Nothing Then
                Try
                    transaction.Rollback()
                Catch
                End Try
            End If
            Throw New Exception("Registration failed: " & ex.Message)
        Finally
            ' Clean up resources
            If transaction IsNot Nothing Then
                Try
                    transaction.Dispose()
                Catch
                End Try
            End If
            If conn IsNot Nothing Then
                Try
                    If conn.State = ConnectionState.Open Then
                        conn.Close()
                    End If
                    conn.Dispose()
                Catch
                End Try
            End If
        End Try

        Return False
    End Function

    Private Function CreateStudentRecord(conn As NpgsqlConnection, transaction As NpgsqlTransaction, firstName As String, lastName As String, email As String) As Integer
        Try
            Dim query As String = "INSERT INTO students (first_name, last_name, email, enrollment_date) VALUES (@fn, @ln, @em, @ed) RETURNING id"

            Using cmd As New NpgsqlCommand(query, conn, transaction)
                cmd.CommandTimeout = 30 ' 30 seconds timeout
                cmd.Parameters.Add("@fn", NpgsqlTypes.NpgsqlDbType.Text).Value = firstName
                cmd.Parameters.Add("@ln", NpgsqlTypes.NpgsqlDbType.Text).Value = lastName
                cmd.Parameters.Add("@em", NpgsqlTypes.NpgsqlDbType.Text).Value = email
                cmd.Parameters.Add("@ed", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTime.Today

                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso IsNumeric(result) Then
                    Return Convert.ToInt32(result)
                Else
                    Throw New Exception("Failed to get student ID after insert - no ID returned")
                End If
            End Using
        Catch ex As Exception
            Throw New Exception("Failed to create student record: " & ex.Message)
        End Try
    End Function



    Private Sub CreateUserRecord(conn As NpgsqlConnection, transaction As NpgsqlTransaction, email As String, hashedPassword As String, role As String, relatedId As Integer)
        Try
            Dim query As String = ""

            If role = "student" Then
                query = "INSERT INTO users (email, password_hash, role, student_id) VALUES (@em, @pw, @rl, @rid)"
            Else
                Throw New Exception("Invalid role: " & role)
            End If

            Using cmd As New NpgsqlCommand(query, conn, transaction)
                cmd.CommandTimeout = 30 ' 30 seconds timeout
                cmd.Parameters.Add("@em", NpgsqlTypes.NpgsqlDbType.Text).Value = email
                cmd.Parameters.Add("@pw", NpgsqlTypes.NpgsqlDbType.Text).Value = hashedPassword
                cmd.Parameters.Add("@rl", NpgsqlTypes.NpgsqlDbType.Text).Value = role
                cmd.Parameters.Add("@rid", NpgsqlTypes.NpgsqlDbType.Integer).Value = relatedId

                Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                If rowsAffected = 0 Then
                    Throw New Exception("No rows were inserted into users table")
                End If
            End Using
        Catch ex As Exception
            Throw New Exception("Failed to create user record: " & ex.Message)
        End Try
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

    Private Sub ClearForm()
        txtFirstName.Text = ""
        txtLastName.Text = ""
        txtEmail.Text = ""
        txtPassword.Text = ""
        txtConfirmPassword.Text = ""
        txtDateOfBirth.Text = ""
        ddlUserRole.SelectedIndex = 0
        ddlProgram.SelectedIndex = 0
        ddlYearLevel.SelectedIndex = 0


        ' Hide role-specific panels
        pnlStudentFields.Visible = False

    End Sub

End Class

