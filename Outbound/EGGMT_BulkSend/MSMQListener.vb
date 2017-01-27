
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

Public Delegate Sub AddToDelegate(ByVal body As SendSMSAPIStrc)


Public Class MSMQListener

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
    Private bulkqueue As String = ConfigurationManager.AppSettings("bulkqueue")
    Private URL_Subscription As String = ConfigurationManager.AppSettings("Subscription_URL")
    Private URL_IOD As String = ConfigurationManager.AppSettings("IOD_URL")
    Private RetryCount As String = ConfigurationManager.AppSettings("RetryCount")
    Private PostURLTimeOut As Integer = CInt(ConfigurationManager.AppSettings("PostURLTimeOut"))
    Private RetryResponseCode As String = ConfigurationManager.AppSettings("RetryResponseCode")
    Private SuccessResponseCode As String = ConfigurationManager.AppSettings("SuccessResponseCode")
    Private UnsubResponseCode As String = ConfigurationManager.AppSettings("UnsubResponseCode")

    Private timeout As String = ConfigurationManager.AppSettings("timeout")

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
        Dim ChargedStatus As String = ""

        Dim pTransactionID As String = ""

        logger.Debug("[Sleep - Posting speed control] miliseconds=" & timeout & "Date=" & Date.Now().ToString("dd/MM/yyyy H:mm:ss FFFFF"))

        Thread.Sleep(CInt(timeout))
        logger.Debug("[Sleep - Posting speed control After] Date=" & Date.Now().ToString("dd/MM/yyyy H:mm:ss FFFFF"))
        Try
            'logger.Debug("[start]")
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)


            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTCSD = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using
            'logger.Debug("[After strTCSD]")
            MKCollection.Add("number", objMessage.MSISDN)
            MKCollection.Add("rrn", objMessage.TransactionID)
            MKCollection.Add("message", objMessage.Message)
            MKCollection.Add("telco", EGG_GetTelcoDesc(objMessage.TelcoID)) 'MSISDN
            MKCollection.Add("tariff", strTCSD)

            If objMessage.Charge = 0 Then
                MKCollection.Add("charged", "FALSE")
                objMessage.Charged = "FALSE"
            Else
                MKCollection.Add("charged", "TRUE")
                objMessage.Charged = "TRUE"
            End If



            'logger.Debug("[before post] rrn=" & objMessage.rrn & ";Transid=" & objMessage.TransactionID)
            PostURL = URL_IOD
            MTID = objMessage.rrn
            If objMessage.KeywordType <> KeywordType._IOD Then ' rrn id for Subscription is generate by us
                PostURL = URL_Subscription ' different URL for IOD and sub
                logger.Debug("Existed ID]" & MTID)
                If objMessage.TransactionID = "" Then
                    MTID = GetUniqueNumber()
                    objMessage.rrn = MTID
                    logger.Debug("[Generate new ID]" & MTID)
                End If
                MKCollection.Set("rrn", MTID)
                objMessage.TransactionID = MTID
            End If

            'logger.Debug("[Split msisdn]")
            '24/04/2013,A,To determine the transaction it is single send or bulk send
            Dim pMSISDN As String = objMessage.MSISDN
            Dim pMSISDNCount As Integer = pMSISDN.Split(CChar(",")).Count

            'If is more than 1 mean is bulk send and need to add mode=bulk parameters
            If pMSISDNCount > 1 Then
                MKCollection.Add("mode", "Bulk")
                objMessage.Mode = "Bulk"
            End If

            objMessage.SendDate = Date.Now

            'pTransactionID = MTID

            strColl = ""      ' reset the strCOLL
            For ai = 0 To MKCollection.Count - 1
                strColl = strColl & MKCollection.GetKey(ai) & "=" & System.Web.HttpUtility.UrlEncode(MKCollection.Get(ai)) & "&"
            Next
            strColl = strColl.Remove(strColl.Length - 1, 1)

            ' logger.Debug("[Before Send]" & strColl)
            strWebSendRes = WebRequest(PostURL, strColl)

            FullPostURL = PostURL & "?" & strColl
            logger.Debug("[After Send]" & strWebSendRes & ";URL=" & FullPostURL)
            'logger.Debug("[Result]" & strWebSendRes)
            '24/04/2013,A, Single send and Bulk send have different method to insert data to db

            If pMSISDNCount > 1 Then
                Dim pMSISDNList As String() = pMSISDN.Split(CChar(","))

                If strWebSendRes = "FAIL" Then
                    send2Death(objMessage, "", strWebSendRes, MTID)
                    logger.Info("[FAIL]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                Else


                    '18/03/2013,A, Change to detect the result value contain the MSISDN of the MT, if yes mean is success
                    If SuccessResponseCode.Contains("," & strWebSendRes & ",") Then  'Success
                        SendToBulkQueue(bulkqueue, objMessage)

                        '24/07/2013,A, Seperated the insert function to test the speed
                        'For Each _List In pMSISDNList
                        '    logger.Debug("[Insert MT to outbox] post result=" & strWebSendRes & ";MSisdn=" & _List)

                        '    objMessage.MSISDN = _List.Trim 'Replace the MSISDN from a list to single MSISDN
                        '    objMessage.TransactionID = MTID '& "-" & _List.Trim 'Special case for EGG MT bulksend, the MTID are TransactionID-MSISDN

                        '    send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Success)
                        '    logger.Debug("[SUCCESS]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                        logger.Info("[SUCCESS]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                        'Next

                    Else
                        send2Death(objMessage, "", strWebSendRes, MTID)
                        'send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Fail) 'Other Error
                        'logger.Debug("[ERROR]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                        'logger.Error("[ERROR]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                        logger.Info("[ERROR]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                    End If
                End If
            Else
                If strWebSendRes = "FAIL" Then
                    send2Death(objMessage, "", strWebSendRes, MTID)
                Else
                    '03/06/2013,A, Success MT status still mantain as "200"
                    'If strWebSendRes.Contains(objMessage.MSISDN) Then
                    '    send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Success)
                    '    logger.Info("[SUCCESS]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                    'Else
                    '    send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Fail) 'Other Error
                    '    logger.Error("[ERROR]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                    'End If

                    '18/03/2013,A, Change to detect the result value contain the MSISDN of the MT, if yes mean is success
                    If SuccessResponseCode.Contains("," & strWebSendRes & ",") Then  'Success
                        send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Success)
                        logger.Info("[SUCCESS]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                    Else
                        send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Fail) 'Other Error
                        logger.Error("[ERROR]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
                    End If
                End If
            End If
            logger.Debug("[Checking - 1 ]" & getObjInfo(objMessage) & "::MT ID=" & MTID)
            logger.Debug("[Checking - 2 ]" & FullPostURL)

        Catch ex As Exception
            send2Death(objMessage, "", "", MTID)
            logger.Debug("[FATAL]" & strWebSendRes & ";" & FullPostURL)
            logger.Fatal("[FATAL]" & strWebSendRes & ";" & FullPostURL)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub


    'Private Sub FireRecieveEvent2(ByVal objMessage As SendSMSAPIStrc)
    '    Dim MKCollection As New NameValueCollection
    '    Dim strMessage As String = ""
    '    Dim strDateTime As String = ""
    '    Dim strResultpost As String = ""
    '    Dim strColl As String = ""
    '    Dim strWebSendRes As String = ""
    '    Dim strTempWebSendRes As String = ""
    '    Dim strTCSD As String = ""
    '    Dim PostURL As String
    '    Dim MTID As String = ""
    '    Dim FullPostURL As String = ""
    '    Dim ChargedStatus As String = "True"

    '    Try

    '        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
    '        ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)


    '        Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
    '            strTCSD = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
    '        End Using

    '        'http://64.49.216.30/services/egg/apps/listener_bulk_capi.jsp?number=09267351234,09267351234&rrn=201302281432&message=test&telco=GLOBE
    '        '&tariff=CHERRY250_EGGPUSH&mode=bulk&charged=true


    '        MKCollection.Add("number", objMessage.MSISDN)
    '        MKCollection.Add("rrn", objMessage.TransactionID)
    '        MKCollection.Add("message", objMessage.Message)
    '        MKCollection.Add("telco", EGG_GetTelcoDesc(objMessage.TelcoID)) 'MSISDN
    '        MKCollection.Add("tariff", strTCSD)
    '        MKCollection.Add("charged", ChargedStatus)

    '        PostURL = URL_IOD
    '        MTID = objMessage.TransactionID
    '        If objMessage.KeywordType <> KeywordType._IOD Then ' rrn id for Subscription is generate by us
    '            PostURL = URL_Subscription ' different URL for IOD and sub
    '            MTID = GetUniqueNumber()
    '            MKCollection.Set("rrn", MTID)
    '        End If

    '        objMessage.SendDate = Date.Now
    '        strColl = ""      ' reset the strCOLL
    '        For ai = 0 To MKCollection.Count - 1
    '            strColl = strColl & MKCollection.GetKey(ai) & "=" & MKCollection.Get(ai) & "&"
    '        Next
    '        strColl = strColl.Remove(strColl.Length - 1, 1)

    '        logger.Info("[Before Send]" & strColl)
    '        strWebSendRes = WebRequest(PostURL, strColl)

    '        FullPostURL = PostURL & "?" & strColl
    '        logger.Info("[After Send]" & FullPostURL)
    '        '13/03/2013,A, success transaction consist of RRN-mobtel (format) example 201312031108-902389123
    '        If strWebSendRes = "" Then
    '            send2Death(objMessage, "", strWebSendRes, MTID)
    '        Else

    '            'If13/03/3013,A, if return value contain mobtel consider as success
    '            If strWebSendRes.Contains(objMessage.MSISDN) Then  'Success
    '                send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Success)
    '                logger.Info("[SUCCESS]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
    '            Else
    '                send2Insert(objMessage, strWebSendRes, MTID, MTStatus._Fail) 'Other Error
    '                logger.Error("[ERROR]" & getObjInfo(objMessage) & ";" & FullPostURL & ";" & strWebSendRes)
    '            End If
    '        End If

    '    Catch ex As Exception
    '        send2Death(objMessage, "", "", MTID)
    '        logger.Fatal("[FATAL]" & strWebSendRes & ";" & FullPostURL)
    '        logger.Fatal("[FATAL]", ex)
    '    End Try
    'End Sub

    Public Function ValidateServerCertificate(ByVal sender As Object, ByVal cert As X509Certificate, ByVal chain As X509Chain, _
    ByVal ssl As SslPolicyErrors) As Boolean
        Return True
    End Function

    Public Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
        Try
            Dim encoding As New System.Text.ASCIIEncoding
            Dim B As Byte() = Nothing
            Dim response As Byte() = Nothing
            Dim strRes As String = ""
            ' Dim Data() As Byte = encoding.GetBytes(PostData)
            'Dim LoginReq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(URL), Net.HttpWebRequest)
            'With LoginReq
            '    .KeepAlive = False
            '    .Method = "POST"
            '    .ContentType = "application/x-www-form-urlencoded"
            '    .ContentLength = Data.Length
            '    .Timeout = PostURLTimeOut
            'End With

            'Dim SendReq As Stream = LoginReq.GetRequestStream
            'SendReq.Write(Data, 0, Data.Length)
            'SendReq.Close()

            'Dim LoginRes As System.Net.HttpWebResponse = CType(LoginReq.GetResponse(), Net.HttpWebResponse)
            'Dim HTML As String = ""
            'Using sReader As StreamReader = New StreamReader(LoginRes.GetResponseStream)
            '    HTML = sReader.ReadToEnd
            'End Using

            'HTML = HTML.Trim


            'LoginRes.Close()
            'LoginRes = Nothing

            'Return HTML.Trim

            Using web As New System.Net.WebClient
                Dim postURL = URL & "?" & PostData.ToString()
                web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
                B = System.Text.Encoding.ASCII.GetBytes(PostData.ToString())
                '  logger.Info("[URL called]-" & postURL)
                response = web.DownloadData(postURL)
                'response = web.UploadData(URL, "POST", B)
                'response = web.UploadData(postURL, "GET", B)
                strRes = System.Text.Encoding.ASCII.GetString(response)

                Return RemoveExtraNewLine(strRes).Replace(" ", String.Empty).Replace(vbLf, String.Empty)

            End Using
        Catch ex As Exception
            logger.Fatal("URL=" & URL & "?" & PostData)
            logger.Fatal("[FATAL]", ex)
            Return "Fail"
        End Try
    End Function

    Private Sub send2Death(ByVal obj As SendSMSAPIStrc, ByVal resp As String, ByVal strWebSendRes As String, ByVal pMTID As String)
        Dim TempRetryCount As Integer = 0
        TempRetryCount = obj.RetryCount + 1

        '11/07/2013,A, Temporary do unlimited retry
        'If TempRetryCount <= CInt(RetryCount) Then
        obj.RetryCount = TempRetryCount
        SendToDeathQueue(death_queue, obj)
        'Else
        '    logger.Error("[ERROR]" & getObjInfo(obj) & "; " & strWebSendRes)
        '    send2Insert(obj, resp, pMTID, MTStatus._Fail)
        'End If
    End Sub

    Private Sub send2Insert(ByVal obj As SendSMSAPIStrc, ByVal MTResponse As String, ByVal pMTID As String, ByVal Status As Integer)

        Dim Tbl_Outbox As New Tbl_Outbox
        With Tbl_Outbox
            .Charge = obj.Charge
            .ContentURL = obj.URL
            .CPID = obj.CPID
            .KeywordID = obj.KeywordID
            .Message = obj.Message
            .MessageTypeID = obj.MsgType
            .MsgCount = obj.MsgCount
            .MsgGUID = obj.MsgGUID
            .MSISDN = obj.MSISDN
            .ReceiveDate = obj.ReceiveDate
            .RetryCount = obj.RetryCount
            .ShortCode = obj.ShortCode
            .StatusCode = MTResponse '11/04/2013,A,From this day onwards this column will be used to stored DN respnse (Egg only)
            .TelcoID = obj.TelcoID
            .TransactionID = obj.rrn
            .SendDate = obj.SendDate
            .Status = Status
            .MTID = obj.TransactionID
        End With
        SendToINsertQueue(Insert_queue, Tbl_Outbox)
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

    Private Sub SendToBulkQueue(ByVal destination As String, ByVal obj As LibraryDAL.SendSMSAPIStrc)
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
                "mode=" + objMessage.Mode + ";" & _
                "charged=" + objMessage.Charged & ";" + ";" & _
                "rrn=" + objMessage.rrn & ";"
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
End Class


