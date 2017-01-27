Imports System.Data.SqlClient

Public Class UserCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)
    Dim dt As DataSet

    Public Sub New()

    End Sub

    Public Function CheckUserSubs(ByVal pMSISDN As String, ByVal pKeywordID As Integer) As Integer

        Dim Result As Integer = 0

        Dim sql As String = "select statusID from [User] where MSISDN=@MSISDN and KeywordID = @KeywordID "

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(0).Value = pMSISDN

        par(1) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(1).Value = pKeywordID

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                Result = SQLHelper.ExecuteScalar(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return Result
    End Function

    Public Function GetUserSubsKeyword(ByVal pMSISDN As String, ByVal pTelcoID As Integer) As DataSet


        Dim sql As String = "select Keyword from dbo.[User] a inner join " & _
                                     "dbo.Keyword b on a.KeywordID = b.KeywordID " & _
                                      "where a.msisdn = @MSISDN and a.telcoid=@telcoid and a.statusID=@statusID"

        Dim par(2) As SqlParameter
        par(0) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(0).Value = pMSISDN

        par(1) = New SqlParameter("@telcoid", SqlDbType.Int)
        par(1).Value = pTelcoID

        par(2) = New SqlParameter("@statusID", SqlDbType.Int)
        par(2).Value = UserStatus._Active

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                dt = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return dt
    End Function


    Public Function UpdateUserSubs(ByVal pMSISDN As String, ByVal pKeywordID As Integer, ByVal pTransactionID As String, ByVal pStatus As Integer) As Integer
        Dim result As Integer = 0
        Dim sql As String = ""
        Dim par(4) As SqlParameter

        If pStatus = General.UserStatus._Active Then
            sql = "update [User] set TransactionID = @TransactionID,DateJoined = @DateJoined,StatusID=@StatusID " & _
                              "where KeywordID=@KeywordID and MSISDN=@MSISDN "

            par(0) = New SqlParameter("@TransactionID", SqlDbType.NVarChar)
            par(0).Value = pTransactionID

            par(1) = New SqlParameter("@DateJoined ", SqlDbType.DateTime)
            par(1).Value = Date.Now

            par(2) = New SqlParameter("@KeywordID", SqlDbType.Int)
            par(2).Value = pKeywordID

            par(3) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
            par(3).Value = pMSISDN

            par(4) = New SqlParameter("@StatusID", SqlDbType.Int)
            par(4).Value = General.UserStatus._Active

        Else
            sql = "update [User] set TransactionID = @TransactionID,DateTerminate = @DateTerminate,StatusID=@StatusID " & _
                                  "where KeywordID=@KeywordID and MSISDN=@MSISDN "

            par(0) = New SqlParameter("@TransactionID", SqlDbType.NVarChar)
            par(0).Value = pTransactionID

            par(1) = New SqlParameter("@DateTerminate ", SqlDbType.DateTime)
            par(1).Value = Date.Now

            par(2) = New SqlParameter("@KeywordID", SqlDbType.Int)
            par(2).Value = pKeywordID

            par(3) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
            par(3).Value = pMSISDN

            par(4) = New SqlParameter("@StatusID", SqlDbType.Int)
            par(4).Value = General.UserStatus._Inactive

        End If

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            logger.Fatal("[SQL FATAL] - " & sql, ex)
        End Try

        Return result

    End Function

    Public Function UpdateActiveUserSubs(ByVal pMSISDN As String, ByVal pKeywordID As Integer) As Integer
        Dim result As Integer = -1
        Dim sql As String = ""

        Dim par(2) As SqlParameter
        sql = "update [User] set StatusID=@StatusID " & _
                          "where KeywordID=@KeywordID and MSISDN=@MSISDN "

        par(0) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(0).Value = pKeywordID

        par(1) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(1).Value = pMSISDN

        par(2) = New SqlParameter("@StatusID", SqlDbType.Int)
        par(2).Value = General.UserStatus._Active

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            logger.Fatal("[SQL FATAL] - " & sql, ex)
        End Try

        Return result

    End Function

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

    Public Function UpdateAllUserSubs(ByVal pMSISDN As String, ByVal pStatus As Integer) As Integer
        Dim result As Integer = 0
        Dim sql As String = ""
        Dim par(2) As SqlParameter

        sql = "update [User] set DateTerminate=@DateTerminate,StatusID=@StatusID " & _
                          "where MSISDN=@MSISDN "


        par(0) = New SqlParameter("@DateTerminate", SqlDbType.DateTime)
        par(0).Value = Date.Now

        par(1) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(1).Value = pMSISDN

        par(2) = New SqlParameter("@StatusID", SqlDbType.Int)
        par(2).Value = pStatus

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            logger.Fatal("[SQL FATAL] - " & sql, ex)
        End Try

        Return result

    End Function

    Public Function GetUserSubsKeyword(ByVal MSISDN As String) As DataSet

        Dim sql As String = "select b.KeywordID,b.MOUrl,b.Keyword from [User] a inner join Keyword b on a.KeywordID=b.KeywordID " & _
                                      "where a.MSISDN=@MSISDN and a.StatusID=@StatusID"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(0).Value = MSISDN

        par(1) = New SqlParameter("@StatusID", SqlDbType.Int)
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

    'function to update subs time based user  for active subs only
    Public Function UpdateUserSubsTimeBased(ByVal pMSISDN As String, ByVal pKeywordID As Integer, ByVal pTransactionID As String, ByVal pTimeFrame As Integer) As Integer
        Dim result As Integer = 0
        Dim sql As String = ""
        Dim par(5) As SqlParameter

        sql = "update [User] set TransactionID = @TransactionID,DateJoined = @DateJoined,DateExpire=@DateExpire,StatusID=@StatusID " & _
                              "where KeywordID=@KeywordID and MSISDN=@MSISDN "

        par(0) = New SqlParameter("@TransactionID", SqlDbType.Int)
        par(0).Value = pTransactionID

        par(1) = New SqlParameter("@DateJoined ", SqlDbType.DateTime)
        par(1).Value = Date.Now

        par(2) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(2).Value = pKeywordID

        par(3) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(3).Value = pMSISDN

        par(4) = New SqlParameter("@StatusID", SqlDbType.Int)
        par(4).Value = General.UserStatus._Active

        par(5) = New SqlParameter("@DateExpire", SqlDbType.DateTime)
        par(5).Value = Date.Now.AddDays(pTimeFrame)


        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return result

    End Function

    Public Function UserInsert(ByVal body As Tbl_User) As Integer

        Dim result As Integer = 0
        Dim sql As String = "INSERT INTO [User]([TelcoID],[KeywordID],[StatusID],[TransactionID],[Name],[MSISDN],[DateJoined],[DateExpire],[DateTerminate],[TimeStamp]) " & _
                                     "VALUES(@TelcoID,@KeywordID,@StatusID,@TransactionID,@Name,@MSISDN,@DateJoined,@DateExpire,@DateTerminate,@TimeStamp) "

        Dim par(9) As SqlParameter
        par(0) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(0).Value = body.TelcoID

        par(1) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(1).Value = body.KeywordID

        par(2) = New SqlParameter("@StatusID", SqlDbType.Int)
        par(2).Value = body.StatusID

        par(3) = New SqlParameter("@TransactionID", SqlDbType.NVarChar)
        par(3).Value = body.TransactionID

        par(4) = New SqlParameter("@Name", SqlDbType.NVarChar)
        par(4).Value = body.Name

        par(5) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(5).Value = body.MSISDN

        par(6) = New SqlParameter("@DateJoined", SqlDbType.DateTime)
        par(6).Value = body.DateJoined

        par(7) = New SqlParameter("@DateExpire", SqlDbType.DateTime)
        par(7).Value = body.DateExpire

        par(8) = New SqlParameter("@DateTerminate", SqlDbType.DateTime)
        par(8).Value = body.DateTerminate

        par(9) = New SqlParameter("@TimeStamp", SqlDbType.DateTime)
        par(9).Value = Date.Now

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return result

    End Function

#Region "IDisposable"
    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other statde (managed objects).
            End If

            ' TODO: free your own state (unmanaged objects).
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

#Region " IDisposable Support "
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

#End Region

End Class
