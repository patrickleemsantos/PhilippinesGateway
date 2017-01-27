Imports System.Data.SqlClient

Public Class CPCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Dim dt As DataSet


    Public Function GetCPInfoByKeywordID(ByVal pKeywordID As Integer) As Tbl_CP

        Dim result As String = ""

        Dim sql As String = "select * from dbo.CP a inner join " & _
                                     "dbo.Keyword b on a.CPID=b.CPID where b.KeywordID=@KeywordID"

        Dim par As SqlParameter
        par = New SqlParameter("@KeywordID", SqlDbType.Int)
        par.Value = pKeywordID

        Dim DTO As New Tbl_CP
        DTO.CPID = 0

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                Dim reader = SQLHelper.ExecuteReader(conn, CommandType.Text, sql, par)
                While reader.Read
                    DTO.UserName = reader("UserName")
                    DTO.Password = reader("Password")
                    DTO.RevShareID = reader("RevShareID")
                    DTO.StatusID = reader("StatusID")
                    DTO.Name = SQLHelper.CheckStringNull(reader("Name"))
                    DTO.Email = reader("Email")
                    DTO.Remark = reader("Remark")
                    DTO.Address = reader("Address")
                    DTO.CPID = reader("CPID")
                End While
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return DTO
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
