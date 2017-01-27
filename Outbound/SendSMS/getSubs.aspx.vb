Imports System.Messaging
Imports LibraryDAL
Imports LibraryDAL.General
Imports System.Data.SqlClient

Partial Public Class getSubs
    Inherits System.Web.UI.Page

    Private MSISDN As String
    Dim dt As DataSet
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        MSISDN = RequestData("msisdn")

        Try
            Dim UserCls As New UserCRUD
            Dim KeywordList As String = ""
            Dim dt1 As New DataSet
            dt1 = GetUserSubsServices(MSISDN)

            If dt1.Tables(0).Rows.Count > 0 Then
                For Each r As DataRow In dt1.Tables(0).Rows
                    KeywordList = KeywordList & "<br/> Keyword= " & r.Item("Keyword").ToString & ": Opt In Date= " & _
                    r.Item("datejoined").ToString() & ": <a href='./UnSubs.aspx?msisdn=" & _
                    MSISDN & "&keyid=" & r.Item("keyid").ToString & _
                    "&keyword=" & r.Item("Keyword").ToString & _
                    "&telcoid=" & r.Item("telcoid").ToString & _
                    "&userid=" & r.Item("userid").ToString & "'>UnSubscribe</a>"
                Next
            Else
                KeywordList = "No Keyword Found"
            End If

            Response.Write(Date.Now & "MSISDN=" & MSISDN & KeywordList)
            logger.Info(Date.Now & "[CheckServices] MSISDN=" & MSISDN & ";KeywordList=" & KeywordList)

        Catch ex As Exception
            logger.Fatal(Date.Now & "[Error-CheckServices]", ex)
        End Try
    End Sub


    Public Function GetUserSubsServices(ByVal pMSISDN As String) As DataSet
        Dim sql As String = "select Keyword, a.KeywordID as keyid, a.DateJoined as datejoined, a.UserID as userid, a.TelcoID as telcoid from dbo.[User] a inner join " & _
                                     "dbo.Keyword b on a.KeywordID = b.KeywordID " & _
                                      "where a.msisdn = @MSISDN and a.statusID=@statusID"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(0).Value = pMSISDN

        par(1) = New SqlParameter("@statusID", SqlDbType.Int)
        par(1).Value = UserStatus._Active

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                dt = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return dt
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