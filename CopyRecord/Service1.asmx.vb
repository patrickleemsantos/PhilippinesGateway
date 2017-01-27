Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel
Imports System.IO
Imports MySql.Data.MySqlClient

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class Service1
    Inherits System.Web.Services.WebService
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)
    Dim conn As String = System.Web.Configuration.WebConfigurationManager.AppSettings("SLconn").ToString
    Dim pDateFrom As String = System.Web.Configuration.WebConfigurationManager.AppSettings("pDateFrom").ToString
    Dim pDateTo As String = System.Web.Configuration.WebConfigurationManager.AppSettings("pDateTo").ToString



    <WebMethod()> _
    Public Function CopyRecord() As String

        'To config the log4net
        log4net.Config.XmlConfigurator.Configure()

        'Get MT from DB
        Dim c As New CRUD

        'Straight insert into Softlayer egg outbox with dn success
        Dim strsql As String = ""
        Dim pResult As String = ""
        Dim pDate As DateTime
        Dim pCount As String = "1"
        Dim ds As DataSet
        Dim msgGUID As String = ""

        ds = c.GetAllFromEggDN(pDateFrom, pDateTo)

        For Each d As DataRow In ds.Tables(0).Rows
            msgGUID = d.Item("Transid").ToString
            pDate = d.Item("ReceiveDate")

            pCount = msgGUID.Length()


            If pCount < 18 Then
                strsql = "insert into mt_egg_201307 values (0,3,2656,1,99,'09999999999',250,'" & _
            "Port from 12 26','','','',0,'','" & msgGUID & "','200','','','" & pDate.ToString("yyyy/MM/dd HH:mm:ss") & "','200','SUCCESS','" & pDate.ToString("yyyy/MM/dd HH:mm:ss") & "','');"
            Else
                strsql = "insert into mt_egg_201307 values (0,3,2656,1,99,'" & msgGUID.Substring(17) & "',250,'" & _
                "Port from 12 26','','','',0,'','" & msgGUID & "','200','','','" & pDate.ToString("yyyy/MM/dd HH:mm:ss") & "','200','SUCCESS','" & pDate.ToString("yyyy/MM/dd HH:mm:ss") & "','');"
            End If

            logger.Info("[Insert Queue]" & strsql)

            pResult = ExecuteCommand(strsql)

            If pResult = "Fail" Then

                logger.Error("[Post result] Result=" & pResult & ";URL=" & strsql)
            Else
                logger.Info("[Post result] Result=" & pResult & ";URL=" & strsql)

            End If
        Next

        Return "1"
    End Function

    Public Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
        Try
            Dim encoding As New System.Text.ASCIIEncoding
            Dim Data() As Byte = encoding.GetBytes(PostData)
            Dim LoginReq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(URL), Net.HttpWebRequest)
            With LoginReq
                .KeepAlive = False
                .Method = "POST"
                .ContentType = "application/x-www-form-urlencoded"
                .ContentLength = Data.Length
                .Timeout = CInt(60) * 1000
            End With

            Dim SendReq As Stream = LoginReq.GetRequestStream
            SendReq.Write(Data, 0, Data.Length)
            SendReq.Close()

            Dim LoginRes As System.Net.HttpWebResponse = CType(LoginReq.GetResponse(), Net.HttpWebResponse)
            Dim HTML As String = ""
            Using sReader As StreamReader = New StreamReader(LoginRes.GetResponseStream)
                HTML = sReader.ReadToEnd
            End Using

            HTML = HTML.Trim & " (" & LoginRes.StatusCode.ToString & ")"

            LoginRes.Close()
            LoginRes = Nothing

            Return HTML.Trim
        Catch ex As Exception
            Return "Fail"
        End Try
    End Function

    Public Function ExecuteCommand(ByVal sql As String) As String
        Try
            Using magentConn As New MySqlConnection(conn)
                magentConn.Open()
                Dim res As Object = Nothing
                Dim objCommand As New MySqlCommand(sql, magentConn)
                objCommand.ExecuteNonQuery()
                magentConn.Close()
                Return "Success"
            End Using
        Catch
            Return "Fail"
        End Try
    End Function
End Class