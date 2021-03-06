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
    Private retry_queue As String = ConfigurationManager.AppSettings("retry_queue")
    Private url As String = ConfigurationManager.AppSettings("url")
    Private url2 As String = ConfigurationManager.AppSettings("url2")
    Private RetryCount As String = ConfigurationManager.AppSettings("RetryCount")
    Private RetryResponseCode As String = ConfigurationManager.AppSettings("RetryResponseCode")
    Private InsufficientResponseCode As String = ConfigurationManager.AppSettings("InsufficientResponseCode")
    Private SuccessResponseCode As String = ConfigurationManager.AppSettings("SuccessResponseCode")
    Private UnsubResponseCode As String = ConfigurationManager.AppSettings("UnsubResponseCode")
    Private ForwardDNQueue As String = ConfigurationManager.AppSettings("ForwarDNQueue")
    Private ForwardMOQueue As String = ConfigurationManager.AppSettings("ForwardMO_queue")
    Private ErrorRespCode As String = ConfigurationManager.AppSettings("ErrorRespCode")

    Private conn As String = ConfigurationManager.AppSettings("mysql")
    Private dbqueue As String = ConfigurationManager.AppSettings("dbqueue")
    Private magentConn As MySqlConnection
    Private MT_URL As String = ConfigurationManager.AppSettings("MT_URL")

    Private UserName As String = ConfigurationManager.AppSettings("UserName")
    Private Password As String = ConfigurationManager.AppSettings("Password")

    Private TelcoCPID As String

    Public Delegate Function ServerCertificateValidationCallback(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New(ByVal queuePath As String)

        'magentConn = New MySqlConnection(conn)
        'magentConn.Open()
        _queue = New MessageQueue(queuePath)
        MessageQueue.EnableConnectionCache = False
      
    End Sub

    Public Sub [Stop]()
        _listen = False
        'Try
        '    magentConn.Close()
        'Catch ex As Exception

        'End Try

        RemoveHandler _queue.PeekCompleted, AddressOf OnPeekCompleted

    End Sub

    Public Sub Start()
        del = New AddToDelegate(AddressOf sendToURLQueue) 'sendToURLQueue 'sendToDBQueue 'FireRecieveEvent
        _listen = True
        AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted

        StartListening()
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
        Dim res As Byte()
        Dim ServiceID As String
        Dim SendDate As DateTime
        Dim Flag As Integer
        Dim strSD As String = ""
        Dim strTC As String = ""
        Dim MTResponseEC As String = ""
        Dim PostSmartMT As Boolean = True

        Dim MTResponseBackList As New ArrayList

        Dim dt As DataSet

        Try
       
            Dim MessageList As ArrayList
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)

            Using ShortCodeCRUD As New ShortCodeCRUD
                TelcoCPID = ShortCodeCRUD.GetShortCodeUserID(objMessage.ShortCode, objMessage.TelcoID)
            End Using

            Using KeywordCRUD As New KeywordCRUD
                dt = KeywordCRUD.GetKeywordShotcodeSD(objMessage.ShortCode, objMessage.KeywordID, objMessage.TelcoID)

                For Each d As DataRow In dt.Tables(0).Rows
                    ServiceID = d.Item("ServiceID").ToString
                    strSD = d.Item("ServiceDescription").ToString
                Next
            End Using

            If objMessage.Charge = 0 Then
                strSD = "00"
            End If

            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTC = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using


            Dim OA As String = ""

            If objMessage.KeywordType = KeywordType._IOD Or objMessage.KeywordType = KeywordType._Conditional Then
                OA = objMessage.ShortCode & TelcoCPID & objMessage.TransactionID
            Else  ' subscrrtion service without moid
                OA = objMessage.ShortCode & TelcoCPID
            End If

            MKCollection.Add("UNM", UserName)
            MKCollection.Add("PWD", Password)
            MKCollection.Add("OA", OA) 'Format  IOD = Access Code + CP ID(3digits) + RRN (12 digits) , Subs= Access Code + CP ID(3digits)
            MKCollection.Add("DA", objMessage.MSISDN) 'MSISDN
            MKCollection.Add("TC", strTC) ' Tariff , Charge Code
            MKCollection.Add("SD", strSD) 'Service Description 
            MKCollection.Add("PID", "0") ' Protocol Identifier just pust it as 0
            MKCollection.Add("UD", "")
            'MKCollection.Add("SCT", "") 'Time of message arrival  no need to include

            'Binary Type  81 – Ring tone,82 – Logo ,8A – Picture Message
            'DCS -Data Coding Scheme 0- Normal SMS, 245 - BInary SMS
            If objMessage.MsgType = MsgType.TextSMS Then 'Regular Text Message

                MessageList = CompileTextSMS(objMessage.Message, 160)
                MKCollection.Add("DCS", "0")

            ElseIf objMessage.MsgType = MsgType.Logo Then

                MessageList = CompileBinarySMS("82", objMessage.Message, 280)
                MKCollection.Add("DCS", "245")

            ElseIf objMessage.MsgType = MsgType.PictureMessage Then

                MessageList = CompileBinarySMS("81", objMessage.Message, 280)
                MKCollection.Add("DCS", "245")

            ElseIf objMessage.MsgType = MsgType.RingTone Then

                MessageList = CompileBinarySMS("8A", objMessage.Message, 280)
                MKCollection.Add("DCS", "245")

            End If

            ' for subs based mt have to pass ANA - Service ID
            If objMessage.KeywordType <> KeywordType._IOD Then
                MKCollection.Add("ANA", ServiceID)
            End If

            Using web As New System.Net.WebClient
                web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")

                Dim MTCount As Integer = 1


                For Each d As SmartMessageFormat In MessageList


                    'If first mt post get return error or insufficient bln, second part also stop to post
                    If PostSmartMT = False Then
                        Exit For
                    End If

                    Dim MTResponseBack As New MTResponse
                    strTempWebSendRes = ""
                    Flag = 1
                    SendDate = Date.Now
                    MKCollection.Set("UD", d._UD)

                    If d._UDH <> "" Then
                        MKCollection.Remove("UDH") ' remove first before add to avoid duplicate UDH
                        MKCollection.Add("UDH", d._UDH)
                    End If


                    'for second part above message tcsd has to set 00 value - smart rules
                    If MTCount > 1 Then
                        MKCollection.Set("TC", "00")
                        MKCollection.Set("SD", "00")
                    End If


                    ' MKCollection.Set("SCT", SendDate.ToString("yyyymmddhhmmss")) 'Time of message arrival no need to include
                    objMessage.SendDate = SendDate

                    strColl = ""      ' reset the strCOLL
                    For ai = 0 To MKCollection.Count - 1
                        strColl = strColl & MKCollection.GetKey(ai) & "=" & System.Web.HttpUtility.UrlEncode(MKCollection.Get(ai)) & "&"
                    Next


                    strColl = strColl.Remove(strColl.Length - 1, 1)

                    '11012013, A
                    'To balance dividing the MT send to two different URL
                    'Logic here used is determine by the last digit of the MSISDN, if is odd then url, if is even then url2
                    Dim pDigit As Integer = Convert.ToInt32(objMessage.MSISDN.Substring(Len(objMessage.MSISDN) - 1))

                    Dim SmartMTURL As String = ""

                    If pDigit Mod 2 = 0 Then
                        strTempWebSendRes = "URL=" & url2 & "?" & strColl

                        SmartMTURL = url2 & "?" & strColl
                    Else
                        strTempWebSendRes = "URL=" & url & "?" & strColl

                        SmartMTURL = url & "?" & strColl
                    End If

                    'post to Smart 
                    Try

                        strWebSendRes = strWebSendRes & " || " & strTempWebSendRes

                        web.Headers.Add("Content-Type", " ")
                        res = web.DownloadData(SmartMTURL)
                        strResultpost = System.Text.Encoding.ASCII.GetString(res)
                        strWebSendRes = strWebSendRes & ";RESULT=" & strResultpost

                        MTResponseBack._EC = "200" ' Success Return
                        PostSmartMT = True

                    Catch webex As WebException
                        Dim httpResp As HttpWebResponse = DirectCast(webex.Response, HttpWebResponse)
                        If Not httpResp Is Nothing Then
                            Dim streamReader = New StreamReader(webex.Response.GetResponseStream())
                            Dim MTFailReturn As String = streamReader.ReadToEnd()
                            strWebSendRes = "URL=" & strTempWebSendRes & ";RESULT=" & httpResp.StatusCode & ";Response=" & MTFailReturn

                            MTFailReturn = MTFailReturn.Replace("<br>", "") ' because return contain <br> so replace with empty space
                            MTResponseBack = GetMTFailResponse(MTFailReturn)
                        Else
                            strWebSendRes = "URL=" & strTempWebSendRes & ";RESULT=" & webex.Message
                        End If

                        PostSmartMT = False
                    Catch ex As Exception
                        logger.Fatal("[FATAL]" & strWebSendRes)
                        logger.Fatal("[FATAL]", ex)
                        Flag = 0
                        PostSmartMT = False
                    End Try

                    MTResponseBackList.Add(MTResponseBack)

                    MTCount = MTCount + 1
                Next

                'get the message count
                objMessage.MsgCount = MessageList.Count


                Dim MTRespCount As Integer = 1
                For Each d As MTResponse In MTResponseBackList

                    If MTRespCount = 1 Then
                        MTResponseEC = d._EC
                    End If

                    MTRespCount = MTRespCount + 1
                Next

                'logger.Info("[Checking MT Response]: " & strWebSendRes)
                If Flag = 1 Then
                    'Insert Into Queue
                    If SuccessResponseCode.Contains("," & MTResponseEC & ",") Then  'Success
                        send2Insert(objMessage, MTResponseEC, MTStatus._Success)
                        logger.Info("[SUCCESS]" & getObjInfo(objMessage) & "; " & strWebSendRes)
                    ElseIf RetryResponseCode.Contains("," & MTResponseEC & ",") Then  'Retry
                        sendToDBQueue(objMessage)
                        'send2Death(objMessage, MTResponseEC, strWebSendRes)
                    ElseIf UnsubResponseCode.Contains("," & MTResponseEC & ",") Then  ' Inactive The User
                        send2unsub(objMessage.KeywordID, objMessage.MSISDN, objMessage.TransactionID) ' send to queue to unsubs the user
                        send2Insert(objMessage, MTResponseEC, MTStatus._Fail)
                        logger.Error("[UNSUB]" & getObjInfo(objMessage) & "; " & strWebSendRes)
                        
                    ElseIf strWebSendRes.Contains("timed out") Then
                        sendToDBQueue(objMessage)
                        'send2Death(objMessage, MTResponseEC, strWebSendRes)
                        logger.Error("[ERROR]" & getObjInfo(objMessage) & "; " & strWebSendRes)
                    Else
                        send2Insert(objMessage, MTResponseEC, MTStatus._Fail) 'Other Error
                        logger.Info("[ERROR]" & getObjInfo(objMessage) & "; " & strWebSendRes)
                    End If

                    ' Use to determine the welcome message
                    If objMessage.Other = "Welcome" And (objMessage.KeywordType = KeywordType._ContentBasedSubs Or objMessage.KeywordType = KeywordType._TimeBasedSubs) Then

                        Dim UserCRUD As New UserCRUD
                        Dim KeywordCRUD As New KeywordCRUD
                        Dim KeywordInfo As New Tbl_Keyword
                        Dim UserUpdateResult As Integer
                        Dim MOMsgData As String = ""

                        logger.Info("[Checking MT Response]MSISDN=" & objMessage.MSISDN & ": " & MTResponseEC)
                        If SuccessResponseCode.Contains("," & MTResponseEC & ",") Then  'Success
                            UserUpdateResult = UserCRUD.UpdateActiveUserSubs(objMessage.MSISDN, objMessage.KeywordID)
                            KeywordInfo = KeywordCRUD.GetKeywordInfo_byKeywordID(objMessage.KeywordID)

                            MOMsgData = "&from=" & objMessage.MSISDN & _
                                 "&message=" & System.Web.HttpUtility.UrlDecode(KeywordInfo.Keyword & " ON") & _
                                 "&sc=" & objMessage.ShortCode & _
                                  "&telcoid=" & objMessage.TelcoID & _
                                  "&serviceid=" & objMessage.KeywordID & _
                                  "&msgid=" & System.Web.HttpUtility.UrlDecode(objMessage.TransactionID)

                            'forward to MO Forward Queue
                            Dim MOForwardStrc As New LibraryDAL.MOForwardStrc
                            With MOForwardStrc
                                .URL = KeywordInfo.MOUrl
                                .URLData = MOMsgData
                            End With
                            SendToQueue(ForwardMOQueue, MOForwardStrc)

                            'logger.Info("[ACTIVE_USER]MSISDN=" & objMessage.MSISDN & ";KeywordID=" & objMessage.KeywordID & ";KeywordMORUL=" & KeywordInfo.MOUrl & "?" & MOMsgData & ";UpdateStatus=" & UserUpdateResult.ToString)
                        Else
                            '[Wesley]: 24/4/2013: disable deactive user when welcome failed to Deliver/Charge.
                            'UserUpdateResult = UserCRUD.UpdateInActiveUserSubs(objMessage.MSISDN, objMessage.KeywordID)
                            'logger.Info("[INACTIVE_USER]MSISDN=" & objMessage.MSISDN & ";KeywordID=" & objMessage.KeywordID & ";UpdateStatus=" & UserUpdateResult.ToString)

                        End If
                    Else
                        SendToForwardDN(MTResponseEC, objMessage)  ' send forward dn to cp URL
                    End If


                Else
                    sendToDBQueue(objMessage)
                    'send2Death(objMessage, "", strWebSendRes)
                End If

            End Using

        Catch ex As Exception
            sendToDBQueue(objMessage)
            'send2Death(objMessage, "", "")
            logger.Fatal("[FATAL]" & strWebSendRes)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub


    Private Sub sendToURLQueue(ByVal objMessage As SendSMSAPIStrc)
        Dim MKCollection As New NameValueCollection
        Dim strMessage As String = ""
        Dim strDateTime As String = ""
        Dim strResultpost As String = ""
        Dim strColl As String = ""
        Dim strWebSendRes As String = ""
        Dim strTempWebSendRes As String = ""
        'Dim res As Byte()
        Dim ServiceID As String = "6886"
        'Dim SendDate As DateTime
        'Dim Flag As Integer
        Dim strSD As String = ""
        Dim strTC As String = ""
        Dim MTResponseEC As String = ""
        Dim PostSmartMT As Boolean = True

        Dim MTResponseBackList As New ArrayList

        Dim dt As DataSet

        Try

            'Dim MessageList As ArrayList
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)

            Using ShortCodeCRUD As New ShortCodeCRUD
                TelcoCPID = ShortCodeCRUD.GetShortCodeUserID(objMessage.ShortCode, objMessage.TelcoID)
            End Using

            Using KeywordCRUD As New KeywordCRUD
                dt = KeywordCRUD.GetKeywordShotcodeSD(objMessage.ShortCode, objMessage.KeywordID, objMessage.TelcoID)

                For Each d As DataRow In dt.Tables(0).Rows
                    ServiceID = d.Item("ServiceID").ToString
                    strSD = d.Item("ServiceDescription").ToString
                Next
            End Using

            If objMessage.Charge = 0 Then
                strSD = "00"
            End If

            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTC = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using
            Dim OA As String = ""

            If objMessage.KeywordType = KeywordType._IOD Or objMessage.KeywordType = KeywordType._Conditional Then
                OA = objMessage.ShortCode & TelcoCPID & objMessage.TransactionID
            Else  ' subscrrtion service without moid
                OA = objMessage.ShortCode & TelcoCPID
            End If

            Dim strDCS As String = "0"

            If objMessage.MsgType = MsgType.TextSMS Then 'Regular Text Message
                strDCS = "0"
            ElseIf objMessage.MsgType = MsgType.Logo Then
                strDCS = "245"
            ElseIf objMessage.MsgType = MsgType.PictureMessage Then
                strDCS = "245"
            ElseIf objMessage.MsgType = MsgType.RingTone Then
                strDCS = "245"
            End If

            'Dim objConnectPremium As New ConnectDB
            'Dim sQuery As String = "insert into " & dbqueue & " values (0,'1','" & _
            'objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0',now(),'" & OA & "','" & objMessage.MSISDN & "','" & strTC & "','" & strSD & "','0','" & _
            'objMessage.Message & "','" & strDCS & "','" & ServiceID & "');"

            'Dim sQuery As String = "INSERT INTO " & dbqueue & " (id,cpid,keywordid,charge,shortcode,count,timestamp,OA,DA,TC,SD,PID,UD,DCS,ANA,msguid) VALUES (0,'1','" & objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0',now(),'" & OA & "','" & objMessage.MSISDN & "','" & strTC & "','" & strSD & "','0','" & objMessage.Message & "','" & strDCS & "','" & ServiceID & "','" & objMessage.MsgGUID & "')"
            Dim sQuery As String = "INSERT INTO " & dbqueue & " (id,msisdn,telco_id,cp_id,keyword_id,charge,shortcode,retry,service_id,product_id,link_id,message,msgguid,timestamp) VALUES (0,'" & objMessage.MSISDN & "','1','1','" & objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0','0027002000001222','1000001811','" & objMessage.TransactionID & "','" & objMessage.Message & "','" & objMessage.MsgGUID & "',now())"

            'logger.Info("Entering " & sQuery)
            'Dim result As Integer = ExecuteCommand(sQuery)
            Dim result As String = ""
            strWebSendRes = MT_URL & "?telco=SMART&query=" & System.Web.HttpUtility.UrlEncode(sQuery)

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
                send2Death(objMessage, "", "")
            End If

        Catch ex As Exception
            send2Death(objMessage, "", "")

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
        'Dim res As Byte()
        Dim ServiceID As String = "6886"
        'Dim SendDate As DateTime
        'Dim Flag As Integer
        Dim strSD As String = ""
        Dim strTC As String = ""
        Dim MTResponseEC As String = ""
        Dim PostSmartMT As Boolean = True

        Dim MTResponseBackList As New ArrayList

        Dim dt As DataSet

        Try

            'Dim MessageList As ArrayList
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
            ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)

            Using ShortCodeCRUD As New ShortCodeCRUD
                TelcoCPID = ShortCodeCRUD.GetShortCodeUserID(objMessage.ShortCode, objMessage.TelcoID)
            End Using

            Using KeywordCRUD As New KeywordCRUD
                dt = KeywordCRUD.GetKeywordShotcodeSD(objMessage.ShortCode, objMessage.KeywordID, objMessage.TelcoID)

                For Each d As DataRow In dt.Tables(0).Rows
                    ServiceID = d.Item("ServiceID").ToString
                    strSD = d.Item("ServiceDescription").ToString
                Next
            End Using

            If objMessage.Charge = 0 Then
                strSD = "00"
            End If

            Using TelcoChargeCodeCRUD As New TelcoChargeCodeCRUD
                strTC = TelcoChargeCodeCRUD.GetTelcoChargeCode(objMessage.TelcoID, objMessage.Charge, objMessage.ShortCode)
            End Using
            Dim OA As String = ""

            If objMessage.KeywordType = KeywordType._IOD Or objMessage.KeywordType = KeywordType._Conditional Then
                OA = objMessage.ShortCode & TelcoCPID & objMessage.TransactionID
            Else  ' subscrrtion service without moid
                OA = objMessage.ShortCode & TelcoCPID
            End If

            Dim strDCS As String = "0"

            If objMessage.MsgType = MsgType.TextSMS Then 'Regular Text Message
                strDCS = "0"
            ElseIf objMessage.MsgType = MsgType.Logo Then
                strDCS = "245"
            ElseIf objMessage.MsgType = MsgType.PictureMessage Then
                strDCS = "245"
            ElseIf objMessage.MsgType = MsgType.RingTone Then
                strDCS = "245"
            End If

            'Dim objConnectPremium As New ConnectDB
            Dim sQuery As String = "insert into " & dbqueue & " values (0,'1','" & _
            objMessage.KeywordID & "','" & objMessage.Charge & "','" & objMessage.ShortCode & "','0',now(),'" & OA & "','" & objMessage.MSISDN & "','" & strTC & "','" & strSD & "','0','" & _
            objMessage.Message & "','" & strDCS & "','" & ServiceID & "');"

            logger.Info("Entering " & sQuery)
            Dim result As Integer = ExecuteCommand(sQuery)
            logger.Info(result & "|" & sQuery)

            If result = -1 Then
                send2Death(objMessage, "", "")
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
            send2Death(objMessage, "", "")
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


    Public Function ValidateServerCertificate(ByVal sender As Object, ByVal cert As X509Certificate, ByVal chain As X509Chain, _
    ByVal ssl As SslPolicyErrors) As Boolean
        Return True
    End Function


    Private Function GetMTFailResponse(ByVal response As String) As MTResponse
        Dim rev As String = ""
        Dim xml As XDocument = XDocument.Parse(response)
        Dim MTResponse As New MTResponse

        Dim ChildQuery = From c In xml.Element("HTML").Descendants("BODY") Select c

        For Each d As XElement In ChildQuery
            With MTResponse
                ._DA = d.Element("DA").Value
                ._EC = d.Element("EC").Value  'Response Code
                ._ECT = d.Element("ECT").Value  'Response Description
            End With
        Next

        Return MTResponse
    End Function

    'Private Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
    '    Try
    '        Dim B As Byte() = Nothing
    '        Dim response As Byte() = Nothing
    '        Dim HTML As String = ""

    '        Using web As New System.Net.WebClient
    '            web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
    '            B = System.Text.Encoding.ASCII.GetBytes(PostData)
    '            response = web.UploadData(URL, "POST", B)
    '            HTML = System.Text.Encoding.ASCII.GetString(response)
    '        End Using

    '        Return HTML.Trim
    '    Catch ex As Exception
    '        logger.Fatal(URL & ";" & PostData)
    '        logger.Fatal("[FATAL]", ex)
    '        Return "FAIL"
    '    End Try

    'End Function

    Public Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
        Try
            logger.Fatal(URL & PostData)
            Dim encoding As New System.Text.ASCIIEncoding
            Dim Data() As Byte = encoding.GetBytes(PostData)
            Dim LoginReq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(URL), Net.HttpWebRequest)
            With LoginReq
                .KeepAlive = False
                .Method = "GET"
                .ContentType = "application/x-www-form-urlencoded"
                .ContentLength = Data.Length
                .Timeout = 15000
            End With

            Dim SendReq As Stream = LoginReq.GetRequestStream
            SendReq.Write(Data, 0, Data.Length)
            SendReq.Close()

            Dim LoginRes As System.Net.HttpWebResponse = CType(LoginReq.GetResponse(), Net.HttpWebResponse)
            Dim HTML As String = ""
            Using sReader As StreamReader = New StreamReader(LoginRes.GetResponseStream)
                HTML = sReader.ReadToEnd
            End Using

            HTML = HTML.Trim

            '& " (" & LoginRes.StatusCode.ToString & ")"

            LoginRes.Close()
            LoginRes = Nothing

            Return HTML.Trim
        Catch ex As Exception
            logger.Fatal("URL=" & URL & "?" & PostData)
            logger.Fatal("[FATAL] Message : " & ex.Message)
            logger.Fatal("[FATAL] Source : " & ex.Source)
            logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)

            Return "Fail"
        End Try
    End Function

    Private Function CompileBinarySMS(ByVal BinaryType As String, ByVal pMessage As String, ByVal pCharacterPerSMS As Integer) As ArrayList
        Dim SMSManager As New SMSManager
        Dim MessageList As New ArrayList

        Dim MsgLength As Integer = CInt(System.Text.Encoding.ASCII.GetByteCount(pMessage)) ' get message length
        Dim MessageArray() As String
        Dim LengthSplit As Integer = 0
        Dim Header As String = ""

        If MsgLength > pCharacterPerSMS Then
            Header = "0B050415" & BinaryType & "00000003" & "FF"  'Smart Concatenated Binary SMS Header
            LengthSplit = pCharacterPerSMS - (Header.Length + 4)
            MessageArray = SMSManager.SplitTextByLength(pMessage, LengthSplit)

            For i As Integer = 1 To MessageArray.Count
                Dim SmartMsgStr As New SmartMessageFormat
                With SmartMsgStr
                    ._UDH = Header & "0" & MessageArray.Count & "0" & i.ToString
                    If MessageArray.Count >= 10 Then
                        ._UDH = Header & MessageArray.Count & i.ToString
                    End If
                    ._UD = MessageArray(i - 1)
                End With

                MessageList.Add(SmartMsgStr)
            Next
        Else
            Header = "06050415" & BinaryType & "0000"  'Smart Binary SMS Header
            Dim SmartMsgStr As New SmartMessageFormat
            With SmartMsgStr
                ._UDH = Header
                ._UD = pMessage
            End With
            MessageList.Add(SmartMsgStr)
        End If

        Return MessageList
    End Function

    Private Function CompileTextSMS(ByVal pMessage As String, ByVal pCharacterPerSMS As Integer) As ArrayList
        Dim SMSManager As New SMSManager
        Dim MessageList As New ArrayList

        Dim MsgLength As Integer = CInt(System.Text.Encoding.ASCII.GetByteCount(pMessage)) ' get message length
        Dim MessageArray() As String
        Dim LengthSplit As Integer = 0
        Dim Header As String = "0500030A" ' Smart Concatenated Text SMS Header

        If MsgLength > pCharacterPerSMS Then
            LengthSplit = pCharacterPerSMS - (Header.Length + 4)
            MessageArray = SMSManager.SplitTextByLength(pMessage, LengthSplit)
            For i As Integer = 1 To MessageArray.Count
                Dim SmartMsgStr As New SmartMessageFormat
                With SmartMsgStr
                    ._UDH = Header & "0" & MessageArray.Count & "0" & i.ToString
                    If MessageArray.Count >= 10 Then
                        ._UDH = Header & MessageArray.Count & i.ToString
                    End If
                    ._UD = MessageArray(i - 1)
                End With

                MessageList.Add(SmartMsgStr)
            Next
        Else
            Dim SmartMsgStr As New SmartMessageFormat
            With SmartMsgStr
                ._UDH = ""
                ._UD = pMessage
            End With
            MessageList.Add(SmartMsgStr)
        End If

        Return MessageList
    End Function

    Private Sub send2Death(ByVal obj As SendSMSAPIStrc, ByVal resp As String, ByVal strWebSendRes As String)
        Dim TempRetryCount As Integer = 0
        TempRetryCount = obj.RetryCount + 1

        'If TempRetryCount <= CInt(RetryCount) Then
        obj.RetryCount = TempRetryCount
        SendToDeathQueue(death_queue, obj)
        'Else
        'logger.Error("[ERROR]" & getObjInfo(obj) & "; " & strWebSendRes)
        'send2Insert(obj, resp, MTStatus._Fail)
        'SendToForwardDN(resp, obj)  ' forward to cp url
        'End If
    End Sub

    Private Sub send2Insert(ByVal obj As SendSMSAPIStrc, ByVal MTResponse As String, ByVal Status As Integer)

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
            .StatusCode = MTResponse
            .TelcoID = obj.TelcoID
            .TransactionID = obj.TransactionID
            .SendDate = obj.SendDate
            .Status = Status
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

    Private Sub SendToQueue(ByVal destination As String, ByVal obj As Object)
        Try
            Using q As New MessageQueue(destination, True)
                MessageQueue.EnableConnectionCache = False
                Using qtrans As New MessageQueueTransaction
                    qtrans.Begin()
                    q.Send(obj, qtrans)
                    qtrans.Commit()
                End Using
            End Using
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub


    'fucntion Send DN to CP URL
    Private Sub SendToForwardDN(ByVal StatusCode As String, ByVal body As SendSMSAPIStrc)
        Dim DNURL As String = ""
        Dim RespCode As New ResponseCode
        Dim FinalRespCode As String = ""

        Using KeywordCRUD As New KeywordCRUD
            DNURL = KeywordCRUD.GetDNURL_byKeyid(body.KeywordID)
        End Using

        If Not String.IsNullOrEmpty(DNURL) Then

            Using ResponseCodeCRUD1 As New ResponseCodeCRUD
                FinalRespCode = ResponseCodeCRUD1.Single(body.TelcoID, StatusCode).ResponseCode
            End Using

            If String.IsNullOrEmpty(FinalRespCode) Then
                FinalRespCode = ErrorRespCode
            End If

            Dim ForwardDNType As New DNForwardStrc
            With ForwardDNType
                .URLData = "msgguid=" & body.MsgGUID & "&serviceid=" & body.KeywordID.ToString & "&status=" & FinalRespCode
                .URL = DNURL
                .RetryCount = 0
            End With

            'logger.Info(ForwardDNType.URL & "?" & ForwardDNType.URLData)
            SendToQueue(ForwardDNQueue, ForwardDNType)
        End If

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

    Private Sub send2MTRetry(ByVal obj As SendSMSAPIStrc, ByVal resp As String, ByVal strWebSendRes As String)
        Dim TempRetryCount As Integer = 0
        TempRetryCount = 0

        If TempRetryCount <= CInt(RetryCount) Then
            logger.Error("[ERROR] Retry insert:" & getObjInfo(obj))
            obj.RetryCount = TempRetryCount
            SendToRetryQueue(retry_queue, obj)
        Else
            logger.Error("[ERROR] Retry insert Failed:" & getObjInfo(obj))
            SendToRetryQueue(retry_queue, obj)
        End If
    End Sub

    Private Sub SendToRetryQueue(ByVal destination As String, ByVal obj As LibraryDAL.SendSMSAPIStrc)
        Try
            Using q As New MessageQueue(destination, True)
                MessageQueue.EnableConnectionCache = False
                Using qtrans As New MessageQueueTransaction
                    qtrans.Begin()
                    q.Send(obj, qtrans)
                    qtrans.Commit()
                End Using
            End Using
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub

End Class


