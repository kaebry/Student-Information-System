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

            Dim retryCount As Integer = 0
            Dim maxRetries As Integer = 2
            Dim studentsLoaded As Boolean = False

            While retryCount <= maxRetries And Not studentsLoaded
                Try
                    Using conn As NpgsqlConnection = GetConnection()
                        Dim query As String = "SELECT id, first_name, last_name, email, enrollment_date FROM students ORDER BY last_name, first_name"

                        Using cmd As New NpgsqlCommand(query, conn)
                            cmd.CommandTimeout = 20

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

                                studentsLoaded = True
                            End Using
                        End Using
                    End Using

                Catch connEx As Exception When (connEx.Message.Contains("stream") OrElse
                                               connEx.Message.Contains("connection") OrElse
                                               connEx.Message.Contains("timeout")) AndAlso retryCount < maxRetries
                    retryCount += 1
                    ShowMessage($"🔄 Connection issue (attempt {retryCount + 1}). Retrying...", "alert alert-warning")
                    System.Threading.Thread.Sleep(1000 * retryCount) ' Progressive delay
                    Continue While
                End Try
            End While

            If Not studentsLoaded Then
                Throw New Exception("Failed to load students after multiple attempts")
            End If

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
        ' Only validate for Add operations
        Page.Validate("StudentForm")
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

            Dim addSuccessful As Boolean = False

            ' Try the add operation with robust error handling
            Try
                Using conn As NpgsqlConnection = GetConnection()
                    Dim query As String = "INSERT INTO students (first_name, last_name, email, enrollment_date) VALUES (@fn, @ln, @em, @ed)"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 30
                        cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim())
                        cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim())
                        cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim().ToLower())
                        cmd.Parameters.AddWithValue("@ed", enrollmentDate)

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        addSuccessful = (rowsAffected > 0)
                    End Using
                End Using

            Catch connEx As Exception When connEx.Message.Contains("stream") OrElse
                                         connEx.Message.Contains("connection") OrElse
                                         connEx.Message.Contains("timeout")
                ' Connection issue - check if add actually succeeded
                ShowMessage("🔄 Connection issue detected. Verifying addition...", "alert alert-warning")
                System.Threading.Thread.Sleep(1000)

                ' Check if the student was actually added
                addSuccessful = EmailExists(txtEmail.Text.Trim())
            End Try

            If addSuccessful Then
                ShowMessage("✅ Student added successfully!", "alert alert-success")
                ClearForm()
                LoadStudents()
            Else
                ShowMessage("❌ Failed to add student.", "alert alert-danger")
            End If

        Catch ex As Exception
            ShowMessage($"❌ Error adding student: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs)
        ' Only validate for Update operations
        Page.Validate("StudentForm")
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
            Dim updateSuccessful As Boolean = False

            ' Try the update with robust error handling
            Try
                Using conn As NpgsqlConnection = GetConnection()
                    Dim query As String = "UPDATE students SET first_name = @fn, last_name = @ln, email = @em, enrollment_date = @ed WHERE id = @id"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 30 ' Increased timeout
                        cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim())
                        cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim())
                        cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim().ToLower())
                        cmd.Parameters.AddWithValue("@ed", enrollmentDate)
                        cmd.Parameters.AddWithValue("@id", studentId)

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        updateSuccessful = (rowsAffected > 0)
                    End Using
                End Using

            Catch connEx As Exception When connEx.Message.Contains("stream") OrElse
                                         connEx.Message.Contains("connection") OrElse
                                         connEx.Message.Contains("timeout")
                ' Connection issue - check if update actually succeeded
                ShowMessage("🔄 Connection issue detected. Verifying update...", "alert alert-warning")
                System.Threading.Thread.Sleep(1000) ' Wait a moment

                ' Check if the update actually happened
                updateSuccessful = VerifyStudentUpdate(studentId, txtFirstName.Text.Trim(), txtLastName.Text.Trim(), txtEmail.Text.Trim().ToLower(), enrollmentDate)
            End Try

            ' Handle the result
            If updateSuccessful Then
                ShowMessage("✅ Student updated successfully!", "alert alert-success")
                ClearForm()
                LoadStudents()
            Else
                ShowMessage("❌ Student not found or no changes made.", "alert alert-warning")
            End If

        Catch ex As Exception
            ShowMessage($"❌ Error updating student: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        ' NO VALIDATION for delete operations - this is the key fix!
        If ViewState("SelectedStudentId") Is Nothing Then
            ShowMessage("❌ No student selected for deletion.", "alert alert-danger")
            Return
        End If

        Try
            Dim studentId As Long = Convert.ToInt64(ViewState("SelectedStudentId"))
            Dim deleteSuccessful As Boolean = False

            ' Check if student has enrollments first
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

                ' Try the delete operation
                Try
                    Using cmd As New NpgsqlCommand("DELETE FROM students WHERE id = @id", conn)
                        cmd.CommandTimeout = 30
                        cmd.Parameters.AddWithValue("@id", studentId)

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        deleteSuccessful = (rowsAffected > 0)
                    End Using

                Catch connEx As Exception When connEx.Message.Contains("stream") OrElse
                                             connEx.Message.Contains("connection") OrElse
                                             connEx.Message.Contains("timeout")
                    ' Connection issue - check if delete actually succeeded
                    ShowMessage("🔄 Connection issue detected. Verifying deletion...", "alert alert-warning")
                    System.Threading.Thread.Sleep(1000)

                    ' Check if the student was actually deleted
                    deleteSuccessful = Not StudentExists(studentId)
                End Try
            End Using

            If deleteSuccessful Then
                ShowMessage("🗑️ Student deleted successfully.", "alert alert-warning")
                ClearForm()
                LoadStudents()
            Else
                ShowMessage("❌ Student not found or could not be deleted.", "alert alert-danger")
            End If

        Catch ex As Exception
            ShowMessage($"❌ Error deleting student: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        ' NO VALIDATION for clear operations
        ClearForm()
        ShowMessage("✅ Form cleared.", "alert alert-success")
    End Sub

    Protected Sub gvStudents_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            Dim row As GridViewRow = gvStudents.SelectedRow
            If row IsNot Nothing Then
                ' Clear any existing validation errors when selecting a row
                Page.Validators.Cast(Of BaseValidator)().ToList().ForEach(Sub(v) v.IsValid = True)

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
        ' Force clean connections for better reliability
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200)
        Catch
            ' Ignore pool clearing errors
        End Try

        ' Use the original connection string - don't modify it since your Web.config already has the right settings
        Dim conn As New NpgsqlConnection(connStr)
        Try
            conn.Open()

            ' Quick connection test with shorter timeout
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

    Private Function VerifyStudentUpdate(studentId As Long, firstName As String, lastName As String, email As String, enrollmentDate As Date) As Boolean
        Try
            ' Use a fresh connection to verify the update
            System.Threading.Thread.Sleep(500) ' Give database a moment

            Using conn As NpgsqlConnection = GetConnection()
                Using cmd As New NpgsqlCommand("SELECT first_name, last_name, email, enrollment_date FROM students WHERE id = @id", conn)
                    cmd.CommandTimeout = 10
                    cmd.Parameters.AddWithValue("@id", studentId)

                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim dbFirstName As String = reader("first_name").ToString()
                            Dim dbLastName As String = reader("last_name").ToString()
                            Dim dbEmail As String = reader("email").ToString()
                            Dim dbEnrollmentDate As Date = Convert.ToDateTime(reader("enrollment_date"))

                            ' Check if the data matches what we tried to update
                            Return dbFirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) AndAlso
                                   dbLastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) AndAlso
                                   dbEmail.Equals(email, StringComparison.OrdinalIgnoreCase) AndAlso
                                   dbEnrollmentDate.Date = enrollmentDate.Date
                        End If
                    End Using
                End Using
            End Using
        Catch
            ' If verification fails, assume update didn't work
            Return False
        End Try

        Return False
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

    Private Function StudentExists(studentId As Long) As Boolean
        Try
            Using conn As NpgsqlConnection = GetConnection()
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM students WHERE id = @id", conn)
                    cmd.CommandTimeout = 10
                    cmd.Parameters.AddWithValue("@id", studentId)
                    Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                End Using
            End Using
        Catch
            Return True ' If we can't check, assume it still exists
        End Try
    End Function

    Private Sub ClearForm()
        Try
            txtFirstName.Text = ""
            txtLastName.Text = ""
            txtEmail.Text = ""
            txtEnrollmentDate.Text = DateTime.Today.ToString("yyyy-MM-dd")
            ViewState("SelectedStudentId") = Nothing
            btnUpdate.Enabled = False
            btnDelete.Enabled = False
            gvStudents.SelectedIndex = -1

            ' Clear any validation errors safely
            Try
                If Page.Validators IsNot Nothing Then
                    For Each validator As BaseValidator In Page.Validators
                        validator.IsValid = True
                    Next
                End If
            Catch
                ' Ignore validation clearing errors
            End Try
        Catch ex As Exception
            ' If clearing fails, just log it but don't break the flow
            ' In a real app, you might want to log this
        End Try
    End Sub

    Private Sub ShowMessage(message As String, cssClass As String)
        lblMessage.Text = message
        lblMessage.CssClass = cssClass
        lblMessage.Visible = True
    End Sub

End Class
