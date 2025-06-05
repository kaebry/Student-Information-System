Imports System.Data
Imports Npgsql
Imports System.Configuration

Public Class ManageCourses
    Inherits System.Web.UI.Page

    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Check admin access
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
        End If

        If Not IsPostBack Then
            ' Auto-clear pools to prevent stale connections (Supabase fix)
            Try
                NpgsqlConnection.ClearAllPools()
                System.Threading.Thread.Sleep(300) ' Shorter wait
            Catch
                ' Ignore pool clearing errors
            End Try

            LoadCourses()
            UpdateTotalCoursesLabel()
        End If
    End Sub

    Private Function GetRobustConnection() As NpgsqlConnection
        Dim enhancedConnStr As String = connStr

        ' Ensure clean connection parameters
        If Not enhancedConnStr.Contains("Connection Lifetime") Then
            enhancedConnStr &= ";Connection Lifetime=30;Maximum Pool Size=10;Minimum Pool Size=1;Connection Idle Lifetime=30;"
        End If

        Dim conn As New NpgsqlConnection(enhancedConnStr)

        Try
            conn.Open()

            ' Always test connection before returning it
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn

        Catch
            ' If connection fails, clean up and try with fresh pool
            Try
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If
                conn.Dispose()
            Catch
            End Try

            ' Clear pools and create fresh connection
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(500)

            conn = New NpgsqlConnection(enhancedConnStr)
            conn.Open()

            ' Test the fresh connection
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn
        End Try
    End Function

    Private Sub LoadCourses(Optional searchName As String = "", Optional filterFormat As String = "", Optional searchInstructor As String = "")
        Dim dt As New DataTable()
        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 3

        While retryCount < maxRetries
            Try
                Using conn As NpgsqlConnection = GetRobustConnection()
                    conn.Open()

                    ' Test connection
                    Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                        testCmd.CommandTimeout = 10
                        testCmd.ExecuteScalar()
                    End Using

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
                        cmd.CommandTimeout = 30

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
                            adapter.Fill(dt)
                        End Using
                    End Using
                End Using

                ' If we reach here, the operation was successful
                gvCourses.DataSource = dt
                gvCourses.DataBind()
                UpdateTotalCoursesLabel(dt.Rows.Count)
                Return ' Exit the retry loop

            Catch ex As Exception
                retryCount += 1

                If retryCount >= maxRetries Then
                    ShowMessage($"❌ Failed to load courses after {maxRetries} attempts: {ex.Message}", "alert alert-danger")

                    ' Create empty DataTable to prevent further errors
                    dt = New DataTable()
                    dt.Columns.Add("course_id", GetType(Integer))
                    dt.Columns.Add("course_name", GetType(String))
                    dt.Columns.Add("ects", GetType(Integer))
                    dt.Columns.Add("hours", GetType(Integer))
                    dt.Columns.Add("format", GetType(String))
                    dt.Columns.Add("instructor", GetType(String))

                    gvCourses.DataSource = dt
                    gvCourses.DataBind()
                    UpdateTotalCoursesLabel(0)
                Else
                    ' Wait before retry
                    System.Threading.Thread.Sleep(1000 * retryCount)
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(500)
                End If
            End Try
        End While
    End Sub

    Protected Sub btnAdd_Click(sender As Object, e As EventArgs)
        If Not Page.IsValid Then
            Return
        End If

        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 3

        While retryCount < maxRetries
            Try
                Using conn As NpgsqlConnection = GetRobustConnection()
                    conn.Open()

                    Using cmd As New NpgsqlCommand("INSERT INTO courses (course_name, ects, hours, format, instructor) VALUES (@name, @ects, @hours, @format, @instructor)", conn)
                        cmd.CommandTimeout = 30
                        cmd.Parameters.AddWithValue("@name", txtCourseName.Text.Trim())
                        cmd.Parameters.AddWithValue("@ects", Convert.ToInt32(txtECTS.Text))
                        cmd.Parameters.AddWithValue("@hours", Convert.ToInt32(txtHours.Text))
                        cmd.Parameters.AddWithValue("@format", ddlFormat.SelectedValue)
                        cmd.Parameters.AddWithValue("@instructor", txtInstructor.Text.Trim())

                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                ShowMessage("✅ Course added successfully!", "alert alert-success")
                ClearForm()
                LoadCourses()
                UpdateTotalCoursesLabel()
                Return ' Exit retry loop

            Catch ex As Exception
                retryCount += 1
                If retryCount >= maxRetries Then
                    ShowMessage($"❌ Failed to add course after {maxRetries} attempts: {ex.Message}", "alert alert-danger")
                Else
                    System.Threading.Thread.Sleep(1000 * retryCount)
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(500)
                End If
            End Try
        End While
    End Sub

    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs)
        If Not Page.IsValid Then
            Return
        End If

        If ViewState("SelectedCourseId") Is Nothing Then
            ShowMessage("❌ No course selected for update.", "alert alert-danger")
            Return
        End If

        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 3

        While retryCount < maxRetries
            Try
                Dim courseId As Integer = CInt(ViewState("SelectedCourseId"))

                Using conn As NpgsqlConnection = GetRobustConnection()
                    conn.Open()

                    Using cmd As New NpgsqlCommand("UPDATE courses SET course_name = @name, ects = @ects, hours = @hours, format = @format, instructor = @instructor WHERE course_id = @id", conn)
                        cmd.CommandTimeout = 30
                        cmd.Parameters.AddWithValue("@name", txtCourseName.Text.Trim())
                        cmd.Parameters.AddWithValue("@ects", Convert.ToInt32(txtECTS.Text))
                        cmd.Parameters.AddWithValue("@hours", Convert.ToInt32(txtHours.Text))
                        cmd.Parameters.AddWithValue("@format", ddlFormat.SelectedValue)
                        cmd.Parameters.AddWithValue("@instructor", txtInstructor.Text.Trim())
                        cmd.Parameters.AddWithValue("@id", courseId)

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                        If rowsAffected > 0 Then
                            ShowMessage("✅ Course updated successfully!", "alert alert-info")
                            ClearForm()
                            LoadCourses()
                        Else
                            ShowMessage("❌ Course not found or no changes made.", "alert alert-warning")
                        End If
                    End Using
                End Using
                Return ' Exit retry loop

            Catch ex As Exception
                retryCount += 1
                If retryCount >= maxRetries Then
                    ShowMessage($"❌ Failed to update course after {maxRetries} attempts: {ex.Message}", "alert alert-danger")
                Else
                    System.Threading.Thread.Sleep(1000 * retryCount)
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(500)
                End If
            End Try
        End While
    End Sub

    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        If ViewState("SelectedCourseId") Is Nothing Then
            ShowMessage("❌ No course selected for deletion.", "alert alert-danger")
            Return
        End If

        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 3

        While retryCount < maxRetries
            Try
                Dim courseId As Integer = CInt(ViewState("SelectedCourseId"))

                ' Check if course has enrollments
                Dim enrollmentCount As Integer = GetEnrolledStudentsCount(courseId)
                If enrollmentCount > 0 Then
                    ShowMessage($"❌ Cannot delete course. It has {enrollmentCount} enrolled student(s). Please remove enrollments first.", "alert alert-danger")
                    Return
                End If

                Using conn As NpgsqlConnection = GetRobustConnection()
                    conn.Open()

                    Using cmd As New NpgsqlCommand("DELETE FROM courses WHERE course_id = @id", conn)
                        cmd.CommandTimeout = 30
                        cmd.Parameters.AddWithValue("@id", courseId)

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                        If rowsAffected > 0 Then
                            ShowMessage("🗑️ Course deleted successfully.", "alert alert-warning")
                            ClearForm()
                            LoadCourses()
                            UpdateTotalCoursesLabel()
                        Else
                            ShowMessage("❌ Course not found.", "alert alert-danger")
                        End If
                    End Using
                End Using
                Return ' Exit retry loop

            Catch ex As Exception
                retryCount += 1
                If retryCount >= maxRetries Then
                    ShowMessage($"❌ Failed to delete course after {maxRetries} attempts: {ex.Message}", "alert alert-danger")
                Else
                    System.Threading.Thread.Sleep(1000 * retryCount)
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(500)
                End If
            End Try
        End While
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        ClearForm()
        ShowMessage("", "")
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

    ' Debug methods (remove after fixing connection issues)
    Protected Sub btnTestConnection_Click(sender As Object, e As EventArgs)
        Try
            ShowMessage("🔄 Testing database connection...", "alert alert-info")

            Using conn As NpgsqlConnection = GetRobustConnection()
                conn.Open()

                Using cmd As New NpgsqlCommand("SELECT version(), current_database(), current_user", conn)
                    cmd.CommandTimeout = 10
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim version As String = reader(0).ToString()
                            Dim database As String = reader(1).ToString()
                            Dim user As String = reader(2).ToString()

                            ShowMessage($"✅ Connection successful!<br/>" &
                                      $"<strong>Database:</strong> {database}<br/>" &
                                      $"<strong>User:</strong> {user}<br/>" &
                                      $"<strong>Version:</strong> {version.Substring(0, Math.Min(50, version.Length))}...", "alert alert-success")
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Connection test failed: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnClearPools_Click(sender As Object, e As EventArgs)
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(1000)
            ShowMessage("✅ Connection pools cleared successfully!", "alert alert-success")
        Catch ex As Exception
            ShowMessage($"❌ Error clearing pools: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnForceReload_Click(sender As Object, e As EventArgs)
        Try
            ShowMessage("🔄 Force reloading courses...", "alert alert-info")

            ' Clear pools first
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(500)

            ' Force reload
            LoadCourses()

            ShowMessage("✅ Courses reloaded successfully!", "alert alert-success")
        Catch ex As Exception
            ShowMessage($"❌ Force reload failed: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub gvCourses_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            Dim row As GridViewRow = gvCourses.SelectedRow
            If row IsNot Nothing Then
                txtCourseName.Text = HttpUtility.HtmlDecode(row.Cells(1).Text)
                txtECTS.Text = row.Cells(2).Text
                txtHours.Text = row.Cells(3).Text

                ' Extract format from HTML badge
                Dim formatCell As String = row.Cells(4).Text
                Dim format As String = ExtractFormatFromBadge(formatCell)
                ddlFormat.SelectedValue = format

                txtInstructor.Text = HttpUtility.HtmlDecode(row.Cells(5).Text)

                ViewState("SelectedCourseId") = gvCourses.DataKeys(gvCourses.SelectedIndex).Value
                btnUpdate.Enabled = True
                btnDelete.Enabled = True
                lblFormMode.Text = "Edit Course"
                lblFormMode.CssClass = "badge badge-warning ml-2"

                ShowMessage($"📝 Course selected for editing: {txtCourseName.Text}", "alert alert-info")
            End If
        Catch ex As Exception
            ShowMessage("❌ Error selecting course: " & ex.Message, "alert alert-danger")
        End Try
    End Sub

    Private Function ExtractFormatFromBadge(badgeHtml As String) As String
        ' Extract format from badge HTML like "<span class='badge badge-primary'>Lecture</span>"
        Try
            If badgeHtml.Contains(">") Then
                Dim startPos As Integer = badgeHtml.LastIndexOf(">") + 1
                Dim endPos As Integer = badgeHtml.LastIndexOf("<")
                If startPos > 0 AndAlso endPos > startPos Then
                    Return badgeHtml.Substring(startPos, endPos - startPos).ToLower()
                End If
            End If
            Return badgeHtml.ToLower()
        Catch
            Return badgeHtml.ToLower()
        End Try
    End Function

    Protected Sub gvCourses_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' Format the course format display
            Dim formatCell As TableCell = e.Row.Cells(4)
            Dim originalFormat As String = formatCell.Text.ToLower()

            Select Case originalFormat
                Case "lecture"
                    formatCell.Text = "<span class='badge badge-primary'>Lecture</span>"
                Case "seminar"
                    formatCell.Text = "<span class='badge badge-success'>Seminar</span>"
                Case "workshop"
                    formatCell.Text = "<span class='badge badge-info'>Workshop</span>"
                Case "laboratory"
                    formatCell.Text = "<span class='badge badge-warning'>Laboratory</span>"
                Case "online"
                    formatCell.Text = "<span class='badge badge-secondary'>Online</span>"
                Case "hybrid"
                    formatCell.Text = "<span class='badge badge-dark'>Hybrid</span>"
                Case "practical"
                    formatCell.Text = "<span class='badge badge-light text-dark'>Practical</span>"
                Case Else
                    formatCell.Text = $"<span class='badge badge-secondary'>{originalFormat.Substring(0, 1).ToUpper() + originalFormat.Substring(1)}</span>"
            End Select
        End If
    End Sub

    Protected Function GetEnrolledStudentsCount(courseId As Object) As Integer
        If courseId Is Nothing Then Return 0

        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 2

        While retryCount < maxRetries
            Try
                Using conn As NpgsqlConnection = GetRobustConnection()
                    conn.Open()

                    Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments WHERE course_id = @courseId", conn)
                        cmd.CommandTimeout = 15
                        cmd.Parameters.AddWithValue("@courseId", Convert.ToInt32(courseId))
                        Return Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch
                retryCount += 1
                If retryCount >= maxRetries Then
                    Return 0 ' Return 0 if we can't get the count
                Else
                    System.Threading.Thread.Sleep(500)
                    NpgsqlConnection.ClearAllPools()
                End If
            End Try
        End While

        Return 0
    End Function

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

        ' Clear GridView selection
        gvCourses.SelectedIndex = -1
    End Sub

    Private Sub UpdateTotalCoursesLabel(Optional count As Integer = -1)
        If count = -1 Then
            Dim retryCount As Integer = 0
            Dim maxRetries As Integer = 2

            While retryCount < maxRetries
                Try
                    Using conn As NpgsqlConnection = GetRobustConnection()
                        conn.Open()

                        Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM courses", conn)
                            cmd.CommandTimeout = 15
                            count = Convert.ToInt32(cmd.ExecuteScalar())
                        End Using
                    End Using
                    Exit While ' Success, exit retry loop
                Catch
                    retryCount += 1
                    If retryCount >= maxRetries Then
                        count = 0
                    Else
                        System.Threading.Thread.Sleep(500)
                        NpgsqlConnection.ClearAllPools()
                    End If
                End Try
            End While
        End If

        lblTotalCourses.Text = $"Total: {count}"
    End Sub

    Private Sub ShowMessage(message As String, cssClass As String)
        If String.IsNullOrEmpty(message) Then
            MessagePanel.Visible = False
            MessageLiteral.Text = ""
        Else
            MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
            MessagePanel.Visible = True
        End If
    End Sub

End Class