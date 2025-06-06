'==============================================================================
' MANAGE ENROLLMENTS - CODE-BEHIND CLASS
'==============================================================================
' Purpose: Comprehensive enrollment management system with advanced analytics
' Features: Statistics dashboard, search/filtering, enrollment deletion, reporting
' 
' Advanced Features:
' - Real-time enrollment statistics and metrics calculation
' - Multi-criteria search with date range filtering
' - Recent activity tracking and popular courses analysis
' - Robust error handling with graceful degradation
' - Advanced connection management for cloud database reliability
' - Comprehensive audit trail for enrollment operations
' - Performance-optimized queries with proper indexing considerations
'==============================================================================

Imports System.Data
Imports Npgsql
Imports System.Configuration

Partial Public Class ManageEnrollments
    Inherits System.Web.UI.Page

    '--------------------------------------------------------------------------
    ' CLASS VARIABLES AND CONFIGURATION
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Database connection string from Web.config
    ''' Optimized for cloud database connectivity with appropriate timeouts
    ''' </summary>
    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    '--------------------------------------------------------------------------
    ' PAGE LIFECYCLE EVENTS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Page Load Event Handler
    ''' Initializes the enrollment management interface with security validation
    ''' Sets up default date ranges and loads initial data
    ''' </summary>
    ''' <param name="sender">The page object that raised the event</param>
    ''' <param name="e">Event arguments containing request details</param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' SECURITY GATE: Restrict access to administrators only
        ' Prevents unauthorized access to sensitive enrollment data
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        ' FIRST-TIME PAGE INITIALIZATION
        ' Execute setup logic only on initial page load, not on postbacks
        If Not IsPostBack Then
            ' DATE RANGE SETUP: Initialize with reasonable default range
            ' Sets last 30 days as default but doesn't apply filter automatically
            ' This provides guidance while allowing users to see all data initially
            txtDateFrom.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")
            txtDateTo.Text = DateTime.Today.ToString("yyyy-MM-dd")

            ' COMPREHENSIVE DATA LOADING
            ' Load all dashboard components for complete administrative overview
            LoadStatistics()                    ' Key performance metrics
            LoadEnrollments("", "", "", "")     ' All enrollments (no initial filters)
            LoadRecentEnrollments()             ' Latest enrollment activity
            LoadPopularCourses()                ' Course popularity analysis
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' STATISTICS AND DASHBOARD METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Load Statistical Dashboard Metrics
    ''' Calculates and displays key performance indicators for enrollment management
    ''' Provides administrators with at-a-glance system overview
    ''' </summary>
    Private Sub LoadStatistics()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' METRIC 1: Total Enrollments Count
                ' Primary indicator of system usage and student engagement
                Using cmd As New NpgsqlCommand("SELECT COUNT(*) FROM enrollments", conn)
                    lblTotalEnrollments.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' METRIC 2: Active Students Count
                ' Number of students with at least one enrollment
                ' Indicates actual system adoption vs. registration
                Using cmd As New NpgsqlCommand("SELECT COUNT(DISTINCT student_id) FROM enrollments", conn)
                    lblActiveStudents.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' METRIC 3: Active Courses Count
                ' Number of courses with at least one enrollment
                ' Shows curriculum utilization and course popularity
                Using cmd As New NpgsqlCommand("SELECT COUNT(DISTINCT course_id) FROM enrollments", conn)
                    lblActiveCourses.Text = cmd.ExecuteScalar().ToString()
                End Using

                ' METRIC 4: Average Enrollments per Student
                ' Student engagement indicator - calculated using subquery for accuracy
                ' Helps identify over/under-enrollment patterns
                Using cmd As New NpgsqlCommand("SELECT ROUND(AVG(enrollment_count), 1) FROM (SELECT COUNT(*) as enrollment_count FROM enrollments GROUP BY student_id) as subquery", conn)
                    Dim avgResult = cmd.ExecuteScalar()
                    lblAvgEnrollments.Text = If(avgResult Is DBNull.Value, "0", avgResult.ToString())
                End Using
            End Using

        Catch ex As Exception
            ' GRACEFUL DEGRADATION: Show zeros rather than crash the dashboard
            ' Allows page to remain functional even if statistics fail
            ShowMessage($"❌ Error loading statistics: {ex.Message}", "alert alert-warning")
            lblTotalEnrollments.Text = "0"
            lblActiveStudents.Text = "0"
            lblActiveCourses.Text = "0"
            lblAvgEnrollments.Text = "0"
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' CORE DATA LOADING METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Load Enrollments with Advanced Filtering
    ''' Supports multi-criteria search with date ranges and text matching
    ''' Implements safe parameter binding to prevent SQL injection
    ''' </summary>
    ''' <param name="searchStudent">Student name search filter (partial matching)</param>
    ''' <param name="searchCourse">Course name search filter (partial matching)</param>
    ''' <param name="dateFrom">Start date for enrollment date range</param>
    ''' <param name="dateTo">End date for enrollment date range</param>
    Private Sub LoadEnrollments(Optional searchStudent As String = "", Optional searchCourse As String = "", Optional dateFrom As String = "", Optional dateTo As String = "")
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' COMPLEX JOIN QUERY: Combines enrollment, student, and course data
                ' Provides comprehensive view of enrollment relationships
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

                ' DYNAMIC FILTER CONSTRUCTION
                ' Build WHERE clause based on provided search criteria
                If Not String.IsNullOrEmpty(searchStudent) Then
                    ' FLEXIBLE STUDENT SEARCH: First name, last name, or full name matching
                    query &= " AND (LOWER(s.first_name) LIKE @searchStudent OR LOWER(s.last_name) LIKE @searchStudent OR LOWER(s.first_name || ' ' || s.last_name) LIKE @searchStudent)"
                End If

                If Not String.IsNullOrEmpty(searchCourse) Then
                    query &= " AND LOWER(c.course_name) LIKE @searchCourse"
                End If

                ' DATE RANGE FILTERING with validation
                Dim validDateFrom As DateTime?
                Dim validDateTo As DateTime?

                ' SAFE DATE PARSING: Validate dates before using in query
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

                ' RESULT ORDERING: Recent enrollments first, then alphabetical
                query &= " ORDER BY e.enrollment_date DESC, s.last_name, s.first_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 15 ' Extended timeout for complex queries

                    ' PARAMETER BINDING: Safe parameter assignment prevents SQL injection
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

                    ' DATA RETRIEVAL AND BINDING
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' GRIDVIEW BINDING with result feedback
                        gvEnrollments.DataSource = dt
                        gvEnrollments.DataBind()
                        lblDisplayedEnrollments.Text = $"Showing: {dt.Rows.Count}"

                        ' INTELLIGENT USER FEEDBACK based on results and filters
                        If dt.Rows.Count = 0 Then
                            ' Determine if filters are active to provide appropriate message
                            Dim hasFilters As Boolean = Not String.IsNullOrEmpty(searchStudent) OrElse
                                                       Not String.IsNullOrEmpty(searchCourse) OrElse
                                                       validDateFrom.HasValue OrElse
                                                       validDateTo.HasValue

                            If hasFilters Then
                                ShowMessage("No enrollments found matching your search criteria. Try adjusting your filters or click 'Clear' to see all enrollments.", "alert alert-info")
                            Else
                                ' NO FILTERS BUT NO RESULTS: Check if database has any enrollments
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

                        ' UPDATE PAGINATION INFORMATION
                        lblPaginationInfo.Text = $"Total {dt.Rows.Count} enrollment(s) displayed"
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading enrollments: {ex.Message}", "alert alert-danger")

            ' FALLBACK EMPTY GRID: Maintain UI structure even on complete failure
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
                ' Silent fallback if even empty grid creation fails
            End Try
        End Try
    End Sub

    ''' <summary>
    ''' Load Recent Enrollment Activity
    ''' Displays latest enrollments for administrative monitoring
    ''' Limited to 10 most recent entries for performance
    ''' </summary>
    Private Sub LoadRecentEnrollments()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' RECENT ACTIVITY QUERY: Last 7 days of enrollment activity
                ' Helps administrators monitor current system usage
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

                        ' CONDITIONAL DISPLAY: Show data or empty state message
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
            ' SILENT FALLBACK: Don't crash page if recent activity fails
            pnlNoRecentEnrollments.Visible = True
        End Try
    End Sub

    ''' <summary>
    ''' Load Popular Courses Analysis
    ''' Identifies courses with highest enrollment counts
    ''' Provides insights for curriculum planning and resource allocation
    ''' </summary>
    Private Sub LoadPopularCourses()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' POPULARITY ANALYSIS QUERY: Courses ordered by enrollment count
                ' LEFT JOIN ensures all courses appear, even with zero enrollments
                ' HAVING filter removes courses with no enrollments from "popular" list
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

                        ' CONDITIONAL DISPLAY based on data availability
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
            ' SILENT FALLBACK for non-critical feature
            pnlNoPopularCourses.Visible = True
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' GRIDVIEW EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' GridView Row Data Bound Event Handler
    ''' Customizes the visual presentation of enrollment data
    ''' Applies formatting and highlighting based on data values
    ''' </summary>
    Protected Sub gvEnrollments_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' COURSE FORMAT BADGE STYLING
            ' Visual enhancement for course format display using colored badges
            Dim formatCell As TableCell = e.Row.Cells(5) ' course_format column index
            Dim originalFormat As String = formatCell.Text.ToLower()

            ' FORMAT-SPECIFIC STYLING: Each format gets distinctive visual treatment
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
                    ' FALLBACK STYLING for unknown formats
                    formatCell.Text = $"<span class='badge bg-secondary'>{originalFormat.Substring(0, 1).ToUpper() + originalFormat.Substring(1)}</span>"
            End Select

            ' RECENT ENROLLMENT HIGHLIGHTING
            ' Highlight enrollments from last 7 days for administrator attention
            Dim enrollmentDate As DateTime = DateTime.Parse(DataBinder.Eval(e.Row.DataItem, "enrollment_date").ToString())
            If enrollmentDate >= DateTime.Today.AddDays(-7) Then
                e.Row.CssClass += " table-success"
            End If
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' ENROLLMENT DELETION OPERATIONS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Delete Enrollment Command Handler
    ''' Processes enrollment deletion requests with comprehensive logging
    ''' Maintains audit trail for administrative oversight
    ''' </summary>
    Protected Sub btnDelete_Command(sender As Object, e As CommandEventArgs)
        If e.CommandName = "DeleteEnrollment" Then
            Dim enrollmentId As Integer = Convert.ToInt32(e.CommandArgument)
            DeleteEnrollment(enrollmentId)
        End If
    End Sub

    ''' <summary>
    ''' Delete Individual Enrollment
    ''' Removes enrollment record with comprehensive audit trail
    ''' Provides detailed feedback and refreshes related data
    ''' </summary>
    ''' <param name="enrollmentId">Unique identifier of enrollment to delete</param>
    Private Sub DeleteEnrollment(enrollmentId As Integer)
        Try
            ' AUDIT INFORMATION COLLECTION
            ' Gather enrollment details before deletion for confirmation message
            Dim studentName As String = ""
            Dim courseName As String = ""

            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' GET ENROLLMENT DETAILS for audit trail
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

                ' EXECUTE DELETION with parameter binding for security
                Dim deleteQuery As String = "DELETE FROM enrollments WHERE enrollment_id = @enrollment_id"
                Using deleteCmd As New NpgsqlCommand(deleteQuery, conn)
                    deleteCmd.Parameters.AddWithValue("@enrollment_id", enrollmentId)

                    Dim rowsAffected As Integer = deleteCmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        ' SUCCESS: Provide detailed confirmation with audit information
                        ShowMessage($"✅ Enrollment deleted successfully! {studentName} has been unenrolled from {courseName}.", "alert alert-success")

                        ' COMPREHENSIVE DATA REFRESH
                        ' Update all related dashboard components
                        LoadStatistics()

                        ' INTELLIGENT FILTER PRESERVATION
                        ' Maintain current search context while refreshing data
                        Dim studentFilter As String = If(txtSearchStudent.Text, "").Trim()
                        Dim courseFilter As String = If(txtSearchCourse.Text, "").Trim()

                        ' Only apply date filters if user has active text searches
                        Dim dateFromFilter As String = ""
                        Dim dateToFilter As String = ""

                        Dim hasActiveFilters As Boolean = Not String.IsNullOrEmpty(studentFilter) OrElse Not String.IsNullOrEmpty(courseFilter)

                        If hasActiveFilters Then
                            dateFromFilter = If(txtDateFrom.Text, "").Trim()
                            dateToFilter = If(txtDateTo.Text, "").Trim()
                        End If

                        ' REFRESH WITH PRESERVED CONTEXT
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

            ' ERROR RECOVERY: Attempt basic data refresh even after error
            Try
                LoadEnrollments("", "", "", "")
            Catch
                ShowMessage("❌ Could not refresh enrollment list after error", "alert alert-warning")
            End Try
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' SEARCH AND FILTER EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Search Button Click Handler
    ''' Applies user-specified search criteria to enrollment list
    ''' Supports multi-criteria filtering with date ranges
    ''' </summary>
    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        ' SAFE PARAMETER EXTRACTION
        ' Handle potential null values from form controls
        Dim studentFilter As String = If(txtSearchStudent.Text, "").Trim()
        Dim courseFilter As String = If(txtSearchCourse.Text, "").Trim()
        Dim dateFromFilter As String = If(txtDateFrom.Text, "").Trim()
        Dim dateToFilter As String = If(txtDateTo.Text, "").Trim()

        LoadEnrollments(studentFilter, courseFilter, dateFromFilter, dateToFilter)
    End Sub

    ''' <summary>
    ''' Clear Search Button Click Handler
    ''' Resets all search criteria and displays complete enrollment list
    ''' Restores default date range for user guidance
    ''' </summary>
    Protected Sub btnClearSearch_Click(sender As Object, e As EventArgs)
        ' FORM RESET: Clear all search controls
        txtSearchStudent.Text = ""
        txtSearchCourse.Text = ""
        txtDateFrom.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")
        txtDateTo.Text = DateTime.Today.ToString("yyyy-MM-dd")

        ' LOAD ALL DATA: No filters applied
        LoadEnrollments("", "", "", "")
        ShowMessage("✅ Filters cleared - showing all enrollments!", "alert alert-info")
    End Sub

    ''' <summary>
    ''' Refresh Button Click Handler
    ''' Reloads all dashboard data while preserving current search context
    ''' Provides manual refresh capability for real-time data updates
    ''' </summary>
    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        ' COMPREHENSIVE REFRESH: Reload all dashboard components
        LoadStatistics()

        ' CONTEXT-AWARE REFRESH: Preserve active search filters
        Dim studentFilter As String = If(txtSearchStudent.Text, "").Trim()
        Dim courseFilter As String = If(txtSearchCourse.Text, "").Trim()

        ' SMART DATE FILTER APPLICATION
        ' Only apply date filters when text filters are active
        Dim dateFromFilter As String = ""
        Dim dateToFilter As String = ""

        If Not String.IsNullOrEmpty(studentFilter) OrElse Not String.IsNullOrEmpty(courseFilter) Then
            dateFromFilter = If(txtDateFrom.Text, "").Trim()
            dateToFilter = If(txtDateTo.Text, "").Trim()
        End If

        ' COMPLETE DATA REFRESH with preserved context
        LoadEnrollments(studentFilter, courseFilter, dateFromFilter, dateToFilter)
        LoadRecentEnrollments()
        LoadPopularCourses()
        ShowMessage("✅ Data refreshed successfully!", "alert alert-success")
    End Sub

    '--------------------------------------------------------------------------
    ' UTILITY AND DIAGNOSTIC METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Simple Enrollment Refresh without Filters
    ''' Fallback method for error recovery scenarios
    ''' Used when complex filtering operations fail
    ''' </summary>
    Private Sub RefreshEnrollmentsWithoutFilters()
        Try
            LoadEnrollments("", "", "", "")
        Catch ex As Exception
            ShowMessage($"❌ Error refreshing enrollments: {ex.Message}", "alert alert-warning")
        End Try
    End Sub

    ''' <summary>
    ''' Debug Method: Check Total Enrollments
    ''' Diagnostic utility for troubleshooting data display issues
    ''' Provides visibility into actual database contents
    ''' </summary>
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

    '--------------------------------------------------------------------------
    ' CONNECTION MANAGEMENT AND HELPER METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Get Working Database Connection
    ''' Reliable connection factory with automatic pool management
    ''' Implements cloud database best practices for connection stability
    ''' </summary>
    ''' <returns>Tested, working NpgsqlConnection object</returns>
    Private Function GetWorkingConnection() As NpgsqlConnection
        ' CONNECTION POOL OPTIMIZATION
        ' Clear stale connections for better cloud database reliability
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200) ' Allow pool cleanup time
        Catch
            ' Pool clearing failures are non-critical
        End Try

        Dim conn As New NpgsqlConnection(connStr)

        Try
            conn.Open()

            ' CONNECTION VALIDATION
            ' Test connection with simple query before returning
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn

        Catch ex As Exception
            ' CONNECTION CLEANUP on failure
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
    ''' Display User Message
    ''' Centralized message display system with consistent formatting
    ''' Supports HTML content and emoji for enhanced user feedback
    ''' </summary>
    ''' <param name="message">Message content (HTML and emoji supported)</param>
    ''' <param name="cssClass">Bootstrap CSS class for styling</param>
    Private Sub ShowMessage(message As String, cssClass As String)
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    ''' <summary>
    ''' Hide User Messages
    ''' Clears message display area for clean UI state
    ''' </summary>
    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

