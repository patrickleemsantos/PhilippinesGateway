Imports System.Data.SqlClient

Public Class KeywordCRUD
    Implements IDisposable

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()

    End Sub

    Dim dt As DataSet

    Public Function ChecKeyword(ByVal FirstKeyword As String, ByVal SecondKeyword As String, ByVal TelcoID As Integer, ByVal ShortCode As String) As DataSet

        Dim sql As String = "select b.MOUrl as rMOUrl,c.MOUrl as kMOUrl,a.KeywordID,c.TypeID as KeywordType " & _
                                      "from keyworddetect a inner join " & _
                                      "reserveKeyword b on b.reservekeywordid = a.reservekeywordid inner join " & _
                                      "Keyword c on c.KeywordID = a.keywordid " & _
                                       "where a.firstkeyword =@FirstKeyword and a.secondkeyword =@SecondKeyword " & _
                                       "and b.TelcoID =@TelcoID and c.ShortCode = @ShortCode "

        Dim par(3) As SqlParameter
        par(0) = New SqlParameter("@FirstKeyword", SqlDbType.NVarChar)
        par(0).Value = FirstKeyword

        par(1) = New SqlParameter("@SecondKeyword", SqlDbType.NVarChar)
        par(1).Value = SecondKeyword

        par(2) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(2).Value = TelcoID

        par(3) = New SqlParameter("@ShortCode", SqlDbType.NVarChar)
        par(3).Value = ShortCode


        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                dt = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return dt
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
            Throw ex
        End Try

        Return dt
    End Function

    Public Function GetKeywordInfo_byKeywordID(ByVal pKeywordID As Integer) As Tbl_Keyword

        Dim sql As String = "select * from Keyword where KeywordID= @KeywordID"

        Dim par(0) As SqlParameter
        par(0) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(0).Value = pKeywordID

        Dim DTO As New Tbl_Keyword
        DTO.KeywordID = 0

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                Dim reader = SQLHelper.ExecuteReader(conn, CommandType.Text, sql, par)
                While reader.Read
                    DTO.KeywordID = SQLHelper.CheckIntNull(reader("KeywordID"))
                    DTO.Keyword = reader("Keyword")
                    DTO.ShortCode = reader("ShortCode")
                    DTO.StatusID = reader("StatusID")
                    DTO.Charge = SQLHelper.CheckStringNull(reader("Charge"))
                    DTO.CPID = reader("CPID")
                    DTO.TypeID = reader("TypeID")
                    DTO.TimeFrame = SQLHelper.CheckDateNull(reader("TimeFrame"))
                    DTO.Remark = SQLHelper.CheckStringNull(reader("Remark"))
                    DTO.MOUrl = SQLHelper.CheckStringNull(reader("MOUrl"))
                    DTO.DNUrl = SQLHelper.CheckStringNull(reader("DNUrl"))
                End While
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return DTO
    End Function

    Public Function GetKeywordInfo_byMSISDN(ByVal MSISDN As String) As DataSet ' get keyword infor is user is active

        Dim sql As String = "select b.KeywordID,b.MOUrl,b.Keyword from [User] a inner join Keyword b on a.KeywordID=b.KeywordID " & _
                                      "where a.MSISDN=@MSISDN and a.StatusID=@StatusID"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(0).Value = MSISDN

        par(1) = New SqlParameter("@StatusID", SqlDbType.Int)
        par(1).Value = UserStatus._Active

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                dt = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return dt
    End Function

    Public Function GetKeywordURL_byMSISDN(ByVal MSISDN As String) As DataSet

        Dim sql As String = "select isnull(b.MOUrl,'') as MOUrl from [User] a inner join Keyword b on a.KeywordID=b.KeywordID " & _
                                     "where a.MSISDN=@MSISDN and a.StatusID=@StatusID group by  b.MOUrl"

        Dim par(1) As SqlParameter
        par(0) = New SqlParameter("@MSISDN", SqlDbType.NVarChar)
        par(0).Value = MSISDN

        par(1) = New SqlParameter("@StatusID", SqlDbType.Int)
        par(1).Value = UserStatus._Active

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                dt = SQLHelper.ExecuteDataset(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return dt
    End Function


    

    Public Function GetSmartTelcoKeywordInfo(ByVal pKeywordID As Integer) As String
        Dim Result As String = ""
        Dim sql As String = "select ServiceID From SmartKeyword where KeywordID = @KeywordID"

        Dim par As SqlParameter
        par = New SqlParameter("@KeywordID", SqlDbType.NVarChar)
        par.Value = pKeywordID

        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                Result = SQLHelper.ExecuteScalar(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try

        Return Result
    End Function

    Public Function GetDNURL_byKeyid(ByVal keyid As Integer) As String
        Dim sql As String = ""
        Dim result As String = ""
        sql = "SELECT DNUrl FROM Keyword WHERE KeywordID=@KeywordID"
        Dim par As SqlParameter
        par = New SqlParameter("@KeywordID", SqlDbType.Int)
        par.Value = keyid
        Try
            Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                result = SQLHelper.ExecuteScalar(conn, CommandType.Text, sql, par)
            End Using
        Catch ex As Exception
            Throw ex
        End Try
        Return result

    End Function

    Public Function GetKeywordShotcodeSD(ByVal pShortCode As String, ByVal pKeywordID As Integer, ByVal pTelcoID As Integer) As DataSet
        Dim result As String = ""

        Dim sql As String = "select ServiceDescription,ServiceID from  dbo.KeywordShortCode a inner join " & _
                                      "dbo.ShortCode b on a.ShortCodeID=b.ShortCodeID " & _
                                     "where a.KeywordID =@KeywordID and b.ShortCode=@ShortCode and b.TelcoID =@TelcoID  "

        Dim par(2) As SqlParameter
        par(0) = New SqlParameter("@TelcoID", SqlDbType.Int)
        par(0).Value = pTelcoID

        par(1) = New SqlParameter("@KeywordID", SqlDbType.Int)
        par(1).Value = pKeywordID

        par(2) = New SqlParameter("@ShortCode", SqlDbType.NVarChar)
        par(2).Value = pShortCode

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
