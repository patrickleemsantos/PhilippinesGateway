Imports System.Data.SqlClient

Public Class OutboxCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Public Function OutboxInsert(ByVal body As Tbl_Outbox) As Integer

        Dim result As Integer = 0
        Dim sql As String = "INSERT INTO Outbox ([MSISDN],[Message],[CPID],[MessageTypeID],[Charge],[StatusCode],[MsgGUID],[KeywordID],[TransactionID]" & _
                                      ",[MTID],[RetryCount],[ContentURL],[TelcoID],[MsgCount],[ShortCode],[SendDate],[ReceiveDate],[TimeStamp],[Status]) " & _
                                     "VALUES(@MSISDN,@Message,@CPID,@MessageTypeID,@Charge,@StatusCode,@MsgGUID,@KeywordID,@TransactionID," & _
                                    "@MTID,@RetryCount,@ContentURL,@TelcoID,@MsgCount,@ShortCode,@SendDate,@ReceiveDate,@TimeStamp,@Status) "

        Dim par(18) As SqlParameter
        par(0) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(0).Value = body.MSISDN

        par(1) = New SqlParameter("@Message", SqlDbType.NVarChar)
        par(1).Value = body.Message

        par(2) = New SqlParameter("@CPID", SqlDbType.Int)
        par(2).Value = body.CPID

        par(3) = New SqlParameter("@MessageTypeID", SqlDbType.Int)
        par(3).Value = body.MessageTypeID

        par(4) = New SqlParameter("@Charge", SqlDbType.Int)
        par(4).Value = body.Charge

        par(5) = New SqlParameter("@StatusCode", SqlDbType.NVarChar)
        par(5).Value = body.StatusCode

        par(6) = New SqlParameter("@MsgGUID", SqlDbType.NVarChar)
        par(6).Value = body.MsgGUID

        par(7) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(7).Value = body.KeywordID

        par(8) = New SqlParameter("@TransactionID", SqlDbType.NVarChar)
        par(8).Value = body.TransactionID

        par(9) = New SqlParameter("@MTID", SqlDbType.NVarChar)
        par(9).Value = body.MTID

        par(10) = New SqlParameter("@RetryCount", SqlDbType.Int)
        par(10).Value = body.RetryCount

        par(11) = New SqlParameter("@ContentURL", SqlDbType.NVarChar)
        par(11).Value = body.ContentURL

        par(12) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(12).Value = body.TelcoID

        par(13) = New SqlParameter("@MsgCount", SqlDbType.Int)
        par(13).Value = body.MsgCount

        par(14) = New SqlParameter("@ShortCode", SqlDbType.NVarChar)
        par(14).Value = body.ShortCode

        par(15) = New SqlParameter("@SendDate", SqlDbType.DateTime)
        par(15).Value = Date.Now 'body.SendDate

        par(16) = New SqlParameter("@ReceiveDate", SqlDbType.DateTime)
        par(16).Value = Date.Now 'body.ReceiveDate

        par(17) = New SqlParameter("@TimeStamp", SqlDbType.DateTime)
        par(17).Value = Date.Now

        par(18) = New SqlParameter("@Status", SqlDbType.Int)
        par(18).Value = body.Status

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteNonQuery(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return result

    End Function

    Public Function CheckOutboxDN(ByVal pMsgMTID As String) As DataSet
        Dim sql As String = ""
        Dim Result As DataSet
        Try

            '15/04/2013,A, Set a date to search rather to search all
            Dim pDateFrom As DateTime
            pDateFrom = Date.Now.AddDays(-3).ToString("MM/dd/yyyy")
            sql = "SELECT KeywordID,MsgGUID FROM Outbox WHERE MTID=@MTID and Senddate>=@pDateFrom"

            Dim par(1) As SqlParameter

            par(0) = New SqlParameter("@MTID", SqlDbType.VarChar)
            par(0).Value = pMsgMTID

            par(1) = New SqlParameter("@pDateFrom", SqlDbType.DateTime2)
            par(1).Value = pDateFrom

            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                Result = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return Result
    End Function

    Public Function GetAllFromEggDN() As DataSet
        Dim sql As String = ""
        Dim Result As DataSet
        Try

            sql = "SELECT Transid,Status,ReceiveDate FROM eggdn WHERE ReceiveDate >='2013-07-01' and ReceiveDate <'2013-07-29' and status = 200"

            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                Result = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return Result
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
