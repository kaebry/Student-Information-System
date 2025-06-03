Imports Microsoft.AspNet.Identity
Imports System.Web.Security

Public Class SiteMaster
    Inherits MasterPage
    Private Const AntiXsrfTokenKey As String = "__AntiXsrfToken"
    Private Const AntiXsrfUserNameKey As String = "__AntiXsrfUserName"
    Private _antiXsrfTokenValue As String

    Protected Sub Page_Init(sender As Object, e As EventArgs)
        ' The code below helps to protect against XSRF attacks
        Dim requestCookie = Request.Cookies(AntiXsrfTokenKey)
        Dim requestCookieGuidValue As Guid
        If requestCookie IsNot Nothing AndAlso Guid.TryParse(requestCookie.Value, requestCookieGuidValue) Then
            ' Use the Anti-XSRF token from the cookie
            _antiXsrfTokenValue = requestCookie.Value
            Page.ViewStateUserKey = _antiXsrfTokenValue
        Else
            ' Generate a new Anti-XSRF token and save to the cookie
            _antiXsrfTokenValue = Guid.NewGuid().ToString("N")
            Page.ViewStateUserKey = _antiXsrfTokenValue

            Dim responseCookie = New HttpCookie(AntiXsrfTokenKey) With {
                 .HttpOnly = True,
                 .Value = _antiXsrfTokenValue
            }
            If FormsAuthentication.RequireSSL AndAlso Request.IsSecureConnection Then
                responseCookie.Secure = True
            End If
            Response.Cookies.[Set](responseCookie)
        End If

        AddHandler Page.PreLoad, AddressOf master_Page_PreLoad
    End Sub

    Protected Sub master_Page_PreLoad(sender As Object, e As EventArgs)
        If Not IsPostBack Then
            ' Set Anti-XSRF token
            ViewState(AntiXsrfTokenKey) = Page.ViewStateUserKey
            ViewState(AntiXsrfUserNameKey) = If(Context.User.Identity.Name, [String].Empty)
        Else
            ' Validate the Anti-XSRF token
            If DirectCast(ViewState(AntiXsrfTokenKey), String) <> _antiXsrfTokenValue OrElse DirectCast(ViewState(AntiXsrfUserNameKey), String) <> (If(Context.User.Identity.Name, [String].Empty)) Then
                Throw New InvalidOperationException("Validation of Anti-XSRF token failed.")
            End If
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' You can add any page load logic here if needed
    End Sub

    ' Logout functionality
    Protected Sub Logout_Click(sender As Object, e As EventArgs)
        Try
            ' Clear all session variables
            Session.Clear()
            Session.Abandon()

            ' Clear authentication cookie if it exists
            If Request.Cookies(FormsAuthentication.FormsCookieName) IsNot Nothing Then
                Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, "")
                cookie.Expires = DateTime.Now.AddDays(-1)
                cookie.HttpOnly = True
                cookie.Secure = Request.IsSecureConnection
                Response.Cookies.Add(cookie)
            End If

            ' Clear any other authentication cookies
            For Each cookieName As String In Request.Cookies.AllKeys
                If cookieName.Contains("Auth") OrElse cookieName.Contains("Login") Then
                    Dim clearCookie As New HttpCookie(cookieName, "")
                    clearCookie.Expires = DateTime.Now.AddDays(-1)
                    Response.Cookies.Add(clearCookie)
                End If
            Next

            ' Redirect to login page with logout confirmation
            Response.Redirect("~/Account/Login.aspx?loggedout=true", True)

        Catch ex As Exception
            ' If something goes wrong, still try to redirect
            Response.Redirect("~/Account/Login.aspx")
        End Try
    End Sub

    ' Properties to check login state and get user info
    Protected ReadOnly Property IsUserLoggedIn As Boolean
        Get
            Return Session("UserEmail") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Session("UserEmail").ToString())
        End Get
    End Property

    Protected ReadOnly Property UserFullName As String
        Get
            If IsUserLoggedIn Then
                Dim fullName As String = If(Session("UserFullName")?.ToString(), "")
                If String.IsNullOrEmpty(fullName) Then
                    ' Fallback to constructing name from first/last name
                    Dim firstName As String = If(Session("UserFirstName")?.ToString(), "")
                    Dim lastName As String = If(Session("UserLastName")?.ToString(), "")
                    fullName = $"{firstName} {lastName}".Trim()

                    If String.IsNullOrEmpty(fullName) Then
                        ' Final fallback to email
                        Return If(Session("UserEmail")?.ToString(), "User")
                    End If
                End If
                Return fullName
            Else
                Return "Guest"
            End If
        End Get
    End Property

    Protected ReadOnly Property UserRole As String
        Get
            If IsUserLoggedIn Then
                Return If(Session("UserRole")?.ToString(), "user")
            Else
                Return ""
            End If
        End Get
    End Property

    Protected ReadOnly Property UserEmail As String
        Get
            If IsUserLoggedIn Then
                Return If(Session("UserEmail")?.ToString(), "")
            Else
                Return ""
            End If
        End Get
    End Property

    ' For backward compatibility with existing code
    Protected Sub Unnamed_LoggingOut(sender As Object, e As LoginCancelEventArgs)
        ' This method is kept for compatibility but we now use Logout_Click
        Logout_Click(sender, New EventArgs())
    End Sub

End Class