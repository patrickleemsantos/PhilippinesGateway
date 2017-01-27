Imports System.Data.SqlClient

Public Class ShortCodeCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Public Function GetShortCodeUserID(ByVal pShortCode As String, ByVal pTelcoID As Integer) As String

        Dim result As String = ""
        Dim sql As String = "Select ShortCodeUserID from shortcode where shortcode=@ShortCode and telcoid=@TelcoID"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@ShortCode", SqlDbType.NVarChar)
        par(0).Value = pShortCode
        par(1) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(1).Value = pTelcoID


        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteScalar(conn, CommandType.Text, sql, par)
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
