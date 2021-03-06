﻿
Imports System.Messaging
Imports System.Globalization
Imports System.Configuration
Imports System.Threading
Imports System.Collections.Specialized
Imports System.Net
Imports System.Web
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports LibraryDAL
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Web.UI.WebControls
Imports Newtonsoft
Imports System.Xml
Imports MySql.Data.MySqlClient

Public Delegate Sub AddToDelegate(ByVal body As SendSMSAPIStrc)


Public Class MSMQListener

    Private mySqlConStr As String = ConfigurationManager.AppSettings("mySqlConn")

    Private Structure SmartMessageFormat
        Dim _UDH As String
        Dim _UD As String
    End Structure

    Private Structure MTResponse
        Dim _DA As String
        Dim _EC As String
        Dim _ECT As String
    End Structure


    Private _a As Integer = 0
    Private _listen As Boolean
    Private _queue As MessageQueue
    Private del As AddToDelegate
    Private death_queue As String = ConfigurationManager.AppSettings("death_queue")
    Private Insert_queue As String = ConfigurationManager.AppSettings("Insert_queue")
    Private unsub_queue As String = ConfigurationManager.AppSettings("unsub_queue")
    Private URL_Subscription As String = ConfigurationManager.AppSettings("Subscription_URL")
    Private URL_EWar As String = ConfigurationManager.AppSettings("EWar_URL")
    Private RetryCount As String = ConfigurationManager.AppSettings("RetryCount")
    Private PostURLTimeOut As Integer = CInt(ConfigurationManager.AppSettings("PostURLTimeOut"))
    Private RetryResponseCode As String = ConfigurationManager.AppSettings("RetryResponseCode")
    Private SuccessResponseCode As String = ConfigurationManager.AppSettings("SuccessResponseCode")
    Private UnsubResponseCode As String = ConfigurationManager.AppSettings("UnsubResponseCode")

    Private UserName As String = ConfigurationManager.AppSettings("UserName")
    Private Password As String = ConfigurationManager.AppSettings("Password")

    Private TelcoCPID As String

    Public Delegate Function ServerCertificateValidationCallback(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New(ByVal queuePath As String)
        _queue = New MessageQueue(queuePath)
        MessageQueue.EnableConnectionCache = False
    End Sub

    Public Sub Start()
        del = New AddToDelegate(AddressOf FireRecieveEvent)
        _listen = True
        AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted

        StartListening()
    End Sub

    Public Sub [Stop]()
        _listen = False
        RemoveHandler _queue.PeekCompleted, AddressOf OnPeekCompleted

    End Sub

    Private Sub StartListening()
        If Not _listen Then
            Exit Sub
        End If
        _queue.BeginPeek()
    End Sub

    Private Sub OnPeekCompleted(ByVal sender As Object, ByVal e As PeekCompletedEventArgs)
        Dim msg As Message = Nothing

        _queue.EndPeek(e.AsyncResult)

        Using trans As New MessageQueueTransaction()
            _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(SendSMSAPIStrc)})
            trans.Begin()
            msg = _queue.Receive(trans)
            trans.Commit()
        End Using

        StartListening()
        del.Invoke(CType(msg.Body, SendSMSAPIStrc))
    End Sub


    Private Sub FireRecieveEvent(ByVal objMessage As SendSMSAPIStrc)
        Dim MKCollection As New NameValueCollection
        Dim strMessage As String = ""
        Dim strDateTime As String = ""
        Dim strResultpost As String = ""
        Dim strColl As String = ""
        Dim strWebSendRes As String = ""
        Dim strTempWebSendRes As String = ""
        Dim strTCSD As String = ""
        Dim PostURL As String
        Dim MTID As String = ""
        Dim FullPostURL As String = ""

        Try

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)

            'msisdn  - mobile number to charged and received sms message
            'rcvd_transid  -  received transaction id.
            'message - Charging Keywords + SMS message 


            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTCSD = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using

            'MKCollection.Add("number", objMessage.MSISDN)
            'MKCollection.Add("rrn", objMessage.TransactionID)
            'MKCollection.Add("message", objMessage.Message)
            'MKCollection.Add("telco", EGG_GetTelcoDesc(objMessage.TelcoID)) 'MSISDN
            'MKCollection.Add("tariff", strTCSD)

            objMessage.Message = HttpUtility.UrlEncode("ew250 " & objMessage.Message)

            MKCollection.Add("msisdn", objMessage.MSISDN)
            MKCollection.Add("rcvd_transid", objMessage.TransactionID)
            MKCollection.Add("message", objMessage.Message)

            PostURL = URL_EWar
            MTID = objMessage.TransactionID

            If objMessage.KeywordType <> KeywordType._IOD Then ' rrn id for Subscription is generate by us
                PostURL = URL_Subscription ' different URL for IOD and sub
                MTID = GetUniqueNumber()
                MKCollection.Set("rrn", MTID)
            End If

            objMessage.SendDate = Date.Now

            strColl = ""      ' reset the strCOLL
            For ai = 0 To MKCollection.Count - 1
                strColl = strColl & MKCollection.GetKey(ai) & "=" & MKCollection.Get(ai) & "&"
            Next
            strColl = strColl.Remove(strColl.Length - 1, 1)

            strWebSendRes = WebRequest(PostURL, strColl)

            FullPostURL = PostURL & "?" & strColl
            'logger.Info("[POST RESULT] Result=," & strWebSendRes & ",")

            'Change MTID to objMessage.TransactionID
            If strWebSendRes = "500" Then
                send2Death(objMessage, "", strWebSendRes, MTID)
            Else
                If SuccessResponseCode.Contains("," & strWebSendRes & ",") Then  'Success
                    send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Success)
                    logger.Info("[SUCCESS]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                Else
                    send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Fail) 'Other Error
                    logger.Error("[ERROR]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                End If
            End If

        Catch ex As Exception
            send2Death(objMessage, "", "", MTID)
            logger.Fatal("[FATAL]" & strWebSendRes & ";" & FullPostURL)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub

    Public Function ValidateServerCertificate(ByVal sender As Object, ByVal cert As X509Certificate, ByVal chain As X509Chain, _
    ByVal ssl As SslPolicyErrors) As Boolean
        Return True
    End Function

    'Public Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
    '    Try
    '        Dim encoding As New System.Text.ASCIIEncoding
    '        Dim B As Byte() = Nothing
    '        Dim response As Byte() = Nothing
    '        Dim strRes As String = ""
    '        ' Dim Data() As Byte = encoding.GetBytes(PostData)
    '        'Dim LoginReq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(URL), Net.HttpWebRequest)
    '        'With LoginReq
    '        '    .KeepAlive = False
    '        '    .Method = "POST"
    '        '    .ContentType = "application/x-www-form-urlencoded"
    '        '    .ContentLength = Data.Length
    '        '    .Timeout = PostURLTimeOut
    '        'End With

    '        'Dim SendReq As Stream = LoginReq.GetRequestStream
    '        'SendReq.Write(Data, 0, Data.Length)
    '        'SendReq.Close()

    '        'Dim LoginRes As System.Net.HttpWebResponse = CType(LoginReq.GetResponse(), Net.HttpWebResponse)
    '        'Dim HTML As String = ""
    '        'Using sReader As StreamReader = New StreamReader(LoginRes.GetResponseStream)
    '        '    HTML = sReader.ReadToEnd
    '        'End Using

    '        'HTML = HTML.Trim


    '        'LoginRes.Close()
    '        'LoginRes = Nothing

    '        'Return HTML.Trim

    '        Using web As New System.Net.WebClient
    '            Dim postURL = URL & "?" & PostData.ToString()
    '            web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
    '            B = System.Text.Encoding.ASCII.GetBytes(PostData.ToString())
    '            '  logger.Info("[URL called]-" & postURL)
    '            'response = web.DownloadData(postURL)
    '            response = web.UploadData(URL, "POST", B)
    '            strRes = System.Text.Encoding.ASCII.GetString(response)

    '            'Dim pJson As IDictionary = Json.JsonConvert.DeserializeObject(Of IDictionary)(strRes)
    '            '''Dim j As String = ""
    '            '''For Each i As String In pJson
    '            '''    j = j & ";" & i(0).ToString
    '            '''Next

    '            logger.Info("[MT POST] Result:" & response.ToString & ";" & strRes)
    '            'If pJson.Contains("status") Then
    '            '    Return pJson.Item("status").ToString
    '            'Else
    '            '    Return "Fail"
    '            'End If

    '            Return RemoveExtraNewLine(strRes).Replace(" ", String.Empty).Replace(vbLf, String.Empty)

    '        End Using
    '    Catch ex As Exception
    '        logger.Fatal("URL=" & URL & "?" & PostData)
    '        logger.Fatal("[FATAL]", ex)
    '        Return "Fail"
    '    End Try
    'End Function

    Public Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
        Try
            If Left(URL.ToLower, 5) = "https" Then
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
                ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)
            End If

            'Dim B As Byte() = Nothing
            'Dim response As Byte() = Nothing
            Dim HTML As String = ""

            Dim pURL As String = URL & "?" & PostData

            'Using web As New System.Net.WebClient
            '    web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
            '    B = System.Text.Encoding.ASCII.GetBytes(PostData)
            '    response = web.UploadData(URL, "GET", B)
            '    HTML = System.Text.Encoding.ASCII.GetString(response)
            'End Using
            '         System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
            'req.Proxy = new System.Net.WebProxy(ProxyString, true); //true means no proxy
            'System.Net.WebResponse resp = req.GetResponse();
            'System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            'return sr.ReadToEnd().Trim();

            Dim req = System.Net.WebRequest.Create(pURL)
            Dim resp As WebResponse = req.GetResponse()
            Dim sr As New StreamReader(resp.GetResponseStream())

            Dim pResult As String = sr.ReadToEnd().Trim().ToString

            logger.Info("[MT POST] Result=" & pResult & " ;URL= " & URL & "?" & PostData)

            Return pResult
        Catch ex As Exception
            logger.Fatal(URL & ";" & PostData)
            logger.Fatal("[FATAL] Message : " & ex.Message)
            logger.Fatal("[FATAL] Source : " & ex.Source)
            logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)
            Return "Fail"
        End Try
    End Function

    Private Sub send2Death(ByVal obj As SendSMSAPIStrc, ByVal resp As String, ByVal strWebSendRes As String, ByVal pMTID As String)
        Dim TempRetryCount As Integer = 0
        TempRetryCount = obj.RetryCount + 1

        If TempRetryCount <= CInt(RetryCount) Then
            obj.RetryCount = TempRetryCount
            SendToDeathQueue(death_queue, obj)
        Else
            logger.Error("[ERROR]" & getObjInfo(obj) & "; " & strWebSendRes)
            send2Insert(obj, resp, pMTID, MTStatus._Fail)
        End If
    End Sub

    Private Sub send2Insert(ByVal obj As SendSMSAPIStrc, ByVal MTResponse As String, ByVal pMTID As String, ByVal Status As Integer)

        'Dim Tbl_Outbox As New Tbl_Outbox
        'With Tbl_Outbox
        '    .Charge = obj.Charge
        '    .ContentURL = obj.URL
        '    .CPID = obj.CPID
        '    .KeywordID = obj.KeywordID
        '    .Message = obj.Message
        '    .MessageTypeID = obj.MsgType
        '    .MsgCount = obj.MsgCount
        '    .MsgGUID = obj.MsgGUID
        '    .MSISDN = obj.MSISDN
        '    .ReceiveDate = obj.ReceiveDate
        '    .RetryCount = obj.RetryCount
        '    .ShortCode = obj.ShortCode
        '    .StatusCode = MTResponse
        '    .TelcoID = obj.TelcoID
        '    .TransactionID = obj.TransactionID
        '    .SendDate = obj.SendDate
        '    .Status = Status
        '    .MTID = pMTID
        'End With
        'SendToINsertQueue(Insert_queue, Tbl_Outbox)

        Try
            Dim sql As String = "INSERT INTO premium_sms_cp.mt_egg_" & Format(Now, "yyyyMM") & "_" & GetWeekOfMonth(Now) & " (id,telco_id,shortcode,cp_id,keyword_id,msisdn,charge,content,content_type,wappush_url,data_coding,retry,mo_id,mt_id,mt_response_code,mt_error_code,mt_error_desc,timestamp,dn_code,dn_desc,dn_timestamp,remark) VALUES (0,'" & obj.TelcoID & "','" & obj.ShortCode & "','" & obj.CPID & "','" & obj.KeywordID & "','" & obj.MSISDN & "','" & obj.Charge & "','" & obj.Message & "','1','1','0','" & obj.RetryCount & "','','" & obj.TransactionID & "','" & MTResponse & "','','',now(),'','',0,'" & obj.MsgGUID & "')"

            Dim result As Integer = -1
            logger.Info("[INSERT EGG MT]: " & sql)

            result = sendToMySqlDB(sql)
            logger.Info("[INSERT EGG MT STATUS]: " & result)
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Sub SendToDeathQueue(ByVal destination As String, ByVal obj As LibraryDAL.SendSMSAPIStrc)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

    Private Sub SendToINsertQueue(ByVal destination As String, ByVal obj As LibraryDAL.Tbl_Outbox)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

    Public Function getObjInfo(ByVal objMessage As SendSMSAPIStrc) As String
        Dim myText As String = ""
        myText = "CPID=" + objMessage.CPID.ToString + ";" & _
                "MSISDN=" + objMessage.MSISDN + ";" & _
                "ContentURL=" + objMessage.URL + ";" & _
                "MsgGUID=" + objMessage.MsgGUID + ";" & _
                "ShortCode=" + objMessage.ShortCode + ";" & _
                "MessageType=" + objMessage.MsgType.ToString + ";" & _
                "MsgCharge=" + objMessage.Charge.ToString + ";" & _
                "KeywordID=" + objMessage.KeywordID.ToString + ";" & _
                "MsgCount=" + objMessage.MsgCount.ToString + ";" & _
                "RetryCount=" + objMessage.RetryCount.ToString + ";" & _
                "TransactionID=" + objMessage.TransactionID + ";" & _
                "TelcoID=" + objMessage.TelcoID.ToString() + ";" & _
                "ReceiveDate=" + objMessage.ReceiveDate.ToString() + ";" & _
                "SendDate=" + objMessage.SendDate.ToString() + ";" & _
                "Message=" + objMessage.Message & ";"
        Return myText
    End Function

    Private Sub send2unsub(ByVal pKeywordID As Integer, ByVal pMSISDN As String, ByVal pTransactionID As String)
        Dim UnSub As New UnsubUserStrc
        With UnSub
            .KeywordID = pKeywordID
            .MSISDN = pMSISDN
            .TransactionID = pTransactionID
        End With

        Using q As New MessageQueue(unsub_queue, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(UnSub, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub
    'sum 29/11/2011 remove extra new line
    Public Function RemoveExtraNewLine(ByVal sText As String) As String
        Dim sPattern, sReplaceText As String
        sPattern = "\r\n"
        sReplaceText = String.Empty
        Return Regex.Replace(sText, sPattern, sReplaceText)
    End Function

    Private Function sendToMySqlDB(ByVal sql As String) As Integer
        Dim result As Integer = -1
        Using mysqlCon As New MySqlConnection(mySqlConStr)
            mysqlCon.Open()
            Using mysqlCmd As New MySqlCommand(sql, mysqlCon)
                mysqlCmd.CommandTimeout = 5 * 60 * 1000
                result = mysqlCmd.ExecuteNonQuery
                mysqlCmd.Connection.Close()
            End Using
            mysqlCon.Close()
        End Using
        Return result
    End Function

    Private Function GetWeekOfMonth(ByVal [date] As DateTime) As Integer
        Dim beginningOfMonth As New DateTime([date].Year, [date].Month, 1)

        While [date].[Date].AddDays(1).DayOfWeek <> CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
            [date] = [date].AddDays(1)
        End While

        Return CInt(Math.Truncate(CDbl([date].Subtract(beginningOfMonth).TotalDays) / 7.0F)) + 1
    End Function
End Class


