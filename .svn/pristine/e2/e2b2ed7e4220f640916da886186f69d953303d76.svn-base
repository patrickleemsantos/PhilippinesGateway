Imports System.Data.SqlClient

Public Class OtherCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Public Function CheckSendSMSApi(ByVal pUserName As String, ByVal pPassword As String, ByVal pKeywordID As String, ByVal pShortCode As String, _
                                                        ByVal pMessageType As Integer, ByVal pMSISDN As String, ByVal pTelcoID As Integer, ByVal pCharge As Integer, _
                                                        ByVal pTransactionID As String) As String

        Dim result As String = ""
        Dim sql As String = "dbo.CheckAPI"

        Dim par(8) As SqlParameter
        par(0) = New SqlParameter("@UserName", SqlDbType.NVarChar)
        par(0).Value = pUserName

        par(1) = New SqlParameter("@Password", SqlDbType.NVarChar)
        par(1).Value = pPassword

        par(2) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(2).Value = pKeywordID

        par(3) = New SqlParameter("@ShortCode", SqlDbType.NVarChar)
        par(3).Value = pShortCode

        par(4) = New SqlParameter("@MessageType", SqlDbType.Int)
        par(4).Value = pMessageType

        par(5) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(5).Value = pMSISDN

        par(6) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(6).Value = pTelcoID

        par(7) = New SqlParameter("@Charge", SqlDbType.Int)
        par(7).Value = pCharge

        par(8) = New SqlParameter("@TransactionID", SqlDbType.NVarChar)
        par(8).Value = pTransactionID


        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteScalar(conn, CommandType.StoredProcedure, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return (result & "")

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
