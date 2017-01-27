Imports System.Messaging
Imports System.Configuration
Imports LibraryDAL

Public Delegate Sub AddToDelegate(ByVal body As LibraryDAL.SendSMSAPIStrc)
Public Class InsertOutbox_bulksend
    Inherits System.ServiceProcess.ServiceBase

    Private del As AddToDelegate

    Private _listen As Boolean
    Private _queue As MessageQueue

    Private insert_queue As String = ConfigurationManager.AppSettings("insert_queue")
    Private bulkqueue As String = ConfigurationManager.AppSettings("bulkqueue")
    Private bulkqueue2 As String = ConfigurationManager.AppSettings("bulkqueue2")

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        log4net.Config.XmlConfigurator.Configure()

        del = New AddToDelegate(AddressOf send2DB)

        StartListen()
    End Sub

    Protected Overrides Sub OnStop()
        StopListen()
    End Sub

    Private Sub StartListen()
        Try
            _queue = New MessageQueue(bulkqueue)
            MessageQueue.EnableConnectionCache = False

            _listen = True
            AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
            _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(LibraryDAL.SendSMSAPIStrc)})

            StartListening()
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try



    End Sub

    Private Sub StopListen()
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
            del.Invoke(CType(msg.Body, LibraryDAL.SendSMSAPIStrc))
            StartListening()
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Sub send2DB(ByVal body As LibraryDAL.SendSMSAPIStrc)
        Dim pTran As New LibraryDAL.Tbl_Outbox
        logger.Info("[Get Data]" & getObjInfo(body))

        Try

            Dim pMSISDNList = body.MSISDN.Split(CChar(","))
            Dim pTransid As String = body.TransactionID

            For Each _List In pMSISDNList
                body.MSISDN = _List.Trim 'Replace the MSISDN from a list to single MSISDN
                body.TransactionID = pTransid & "-" & _List.Trim 'Special case for EGG MT bulksend, the MTID are TransactionID-MSISDN

                send2Insert(body, "200", pTransid, MTStatus._Success)
                logger.Info("[SUCCESS]" & getObjInfo(body))
            Next
            'Dim OutboxCRUD As New LibraryDAL.OutboxCRUD
            'OutboxCRUD.OutboxInsert(pTran)

        Catch ex As Exception
            sendToQueueDeath(bulkqueue2, body)
            logger.Fatal("[FATAL]", ex)
            logger.Fatal("[FATAL]" & getObjInfo(body))
        End Try
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
            .StatusCode = MTResponse
            .TelcoID = obj.TelcoID
            .TransactionID = obj.rrn
            .SendDate = obj.SendDate
            .Status = Status
            .MTID = obj.TransactionID
        End With
        sendToQueue(insert_queue, Tbl_Outbox)
        logger.Info("[Insert Data]" & Tbl_Outbox.ToString)
    End Sub

    Public Sub sendToQueue(ByVal destination As String, ByVal obj As LibraryDAL.Tbl_Outbox)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

    Public Sub sendToQueueDeath(ByVal destination As String, ByVal obj As LibraryDAL.SendSMSAPIStrc)
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
End Class
