Imports System.Data
Imports Npgsql
Imports System.Configuration
Imports System.Text
Imports System.Web.Script.Serialization

Partial Public Class Reports
    Inherits System.Web.UI.Page

    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Check admin access
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        If Not IsPostBack Then
            LoadReportsData()
        End If
    End Sub

    Private Sub LoadReportsData()
        Try
            LoadStatistics()
            LoadStudentsPerCourseChart()
            LoadEnrollmentTrendsChart()
            LoadCourseFormatDistribution()
            LoadRecentActivity()
        Catch ex As Exception
            ShowMessage($"❌ Error loading reports data: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    Private Sub LoadStatistics()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' Total students
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM students", conn)
                    lblTotalStudents.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' Total courses
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM courses", conn)
                    lblTotalCourses.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' Total enrollments
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments", conn)
                    lblTotalEnrollments.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' Average enrollments per student
                Using cmd As New NpgsqlCommand("SELECT ROUND(AVG(enrollment_count), 1) FROM (SELECT COUNT(*) as enrollment_count FROM enrollments GROUP BY student_id) as subquery", conn)
                    Dim avgResult = cmd.ExecuteScalar()
                    lblAvgEnrollments.Text = If(avgResult Is DBNull.Value, "0", avgResult.ToString())
                End Using

                ' Most popular course
                Using cmd As New NpgsqlCommand("SELECT c.course_name FROM courses c INNER JOIN enrollments e ON c.course_id = e.course_id GROUP BY c.course_id, c.course_name ORDER BY COUNT(*) DESC LIMIT 1", conn)
                    Dim result = cmd.ExecuteScalar()
                    lblMostPopularCourse.Text = If(result Is Nothing, "No enrollments yet", result.ToString())
                End Using

                ' Enrollment rate (students with at least one enrollment)
                Using cmd As New NpgsqlCommand("SELECT ROUND((COUNT(DISTINCT e.student_id)::decimal / NULLIF(COUNT(DISTINCT s.id), 0)) * 100, 1) FROM students s LEFT JOIN enrollments e ON s.id = e.student_id", conn)
                    Dim result = cmd.ExecuteScalar()
                    lblEnrollmentRate.Text = If(result Is DBNull.Value, "0", result.ToString()) & "%"
                End Using

            End Using
        Catch ex As Exception
            ' Set default values on error
            lblTotalStudents.Text = "0"
            lblTotalCourses.Text = "0"
            lblTotalEnrollments.Text = "0"
            lblAvgEnrollments.Text = "0"
            lblMostPopularCourse.Text = "Error loading"
            lblEnrollmentRate.Text = "0%"
            ShowMessage($"❌ Error loading statistics: {ex.Message}", "alert alert-warning")
        End Try
    End Sub

    Private Sub LoadStudentsPerCourseChart()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT c.course_name, COALESCE(COUNT(e.enrollment_id), 0) as student_count " &
                                     "FROM courses c " &
                                     "LEFT JOIN enrollments e ON c.course_id = e.course_id " &
                                     "GROUP BY c.course_id, c.course_name " &
                                     "ORDER BY student_count DESC, c.course_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        If dt.Rows.Count > 0 Then
                            ' Prepare data for Chart.js
                            Dim courseNames As New List(Of String)
                            Dim studentCounts As New List(Of Integer)

                            For Each row As DataRow In dt.Rows
                                courseNames.Add(row("course_name").ToString())
                                studentCounts.Add(Convert.ToInt32(row("student_count")))
                            Next

                            ' Convert to JSON for JavaScript
                            Dim serializer As New JavaScriptSerializer()
                            Dim courseNamesJson As String = serializer.Serialize(courseNames)
                            Dim studentCountsJson As String = serializer.Serialize(studentCounts)

                            ' Store in hidden fields for JavaScript access
                            hdnCourseNames.Value = courseNamesJson
                            hdnStudentCounts.Value = studentCountsJson

                            ' Show chart container
                            chartContainer.Visible = True
                            noDataMessage.Visible = False
                        Else
                            ' No data available
                            chartContainer.Visible = False
                            noDataMessage.Visible = True
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            chartContainer.Visible = False
            noDataMessage.Visible = True
            ShowMessage($"❌ Error loading chart data: {ex.Message}", "alert alert-warning")
        End Try
    End Sub

    Private Sub LoadEnrollmentTrendsChart()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' Get enrollments by month for the last 6 months
                Dim query As String = "SELECT DATE_TRUNC('month', enrollment_date) as month, COUNT(*) as enrollments " &
                                     "FROM enrollments " &
                                     "WHERE enrollment_date >= CURRENT_DATE - INTERVAL '6 months' " &
                                     "GROUP BY DATE_TRUNC('month', enrollment_date) " &
                                     "ORDER BY month"

                Using cmd As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        If dt.Rows.Count > 0 Then
                            Dim months As New List(Of String)
                            Dim enrollments As New List(Of Integer)

                            For Each row As DataRow In dt.Rows
                                Dim monthDate As DateTime = Convert.ToDateTime(row("month"))
                                months.Add(monthDate.ToString("MMM yyyy"))
                                enrollments.Add(Convert.ToInt32(row("enrollments")))
                            Next

                            ' Convert to JSON
                            Dim serializer As New JavaScriptSerializer()
                            hdnTrendMonths.Value = serializer.Serialize(months)
                            hdnTrendEnrollments.Value = serializer.Serialize(enrollments)

                            trendsChartContainer.Visible = True
                        Else
                            trendsChartContainer.Visible = False
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            trendsChartContainer.Visible = False
        End Try
    End Sub

    Private Sub LoadCourseFormatDistribution()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT c.format, COUNT(e.enrollment_id) as enrollment_count " &
                                     "FROM courses c " &
                                     "LEFT JOIN enrollments e ON c.course_id = e.course_id " &
                                     "GROUP BY c.format " &
                                     "HAVING COUNT(e.enrollment_id) > 0 " &
                                     "ORDER BY enrollment_count DESC"

                Using cmd As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        If dt.Rows.Count > 0 Then
                            Dim formats As New List(Of String)
                            Dim counts As New List(Of Integer)

                            For Each row As DataRow In dt.Rows
                                formats.Add(row("format").ToString().ToUpper())
                                counts.Add(Convert.ToInt32(row("enrollment_count")))
                            Next

                            ' Convert to JSON
                            Dim serializer As New JavaScriptSerializer()
                            hdnFormatLabels.Value = serializer.Serialize(formats)
                            hdnFormatCounts.Value = serializer.Serialize(counts)

                            formatChartContainer.Visible = True
                        Else
                            formatChartContainer.Visible = False
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            formatChartContainer.Visible = False
        End Try
    End Sub

    Private Sub LoadRecentActivity()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT s.first_name || ' ' || s.last_name as student_name, " &
                                     "c.course_name, " &
                                     "e.enrollment_date " &
                                     "FROM enrollments e " &
                                     "INNER JOIN students s ON e.student_id = s.id " &
                                     "INNER JOIN courses c ON e.course_id = c.course_id " &
                                     "ORDER BY e.enrollment_date DESC " &
                                     "LIMIT 10"

                Using cmd As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        rpRecentActivity.DataSource = dt
                        rpRecentActivity.DataBind()

                        pnlNoActivity.Visible = (dt.Rows.Count = 0)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            pnlNoActivity.Visible = True
        End Try
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadReportsData()
        ShowMessage("✅ Reports data refreshed successfully!", "alert alert-success")
    End Sub

    Protected Sub btnExportData_Click(sender As Object, e As EventArgs)
        Try
            ' Simple CSV export of enrollment data
            Dim csv As New StringBuilder()
            csv.AppendLine("Course Name,Student Count,ECTS,Format")

            Using conn As NpgsqlConnection = GetWorkingConnection()
                Dim query As String = "SELECT c.course_name, COALESCE(COUNT(e.enrollment_id), 0) as student_count, c.ects, c.format " &
                                     "FROM courses c " &
                                     "LEFT JOIN enrollments e ON c.course_id = e.course_id " &
                                     "GROUP BY c.course_id, c.course_name, c.ects, c.format " &
                                     "ORDER BY student_count DESC"

                Using cmd As New NpgsqlCommand(query, conn)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            csv.AppendLine($"{reader("course_name")},{reader("student_count")},{reader("ects")},{reader("format")}")
                        End While
                    End Using
                End Using
            End Using

            ' Send as download
            Response.Clear()
            Response.ContentType = "text/csv"
            Response.AddHeader("Content-Disposition", $"attachment; filename=enrollment_report_{DateTime.Now:yyyyMMdd}.csv")
            Response.Write(csv.ToString())
            Response.End()

        Catch ex As Exception
            ShowMessage($"❌ Error exporting data: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    ' Helper methods
    Private Function GetWorkingConnection() As NpgsqlConnection
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200)
        Catch
        End Try

        Dim conn As New NpgsqlConnection(connStr)

        Try
            conn.Open()
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using
            Return conn
        Catch ex As Exception
            Try
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If
                conn.Dispose()
            Catch
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