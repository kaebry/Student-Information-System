
Imports System.Data
Imports Npgsql
Imports System.Configuration

Public Class ManageStudents
    Inherits System.Web.UI.Page

    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Check admin access
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        If Not IsPostBack Then
            ' Set default enrollment date
            txtEnrollmentDate.Text = DateTime.Today.ToString("yyyy-MM-dd")

            ' Load students immediately with better error handling
            LoadStudents()
        End If
    End Sub

    Private Sub LoadStudents()
        Try
            ShowMessage("🔄 Loading students...", "alert alert-info")

            Using conn As NpgsqlConnection = GetConnection()
                Dim query As String = "SELECT id, first_name, last_name, email, enrollment_date FROM students ORDER BY last_name, first_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 15

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        gvStudents.DataSource = dt
                        gvStudents.DataBind()

                        If dt.Rows.Count = 0 Then
                            ShowMessage("📝 No students found. Add your first student using the form above.", "alert alert-info")
                        Else
                            ShowMessage($"✅ {dt.Rows.Count} student(s) loaded successfully.", "alert alert-success")
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading students: {ex.Message}", "alert alert-danger")

            ' Show empty grid on error
            Try
                Dim emptyDt As New DataTable()
                emptyDt.Columns.Add("id", GetType(Long))
                emptyDt.Columns.Add("first_name", GetType(String))
                emptyDt.Columns.Add("last_name", GetType(String))
                emptyDt.Columns.Add("email", GetType(String))
                emptyDt.Columns.Add("enrollment_date", GetType(DateTime))

                gvStudents.DataSource = emptyDt
                gvStudents.DataBind()
            Catch
                ' If even this fails, just continue
            End Try
        End Try
    End Sub

    Protected Sub btnCreate_Click(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return

        Try
            ' Validate enrollment date
            Dim enrollmentDate As Date
            If Not Date.TryParse(txtEnrollmentDate.Text, enrollmentDate) Then
                ShowMessage("❌ Invalid date format.", "alert alert-danger")
                Return
            End If

            ' Check if email already exists
            If EmailExists(txtEmail.Text.Trim()) Then
                ShowMessage("❌ A student with this email already exists.", "alert alert-danger")
                Return
            End If

            ' Add student
            Using conn As NpgsqlConnection = GetConnection()
                Dim query As String = "INSERT INTO students (first_name, last_name, email, enrollment_date) VALUES (@fn, @ln, @em, @ed)"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 15
                    cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim())
                    cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim())
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim().ToLower())
                    cmd.Parameters.AddWithValue("@ed", enrollmentDate)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        ShowMessage("✅ Student added successfully!", "alert alert-success")
                        ClearForm()
                        LoadStudents()
                    Else
                        ShowMessage("❌ Failed to add student.", "alert alert-danger")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error adding student: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return

        If ViewState("SelectedStudentId") Is Nothing Then
            ShowMessage("❌ No student selected for update.", "alert alert-danger")
            Return
        End If

        Try
            Dim enrollmentDate As Date
            If Not Date.TryParse(txtEnrollmentDate.Text, enrollmentDate) Then
                ShowMessage("❌ Invalid date format.", "alert alert-danger")
                Return
            End If

            Dim studentId As Long = Convert.ToInt64(ViewState("SelectedStudentId"))

            Using conn As NpgsqlConnection = GetConnection()
                Dim query As String = "UPDATE students SET first_name = @fn, last_name = @ln, email = @em, enrollment_date = @ed WHERE id = @id"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 15
                    cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim())
                    cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim())
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim().ToLower())
                    cmd.Parameters.AddWithValue("@ed", enrollmentDate)
                    cmd.Parameters.AddWithValue("@id", studentId)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        ShowMessage("✅ Student updated successfully!", "alert alert-info")
                        ClearForm()
                        LoadStudents()
                    Else
                        ShowMessage("❌ Student not found or no changes made.", "alert alert-warning")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error updating student: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        If ViewState("SelectedStudentId") Is Nothing Then
            ShowMessage("❌ No student selected for deletion.", "alert alert-danger")
            Return
        End If

        Try
            Dim studentId As Long = Convert.ToInt64(ViewState("SelectedStudentId"))

            ' Check if student has enrollments
            Dim enrollmentCount As Integer = 0
            Using conn As NpgsqlConnection = GetConnection()
                Using checkCmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments WHERE student_id = @id", conn)
                    checkCmd.CommandTimeout = 10
                    checkCmd.Parameters.AddWithValue("@id", studentId)
                    enrollmentCount = Convert.ToInt32(checkCmd.ExecuteScalar())
                End Using

                If enrollmentCount > 0 Then
                    ShowMessage($"❌ Cannot delete student. They have {enrollmentCount} enrollment(s). Remove enrollments first.", "alert alert-danger")
                    Return
                End If

                ' Delete the student
                Using cmd As New NpgsqlCommand("DELETE FROM students WHERE id = @id", conn)
                    cmd.CommandTimeout = 15
                    cmd.Parameters.AddWithValue("@id", studentId)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        ShowMessage("🗑️ Student deleted successfully.", "alert alert-warning")
                        ClearForm()
                        LoadStudents()
                    Else
                        ShowMessage("❌ Student not found.", "alert alert-danger")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error deleting student: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        ClearForm()
        ShowMessage("✅ Form cleared.", "alert alert-success")
    End Sub

    Protected Sub gvStudents_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            Dim row As GridViewRow = gvStudents.SelectedRow
            If row IsNot Nothing Then
                txtFirstName.Text = HttpUtility.HtmlDecode(row.Cells(1).Text.Trim())
                txtLastName.Text = HttpUtility.HtmlDecode(row.Cells(2).Text.Trim())
                txtEmail.Text = HttpUtility.HtmlDecode(row.Cells(3).Text.Trim())

                ' Parse and format the date
                Dim enrollmentDate As DateTime
                If DateTime.TryParse(row.Cells(4).Text, enrollmentDate) Then
                    txtEnrollmentDate.Text = enrollmentDate.ToString("yyyy-MM-dd")
                End If

                ViewState("SelectedStudentId") = gvStudents.DataKeys(gvStudents.SelectedIndex).Value
                btnUpdate.Enabled = True
                btnDelete.Enabled = True

                ShowMessage($"📝 Student selected: {txtFirstName.Text} {txtLastName.Text}", "alert alert-info")
            End If
        Catch ex As Exception
            ShowMessage($"❌ Selection error: {ex.Message}", "alert alert-danger")
            ClearForm()
        End Try
    End Sub

    ' Helper Methods
    Private Function GetConnection() As NpgsqlConnection
        Try
            ' Clear pools only when absolutely needed
            If DateTime.Now.Minute Mod 5 = 0 Then ' Every 5 minutes
                NpgsqlConnection.ClearAllPools()
                System.Threading.Thread.Sleep(100)
            End If
        Catch
            ' Ignore pool clearing errors
        End Try

        Dim conn As New NpgsqlConnection(connStr)
        Try
            conn.Open()

            ' Quick connection test
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn
        Catch ex As Exception
            Try
                conn?.Dispose()
            Catch
            End Try
            Throw New Exception($"Failed to connect to database: {ex.Message}")
        End Try
    End Function

    Private Function EmailExists(email As String) As Boolean
        Try
            Using conn As NpgsqlConnection = GetConnection()
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM students WHERE LOWER(email) = LOWER(@email)", conn)
                    cmd.CommandTimeout = 10
                    cmd.Parameters.AddWithValue("@email", email)
                    Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Sub ClearForm()
        txtFirstName.Text = ""
        txtLastName.Text = ""
        txtEmail.Text = ""
        txtEnrollmentDate.Text = DateTime.Today.ToString("yyyy-MM-dd")
        ViewState("SelectedStudentId") = Nothing
        btnUpdate.Enabled = False
        btnDelete.Enabled = False
        gvStudents.SelectedIndex = -1
    End Sub

    Private Sub ShowMessage(message As String, cssClass As String)
        lblMessage.Text = message
        lblMessage.CssClass = cssClass
        lblMessage.Visible = True
    End Sub

End Class
