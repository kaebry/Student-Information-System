Imports System.Data
Imports Npgsql
Imports System.Configuration

Partial Public Class AvailableCourses
    Inherits System.Web.UI.Page

    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString
    Private currentStudentId As Long = 0

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Check if user is logged in as a student
        If Session("UserRole")?.ToString() <> "student" Then
            Response.Redirect("~/Account/Login.aspx?error=access_denied")
            Return
        End If

        ' Get current student ID from session
        If Session("ProfileId") IsNot Nothing Then
            Try
                currentStudentId = Convert.ToInt64(Session("ProfileId"))
                ShowMessage($"🔍 DEBUG - Student ID retrieved: {currentStudentId}", "alert alert-info")
            Catch ex As Exception
                ShowMessage($"❌ Error converting ProfileId to Long: {ex.Message}. ProfileId value: {Session("ProfileId")}", "alert alert-danger")
                Return
            End Try
        Else
            ShowMessage("❌ Student profile not found in session. Please log out and log back in.", "alert alert-danger")
            Return
        End If

        If Not IsPostBack Then
            LoadStudentInfo()
            LoadAvailableCourses()
            LoadMyEnrollments()
        End If
    End Sub

    Private Sub LoadStudentInfo()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' Get student information
                Dim query As String = "SELECT first_name, last_name, email FROM students WHERE id = @student_id"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            lblStudentName.Text = $"{reader("first_name")} {reader("last_name")}"
                            lblStudentEmail.Text = reader("email").ToString()
                            ShowMessage($"✅ Student info loaded successfully", "alert alert-success")
                        Else
                            ShowMessage($"❌ No student found with ID: {currentStudentId}", "alert alert-danger")
                            Return
                        End If
                    End Using
                End Using

                ' Get enrollment counts and total ECTS
                Dim enrollmentQuery As String = "SELECT COUNT(*), COALESCE(SUM(c.ects), 0) FROM enrollments e " &
                                               "INNER JOIN courses c ON e.course_id = c.course_id " &
                                               "WHERE e.student_id = @student_id"
                Using cmd As New NpgsqlCommand(enrollmentQuery, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            lblCurrentEnrollments.Text = reader(0).ToString()
                            lblTotalECTS.Text = reader(1).ToString()
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading student information: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Private Sub LoadAvailableCourses(Optional searchName As String = "", Optional filterFormat As String = "", Optional filterECTS As String = "")
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' First check total courses in database
                Dim countQuery As String = "SELECT COUNT(*) FROM courses"
                Using countCmd As New NpgsqlCommand(countQuery, conn)
                    Dim totalCourses As Integer = Convert.ToInt32(countCmd.ExecuteScalar())
                    If totalCourses = 0 Then
                        ShowMessage("⚠️ No courses found in database. Please add some courses first.", "alert alert-warning")
                        Return
                    End If
                End Using

                Dim query As String = "SELECT course_id, course_name, ects, hours, format, instructor FROM courses WHERE 1=1"

                ' Add search and filter conditions
                If Not String.IsNullOrEmpty(searchName) Then
                    query &= " AND LOWER(course_name) LIKE @searchName"
                End If

                If Not String.IsNullOrEmpty(filterFormat) Then
                    query &= " AND format = @filterFormat"
                End If

                If Not String.IsNullOrEmpty(filterECTS) Then
                    Select Case filterECTS
                        Case "1-3"
                            query &= " AND ects BETWEEN 1 AND 3"
                        Case "4-6"
                            query &= " AND ects BETWEEN 4 AND 6"
                        Case "7-9"
                            query &= " AND ects BETWEEN 7 AND 9"
                        Case "10+"
                            query &= " AND ects >= 10"
                    End Select
                End If

                query &= " ORDER BY course_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 15

                    ' Add parameters
                    If Not String.IsNullOrEmpty(searchName) Then
                        cmd.Parameters.AddWithValue("@searchName", "%" & searchName.ToLower() & "%")
                    End If

                    If Not String.IsNullOrEmpty(filterFormat) Then
                        cmd.Parameters.AddWithValue("@filterFormat", filterFormat)
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
                            ShowMessage($"✅ {dt.Rows.Count} courses loaded successfully", "alert alert-success")
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading courses: {ex.Message}", "alert alert-danger")

            ' Show empty grid on error
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

    Private Sub LoadMyEnrollments()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT c.course_name, c.ects, c.hours, c.format, c.instructor, e.enrollment_date " &
                                     "FROM enrollments e " &
                                     "INNER JOIN courses c ON e.course_id = c.course_id " &
                                     "WHERE e.student_id = @student_id " &
                                     "ORDER BY e.enrollment_date DESC"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        If dt.Rows.Count > 0 Then
                            rpMyEnrollments.DataSource = dt
                            rpMyEnrollments.DataBind()
                            pnlNoEnrollments.Visible = False
                        Else
                            pnlNoEnrollments.Visible = True
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading enrollments: {ex.Message}", "alert alert-warning")
            pnlNoEnrollments.Visible = True
        End Try
    End Sub

    Private Function IsStudentEnrolledInCourse(courseId As Integer) As Boolean
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT COUNT(*) FROM enrollments WHERE student_id = @student_id AND course_id = @course_id"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    cmd.Parameters.AddWithValue("@course_id", courseId)
                    Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                End Using
            End Using
        Catch
            Return False
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

            ' Handle enrollment status and buttons
            Dim courseId As Integer = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "course_id"))
            Dim isEnrolled As Boolean = IsStudentEnrolledInCourse(courseId)

            ' Find the controls in the row
            Dim lblEnrollmentStatus As Label = CType(e.Row.FindControl("lblEnrollmentStatus"), Label)
            Dim btnEnroll As Button = CType(e.Row.FindControl("btnEnroll"), Button)
            Dim btnUnenroll As Button = CType(e.Row.FindControl("btnUnenroll"), Button)

            If lblEnrollmentStatus IsNot Nothing Then
                If isEnrolled Then
                    lblEnrollmentStatus.Text = "<span class='badge status-enrolled'>Enrolled</span>"
                Else
                    lblEnrollmentStatus.Text = "<span class='badge status-not-enrolled'>Not Enrolled</span>"
                End If
            End If

            If btnEnroll IsNot Nothing And btnUnenroll IsNot Nothing Then
                If isEnrolled Then
                    btnEnroll.Visible = False
                    btnUnenroll.Visible = True
                Else
                    btnEnroll.Visible = True
                    btnUnenroll.Visible = False
                End If
            End If
        End If
    End Sub

    Protected Sub btnEnroll_Command(sender As Object, e As CommandEventArgs)
        If e.CommandName = "Enroll" Then
            Dim courseId As Integer = Convert.ToInt32(e.CommandArgument)
            EnrollInCourse(courseId)
        End If
    End Sub

    Protected Sub btnUnenroll_Command(sender As Object, e As CommandEventArgs)
        If e.CommandName = "Unenroll" Then
            Dim courseId As Integer = Convert.ToInt32(e.CommandArgument)
            UnenrollFromCourse(courseId)
        End If
    End Sub

    Private Sub EnrollInCourse(courseId As Integer)
        Try
            ' First check if already enrolled
            If IsStudentEnrolledInCourse(courseId) Then
                ShowMessage("❌ You are already enrolled in this course.", "alert alert-warning")
                Return
            End If

            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "INSERT INTO enrollments (student_id, course_id, enrollment_date) VALUES (@student_id, @course_id, @enrollment_date)"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    cmd.Parameters.AddWithValue("@course_id", courseId)
                    cmd.Parameters.AddWithValue("@enrollment_date", DateTime.Today)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        ShowMessage("✅ Successfully enrolled in the course!", "alert alert-success")

                        ' Refresh the page data
                        LoadStudentInfo()
                        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
                        LoadMyEnrollments()
                    Else
                        ShowMessage("❌ Failed to enroll in the course.", "alert alert-danger")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error enrolling in course: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Private Sub UnenrollFromCourse(courseId As Integer)
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "DELETE FROM enrollments WHERE student_id = @student_id AND course_id = @course_id"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    cmd.Parameters.AddWithValue("@course_id", courseId)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        ShowMessage("✅ Successfully unenrolled from the course.", "alert alert-info")

                        ' Refresh the page data
                        LoadStudentInfo()
                        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
                        LoadMyEnrollments()
                    Else
                        ShowMessage("❌ Failed to unenroll from the course or you were not enrolled.", "alert alert-warning")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error unenrolling from course: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
    End Sub

    Protected Sub btnClearSearch_Click(sender As Object, e As EventArgs)
        txtSearchName.Text = ""
        ddlFilterFormat.SelectedIndex = 0
        ddlFilterECTS.SelectedIndex = 0
        LoadAvailableCourses()
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadStudentInfo()
        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
        LoadMyEnrollments()
        ShowMessage("✅ Page refreshed successfully!", "alert alert-success")
    End Sub

    ' Helper methods - FIXED CONNECTION METHOD
    Private Function GetWorkingConnection() As NpgsqlConnection
        ' Clear connection pools for better reliability
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200)
        Catch
            ' Ignore cleanup errors
        End Try

        ' Use the connection string as-is from Web.config
        ' Don't modify it since your existing Web.config already has the correct parameters
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
        ' Show only the latest message to avoid clutter
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

End Class