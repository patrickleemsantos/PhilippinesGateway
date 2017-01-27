Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel
Imports System.IO

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class Service1
    Inherits System.Web.Services.WebService
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)


    <WebMethod()> _
    Public Function ResendMT() As String

        'To config the log4net
        log4net.Config.XmlConfigurator.Configure()

        'Get MT from DB
        Dim c As New CRUD
        Dim _data = c.getMT()

        If _data.Tables(0).Rows.Count > 0 Then
            'pRootNode.Checked = False
            For Each pRows As Data.DataRow In _data.Tables(0).Rows

                Dim parameter As String = ""
                Dim url As String = "http://115.85.17.59:8001/sendsms.aspx?"
                Dim result As String = ""

                'MSISDN,[Message],Charge,MsgGUID,KeywordID,MTID,shortcode
                '2013-03-19 00:01:27,568 - [SMS]::1; user=Test&pass=Test1234&to=639463339821&sc=2855&serviceid=14&type=1
                '&msg=Thank you for subscribing to Daily Horoscope!Ul now receive Horoscope Tips for Leo.2.50/txt.Quit?Txt CHECK to 2855 for free.&charge=250&url=&telcoid=1&msgid=620023492794
                '&other=Welcome; CPID=1;Queue=.\private$\smart_mt;ReceiveDate=3/19/2013 12:01:27 AM;MsgGUID=dbe8f546-c4b2-411a-82fb-4569f27db87d;0000
                Dim pMsisdn As String = pRows.Item("MSISDN").ToString
                Dim pMessage As String = pRows.Item("Message").ToString
                Dim pCharge As String = pRows.Item("Charge").ToString
                Dim pMsgGUID As String = pRows.Item("MsgGUID").ToString
                Dim pKeywordID As String = pRows.Item("KeywordID").ToString
                Dim pMTID As String = pRows.Item("MTID").ToString
                Dim pshortcode As String = pRows.Item("shortcode").ToString

                parameter = "user=test&pass=test1234&to=" & pMsisdn & "&sc=" & pshortcode & "&serviceid=" & pKeywordID & "&type=1&msg=" & pMessage & "&charge=" & pCharge & "&telcoid=3&msgid=" & pMTID
                url = url & parameter

                If pMsisdn = "09062645297" Then

                Else
                    result = WebRequest(url, parameter)
                End If

                logger.Info("resend MT" & url & ";Result=" & result)

                'cc109d82-56fd-4d54-86b5-7c5971000722
                'http://115.85.17.59:8001/sendsms.aspx?user=test&pass=1234&to=1234567&sc=2889&serviceid=12&type=1&msg=test1234&charge=100&telcoid=1&msgid=abc123


            Next
        End If

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
End Class