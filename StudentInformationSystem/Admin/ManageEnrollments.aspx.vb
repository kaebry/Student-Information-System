Imports System.Data
Imports Npgsql
Imports System.Configuration

Partial Public Class ManageEnrollments
    Inherits System.Web.UI.Page

    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Check admin access
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        If Not IsPostBack Then
            ' Set default date range (last 30 days to today) but don't apply it initially
            txtDateFrom.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")
            txtDateTo.Text = DateTime.Today.ToString("yyyy-MM-dd")

            LoadStatistics()
            ' Load ALL enrollments initially (no date filter applied)
            LoadEnrollments("", "", "", "")
            LoadRecentEnrollments()
            LoadPopularCourses()
        End If
    End Sub

    Private Sub LoadStatistics()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' Total enrollments
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments", conn)
                    lblTotalEnrollments.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' Active students (students with at least one enrollment)
                Using cmd As New NpgsqlCommand("SELECT COUNT(DISTINCT student_id) FROM enrollments", conn)
                    lblActiveStudents.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' Active courses (courses with at least one enrollment)
                Using cmd As New NpgsqlCommand("SELECT COUNT(DISTINCT course_id) FROM enrollments", conn)
                    lblActiveCourses.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' Average enrollments per student
                Using cmd As New NpgsqlCommand("SELECT ROUND(AVG(enrollment_count), 1) FROM (SELECT COUNT(*) as enrollment_count FROM enrollments GROUP BY student_id) as subquery", conn)
                    Dim avgResult = cmd.ExecuteScalar()
                    lblAvgEnrollments.Text = If(avgResult Is DBNull.Value, "0", avgResult.ToString())
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading statistics: {ex.Message}", "alert alert-warning")
            lblTotalEnrollments.Text = "0"
            lblActiveStudents.Text = "0"
            lblActiveCourses.Text = "0"
            lblAvgEnrollments.Text = "0"
        End Try
    End Sub

    Private Sub LoadEnrollments(Optional searchStudent As String = "", Optional searchCourse As String = "", Optional dateFrom As String = "", Optional dateTo As String = "")
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT e.enrollment_id, " &
                                     "s.first_name || ' ' || s.last_name as student_name, " &
                                     "s.email as student_email, " &
                                     "c.course_name, " &
                                     "c.ects as course_ects, " &
                                     "c.format as course_format, " &
                                     "e.enrollment_date " &
                                     "FROM enrollments e " &
                                     "INNER JOIN students s ON e.student_id = s.id " &
                                     "INNER JOIN courses c ON e.course_id = c.course_id " &
                                     "WHERE 1=1"

                ' Add search conditions
                If Not String.IsNullOrEmpty(searchStudent) Then
                    query &= " AND (LOWER(s.first_name) LIKE @searchStudent OR LOWER(s.last_name) LIKE @searchStudent OR LOWER(s.first_name || ' ' || s.last_name) LIKE @searchStudent)"
                End If

                If Not String.IsNullOrEmpty(searchCourse) Then
                    query &= " AND LOWER(c.course_name) LIKE @searchCourse"
                End If

                ' Better date handling with validation
                Dim validDateFrom As DateTime?
                Dim validDateTo As DateTime?

                If Not String.IsNullOrEmpty(dateFrom) Then
                    Dim tempDate As DateTime
                    If DateTime.TryParse(dateFrom, tempDate) Then
                        validDateFrom = tempDate
                        query &= " AND e.enrollment_date >= @dateFrom"
                    End If
                End If

                If Not String.IsNullOrEmpty(dateTo) Then
                    Dim tempDate As DateTime
                    If DateTime.TryParse(dateTo, tempDate) Then
                        validDateTo = tempDate
                        query &= " AND e.enrollment_date <= @dateTo"
                    End If
                End If

                query &= " ORDER BY e.enrollment_date DESC, s.last_name, s.first_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 15

                    ' Add parameters
                    If Not String.IsNullOrEmpty(searchStudent) Then
                        cmd.Parameters.AddWithValue("@searchStudent", "%" & searchStudent.ToLower() & "%")
                    End If

                    If Not String.IsNullOrEmpty(searchCourse) Then
                        cmd.Parameters.AddWithValue("@searchCourse", "%" & searchCourse.ToLower() & "%")
                    End If

                    If validDateFrom.HasValue Then
                        cmd.Parameters.AddWithValue("@dateFrom", validDateFrom.Value)
                    End If

                    If validDateTo.HasValue Then
                        cmd.Parameters.AddWithValue("@dateTo", validDateTo.Value)
                    End If

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        gvEnrollments.DataSource = dt
                        gvEnrollments.DataBind()
                        lblDisplayedEnrollments.Text = $"Showing: {dt.Rows.Count}"

                        If dt.Rows.Count = 0 Then
                            ' Check if any filters are applied
                            Dim hasFilters As Boolean = Not String.IsNullOrEmpty(searchStudent) OrElse
                                                       Not String.IsNullOrEmpty(searchCourse) OrElse
                                                       validDateFrom.HasValue OrElse
                                                       validDateTo.HasValue

                            If hasFilters Then
                                ShowMessage("No enrollments found matching your search criteria. Try adjusting your filters or click 'Clear' to see all enrollments.", "alert alert-info")
                            Else
                                ' No filters applied but still no results - check database
                                Using countCmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments", conn)
                                    Dim totalCount As Integer = Convert.ToInt32(countCmd.ExecuteScalar())
                                    If totalCount = 0 Then
                                        ShowMessage("No enrollments exist in the system yet. Students need to enroll in courses first.", "alert alert-warning")
                                    Else
                                        ShowMessage($"Database has {totalCount} enrollments but none are being displayed. There may be a technical issue.", "alert alert-warning")
                                    End If
                                End Using
                            End If
                        Else
                            HideMessage()
                        End If

                        ' Update pagination info
                        lblPaginationInfo.Text = $"Total {dt.Rows.Count} enrollment(s) displayed"
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading enrollments: {ex.Message}", "alert alert-danger")

            ' Show empty grid on error
            Try
                Dim emptyDt As New DataTable()
                emptyDt.Columns.Add("enrollment_id", GetType(Integer))
                emptyDt.Columns.Add("student_name", GetType(String))
                emptyDt.Columns.Add("student_email", GetType(String))
                emptyDt.Columns.Add("course_name", GetType(String))
                emptyDt.Columns.Add("course_ects", GetType(Integer))
                emptyDt.Columns.Add("course_format", GetType(String))
                emptyDt.Columns.Add("enrollment_date", GetType(DateTime))

                gvEnrollments.DataSource = emptyDt
                gvEnrollments.DataBind()
                lblDisplayedEnrollments.Text = "Showing: 0"
            Catch
            End Try
        End Try
    End Sub

    Private Sub LoadRecentEnrollments()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT s.first_name || ' ' || s.last_name as student_name, " &
                                     "c.course_name, " &
                                     "e.enrollment_date " &
                                     "FROM enrollments e " &
                                     "INNER JOIN students s ON e.student_id = s.id " &
                                     "INNER JOIN courses c ON e.course_id = c.course_id " &
                                     "WHERE e.enrollment_date >= @sevenDaysAgo " &
                                     "ORDER BY e.enrollment_date DESC " &
                                     "LIMIT 10"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@sevenDaysAgo", DateTime.Today.AddDays(-7))

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        If dt.Rows.Count > 0 Then
                            rpRecentEnrollments.DataSource = dt
                            rpRecentEnrollments.DataBind()
                            pnlNoRecentEnrollments.Visible = False
                        Else
                            pnlNoRecentEnrollments.Visible = True
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            pnlNoRecentEnrollments.Visible = True
        End Try
    End Sub

    Private Sub LoadPopularCourses()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT c.course_name, " &
                                     "c.format as course_format, " &
                                     "c.ects, " &
                                     "COUNT(e.enrollment_id) as enrollment_count " &
                                     "FROM courses c " &
                                     "LEFT JOIN enrollments e ON c.course_id = e.course_id " &
                                     "GROUP BY c.course_id, c.course_name, c.format, c.ects " &
                                     "HAVING COUNT(e.enrollment_id) > 0 " &
                                     "ORDER BY enrollment_count DESC " &
                                     "LIMIT 5"

                Using cmd As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        If dt.Rows.Count > 0 Then
                            rpPopularCourses.DataSource = dt
                            rpPopularCourses.DataBind()
                            pnlNoPopularCourses.Visible = False
                        Else
                            pnlNoPopularCourses.Visible = True
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            pnlNoPopularCourses.Visible = True
        End Try
    End Sub

    Protected Sub gvEnrollments_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' Format the course format display
            Dim formatCell As TableCell = e.Row.Cells(5) ' course_format column
            Dim originalFormat As String = formatCell.Text.ToLower()

            Select Case originalFormat
                Case "lecture"
                    formatCell.Text = "<span class='badge format-lecture'>Lecture</span>"
                Case "seminar"
                    formatCell.Text = "<span class='badge format-seminar'>Seminar</span>"
                Case "workshop"
                    formatCell.Text = "<span class='badge format-workshop'>Workshop</span>"
                Case "laboratory"
                    formatCell.Text = "<span class='badge format-laboratory'>Laboratory</span>"
                Case "online"
                    formatCell.Text = "<span class='badge format-online'>Online</span>"
                Case "hybrid"
                    formatCell.Text = "<span class='badge format-hybrid'>Hybrid</span>"
                Case "practical"
                    formatCell.Text = "<span class='badge format-practical'>Practical</span>"
                Case Else
                    formatCell.Text = $"<span class='badge bg-secondary'>{originalFormat.Substring(0, 1).ToUpper() + originalFormat.Substring(1)}</span>"
            End Select

            ' Highlight recent enrollments (last 7 days)
            Dim enrollmentDate As DateTime = DateTime.Parse(DataBinder.Eval(e.Row.DataItem, "enrollment_date").ToString())
            If enrollmentDate >= DateTime.Today.AddDays(-7) Then
                e.Row.CssClass += " table-success"
            End If
        End If
    End Sub

    Protected Sub btnDelete_Command(sender As Object, e As CommandEventArgs)
        If e.CommandName = "DeleteEnrollment" Then
            Dim enrollmentId As Integer = Convert.ToInt32(e.CommandArgument)
            DeleteEnrollment(enrollmentId)
        End If
    End Sub

    Private Sub DeleteEnrollment(enrollmentId As Integer)
        Try
            ' Get enrollment details before deleting for the confirmation message
            Dim studentName As String = ""
            Dim courseName As String = ""

            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' Get enrollment details
                Dim detailQuery As String = "SELECT s.first_name || ' ' || s.last_name as student_name, c.course_name " &
                                           "FROM enrollments e " &
                                           "INNER JOIN students s ON e.student_id = s.id " &
                                           "INNER JOIN courses c ON e.course_id = c.course_id " &
                                           "WHERE e.enrollment_id = @enrollment_id"

                Using detailCmd As New NpgsqlCommand(detailQuery, conn)
                    detailCmd.Parameters.AddWithValue("@enrollment_id", enrollmentId)
                    Using reader As NpgsqlDataReader = detailCmd.ExecuteReader()
                        If reader.Read() Then
                            studentName = reader("student_name").ToString()
                            courseName = reader("course_name").ToString()
                        End If
                    End Using
                End Using

                ' Delete the enrollment
                Dim deleteQuery As String = "DELETE FROM enrollments WHERE enrollment_id = @enrollment_id"
                Using deleteCmd As New NpgsqlCommand(deleteQuery, conn)
                    deleteCmd.Parameters.AddWithValue("@enrollment_id", enrollmentId)

                    Dim rowsAffected As Integer = deleteCmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        ShowMessage($"✅ Enrollment deleted successfully! {studentName} has been unenrolled from {courseName}.", "alert alert-success")

                        ' Refresh all data with current filters (only if they have values)
                        LoadStatistics()

                        ' Get current filter values safely - only use dates if user has searched
                        Dim studentFilter As String = If(txtSearchStudent.Text, "").Trim()
                        Dim courseFilter As String = If(txtSearchCourse.Text, "").Trim()

                        ' Only apply date filters if user has actually searched/filtered
                        Dim dateFromFilter As String = ""
                        Dim dateToFilter As String = ""

                        ' Check if any text search filters are active OR if date fields have been modified from default
                        Dim hasActiveFilters As Boolean = Not String.IsNullOrEmpty(studentFilter) OrElse Not String.IsNullOrEmpty(courseFilter)

                        If hasActiveFilters Then
                            dateFromFilter = If(txtDateFrom.Text, "").Trim()
                            dateToFilter = If(txtDateTo.Text, "").Trim()
                        End If

                        ' Load enrollments with current filters
                        LoadEnrollments(studentFilter, courseFilter, dateFromFilter, dateToFilter)
                        LoadRecentEnrollments()
                        LoadPopularCourses()
                    Else
                        ShowMessage("❌ Enrollment not found or could not be deleted.", "alert alert-warning")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error deleting enrollment: {ex.Message}", "alert alert-danger")
            ' Still try to refresh the list even if deletion failed - load all enrollments
            Try
                LoadEnrollments("", "", "", "")
            Catch
                ' If that fails, show error
                ShowMessage("❌ Could not refresh enrollment list after error", "alert alert-warning")
            End Try
        End Try
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        ' Get filter values safely
        Dim studentFilter As String = If(txtSearchStudent.Text, "").Trim()
        Dim courseFilter As String = If(txtSearchCourse.Text, "").Trim()
        Dim dateFromFilter As String = If(txtDateFrom.Text, "").Trim()
        Dim dateToFilter As String = If(txtDateTo.Text, "").Trim()

        LoadEnrollments(studentFilter, courseFilter, dateFromFilter, dateToFilter)
    End Sub

    Protected Sub btnClearSearch_Click(sender As Object, e As EventArgs)
        ' Clear all search fields
        txtSearchStudent.Text = ""
        txtSearchCourse.Text = ""
        txtDateFrom.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")
        txtDateTo.Text = DateTime.Today.ToString("yyyy-MM-dd")

        ' Load ALL enrollments (no filters applied)
        LoadEnrollments("", "", "", "")
        ShowMessage("✅ Filters cleared - showing all enrollments!", "alert alert-info")
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadStatistics()
        ' Get current filter values safely and load enrollments
        Dim studentFilter As String = If(txtSearchStudent.Text, "").Trim()
        Dim courseFilter As String = If(txtSearchCourse.Text, "").Trim()

        ' Only apply date filters if user has other active filters
        Dim dateFromFilter As String = ""
        Dim dateToFilter As String = ""

        If Not String.IsNullOrEmpty(studentFilter) OrElse Not String.IsNullOrEmpty(courseFilter) Then
            dateFromFilter = If(txtDateFrom.Text, "").Trim()
            dateToFilter = If(txtDateTo.Text, "").Trim()
        End If

        LoadEnrollments(studentFilter, courseFilter, dateFromFilter, dateToFilter)
        LoadRecentEnrollments()
        LoadPopularCourses()
        ShowMessage("✅ Data refreshed successfully!", "alert alert-success")
    End Sub

    ' Add a simple refresh method without filters as backup
    Private Sub RefreshEnrollmentsWithoutFilters()
        Try
            LoadEnrollments("", "", "", "")
        Catch ex As Exception
            ShowMessage($"❌ Error refreshing enrollments: {ex.Message}", "alert alert-warning")
        End Try
    End Sub

    ' Add a debug method to check total enrollments
    Protected Sub CheckTotalEnrollments()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments", conn)
                    Dim totalCount As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                    ShowMessage($"🔍 DEBUG: Total enrollments in database: {totalCount}", "alert alert-info")
                End Using
            End Using
        Catch ex As Exception
            ShowMessage($"❌ Error checking total enrollments: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    ' Helper methods
    Private Function GetWorkingConnection() As NpgsqlConnection
        ' Clear connection pools for better reliability
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200)
        Catch
            ' Ignore cleanup errors
        End Try

        Dim conn As New NpgsqlConnection(connStr)

        Try
            conn.Open()

            ' Test the connection
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn

        Catch ex As Exception
            ' If connection fails, try to clean up and throw a more helpful error
            Try
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If
                conn.Dispose()
            Catch
                ' Ignore cleanup errors
            End Try

            Throw New Exception($"Failed to connect to database: {ex.Message}")
        End Try
    End Function

    Private Sub ShowMessage(message As String, cssClass As String)
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

End Class