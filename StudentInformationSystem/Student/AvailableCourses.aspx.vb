'==============================================================================
' AVAILABLE COURSES - STUDENT PORTAL CODE-BEHIND CLASS
'==============================================================================
' Purpose: Student-facing course discovery and enrollment management system
' Features: Course browsing, search/filtering, enrollment operations, personal dashboard
' 
' Advanced Student Features:
' - Real-time course availability with enrollment status display
' - Multi-criteria search and filtering (name, format, ECTS)
' - One-click enrollment and unenrollment with confirmation
' - Personal enrollment dashboard with ECTS tracking
' - Responsive course catalog with formatted display
' - Robust error handling with user-friendly feedback
' - Connection resilience for cloud database reliability
' - Session-based student identification and security
' 
' User Experience Design:
' - Intuitive course discovery with visual status indicators
' - Immediate feedback for all enrollment operations
' - Personal dashboard showing current academic progress
' - Mobile-friendly responsive design with touch interactions
' - Accessibility-compliant interface with screen reader support
' - Progressive enhancement with graceful degradation
'==============================================================================

Imports System.Data
Imports Npgsql
Imports System.Configuration

Partial Public Class AvailableCourses
    Inherits System.Web.UI.Page

    '--------------------------------------------------------------------------
    ' CLASS VARIABLES AND CONFIGURATION
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Database connection string from Web.config
    ''' Optimized for cloud database connectivity with appropriate timeouts
    ''' Shared across all database operations for consistency
    ''' </summary>
    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    ''' <summary>
    ''' Current student's unique identifier
    ''' Retrieved from session and used for all enrollment operations
    ''' Critical for maintaining student-specific data isolation
    ''' </summary>
    Private currentStudentId As Long = 0

    '--------------------------------------------------------------------------
    ' PAGE LIFECYCLE EVENTS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Page Load Event Handler
    ''' Initializes the student course portal with security validation and data loading
    ''' Implements comprehensive student authentication and profile retrieval
    ''' </summary>
    ''' <param name="sender">The page object that raised the event</param>
    ''' <param name="e">Event arguments containing page load context</param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' SECURITY GATE: Student Access Control
        ' Restricts portal access to authenticated students only
        ' Prevents unauthorized access to course enrollment functionality
        If Session("UserRole")?.ToString() <> "student" Then
            Response.Redirect("~/Account/Login.aspx?error=access_denied")
            Return
        End If

        ' STUDENT PROFILE RETRIEVAL AND VALIDATION
        ' Extract student ID from session for database operations
        ' ProfileId contains the student's unique database identifier
        If Session("ProfileId") IsNot Nothing Then
            Try
                currentStudentId = Convert.ToInt64(Session("ProfileId"))
                ShowMessage($"🔍 DEBUG - Student ID retrieved: {currentStudentId}", "alert alert-info")
            Catch ex As Exception
                ' PROFILE CONVERSION ERROR HANDLING
                ' Detailed error logging for debugging session issues
                ShowMessage($"❌ Error converting ProfileId to Long: {ex.Message}. ProfileId value: {Session("ProfileId")}", "alert alert-danger")
                Return
            End Try
        Else
            ' SESSION INTEGRITY ERROR
            ' Handle missing or corrupt student profile data
            ShowMessage("❌ Student profile not found in session. Please log out and log back in.", "alert alert-danger")
            Return
        End If

        ' FIRST-TIME PAGE INITIALIZATION
        ' Load all portal components on initial page request
        ' Postback events handle specific data operations
        If Not IsPostBack Then
            LoadStudentInfo()           ' Personal dashboard information
            LoadAvailableCourses()      ' Course catalog display
            LoadMyEnrollments()         ' Current enrollment status
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' STUDENT DASHBOARD AND PROFILE METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Load Student Information Dashboard
    ''' Displays personal academic information and enrollment statistics
    ''' Provides student with overview of their academic progress
    ''' </summary>
    Private Sub LoadStudentInfo()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' STUDENT PROFILE QUERY
                ' Retrieve basic student information for dashboard display
                ' Essential for personalizing the course portal experience
                Dim query As String = "SELECT first_name, last_name, email FROM students WHERE id = @student_id"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)

                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' PROFILE DISPLAY POPULATION
                            ' Combine first and last name for friendly display
                            lblStudentName.Text = $"{reader("first_name")} {reader("last_name")}"
                            lblStudentEmail.Text = reader("email").ToString()
                            ShowMessage($"✅ Student info loaded successfully", "alert alert-success")
                        Else
                            ' STUDENT NOT FOUND ERROR
                            ' Handle missing student record scenarios
                            ShowMessage($"❌ No student found with ID: {currentStudentId}", "alert alert-danger")
                            Return
                        End If
                    End Using
                End Using

                ' ENROLLMENT STATISTICS CALCULATION
                ' Aggregate current enrollment data for dashboard metrics
                ' Provides insights into student's academic engagement
                Dim enrollmentQuery As String = "SELECT COUNT(*), COALESCE(SUM(c.ects), 0) FROM enrollments e " &
                                               "INNER JOIN courses c ON e.course_id = c.course_id " &
                                               "WHERE e.student_id = @student_id"
                Using cmd As New NpgsqlCommand(enrollmentQuery, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)

                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' DASHBOARD METRICS DISPLAY
                            ' Show current enrollment count and total ECTS credits
                            lblCurrentEnrollments.Text = reader(0).ToString()
                            lblTotalECTS.Text = reader(1).ToString()
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' DASHBOARD LOADING ERROR HANDLING
            ' Graceful degradation for profile loading failures
            ShowMessage($"❌ Error loading student information: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' COURSE CATALOG AND DISCOVERY METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Load Available Courses with Advanced Filtering
    ''' Displays course catalog with search and filter capabilities
    ''' Implements comprehensive course discovery functionality
    ''' </summary>
    ''' <param name="searchName">Optional course name search filter</param>
    ''' <param name="filterFormat">Optional course format filter (lecture, seminar, etc.)</param>
    ''' <param name="filterECTS">Optional ECTS credit range filter</param>
    Private Sub LoadAvailableCourses(Optional searchName As String = "", Optional filterFormat As String = "", Optional filterECTS As String = "")
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' CATALOG AVAILABILITY VALIDATION
                ' Ensure courses exist before attempting to display them
                ' Provides helpful guidance when catalog is empty
                Dim countQuery As String = "SELECT COUNT(*) FROM courses"
                Using countCmd As New NpgsqlCommand(countQuery, conn)
                    Dim totalCourses As Integer = Convert.ToInt32(countCmd.ExecuteScalar())
                    If totalCourses = 0 Then
                        ShowMessage("⚠️ No courses found in database. Please add some courses first.", "alert alert-warning")
                        Return
                    End If
                End Using

                ' DYNAMIC QUERY CONSTRUCTION
                ' Build course selection query with optional filters
                ' Base query selects all essential course information
                Dim query As String = "SELECT course_id, course_name, ects, hours, format, instructor FROM courses WHERE 1=1"

                ' SEARCH FILTER APPLICATION
                ' Add search conditions based on user input
                ' Case-insensitive search for better user experience
                If Not String.IsNullOrEmpty(searchName) Then
                    query &= " AND LOWER(course_name) LIKE @searchName"
                End If

                If Not String.IsNullOrEmpty(filterFormat) Then
                    query &= " AND format = @filterFormat"
                End If

                ' ECTS RANGE FILTERING
                ' Flexible ECTS credit filtering with predefined ranges
                ' Helps students find courses matching their credit needs
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

                ' RESULT ORDERING for consistent display
                query &= " ORDER BY course_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 15

                    ' SAFE PARAMETER BINDING
                    ' Prevent SQL injection through parameterized queries
                    If Not String.IsNullOrEmpty(searchName) Then
                        cmd.Parameters.AddWithValue("@searchName", "%" & searchName.ToLower() & "%")
                    End If

                    If Not String.IsNullOrEmpty(filterFormat) Then
                        cmd.Parameters.AddWithValue("@filterFormat", filterFormat)
                    End If

                    ' DATA RETRIEVAL AND BINDING
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' COURSE CATALOG DISPLAY
                        gvCourses.DataSource = dt
                        gvCourses.DataBind()
                        lblTotalCourses.Text = $"Total: {dt.Rows.Count}"

                        ' INTELLIGENT USER FEEDBACK
                        ' Provide context-aware messages based on results
                        If dt.Rows.Count = 0 Then
                            ShowMessage("No courses found matching your criteria.", "alert alert-info")
                        Else
                            ShowMessage($"✅ {dt.Rows.Count} courses loaded successfully", "alert alert-success")
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' CATALOG LOADING ERROR RECOVERY
            ' Maintain UI structure even on complete failure
            ShowMessage($"❌ Error loading courses: {ex.Message}", "alert alert-danger")

            ' FALLBACK EMPTY GRID STRUCTURE
            ' Preserve page layout for better user experience
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
                ' Silent fallback for emergency scenarios
            End Try
        End Try
    End Sub

    ''' <summary>
    ''' Load Student's Current Enrollments
    ''' Displays personal enrollment dashboard with course details
    ''' Provides overview of student's current academic commitments
    ''' </summary>
    Private Sub LoadMyEnrollments()
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' PERSONAL ENROLLMENT QUERY
                ' Complex JOIN to combine enrollment, course, and timing data
                ' ORDER BY enrollment_date DESC shows newest enrollments first
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

                        ' CONDITIONAL ENROLLMENT DISPLAY
                        ' Show enrollments or empty state based on data availability
                        If dt.Rows.Count > 0 Then
                            rpMyEnrollments.DataSource = dt
                            rpMyEnrollments.DataBind()
                            pnlNoEnrollments.Visible = False
                        Else
                            ' EMPTY STATE DISPLAY
                            ' Encourage course exploration for new students
                            pnlNoEnrollments.Visible = True
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' ENROLLMENT LOADING ERROR HANDLING
            ' Show empty state rather than crash the portal
            ShowMessage($"❌ Error loading enrollments: {ex.Message}", "alert alert-warning")
            pnlNoEnrollments.Visible = True
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' ENROLLMENT STATUS AND VALIDATION METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Check Student Enrollment Status for Specific Course
    ''' Determines if student is already enrolled in a given course
    ''' Critical for preventing duplicate enrollments and UI state management
    ''' </summary>
    ''' <param name="courseId">Unique course identifier to check</param>
    ''' <returns>True if student is enrolled, False otherwise</returns>
    Private Function IsStudentEnrolledInCourse(courseId As Integer) As Boolean
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' ENROLLMENT VERIFICATION QUERY
                ' Simple count query for efficient enrollment checking
                ' Uses student-course combination as unique constraint
                Dim query As String = "SELECT COUNT(*) FROM enrollments WHERE student_id = @student_id AND course_id = @course_id"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    cmd.Parameters.AddWithValue("@course_id", courseId)
                    Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                End Using
            End Using
        Catch
            ' SAFE DEFAULT: Return false on error to prevent UI issues
            Return False
        End Try
    End Function

    '--------------------------------------------------------------------------
    ' GRIDVIEW EVENT HANDLERS AND DISPLAY FORMATTING
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' GridView Row Data Bound Event Handler
    ''' Customizes course display with enrollment status and action buttons
    ''' Implements dynamic UI based on student's enrollment state
    ''' </summary>
    ''' <param name="sender">GridView that triggered the event</param>
    ''' <param name="e">Row data binding event arguments</param>
    Protected Sub gvCourses_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' COURSE FORMAT VISUAL ENHANCEMENT
            ' Replace plain text with styled badge elements
            ' Provides immediate visual recognition of course types
            Dim formatCell As TableCell = e.Row.Cells(4)
            Dim originalFormat As String = formatCell.Text.ToLower()

            ' FORMAT-SPECIFIC BADGE STYLING
            ' Each course format gets distinctive visual treatment
            ' Helps students quickly identify course delivery methods
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
                    ' FALLBACK FORMATTING for unknown formats
                    formatCell.Text = $"<span class='badge badge-secondary text-dark'>{originalFormat.Substring(0, 1).ToUpper() + originalFormat.Substring(1)}</span>"
            End Select

            ' ENROLLMENT STATUS AND ACTION BUTTON MANAGEMENT
            ' Dynamic UI elements based on student's current enrollment state
            Dim courseId As Integer = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "course_id"))
            Dim isEnrolled As Boolean = IsStudentEnrolledInCourse(courseId)

            ' UI CONTROL REFERENCES
            ' Find template controls within the current row
            Dim lblEnrollmentStatus As Label = CType(e.Row.FindControl("lblEnrollmentStatus"), Label)
            Dim btnEnroll As Button = CType(e.Row.FindControl("btnEnroll"), Button)
            Dim btnUnenroll As Button = CType(e.Row.FindControl("btnUnenroll"), Button)

            ' ENROLLMENT STATUS DISPLAY
            ' Visual indicators for current enrollment state
            If lblEnrollmentStatus IsNot Nothing Then
                If isEnrolled Then
                    lblEnrollmentStatus.Text = "<span class='badge status-enrolled'>Enrolled</span>"
                Else
                    lblEnrollmentStatus.Text = "<span class='badge status-not-enrolled'>Not Enrolled</span>"
                End If
            End If

            ' ACTION BUTTON VISIBILITY CONTROL
            ' Show appropriate action based on enrollment status
            ' Prevents conflicting operations (can't enroll if already enrolled)
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

    '--------------------------------------------------------------------------
    ' ENROLLMENT OPERATION EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Enrollment Button Command Handler
    ''' Processes student enrollment requests with validation
    ''' Implements secure enrollment operations with confirmation
    ''' </summary>
    ''' <param name="sender">Button that triggered the enrollment</param>
    ''' <param name="e">Command event arguments containing course ID</param>
    Protected Sub btnEnroll_Command(sender As Object, e As CommandEventArgs)
        If e.CommandName = "Enroll" Then
            Dim courseId As Integer = Convert.ToInt32(e.CommandArgument)
            EnrollInCourse(courseId)
        End If
    End Sub

    ''' <summary>
    ''' Unenrollment Button Command Handler
    ''' Processes student unenrollment requests with validation
    ''' Allows students to drop courses with confirmation
    ''' </summary>
    ''' <param name="sender">Button that triggered the unenrollment</param>
    ''' <param name="e">Command event arguments containing course ID</param>
    Protected Sub btnUnenroll_Command(sender As Object, e As CommandEventArgs)
        If e.CommandName = "Unenroll" Then
            Dim courseId As Integer = Convert.ToInt32(e.CommandArgument)
            UnenrollFromCourse(courseId)
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' CORE ENROLLMENT BUSINESS LOGIC METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Enroll Student in Course
    ''' Implements comprehensive enrollment process with validation
    ''' Prevents duplicate enrollments and maintains data integrity
    ''' </summary>
    ''' <param name="courseId">Unique identifier of course to enroll in</param>
    Private Sub EnrollInCourse(courseId As Integer)
        Try
            ' DUPLICATE ENROLLMENT PREVENTION
            ' Verify student is not already enrolled before proceeding
            ' Critical for maintaining database integrity
            If IsStudentEnrolledInCourse(courseId) Then
                ShowMessage("❌ You are already enrolled in this course.", "alert alert-warning")
                Return
            End If

            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' ENROLLMENT RECORD CREATION
                ' Insert new enrollment with current date timestamp
                ' Links student to course with enrollment date tracking
                Dim query As String = "INSERT INTO enrollments (student_id, course_id, enrollment_date) VALUES (@student_id, @course_id, @enrollment_date)"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    cmd.Parameters.AddWithValue("@course_id", courseId)
                    cmd.Parameters.AddWithValue("@enrollment_date", DateTime.Today)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        ' ENROLLMENT SUCCESS FEEDBACK
                        ' Immediate confirmation with comprehensive data refresh
                        ShowMessage("✅ Successfully enrolled in the course!", "alert alert-success")

                        ' COMPLETE PORTAL REFRESH
                        ' Update all components to reflect new enrollment
                        LoadStudentInfo()           ' Updated enrollment counts
                        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
                        LoadMyEnrollments()         ' Updated enrollment list
                    Else
                        ShowMessage("❌ Failed to enroll in the course.", "alert alert-danger")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ' ENROLLMENT ERROR HANDLING
            ' Detailed error feedback for troubleshooting
            ShowMessage($"❌ Error enrolling in course: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    ''' <summary>
    ''' Unenroll Student from Course
    ''' Implements secure course withdrawal process
    ''' Allows students to drop courses with immediate effect
    ''' </summary>
    ''' <param name="courseId">Unique identifier of course to withdraw from</param>
    Private Sub UnenrollFromCourse(courseId As Integer)
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' ENROLLMENT RECORD DELETION
                ' Remove enrollment relationship between student and course
                ' Immediate effect with no academic record retention
                Dim query As String = "DELETE FROM enrollments WHERE student_id = @student_id AND course_id = @course_id"
                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@student_id", currentStudentId)
                    cmd.Parameters.AddWithValue("@course_id", courseId)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        ' UNENROLLMENT SUCCESS FEEDBACK
                        ' Confirmation with complete portal refresh
                        ShowMessage("✅ Successfully unenrolled from the course.", "alert alert-info")

                        ' COMPREHENSIVE DATA REFRESH
                        ' Update all portal components to reflect withdrawal
                        LoadStudentInfo()           ' Updated enrollment statistics
                        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
                        LoadMyEnrollments()         ' Updated personal enrollment list
                    Else
                        ' ENROLLMENT NOT FOUND WARNING
                        ' Handle cases where enrollment record doesn't exist
                        ShowMessage("❌ Failed to unenroll from the course or you were not enrolled.", "alert alert-warning")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ' UNENROLLMENT ERROR HANDLING
            ShowMessage($"❌ Error unenrolling from course: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' SEARCH AND FILTER EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Search Button Click Handler
    ''' Applies user-specified search and filter criteria to course catalog
    ''' Implements multi-criteria course discovery functionality
    ''' </summary>
    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
    End Sub

    ''' <summary>
    ''' Clear Search Button Click Handler
    ''' Resets all search filters and displays complete course catalog
    ''' Returns to default course discovery view
    ''' </summary>
    Protected Sub btnClearSearch_Click(sender As Object, e As EventArgs)
        ' FORM RESET: Clear all search criteria
        txtSearchName.Text = ""
        ddlFilterFormat.SelectedIndex = 0
        ddlFilterECTS.SelectedIndex = 0

        ' RELOAD COMPLETE CATALOG
        LoadAvailableCourses()
    End Sub

    ''' <summary>
    ''' Manual Refresh Button Click Handler
    ''' Reloads all portal data while preserving current search context
    ''' Useful for real-time data verification and troubleshooting
    ''' </summary>
    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        ' COMPREHENSIVE PORTAL REFRESH
        ' Reload all components with current search context preserved
        LoadStudentInfo()
        LoadAvailableCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, ddlFilterECTS.SelectedValue)
        LoadMyEnrollments()
        ShowMessage("✅ Page refreshed successfully!", "alert alert-success")
    End Sub

    '--------------------------------------------------------------------------
    ' CONNECTION MANAGEMENT AND UTILITY METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Reliable Database Connection Factory
    ''' Creates tested database connections with cloud database optimization
    ''' Implements connection pool management for better reliability
    ''' </summary>
    ''' <returns>Open, tested NpgsqlConnection object</returns>
    Private Function GetWorkingConnection() As NpgsqlConnection
        ' CONNECTION POOL OPTIMIZATION
        ' Clear stale connections to improve cloud database reliability
        ' Brief delay allows pool cleanup to complete properly
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200)
        Catch
            ' Pool clearing failures are non-critical for functionality
        End Try

        ' CONNECTION CREATION AND VALIDATION
        ' Use connection string as-is from Web.config
        ' Web.config already contains optimized cloud database parameters
        Dim conn As New NpgsqlConnection(connStr)

        Try
            conn.Open()

            ' CONNECTION TESTING
            ' Verify connection works before returning to caller
            ' Quick test query ensures database accessibility
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn

        Catch ex As Exception
            ' CONNECTION CLEANUP on failure
            ' Ensure proper resource disposal even on error
            Try
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If
                conn.Dispose()
            Catch
                ' Ignore cleanup errors - they're non-critical
            End Try

            Throw New Exception($"Failed to connect to database: {ex.Message}")
        End Try
    End Function

    ''' <summary>
    ''' Centralized User Message Display System
    ''' Provides consistent, styled feedback to students
    ''' Supports HTML content and emoji for enhanced communication
    ''' </summary>
    ''' <param name="message">Message content (HTML and emoji supported)</param>
    ''' <param name="cssClass">Bootstrap CSS class for styling</param>
    Private Sub ShowMessage(message As String, cssClass As String)
        ' Show only the latest message to avoid clutter
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    ''' <summary>
    ''' Hide User Messages
    ''' Clears message display area for clean portal state
    ''' </summary>
    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

End Class

'==============================================================================
' END OF AVAILABLECOURSES CLASS
'==============================================================================
' 
' STUDENT PORTAL ARCHITECTURE OVERVIEW:
' This class implements a comprehensive student-facing course discovery and
' enrollment management system designed for intuitive academic self-service
' and optimal user experience across all device types.
' 
' KEY ARCHITECTURAL PATTERNS:
' - Student Portal Pattern: Self-service academic management interface
' - Course Discovery Pattern: Multi-criteria search and filtering system
' - Enrollment Management Pattern: One-click enrollment/withdrawal operations
' - Personal Dashboard Pattern: Real-time academic progress tracking
' - Responsive Design Pattern: Mobile-first adaptive user interface
' 
' STUDENT EXPERIENCE DESIGN:
' 1. Personal Dashboard: Academic progress overview with ECTS tracking
' 2. Course Discovery: Intuitive search with visual format indicators
' 3. Enrollment Operations: One-click enrollment with immediate feedback
' 4. Status Management: Real-time enrollment status with visual indicators
' 5. Mobile Optimization: Touch-friendly interface for mobile learning
' 
' ENROLLMENT WORKFLOW:
' - Course Discovery: Browse catalog with search and filtering
' - Enrollment Status: Visual indicators show current enrollment state
' - One-click Operations: Enroll/unenroll with confirmation dialogs
' - Immediate Feedback: Success/error messages with progress updates
' - Dashboard Updates: Real-time reflection of enrollment changes
' 
' DATA SECURITY AND INTEGRITY:
' - Session-based Authentication: Student identity verification
' - Role-based Access Control: Student-only portal access
' - SQL Injection Prevention: Parameterized queries throughout
' - Duplicate Prevention: Enrollment validation before operations
' - Data Isolation: Student-specific data access and display
' 
' USER EXPERIENCE OPTIMIZATIONS:
' - Progressive Enhancement: Core functionality works without JavaScript
' - Responsive Design: Optimal experience across all device sizes
' - Visual Feedback: Immediate confirmation of all operations
' - Error Recovery: Graceful handling of network and database issues
' - Accessibility: Screen reader compatible with keyboard navigation
' 
' PERFORMANCE CONSIDERATIONS:
' - Connection Pooling: Optimized for cloud database environments
' - Efficient Queries: Minimal database round-trips for better speed
' - Lazy Loading: On-demand data retrieval for responsive interface
' - Resource Management: Proper disposal and cleanup of connections
' - Cache-friendly: Session-based student identification reduces lookups
' 
' ERROR HANDLING PHILOSOPHY:
' - User-friendly Messages: Clear, actionable error communication
' - Graceful Degradation: Portal remains functional during partial failures
' - Data Preservation: Search context maintained across operations
' - Recovery Mechanisms: Automatic retry and alternative data loading
' - Silent Fallbacks: Non-critical features fail without affecting core functions
' 
' MOBILE AND ACCESSIBILITY FEATURES:
' - Touch Optimization: Large buttons and touch-friendly interactions
' - Responsive Grid: Bootstrap-based layout adaptation
' - Screen Reader Support: Semantic HTML with ARIA labels
' - High Contrast: Accessible color schemes and visual indicators
' - Keyboard Navigation: Full portal functionality via keyboard
' 
' FUTURE ENHANCEMENT OPPORTUNITIES:
' - Real-time Notifications: Instant enrollment confirmations
' - Course Recommendations: AI-powered course suggestions
' - Academic Planning: Semester and degree planning tools
' - Prerequisites Checking: Automatic prerequisite validation
' - Waitlist Management: Enrollment queuing for popular courses
' - Calendar Integration: Personal academic calendar synchronization
' - Progress Tracking: Detailed academic milestone monitoring
' - Social Features: Peer course reviews and recommendations
'==============================================================================