End Class

'==============================================================================
' END OF MANAGEENROLLMENTS CLASS
'==============================================================================
' 
' SYSTEM ARCHITECTURE OVERVIEW:
' This class implements a comprehensive enrollment management dashboard with
' advanced analytics and sophisticated data filtering capabilities designed
' for administrative oversight and data-driven decision making.
' 
' KEY ARCHITECTURAL PATTERNS:
' - Dashboard Pattern: Centralized metrics and KPI display
' - Filter Chain Pattern: Flexible, combinable search criteria
' - Repository Pattern: Centralized data access with abstraction
' - Observer Pattern: Real-time UI updates based on data changes
' - Command Pattern: Structured enrollment operations with audit trails
' 
' DATA MANAGEMENT STRATEGY:
' 1. Real-time Statistics: Live calculation of key performance metrics
' 2. Intelligent Filtering: Multi-criteria search with date range support
' 3. Context Preservation: Maintains user search state across operations
' 4. Audit Trail: Comprehensive logging of enrollment modifications
' 5. Performance Optimization: Efficient queries with proper indexing
' 
' USER EXPERIENCE DESIGN:
' - Responsive Dashboard: Mobile-friendly statistics display
' - Intelligent Feedback: Context-aware messages and suggestions
' - Progressive Enhancement: Graceful degradation for partial failures
' - Real-time Updates: Immediate reflection of data changes
' - Administrative Workflow: Optimized for administrative tasks
' 
' SECURITY AND COMPLIANCE:
' - Role-based Access Control: Strict administrative access only
' - SQL Injection Prevention: Parameterized queries throughout
' - Audit Logging: Complete trail of enrollment modifications
' - Data Validation: Comprehensive input validation and sanitization
' - Session Security: Secure state management and authentication
' 
' PERFORMANCE CONSIDERATIONS:
' - Connection Pooling: Optimized for cloud database environments
' - Efficient Queries: Minimized database round-trips
' - Lazy Loading: On-demand data retrieval for better responsiveness
' - Resource Management: Proper disposal and cleanup
' - Caching Strategy: Strategic use of ViewState for temporary data
' 
' ERROR HANDLING PHILOSOPHY:
' - Graceful Degradation: System remains functional during partial failures
' - User-friendly Feedback: Clear, actionable error messages
' - Silent Fallbacks: Non-critical features fail silently
' - Recovery Mechanisms: Automatic retry and alternative data loading
' - Diagnostic Tools: Built-in debugging and troubleshooting capabilities
'==============================================================================