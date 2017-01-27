Imports System.Data.SqlClient

Public Class EGGDNCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Public Function EGGDNInsert(ByVal body As EGGDNStrc) As Integer

        Dim result As Integer = 0
        Dim sql As String = "INSERT INTO EGGDN([TransID],[Status],[ShortCode],[ReceiveDate],[TimeStamp]) VALUES(@TransID,@Status,@ShortCode,@ReceiveDate,@TimeStamp)"

        Dim par(4) As SqlParameter
        par(0) = New SqlParameter("@TransID", SqlDbType.NVarChar)
        par(0).Value = body.TransID

        par(1) = New SqlParameter("@Status", SqlDbType.NVarChar)
        par(1).Value = body.Status

        par(2) = New SqlParameter("@ShortCode", SqlDbType.NVarChar)
        par(2).Value = body.ShortCode

        par(3) = New SqlParameter("@ReceiveDate", SqlDbType.DateTime)
        par(3).Value = body.ReceiveDate

        par(4) = New SqlParameter("@TimeStamp", SqlDbType.DateTime)
        par(4).Value = DateTime.Now


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
