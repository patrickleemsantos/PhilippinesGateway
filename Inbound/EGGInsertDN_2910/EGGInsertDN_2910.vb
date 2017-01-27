Imports System.Messaging
Imports System.Configuration
Imports LibraryDAL

Public Delegate Sub AddToDelegate(ByVal body As LibraryDAL.EGGDNStrc)
Public Class EGGInsertDN_2910
    Inherits System.ServiceProcess.ServiceBase

    Private del As AddToDelegate

    Private _listen As Boolean
    Private _queue As MessageQueue

    Private insert_queue As String = ConfigurationManager.AppSettings("insert_queue")
    Private insert_queue2 As String = ConfigurationManager.AppSettings("insertdeath_queue")

    Private forward_queue As String = ConfigurationManager.AppSettings("forwarddn_queue")

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
        _queue = New MessageQueue(insert_queue)
        MessageQueue.EnableConnectionCache = False

        _listen = True
        AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
        _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(LibraryDAL.EGGDNStrc)})

        StartListening()
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
        _queue.EndPeek(e.AsyncResult)
        Dim trans As New MessageQueueTransaction()
        Dim msg As Message = Nothing
        trans.Begin()
        msg = _queue.Receive(trans)
        trans.Commit()
        del.Invoke(CType(msg.Body, LibraryDAL.EGGDNStrc))
        StartListening()
    End Sub

    Private Sub send2DB(ByVal body As LibraryDAL.EGGDNStrc)
        Try

            Dim EGGDNCRUD As New LibraryDAL.EGGDNCRUD
            EGGDNCRUD.EGGDNInsert(body)

            Dim DTO As New ForwardDNType

            With DTO
                .DnDate = body.ReceiveDate
                .msgMTid = body.TransID
                .status_code = body.Status
                .TelcoID = TelcoID._Globe
            End With

            sendToQueue(forward_queue, DTO)

        Catch ex As Exception
            sendToQueue(insert_queue2, body)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub

    Public Sub sendToQueue(ByVal destination As String, ByVal obj As Object)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub


End Class
