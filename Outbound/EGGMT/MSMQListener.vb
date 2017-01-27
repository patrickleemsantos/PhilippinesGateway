
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
Imports MySql.Data.MySqlClient

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
    Private URL_Subscription As String = ConfigurationManager.AppSettings("Subscription_URL")
    Private URL_IOD As String = ConfigurationManager.AppSettings("IOD_URL")
    Private RetryCount As String = ConfigurationManager.AppSettings("RetryCount")
    Private PostURLTimeOut As Integer = CInt(ConfigurationManager.AppSettings("PostURLTimeOut"))
    Private RetryResponseCode As String = ConfigurationManager.AppSettings("RetryResponseCode")
    Private SuccessResponseCode As String = ConfigurationManager.AppSettings("SuccessResponseCode")
    Private UnsubResponseCode As String = ConfigurationManager.AppSettings("UnsubResponseCode")

    Private UserName As String = ConfigurationManager.AppSettings("UserName")
    Private Password As String = ConfigurationManager.AppSettings("Password")

    Private conn As String = ConfigurationManager.AppSettings("mysql")
    Private dbqueue As String = ConfigurationManager.AppSettings("dbqueue")

    Private magentConn As MySqlConnection
    Private MT_URL As String = ConfigurationManager.AppSettings("MT_URL")

    Private MT_MODE As String = ConfigurationManager.AppSettings("MT_MODE")

    Private TelcoCPID As String

    Public Delegate Function ServerCertificateValidationCallback(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New(ByVal queuePath As String)
        'magentConn = New MySqlConnection(conn)
        'magentConn.Open()
        _queue = New MessageQueue(queuePath)
        MessageQueue.EnableConnectionCache = False
    End Sub

    Public Sub Start()
        del = New AddToDelegate(AddressOf sendToURLQueue) 'FireRecieveEvent
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

    Private Sub sendToURLQueue(ByVal objMessage As SendSMSAPIStrc)
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

        Try

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)


            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTCSD = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using

            Dim rrn As String = objMessage.TransactionID
            MKCollection.Add("number", objMessage.MSISDN)
            MKCollection.Add("rrn", objMessage.TransactionID)
            MKCollection.Add("message", objMessage.Message)
            MKCollection.Add("telco", EGG_GetTelcoDesc(objMessage.TelcoID)) 'MSISDN
            MKCollection.Add("tariff", strTCSD)
            'MKCollection.Add("charged", ChargedStatus)

            Dim charged As String = ""
            If objMessage.Charge = 0 Then
                MKCollection.Add("charged", "FALSE")
                charged = "FALSE"
            Else
                MKCollection.Add("charged", "TRUE")
                charged = "TRUE"
            End If

            PostURL = URL_IOD
            MTID = objMessage.TransactionID
            If objMessage.KeywordType <> KeywordType._IOD Then ' rrn id for Subscription is generate by us
                PostURL = URL_Subscription ' different URL for IOD and sub
                MTID = GetUniqueNumber()
                MKCollection.Set("rrn", MTID)
                rrn = MTID
            End If

            objMessage.SendDate = Date.Now

            strColl = ""      ' reset the strCOLL
            For ai = 0 To MKCollection.Count - 1
                strColl = strColl & MKCollection.GetKey(ai) & "=" & System.Web.HttpUtility.UrlEncode(MKCollection.Get(ai)) & "&"
            Next
            strColl = strColl.Remove(strColl.Length - 1, 1)

            ' Dim sQuery As String = "insert into " & dbqueue & " values (0,'1','" & _
            'objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0',now(),'" & OA & "','" & objMessage.MSISDN & "','" & strTC & "','" & strSD & "','0','" & _
            'objMessage.Message & "','" & strDCS & "','" & ServiceID & "');"

            Dim sQuery As String = "insert into " & dbqueue & " values (0,'1','" & _
            objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0',now(),'" & objMessage.MSISDN & "','" & rrn & "','" & objMessage.Message.Replace("'", "") & "','" & EGG_GetTelcoDesc(objMessage.TelcoID) & "','" & strTCSD & "','" & _
            charged & "','" & MT_MODE & "');"

            'logger.Info("Entering " & sQuery)
            'Dim result As Integer = ExecuteCommand(sQuery)
            Dim result As String = ""
            strWebSendRes = MT_URL & "?telco=EGG&query=" & System.Web.HttpUtility.UrlEncode(sQuery)

            logger.Info("URL: " & strWebSendRes)
            Using web As New System.Net.WebClient
                result = web.DownloadString(strWebSendRes)
                'Dim res As Byte() = web.UploadValues(MT_URL, "POST", MKCollection)

                'result = System.Text.Encoding.ASCII.GetString(res)
                Try
                    web.Dispose()

                Catch ex As Exception
                    'logger.Info("Error Closing WebClient")
                End Try
            End Using

            logger.Info(result & "|" & sQuery)
            logger.Info(result & "|" & strWebSendRes)

            If result = "0" Or result = "" Then
                send2Death(objMessage, "", "", MTID)
            End If

        Catch ex As Exception
            send2Death(objMessage, "", "", MTID)

            logger.Fatal("[FATAL]" & strWebSendRes)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub

    Private Sub sendToDBQueue(ByVal objMessage As SendSMSAPIStrc)
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

        Try

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)


            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTCSD = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using

            Dim rrn As String = objMessage.TransactionID
            MKCollection.Add("number", objMessage.MSISDN)
            MKCollection.Add("rrn", objMessage.TransactionID)
            MKCollection.Add("message", objMessage.Message)
            MKCollection.Add("telco", EGG_GetTelcoDesc(objMessage.TelcoID)) 'MSISDN
            MKCollection.Add("tariff", strTCSD)
            'MKCollection.Add("charged", ChargedStatus)

            Dim charged As String = ""
            If objMessage.Charge = 0 Then
                MKCollection.Add("charged", "FALSE")
                charged = "FALSE"
            Else
                MKCollection.Add("charged", "TRUE")
                charged = "TRUE"
            End If

            PostURL = URL_IOD
            MTID = objMessage.TransactionID
            If objMessage.KeywordType <> KeywordType._IOD Then ' rrn id for Subscription is generate by us
                PostURL = URL_Subscription ' different URL for IOD and sub
                MTID = GetUniqueNumber()
                MKCollection.Set("rrn", MTID)
                rrn = MTID
            End If

            objMessage.SendDate = Date.Now

            strColl = ""      ' reset the strCOLL
            For ai = 0 To MKCollection.Count - 1
                strColl = strColl & MKCollection.GetKey(ai) & "=" & System.Web.HttpUtility.UrlEncode(MKCollection.Get(ai)) & "&"
            Next
            strColl = strColl.Remove(strColl.Length - 1, 1)

            ' Dim sQuery As String = "insert into " & dbqueue & " values (0,'1','" & _
            'objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0',now(),'" & OA & "','" & objMessage.MSISDN & "','" & strTC & "','" & strSD & "','0','" & _
            'objMessage.Message & "','" & strDCS & "','" & ServiceID & "');"

            Dim sQuery As String = "insert into " & dbqueue & " values (0,'1','" & _
            objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0',now(),'" & objMessage.MSISDN & "','" & rrn & "','" & objMessage.Message & "','" & EGG_GetTelcoDesc(objMessage.TelcoID) & "','" & strTCSD & "','" & _
            charged & "','bulk');"

            logger.Info("Entering " & sQuery)
            Dim result As Integer = ExecuteCommand(sQuery)
            logger.Info(result & "|" & sQuery)

            If result = -1 Then
                send2Death(objMessage, "", "", MTID)
                Try
                    'magentConn = New MySqlConnection(conn)
                    If magentConn.State = ConnectionState.Closed Or magentConn.State = ConnectionState.Broken Then
                        magentConn.Open()
                        logger.Info("Re-Open DB Connection.....")
                    End If


                Catch ex1 As Exception
                    logger.Fatal("[FATAL]", ex1)
                    logger.Info("Failed Re-Open DB Connection.....")
                End Try
            End If
            'objConnectPremium.Dispose()
        Catch ex As Exception
            send2Death(objMessage, "", "", MTID)
            Try
                'magentConn = New MySqlConnection(conn)
                If magentConn.State = ConnectionState.Closed Or magentConn.State = ConnectionState.Broken Then
                    magentConn.Open()
                    logger.Info("Re-Open DB Connection.....")
                End If


            Catch ex1 As Exception
                logger.Fatal("[FATAL]", ex1)
                logger.Info("Failed Re-Open DB Connection.....")
            End Try
            logger.Fatal("[FATAL]" & strWebSendRes)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub


    Public Function ExecuteCommand(ByVal sql As String) As Integer
        Dim result As Integer = -1
        Try
            'Using magentConn As New MySqlConnection(conn)
            'logger.Info("Opening... ")
            'magentConn.Open()
            'logger.Info("Open... ")
            Using objCommand As New MySqlCommand(sql, magentConn)
                result = objCommand.ExecuteNonQuery()
            End Using
            'magentConn.Close()
            'End Using
            Return result
        Catch ex As Exception
            Try
                'magentConn = New MySqlConnection(conn)
                If magentConn.State = ConnectionState.Closed Or magentConn.State = ConnectionState.Broken Then
                    magentConn.Open()
                    logger.Info("Re-Open DB Connection.....")
                End If


            Catch ex1 As Exception
                logger.Fatal("[FATAL]", ex1)
                logger.Info("Failed Re-Open DB Connection.....")
            End Try

            logger.Fatal("[FATAL] " & sql)
            logger.Fatal("[FATAL]", ex)
            Return result
        End Try
    End Function

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

        Try

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)


            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTCSD = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using

            MKCollection.Add("number", objMessage.MSISDN)
            MKCollection.Add("rrn", objMessage.TransactionID)
            MKCollection.Add("message", objMessage.Message)
            MKCollection.Add("telco", EGG_GetTelcoDesc(objMessage.TelcoID)) 'MSISDN
            MKCollection.Add("tariff", strTCSD)
            'MKCollection.Add("charged", ChargedStatus)

            If objMessage.Charge = 0 Then
                MKCollection.Add("charged", "FALSE")
            Else
                MKCollection.Add("charged", "TRUE")
            End If

            PostURL = URL_IOD
            MTID = objMessage.TransactionID
            If objMessage.KeywordType <> KeywordType._IOD Then ' rrn id for Subscription is generate by us
                PostURL = URL_Subscription ' different URL for IOD and sub
                MTID = GetUniqueNumber()
                MKCollection.Set("rrn", MTID)
            End If

            objMessage.SendDate = Date.Now

            strColl = ""      ' reset the strCOLL
            For ai = 0 To MKCollection.Count - 1
                strColl = strColl & MKCollection.GetKey(ai) & "=" & System.Web.HttpUtility.UrlEncode(MKCollection.Get(ai)) & "&"
            Next
            strColl = strColl.Remove(strColl.Length - 1, 1)

            strWebSendRes = WebRequest(PostURL, strColl)

            FullPostURL = PostURL & "?" & strColl

            If strWebSendRes = "FAIL" Then
                send2Death(objMessage, "", strWebSendRes, MTID)
            Else

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

        Catch ex As Exception
            send2Death(objMessage, "", "", MTID)
            logger.Fatal("[FATAL]" & strWebSendRes & ";" & FullPostURL)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub


    Private Sub FireRecieveEvent2(ByVal objMessage As SendSMSAPIStrc)
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
        Dim ChargedStatus As String = "True"

        Try

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)


            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTCSD = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using

            'http://64.49.216.30/services/egg/apps/listener_bulk_capi.jsp?number=09267351234,09267351234&rrn=201302281432&message=test&telco=GLOBE
            '&tariff=CHERRY250_EGGPUSH&mode=bulk&charged=true


            MKCollection.Add("number", objMessage.MSISDN)
            MKCollection.Add("rrn", objMessage.TransactionID)
            MKCollection.Add("message", objMessage.Message)
            MKCollection.Add("telco", EGG_GetTelcoDesc(objMessage.TelcoID)) 'MSISDN
            MKCollection.Add("tariff", strTCSD)
            MKCollection.Add("charged", ChargedStatus)

            PostURL = URL_IOD
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

            '13/03/2013,A, success transaction consist of RRN-mobtel (format) example 201312031108-902389123
            If strWebSendRes = "" Then
                send2Death(objMessage, "", strWebSendRes, MTID)
            Else

                'If13/03/3013,A, if return value contain mobtel consider as success
                If strWebSendRes.Contains(objMessage.MSISDN) Then  'Success
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

        'If TempRetryCount <= CInt(RetryCount) Then
        obj.RetryCount = TempRetryCount
        SendToDeathQueue(death_queue, obj)
        'Else
        'logger.Error("[ERROR]" & getObjInfo(obj) & "; " & strWebSendRes)
        'send2Insert(obj, resp, pMTID, MTStatus._Fail)
        ' End If
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
            .StatusCode = "" 'MTResponse 11/04/2013,A,From this day onwards this column will be used to stored DN respnse (Egg only)
            .TelcoID = obj.TelcoID
            .TransactionID = obj.TransactionID
            .SendDate = obj.SendDate
            .Status = Status
            .MTID = pMTID
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
End Class


