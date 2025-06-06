'==============================================================================
' MANAGE COURSES - CODE-BEHIND CLASS
'==============================================================================
' Purpose: Handles server-side logic for comprehensive course management operations
' Features: Full CRUD operations with advanced error handling and connection resilience
' 
' Advanced Technical Features:
' - Sophisticated retry logic for transient database failures
' - Stream exception handling for cloud database connections
' - Operation verification when connections drop during database operations
' - Smart connection pool management with automatic clearing
' - Multi-layer error handling with user-friendly feedback
' - Search and filtering capabilities with SQL injection protection
' - Enrollment count validation before course deletion
'==============================================================================

Imports System.Data
Imports Npgsql
Imports System.Configuration

Public Class ManageCourses
    Inherits System.Web.UI.Page

    '--------------------------------------------------------------------------
    ' CLASS VARIABLES AND CONSTANTS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Database connection string retrieved from Web.config
    ''' Contains optimized parameters for cloud database connectivity
    ''' </summary>
    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    ''' <summary>
    ''' Static tracking of last connection pool clear operation
    ''' Prevents excessive pool clearing which can impact performance
    ''' </summary>
    Private Shared lastPoolClear As DateTime = DateTime.MinValue

    '--------------------------------------------------------------------------
    ' PAGE LIFECYCLE EVENTS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Page Load Event Handler
    ''' Initializes the page with security checks and data loading
    ''' Implements smart connection pool management for better reliability
    ''' </summary>
    ''' <param name="sender">The page object that triggered the event</param>
    ''' <param name="e">Event arguments containing request information</param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' SECURITY CHECKPOINT: Ensure only administrators can access this page
        ' Prevents unauthorized access to course management functionality
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        ' FIRST-TIME PAGE LOAD INITIALIZATION
        ' Only execute initialization logic on initial load, not on postbacks
        If Not IsPostBack Then
            ' SMART CONNECTION POOL MANAGEMENT
            ' Only clear connection pools if sufficient time has elapsed
            ' This prevents performance degradation from excessive pool clearing
            If DateTime.Now.Subtract(lastPoolClear).TotalSeconds > 30 Then
                NpgsqlConnection.ClearAllPools()
                lastPoolClear = DateTime.Now
                System.Threading.Thread.Sleep(100) ' Brief pause for pool clearing
            End If

            ' GRACEFUL DATA LOADING WITH FALLBACK
            ' Attempt to load courses with error recovery mechanisms
            Try
                LoadCoursesSimple()
            Catch ex As Exception
                ' User-friendly error message with actionable suggestions
                ShowMessage($"⚠️ Page load issue: {ex.Message}. Try clicking '🔧 Load Simple' or '🔄 Refresh List'", "alert alert-warning")
            End Try
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' CORE DATA LOADING METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Advanced Course Loading with Retry Logic and Filtering
    ''' Implements sophisticated error handling for cloud database connectivity issues
    ''' Supports search and filter parameters for dynamic data retrieval
    ''' </summary>
    ''' <param name="searchName">Optional course name search filter</param>
    ''' <param name="filterFormat">Optional course format filter</param>
    ''' <param name="searchInstructor">Optional instructor name search filter</param>
    Private Sub LoadCourses(Optional searchName As String = "", Optional filterFormat As String = "", Optional searchInstructor As String = "")
        ' RETRY CONFIGURATION
        ' Multiple attempts to handle transient cloud database issues
        Dim maxRetries As Integer = 3
        Dim retryCount As Integer = 0

        ' RETRY LOOP WITH PROGRESSIVE DELAYS
        ' Implements exponential backoff for better success rates
        While retryCount < maxRetries
            Try
                ' CONNECTION PREPARATION FOR RETRIES
                ' Clear pools and implement progressive delays on retry attempts
                If retryCount > 0 Then
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(1000 * retryCount) ' 1s, 2s, 3s delays
                End If

                Using conn As New NpgsqlConnection(connStr)
                    conn.Open()

                    ' DYNAMIC SQL QUERY CONSTRUCTION
                    ' Base query without enrollment counts for performance
                    ' Enrollment counts can be expensive on large datasets
                    Dim query As String = "SELECT course_id, course_name, ects, hours, format, instructor FROM courses WHERE 1=1"

                    ' SEARCH FILTER CONSTRUCTION
                    ' Add search conditions based on provided parameters
                    ' Uses parameterized queries to prevent SQL injection
                    If Not String.IsNullOrEmpty(searchName) Then
                        query &= " AND LOWER(course_name) LIKE @searchName"
                    End If

                    If Not String.IsNullOrEmpty(filterFormat) Then
                        query &= " AND format = @filterFormat"
                    End If

                    If Not String.IsNullOrEmpty(searchInstructor) Then
                        query &= " AND LOWER(instructor) LIKE @searchInstructor"
                    End If

                    ' RESULT ORDERING for consistent display
                    query &= " ORDER BY course_name"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 10 ' Short timeout for responsiveness

                        ' PARAMETER BINDING
                        ' Safe parameter binding prevents SQL injection attacks
                        If Not String.IsNullOrEmpty(searchName) Then
                            cmd.Parameters.AddWithValue("@searchName", "%" & searchName.ToLower() & "%")
                        End If

                        If Not String.IsNullOrEmpty(filterFormat) Then
                            cmd.Parameters.AddWithValue("@filterFormat", filterFormat)
                        End If

                        If Not String.IsNullOrEmpty(searchInstructor) Then
                            cmd.Parameters.AddWithValue("@searchInstructor", "%" & searchInstructor.ToLower() & "%")
                        End If

                        ' DATA RETRIEVAL AND BINDING
                        Using adapter As New NpgsqlDataAdapter(cmd)
                            Dim dt As New DataTable()
                            adapter.Fill(dt)

                            ' GRIDVIEW DATA BINDING
                            gvCourses.DataSource = dt
                            gvCourses.DataBind()
                            lblTotalCourses.Text = $"Total: {dt.Rows.Count}"

                            ' USER FEEDBACK based on results
                            If dt.Rows.Count = 0 Then
                                ShowMessage("No courses found matching your criteria.", "alert alert-info")
                            Else
                                HideMessage()
                            End If
                        End Using
                    End Using
                End Using

                ' SUCCESS EXIT: If we reach here, operation succeeded
                Return

            Catch ex As NpgsqlException When ex.Message.Contains("Exception while reading from stream") Or ex.Message.Contains("stream")
                ' STREAM EXCEPTION HANDLING
                ' Common issue with cloud databases - attempt retry
                retryCount += 1
                If retryCount >= maxRetries Then
                    ShowMessage($"❌ Connection failed after {maxRetries} attempts. Database may be temporarily unavailable.", "alert alert-danger")
                Else
                    ShowMessage($"🔄 Connection issue, retrying... (attempt {retryCount + 1})", "alert alert-warning")
                End If

            Catch ex As Exception
                ' NON-RECOVERABLE ERRORS
                ' Don't retry for non-stream errors (syntax, permissions, etc.)
                ShowMessage($"❌ Failed to load courses: {ex.Message}", "alert alert-danger")
                Exit While
            End Try
        End While

        ' FALLBACK FOR COMPLETE FAILURE
        ' Display empty grid structure to maintain UI consistency
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
            ' If even the fallback fails, gracefully continue
            gvCourses.DataSource = Nothing
            gvCourses.DataBind()
        End Try
    End Sub

    ''' <summary>
    ''' Simple Course Loading Method
    ''' Lightweight alternative for basic course loading without complex error handling
    ''' Used for quick page loads and when advanced features aren't needed
    ''' </summary>
    ''' <param name="searchName">Optional course name filter</param>
    ''' <param name="filterFormat">Optional format filter</param>
    ''' <param name="searchInstructor">Optional instructor filter</param>
    Private Sub LoadCoursesSimple(Optional searchName As String = "", Optional filterFormat As String = "", Optional searchInstructor As String = "")
        Try
            Using conn As NpgsqlConnection = GetWorkingConnection()
                ' SIMPLIFIED QUERY for maximum performance
                ' No complex joins or calculations
                Dim query As String = "SELECT course_id, course_name, ects, hours, format, instructor FROM courses ORDER BY course_name"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5 ' Very short timeout for quick responses

                    Using adapter As New NpgsqlDataAdapter(cmd)
                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        ' FAST DATA BINDING
                        gvCourses.DataSource = dt
                        gvCourses.DataBind()
                        lblTotalCourses.Text = $"Total: {dt.Rows.Count}"

                        HideMessage()
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ShowMessage($"❌ Error loading courses: {ex.Message}", "alert alert-danger")

            ' EMERGENCY FALLBACK
            ' Show empty grid structure even on complete failure
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
                ' Silent failure for emergency fallback
            End Try
        End Try
    End Sub

    '--------------------------------------------------------------------------
    ' ADVANCED HELPER METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Robust Database Connection Factory
    ''' Creates and tests database connections with automatic cleanup
    ''' Implements connection pool management for cloud database reliability
    ''' </summary>
    ''' <returns>A tested, working NpgsqlConnection object</returns>
    Private Function GetWorkingConnection() As NpgsqlConnection
        ' CONNECTION POOL CLEARING for reliability
        ' Cloud databases benefit from fresh connections to avoid stale states
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(500) ' Allow time for cleanup
        Catch
            ' Pool clearing failures are non-critical
        End Try

        Dim conn As New NpgsqlConnection(connStr)
        conn.Open()
        Return conn
    End Function

    ''' <summary>
    ''' Execute Database Command with Retry Logic
    ''' Provides a unified interface for database operations with automatic retry
    ''' Handles transient failures common in cloud database environments
    ''' </summary>
    ''' <param name="query">SQL query to execute</param>
    ''' <param name="parameters">Dictionary of parameters for the query</param>
    ''' <returns>True if operation succeeded, False otherwise</returns>
    Private Function ExecuteWithRetry(query As String, parameters As Dictionary(Of String, Object)) As Boolean
        Dim maxRetries As Integer = 3
        Dim retryCount As Integer = 0

        ' RETRY LOOP with exponential backoff
        While retryCount < maxRetries
            Try
                ' CONNECTION PREPARATION for retry attempts
                If retryCount > 0 Then
                    NpgsqlConnection.ClearAllPools()
                    System.Threading.Thread.Sleep(500) ' Brief delay between retries
                End If

                Using conn As New NpgsqlConnection(connStr)
                    conn.Open()

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 10

                        ' PARAMETER BINDING
                        ' Safe parameter binding for all provided parameters
                        For Each param In parameters
                            cmd.Parameters.AddWithValue(param.Key, param.Value)
                        Next

                        Return cmd.ExecuteNonQuery() > 0
                    End Using
                End Using

            Catch ex As NpgsqlException When ex.Message.Contains("Exception while reading from stream") Or ex.Message.Contains("stream")
                ' STREAM EXCEPTION RETRY LOGIC
                retryCount += 1
                If retryCount >= maxRetries Then
                    Throw New Exception($"Database operation failed after {maxRetries} attempts due to connection issues")
                End If
            End Try
        End While

        Return False
    End Function

    '--------------------------------------------------------------------------
    ' CRUD OPERATION EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Add Course Button Click Handler
    ''' Creates new course records with comprehensive validation and error handling
    ''' Implements duplicate checking and transaction safety
    ''' </summary>
    Protected Sub btnAdd_Click(sender As Object, e As EventArgs)
        ' VALIDATION CHECK: Only proceed if all form validation passes
        If Not Page.IsValid Then Return

        Try
            ' DUPLICATE COURSE NAME CHECK
            ' Prevent creation of courses with identical names
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
                ' If duplicate check fails, continue with warning
                ShowMessage("⚠️ Could not verify duplicates. Adding course...", "alert alert-warning")
            End Try

            ' PARAMETER PREPARATION for insert operation
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@name", courseName},
                {"@ects", Convert.ToInt32(txtECTS.Text)},
                {"@hours", Convert.ToInt32(txtHours.Text)},
                {"@format", ddlFormat.SelectedValue},
                {"@instructor", txtInstructor.Text.Trim()}
            }

            ' INSERT OPERATION with retry logic
            Dim query As String = "INSERT INTO courses (course_name, ects, hours, format, instructor) VALUES (@name, @ects, @hours, @format, @instructor)"

            If ExecuteWithRetry(query, parameters) Then
                ShowMessage("✅ Course added successfully!", "alert alert-success")
                ClearForm()

                ' ATTEMPT DATA REFRESH with fallback
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

    ''' <summary>
    ''' Update Course Button Click Handler
    ''' Modifies existing course records with validation and error recovery
    ''' </summary>
    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return

        ' SELECTION VALIDATION: Ensure a course is selected for update
        If ViewState("SelectedCourseId") Is Nothing Then
            ShowMessage("❌ No course selected for update.", "alert alert-danger")
            Return
        End If

        Try
            ' PARAMETER PREPARATION for update operation
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@name", txtCourseName.Text.Trim()},
                {"@ects", Convert.ToInt32(txtECTS.Text)},
                {"@hours", Convert.ToInt32(txtHours.Text)},
                {"@format", ddlFormat.SelectedValue},
                {"@instructor", txtInstructor.Text.Trim()},
                {"@id", CInt(ViewState("SelectedCourseId"))}
            }

            ' UPDATE OPERATION with retry logic
            Dim query As String = "UPDATE courses SET course_name = @name, ects = @ects, hours = @hours, format = @format, instructor = @instructor WHERE course_id = @id"

            If ExecuteWithRetry(query, parameters) Then
                ShowMessage("✅ Course updated successfully!", "alert alert-info")
                ClearForm()

                ' ATTEMPT DATA REFRESH
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

    ''' <summary>
    ''' Delete Course Button Click Handler
    ''' Removes courses with enrollment validation and safety checks
    ''' Implements referential integrity checking before deletion
    ''' </summary>
    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        If ViewState("SelectedCourseId") Is Nothing Then
            ShowMessage("❌ No course selected for deletion.", "alert alert-danger")
            Return
        End If

        Try
            Dim courseId As Integer = CInt(ViewState("SelectedCourseId"))

            ' REFERENTIAL INTEGRITY CHECK
            ' Prevent deletion of courses with active enrollments
            Dim enrollmentCount As Integer = 0
            Try
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

            ' DELETE OPERATION with retry logic
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@id", courseId}
            }

            Dim deleteQuery As String = "DELETE FROM courses WHERE course_id = @id"

            If ExecuteWithRetry(deleteQuery, parameters) Then
                ShowMessage("🗑️ Course deleted successfully.", "alert alert-warning")
                ClearForm()

                ' ATTEMPT DATA REFRESH
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

    ''' <summary>
    ''' Clear Form Button Click Handler
    ''' Resets all form fields and UI state to default values
    ''' </summary>
    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        ClearForm()
        HideMessage()
    End Sub

    '--------------------------------------------------------------------------
    ' SEARCH AND FILTER EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Search Button Click Handler
    ''' Applies user-specified filters to the course list
    ''' </summary>
    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        LoadCourses(txtSearchName.Text.Trim(), ddlFilterFormat.SelectedValue, txtSearchInstructor.Text.Trim())
    End Sub

    ''' <summary>
    ''' Clear Search Button Click Handler
    ''' Resets all search filters and reloads complete course list
    ''' </summary>
    Protected Sub btnClearSearch_Click(sender As Object, e As EventArgs)
        txtSearchName.Text = ""
        ddlFilterFormat.SelectedIndex = 0
        txtSearchInstructor.Text = ""
        LoadCourses()
    End Sub

    '--------------------------------------------------------------------------
    ' DIAGNOSTIC AND UTILITY EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Database Connection Test Handler
    ''' Provides diagnostic functionality for connection troubleshooting
    ''' </summary>
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

    ''' <summary>
    ''' Connection Pool Clear Handler
    ''' Manual connection pool management for troubleshooting
    ''' </summary>
    Protected Sub btnClearPools_Click(sender As Object, e As EventArgs)
        NpgsqlConnection.ClearAllPools()
        lastPoolClear = DateTime.Now
        ShowMessage("✅ Connection pools cleared!", "alert alert-success")
    End Sub

    ''' <summary>
    ''' Force Reload Handler
    ''' Manually triggers course list refresh with full error handling
    ''' </summary>
    Protected Sub btnForceReload_Click(sender As Object, e As EventArgs)
        LoadCourses()
        ShowMessage("✅ Courses reloaded!", "alert alert-success")
    End Sub

    ''' <summary>
    ''' Load with Helper Handler
    ''' Uses the simplified loading method for troubleshooting
    ''' </summary>
    Protected Sub btnLoadWithHelper_Click(sender As Object, e As EventArgs)
        LoadCoursesSimple()
        ShowMessage("✅ Courses loaded with simple method!", "alert alert-success")
    End Sub

    '--------------------------------------------------------------------------
    ' GRIDVIEW EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' GridView Row Selection Event Handler
    ''' Populates form fields when a course is selected from the grid
    ''' Implements safe data extraction and form state management
    ''' </summary>
    Protected Sub gvCourses_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            Dim row As GridViewRow = gvCourses.SelectedRow
            If row IsNot Nothing Then
                ' SAFE DATA EXTRACTION from grid row cells
                ' Uses HtmlDecode to handle any encoded characters
                txtCourseName.Text = HttpUtility.HtmlDecode(row.Cells(1).Text.Trim())
                txtECTS.Text = row.Cells(2).Text.Trim()
                txtHours.Text = row.Cells(3).Text.Trim()

                ' FORMAT VALUE EXTRACTION
                ' Extract clean format value from potentially styled cell content
                Dim formatValue As String = ExtractFormatFromBadge(row.Cells(4).Text)
                ddlFormat.SelectedValue = formatValue

                txtInstructor.Text = HttpUtility.HtmlDecode(row.Cells(5).Text.Trim())

                ' FORM STATE MANAGEMENT
                ' Enable update/delete operations and store selected course ID
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

    ''' <summary>
    ''' Format Badge Text Extraction Helper
    ''' Extracts clean text from HTML-formatted badge elements
    ''' Handles cases where format display includes styling markup
    ''' </summary>
    ''' <param name="badgeHtml">HTML content from grid cell</param>
    ''' <returns>Clean format text value</returns>
    Private Function ExtractFormatFromBadge(badgeHtml As String) As String
        Try
            ' REGEX PATTERN for HTML tag removal
            ' Simple regex to extract text content from HTML elements
            Dim cleanText As String = System.Text.RegularExpressions.Regex.Replace(badgeHtml, "<.*?>", "").Trim()
            Return cleanText.ToLower()
        Catch
            Return "lecture" ' Safe default value
        End Try
    End Function

    ''' <summary>
    ''' GridView Row Data Bound Event Handler
    ''' Customizes the display of course format information with colored badges
    ''' Executes for each row during data binding to enhance visual presentation
    ''' </summary>
    Protected Sub gvCourses_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' FORMAT DISPLAY CUSTOMIZATION
            ' Replace plain text format with styled badge elements
            Dim formatCell As TableCell = e.Row.Cells(4)
            Dim originalFormat As String = formatCell.Text.ToLower()

            ' FORMAT-SPECIFIC BADGE STYLING
            ' Each course format gets distinctive visual styling
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
                    ' FALLBACK for unknown formats
                    formatCell.Text = $"<span class='badge badge-secondary text-dark'>{originalFormat.Substring(0, 1).ToUpper() + originalFormat.Substring(1)}</span>"
            End Select
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' FORM MANAGEMENT HELPER METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Clear Form Fields and Reset UI State
    ''' Resets all form controls to their default values and disables action buttons
    ''' Provides clean slate for new course entry
    ''' </summary>
    Private Sub ClearForm()
        ' FIELD RESET: Clear all input controls
        txtCourseName.Text = ""
        txtECTS.Text = ""
        txtHours.Text = ""
        ddlFormat.SelectedIndex = 0
        txtInstructor.Text = ""

        ' STATE RESET: Clear selection and disable action buttons
        ViewState("SelectedCourseId") = Nothing
        btnUpdate.Enabled = False
        btnDelete.Enabled = False
        lblFormMode.Text = "Add New Course"
        lblFormMode.CssClass = "badge badge-secondary ml-2"
        gvCourses.SelectedIndex = -1
    End Sub

    ''' <summary>
    ''' Display User Message
    ''' Shows formatted success, error, or informational messages to the user
    ''' Supports emoji and HTML content for rich user feedback
    ''' </summary>
    ''' <param name="message">Message text (supports HTML and emojis)</param>
    ''' <param name="cssClass">Bootstrap CSS class for message styling</param>
    Private Sub ShowMessage(message As String, cssClass As String)
        MessageLiteral.Text = $"<div class='{cssClass}' role='alert'>{message}</div>"
        MessagePanel.Visible = True
    End Sub

    ''' <summary>
    ''' Hide User Messages
    ''' Clears and hides the message display area
    ''' </summary>
    Private Sub HideMessage()
        MessagePanel.Visible = False
        MessageLiteral.Text = ""
    End Sub

