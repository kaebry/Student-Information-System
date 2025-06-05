' Créez ce fichier dans App_Code/DatabaseHelper.vb
' Cela centralisera la gestion des connexions pour toute l'application

Imports System.Configuration
Imports System.Data
Imports Npgsql

Public Class DatabaseHelper

    Private Shared ReadOnly _connectionString As String = ConfigurationManager.ConnectionStrings("DefaultConnection").ConnectionString
    Private Shared _lastPoolClear As DateTime = DateTime.MinValue
    Private Shared ReadOnly _poolClearInterval As TimeSpan = TimeSpan.FromMinutes(5)

    ''' <summary>
    ''' Obtient une connexion robuste avec gestion automatique des pools stales
    ''' </summary>
    Public Shared Function GetConnection() As NpgsqlConnection
        ' Auto-clear pools périodiquement pour éviter les connexions stales
        If DateTime.Now.Subtract(_lastPoolClear) > _poolClearInterval Then
            Try
                NpgsqlConnection.ClearAllPools()
                _lastPoolClear = DateTime.Now
                System.Threading.Thread.Sleep(200)
            Catch
                ' Ignore les erreurs de nettoyage
            End Try
        End If

        Dim enhancedConnStr As String = _connectionString

        ' Assurer les paramètres optimaux pour Supabase
        If Not enhancedConnStr.Contains("Connection Lifetime") Then
            enhancedConnStr &= ";Connection Lifetime=30;Maximum Pool Size=10;Minimum Pool Size=1;Connection Idle Lifetime=30;"
        End If

        Dim conn As New NpgsqlConnection(enhancedConnStr)

        Try
            conn.Open()

            ' Toujours tester la connexion avant de la retourner
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn

        Catch
            ' Si la connexion échoue, nettoyer et réessayer avec un pool frais
            Try
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If
                conn.Dispose()
            Catch
            End Try

            ' Forcer le nettoyage des pools et créer une connexion fraîche
            NpgsqlConnection.ClearAllPools()
            _lastPoolClear = DateTime.Now
            System.Threading.Thread.Sleep(300)

            conn = New NpgsqlConnection(enhancedConnStr)
            conn.Open()

            ' Tester la connexion fraîche
            Using testCmd As New NpgsqlCommand("SELECT 1", conn)
                testCmd.CommandTimeout = 5
                testCmd.ExecuteScalar()
            End Using

            Return conn
        End Try
    End Function

    ''' <summary>
    ''' Exécute une requête avec gestion automatique des connexions et retry
    ''' </summary>
    Public Shared Function ExecuteScalar(query As String, ParamArray parameters As NpgsqlParameter()) As Object
        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 3

        While retryCount < maxRetries
            Try
                Using conn As NpgsqlConnection = GetConnection()
                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 30

                        If parameters IsNot Nothing Then
                            cmd.Parameters.AddRange(parameters)
                        End If

                        Return cmd.ExecuteScalar()
                    End Using
                End Using

            Catch ex As Exception
                retryCount += 1
                If retryCount >= maxRetries Then
                    Throw New Exception($"Database operation failed after {maxRetries} attempts: {ex.Message}")
                Else
                    System.Threading.Thread.Sleep(500 * retryCount)
                    NpgsqlConnection.ClearAllPools()
                    _lastPoolClear = DateTime.Now
                End If
            End Try
        End While

        Return Nothing
    End Function

    ''' <summary>
    ''' Exécute une requête non-query avec gestion automatique des connexions et retry
    ''' </summary>
    Public Shared Function ExecuteNonQuery(query As String, ParamArray parameters As NpgsqlParameter()) As Integer
        Dim retryCount As Integer = 0
        Dim maxRetries As Integer = 3

        While retryCount < maxRetries
            Try
                Using conn As NpgsqlConnection = GetConnection()
                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.CommandTimeout = 30

                        If parameters IsNot Nothing Then
                            cmd.Parameters.AddRange(parameters)
                        End If

                        Return cmd.ExecuteNonQuery()
                    End Using
                End Using

            Catch ex As Exception
                retryCount += 1
                If retryCount >= maxRetries Then
                    Throw New Exception($"Database operation failed after {maxRetries} attempts: {ex.Message}")
                Else
                    System.Threading.Thread.Sleep(500 * retryCount)
                    NpgsqlConnection.ClearAllPools()
                    _lastPoolClear = DateTime.Now
                End If
            End Try
        End While

        Return 0
    End Function

    ''' <summary>
    ''' Force le nettoyage des pools de connexion
    ''' </summary>
    Public Shared Sub ClearConnectionPools()
        Try
            NpgsqlConnection.ClearAllPools()
            _lastPoolClear = DateTime.Now
            System.Threading.Thread.Sleep(500)
        Catch
            ' Ignore les erreurs
        End Try
    End Sub

End Class

' Exemple d'utilisation dans vos pages :
' Dim count As Integer = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM courses"))
' DatabaseHelper.ExecuteNonQuery("INSERT INTO courses (name) VALUES (@name)", New NpgsqlParameter("@name", "Test Course"))