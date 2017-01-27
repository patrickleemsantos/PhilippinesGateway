Imports ALT.SMS
Imports System.Messaging
Imports System.Configuration
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.Net
Imports System.Threading
Imports LibraryDAL

Public Delegate Sub AddToDelegate2(ByVal data As DeliverSm) 'new

Public Delegate Sub queueDelegate1(ByVal body As SendSMSAPIStrc)

Public Class EGGSMPP
    Inherits System.ServiceProcess.ServiceBase
    Private delqueue1 As queueDelegate1

    Private del2 As AddToDelegate2
    Private collector As New Dictionary(Of String, UserData())()

    Private client As New SmppClient

    Private _queue1 As MessageQueue

    Private _a As Integer = 0
    Private _listen As Boolean = False
    Private alpha As Boolean = False

    Private strSMPPUser As String = ""
    Private strSMPPPass As String = ""
    Private strSMPPSMSC As String = ""

    Private sys_id As String = ConfigurationManager.AppSettings("sys_id")
    Private sys_pass As String = ConfigurationManager.AppSettings("sys_pass")
    Private sys_host As String = ConfigurationManager.AppSettings("sys_host")
    Private sys_port As String = ConfigurationManager.AppSettings("sys_port")
    Private sys_type As String = ConfigurationManager.AppSettings("sys_type")
    Private sys_addr_ton As String = ConfigurationManager.AppSettings("sys_addr_ton")
    Private sys_addr_npi As String = ConfigurationManager.AppSettings("sys_addr_npi")
    Private sys_con_mode As String = ConfigurationManager.AppSettings("sys_con_mode")
    Private sys_timeout_interval As String = ConfigurationManager.AppSettings("sys_timeout_interval")
    Private sys_enquiry_interval As String = ConfigurationManager.AppSettings("sys_enquiry_interval")
    Private src_npi As String = ConfigurationManager.AppSettings("src_npi")
    Private src_ton As String = ConfigurationManager.AppSettings("src_ton")
    Private dest_npi As String = ConfigurationManager.AppSettings("dest_npi")
    Private dest_ton As String = ConfigurationManager.AppSettings("dest_ton")

    Private gateway_queue As String = ConfigurationManager.AppSettings("gateway_queue")
    Private death_queue As String = ConfigurationManager.AppSettings("death_queue")
    Private update_queue As String = ConfigurationManager.AppSettings("outbox_queue")
    Private dn_queue As String = ConfigurationManager.AppSettings("dn_queue")

    Private priority As String = ConfigurationManager.AppSettings("priority")
    Private RetryCount As Integer = CInt(ConfigurationManager.AppSettings("RetryCount"))
    Private telcoid As String = ConfigurationManager.AppSettings("telcoid")
    Private smpp_smsc As String = ConfigurationManager.AppSettings("smpp_smsc")


    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        log4net.Config.XmlConfigurator.Configure()
        _queue1 = New MessageQueue(gateway_queue)

        MessageQueue.EnableConnectionCache = False

        delqueue1 = New queueDelegate1(AddressOf send2SMPP)

        del2 = New AddToDelegate2(AddressOf PostMO)

        client.Timeout = CInt(sys_timeout_interval)
        client.NeedEnquireLink = True
        client.EnquireInterval = CInt(sys_enquiry_interval)
        client.AddrNpi = Convert.ToByte(sys_addr_npi)
        client.AddrTon = Convert.ToByte(sys_addr_ton)
        client.SystemType = sys_type

        AddHandler _queue1.PeekCompleted, AddressOf OnPeekCompleted1

        AddHandler client.evError, AddressOf client_evError
        'AddHandler client.evDisconnect, AddressOf client_evDisconnect
        AddHandler client.evEnquireLink, AddressOf client_evEnquireLink
        AddHandler client.evDeliverSm, AddressOf client_evDeliverSm
        AddHandler client.evGenericNack, AddressOf client_evGenericNack
        AddHandler client.evSubmitComplete, AddressOf client_evSubmitComplete

        Connect()
    End Sub

    Protected Overrides Sub OnStop()
        RemoveHandler _queue1.PeekCompleted, AddressOf OnPeekCompleted1

        RemoveHandler client.evError, AddressOf client_evError
        'RemoveHandler client.evDisconnect, AddressOf client_evDisconnect
        RemoveHandler client.evEnquireLink, AddressOf client_evEnquireLink
        RemoveHandler client.evGenericNack, AddressOf client_evGenericNack
        RemoveHandler client.evSubmitComplete, AddressOf client_evSubmitComplete

        StopListen()
        Disconnect()
    End Sub

    Private Sub Connect()
        Try
            If client.Status = ConnectionStatus.Closed Then
                'client.Connect(sys_host, CInt(sys_port.ToString))   ' for local host testing 
                Dim endpoint As New IPEndPoint(IPAddress.Parse(sys_host), CInt(sys_port)) ' for production line (live)
                client.Connect(endpoint)
            End If

            If client.Status = ConnectionStatus.Open Then
                Bind()
            End If
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Sub Disconnect()
        If client.Status = ConnectionStatus.Bound Then
            UnBind()
        End If

        If client.Status = ConnectionStatus.Open Then
            client.Disconnect()
        End If
    End Sub

    Private Sub Bind()
        Dim btrp As pduBindResp = client.Bind(sys_id, sys_pass, CType(sys_con_mode, ConnectionMode))

        Select Case btrp.Status
            Case CommandStatus.ESME_ROK
                logger.Fatal("Conn - Status returned during Bind : " + btrp.Command.ToString() + " with status " + btrp.Status.ToString())
                StartListen()
                Exit Select
            Case Else
                logger.Fatal("Conn - Bad status returned during Bind : " + btrp.Command.ToString() + " with status " + btrp.Status.ToString())
                Disconnect()
                Connect()
                Exit Select
        End Select

    End Sub

    Private Sub UnBind()
        Dim ubtrp As pduUnBindResp = client.UnBind()

        Select Case ubtrp.Status
            Case CommandStatus.ESME_ROK
                logger.Fatal("DisConn - Status returned during UnBind : " + ubtrp.Command.ToString() + " with status " + ubtrp.Status.ToString())
                Exit Select
            Case Else
                logger.Fatal("DisConn - Bad status returned during UnBind : " + ubtrp.Command.ToString() + " with status " + ubtrp.Status.ToString())
                client.Disconnect()
                Exit Select
        End Select
    End Sub

    Private Sub StartListen()
        Try
            _listen = True
            _queue1.Formatter = New XmlMessageFormatter(New System.Type() {GetType(SendSMSAPIStrc)})

            StartListening1()
        Catch exy As MessageQueueException
            logger.Fatal("Message queue error code : " & exy.MessageQueueErrorCode & ". Please make sure the Message queue is running and then restart this service.")
            Dim myProcess As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
            myProcess.Kill()
        Catch ex As Exception
            logger.Fatal("[FATAL] Message : " & ex.Message)
            logger.Fatal("[FATAL] Source : " & ex.Source)
            logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)
        End Try
    End Sub

    Private Sub StopListen()
        _listen = False
    End Sub

    Private Sub StartListening1()
        If Not _listen Then
            Exit Sub
        End If
        _queue1.BeginPeek()
    End Sub

    Private Sub OnPeekCompleted1(ByVal sender As Object, ByVal e As PeekCompletedEventArgs)
        Try
            _queue1.EndPeek(e.AsyncResult)
            Dim trans As New MessageQueueTransaction()
            Dim msg As Message = Nothing
            trans.Begin()
            msg = _queue1.Receive(trans)
            trans.Commit()
            delqueue1.Invoke(CType(msg.Body, SendSMSAPIStrc))
            StartListening1()
        Catch ex As Exception
            logger.Fatal("[FATAL] Message : " & ex.Message)
            logger.Fatal("[FATAL] Source : " & ex.Source)
            logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)
        End Try

    End Sub

    Private Sub client_evError(ByVal sender As Object, ByVal args As SmppClientErrorEventArgs)
        If Not args.Exception Is Nothing Then
            logger.Fatal("[FATAL] Comment : " & args.Comment)
            logger.Fatal("[FATAL] Exception : " & args.Exception.ToString())
            logger.Fatal("[FATAL] Message : " & args.Exception.Message.ToString)
            logger.Fatal("[FATAL] StackTrace : " & args.Exception.StackTrace.ToString)
            logger.Fatal("[FATAL] InnerException : " & args.Exception.InnerException.ToString)
        End If

        Dim emailcontent As String = "SMPP Client (" & smpp_smsc & ") have encounter an error exception. Below is the details of the error : <br/><br />" & _
            "Error Message : " & args.Exception.Message.ToString & " <br/>" & _
            "Source : " & args.Exception.Source.ToString & " <br/>" & _
            "Stacktrace : " & args.Exception.StackTrace.ToString & " <br/>" & _
            "InnerException : " & args.Exception.InnerException.ToString & " <br/>" & _
            "Date time : " & Now.AddDays(25).ToString("yyyy-MM-dd HH:mm:ss")

        Dim smscontent As String = "SMPP Client (" & smpp_smsc & ") have encontered an error exception. Please consult email/logs for details of the error. Thank you"

        Try
            MailSender.SendEmail(emailcontent)
            Dim myProcess As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
            myProcess.Kill()
        Catch ex As Exception
            logger.Fatal("[FATAL] Message : " & ex.Message)
            logger.Fatal("[FATAL] Source : " & ex.Source)
            logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)
        End Try

    End Sub

    Private Sub client_evDisconnect(ByVal sender As Object)
    End Sub

    Private Sub client_evEnquireLink(ByVal sender As Object, ByVal data As EnquireLink)
    End Sub

    Private Sub client_evDeliverSm(ByVal sender As Object, ByVal data As DeliverSm)
        del2.Invoke(data)
    End Sub

    Private Sub client_evSubmitComplete(ByVal sender As Object, ByVal data As SubmitSmResp)
        logger.Fatal("SubmitSmResp received. Status: " + data.Status.ToString + ", Message Id: " + data.MessageId + ", Sequence: " + data.Sequence.ToString())
    End Sub

    Private Sub client_evGenericNack(ByVal sender As Object, ByVal data As GenericNack)
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

    Private Function HexStringToByteArray(ByVal str As String) As Byte()
        If str.Length = 0 OrElse str.Length Mod 2 <> 0 Then
            Return Nothing
        End If

        Dim len As Integer = CInt(str.Length / 2)
        Dim buf As Byte() = New Byte(len - 1) {}

        Dim i As Integer = 0
        While i < len
            buf(i) = Byte.Parse(str.Substring(i * 2, 2), NumberStyles.HexNumber)
            System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
        End While
        Return buf
    End Function

    Private Function splitSms(ByVal msgContent As String, ByVal characterPerMsg As Int16) As String()

        Dim msgBytesCount As Int16 = CShort(System.Text.Encoding.ASCII.GetByteCount(msgContent) - 1)
        Dim msgCount As Int16 = 1
        msgCount = msgCount + msgBytesCount \ characterPerMsg

        Dim arrayString(msgCount - 1) As String
        Dim startPosition As Int16 = 0
        Dim i As Int16
        For i = 0 To CShort(msgCount - 1)
            startPosition = 0
            If msgContent.Length >= characterPerMsg Then
                arrayString(i) = msgContent.Substring(startPosition, characterPerMsg)
                startPosition = startPosition + characterPerMsg
                msgContent = msgContent.Substring(startPosition, msgContent.Length - characterPerMsg)
            Else
                arrayString(i) = msgContent
            End If
        Next
        Return arrayString
    End Function

    Public Function isNumeric(ByVal val As String, ByVal NumberStyle As System.Globalization.NumberStyles) As Boolean
        Dim result As Double
        Return Double.TryParse(val, NumberStyle, System.Globalization.CultureInfo.CurrentCulture, result)
    End Function

    Private Sub send2SMPP(ByVal body As SendSMSAPIStrc)
        If client.Status <> ConnectionStatus.Bound Then
            dumpToQueue(gateway_queue, body)
            StopListen()
            Connect()
        Else
            PostMsg(body)
        End If
    End Sub

    Private Sub PostMsg(ByVal body As SendSMSAPIStrc)

        If body.MsgType = MsgType.TextSMS Then
            sendMsg(body, DataCodings.Default)
        End If

        strSMPPUser = ""
        strSMPPPass = ""
        strSMPPSMSC = ""
    End Sub

    'Public Sub sendRPW(ByVal body As SendSMSAPIStrc)
    '    Try
    '        Dim resp As SubmitSmResp = Nothing
    '        Dim strArrayMessage As String() = Nothing
    '        Dim i As Int16
    '        Dim temp_destno As String = ""

    '        temp_destno = Regex.Replace(body.MsgDestinationNo.TrimStart.TrimEnd.Trim, "[^0-9]", "")
    '        body.MsgDestinationNo = temp_destno

    '        If body.MsgType = "2" Then
    '            strArrayMessage = RTMgr.getRingtone(body.MsgContent)
    '        ElseIf body.MsgType = "5" Then
    '            If (Left(body.MsgContent, 14) = "30000000020100") Then
    '                body.MsgContent = body.MsgContent.Replace(Left(body.MsgContent, 14), "")
    '            End If
    '            strArrayMessage = PMMgr.getPictureMessage(body.MsgContent)
    '        ElseIf body.MsgType = "10" Then
    '            strArrayMessage = WapMgr.Push(body.MsgUrl.Replace("http://", ""), body.MsgContent)
    '        End If

    '        For i = 0 To CShort(strArrayMessage.Length - 1)
    '            Dim data As New SubmitSm
    '            With data
    '                If alpha = True Then
    '                    .SrcTon = CByte("5")
    '                    .SrcNpi = CByte("0")
    '                Else
    '                    .SrcTon = CByte(src_ton)
    '                    .SrcNpi = CByte(src_npi)
    '                End If
    '                .SrcAddr = body.MsgBrandName
    '                .DestTon = CByte(dest_ton)
    '                .DestNpi = CByte(dest_npi)
    '                .DestAddr = body.MsgDestinationNo
    '                .EsmClass = 64
    '                .UserDataPdu = HexStringToByteArray(strArrayMessage(i))
    '                .DataCoding = DataCodings.OctetUnspecified
    '                .RegisteredDelivery = 1
    '            End With

    '            'resp = client.Submit(data)
    '            'body.MsgSMPPID = resp.MessageId
    '            'DN.smppid = resp.MessageId

    '            client.SubmitAsync(data)
    '            body.MsgStatus = "Success"
    '            smsSent(body, DN)
    '        Next

    '        'If resp.Length > 0 AndAlso resp.Status = CommandStatus.ESME_ROK Then
    '        '    smsSent(body, DN)
    '        'Else
    '        '    failSent(body, DN, CStr(resp.Status))
    '        'End If
    '    Catch ex As Exception
    '        logger.Fatal("[FATAL] Message : " & ex.Message)
    '        logger.Fatal("[FATAL] Source : " & ex.Source)
    '        logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)
    '        logger.Fatal(_qm.getObjInfo(body))
    '    End Try
    'End Sub

    'Public Sub sendIconLogo(ByVal body As MessageStructure, ByVal DN As MsgDN)
    '    Try
    '        Dim Msg As String = ""
    '        Dim data As New SubmitSm
    '        Dim temp_destno As String = ""

    '        temp_destno = Regex.Replace(body.MsgDestinationNo.TrimStart.TrimEnd.Trim, "[^0-9]", "")
    '        body.MsgDestinationNo = temp_destno

    '        If body.MsgType = "3" Then
    '            Msg = OLMgr.getOperatorLogo(body.MsgContent, body.MsgDestinationNo)
    '        ElseIf body.MsgType = "4" Then
    '            Msg = IMgr.getIcon(body.MsgContent)
    '        End If

    '        With data
    '            If alpha = True Then
    '                .SrcTon = CByte("5")
    '                .SrcNpi = CByte("0")
    '            Else
    '                .SrcTon = CByte(src_ton)
    '                .SrcNpi = CByte(src_npi)
    '            End If
    '            .SrcAddr = body.MsgBrandName
    '            .DestTon = CByte(dest_ton)
    '            .DestNpi = CByte(dest_npi)
    '            .DestAddr = body.MsgDestinationNo
    '            .EsmClass = 64
    '            .UserDataPdu = HexStringToByteArray(Msg)
    '            .DataCoding = DataCodings.OctetUnspecified
    '            .RegisteredDelivery = 1
    '        End With

    '        'Dim resp As SubmitSmResp = client.Submit(data)
    '        'body.MsgSMPPID = resp.MessageId
    '        'DN.smppid = resp.MessageId

    '        'If resp.Length > 0 AndAlso resp.Status = CommandStatus.ESME_ROK Then
    '        '    smsSent(body, DN)
    '        'Else
    '        '    failSent(body, DN, CStr(resp.Status))
    '        'End If

    '        client.SubmitAsync(data)
    '        body.MsgStatus = "Success"
    '        smsSent(body, DN)
    '    Catch ex As Exception
    '        logger.Fatal("[FATAL] Message : " & ex.Message)
    '        logger.Fatal("[FATAL] Source : " & ex.Source)
    '        logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)
    '        logger.Fatal(_qm.getObjInfo(body))
    '    End Try
    'End Sub

    Public Function sendMsg(ByVal body As SendSMSAPIStrc, ByVal datacoding As DataCodings) As Boolean
        Try
            Dim err As Boolean = False
            Dim strArrayMessage As String() = Nothing
            Dim stat As String = Nothing
            Dim tempstr As String = ""
            Dim temp_destno As String = ""
            Dim total As String = ""
            Dim part As String = ""
            Dim MTID As String = ""

            'temp_destno = Regex.Replace(body.MSISDN.TrimStart.TrimEnd.Trim, "[^0-9]", "")
            'body.MSISDN = temp_destno

            strArrayMessage = splitSms(body.Message, 160)
            tempstr = body.Message

            Dim temp_ton As String = src_ton
            Dim temp_npi As String = src_npi
            Dim resp As New List(Of SubmitSmResp)

            If alpha = True Then
                temp_ton = "5"
                temp_npi = "0"
            End If

            body.SendDate = DateTime.Now  ' set the MT submit date

            Dim req As List(Of SubmitSm) = client.PrepareSubmit(SubmitMode.ShortMessage, CByte(temp_ton), _
                    CByte(temp_npi), body.ShortCode, CByte(dest_ton), CByte(dest_npi), _
                    body.MSISDN, datacoding, tempstr)

            Dim servicetype As String = ""
            If body.Charge = 250 Then ' charge 250
                servicetype = "ew250"
            Else  ' 0 Charge
                servicetype = "cmew"
            End If

            For Each sm As SubmitSm In req
                sm.RegisteredDelivery = 1
                sm.PriorityFlag = CByte(priority)
                sm.ServiceType = servicetype
            Next

            resp = client.Submit(req)
            MTID = resp(0).MessageId

            If resp.Count > 0 AndAlso resp(0).Status = CommandStatus.ESME_ROK Then
                Send2Outbox(body, CStr(resp(0).Status), MTStatus._Success, MTID)
                logger.Info("[SUCCESS]" & getObjInfo(body) & ";MTID=" & MTID & ";Status=" & CStr(MTStatus._Success) & ";StatusCode=" & CStr(resp(0).Status))
            Else
                send2Death(body, CStr(resp(0).Status), MTStatus._Fail, MTID)
            End If

        Catch ex As Exception
            logger.Fatal("[FATAL]" & getObjInfo(body))
            logger.Fatal("[FATAL]", ex)
            send2Death(body, "", MTStatus._Fail, "")
        End Try
    End Function


    Private Sub PostMO(ByVal data As DeliverSm)
        Try
            Dim messageText As String = ""
            Dim fullMessage As String = ""
            Dim Arraybody As String() = Nothing
            Dim str_source_addr As String = ""
            Dim str_dest_addr As String = ""



            If data.SegmentNumber > 0 Then
                AddMessageSegmentToCollector(data)
                messageText = SmppClient.GetMessageText(data.UserDataPdu.ShortMessage, data.DataCoding)
                If IsLastSegment(data) Then
                    fullMessage = RetrieveFullMessage(data)
                End If
                str_source_addr = data.SourceAddr
                str_dest_addr = data.DestAddr
            Else
                messageText = SmppClient.GetMessageText(data.UserDataPdu.ShortMessage, data.DataCoding)
                str_source_addr = data.SourceAddr
                str_dest_addr = data.DestAddr
            End If



            'keep track for incomming message for debugging only
            logger.Info("[Keep_Track_Incomming]SourceAddress:" & str_source_addr & ";DestinationAddr:" & str_dest_addr & ";MessageText:" & messageText & "")

            Dim DnStatus As SMSCDeliveryReceipt = data.SMSCReceipt
            Dim MessageGet As String = "DataCoding=" & data.DataCoding.ToString & ";MessageRefId=" & data.MessageReferenceNumber.ToString & ";MessageStatus="
          
            If messageText.Contains("id:") = True Or messageText.Contains("sub:") = True Or _
            messageText.Contains("dlvrd:") = True Or messageText.Contains("submit date:") Or _
            messageText.Contains("done date:") = True Or messageText.Contains("stat:") Or _
            messageText.Contains("err:") = True Or messageText.Contains("text:") Then ' for capturing dn
                messageText = messageText.Trim.Replace("id:", " ").Replace(" sub:", " ").Replace(" dlvrd:", " ").Replace(" submit date:", " ").Replace(" done date:", " ")
                messageText = messageText.Trim.Replace(" stat:", " ").Replace(" err:", " ").Replace(" text:", " ")
                messageText = messageText.TrimEnd.TrimStart.Trim

                Arraybody = Split(messageText + " ", " ")

                Dim dn As New SMPPDNStrc
                With dn
                    .MSISDN = str_source_addr
                    .SMPPID = Arraybody(0)
                    .TelcoID = CInt(telcoid)
                    .Status = Arraybody(5)
                    .ReceiveDate = DateTime.Now
                End With

                logger.Warn("[DN]: MSISDN=" & dn.MSISDN & ";" & "Status=" & dn.Status & ";" & "TelcoID=" & dn.TelcoID.ToString & ";" & "SMPPID=" & dn.SMPPID)
                dumpToQueue(dn_queue, dn)
            Else 'for capturing MO sms
                Arraybody = Split(messageText + " ", " ")

                Dim objMOType As New ConMO.SmartMOStr  ' use smart structure for post EGG MO, cause is quite same
                With objMOType

                    .Message = messageText.Replace("¡", "@")
                    .TransactionID = data.Sequence.ToString
                    .ShortCode = data.DestAddr.ToString
                    .MSISDN = data.SourceAddr.ToString
                    .ReceiveDate = DateTime.Now
                    .TelcoID = CInt(telcoid)
                    .RetryCount = 0

                    logger.Info("[MO] Message : " & .Message & " || " & _
                        "Full Message : " & fullMessage & " || " & _
                        "Originating Id : " & .TransactionID & " || " & _
                        "Shortcode : " & .ShortCode & " || " & _
                        "Received Phone : " & .MSISDN & " || " & _
                        "Received Date : " & .ReceiveDate & " || " & _
                        "Datacoding : " & data.DataCoding.ToString & " || " & _
                        "Telco Id : " & .TelcoID.ToString)
                End With


                Using MOsvc As New ConMO.MOClient
                    MOsvc.PostSmartMO(objMOType)

                    MOsvc.Close()
                End Using

            End If
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Sub AddMessageSegmentToCollector(ByVal data As DeliverSm)
        Dim userDataArray As UserData() = Nothing
        If collector.ContainsKey(CStr(CDbl(data.SourceAddr) + data.MessageReferenceNumber)) Then
            userDataArray = DirectCast(collector(CStr(CDbl(data.SourceAddr) + data.MessageReferenceNumber)), UserData())
        Else
            userDataArray = New UserData(data.TotalSegments - 1) {}
        End If

        userDataArray(data.SegmentNumber - 1) = data.UserDataPdu
        collector(CStr(CDbl(data.SourceAddr) + data.MessageReferenceNumber)) = userDataArray
    End Sub

    Private Function RetrieveFullMessage(ByVal data As DeliverSm) As String
        Dim message As String = Nothing
        Dim key As String = CStr(CDbl(data.SourceAddr) + data.MessageReferenceNumber)
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

    Private Function IsLastSegment(ByVal data As DeliverSm) As Boolean
        Dim finished As Boolean = False
        Dim userDataArray As UserData() = Nothing
        Dim key As String = CStr(CDbl(data.SourceAddr) + data.MessageReferenceNumber)

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

    Private Sub dumpToQueue(ByVal destination As String, ByVal obj As Object)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
                q.Close()
            End Using
        End Using
    End Sub

    Private Sub Send2Outbox(ByVal obj As SendSMSAPIStrc, ByVal MTResponse As String, ByVal Status As Integer, ByVal pMTID As String)

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
            .MTID = pMTID
        End With
        dumpToQueue(update_queue, Tbl_Outbox)
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

    Private Sub send2Death(ByVal obj As SendSMSAPIStrc, ByVal MTResponse As String, ByVal Status As Integer, ByVal pMTID As String)
        Dim TempRetryCount As Integer = 0
        TempRetryCount = obj.RetryCount + 1

        If TempRetryCount <= CInt(RetryCount) Then
            obj.RetryCount = TempRetryCount
            dumpToQueue(death_queue, obj)
        Else
            logger.Error("[ERROR]" & getObjInfo(obj) & ";MTID=" & pMTID & ";Status=" & CStr(Status) & ";StatusCode=" & MTResponse)
            Send2Outbox(obj, MTResponse, Status, pMTID)
        End If
    End Sub


End Class