Imports System
Imports System.Configuration
Imports System.Data
Imports Npgsql
Imports BCrypt.Net

Partial Class Register
    Inherits System.Web.UI.Page

    Protected Sub CreateUser_Click(sender As Object, e As EventArgs)
        Dim email As String = email.Text.Trim()
        Dim password As String = password.Text.Trim()
        Dim confirmPassword As String = confirmPassword.Text.Trim()
        Dim firstName As String = firstName.Text.Trim()
        Dim lastName As String = lastName.Text.Trim()


        Dim role As String = UserRole.SelectedValue

        ' Basic validation
        If password <> confirmPassword Then
            ErrorMessage.Text = "❌ Passwords do not match."
            Return
        End If

        ' Hash the password using BCrypt
        Dim hashedPassword As String = BCrypt.Net.BCrypt.HashPassword(password)

        Dim connStr As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString

        Try
            Using conn As New NpgsqlConnection(connStr)
                conn.Open()

                Dim userId As Integer

                ' Insert into students or teachers
                If role = "student" Then
                    Using studentCmd As New NpgsqlCommand("INSERT INTO students (first_name, last_name, email, enrollment_date) VALUES (@fn, @ln, @em, CURRENT_DATE) RETURNING id", conn)
                        studentCmd.Parameters.AddWithValue("@fn", firstName)
                        studentCmd.Parameters.AddWithValue("@ln", lastName)
                        studentCmd.Parameters.AddWithValue("@em", email)
                        userId = Convert.ToInt32(studentCmd.ExecuteScalar())
                    End Using
                ElseIf role = "teacher" Then
                    Using teacherCmd As New NpgsqlCommand("INSERT INTO teachers (first_name, last_name, email) VALUES (@fn, @ln, @em) RETURNING id", conn)
                        teacherCmd.Parameters.AddWithValue("@fn", firstName)
                        teacherCmd.Parameters.AddWithValue("@ln", lastName)
                        teacherCmd.Parameters.AddWithValue("@em", email)
                        userId = Convert.ToInt32(teacherCmd.ExecuteScalar())
                    End Using
                End If

                ' Insert into users table
                Using userCmd As New NpgsqlCommand("INSERT INTO users (email, password, role, related_id) VALUES (@em, @pw, @rl, @rid)", conn)
                    userCmd.Parameters.AddWithValue("@em", email)
                    userCmd.Parameters.AddWithValue("@pw", hashedPassword)
                    userCmd.Parameters.AddWithValue("@rl", role)
                    userCmd.Parameters.AddWithValue("@rid", userId)
                    userCmd.ExecuteNonQuery()
                End Using
            End Using

            Response.Redirect("~/Login.aspx")

        Catch ex As Exception
            ErrorMessage.Text = "❌ Registration failed: " & ex.Message
        End Try
    End Sub

End Class

