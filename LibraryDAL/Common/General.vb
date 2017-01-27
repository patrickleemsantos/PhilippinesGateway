Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security
Imports System.Net
Imports System.Web
Imports System.IO

Public Module General


    Private Declare Function QueryPerformanceCounter Lib "Kernel32" (ByRef X As Long) As Short
    Private Declare Function QueryPerformanceFrequency Lib "Kernel32" (ByRef X As Long) As Short

    Public _defaultDateTime As DateTime = New DateTime(1900, 1, 1)

    'SMS API Retrun
    Public Const SuccessSend As String = "0000"
    Public Const MissingParameterField As String = "0001"
    Public Const ServerError As String = "1000"

    Public Enum KeywordType
        _IOD = 1
        _ContentBasedSubs = 2
        _TimeBasedSubs = 3
        _Conditional = 4
    End Enum

    Public Enum FormatType ' reserve keyword position exp: STOP NEWS - FormatType =1, NEWS STOP -  FormatType =2
        _First = 1
        _Second = 2
    End Enum

    Public Enum TelcoID
        _Smart = 1
        _Sun = 2
        _Globe = 3
    End Enum

    Public Enum UserStatus
        _Active = 1
        _Inactive = 2
    End Enum

    Public Enum MsgType
        TextSMS = 1
        RingTone = 2
        Logo = 3
        PictureMessage = 4
    End Enum

    'SmartMTResponse
    Public Enum SmartMTResponse
        _InsufficientBalance = 950
        _ResponseTimeout = 960
    End Enum

    Public Enum SystemCode
        _SmartTarrif = 9
    End Enum

    Public Enum MTStatus
        _Success = 1
        _Fail = 2
    End Enum

    Public Enum KeywordMsgSettingStatus
        _Active = 1
        _Inactive = 2
    End Enum


    Public Function TraceTime() As String
        Dim cnt As Long = 0
        Dim frq As Long = 0
        QueryPerformanceCounter(cnt)
        QueryPerformanceFrequency(frq)
        Dim c As Double = 1000 * CDbl(cnt) / CDbl(frq)
        TraceTime = c.ToString("#0.0000")
    End Function


    Public Function GetUniqueNumber() As String
        Dim cputime As String = TraceTime()
        Dim DatetimeStick As Long = ((DateTime.Now.Ticks / 12) Mod 100000000000)
        Dim number As String = String.Format("{0:D11}", DatetimeStick)

        Return number & Mid(cputime.Replace(".", "").Replace(",", ""), 8, 5)
    End Function

    Public Function SMSMessageCount(ByVal pMessage As String, ByVal pMsgType As Integer) As Integer

        Dim SMSHeader_2nd As Integer = 15
        Dim SMSHeader_Other As Integer = 7
        Dim SMSHeaderHex_2nd As Integer = 24  ' UTF 6 x 4 bits
        Dim SMSHeaderHex_Other As Integer = 12  ' UTF 3 x 4 bits

        Dim SMSTextSpecialCharacter As String = "^{}\\[~]|€"  ' special character in text sms will  count as 2 character


        Dim CharCount As Integer = 0


        If pMsgType = MsgType.TextSMS Then ' count for text

            'count message length with special character
            Dim SMSTempCount As Integer = 0
            For Each c In pMessage
                If SMSTextSpecialCharacter.IndexOf(c) <> -1 Then
                    SMSTempCount = SMSTempCount + 2
                Else
                    SMSTempCount = SMSTempCount + 1
                End If
            Next

            CharCount = SMSTempCount

            'get message count
            If CharCount <= 160 Then
                Return 1
            ElseIf CharCount > 160 And CharCount <= 306 Then ' second sms header need =  15
                Return 2
            ElseIf CharCount > 306 Then   ' second sms header need =  7
                CharCount = CharCount - 306
                Return 2 + Math.Ceiling((CharCount / 153))
            End If
        Else  ' count for Hex

            CharCount = pMessage.Length
            If CharCount <= 280 Then
                Return 1
            ElseIf CharCount > 280 And CharCount <= 536 Then ' second sms header need =  24
                Return 2
            ElseIf CharCount > 536 Then   ' second sms header need = 12
                CharCount = CharCount - 536
                Return 2 + Math.Ceiling((CharCount / 268))
            End If
        End If

    End Function

#Region "EGG"
    Public Function EGG_GetTelcoID(ByVal pTelcoDesc As String) As Integer

        Select Case pTelcoDesc
            Case "SMART"
                Return TelcoID._Smart
            Case "SUN"
                Return TelcoID._Sun
            Case "GLOBE"
                Return TelcoID._Globe
            Case Else
                Return 0
        End Select

    End Function

    Public Function EGG_GetTelcoDesc(ByVal pTelcoID As Integer) As String

        Select Case pTelcoID
            Case TelcoID._Smart
                Return "SMART"
            Case TelcoID._Sun
                Return "SUN"
            Case TelcoID._Globe
                Return "GLOBE"
            Case Else
                Return ""
        End Select

    End Function
#End Region

#Region "WebPost"
    Public Function WebURLPost(ByVal URL As String, ByVal PostData As String, ByVal TimeOut As Integer) As String
        Try
            If Left(URL.ToLower, 5) = "https" Then
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
                ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)
            End If

            Dim encoding As New System.Text.ASCIIEncoding
            Dim Data() As Byte = encoding.GetBytes(PostData)
            Dim LoginReq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(URL), Net.HttpWebRequest)
            With LoginReq
                .KeepAlive = False
                .Method = "POST"
                .ContentType = "application/x-www-form-urlencoded"
                .ContentLength = Data.Length
                .Timeout = TimeOut * 1000
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
            Return ex.Message
        End Try
    End Function

    Public Function ValidateServerCertificate(ByVal sender As Object, ByVal cert As X509Certificate, ByVal chain As X509Chain, _
ByVal ssl As SslPolicyErrors) As Boolean
        Return True
    End Function
#End Region

    Public Function SendToAPI(ByVal APIUrl As String, ByVal TimeOut As String, ByVal UserName As String, ByVal Password As String, ByVal MSISDN As String, ByVal ShortCode As String, ByVal MsgType As String, _
                                               ByVal Message As String, ByVal Charge As String, ByVal TelcoID As String, ByVal KeywordID As String, ByVal Other As String, ByVal MsgID As String) As String

        Dim PostResult As String = ""
        Dim URLData As String = "user=" & UserName & _
                             "&pass=" & Password & _
                             "&to=" & MSISDN & _
                             "&sc=" & ShortCode & _
                             "&type=" & MsgType & _
                             "&msg=" & System.Web.HttpUtility.UrlEncode(Message) & _
                             "&charge=" & Charge & _
                             "&telcoid=" & TelcoID & _
                             "&serviceid=" & KeywordID & _
                             "&other=" & Other & _
                             "&msgid=" & MsgID

        PostResult = WebURLPost(APIUrl, URLData, TimeOut)

        Return APIUrl & "?" & URLData & ";PostResult=" & PostResult
    End Function





End Module
