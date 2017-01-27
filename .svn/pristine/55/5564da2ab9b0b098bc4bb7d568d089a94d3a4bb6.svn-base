Imports System.Data.SqlClient

Public Class ReserveKeywordCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Dim dt As DataSet

    Public Function CheckReserveKeyword(ByVal ReserveKeyword As String, ByVal TelcoID As Integer, ByVal FormatType As Integer) As String

        Dim MOUrl As String = ""
        Dim sql As String = "Select MOUrl from reservekeyword where reservekeyword=?ReserveKeyword and telcoid =?TelcoID and FormatType=?FormatType"


        Dim par(2) As SqlParameter
        par(0) = New SqlParameter("?ReserveKeyword", SqlDbType.NVarChar)
        par(0).Value = ReserveKeyword

        par(1) = New SqlParameter("?TelcoID", SqlDbType.Int)
        par(1).Value = TelcoID

        par(2) = New SqlParameter("?FormatType", SqlDbType.Int)
        par(2).Value = FormatType


        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                MOUrl = SQLHelper.ExecuteScalar(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            logger.Fatal("[SQL FATAL] - " & sql, ex)
        End Try

        Return MOUrl
    End Function



    Public Function GetKeywordInfo_byKeywordShortCodeType(ByVal keyword As String, ByVal shortcode As String) As DataSet

        Dim sql As String = "SELECT * FROM Keyword a inner join ShortCode b WHERE a.Keyword= ?Keyword and b.Shortcode=?ShortCode"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("?Keyword", SqlDbType.NVarChar)
        par(0).Value = keyword

        par(1) = New SqlParameter("?ShortCode", SqlDbType.NVarChar)
        par(1).Value = shortcode

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                dt = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            logger.Fatal("[SQL FATAL] - " & sql, ex)
        End Try

        Return dt
    End Function


#Region "IDisposable"
    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other statde (managed objects).
                dt.Dispose()
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
