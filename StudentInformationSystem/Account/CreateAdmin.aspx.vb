Imports System.Configuration
Imports System.Data
Imports Npgsql
Imports BCrypt.Net

Partial Public Class CreateAdmin
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' Show warning about deleting this page
            ShowMessage("⚠️ <strong>Security Warning:</strong> Delete this page after creating your admin user!", "warning")
        End If
    End Sub

    Protected Sub btnCreateAdmin_Click(sender As Object, e As EventArgs)
        Try
            ' Get form values
            Dim email As String = txtEmail.Text.Trim().ToLower()
            Dim password As String = txtPassword.Text.Trim()
            Dim confirmPassword As String = txtConfirmPassword.Text.Trim()

            ' Validation
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

            If password <> confirmPassword Then
                ShowMessage("Passwords do not match.", "danger")
                Return
            End If

            ' Check if admin already exists
            If AdminExists(email) Then
                ShowMessage("An admin user with this email already exists.", "danger")
                Return
            End If

            ' Create admin user
            If CreateAdminUser(email, password) Then
                ShowMessage($"✅ Admin user created successfully!<br/>" &
                          $"<strong>Email:</strong> {email}<br/>" &
                          $"<strong>Role:</strong> admin<br/>" &
                          $"You can now <a href='~/Account/Login.aspx'>login</a> with these credentials.", "success")

                ' Clear form
                txtEmail.Text = ""
                txtPassword.Text = ""
                txtConfirmPassword.Text = ""
            Else
                ShowMessage("Failed to create admin user.", "danger")
            End If

        Catch ex As Exception
            ShowMessage($"Error creating admin user: {ex.Message}", "danger")
        End Try
    End Sub

    Protected Sub btnCheckExistingAdmins_Click(sender As Object, e As EventArgs)
        Try
            Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

            Using conn As New NpgsqlConnection(connStr)
                conn.Open()

                Using cmd As New NpgsqlCommand("SELECT email, role FROM users WHERE role = 'admin' ORDER BY id", conn)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        Dim adminList As String = "<h6>Existing Admin Users:</h6><ul>"
                        Dim adminCount As Integer = 0

                        While reader.Read()
                            adminCount += 1
                            adminList &= $"<li><strong>{reader("email")}</strong> - {reader("role")}</li>"
                        End While

                        adminList &= "</ul>"

                        If adminCount = 0 Then
                            ShowMessage("No admin users found in the system.", "info")
                        Else
                            ShowMessage($"Found {adminCount} admin user(s): {adminList}", "info")
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"Error checking admin users: {ex.Message}", "danger")
        End Try
    End Sub

    Private Function AdminExists(email As String) As Boolean
        Try
            Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

            Using conn As New NpgsqlConnection(connStr)
                conn.Open()

                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM users WHERE email = @email AND role = 'admin'", conn)
                    cmd.Parameters.AddWithValue("@email", email)
                    Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    Return count > 0
                End Using
            End Using

        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function CreateAdminUser(email As String, password As String) As Boolean
        Try
            ' Hash the password
            Dim hashedPassword As String = BCrypt.Net.BCrypt.HashPassword(password)

            Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

            Using conn As New NpgsqlConnection(connStr)
                conn.Open()

                ' Insert admin user (no student_id or teacher_id)
                Using cmd As New NpgsqlCommand("INSERT INTO users (email, password_hash, role) VALUES (@email, @password, @role)", conn)
                    cmd.Parameters.AddWithValue("@email", email)
                    cmd.Parameters.AddWithValue("@password", hashedPassword)
                    cmd.Parameters.AddWithValue("@role", "admin")

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    Return rowsAffected > 0
                End Using
            End Using

        Catch ex As Exception
            Return False
        End Try
    End Function

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

End Class