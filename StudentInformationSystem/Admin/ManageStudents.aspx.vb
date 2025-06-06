'==============================================================================
' MANAGE STUDENTS - CODE-BEHIND CLASS
'==============================================================================
' Purpose: Handles server-side logic for student management operations
' Features: CRUD operations with robust error handling and connection management
' 
' Key Technical Features:
' - Smart error handling for connection timeouts and stream exceptions
' - Operation verification when connections drop during database operations
' - Validation groups to separate form validation from delete operations
' - Comprehensive connection management with pool clearing
' - User-friendly feedback messages with emojis for better UX
'==============================================================================

Imports System.Data
Imports Npgsql
Imports System.Configuration

Public Class ManageStudents
    Inherits System.Web.UI.Page

    '--------------------------------------------------------------------------
    ' CLASS VARIABLES
    '--------------------------------------------------------------------------

    ' Database connection string - retrieved from Web.config
    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    '--------------------------------------------------------------------------
    ' PAGE LIFECYCLE EVENTS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' Page Load Event - Initializes the page and loads student data
    ''' </summary>
    ''' <param name="sender">Event sender</param>
    ''' <param name="e">Event arguments</param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' SECURITY CHECK: Ensure only admins can access this page
        ' Redirects unauthorized users to prevent security breaches
        If Session("UserRole")?.ToString() <> "admin" Then
            Response.Redirect("~/Default.aspx?error=access_denied")
            Return
        End If

        ' FIRST LOAD INITIALIZATION
        ' Only execute on initial page load, not on postbacks
        If Not IsPostBack Then
            ' Set default enrollment date to today for new students
            txtEnrollmentDate.Text = DateTime.Today.ToString("yyyy-MM-dd")

            ' Load the students grid immediately
            LoadStudents()
        End If
    End Sub

    '--------------------------------------------------------------------------
    ' CORE CRUD OPERATIONS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' ADD STUDENT OPERATION
    ''' Handles creating new students with full validation and error recovery
    ''' </summary>
    ''' <param name="sender">Button that triggered the event</param>
    ''' <param name="e">Event arguments</param>
    Protected Sub btnCreate_Click(sender As Object, e As EventArgs)
        ' VALIDATION STEP: Only validate for Add operations using ValidationGroup
        Page.Validate("StudentForm")
        If Not Page.IsValid Then Return

        Try
            ' DATE VALIDATION: Ensure enrollment date is valid
            Dim enrollmentDate As Date
            If Not Date.TryParse(txtEnrollmentDate.Text, enrollmentDate) Then
                ShowMessage("❌ Invalid date format.", "alert alert-danger")
                Return
            End If

            ' DUPLICATE CHECK: Prevent duplicate email addresses
            If EmailExists(txtEmail.Text.Trim()) Then
                ShowMessage("❌ A student with this email already exists.", "alert alert-danger")
                Return
            End If

            Dim addSuccessful As Boolean = False

            ' ROBUST DATABASE OPERATION WITH STREAM EXCEPTION HANDLING
            ' This handles the common "Exception while reading from stream" issue
            Try
                Using conn As NpgsqlConnection = GetConnection()
                    Dim query As String = "INSERT INTO students (first_name, last_name, email, enrollment_date) VALUES (@fn, @ln, @em, @ed)"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 30 ' Extended timeout for reliability
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
                ' CONNECTION ISSUE RECOVERY
                ' When connection drops during operation, verify if it actually succeeded
                ShowMessage("🔄 Connection issue detected. Verifying addition...", "alert alert-warning")
                System.Threading.Thread.Sleep(1000) ' Give database time to process

                ' VERIFICATION: Check if the student was actually added despite connection error
                addSuccessful = EmailExists(txtEmail.Text.Trim())
            End Try

            ' SUCCESS/FAILURE HANDLING
            If addSuccessful Then
                ShowMessage("✅ Student added successfully!", "alert alert-success")
                ClearForm()
                LoadStudents()
            Else
                ShowMessage("❌ Failed to add student.", "alert alert-danger")
            End If

        Catch ex As Exception
            ' GENERAL ERROR HANDLING for unexpected issues
            ShowMessage($"❌ Error adding student: {ex.Message}", "alert alert-danger")
        End Try
    End Sub

    ''' <summary>
    ''' UPDATE STUDENT OPERATION
    ''' Updates existing student with verification for connection issues
    ''' </summary>
    ''' <param name="sender">Button that triggered the event</param>
    ''' <param name="e">Event arguments</param>
    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs)
        ' VALIDATION: Only validate for Update operations
        Page.Validate("StudentForm")
        If Not Page.IsValid Then Return

        ' SELECTION CHECK: Ensure a student is selected for update
        If ViewState("SelectedStudentId") Is Nothing Then
            ShowMessage("❌ No student selected for update.", "alert alert-danger")
            Return
        End If

        Try
            ' DATE VALIDATION
            Dim enrollmentDate As Date
            If Not Date.TryParse(txtEnrollmentDate.Text, enrollmentDate) Then
                ShowMessage("❌ Invalid date format.", "alert alert-danger")
                Return
            End If

            Dim studentId As Long = Convert.ToInt64(ViewState("SelectedStudentId"))
            Dim updateSuccessful As Boolean = False

            ' ROBUST UPDATE OPERATION WITH STREAM EXCEPTION HANDLING
            Try
                Using conn As NpgsqlConnection = GetConnection()
                    Dim query As String = "UPDATE students SET first_name = @fn, last_name = @ln, email = @em, enrollment_date = @ed WHERE id = @id"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 30 ' Extended timeout for reliability
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
                ' CONNECTION ISSUE RECOVERY FOR UPDATES
                ' Verify if the update actually happened despite connection error
                ShowMessage("🔄 Connection issue detected. Verifying update...", "alert alert-warning")
                System.Threading.Thread.Sleep(1000) ' Wait for database processing

                ' VERIFICATION: Check if update succeeded by comparing database values
                updateSuccessful = VerifyStudentUpdate(studentId, txtFirstName.Text.Trim(), txtLastName.Text.Trim(), txtEmail.Text.Trim().ToLower(), enrollmentDate)
            End Try

            ' SUCCESS/FAILURE HANDLING
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

    ''' <summary>
    ''' DELETE STUDENT OPERATION
    ''' Removes student with enrollment checks and no form validation
    ''' </summary>
    ''' <param name="sender">Button that triggered the event</param>
    ''' <param name="e">Event arguments</param>
    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        ' NO VALIDATION for delete operations - this allows deletion of records with invalid data
        ' This is crucial for data cleanup scenarios

        If ViewState("SelectedStudentId") Is Nothing Then
            ShowMessage("❌ No student selected for deletion.", "alert alert-danger")
            Return
        End If

        Try
            Dim studentId As Long = Convert.ToInt64(ViewState("SelectedStudentId"))
            Dim deleteSuccessful As Boolean = False

            ' REFERENTIAL INTEGRITY CHECK
            ' Prevent deletion of students who have enrollments
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

                ' ROBUST DELETE OPERATION
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
                    ' CONNECTION ISSUE RECOVERY FOR DELETES
                    ShowMessage("🔄 Connection issue detected. Verifying deletion...", "alert alert-warning")
                    System.Threading.Thread.Sleep(1000)

                    ' VERIFICATION: Check if the student was actually deleted
                    deleteSuccessful = Not StudentExists(studentId)
                End Try
            End Using

            ' SUCCESS/FAILURE HANDLING
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

    ''' <summary>
    ''' CLEAR FORM OPERATION
    ''' Resets all form fields and states
    ''' </summary>
    ''' <param name="sender">Button that triggered the event</param>
    ''' <param name="e">Event arguments</param>
    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        ' NO VALIDATION for clear operations - allows clearing invalid data
        ClearForm()
        ShowMessage("✅ Form cleared.", "alert alert-success")
    End Sub

    '--------------------------------------------------------------------------
    ' GRIDVIEW EVENT HANDLERS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' GRIDVIEW ROW SELECTION EVENT
    ''' Populates form fields when a student is selected from the grid
    ''' </summary>
    ''' <param name="sender">GridView that triggered the event</param>
    ''' <param name="e">Event arguments</param>
    Protected Sub gvStudents_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            Dim row As GridViewRow = gvStudents.SelectedRow
            If row IsNot Nothing Then
                ' CLEAR VALIDATION ERRORS when selecting a new row
                ' This prevents lingering validation messages from previous operations
                Try
                    If Page.Validators IsNot Nothing Then
                        For Each validator As BaseValidator In Page.Validators
                            validator.IsValid = True
                        Next
                    End If
                Catch
                    ' Ignore validation clearing errors
                End Try

                ' POPULATE FORM FIELDS from selected row
                ' Use HtmlDecode to handle any encoded characters properly
                txtFirstName.Text = HttpUtility.HtmlDecode(row.Cells(1).Text.Trim())
                txtLastName.Text = HttpUtility.HtmlDecode(row.Cells(2).Text.Trim())
                txtEmail.Text = HttpUtility.HtmlDecode(row.Cells(3).Text.Trim())

                ' DATE PARSING with error handling
                Dim enrollmentDate As DateTime
                If DateTime.TryParse(row.Cells(4).Text, enrollmentDate) Then
                    txtEnrollmentDate.Text = enrollmentDate.ToString("yyyy-MM-dd")
                End If

                ' ENABLE UPDATE/DELETE BUTTONS and store selected ID
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

    '--------------------------------------------------------------------------
    ' DATA LOADING METHODS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' LOAD STUDENTS DATA
    ''' Retrieves all students from database with retry logic for connection issues
    ''' </summary>
    Private Sub LoadStudents()
        Try
            ShowMessage("🔄 Loading students...", "alert alert-info")

            ' RETRY LOGIC for connection reliability
            Dim retryCount As Integer = 0
            Dim maxRetries As Integer = 2
            Dim studentsLoaded As Boolean = False

            While retryCount <= maxRetries And Not studentsLoaded
                Try
                    Using conn As NpgsqlConnection = GetConnection()
                        ' ORDER BY for consistent display
                        Dim query As String = "SELECT id, first_name, last_name, email, enrollment_date FROM students ORDER BY last_name, first_name"

                        Using cmd As New NpgsqlCommand(query, conn)
                            cmd.CommandTimeout = 20

                            Using adapter As New NpgsqlDataAdapter(cmd)
                                Dim dt As New DataTable()
                                adapter.Fill(dt)

                                ' BIND DATA to GridView
                                gvStudents.DataSource = dt
                                gvStudents.DataBind()

                                ' USER FEEDBACK based on results
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
                    ' RETRY LOGIC with progressive delays
                    retryCount += 1
                    ShowMessage($"🔄 Connection issue (attempt {retryCount + 1}). Retrying...", "alert alert-warning")
                    System.Threading.Thread.Sleep(1000 * retryCount) ' Progressive delay: 1s, 2s, etc.
                    Continue While
                End Try
            End While

            ' FINAL FAILURE CHECK
            If Not studentsLoaded Then
                Throw New Exception("Failed to load students after multiple attempts")
            End If

        Catch ex As Exception
            ShowMessage($"❌ Error loading students: {ex.Message}", "alert alert-danger")

            ' FALLBACK: Show empty grid on complete failure
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

    '--------------------------------------------------------------------------
    ' HELPER METHODS - CONNECTION MANAGEMENT
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' GET DATABASE CONNECTION
    ''' Returns a tested, working connection with pool management
    ''' </summary>
    ''' <returns>Open NpgsqlConnection object</returns>
    Private Function GetConnection() As NpgsqlConnection
        ' CONNECTION POOL CLEARING for reliability
        ' This helps prevent stale connection issues with cloud databases
        Try
            NpgsqlConnection.ClearAllPools()
            System.Threading.Thread.Sleep(200) ' Give pools time to clear
        Catch
            ' Ignore pool clearing errors - they're not critical
        End Try

        ' CREATE AND TEST CONNECTION
        ' Use the original connection string from Web.config (already optimized)
        Dim conn As New NpgsqlConnection(connStr)
        Try
            conn.Open()

            ' CONNECTION VERIFICATION with quick test query
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn
        Catch ex As Exception
            ' CLEANUP on connection failure
            Try
                conn?.Dispose()
            Catch
            End Try
            Throw New Exception($"Failed to connect to database: {ex.Message}")
        End Try
    End Function

    '--------------------------------------------------------------------------
    ' HELPER METHODS - VERIFICATION FUNCTIONS
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' VERIFY STUDENT UPDATE
    ''' Checks if an update operation actually succeeded by comparing database values
    ''' Used when connection drops during update operations
    ''' </summary>
    ''' <param name="studentId">ID of student to verify</param>
    ''' <param name="firstName">Expected first name</param>
    ''' <param name="lastName">Expected last name</param>
    ''' <param name="email">Expected email</param>
    ''' <param name="enrollmentDate">Expected enrollment date</param>
    ''' <returns>True if database matches expected values</returns>
    Private Function VerifyStudentUpdate(studentId As Long, firstName As String, lastName As String, email As String, enrollmentDate As Date) As Boolean
        Try
            ' Give database a moment to process (important for cloud databases)
            System.Threading.Thread.Sleep(500)

            Using conn As NpgsqlConnection = GetConnection()
                Using cmd As New NpgsqlCommand("SELECT first_name, last_name, email, enrollment_date FROM students WHERE id = @id", conn)
                    cmd.CommandTimeout = 10
                    cmd.Parameters.AddWithValue("@id", studentId)

                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' COMPARE DATABASE VALUES with expected values
                            Dim dbFirstName As String = reader("first_name").ToString()
                            Dim dbLastName As String = reader("last_name").ToString()
                            Dim dbEmail As String = reader("email").ToString()
                            Dim dbEnrollmentDate As Date = Convert.ToDateTime(reader("enrollment_date"))

                            ' CASE-INSENSITIVE COMPARISON for string fields
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

    ''' <summary>
    ''' CHECK EMAIL EXISTS
    ''' Verifies if an email address is already in use
    ''' </summary>
    ''' <param name="email">Email address to check</param>
    ''' <returns>True if email exists in database</returns>
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
            ' On error, assume email doesn't exist (safe default)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' CHECK STUDENT EXISTS
    ''' Verifies if a student ID exists in the database
    ''' </summary>
    ''' <param name="studentId">Student ID to check</param>
    ''' <returns>True if student exists</returns>
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
            ' If we can't check, assume it still exists (safe for delete verification)
            Return True
        End Try
    End Function

    '--------------------------------------------------------------------------
    ' HELPER METHODS - UI MANAGEMENT
    '--------------------------------------------------------------------------

    ''' <summary>
    ''' CLEAR FORM FIELDS
    ''' Resets all form controls to their default state
    ''' </summary>
    Private Sub ClearForm()
        Try
            ' RESET ALL FORM FIELDS
            txtFirstName.Text = ""
            txtLastName.Text = ""
            txtEmail.Text = ""
            txtEnrollmentDate.Text = DateTime.Today.ToString("yyyy-MM-dd")

            ' RESET FORM STATE
            ViewState("SelectedStudentId") = Nothing
            btnUpdate.Enabled = False
            btnDelete.Enabled = False
            gvStudents.SelectedIndex = -1

            ' CLEAR VALIDATION ERRORS safely
            Try
                If Page.Validators IsNot Nothing Then
                    For Each validator As BaseValidator In Page.Validators
                        validator.IsValid = True
                    Next
                End If
            Catch
                ' Ignore validation clearing errors - not critical
            End Try
        Catch ex As Exception
            ' If clearing fails, just log it but don't break the flow
            ' In a production app, you might want to log this
        End Try
    End Sub

    ''' <summary>
    ''' SHOW MESSAGE TO USER
    ''' Displays success, error, or informational messages with appropriate styling
    ''' </summary>
    ''' <param name="message">Message text (supports HTML and emojis)</param>
    ''' <param name="cssClass">Bootstrap CSS class for styling</param>
    Private Sub ShowMessage(message As String, cssClass As String)
        lblMessage.Text = message
        lblMessage.CssClass = cssClass
        lblMessage.Visible = True
    End Sub

End Class

'==============================================================================
' END OF MANAGESTUDENTS CLASS
'==============================================================================
' 
' DESIGN PATTERNS USED:
' - Repository Pattern: Database operations are centralized
' - Error Recovery Pattern: Operations verify success after connection issues
' - Separation of Concerns: UI, validation, and data access are separated
' - Defensive Programming: Extensive error checking and fallback mechanisms
' 
' SECURITY FEATURES:
' - Role-based access control (admin only)
' - SQL injection prevention through parameterized queries
' - Input validation and sanitization
' - Email uniqueness enforcement
' 
' PERFORMANCE OPTIMIZATIONS:
' - Connection pooling management
' - Retry logic for transient failures
' - Efficient data binding
' - Resource cleanup (using statements)
' 
' USER EXPERIENCE FEATURES:
' - Emoji-enhanced status messages
' - Real-time form validation
' - Progressive error recovery
' - Intuitive form state management
'==============================================================================
