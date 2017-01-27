Imports System.Data.SqlClient

Public Class KeywordMsgSettingCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Dim dt As DataSet


    Public Function GetWelcomeMessage(ByVal pKeywordID As Integer) As Tbl_KeywordMsgSetting

        Dim result As String = ""

        Dim sql As String = "SELECT * FROM KeywordMsgSetting WHERE KeywordID=@KeywordID and Status=@Status"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(0).Value = pKeywordID

        par(1) = New SqlParameter("@Status", SqlDbType.Int)
        par(1).Value = KeywordMsgSettingStatus._Active

        Dim DTO As New Tbl_KeywordMsgSetting
        DTO.KeywordMsgSettingID = 0

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                Dim reader = SQLHelper.ExecuteReader(conn, CommandType.Text, sql, par)
                While reader.Read
                    DTO.KeywordMsgSettingID = reader("KeywordMsgSettingID")
                    DTO.KeywordID = reader("KeywordID")
                    DTO.Charge = reader("Charge")
                    DTO.WelcomeMsg = reader("WelcomeMsg")
                    DTO.HelpMsg = reader("HelpMsg")
                    DTO.Status = reader("Status")
                    DTO.MsgType = reader("MsgType")
                End While
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return DTO
    End Function

    Public Function GetHelpMsgByKeywordShortCode(ByVal pKeyword As String, ByVal pShortCode As String) As DataSet
        Dim Result As String = ""
        Dim sql As String = "select b.HelpMsg,a.keywordid,c.username,c.[password],b.charge,b.MsgType from dbo.Keyword a inner join " & _
                                      "dbo.KeywordMsgSetting b on a.keywordid=b.keywordid inner join " & _
                                     "dbo.cp c on a.cpid=c.cpid " & _
                                      "where a.keyword=@keyword and a.ShortCode=@shortcode"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@keyword", SqlDbType.NVarChar)
        par(0).Value = pKeyword

        par(1) = New SqlParameter("@shortcode", SqlDbType.NVarChar)
        par(1).Value = pShortCode

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                dt = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
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
