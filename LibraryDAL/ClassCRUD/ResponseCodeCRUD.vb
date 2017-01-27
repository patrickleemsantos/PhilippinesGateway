Imports System.Data.SqlClient
Public Class ResponseCodeCRUD
    Implements IDisposable


    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Function [Single](ByVal telcoid As Integer, ByVal responsecode As String) As ResponseCode
        Dim DTO As New ResponseCode
        Dim sql As String = "select ResponseID,TelcoID,StatusCode,StatusDesc,ResponseCode,ResponseDesc FROM responsecode WHERE telcoid=@telcoid And responsecode=@responsecode"
        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@telcoid", DbType.Int32)
        par(0).Value = telcoid
        par(1) = New SqlParameter("@responsecode", SqlDbType.NVarChar)
        par(1).Value = responsecode

        Try
            Using con As New SqlConnection(SQLHelper.GetConnectionString())
                Dim reader As SqlDataReader = SQLHelper.ExecuteReader(con, CommandType.Text, sql, par)
                While reader.Read
                    DTO = New ResponseCode
                    With DTO
                        .ResponseCode = SQLHelper.CheckStringNull(reader("ResponseCode"))
                        .ResponseDesc = SQLHelper.CheckStringNull(reader("ResponseDesc"))
                        .ResponseID = SQLHelper.CheckIntNull(reader("ResponseID"))
                        .StatusCode = SQLHelper.CheckStringNull(reader("StatusCode"))
                        .StatusDesc = SQLHelper.CheckStringNull(reader("StatusDesc"))
                        .TelcoID = SQLHelper.CheckIntNull(reader("TelcoID"))
                    End With
                End While
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return DTO
    End Function





    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other state (managed objects).
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

End Class
