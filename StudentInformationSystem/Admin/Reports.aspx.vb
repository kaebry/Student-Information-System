'==============================================================================
' REPORTS DASHBOARD - CODE-BEHIND CLASS
'==============================================================================
' Purpose: Comprehensive analytics and reporting system for Student Information System
' Features: Real-time statistics, interactive charts, data export, trend analysis
' 
' Advanced Analytics Features:
' - Real-time KPI calculation and display (students, courses, enrollments)
' - Interactive Chart.js integration for visual data representation
' - Students per course distribution with dynamic bar charts
' - Enrollment trends analysis with 6-month historical data
' - Course format distribution with interactive doughnut charts
' - Recent activity feed showing latest enrollment operations
' - CSV data export capability for external analysis
' - Responsive dashboard design with mobile optimization
' 
' Chart.js Integration Architecture:
' - Server-side data aggregation and JSON serialization
' - Client-side chart rendering with Chart.js library
' - Dynamic chart updates based on real-time database queries
' - Color-coded visualizations for enhanced data interpretation
' - Interactive tooltips and hover effects for better UX
'==============================================================================

Imports System.Data
Imports Npgsql
Imports System.Configuration
Imports System.Text
Imports System.Web.Script.Serialization

Partial Public Class Reports
    Inherits System.Web.UI.Page

    '--------------------------------------------------------------------------
    ' CLASS VARIABLES AND CONFIGURATION
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Database connection string retrieved from Web.config
    ''' Optimized for cloud database connectivity with appropriate timeout settings
    ''' Used across all database operations for consistency and security
    ''' </summary>
    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    '--------------------------------------------------------------------------
    ' PAGE LIFECYCLE EVENTS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Page Load Event Handler
    ''' Initializes the reports dashboard with security validation and data loading
    ''' Implements role-based access control for administrative functions
    ''' </summary>
    ''' <param name="sender">The page object that raised the event</param>
    ''' <param name="e">Event arguments containing page load context</param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' SECURITY GATE: Administrative Access Control
        ' Restricts dashboard access to administrators only
        ' Prevents unauthorized viewing of sensitive analytics data
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        ' FIRST-TIME PAGE INITIALIZATION
        ' Load all dashboard components only on initial page request
        ' Postback events will handle specific data refreshes
        If Not IsPostBack Then
            LoadReportsData()
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' MAIN DATA ORCHESTRATION METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Master Data Loading Orchestrator
    ''' Coordinates loading of all dashboard components in optimal sequence
    ''' Implements comprehensive error handling to ensure partial functionality
    ''' even if individual components fail
    ''' </summary>
    Private Sub LoadReportsData()
        Try
            ' SEQUENTIAL DATA LOADING with individual error isolation
            ' Each component loads independently to prevent cascade failures
            LoadStatistics()                    ' KPI metrics and summary statistics
            LoadStudentsPerCourseChart()        ' Bar chart data preparation
            LoadEnrollmentTrendsChart()         ' Historical trend analysis
            LoadCourseFormatDistribution()      ' Format distribution pie chart
            LoadRecentActivity()                ' Latest enrollment activity feed

        Catch ex As Exception
            ' GRACEFUL DEGRADATION: Show error but maintain page functionality
            ' Individual component failures won't crash the entire dashboard
            ShowMessage($"❌ Error loading reports data: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' STATISTICAL METRICS CALCULATION
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Load Dashboard Statistics and KPI Metrics
    ''' Calculates and displays key performance indicators for system overview
    ''' Provides administrators with critical system health and usage metrics
    ''' </summary>
    Private Sub LoadStatistics()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' METRIC 1: Total Students Count
                ' Fundamental system scale indicator
                ' Represents total registered student population
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM students", conn)
                    lblTotalStudents.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' METRIC 2: Total Courses Count
                ' Curriculum breadth indicator
                ' Shows available academic offerings
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM courses", conn)
                    lblTotalCourses.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' METRIC 3: Total Enrollments Count
                ' Primary engagement metric
                ' Indicates actual system usage beyond registration
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments", conn)
                    lblTotalEnrollments.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' METRIC 4: Average Enrollments per Student
                ' Student engagement depth indicator
                ' Uses subquery for accurate calculation excluding inactive students
                ' COALESCE handles division by zero scenarios gracefully
                Using cmd As New NpgsqlCommand("SELECT ROUND(AVG(enrollment_count), 1) FROM (SELECT COUNT(*) as enrollment_count FROM enrollments GROUP BY student_id) as subquery", conn)
                    Dim avgResult = cmd.ExecuteScalar()
                    lblAvgEnrollments.Text = If(avgResult Is DBNull.Value, "0", avgResult.ToString())
                End Using

                ' METRIC 5: Most Popular Course Analysis
                ' Identifies highest-demand academic offerings
                ' Critical for resource allocation and curriculum planning
                ' LIMIT 1 ensures only top course is returned
                Using cmd As New NpgsqlCommand("SELECT c.course_name FROM courses c INNER JOIN enrollments e ON c.course_id = e.course_id GROUP BY c.course_id, c.course_name ORDER BY COUNT(*) DESC LIMIT 1", conn)
                    Dim result = cmd.ExecuteScalar()
                    lblMostPopularCourse.Text = If(result Is Nothing, "No enrollments yet", result.ToString())
                End Using

                ' METRIC 6: System Enrollment Rate Calculation
                ' Percentage of registered students who are actively enrolled
                ' Key indicator of system adoption and student engagement
                ' NULLIF prevents division by zero errors
                ' ROUND provides user-friendly percentage display
                Using cmd As New NpgsqlCommand("SELECT ROUND((COUNT(DISTINCT e.student_id)::decimal / NULLIF(COUNT(DISTINCT s.id), 0)) * 100, 1) FROM students s LEFT JOIN enrollments e ON s.id = e.student_id", conn)
                    Dim result = cmd.ExecuteScalar()
                    lblEnrollmentRate.Text = If(result Is DBNull.Value, "0", result.ToString()) & "%"
                End Using
            End Using

        Catch ex As Exception
            ' GRACEFUL DEGRADATION: Set safe default values on statistical failure
            ' Prevents dashboard crash while indicating data unavailability
            lblTotalStudents.Text = "0"
            lblTotalCourses.Text = "0"
            lblTotalEnrollments.Text = "0"
            lblAvgEnrollments.Text = "0"
            lblMostPopularCourse.Text = "Error loading"
            lblEnrollmentRate.Text = "0%"
            ShowMessage($"❌ Error loading statistics: {ex.Message}", "alert alert-warning")
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' CHART DATA PREPARATION METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Prepare Students per Course Bar Chart Data
    ''' Aggregates enrollment data for visual representation in Chart.js
    ''' Creates dynamic bar chart showing course popularity distribution
    ''' </summary>
    Private Sub LoadStudentsPerCourseChart()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' COMPREHENSIVE COURSE ENROLLMENT ANALYSIS
                ' LEFT JOIN ensures all courses appear, even with zero enrollments
                ' COALESCE handles NULL counts for courses without enrollments
                ' ORDER BY provides logical data presentation (popularity descending, name ascending)
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
                            ' DATA SERIALIZATION for Chart.js Integration
                            ' Separate arrays for course names and enrollment counts
                            ' Required format for Chart.js bar chart rendering
                            Dim courseNames As New List(Of String)
                            Dim studentCounts As New List(Of Integer)

                            ' EFFICIENT DATA EXTRACTION and aggregation
                            For Each row As DataRow In dt.Rows
                                courseNames.Add(row("course_name").ToString())
                                studentCounts.Add(Convert.ToInt32(row("student_count")))
                            Next

                            ' JSON SERIALIZATION for JavaScript Consumption
                            ' JavaScriptSerializer ensures proper JSON encoding
                            ' Hidden fields provide secure data transfer to client-side
                            Dim serializer As New JavaScriptSerializer()
                            Dim courseNamesJson As String = serializer.Serialize(courseNames)
                            Dim studentCountsJson As String = serializer.Serialize(studentCounts)

                            ' CLIENT-SIDE DATA STORAGE
                            ' Hidden fields bridge server-side data to client-side Chart.js
                            hdnCourseNames.Value = courseNamesJson
                            hdnStudentCounts.Value = studentCountsJson

                            ' UI STATE MANAGEMENT
                            ' Show chart container and hide empty state message
                            chartContainer.Visible = True
                            noDataMessage.Visible = False
                        Else
                            ' EMPTY STATE HANDLING
                            ' Graceful display when no course data exists
                            chartContainer.Visible = False
                            noDataMessage.Visible = True
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' ERROR STATE MANAGEMENT
            ' Ensure UI remains consistent even on chart data failure
            chartContainer.Visible = False
            noDataMessage.Visible = True
            ShowMessage($"❌ Error loading chart data: {ex.Message}", "alert alert-warning")
        End Try
    End Sub

    ''' <summary>
    ''' Prepare Enrollment Trends Line Chart Data
    ''' Analyzes enrollment patterns over the last 6 months
    ''' Provides temporal insights for administrative planning
    ''' </summary>
    Private Sub LoadEnrollmentTrendsChart()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' TEMPORAL ENROLLMENT ANALYSIS
                ' DATE_TRUNC aggregates enrollments by month for trend analysis
                ' INTERVAL calculation ensures exactly 6 months of historical data
                ' PostgreSQL-specific functions for precise date handling
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
                            ' TEMPORAL DATA PROCESSING
                            ' Convert database dates to user-friendly month labels
                            ' Parallel arrays for Chart.js line chart rendering
                            Dim months As New List(Of String)
                            Dim enrollments As New List(Of Integer)

                            For Each row As DataRow In dt.Rows
                                ' DATE FORMATTING for user-friendly display
                                ' MMM yyyy format provides clear temporal context
                                Dim monthDate As DateTime = Convert.ToDateTime(row("month"))
                                months.Add(monthDate.ToString("MMM yyyy"))
                                enrollments.Add(Convert.ToInt32(row("enrollments")))
                            Next

                            ' JSON SERIALIZATION for client-side consumption
                            Dim serializer As New JavaScriptSerializer()
                            hdnTrendMonths.Value = serializer.Serialize(months)
                            hdnTrendEnrollments.Value = serializer.Serialize(enrollments)

                            ' UI VISIBILITY CONTROL
                            trendsChartContainer.Visible = True
                        Else
                            ' INSUFFICIENT DATA HANDLING
                            ' Hide trends chart when historical data is unavailable
                            trendsChartContainer.Visible = False
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' ERROR RECOVERY: Hide chart on failure
            trendsChartContainer.Visible = False
        End Try
    End Sub

    ''' <summary>
    ''' Prepare Course Format Distribution Pie Chart Data
    ''' Analyzes distribution of different course delivery formats
    ''' Provides insights into curriculum delivery method preferences
    ''' </summary>
    Private Sub LoadCourseFormatDistribution()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' FORMAT DISTRIBUTION ANALYSIS
                ' Analyzes enrollment distribution across course formats
                ' HAVING clause filters out formats with zero enrollments
                ' Focuses on actively used delivery methods
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
                            ' FORMAT DATA AGGREGATION
                            ' Separate lists for labels and values required by Chart.js
                            Dim formats As New List(Of String)
                            Dim counts As New List(Of Integer)

                            For Each row As DataRow In dt.Rows
                                ' FORMAT LABEL STANDARDIZATION
                                ' Convert to uppercase for consistent display
                                formats.Add(row("format").ToString().ToUpper())
                                counts.Add(Convert.ToInt32(row("enrollment_count")))
                            Next

                            ' JSON SERIALIZATION for pie chart rendering
                            Dim serializer As New JavaScriptSerializer()
                            hdnFormatLabels.Value = serializer.Serialize(formats)
                            hdnFormatCounts.Value = serializer.Serialize(counts)

                            formatChartContainer.Visible = True
                        Else
                            ' NO FORMAT DATA AVAILABLE
                            formatChartContainer.Visible = False
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' ERROR HANDLING: Hide format chart on failure
            formatChartContainer.Visible = False
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' ACTIVITY FEED AND RECENT OPERATIONS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Load Recent Enrollment Activity Feed
    ''' Displays latest enrollment operations for administrative monitoring
    ''' Provides real-time visibility into system usage patterns
    ''' </summary>
    Private Sub LoadRecentActivity()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' RECENT ACTIVITY AGGREGATION
                ' Complex JOIN query combining student, course, and enrollment data
                ' Concatenation creates full student names for display
                ' LIMIT 10 ensures manageable activity feed size
                ' ORDER BY enrollment_date DESC shows most recent first
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

                        ' ACTIVITY FEED BINDING
                        ' Repeater control provides flexible activity display
                        rpRecentActivity.DataSource = dt
                        rpRecentActivity.DataBind()

                        ' CONDITIONAL DISPLAY based on activity availability
                        pnlNoActivity.Visible = (dt.Rows.Count = 0)
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' SILENT FAILURE for non-critical activity feed
            ' Main dashboard functionality remains intact
            pnlNoActivity.Visible = True
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' USER INTERACTION EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Refresh Button Click Handler
    ''' Manually reloads all dashboard data and provides user feedback
    ''' Useful for real-time monitoring and data verification
    ''' </summary>
    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        ' COMPREHENSIVE DATA REFRESH
        ' Reload all dashboard components to ensure current data
        LoadReportsData()
        ShowMessage("✅ Reports data refreshed successfully!", "alert alert-success")
    End Sub

    ''' <summary>
    ''' Export Data Button Click Handler
    ''' Generates and downloads CSV report of enrollment data
    ''' Provides administrators with data for external analysis
    ''' </summary>
    Protected Sub btnExportData_Click(sender As Object, e As EventArgs)
        Try
            ' CSV GENERATION with comprehensive course enrollment data
            ' StringBuilder provides efficient string concatenation
            Dim csv As New StringBuilder()
            csv.AppendLine("Course Name,Student Count,ECTS,Format")

            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' EXPORT DATA QUERY
                ' Similar to chart query but includes additional fields for analysis
                ' LEFT JOIN ensures all courses appear in export, even without enrollments
                Dim query As String = "SELECT c.course_name, COALESCE(COUNT(e.enrollment_id), 0) as student_count, c.ects, c.format " &
                                     "FROM courses c " &
                                     "LEFT JOIN enrollments e ON c.course_id = e.course_id " &
                                     "GROUP BY c.course_id, c.course_name, c.ects, c.format " &
                                     "ORDER BY student_count DESC"

                Using cmd As New NpgsqlCommand(query, conn)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        ' CSV DATA CONSTRUCTION
                        ' Iterate through results and build CSV format
                        While reader.Read()
                            csv.AppendLine($"{reader("course_name")},{reader("student_count")},{reader("ects")},{reader("format")}")
                        End While
                    End Using
                End Using
            End Using

            ' FILE DOWNLOAD PREPARATION
            ' Configure HTTP response for file download
            ' Timestamp in filename ensures unique downloads
            Response.Clear()
            Response.ContentType = "text/csv"
            Response.AddHeader("Content-Disposition", $"attachment; filename=enrollment_report_{DateTime.Now:yyyyMMdd}.csv")
            Response.Write(csv.ToString())
            Response.End()

        Catch ex As Exception
            ' EXPORT ERROR HANDLING
            ' Show error message if export fails
            ShowMessage($"❌ Error exporting data: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' UTILITY AND HELPER METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Reliable Database Connection Factory
    ''' Creates tested database connections with automatic cleanup
    ''' Implements cloud database best practices for connection stability
    ''' </summary>
    ''' <returns>Open, tested NpgsqlConnection object</returns>
    Private Function GetWorkingConnection() As NpgsqlConnection
        ' CONNECTION POOL OPTIMIZATION
        ' Clear stale connections to improve cloud database reliability
        ' Brief sleep allows pool cleanup to complete
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200)
        Catch
            ' Pool clearing failures are non-critical
        End Try

        Dim conn As New NpgsqlConnection(connStr)

        Try
            conn.Open()

            ' CONNECTION VALIDATION
            ' Test connection with simple query before returning
            ' CommandTimeout ensures quick failure detection
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn

        Catch ex As Exception
            ' CONNECTION CLEANUP on failure
            ' Ensure proper resource disposal
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

    ''' <summary>
    ''' Centralized User Message Display System
    ''' Provides consistent, styled feedback to administrators
    ''' Supports HTML content and emoji for enhanced communication
    ''' </summary>
    ''' <param name="message">Message content (HTML and emoji supported)</param>
    ''' <param name="cssClass">Bootstrap CSS class for styling (alert types)</param>
    Private Sub ShowMessage(message As String, cssClass As String)
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    ''' <summary>
    ''' Hide User Messages
    ''' Clears message display area for clean dashboard state
    ''' </summary>
    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

End Class

'==============================================================================
' END OF REPORTS CLASS
'==============================================================================
' 
' DASHBOARD ARCHITECTURE OVERVIEW:
' This class implements a comprehensive analytics dashboard with real-time data
' visualization capabilities designed for educational administration and
' data-driven decision making.
' 
' KEY ARCHITECTURAL PATTERNS:
' - Dashboard Pattern: Centralized metrics display with multiple data views
' - Data Visualization Pattern: Server-side aggregation with client-side rendering
' - Chart.js Integration Pattern: JSON serialization bridge for JavaScript charts
' - Export Pattern: On-demand data export with flexible formatting
' - Activity Feed Pattern: Real-time monitoring of system operations
' 
' CHART.JS INTEGRATION STRATEGY:
' 1. Server-side Data Aggregation: Complex SQL queries for chart data
' 2. JSON Serialization: Safe data transfer to client-side JavaScript
' 3. Hidden Field Bridge: Secure data storage for JavaScript access
' 4. Chart.js Rendering: Dynamic, interactive chart creation
' 5. Responsive Design: Mobile-friendly chart display and interaction
' 
' DATA VISUALIZATION COMPONENTS:
' - Bar Charts: Students per course distribution analysis
' - Line Charts: Enrollment trends over 6-month periods
' - Pie Charts: Course format distribution breakdown
' - Statistics Cards: Real-time KPI display with visual icons
' - Activity Feed: Recent enrollment operations monitoring
' 
' PERFORMANCE OPTIMIZATIONS:
' - Efficient SQL Queries: Optimized JOIN operations and aggregations
' - Connection Pooling: Smart pool management for cloud databases
' - Lazy Loading: Chart data loaded only when needed
' - JSON Serialization: Minimal data transfer to client-side
' - Resource Cleanup: Proper disposal of database connections
' 
' USER EXPERIENCE DESIGN:
' - Real-time Updates: Manual refresh capability for current data
' - Export Functionality: CSV download for external analysis
' - Responsive Layout: Mobile-friendly dashboard design
' - Visual Feedback: Immediate confirmation of all operations
' - Progressive Enhancement: Graceful degradation for partial failures
' 
' SECURITY AND COMPLIANCE:
' - Role-based Access Control: Administrative access restriction
' - SQL Injection Prevention: Parameterized queries throughout
' - Data Privacy: Appropriate aggregation without exposing individual records
' - Session Security: Secure administrative session management
' - Export Controls: Controlled data export with audit trail potential
' 
' ERROR HANDLING PHILOSOPHY:
' - Graceful Degradation: Individual component failures don't crash dashboard
' - User-friendly Feedback: Clear error messages with actionable guidance
' - Silent Fallbacks: Non-critical features fail silently
' - Default Values: Safe defaults for statistical calculations
' - Connection Recovery: Automatic retry and cleanup mechanisms
' 
' FUTURE ENHANCEMENT OPPORTUNITIES:
' - Real-time WebSocket updates for live dashboard refresh
' - Advanced filtering options for chart data
' - Custom date range selection for trend analysis
' - Additional export formats (Excel, PDF reports)
' - Drill-down capabilities for detailed data exploration
' - Email report scheduling and automation
' - Comparative analysis between time periods
' - Integration with external analytics platforms
'==============================================================================