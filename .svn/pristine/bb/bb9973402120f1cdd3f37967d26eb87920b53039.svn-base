Imports LibraryDAL



Partial Public Class EGGCheckSubsKw
    Inherits System.Web.UI.Page

    Public UserName As String = ConfigurationManager.AppSettings("EggCheckSub_UserName")
    Public Password As String = ConfigurationManager.AppSettings("EggCheckSub_Password")
    Public TelcoID As String = ConfigurationManager.AppSettings("TelcoID")

    Private Shared ReadOnly loggerSubs As log4net.ILog = log4net.LogManager.GetLogger("CheckSubsKeyword")
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim RecvUserName As String = RequestData("username")
        Dim RecvPassword As String = RequestData("password")
        Dim MSISDN As String = RequestData("number")

        If String.IsNullOrEmpty(UserName) Or String.IsNullOrEmpty(Password) Or String.IsNullOrEmpty(MSISDN) Then
            Response.Write("InvalidParameter")
            Exit Sub
        End If

        If RecvUserName <> UserName And RecvPassword <> Password Then
            Response.Write("InvalidParameter")
            Exit Sub
        End If

        Try

            Dim UserCls As New UserCRUD
            Dim KeywordList As String = ""
            Dim dt As New DataSet
            dt = UserCls.GetUserSubsKeyword(MSISDN, TelcoID)

            If dt.Tables(0).Rows.Count > 0 Then
                For Each r As DataRow In dt.Tables(0).Rows
                    KeywordList = KeywordList & "," & r.Item("Keyword").ToString
                Next

                KeywordList = KeywordList.Remove(0, 1)
            Else
                KeywordList = "NoKeywordFound"
            End If
           
            Response.Write(KeywordList)
            loggerSubs.Info("[INFO] MSISDN=" & MSISDN & ";KeywordList=" & KeywordList)

        Catch ex As Exception
            logger.Fatal("[FATAL-CHECKKW]", ex)
        End Try

    End Sub

    Public Function RequestData(ByVal strName As String) As String
        Dim strReturn As String = ""
        Try

            strReturn = Request.QueryString(strName)
            If strReturn = "" Then
                strReturn = System.Web.HttpUtility.UrlDecode(Request.Form(strName))
            End If
            If strReturn <> "" Then
                strReturn = strReturn.Trim
                Return strReturn
            Else
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try
    End Function

End Class