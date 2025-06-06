' =============================================
' REGISTER PAGE CODE‑BEHIND (Register.aspx.vb)
' ------------------------------------------------
' Handles the user‑registration workflow:
'  • Validates form input on post‑back
'  • Hashes passwords with BCrypt
'  • Inserts a new student + user record in PostgreSQL
'  • Shows feedback messages to the user
' Technologies : ASP.NET Web Forms + VB.NET + Npgsql + BCrypt
' Author       : [Add your name]
' Date         : [Add date]
' =============================================

' --- Framework / library imports ---
Imports System.Configuration      ' Access to <connectionStrings> in Web.config
Imports System.Data               ' ADO.NET base classes
Imports Npgsql                    ' PostgreSQL ADO.NET provider
Imports BCrypt.Net                ' BCrypt hashing helpers

' Code‑behind class bound to Register.aspx
Partial Class Register
    Inherits System.Web.UI.Page

    ' =============================================================
    ' Page_Load
    ' -------------------------------------------------------------
    ' • Redirects authenticated users away from the register page
    ' • Sets focus on the first‑name textbox when the page loads
    ' =============================================================
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' If the user is already logged in, skip registration
            If User.Identity.IsAuthenticated Then
                Response.Redirect("~/Default.aspx")
            End If

            ' UX: place the cursor in the first‑name field
            txtFirstName.Focus()
        End If
    End Sub

    ' =====================================================================
    ' btnRegister_Click – raised when the user presses the Register button
    ' =====================================================================
    Protected Sub btnRegister_Click(sender As Object, e As EventArgs)
        ' Hide any previous bootstrap alert
        HideMessage()

        ' --------------------------------------------------------
        ' 1) ASP.NET validation – check RequiredField/Regex, etc.
        ' --------------------------------------------------------
        If Not Page.IsValid Then
            ShowMessage("Please correct the errors below and try again.", "danger")
            Return
        End If

        ' --------------------------------------------------------
        ' 2) Make sure we can reach the database before doing work
        ' --------------------------------------------------------
        If Not TestDatabaseConnection() Then
            ShowMessage("Cannot connect to the database. Please check your internet connection and try again.", "danger")
            Return
        End If

        Try
            ' --------------------------------------------------
            ' 3) Collect + trim the form values
            ' --------------------------------------------------
            Dim firstName As String = txtFirstName.Text.Trim()
            Dim lastName As String = txtLastName.Text.Trim()
            Dim email As String = txtEmail.Text.Trim().ToLower()
            Dim password As String = txtPassword.Text.Trim()
            Dim role As String = "student"   ' Fixed role – you can extend later

            ' --------------------------------------------------
            ' 4) Extra manual validation (length, matches, etc.)
            ' --------------------------------------------------
            If String.IsNullOrEmpty(firstName) Then
                ShowMessage("First name is required.", "danger") : Return
            End If
            If String.IsNullOrEmpty(lastName) Then
                ShowMessage("Last name is required.", "danger") : Return
            End If
            If String.IsNullOrEmpty(email) Then
                ShowMessage("Email is required.", "danger") : Return
            End If
            If String.IsNullOrEmpty(password) Then
                ShowMessage("Password is required.", "danger") : Return
            End If
            If password.Length < 6 Then
                ShowMessage("Password must be at least 6 characters long.", "danger") : Return
            End If
            If password <> txtConfirmPassword.Text Then
                ShowMessage("Passwords do not match.", "danger") : Return
            End If

            ' --------------------------------------------------
            ' 5) Uniqueness – prevent duplicate accounts by email
            ' --------------------------------------------------
            If EmailExists(email) Then
                ShowMessage("An account with this email already exists. Please use a different email or try logging in.", "danger")
                Return
            End If

            ' --------------------------------------------------
            ' 6) BCrypt hash (salt + multiple rounds)
            ' --------------------------------------------------
            Dim hashedPassword As String = BCrypt.Net.BCrypt.HashPassword(password)

            ' --------------------------------------------------
            ' 7) Insert both student + user rows inside a Tx
            ' --------------------------------------------------
            Dim success As Boolean = CreateUserAccount(firstName, lastName, email, hashedPassword, role)

            If success Then
                ' UX: success message + clear form + auto redirect
                ShowMessage("🎉 Registration successful! You can now log in with your credentials.", "success")
                ClearForm()

                ' JavaScript redirect after 3 seconds
                ClientScript.RegisterStartupScript(Me.GetType(), "redirect", "setTimeout(function(){ window.location.href='Login.aspx?registered=true'; }, 3000);", True)
            Else
                ShowMessage("Registration failed. Please try again.", "danger")
            End If

        Catch ex As Exception
            ' Catch‑all fallback – display the message to the user
            ShowMessage("An error occurred during registration: " & ex.Message, "danger")
        End Try
    End Sub

    ' =============================================================
    ' Utility helpers – DB connection, existence checks, inserts
    ' =============================================================

    ' Quickly checks that we can open a connection AND execute a simple query
    Private Function TestDatabaseConnection() As Boolean
        Try
            Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

            ' Append timeout if missing (keeps Web.config clean)
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
        Catch
            Return False   ' Swallow specifics – we only need a boolean
        End Try
    End Function

    ' Does any user already have this email? Returns True/False
    Private Function EmailExists(email As String) As Boolean
        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        Try
            Using conn As New NpgsqlConnection(connStr)
                conn.Open()
                Dim query As String = "SELECT COUNT(*) FROM users WHERE email = @email"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@email", email)
                    Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        Catch
            ' Fail‑safe: if the check itself fails, pretend it does NOT exist
            Return False
        End Try
    End Function

    ' Creates both the student and user rows within one transaction
    Private Function CreateUserAccount(firstName As String, lastName As String, email As String, hashedPassword As String, role As String) As Boolean
        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        ' Add generous timeouts if they don’t exist already
        If Not connStr.Contains("Timeout") Then
            connStr &= ";Timeout=30;Command Timeout=30;Connection Idle Lifetime=300;"
        End If

        Dim transaction As NpgsqlTransaction = Nothing
        Dim conn As NpgsqlConnection = Nothing

        Try
            conn = New NpgsqlConnection(connStr)
            conn.Open()

            ' Sanity test
            Using sanity As New NpgsqlCommand("SELECT 1", conn)
                sanity.CommandTimeout = 30
                sanity.ExecuteScalar()
            End Using

            transaction = conn.BeginTransaction()

            ' 1) Insert into students, get new id
            Dim studentId As Integer = CreateStudentRecord(conn, transaction, firstName, lastName, email)

            ' 2) Insert into users referencing the studentId
            CreateUserRecord(conn, transaction, email, hashedPassword, role, studentId)

            transaction.Commit()
            Return True

        Catch ex As Exception
            ' Rollback if anything goes wrong
            If transaction IsNot Nothing Then
                Try : transaction.Rollback() : Catch : End Try
            End If

            ' Fine‑grained error messages for common scenarios
            If TypeOf ex Is TimeoutException OrElse ex.Message.ToLower().Contains("timeout") Then
                Throw New Exception("Database connection timeout. Please check your internet connection and try again.")
            ElseIf TypeOf ex Is System.Net.Sockets.SocketException Then
                Throw New Exception("Network connection failed. Please check your internet connection and try again.")
            ElseIf TypeOf ex Is NpgsqlException Then
                Throw New Exception("Database error: " & ex.Message)
            Else
                Throw New Exception("Registration failed: " & ex.Message)
            End If

        Finally
            ' Clean up unmanaged resources
            If transaction IsNot Nothing Then
                Try : transaction.Dispose() : Catch : End Try
            End If
            If conn IsNot Nothing Then
                Try
                    If conn.State = ConnectionState.Open Then conn.Close()
                    conn.Dispose()
                Catch : End Try
            End If
        End Try

        Return False  ' Shouldn’t reach here
    End Function

    ' Inserts the student row – returns the new PK id
    Private Function CreateStudentRecord(conn As NpgsqlConnection, transaction As NpgsqlTransaction, firstName As String, lastName As String, email As String) As Integer
        Dim query As String = "INSERT INTO students (first_name, last_name, email, enrollment_date) VALUES (@fn, @ln, @em, @ed) RETURNING id"

        Using cmd As New NpgsqlCommand(query, conn, transaction)
            cmd.CommandTimeout = 30
            cmd.Parameters.Add("@fn", NpgsqlTypes.NpgsqlDbType.Text).Value = firstName
            cmd.Parameters.Add("@ln", NpgsqlTypes.NpgsqlDbType.Text).Value = lastName
            cmd.Parameters.Add("@em", NpgsqlTypes.NpgsqlDbType.Text).Value = email
            cmd.Parameters.Add("@ed", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTime.Today

            Dim result = cmd.ExecuteScalar()
            If result IsNot Nothing AndAlso IsNumeric(result) Then
                Return Convert.ToInt32(result)
            Else
                Throw New Exception("Failed to get student ID after insert")
            End If
        End Using
    End Function

    ' Inserts the user row linked to studentId
    Private Sub CreateUserRecord(conn As NpgsqlConnection, transaction As NpgsqlTransaction, email As String, hashedPassword As String, role As String, studentId As Integer)
        Dim query As String = "INSERT INTO users (email, password_hash, role, student_id) VALUES (@em, @pw, @rl, @sid)"

        Using cmd As New NpgsqlCommand(query, conn, transaction)
            cmd.CommandTimeout = 30
            cmd.Parameters.Add("@em", NpgsqlTypes.NpgsqlDbType.Text).Value = email
            cmd.Parameters.Add("@pw", NpgsqlTypes.NpgsqlDbType.Text).Value = hashedPassword
            cmd.Parameters.Add("@rl", NpgsqlTypes.NpgsqlDbType.Text).Value = role
            cmd.Parameters.Add("@sid", NpgsqlTypes.NpgsqlDbType.Integer).Value = studentId

            Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
            If rowsAffected = 0 Then
                Throw New Exception("No rows were inserted into users table")
            End If
        End Using
    End Sub

    ' =============================================================
    ' UI helpers – display/clear bootstrap alerts and reset fields
    ' =============================================================

    ' Builds a bootstrap alert div from parameters
    Private Sub ShowMessage(message As String, type As String)
        Dim cssClass As String
        Dim icon As String

        Select Case type.ToLower()
            Case "success" : cssClass = "alert alert-success" : icon = "check-circle"
            Case "danger", "error" : cssClass = "alert alert-danger" : icon = "exclamation-triangle"
            Case "warning" : cssClass = "alert alert-warning" : icon = "exclamation-circle"
            Case "info" : cssClass = "alert alert-info" : icon = "info-circle"
            Case Else : cssClass = "alert alert-secondary" : icon = "info-circle"
        End Select

        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'><i class='fas fa-{icon} me-2'></i>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

    ' Resets form fields after successful registration
    Private Sub ClearForm()
        txtFirstName.Text = ""
        txtLastName.Text = ""
        txtEmail.Text = ""
        txtPassword.Text = ""
        txtConfirmPassword.Text = ""
    End Sub

End Class


