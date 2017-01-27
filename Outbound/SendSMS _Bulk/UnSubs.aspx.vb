Imports System.Messaging
Imports LibraryDAL
Imports LibraryDAL.General
Imports System.Data.SqlClient

Partial Public Class UnSubs
    Inherits System.Web.UI.Page

    Private MSISDN, keyid, telcoid, userid, keyword As String
    Dim dt As DataSet
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        MSISDN = RequestData("msisdn")
        keyid = RequestData("keyid")
        keyword = RequestData("keyword")
        telcoid = RequestData("telcoid")
        userid = RequestData("userid")

        Try
            Dim UserUpdateResult, logInserted As Integer
            Dim UserUpdateResultStr, logInsertedStr As String

            UserUpdateResultStr = "SUCCESS"
            logInsertedStr = "SUCCESS"

            UserUpdateResult = UpdateInActiveUserSubs(MSISDN, keyid)
            logInserted = logHistory(userid, keyid, telcoid)

            If UserUpdateResult = -1 Then
                UserUpdateResultStr = "FAILED"
            End If

            If logInserted = -1 Then
                logInsertedStr = "FAILED"
            End If


            Response.Write(Date.Now & "; MSISDN=" & MSISDN & "Keyword= " & keyword & "; UnSubscribe= " & UserUpdateResultStr & "; LogInsert= " & logInsertedStr)
            logger.Info(Date.Now & "; [Unsubscribe] MSISDN=" & MSISDN & "Keyword= " & keyword & "; UnSubscribe= " & UserUpdateResultStr & "; LogInsert= " & logInsertedStr)

        Catch ex As Exception
            logger.Fatal(Date.Now & "[Error-Unsubscribe]", ex)
        End Try
    End Sub

    Public Function UpdateInActiveUserSubs(ByVal pMSISDN As String, ByVal pKeywordID As Integer) As Integer
        Dim result As Integer = -1
        Dim sql As String = ""

        Dim par(3) As SqlParameter
        sql = "update [User] set DateTerminate = @DateTerminate,StatusID=@StatusID " & _
                              "where KeywordID=@KeywordID and MSISDN=@MSISDN "


        par(0) = New SqlParameter("@DateTerminate ", SqlDbType.DateTime)
        par(0).Value = Date.Now

        par(1) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(1).Value = pKeywordID

        par(2) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(2).Value = pMSISDN

        par(3) = New SqlParameter("@StatusID", SqlDbType.Int)
        par(3).Value = General.UserStatus._Inactive


        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            logger.Fatal("[SQL FATAL] - " & sql, ex)
        End Try

        Return result

    End Function

    Public Function logHistory(ByVal pUserID As Integer, ByVal pKeywordID As Integer, ByVal pTelcoID As Integer) As Integer
        Dim result As Integer = -1
        Dim sql As String = "INSERT INTO [userhistory]([UserID],[KeywordID],[TelcoID],[Action],[TimeStamp]) " & _
        "VALUES(@UserID,@KeywordID,@TelcoID,'Web Terminated',@TimeStamp)"

        Dim par(3) As SqlParameter
        par(0) = New SqlParameter("@UserID", SqlDbType.Int)
        par(0).Value = pUserID

        par(1) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(1).Value = pKeywordID

        par(2) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(2).Value = pTelcoID

        par(3) = New SqlParameter("@TimeStamp", SqlDbType.DateTime)
        par(3).Value = Date.Now


        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return result

    End Function

    Public Function RequestData(ByVal strParameter As String) As String
        Dim strReturn As String = ""
        Try
            strReturn = Request.QueryString(strParameter)
            If strReturn = "" Then
                strReturn = System.Web.HttpUtility.UrlDecode(Request.Form(strParameter))
            End If
            If strReturn <> "" Then
                strReturn = strReturn.Trim
                Return strReturn
            Else
                Return ""
            End If
        Catch
            Return ""
        End Try
    End Function

End Class