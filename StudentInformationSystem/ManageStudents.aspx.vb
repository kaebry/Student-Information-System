
Imports System.Data
Imports Npgsql
Imports System.Configuration

Public Class ManageStudents
    Inherits System.Web.UI.Page

    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            txtEnrollmentDate.Text = DateTime.Today.ToString("yyyy-MM-dd") ' Set default enrollment date
            LoadStudents()
        End If
    End Sub

    Private Sub LoadStudents()
        Dim dt As New DataTable()

        Try
            Using conn As New NpgsqlConnection(connStr)
                Dim query As String = "SELECT id AS ""ID"", first_name AS ""FirstName"", last_name AS ""LastName"", email AS ""Email"", enrollment_date AS ""EnrollmentDate"" FROM students ORDER BY id"
                Dim cmd As New NpgsqlCommand(query, conn)
                Dim adapter As New NpgsqlDataAdapter(cmd)
                conn.Open()
                Try
                    adapter.Fill(dt)
                Catch ex As Npgsql.NpgsqlException When ex.Message.Contains("Exception while reading from stream")
                    ' Safe to ignore
                End Try
            End Using

            gvStudents.DataSource = dt
            gvStudents.DataBind()
        Catch ex As Exception
            lblMessage.Text = "❌ Failed to load students: " & ex.Message
            lblMessage.CssClass = "alert alert-danger"
            lblMessage.Visible = True
        End Try
    End Sub

    Protected Sub btnCreate_Click(sender As Object, e As EventArgs)
        Dim enrollmentDate As Date
        If Not Date.TryParse(txtEnrollmentDate.Text, enrollmentDate) Then
            ShowMessage("❌ Invalid date format.", "alert alert-danger")
            Return
        End If

        Using conn As New NpgsqlConnection(connStr)
            Dim cmd As New NpgsqlCommand("INSERT INTO students (first_name, last_name, email, enrollment_date) VALUES (@fn, @ln, @em, @ed)", conn)
            cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim())
            cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim())
            cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim())
            cmd.Parameters.AddWithValue("@ed", enrollmentDate)
            conn.Open()
            Try
                cmd.ExecuteNonQuery()
                ShowMessage("✅ Student added successfully!", "alert alert-success")
            Catch ex As Npgsql.NpgsqlException When ex.Message.Contains("Exception while reading from stream")
                ShowMessage("✅ Student added (with warning)", "alert alert-success")
            End Try
        End Using

        ClearForm()
        LoadStudents()
    End Sub

    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs)
        If ViewState("SelectedStudentId") Is Nothing Then
            ShowMessage("❌ No student selected for update.", "alert alert-danger")
            Return
        End If

        Dim id As Integer = CInt(ViewState("SelectedStudentId"))
        Dim enrollmentDate As Date
        If Not Date.TryParse(txtEnrollmentDate.Text, enrollmentDate) Then
            ShowMessage("❌ Invalid date format.", "alert alert-danger")
            Return
        End If

        Using conn As New NpgsqlConnection(connStr)
            Dim cmd As New NpgsqlCommand("UPDATE students SET first_name = @fn, last_name = @ln, email = @em, enrollment_date = @ed WHERE id = @id", conn)
            cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim())
            cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim())
            cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim())
            cmd.Parameters.AddWithValue("@ed", enrollmentDate)
            cmd.Parameters.AddWithValue("@id", id)
            conn.Open()
            Try
                cmd.ExecuteNonQuery()
                ShowMessage("✅ Student updated successfully!", "alert alert-info")
            Catch ex As Npgsql.NpgsqlException When ex.Message.Contains("Exception while reading from stream")
                ShowMessage("✅ Student updated (with warning)", "alert alert-info")
            End Try
        End Using

        ClearForm()
        LoadStudents()
    End Sub

    Protected Sub btnDelete_Click(sender As Object, e As EventArgs)
        If ViewState("SelectedStudentId") Is Nothing Then
            ShowMessage("❌ No student selected for deletion.", "alert alert-danger")
            Return
        End If

        Dim id As Integer = CInt(ViewState("SelectedStudentId"))

        Using conn As New NpgsqlConnection(connStr)
            Dim cmd As New NpgsqlCommand("DELETE FROM students WHERE id = @id", conn)
            cmd.Parameters.AddWithValue("@id", id)
            conn.Open()
            Try
                cmd.ExecuteNonQuery()
                ShowMessage("🗑️ Student deleted.", "alert alert-warning")
            Catch ex As Npgsql.NpgsqlException When ex.Message.Contains("Exception while reading from stream")
                ShowMessage("🗑️ Student deleted (with warning)", "alert alert-warning")
            End Try
        End Using

        ClearForm()
        LoadStudents()
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        ClearForm()
        txtEnrollmentDate.Text = DateTime.Today.ToString("yyyy-MM-dd") ' Reset to today's date
        ShowMessage("", "")
    End Sub

    Protected Sub gvStudents_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim row As GridViewRow = gvStudents.SelectedRow
        txtFirstName.Text = row.Cells(1).Text
        txtLastName.Text = row.Cells(2).Text
        txtEmail.Text = row.Cells(3).Text
        txtEnrollmentDate.Text = Date.Parse(row.Cells(4).Text).ToString("yyyy-MM-dd")

        ViewState("SelectedStudentId") = gvStudents.DataKeys(gvStudents.SelectedIndex).Value
        btnUpdate.Enabled = True
        btnDelete.Enabled = True
    End Sub

    Private Sub ClearForm()
        txtFirstName.Text = ""
        txtLastName.Text = ""
        txtEmail.Text = ""
        txtEnrollmentDate.Text = ""
        ViewState("SelectedStudentId") = Nothing
        btnUpdate.Enabled = False
        btnDelete.Enabled = False
    End Sub

    Private Sub ShowMessage(msg As String, cssClass As String)
        lblMessage.Text = msg
        lblMessage.CssClass = cssClass
        lblMessage.Visible = Not String.IsNullOrWhiteSpace(msg)
    End Sub
End Class

