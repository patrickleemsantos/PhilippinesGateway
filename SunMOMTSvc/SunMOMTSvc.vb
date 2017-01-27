Imports System.Configuration
Imports LibraryDAL
Imports smscc
Imports smscc.UCP
Imports System.Messaging
Imports ALT.SMS
Imports System.Net
Imports System.ServiceModel
Imports System.Data.SqlClient
Imports System.IO
Imports System.Text
Imports LibraryDAL.General
Imports LibraryDAL.Extensions

Imports MySql.Data.MySqlClient
Imports MySql.Data
'Imports DOTUCPEMI

Public Delegate Sub AddToDelegate(ByVal body As SendSMSAPIStrc)

Public Class SunMOMTSvc
    Inherits System.ServiceProcess.ServiceBase

    'Added by Patrick for Bulk Inserts 2016-02-10
    Private arrInserts As New ArrayList

    Private del As AddToDelegate

    'Dim telcoid As Integer = 2

    'UCP EMI Client Start
    Private sms_cli As SMSClient

    Private listener As HttpListener
    Private result As IAsyncResult

    Private bol As Boolean = False
    Private _listen As Boolean
    'UCP EMI Client End 

    Private _queue As MessageQueue

    Private inbox_queue As String = ConfigurationManager.AppSettings("inbox_queue")
    Private outbox_queue As String = ConfigurationManager.AppSettings("outbox_queue")

    Private sun_queue As String = ConfigurationManager.AppSettings("gateway_queue")
    Private death_queue As String = ConfigurationManager.AppSettings("death_queue")

    Private update_queue As String = ConfigurationManager.AppSettings("update_queue")

    'Private MOInsert_queue As String = ConfigurationManager.AppSettings("MOInsert_queue")
    Private MOForward_queue As String = ConfigurationManager.AppSettings("MOForward_queue")

    Private SERVER As String = ConfigurationManager.AppSettings("SERVER")
    Private PORT As String = ConfigurationManager.AppSettings("PORT")

    Private CSERVER As String = ConfigurationManager.AppSettings("CSERVER")
    Private CPORT As String = ConfigurationManager.AppSettings("CPORT")

    Private USERNAME As String = ConfigurationManager.AppSettings("USERNAME")
    Private PASSWORD As String = ConfigurationManager.AppSettings("PASSWORD")

    Private dn_subscription As String = ConfigurationManager.AppSettings("dn_subscription")
    Private dn_iod As String = ConfigurationManager.AppSettings("dn_iod")
    Private dn_receiver As String = ConfigurationManager.AppSettings("dn_receiver")

    Private ORIGINATOR As String = ConfigurationManager.AppSettings("ORIGINATOR")
    Private telcoid As String = ConfigurationManager.AppSettings("TELCOID")

    Private MODeath As String = ConfigurationManager.AppSettings("MODeath_queue")

    Private conn As String = ConfigurationManager.AppSettings("ConnectionString")

    Private pre_footer_Nor_Msg As String = ConfigurationManager.AppSettings("NormalFooterMsg")
    Private pre_footer_Fri_Msg As String = ConfigurationManager.AppSettings("FriFooterMsg")
    Private mySqlConStr As String = ConfigurationManager.AppSettings("mySqlConn")

    Private MOType As New LibraryDAL.Tbl_MOStr

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
        log4net.Config.XmlConfigurator.Configure()

        del = New AddToDelegate(AddressOf pushMT)

        StartListen()

        'StartConnecting()
        'StartSMPPConnect()
        StartUCPConnect()
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        StopListen()
    End Sub


    Private Function CheckKeyword(ByVal pFirstKeyword As String, ByVal pSecondKeyword As String, ByVal pShortCode As String, ByVal pTelcoID As Integer) As DataSet
        Dim KeywordCRUD As New KeywordCRUD
        Dim dt As DataSet
        dt = KeywordCRUD.ChecKeyword(pFirstKeyword, pSecondKeyword, pTelcoID, pShortCode)
        Return dt
    End Function

    Private Sub StartListen()
        Try
            _queue = New MessageQueue(sun_queue)
            MessageQueue.EnableConnectionCache = False

            _listen = True
            AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
            _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(SendSMSAPIStrc)})

            StartListening()

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub

    Private Sub StopListen()
        'sms_cli.t.abort()
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
        Try
            _queue.EndPeek(e.AsyncResult)
            Dim trans As New MessageQueueTransaction()
            Dim msg As Message = Nothing
            trans.Begin()
            msg = _queue.Receive(trans)
            trans.Commit()
            del.Invoke(CType(msg.Body, SendSMSAPIStrc))
            StartListening()
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub


    Private Sub pushMTLogs(ByVal body As SendSMSAPIStrc)
        Try

            ' Add new Keyword
            ' 1. add keywordshortcode table
            ' 2. add keyword table
            ' 3. add keywordcharge table
            ' 4. add keyworddetect tbl

            'http://localhost:8001/sendsms.aspx?user=Test&pass=Test1234&serviceid=49&to=09222876064&msg=testing+billing&type=1&charge=200&sc=2488&telcoid=2&msgid=


            'http://localhost:8001/sendsms.aspx?user=Test&pass=Test1234&to=09222876064&sc=2488&type=1&msg=testing.&charge=0&telcoid=2&msgid=&serviceid=106&other=help

            'Dim OutboxCRUD As New OutboxCRUD
            'OutboxCRUD.OutboxInsert(body)

            'body.Charge

            logger.Info("[MT Received and do Nothing]: from: " & body.ShortCode & "; to: " & body.MSISDN & "; msg: " & body.Message)

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub


    Public Shared Function CheckMsgMaxContent(ByVal msgContent As String, ByVal maxCharacterPerMsg As Int16) As Int16
        Dim msgBytesCount As Int16 = CShort(System.Text.Encoding.ASCII.GetByteCount(msgContent))

        ' Calculate the required msg to be sent.
        Dim msgCount As Int16 = 0
        'msgCount = msgCount + (msgBytesCount \ maxCharacterPerMsg)

        'Modified by andylai 2012-06-07 
        msgCount = Math.Ceiling((msgBytesCount / maxCharacterPerMsg))
        CheckMsgMaxContent = msgCount

        Return CheckMsgMaxContent
    End Function


    Public Function splitSms(ByVal msgcontent As String, ByVal maxCharacterPerMsg As Int16) As String()
        Dim msgBytesCount As Int16 = CShort(System.Text.Encoding.ASCII.GetByteCount(msgcontent) - 1)

        ' Calculate the required msg to be sent.
        Dim msgCount As Int16 = 0   'At least one sms
        ' msgCount = msgCount + (msgBytesCount \ maxCharacterPerMsg)

        'Modified by andylai 2012-06-07 
        msgCount = Math.Ceiling((msgBytesCount / maxCharacterPerMsg))

        Dim arrayString(msgCount - 1) As String
        Dim startPosition As Int16 = 0

        ' Truncate the msg and store into array.
        Dim i As Int16 = 0
        Dim arrCount As Int16 = 0
        Dim charArr As Char() = msgcontent.ToCharArray

        Dim strArr As String() = Split(msgcontent & " ", " ")
        Dim scanStr As Boolean = True

        Dim startWord As Integer = 0
        Dim wordCount As Integer = 0

        Dim tempStr As String = ""

        'For i = 0 To msgBytesCount
        While startPosition < strArr.Length - 1
            'Console.WriteLine("Reading.... " & strArr(startPosition) & "; start possition: " & startPosition & "; Total: " & msgBytesCount & "; ArrayCount: " & strArr.Length)
            'startPosition = 0

            If (tempStr + " " + strArr(startPosition)).Length >= maxCharacterPerMsg Then
                arrayString(arrCount) = tempStr
                tempStr = "" + strArr(startPosition)
                arrCount = arrCount + 1
            Else
                If startPosition = 0 Then
                    tempStr = strArr(startPosition)
                Else
                    tempStr = tempStr + " " + strArr(startPosition)
                End If

                'Console.WriteLine(tempStr)
            End If

            startPosition = startPosition + 1
        End While

        Try
            arrayString(arrCount) = tempStr
            tempStr = ""
            arrCount = arrCount + 1
        Catch ex As Exception

        End Try

        'Next

        'splitSms = arrayString
        Return arrayString
    End Function

    Public Shared Function splitSms2(ByVal msgcontent As String, ByVal maxCharacterPerMsg As Int16) As String()
        Dim msgBytesCount As Int16 = CShort(System.Text.Encoding.ASCII.GetByteCount(msgcontent) - 1)

        ' Calculate the required msg to be sent.
        Dim msgCount As Int16 = 0   'At least one sms
        ' msgCount = msgCount + (msgBytesCount \ maxCharacterPerMsg)

        'Modified by andylai 2012-06-07 
        msgCount = Math.Ceiling((msgBytesCount / maxCharacterPerMsg))

        Dim arrayString(msgCount - 1) As String
        Dim startPosition As Int16 = 0

        ' Truncate the msg and store into array.
        Dim i As Int16
        For i = 0 To CShort(msgCount - 1)
            startPosition = 0
            If msgcontent.Length > maxCharacterPerMsg Then
                arrayString(i) = msgcontent.Substring(startPosition, maxCharacterPerMsg)
                startPosition = startPosition + maxCharacterPerMsg
                msgcontent = msgcontent.Substring(startPosition, msgcontent.Length - maxCharacterPerMsg)
            Else
                arrayString(i) = msgcontent
            End If
        Next

        splitSms2 = arrayString
    End Function


    Private Sub pushMT(ByVal body As SendSMSAPIStrc)
        Dim arrayMsg As String()
        Dim dayIndex As Integer = Today.DayOfWeek
        Dim total As Int16 = 0
        'If body.KeywordID = 50 Or body.Other.ToLower = "help" Or body.Other.ToUpper = "DOUBLE" Or body.Charge = 0 Then
        'If body.Other.ToLower = "help" Or body.Other.ToUpper = "DOUBLE" Or body.Charge = 0 Then
        total = CheckMsgMaxContent(body.Message, 160)
        'Else
        'total = CheckMsgMaxContent(body.Message & pre_footer_Fri_Msg, 160)
        'End If

        If total > 1 Then
            'If body.KeywordID = 50 Or body.Other.ToLower = "help" Or body.Other.ToUpper = "DOUBLE" Or body.Charge = 0 Then
            'arrayMsg = splitSms(body.Message, 154)
            'Else
            'If dayIndex = DayOfWeek.Friday Then
            'arrayMsg = splitSms(body.Message, 154)
            'Else
            'arrayMsg = splitSms(body.Message, 154)
            'End If
            'End If

            arrayMsg = splitSms(body.Message, 154)
            Dim count As Int16 = 0
            Dim submiMtResult = False
            For Each fern As String In arrayMsg
                count += 1
                If count > 1 Then
                    'body.Message = "" & count & "/" & arrayMsg.Count & ": " & fern
                    'body.Charge = 0
                Else
                    'body.Message = "" & count & "/" & arrayMsg.Count & ": " & fern
                End If


                If SubmitMT(body, count, total, fern) Then
                    'submiMtResult = True
                Else
                    submiMtResult = False
                    sendToDeathQueue(death_queue, body)
                    Return
                End If
            Next

            'Promo Message Start
            'If body.KeywordID = 50 Or body.Other.ToLower = "help" Or body.Other.ToUpper = "DOUBLE" Or body.Charge = 0 Then
            'Else
            'Dim promo As String = ""
            'If dayIndex = DayOfWeek.Friday Then
            'promo = pre_footer_Fri_Msg
            'Else
            'promo = pre_footer_Nor_Msg
            'End If

            'If SubmitMT(body, count + 1, total, promo) Then

            'Else
            'sendToDeathQueue(death_queue, body)
            'Return
            'End If
            'End If
            'Promo Message End
            'If submiMtResult Then
            send2OutboxDB(body, "SUCCESS", "" & body.MsgGUID, 1)
            'End If
        Else
            Dim result As Boolean = False
            If body.KeywordID = 50 Or body.Other.ToLower = "help" Or body.Other.ToUpper = "DOUBLE" Or body.Charge = 0 Then
                result = SubmitMT(body, 0, 0, body.Message)
            Else
                'If dayIndex = DayOfWeek.Friday Then
                ''result = SubmitMT(body, 1, 2, body.Message & pre_footer_Fri_Msg)
                'result = SubmitMT(body, 1, 2, body.Message)
                'result = SubmitMT(body, 2, 2, pre_footer_Fri_Msg)
                'Else
                ''result = SubmitMT(body, 1, 2, body.Message & pre_footer_Nor_Msg)
                'result = SubmitMT(body, 1, 2, body.Message)
                'result = SubmitMT(body, 2, 2, pre_footer_Nor_Msg)
                'End If
                result = SubmitMT(body, 0, 0, body.Message)
            End If

            If result Then
                send2OutboxDB(body, "SUCCESS", "" & body.MsgGUID, 1)
            Else
                sendToDeathQueue(death_queue, body)
            End If
        End If


    End Sub
    Public Enum DayOf
        Monday = 1
        Tuesday = 2
        Wednesday = 3
        Thursday = 4
        Friday = 5
        Saturday = 6
        Sunday = 7
    End Enum

    Private Function SubmitMT(ByVal body As SendSMSAPIStrc, ByVal page As Int16, ByVal total As Int16, ByVal msgProcessed As String) As Boolean
        Try

            ' Add new Keyword
            ' 1. add keywordshortcode table
            ' 2. add keyword table
            ' 3. add keywordcharge table
            ' 4. add keyworddetect tbl

            'http://localhost:8001/sendsms.aspx?user=Test&pass=Test1234&serviceid=49&to=09222876064&msg=testing+billing&type=1&charge=200&sc=2488&telcoid=2&msgid=

            'http://115.85.17.59:8001/sendsms.aspx?user=Test&pass=Test1234&serviceid=49&to=09222876064&msg=testing+billing&type=1&charge=200&sc=2488&telcoid=2&msgid=

            'Dim OutboxCRUD As New OutboxCRUD
            'OutboxCRUD.OutboxInsert(body)

            'body.Charge

            'Dim day As String = Date.Today.DayOfWeek.ToString()
            Dim msgSent As String = ""
            Dim msgCharge As Integer = body.Charge
            Dim sms_p As New UCPPacket

            Dim msgKeywordID As Integer = body.KeywordID

            'sms_p.packet_message_XSer = "0C12303130303030303143313233303030303230"
            'setPrice(body, sms_p)
            'Dim dayIndex As Integer = Today.DayOfWeek
            If total = 0 Then
                msgSent = msgProcessed

                'If dayIndex < DayOfWeek.Friday Then
                'msgSent = msgProcessed & pre_footer_Fri_Msg
                'Else
                'msgSent = msgProcessed & pre_footer_Nor_Msg
                'End If
                msgCharge = body.Charge
            Else
                If page > 1 Then
                    'If page = total Then
                    '    'msgSent = "" & page & "/" & total & ": " & msgProcessed
                    '    msgSent = "" & msgProcessed
                    '    'If dayIndex < DayOfWeek.Friday Then
                    '    'msgSent = "" & page & "/" & total & ": " & msgProcessed & pre_footer_Fri_Msg
                    '    'Else
                    '    'msgSent = "" & page & "/" & total & ": " & msgProcessed & pre_footer_Nor_Msg
                    '    'End If
                    'Else
                    '    'msgSent = "" & page & "/" & total & ": " & msgProcessed
                    '    msgSent = "" & msgProcessed
                    'End If
                    msgSent = "" & msgProcessed
                    msgCharge = 0
                    'msgCharge = body.Charge
                Else
                    'msgSent = "" & page & "/" & total & ": " & msgProcessed
                    msgSent = msgProcessed
                    msgCharge = body.Charge
                End If

            End If

            If total = 0 Then
                sms_p.Create_Submit_AlphaNum_SMS(body.ShortCode, body.MSISDN, msgSent, msgCharge, "0", msgKeywordID)
            Else
                sms_p.Create_Submit_AlphaNum_SMS(body.ShortCode, body.MSISDN, msgSent, msgCharge, ("0" & Convert.ToString(total) & "0" & Convert.ToString(page)), msgKeywordID)
            End If
            'sms_p.Create_Submit_AlphaNum_SMS(body.ShortCode, body.MSISDN, msgSent, msgCharge, ("0" & total & "0" & page))
            logger.Info("[MT Checking]: " & sms_p.getSendPacketDef())

            Dim result As Boolean = False
            ' Dim result = sms_cli.SubmitSMS(body.ShortCode, body.MSISDN, msgSent, msgCharge, ("0" & total & "0" & page))
            If total = 0 Then
                result = sms_cli.SubmitSMS(body.ShortCode, body.MSISDN, msgSent, msgCharge, "0", msgKeywordID)
            Else
                result = sms_cli.SubmitSMS(body.ShortCode, body.MSISDN, msgSent, msgCharge, ("0" & Convert.ToString(total) & "0" & Convert.ToString(page)), msgKeywordID)
            End If

            If result Then
                'send2OutboxDB(body, "SUCCESS", "" & body.MsgGUID, 1)
                logger.Info("[MT Push Sucess]: from: " & body.ShortCode & "; to: " & body.MSISDN & "; msg: " & msgSent & "[Result]: " & result)
                Return True
            Else
                'sendToDeathQueue(death_queue, body)
                logger.Info("[MT Push Failed]: from: " & body.ShortCode & "; to: " & body.MSISDN & "; msg: " & msgSent & "[Result]: " & result)
                Return False
            End If

            'Added by Patrick to delay sending 2016-10-13
            Threading.Thread.Sleep(3000)

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
            'sendToDeathQueue(death_queue, body)
            Return False
        End Try
        Return False
    End Function

    Private Sub send2OutboxDB(ByVal obj As SendSMSAPIStrc, ByVal MTResponse As String, ByVal pMTID As String, ByVal Status As Integer)
        Dim sql As String = "INSERT INTO premium_sms_db.mt_sun (id,telco_id,shortcode,cp_id,keyword_id,msisdn,charge,content,retry,mo_id,mt_id,mt_response_code,mt_error_code,mt_error_desc,timestamp,dn_code,dn_desc,dn_timestamp,msgguid) VALUES (0,'" & obj.TelcoID & "','" & obj.ShortCode & "','" & obj.CPID & "','" & obj.KeywordID & "','" & obj.MSISDN & "','" & obj.Charge & "','" & obj.Message & "','" & obj.RetryCount & "','" & obj.TransactionID & "','','200','','',now(),'','',0,'" & obj.MsgGUID & "')"
        executeQuery(sql)
        'arrInserts.Add(obj)
        'If arrInserts.Count >= 3 Then
        '    bulkInsert()
        'End If
    End Sub

    Private Sub bulkInsert()
        Dim sql As String = "INSERT INTO premium_sms_db.mt_sun (id,telco_id,shortcode,cp_id,keyword_id,msisdn,charge,content,retry,mo_id,mt_id,mt_response_code,mt_error_code,mt_error_desc,timestamp,dn_code,dn_desc,dn_timestamp,msgguid) VALUES "
        Dim obj As SendSMSAPIStrc

        For Each obj In arrInserts
            sql = sql & "(0,'" & obj.TelcoID & "','" & obj.ShortCode & "','" & obj.CPID & "','" & obj.KeywordID & "','" & obj.MSISDN & "','" & obj.Charge & "','" & obj.Message & "','" & obj.RetryCount & "','" & obj.TransactionID & "','','200','','',now(),'','',0,'" & obj.MsgGUID & "'),"
        Next

        sql = sql.Substring(0, sql.Length - 1)

        logger.Info("[BULK INSERT]: " & sql)

        executeQuery(sql)
        arrInserts.Clear()

    End Sub

    Private Sub updateDN(ByVal msisdn As String)
        Dim sql As String = "UPDATE premium_sms_db.mt_sun SET dn_code = 'SUCCESS', dn_desc = 'SUCCESS', dn_timestamp = NOW() WHERE msisdn = '" & msisdn & "' AND dn_code = '' ORDER BY id DESC LIMIT 1"
        executeQuery(sql)
    End Sub

    Private Function getCPID(ByVal keywordID As Integer)
        Dim cpID As Integer = 0
        Dim sql As String = "SELECT cp_id FROM premium_sms_db.keyword WHERE keyword_id = '" & keywordID & "'"
        Dim ds_keyword As DataSet = getQuery(sql, "keyword")
        If ds_keyword.Tables("keyword").Rows.Count > 0 Then
            cpID = ds_keyword.Tables("keyword").Rows(0).Item("cp_id").ToString
        End If
        Return cpID
    End Function

    Private Sub executeQuery(ByVal sql As String)
        Dim con As MySqlConnection = New MySqlConnection(mySqlConStr)
        Dim cmd As New MySqlCommand
        Dim result As Integer
        Try
            con.Open()
            cmd.Connection = con
            cmd.CommandText = sql
            result = cmd.ExecuteNonQuery
            con.Close()
            logger.Info("[Sun MO/MT Service QUERY]: " & result & "|" & sql)
        Catch ex As Exception
            logger.Info("[Sun MO/MT Service QUERY]: " & ex.Message & "|" & sql)
            con.Close()
        End Try
    End Sub

    Private Function getQuery(ByVal sql As String, ByVal tbl_db As String) As DataSet
        Dim con As MySqlConnection = New MySqlConnection(mySqlConStr)
        Dim cmd As New MySqlCommand
        Dim ds As New DataSet
        Try
            con.Open()
            Using da As New MySqlDataAdapter
                da.SelectCommand = New MySqlCommand(sql, con)
                da.SelectCommand.CommandTimeout = 21600
                da.Fill(ds, tbl_db)
                da.SelectCommand.Connection.Close()
            End Using
            con.Close()
            logger.Info("[SUN MO GET QUERY]: " & sql)
        Catch ex As Exception
            logger.Info("[SUN MO SQL EXCEPTION]: " & ex.Message & "|" & sql)
            con.Close()
            ds.Dispose()
        End Try
        Return ds
    End Function

    Private Sub SendMOForwardQueue(ByVal destination As String, ByVal obj As LibraryDAL.MOForwardStrc)
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

    Public Sub sendToMOQueue(ByVal destination As String, ByVal obj As LibraryDAL.Tbl_MOStr)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub


    Public Sub sendToDeathQueue(ByVal destination As String, ByVal obj As SendSMSAPIStrc)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

    'UCP API Start Here
    Public Sub StartUCPConnect()
        sms_cli = New SMSClient(SERVER, CInt(PORT))
        sms_cli.bind(CSERVER, CInt(CPORT))
        sms_cli.login(USERNAME, PASSWORD)
        sms_cli.autoReconnect = False

        If sms_cli.connected Then
            sms_cli.StartKeepAlive()
            AddToLog("Connected UCP/EMI Server...")
            AddHandler sms_cli.onDebug, AddressOf onDebug
            AddHandler sms_cli.login_failed, AddressOf onLoginfailed
            AddHandler sms_cli.login_success, AddressOf onLoginsucceded
            AddHandler sms_cli.onConnect, AddressOf onConnect
            AddHandler sms_cli.onDisconnect, AddressOf onDisconnect
            AddHandler sms_cli.msg_recv, AddressOf onRecvMsg
            AddHandler sms_cli.send_keep_alive, AddressOf onKeepAlive
            'AddHandler sms_cli.recieved_Deliver_notification, AddressOf onReceivedDN
            'AddHandler sms_cli.recievedShortMessages, AddressOf onReceivedMessage
        Else
            AddToLog("Unable to connect to UCP/EMI Server. Connection status : " & sms_cli.connected & ". Exiting client...")
        End If
    End Sub

    Private Sub onRecvMsg(ByVal msg As UCPPacket)
        logger.Fatal("onRecvMsg: " & msg.Originator & "->" & msg.receiver & ": " & msg.Message)
        ProcessMO(msg)
    End Sub

    Public Function printLog(ByVal msg As UCPPacket) As String
        logger.Fatal("onRecvMsg. AC " & msg.packet_message_AC)
        logger.Fatal("onRecvMsg. AdC " & msg.packet_message_AdC)
        logger.Fatal("onRecvMsg. AMsg " & msg.packet_message_AMsg)
        logger.Fatal("onRecvMsg. DD " & msg.packet_message_DD)
        logger.Fatal("onRecvMsg. DDT " & msg.packet_message_DDT)
        logger.Fatal("onRecvMsg. NAd " & msg.packet_message_NAd)
        logger.Fatal("onRecvMsg. NPID " & msg.packet_message_NPID)
        logger.Fatal("onRecvMsg. NRq " & msg.packet_message_NRq)
        logger.Fatal("onRecvMsg. OAdC " & msg.packet_message_OAdC)
        logger.Fatal("onRecvMsg. VP " & msg.packet_message_VP)

        logger.Fatal("onRecvMsg. OTON " & msg.packet_message_OTON)
        'ONPI   ' Originator Numbering Plan Id
        logger.Fatal("onRecvMsg. ONPI " & msg.packet_message_ONPI)
        'STYP   ' Subtype of operation
        logger.Fatal("onRecvMsg. STYP " & msg.packet_message_STYP)
        'PWD    ' Current password encoded into IA5 characters
        logger.Fatal("onRecvMsg. PWD " & msg.packet_message_PWD)
        'NPWD   ' New password encoded into IA5 characters
        logger.Fatal("onRecvMsg. NPWD " & msg.packet_message_NPWD)
        'VERS   ' Version number
        logger.Fatal("onRecvMsg. VERS " & msg.packet_message_VERS)
        'LAdC   ' Address for VSMSC list operation
        logger.Fatal("onRecvMsg. LADC " & msg.packet_message_LAdC)
        'LTON   ' Type of Number list address
        logger.Fatal("onRecvMsg. LTON " & msg.packet_message_LTON)
        'LNPI   ' Numbering Plan Id list address
        logger.Fatal("onRecvMsg. LNPI " & msg.packet_message_LNPI)
        'OPID   ' Originator Protocol Identifier
        logger.Fatal("onRecvMsg. OPID " & msg.packet_message_OPID)
        'RES1   ' (reserved for future use)
        logger.Fatal("onRecvMsg. RES1 " & msg.packet_message_RES1)
        'Xser   ' (reserved for future use)
        logger.Fatal("onRecvMsg. Xser " & msg.packet_message_XSer)

        Return ""
    End Function

    Public Sub ProcessMO(ByVal msg As UCPPacket)

        Dim TempMessage() As String
        Dim KeywordID As Integer = 0
        Dim FirstKeyword As String = ""
        Dim SecondKeyword As String = ""
        Dim strResult As String = ""
        Dim TempMessage1 As String = ""

        Try
            logger.Info("[PROCESSING MO spliting the msg keyword]")
            logger.Info("[MESSAGE] " & msg.Message)
            TempMessage1 = msg.Message
            TempMessage = Split(TempMessage1, " ").RemoveGap  ' split the message by spacing

            If TempMessage.Count > 0 Then
                FirstKeyword = UCase(TempMessage(0)) ' get first keyword and upper case the keyword 
            End If

            If TempMessage.Count > 1 Then
                SecondKeyword = UCase(TempMessage(1)) ' get second keyword upper case the keyword 
            End If

            logger.Info("[INCOMING DATA] First: " & FirstKeyword & "; Second: " & SecondKeyword)

            If FirstKeyword.Trim.Contains("DELIVERY") Then 'Update DN

                logger.Info("[DN DETAILS] Telco ID: " & telcoid & "; MSISDN: " & msg.Originator & "; Message: " & msg.Message)

                If msg.Originator <> "" And msg.Message <> "" Then

                    'Check if MIN is subscribed to CPLAY
                    Dim sql_check_min As String = "SELECT * FROM premium_sms_db.subscriber WHERE msisdn = '" & msg.Originator & "' AND keyword_id IN " & dn_subscription & ""
                    Dim ds_subs As DataSet = getQuery(sql_check_min, "subscriber")
                    If ds_subs.Tables("subscriber").Rows.Count > 0 Then
                        'Get MT details
                        Dim sql_get_mt As String = "SELECT msgguid FROM premium_sms_db.mt_sun WHERE msisdn = '" & msg.Originator & "' AND msgguid <> '' AND dn_code = '' AND keyword_id IN " & dn_subscription & " AND LEFT(TIMESTAMP,10) = LEFT(NOW(),10) AND (TIMESTAMP BETWEEN DATE_SUB(CURRENT_TIMESTAMP , INTERVAL 5 MINUTE) AND NOW()) ORDER BY id DESC LIMIT 1"
                        Dim ds_mt As DataSet = getQuery(sql_get_mt, "mt")
                        If ds_mt.Tables("mt").Rows.Count > 0 Then
                            Dim msgguid As String
                            msgguid = ds_mt.Tables("mt").Rows(0).Item("msgguid").ToString

                            'POST DN to DN receiver
                            httpPost(dn_receiver, "rrn=" & msgguid & "&status=SUCCESS&telco_id=2")
                        End If

                        updateDN(msg.Originator)
                    Else
                        'Checking for Info On Demand
                        'Get MT details
                        Dim sql_get_mt As String = "SELECT msgguid FROM premium_sms_db.mt_sun WHERE msisdn = '" & msg.Originator & "' AND msgguid <> '' AND dn_code = '' AND keyword_id IN " & dn_iod & " AND LEFT(TIMESTAMP,10) = LEFT(NOW(),10) AND (TIMESTAMP BETWEEN DATE_SUB(CURRENT_TIMESTAMP , INTERVAL 5 MINUTE) AND NOW()) ORDER BY id DESC LIMIT 1"
                        Dim ds_mt As DataSet = getQuery(sql_get_mt, "mt")
                        If ds_mt.Tables("mt").Rows.Count > 0 Then
                            Dim msgguid As String
                            msgguid = ds_mt.Tables("mt").Rows(0).Item("msgguid").ToString

                            'POST DN to DN receiver
                            httpPost(dn_receiver, "rrn=" & msgguid & "&status=SUCCESS&telco_id=2")
                        End If

                        updateDN(msg.Originator)
                    End If
                End If

            Else

                logger.Info("[MO DETAILS] First: " & FirstKeyword & "; Second: " & SecondKeyword)

                If FirstKeyword <> "MESSAGES" Then
                    Dim clsSunMO As New SunMO
                    With clsSunMO
                        .firstKeyword = FirstKeyword
                        .secondKeyword = SecondKeyword
                        .msisdn = msg.Originator
                        .telcoID = telcoid
                        .msgid = ""
                        .shortcode = ORIGINATOR
                        .message = msg.Message

                        If .getKeywordDetails = True Then
                        Else
                            If .getFirstKeywordDetails = False Then
                                .keywordID = 106
                                .sendInvalidMessage()
                                .insertInbox()
                                Exit Sub
                            End If
                        End If

                        If .keywordType = 1 Then
                            .postMOToCP()
                        ElseIf .keywordType = 2 Then
                            If .reserveKeywordType = 1 Then
                                If .isSubscriptionExist = True Then
                                    If .isSubscriberActive = True Then
                                        .sendDoubleOptInMessage()
                                        .postMOToCP()
                                    Else
                                        If .keywordID <> "305" Then
                                            .sendOptInMessage()
                                        End If
                                        .activateSubscriber()
                                        .postMOToCP()
                                    End If
                                Else
                                    If .keywordID <> "305" Then
                                        .sendOptInMessage()
                                    End If
                                    .addSubscriber()
                                    .postMOToCP()
                                End If
                            ElseIf .reserveKeywordType = 2 Then
                                If .isSubscriptionExist = True Then
                                    If .isSubscriberActive = True Then
                                        .sendOptOutMessage()
                                        .deactivateSubscriber()
                                        .postMOToCP()
                                    Else
                                        .sendDoubleOptOutMessage()
                                        .postMOToCP()
                                    End If
                                Else
                                    .sendDoubleOptOutMessage()
                                    .postMOToCP()
                                End If
                            ElseIf .reserveKeywordType = 5 Then
                                .sendStopAllMessage()
                                .deactivateAllSubscription()
                                .postMOToCP()
                            ElseIf .reserveKeywordType = 3 Then
                                .sendHelpMessage()
                            ElseIf .reserveKeywordType = 4 Then
                                .sendCheckMessage()
                            End If
                        End If
                        .insertInbox()
                    End With
                End If

            End If

        Catch ex As Exception
            logger.Info("[SUN PROCESS MO Error]", ex)
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Function getObjInfo(ByVal objMessage As LibraryDAL.Tbl_MOStr, ByVal RetryCount As Integer) As String
        Dim myText As String = ""

        myText = "KeywordID=" + objMessage.KeywordID.ToString & ";" & _
                "MSISDN=" + objMessage.MSISDN + ";" & _
                "TransactionID=" + objMessage.TransactionID + ";" & _
                "DataCoding=" + objMessage.DataCoding.ToString + ";" & _
                "TelcoID=" + objMessage.TelcoID.ToString + ";" & _
                "ReceiveDate=" + objMessage.ReceiveDate.ToString + ";" & _
                "RetryCount=" + RetryCount.ToString

        Return myText
    End Function

    Private Sub onDebug(ByVal msg As String)
        logger.Fatal("sms_cli." & msg)
    End Sub

    Private Sub onLoginsucceded()
        logger.Fatal("Yes, were logged in!")
        logger.Info("Yes, were logged in!")
    End Sub

    Private Sub onLoginfailed(ByVal reason As String)
        logger.Fatal("Login failed: " & reason)
        logger.Info("Login failed: " & reason)
    End Sub

    Private Sub onConnect()
        logger.Fatal("Connected to SMSC")
        logger.Info("Connected to SMSC")
    End Sub

    Private Sub onDisconnect()
        logger.Fatal("Disconnected from SMSC")
        logger.Info("Disconnected from SMSC")

        StopListen()
        'StartUCPConnect()
        'bulkInsert()
    End Sub

    Private Sub onKeepAlive()
        logger.Fatal("Sending keep alive message..")
        logger.Info("Sending keep alive message..")

        'sms_cli.sendKeepAlive()
        'StartUCPConnect()
        'bulkInsert()
    End Sub

    Private Sub httpPost(ByVal url As String, ByVal parameters As String)

        Try
            Dim request As WebRequest = WebRequest.Create(url)
            request.Method = "POST"
            Dim postData As String = parameters
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            dataStream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()
            reader.Close()
            dataStream.Close()
            response.Close()
            logger.Info("[SUN DN POST TO 205] " & url & "?" & parameters)
        Catch ex As Exception
            logger.Info("[SUN DN FAILED POST TO 205] " & url & "?" & parameters, ex)
        End Try

    End Sub

    'UCP API Ends Here

    'SMPP API Start Here
    Private client As SmppClient
    Public Sub StartSMPPConnect()
        client = New SmppClient()
        client.Timeout = 60000
        client.NeedEnquireLink = True
        AddHandler client.evConnect, AddressOf client_evConnect
        AddHandler client.evDisconnect, AddressOf client_evDisconnect
        AddHandler client.evDeliverSm, AddressOf client_evDeliverSm
        AddHandler client.evEnquireLink, AddressOf client_evEnquireLink
        AddHandler client.evGenericNack, AddressOf client_evGenericNack
        AddHandler client.evError, AddressOf client_evError
        AddHandler client.evReceiveData, AddressOf client_evReceiveData
        AddHandler client.evSendData, AddressOf client_evSendData
        AddHandler client.evUnBind, AddressOf client_evUnBind
        AddHandler client.evDataSm, AddressOf client_evDataSm
        AddHandler client.evSubmitComplete, AddressOf client_evSubmitComplete
        AddHandler client.evQueryComplete, AddressOf client_evQueryComplete

        SMPPConnect()
        SMPPBind()
    End Sub

    Private Sub SMPPConnect()

        If client.Status = ConnectionStatus.Closed Then
            client.AddrNpi = 1 'Convert.ToByte(tbAddrNpi.Text)
            client.AddrTon = 1 'Convert.ToByte(tbAddrTon.Text)
            client.SystemType = 1 'tbSystemType.Text

            client.Connect(SERVER, Convert.ToInt32(PORT), True)
        End If

    End Sub

    Private Sub SMPPDisconnect()
        If client.Status = ConnectionStatus.Bound Then
            SMPPUnBind()
        End If

        If client.Status = ConnectionStatus.Open Then
            client.Disconnect()
        End If
    End Sub

    Private Sub SMPPBind()

        Dim btrp As pduBindResp = client.Bind(USERNAME, PASSWORD, ConnectionMode.Transceiver)

        Select Case btrp.Status
            Case CommandStatus.ESME_ROK
                AddToLog("SmppClient bound")
                AddToLog("Bind result : system is " + btrp.SystemId + " with status " + btrp.Status.ToString())
                Exit Select
            Case Else
                AddToLog("Bad status returned during Bind : " + btrp.Command.ToString() + " with status " + btrp.Status.ToString())
                Disconnect()
                Exit Select
        End Select

    End Sub

    Private Sub SMPPUnBind()
        AddToLog("Unbinding SmppClient")
        Dim ubtrp As pduUnBindResp = client.UnBind()

        Select Case ubtrp.Status
            Case CommandStatus.ESME_ROK
                AddToLog("SmppClient unbound")
                AddToLog("UnBind result with status " + ubtrp.Status.ToString())
                Exit Select
            Case Else
                AddToLog("Bad status returned during UnBind : " + ubtrp.Command.ToString() + " with status " + ubtrp.Status.ToString())
                client.Disconnect()
                Exit Select
        End Select

    End Sub


    Private Sub client_evDisconnect(ByVal sender As Object)
        AddToLog("SmppClient disconnected")
        AddToLog("SmppClient Re-Connecting..........")
        StartSMPPConnect()
    End Sub

    Private Sub client_evConnect(ByVal sender As Object, ByVal bSuccess As Boolean)
        If bSuccess Then
            AddToLog("SmppClient connected")
        End If
    End Sub

    Private collector As New Dictionary(Of String, UserData())()

    Private Sub AddMessageSegmentToCollector(ByVal data As DeliverSm)
        Dim userDataArray As UserData() = Nothing
        If collector.ContainsKey(data.SourceAddr + data.MessageReferenceNumber) Then
            userDataArray = DirectCast(collector(data.SourceAddr + data.MessageReferenceNumber), UserData())
        Else
            userDataArray = New UserData(data.TotalSegments - 1) {}
        End If

        userDataArray(data.SegmentNumber - 1) = data.UserDataPdu

        collector(data.SourceAddr + data.MessageReferenceNumber) = userDataArray
    End Sub

    Private Function IsLastSegment(ByVal data As DeliverSm) As Boolean
        Dim finished As Boolean = False

        Dim userDataArray As UserData() = Nothing
        Dim key As String = data.SourceAddr + data.MessageReferenceNumber
        If collector.ContainsKey(key) Then
            userDataArray = DirectCast(collector(key), UserData())

            finished = True

            For Each d As UserData In userDataArray
                If d Is Nothing Then
                    finished = False
                    Exit For
                End If
            Next
        End If

        Return finished
    End Function

    Private Function RetrieveFullMessage(ByVal data As DeliverSm) As String
        Dim message As String = Nothing
        Dim key As String = data.SourceAddr + data.MessageReferenceNumber
        If collector.ContainsKey(key) Then
            Dim userDataArray As UserData() = DirectCast(collector(key), UserData())

            Dim fullUserData As UserData = Nothing
            For Each d As UserData In userDataArray
                fullUserData += d
            Next

            message = SmppClient.GetMessageText(fullUserData.ShortMessage, data.DataCoding)


            collector.Remove(key)
        End If

        Return message
    End Function

    Private Sub client_evDeliverSm(ByVal sender As Object, ByVal data As DeliverSm)
        If data.SegmentNumber > 0 Then

            AddMessageSegmentToCollector(data)

            Dim messageText As String = SmppClient.GetMessageText(data.UserDataPdu.ShortMessage, data.DataCoding)

            AddToLog("DeliverSm part received : " + " Sequence : " + data.Sequence.ToString() + " SourceAddr : " + data.SourceAddr + " Segments ( Number: " + data.SegmentNumber.ToString() + ", Total : " + data.TotalSegments.ToString() + ", Reference : " + data.MessageReferenceNumber.ToString() + " ) Coding : " + data.DataCoding.ToString() + " MessageText : " + messageText)


            If IsLastSegment(data) Then

                Dim fullMessage As String = RetrieveFullMessage(data)

                AddToLog("Full message: " + fullMessage)

            End If
        Else
            Dim messageText As String = SmppClient.GetMessageText(data.UserDataPdu.ShortMessage, data.DataCoding)


            AddToLog("DeliverSm received : " + " Sequence : " + data.Sequence.ToString() + " SourceAddr : " + data.SourceAddr + " Coding : " + data.DataCoding.ToString() + " MessageText : " + messageText)
        End If

        ' Here you can change DeliverSmResp status
        ' data.Response.Status = CommandStatus.ESME_RINVCMDID;
    End Sub


    Private Sub client_evEnquireLink(ByVal sender As Object, ByVal data As EnquireLink)
        AddToLog("EnquireLink received")
    End Sub


    Private Sub client_evGenericNack(ByVal sender As Object, ByVal data As GenericNack)
        AddToLog("GenericNack received with status " + data.Status.ToString())
    End Sub

    Private Sub client_evError(ByVal sender As Object, ByVal args As SmppClientErrorEventArgs)
        AddToLog("ERROR:" + args.Comment)
        If Not args.Exception Is Nothing Then
            AddToLog("Exception:" + args.Exception.ToString())
        End If
    End Sub


    Private Sub client_evSendData(ByVal sender As Object, ByVal data As Byte())
        AddToLog("Sending Data: " + ByteArrayToHexString(data))
    End Sub

    Public Shared Function ByteArrayToHexString(ByVal buf As Byte()) As String
        If buf Is Nothing Then
            Return ""
        End If

        Dim sb As New System.Text.StringBuilder(buf.Length * 2 + 2)
        Dim i As Integer = 0
        While i < buf.Length
            sb.Append(buf(i).ToString("x2"))
            System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
        End While
        Return sb.ToString()
    End Function

    Private Sub client_evReceiveData(ByVal sender As Object, ByVal data As Byte())
        AddToLog("Received Data: " + ByteArrayToHexString(data))
    End Sub


    Private Sub client_evUnBind(ByVal sender As Object, ByVal data As pduUnBind)
        AddToLog("UnBind request received")
    End Sub

    Private Sub client_evSubmitComplete(ByVal sender As Object, ByVal data As SubmitSmResp)
        AddToLog("SubmitSmResp received." + " Status: " + data.Status + ", Message Id: " + data.MessageId + ", Sequence: " + data.Sequence.ToString())
    End Sub

    Private Sub client_evQueryComplete(ByVal sender As Object, ByVal data As QuerySmResp)
        AddToLog("QuerySmResp received." + " Status: " + data.Status + ", Message Id: " + data.MessageId + ", Sequence: " + data.Sequence.ToString() + ", Message State: " + data.MessageState.ToString())
    End Sub


    Private Sub client_evDataSm(ByVal sender As Object, ByVal data As DataSm)
        Dim messageText As String = SmppClient.GetMessageText(data.UserDataPdu.ShortMessage, data.DataCoding)

        AddToLog("DataSm received : Reference Number :" + data.MessageReferenceNumber.ToString() + ", Sequence: " + data.Sequence.ToString() + ", SourceAddr: " + data.SourceAddr + ", DestAddr: " + data.DestAddr + ", Sequence Number: " + data.SequenceNumber.ToString() + ", Total Segments: " + data.TotalSegments.ToString() + ", Coding: " + data.DataCoding.ToString() + ", Text: " + messageText)
    End Sub



    Private Sub AddToLog(ByVal text As String)
        logger.Info(text)
    End Sub


    'SMPP API End Here


    'SMSC Client API Start Here

    Dim WithEvents SMSClient As SMSCclientUCP = New SMSCclientUCP

    Public Sub StartConnecting()
        Connect()
        Bind()
    End Sub

    ' Connect to SMSC
    Private Sub Connect()
        Dim Result As Integer

        Result = SMSClient.tcpConnect(SERVER, CInt(PORT), "")

        If Result <> 0 Then
            'Falied
            'Console.WriteLine("Failed to Connect " & Result)
            logger.Fatal("Failed to Connect " & Result)
        Else
            'Sucess
            'Console.WriteLine("Success Connected " & Result)
            logger.Fatal("Success Connected " & Result)
        End If
    End Sub

    ' Disonnect from SMSC
    Private Sub Disconnect()
        SMSClient.tcpDisconnect() '.tcpDisconnect()
        logger.Fatal("Success Disconnected ")
    End Sub

    ' Initialize session with SMSC
    Private Sub Bind()
        Dim Result As Integer

        Result = SMSClient.ucpInitializeSession(USERNAME, CByte("1"), CByte("1"), PASSWORD, True, "")

        If Result <> 0 Then
            'Falied
            'Console.WriteLine("Failed to Connect " & Result)
            logger.Fatal("Failed to Login " & Result)
        Else
            'Sucess
            'Console.WriteLine("Success Connected " & Result)
            logger.Fatal("Success Login " & Result)
        End If
    End Sub

    ' Submit message to the SMSC
    Private Sub sendSMS(ByVal body As SendSMSAPIStrc)
        Dim Result As Integer
        Dim SystemMessage As String
        Dim TimeStamp As Date
        Dim Encoding As EncodingEnum
        Dim Options As Integer

        SystemMessage = ""

        Select Case body.MsgType
            Case 2
                Encoding = EncodingEnum.etUCS2Text
            Case 1
                Encoding = EncodingEnum.et8BitHexadecimal
            Case Else
                Encoding = EncodingEnum.et7BitText
        End Select

        Options = SubmitOptionEnum.soRequestStatusReport
        ' If chkStatusReport.CheckState = CheckState.Checked Then Options = Options Or SubmitOptionEnum.soRequestStatusReport
        ' If chkDirectDisplay.CheckState = CheckState.Checked Then Options = Options Or SubmitOptionEnum.soDirectDisplay

        Result = SMSClient.ucpSubmitMessage(body.MSISDN, ORIGINATOR, body.Message, Encoding, "UDH", Options, "", TimeStamp)

        If Result <> 0 Then
            'failed
        Else
            'success
        End If
    End Sub

    ' Disconnected from SMSC
    'Private Sub SMSCRelayUCP_OnTcpDisconnected(ByVal sender As Object, ByVal e As tcpDisconnectedEventArgs) Handles SMSCclientUCP.OnTcpDisconnected
    Private Sub SMSCRelayUCP_OnTcpDisconnected(ByVal sender As Object, ByVal e As tcpDisconnectedEventArgs) Handles SMSClient.OnTcpDisconnected
        'Log.AddDisconnectedEvent(e.Reason)
        'Console.WriteLine("Disconnecting............ " & e.Reason)
        logger.Fatal("Disconnecting......... Performing Reconnecting............")
        StartConnecting()
    End Sub

    ' Message received from SMSC
    Private Sub SMSCRelayUCP_OnUcpMessageReceived(ByVal Sender As Object, ByVal e As ucpMessageReceivedEventArgs) Handles SMSClient.OnUcpMessageReceived
        'Log.AddMessageReceivedEvent(e.Destination, e.Originator, e.SMText, e.Encoding, e.UserDataHeader, e.TimeStamp)
        'Console.WriteLine("Msg Received............ " & e.Destination & " " & e.Originator & " " & e.SMText & " " & e.Encoding & " " & e.UserDataHeader & " " & e.TimeStamp)

        logger.Info("SUN MO Be4 Process: Destination=" & e.Destination & "; Originator=" & e.Originator & "; Msg=" & e.SMText & "; Decoding=" & e.Encoding & "; UDH=" & e.UserDataHeader & "; Date=" & e.TimeStamp)


        Dim TempMessage() As String
        Dim MOUrl As String = ""
        Dim sendString As String = ""
        Dim KeywordID As Integer = 0

        Dim dt As DataSet
        Dim FirstKeyword As String = ""
        Dim SecondKeyword As String = ""
        Dim strResult As String = ""
        Dim TempMessage1 As String = ""


        Try

            TempMessage1 = e.SMText
            TempMessage = Split(TempMessage1, " ").RemoveGap  ' split the message by spacing

            If TempMessage.Count > 0 Then
                FirstKeyword = UCase(TempMessage(0)) ' get first keyword and upper case the keyword 
            End If

            If TempMessage.Count > 1 Then
                SecondKeyword = UCase(TempMessage(1)) ' get second keyword upper case the keyword 
            End If

            dt = CheckKeyword(FirstKeyword, SecondKeyword, e.Originator, TELCOID) ' check for first and second keyword


            If dt.Tables(0).Rows.Count <= 0 Then
                dt = CheckKeyword(FirstKeyword, "", e.Originator, TELCOID)  ' check for first keyword
                If dt.Tables(0).Rows.Count <= 0 Then
                    Dim JunkKeyword As String = "JUNK_" & e.Originator
                    dt = CheckKeyword(JunkKeyword, "", e.Originator, TELCOID) ' check for junk keyword 
                End If
            End If

            If dt.Tables(0).Rows.Count > 0 Then
                For Each r As DataRow In dt.Tables(0).Rows
                    KeywordID = r.Item("KeywordID")
                    MOUrl = r.Item("rMOUrl")
                    If MOUrl = "" Then ' if no reserve keyword url  then get keyword url (cp apps url)
                        MOUrl = r.Item("kMOUrl")
                    End If
                Next
            End If

            'post content to URL
            If MOUrl <> "" Then
                sendString = "from=" & System.Web.HttpUtility.UrlEncode(e.Destination) _
                & "&msgid=" & System.Web.HttpUtility.UrlEncode(e.UserDataHeader) _
                & "&message=" & System.Web.HttpUtility.UrlEncode(e.SMText) _
                & "&sc=" & System.Web.HttpUtility.UrlEncode(e.Originator) _
                & "&telcoid=" & System.Web.HttpUtility.UrlEncode(TELCOID) _
                & "&serviceid=" & System.Web.HttpUtility.UrlEncode(KeywordID)

                'forward to MO Forward Queue
                Dim MOForwardStrc As New LibraryDAL.MOForwardStrc
                With MOForwardStrc
                    .URL = MOUrl
                    .URLData = sendString
                End With
                SendMOForwardQueue(MOForward_queue, MOForwardStrc)

            End If


            'insert inbox mo
            With MOType
                .TelcoID = TELCOID
                .MSISDN = e.Originator
                .TransactionID = e.UserDataHeader
                .KeywordID = KeywordID
                .DataCoding = e.Encoding
                .ReceiveDate = e.TimeStamp
                .Message = e.SMText
            End With

            sendToMOQueue(inbox_queue, MOType)


            logger.Info("[SUN_MO Processed]" & e.Destination & "; Originator=" & e.Originator & "; Msg=" & e.SMText & "; Decoding=" & e.Encoding & "; UDH=" & e.UserDataHeader & "; Date=" & e.TimeStamp)

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
            sendToMOQueue(MODeath, MOType)
        End Try
    End Sub

    ' Status Report (SR) received from SMSC
    Private Sub SMSCRelayUCP_OnUcpStatusReportReceived(ByVal Sender As Object, ByVal e As ucpStatusReportReceivedEventArgs) Handles SMSClient.OnUcpStatusReportReceived
        'Log.AddStatusReportReceivedEvent(e.MessageTimeStamp, e.Destination, e.Originator, e.SMStatus, e.FailureReason, e.TimeStamp)
        ' Console.WriteLine("UCP Status Report Received............ " & e.Destination & " " & e.Originator & " " & e.SMText & " " & e.SMStatus & " " & e.FailureReason & " " & e.TimeStamp)
    End Sub

End Class
