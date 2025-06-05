Imports System.Data
Imports Npgsql
Imports System.Configuration

Public Class ManageCourses
    Inherits System.Web.UI.Page

    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString
    Private Shared lastPoolClear As DateTime = DateTime.MinValue

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Check admin access
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        If Not IsPostBack Then
            ' Only clear pools if it's been more than 30 seconds since last clear
            If DateTime.Now.Subtract(lastPoolClear).TotalSeconds > 30 Then
                NpgsqlConnection.ClearAllPools()
                lastPoolClear = DateTime.Now
                System.Threading.Thread.Sleep(100)
            End If

            ' Try simple loading first
            Try
                LoadCoursesSimple()
            Catch ex As Exception
                ShowMessage($"⚠️ Page load issue: {ex.Message}. Try clicking '🔧 Load Simple' or '🔄 Refresh List'", "alert alert-warning")
            End Try
        End If
    End Sub

    ' Simplified and faster LoadCourses method with retry logic
    Private Sub LoadCourses(Optional searchName As String = "", Optional filterFormat As String = "", Optional searchInstructor As String = "")
        Dim maxRetries As Integer = 3
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                If retryCount > 0 Then
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(1000 * retryCount) ' Longer wait for retries
                End If

                Using conn As New NpgsqlConnection(connStr)
                    conn.Open()

                    ' Use simple query without enrollment counts
                    Dim query As String = "SELECT course_id, course_name, ects, hours, format, instructor FROM courses WHERE 1=1"

                    ' Add search conditions
                    If Not String.IsNullOrEmpty(searchName) Then
                        query &= " AND LOWER(course_name) LIKE @searchName"
                    End If

                    If Not String.IsNullOrEmpty(filterFormat) Then
                        query &= " AND format = @filterFormat"
                    End If

                    If Not String.IsNullOrEmpty(searchInstructor) Then
                        query &= " AND LOWER(instructor) LIKE @searchInstructor"
                    End If

                    query &= " ORDER BY course_name"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 10

                        ' Add parameters
                        If Not String.IsNullOrEmpty(searchName) Then
                            cmd.Parameters.AddWithValue("@searchName", "%" & searchName.ToLower() & "%")
                        End If

                        If Not String.IsNullOrEmpty(filterFormat) Then
                            cmd.Parameters.AddWithValue("@filterFormat", filterFormat)
                        End If

                        If Not String.IsNullOrEmpty(searchInstructor) Then
                            cmd.Parameters.AddWithValue("@searchInstructor", "%" & searchInstructor.ToLower() & "%")
                        End If

                        Using adapter As New NpgsqlDataAdapter(cmd)
                            Dim dt As New DataTable()
                            adapter.Fill(dt)

                            gvCourses.DataSource = dt
                            gvCourses.DataBind()
                            lblTotalCourses.Text = $"Total: {dt.Rows.Count}"

                            If dt.Rows.Count = 0 Then
                                ShowMessage("No courses found matching your criteria.", "alert alert-info")
                            Else
                                HideMessage()
                            End If
                        End Using
                    End Using
                End Using

                ' If we reach here, success! Exit retry loop
                Return

            Catch ex As NpgsqlException When ex.Message.Contains("Exception while reading from stream") Or ex.Message.Contains("stream")
                retryCount += 1
                If retryCount >= maxRetries Then
                    ShowMessage($"❌ Connection failed after {maxRetries} attempts. Database may be temporarily unavailable.", "alert alert-danger")
                Else
                    ShowMessage($"🔄 Connection issue, retrying... (attempt {retryCount + 1})", "alert alert-warning")
                End If

            Catch ex As Exception
                ShowMessage($"❌ Failed to load courses: {ex.Message}", "alert alert-danger")
                Exit While ' Don't retry for non-stream errors
            End Try
        End While

        ' If all retries failed, show empty grid
        Try
            Dim emptyDt As New DataTable()
            emptyDt.Columns.Add("course_id", GetType(Integer))
            emptyDt.Columns.Add("course_name", GetType(String))
            emptyDt.Columns.Add("ects", GetType(Integer))
            emptyDt.Columns.Add("hours", GetType(Integer))
            emptyDt.Columns.Add("format", GetType(String))
            emptyDt.Columns.Add("instructor", GetType(String))

            gvCourses.DataSource = emptyDt
            gvCourses.DataBind()
            lblTotalCourses.Text = "Total: 0"
        Catch
            ' If even this fails, just clear the grid
            gvCourses.DataSource = Nothing
            gvCourses.DataBind()
        End Try
    End Sub



    ' Simple connection helper method
    Private Function GetWorkingConnection() As NpgsqlConnection
        ' Clear pools first
        NpgsqlConnection.ClearAllPools()
        System.Threading.Thread.Sleep(500)

        Dim conn As New NpgsqlConnection(connStr)
        conn.Open()
        Return conn
    End Function

    ' Simple and fast LoadCourses method
    Private Sub LoadCoursesSimple(Optional searchName As String = "", Optional filterFormat As String = "", Optional searchInstructor As String = "")
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' Use simple query without enrollment counts
                Dim query As String = "SELECT course_id, course_name, ects, hours, format, instructor FROM courses ORDER BY course_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5 ' Very short timeout

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        gvCourses.DataSource = dt
                        gvCourses.DataBind()
                        lblTotalCourses.Text = $"Total: {dt.Rows.Count}"

                        HideMessage()
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading courses: {ex.Message}", "alert alert-danger")

            ' Show empty grid
            Try
                Dim emptyDt As New DataTable()
                emptyDt.Columns.Add("course_id", GetType(Integer))
                emptyDt.Columns.Add("course_name", GetType(String))
                emptyDt.Columns.Add("ects", GetType(Integer))
                emptyDt.Columns.Add("hours", GetType(Integer))
                emptyDt.Columns.Add("format", GetType(String))
                emptyDt.Columns.Add("instructor", GetType(String))

                gvCourses.DataSource = emptyDt
                gvCourses.DataBind()
                lblTotalCourses.Text = "Total: 0"
            Catch
            End Try
        End Try
    End Sub
    Private Function ExecuteWithRetry(query As String, parameters As Dictionary(Of String, Object)) As Boolean
        Dim maxRetries As Integer = 3
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                If retryCount > 0 Then
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(500)
                End If

                Using conn As New NpgsqlConnection(connStr)
                    conn.Open()

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 10

                        ' Add parameters
                        For Each param In parameters
                            cmd.Parameters.AddWithValue(param.Key, param.Value)
                        Next

                        Return cmd.ExecuteNonQuery() > 0
                    End Using
                End Using

            Catch ex As NpgsqlException When ex.Message.Contains("Exception while reading from stream") Or ex.Message.Contains("stream")
                retryCount += 1
                If retryCount >= maxRetries Then
                    Throw New Exception($"Database operation failed after {maxRetries} attempts due to connection issues")
                End If
            End Try
        End While

        Return False
    End Function

    Protected Sub btnAdd_Click(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return

        Try
            ' Check if course with same name already exists
            Dim courseName As String = txtCourseName.Text.Trim()

            Try
                Using conn As New NpgsqlConnection(connStr)
                    conn.Open()
                    Using checkCmd As New NpgsqlCommand("SELECT COUNT(*) FROM courses WHERE LOWER(course_name) = LOWER(@name)", conn)
                        checkCmd.CommandTimeout = 5
                        checkCmd.Parameters.AddWithValue("@name", courseName)
                        Dim existingCount As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                        If existingCount > 0 Then
                            ShowMessage("❌ A course with this name already exists. Please choose a different name.", "alert alert-danger")
                            Return
                        End If
                    End Using
                End Using
            Catch
                ' If duplicate check fails, proceed but warn
                ShowMessage("⚠️ Could not verify duplicates. Adding course...", "alert alert-warning")
            End Try

            Dim parameters As New Dictionary(Of String, Object) From {
                {"@name", courseName},
                {"@ects", Convert.ToInt32(txtECTS.Text)},
                {"@hours", Convert.ToInt32(txtHours.Text)},
                {"@format", ddlFormat.SelectedValue},
                {"@instructor", txtInstructor.Text.Trim()}
            }

            Dim query As String = "INSERT INTO courses (course_name, ects, hours, format, instructor) VALUES (@name, @ects, @hours, @format, @instructor)"

            If ExecuteWithRetry(query, parameters) Then
                ShowMessage("✅ Course added successfully!", "alert alert-success")
                ClearForm()

                ' Try to refresh, but don't fail if it doesn't work
                Try
                    LoadCoursesSimple()
                Catch loadEx As Exception
                    ShowMessage("✅ Course added successfully! Please click '🔧 Load Simple' to see the new course.", "alert alert-warning")
                End Try
            Else
                ShowMessage("❌ Failed to add course.", "alert alert-danger")
            End If

        Catch ex As Exception
            ShowMessage($"❌ Error adding course: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        If ViewState("SelectedCourseId") Is Nothing Then
            ShowMessage("❌ No course selected for update.", "alert alert-danger")
            Return
        End If

        Try
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@name", txtCourseName.Text.Trim()},
                {"@ects", Convert.ToInt32(txtECTS.Text)},
                {"@hours", Convert.ToInt32(txtHours.Text)},
                {"@format", ddlFormat.SelectedValue},
                {"@instructor", txtInstructor.Text.Trim()},
                {"@id", CInt(ViewState("SelectedCourseId"))}
            }

            Dim query As String = "UPDATE courses SET course_name = @name, ects = @ects, hours = @hours, format = @format, instructor = @instructor WHERE course_id = @id"

            If ExecuteWithRetry(query, parameters) Then
                ShowMessage("✅ Course updated successfully!", "alert alert-info")
                ClearForm()

                Try
                    LoadCourses()
                Catch loadEx As Exception
                    ShowMessage("✅ Course updated successfully! Please click '🔄 Refresh List' to see changes.", "alert alert-warning")
                End Try
            Else
                ShowMessage("❌ Course not found or no changes made.", "alert alert-warning")
            End If

        Catch ex As Exception
            ShowMessage($"❌ Error updating course: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        If ViewState("SelectedCourseId") Is Nothing Then
            ShowMessage("❌ No course selected for deletion.", "alert alert-danger")
            Return
        End If

        Try
            Dim courseId As Integer = CInt(ViewState("SelectedCourseId"))

            ' Simplified delete - just check enrollments with simple query, then delete
            Dim enrollmentCount As Integer = 0
            Try
                ' Simple enrollment check
                Using conn As New NpgsqlConnection(connStr)
                    conn.Open()
                    Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments WHERE course_id = @id", conn)
                        cmd.CommandTimeout = 5
                        cmd.Parameters.AddWithValue("@id", courseId)
                        enrollmentCount = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using

                If enrollmentCount > 0 Then
                    ShowMessage($"❌ Cannot delete course. It has {enrollmentCount} enrolled student(s).", "alert alert-danger")
                    Return
                End If
            Catch
                ' If enrollment check fails, warn but allow deletion
                ShowMessage("⚠️ Could not verify enrollments. Proceeding with deletion...", "alert alert-warning")
            End Try

            ' Delete the course using reliable method
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@id", courseId}
            }

            Dim deleteQuery As String = "DELETE FROM courses WHERE course_id = @id"

            If ExecuteWithRetry(deleteQuery, parameters) Then
                ShowMessage("🗑️ Course deleted successfully.", "alert alert-warning")
                ClearForm()

                Try
                    LoadCoursesSimple()
                Catch loadEx As Exception
                    ShowMessage("🗑️ Course deleted successfully! Please click '🔧 Load Simple' to refresh the list.", "alert alert-warning")
                End Try
            Else
                ShowMessage("❌ Course not found or could not be deleted.", "alert alert-danger")
            End If

        Catch ex As Exception
            ShowMessage($"❌ Error deleting course: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        ClearForm()
        HideMessage()
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        LoadCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, txtSearchInstructor.Text.Trim())
    End Sub

    Protected Sub btnClearSearch_Click(sender As Object, e As EventArgs)
        txtSearchName.Text = ""
        ddlFilterFormat.SelectedIndex = 0
        txtSearchInstructor.Text = ""
        LoadCourses()
    End Sub

    ' Debug/Tool methods
    Protected Sub btnTestConnection_Click(sender As Object, e As EventArgs)
        Try
            Using conn As New NpgsqlConnection(connStr)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT current_database(), current_user", conn)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ShowMessage($"✅ Connection successful! Database: {reader(0)}, User: {reader(1)}", "alert alert-success")
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ShowMessage($"❌ Connection failed: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnClearPools_Click(sender As Object, e As EventArgs)
        NpgsqlConnection.ClearAllPools()
        lastPoolClear = DateTime.Now
        ShowMessage("✅ Connection pools cleared!", "alert alert-success")
    End Sub

    Protected Sub btnForceReload_Click(sender As Object, e As EventArgs)
        LoadCourses()
        ShowMessage("✅ Courses reloaded!", "alert alert-success")
    End Sub

    Protected Sub btnLoadWithHelper_Click(sender As Object, e As EventArgs)
        LoadCoursesSimple()
        ShowMessage("✅ Courses loaded with simple method!", "alert alert-success")
    End Sub

    ' GridView event handlers
    Protected Sub gvCourses_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            Dim row As GridViewRow = gvCourses.SelectedRow
            If row IsNot Nothing Then
                txtCourseName.Text = HttpUtility.HtmlDecode(row.Cells(1).Text.Trim())
                txtECTS.Text = row.Cells(2).Text.Trim()
                txtHours.Text = row.Cells(3).Text.Trim()

                ' Get format value directly from the data
                Dim formatValue As String = ExtractFormatFromBadge(row.Cells(4).Text)
                ddlFormat.SelectedValue = formatValue

                txtInstructor.Text = HttpUtility.HtmlDecode(row.Cells(5).Text.Trim())

                ViewState("SelectedCourseId") = gvCourses.DataKeys(gvCourses.SelectedIndex).Value
                btnUpdate.Enabled = True
                btnDelete.Enabled = True
                lblFormMode.Text = "Edit Course"
                lblFormMode.CssClass = "badge badge-warning ml-2 text-dark"

                ShowMessage($"📝 Course selected: {txtCourseName.Text}", "alert alert-info")
            End If
        Catch ex As Exception
            ShowMessage($"❌ Selection error: {ex.Message}", "alert alert-danger")
            ClearForm()
        End Try
    End Sub

    Private Function ExtractFormatFromBadge(badgeHtml As String) As String
        Try
            ' Simple regex to extract text between > and <
            Dim cleanText As String = System.Text.RegularExpressions.Regex.Replace(badgeHtml, "<.*?>", "").Trim()
            Return cleanText.ToLower()
        Catch
            Return "lecture" ' Safe default
        End Try
    End Function

    Protected Sub gvCourses_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' Format the course format display
            Dim formatCell As TableCell = e.Row.Cells(4)
            Dim originalFormat As String = formatCell.Text.ToLower()

            Select Case originalFormat
                Case "lecture"
                    formatCell.Text = "<span class='badge badge-primary text-dark'>Lecture</span>"
                Case "seminar"
                    formatCell.Text = "<span class='badge badge-success text-dark'>Seminar</span>"
                Case "workshop"
                    formatCell.Text = "<span class='badge badge-info text-dark'>Workshop</span>"
                Case "laboratory"
                    formatCell.Text = "<span class='badge badge-warning text-dark'>Laboratory</span>"
                Case "online"
                    formatCell.Text = "<span class='badge badge-secondary text-dark'>Online</span>"
                Case "hybrid"
                    formatCell.Text = "<span class='badge badge-dark text-dark'>Hybrid</span>"
                Case "practical"
                    formatCell.Text = "<span class='badge badge-light text-dark'>Practical</span>"
                Case Else
                    formatCell.Text = $"<span class='badge badge-secondary text-dark'>{originalFormat.Substring(0, 1).ToUpper() + originalFormat.Substring(1)}</span>"
            End Select
        End If
    End Sub

    ' Helper methods
    Private Sub ClearForm()
        txtCourseName.Text = ""
        txtECTS.Text = ""
        txtHours.Text = ""
        ddlFormat.SelectedIndex = 0
        txtInstructor.Text = ""
        ViewState("SelectedCourseId") = Nothing
        btnUpdate.Enabled = False
        btnDelete.Enabled = False
        lblFormMode.Text = "Add New Course"
        lblFormMode.CssClass = "badge badge-secondary ml-2"
        gvCourses.SelectedIndex = -1
    End Sub

    Private Sub ShowMessage(message As String, cssClass As String)
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

End Class