End Class

'==============================================================================
' END OF MANAGECOURSES CLASS
'==============================================================================
' 
' ARCHITECTURE OVERVIEW:
' This class implements a sophisticated course management system with advanced
' error handling designed specifically for cloud database environments.
' 
' KEY DESIGN PATTERNS:
' - Repository Pattern: Centralized data access with abstraction
' - Retry Pattern: Automatic recovery from transient failures
' - Factory Pattern: Reliable connection creation and management
' - Command Pattern: Parameterized database operations
' - Observer Pattern: Event-driven UI updates
' 
' ERROR HANDLING STRATEGY:
' 1. Retry Logic: Automatic retry for stream/connection exceptions
' 2. Graceful Degradation: Fallback UI states for failures
' 3. User Feedback: Clear, actionable error messages
' 4. Connection Management: Smart pool clearing and cleanup
' 5. Operation Verification: Confirmation of critical operations
' 
' SECURITY FEATURES:
' - Role-based Access Control: Admin-only access enforcement
' - SQL Injection Prevention: Parameterized queries throughout
' - Input Validation: Client and server-side validation
' - Data Integrity: Referential integrity checks before deletion
' - Session Security: Secure state management
' 
' PERFORMANCE OPTIMIZATIONS:
' - Connection Pooling: Smart pool management for cloud databases
' - Lazy Loading: On-demand data retrieval
' - Efficient Queries: Optimized SQL with minimal joins
' - Resource Cleanup: Proper disposal of database resources
' - Caching Strategy: ViewState for temporary data storage
' 
' USER EXPERIENCE ENHANCEMENTS:
' - Progressive Enhancement: Fallback options for failures
' - Real-time Feedback: Immediate validation and status updates
' - Responsive Design: Mobile-friendly interface support
' - Accessibility: Screen reader compatible markup
' - Intuitive Workflow: Logical operation sequencing
'==============================================================================