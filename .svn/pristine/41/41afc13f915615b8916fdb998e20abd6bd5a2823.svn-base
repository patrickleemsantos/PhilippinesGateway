Imports System.Data.SqlClient

Public Class InboxCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Public Function InboxInsert(ByVal body As Tbl_MOStr) As Integer

        Dim result As Integer = 0
        Dim sql As String = "INSERT INTO Inbox ([KeywordID],[MSISDN],[Message],[TransactionID],[TelcoID],[DataCoding],[ReceiveDate],[TimeStamp]) " & _
                                     "VALUES(@KeywordID,@MSISDN,@Message,@TransactionID,@TelcoID,@DataCoding,@ReceiveDate,@TimeStamp) "

        Dim par(7) As SqlParameter
        par(0) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(0).Value = body.KeywordID

        par(1) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(1).Value = body.MSISDN

        par(2) = New SqlParameter("@Message", SqlDbType.NVarChar)
        par(2).Value = body.Message

        par(3) = New SqlParameter("@TransactionID", SqlDbType.NVarChar)
        par(3).Value = body.TransactionID

        par(4) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(4).Value = body.TelcoID

        par(5) = New SqlParameter("@DataCoding", SqlDbType.Int)
        par(5).Value = body.DataCoding

        par(6) = New SqlParameter("@ReceiveDate", SqlDbType.DateTime)
        par(6).Value = body.ReceiveDate

        par(7) = New SqlParameter("@TimeStamp", SqlDbType.DateTime)
        par(7).Value = Date.Now


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
