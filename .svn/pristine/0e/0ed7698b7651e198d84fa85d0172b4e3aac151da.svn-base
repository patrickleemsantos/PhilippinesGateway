Imports System.Messaging
Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web
Imports LibraryDAL
Public Delegate Sub AddToDelegate(ByVal body As ForwardDNType)
Public Class DNForwardMatch


    Private del As AddToDelegate

    Private _listen As Boolean
    Private _queue As MessageQueue

    Private match_queue As String = ConfigurationManager.AppSettings("Match_Queue")
    Private match_queue2 As String = ConfigurationManager.AppSettings("Match_Queue2")
    Private ForwardURL_Queue As String = ConfigurationManager.AppSettings("ForwardDNUrl_Queue")
    Private UpdateOutboxDNQueue As String = ConfigurationManager.AppSettings("UpdateOutboxDNQueue")
    Private conn As String = ConfigurationManager.AppSettings("conn")

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)


    Protected Overrides Sub OnStart(ByVal args() As String)
        log4net.Config.XmlConfigurator.Configure()
        del = New AddToDelegate(AddressOf DNForwardMatching)
        StartListen()
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
    End Sub

    Private Sub StartListen()
        Try
            _queue = New MessageQueue(match_queue)
            MessageQueue.EnableConnectionCache = False

            _listen = True
            AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
            _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(ForwardDNType)})

            StartListening()
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub
    Private Sub OnPeekCompleted(ByVal sender As Object, ByVal e As PeekCompletedEventArgs)
        Try
            _queue.EndPeek(e.AsyncResult)
            Dim trans As New MessageQueueTransaction()
            Dim msg As Message = Nothing
            trans.Begin()
            msg = _queue.Receive(trans)
            trans.Commit()
            del.Invoke(CType(msg.Body, ForwardDNType))
            StartListening()

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Sub StartListening()
        If Not _listen Then
            Exit Sub
        End If
        _queue.BeginPeek()
    End Sub



    Private Sub DNForwardMatching(ByVal body As ForwardDNType)

        Try
            Dim sql_insert As String = ""
            Dim sendString As String = ""
            Dim strResult As String = ""
            Dim ResponseStatusCode As String = ""
            Dim URL As String = ""
            Dim ds As DataSet
            Dim KeywordID As Integer = 0
            Dim msgGUID As String = ""

            Dim DAL_outbox As New OutboxCRUD()

            logger.Info("[INFO] MsgId=" & body.msgMTid & ":Status=" & body.status_code & ":TelcoId=" & body.TelcoID)
            'check data in outbox
            ds = DAL_outbox.CheckOutboxDN(body.msgMTid)
            logger.Info("[INFO] Get Outbox Details")
            If ds.Tables(0).Rows.Count > 0 Then

                For Each dr As DataRow In ds.Tables(0).Rows
                    msgGUID = dr.Item("MsgGUID").ToString
                    KeywordID = dr.Item("KeywordID").ToString
                Next

                '11/04/2013,A, Added another function to pass DN to queue to update outbox (EGG only)
                If body.TelcoID = TelcoID._Globe Then
                    sendToQueue(body, UpdateOutboxDNQueue)
                End If

            Else
                Exit Sub
            End If

            'get url from keyword table
            Dim DAL_keyword As New KeywordCRUD
            URL = DAL_keyword.GetDNURL_byKeyid(KeywordID)
            logger.Info("[INFO] Get DN URL" & URL)
            'get status code from status code table
            If URL <> "" Then
                ResponseStatusCode = GetStatusCode(body.TelcoID, body.status_code)
            Else
                Exit Sub
            End If

            'send to forward url queue
            Dim DNForwardURL As New DNForwardStrc
            With DNForwardURL
                .URL = URL
                .RetryCount = 0
                .URLData = "guid=" & msgGUID & "&keyid=" & KeywordID & "&status=" & ResponseStatusCode
            End With

            sendToForwardDnUrlQueue(DNForwardURL, ForwardURL_Queue)
            logger.Info("[INFO] Forward Queue=" & ForwardURL_Queue & ";URL=" & DNForwardURL.URL & ";URLData=" & DNForwardURL.URLData & ";RetryCount=" & DNForwardURL.RetryCount)
        Catch ex As Exception
            sendToQueue(body, match_queue)
            logger.Fatal("[Fatal] MsgId=" & body.msgMTid & ":Status=" & body.status_code & ":TelcoId=" & body.TelcoID)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub

    Private Sub sendToForwardDnUrlQueue(ByVal objQueueType As DNForwardStrc, ByVal str_q As String)
        Using q As New MessageQueue(str_q, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(objQueueType, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub


    Private Function GetStatusCode(ByVal pTelcoID As Integer, ByVal pStatusCode As String) As String
        'Dim ds As New DataSet
        'Dim rpsStatus As String = ""

        Dim DAL_responseCode As New ResponseCodeCRUD()
        Dim DTO As ResponseCode = DAL_responseCode.Single(pTelcoID, pStatusCode)
        Dim Rev = DTO.StatusCode

        If String.IsNullOrEmpty(Rev) Then
            Rev = 201 'failed status
        End If

        Return DTO.StatusCode

    End Function

    Private Sub sendToQueue(ByVal objQueueType As ForwardDNType, ByVal str_q As String)
        Using q As New MessageQueue(str_q, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(objQueueType, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub
End Class
