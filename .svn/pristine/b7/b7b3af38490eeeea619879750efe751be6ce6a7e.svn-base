Imports System.Data.SqlClient

Public Class TelcoChargeCodeCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Dim dt As DataSet


    Public Function GetTelcoChargeCode(ByVal pTelcoID As Integer, ByVal pChargeCode As Integer, ByVal pShortCode As String) As String

        Dim result As String = ""

        Dim sql As String = "SELECT TelcoChargeCode FROM TelcoChargeCode WHERE telcoid= @telcoid and chargecode=@chargecode and shortcode= @shortcode"

        Dim par(2) As SqlParameter
        par(0) = New SqlParameter("@telcoid", SqlDbType.Int)
        par(0).Value = pTelcoID

        par(1) = New SqlParameter("@chargecode", SqlDbType.Int)
        par(1).Value = pChargeCode

        par(2) = New SqlParameter("@shortcode", SqlDbType.VarChar)
        par(2).Value = pShortCode

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
                'dt.Dispose()
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
