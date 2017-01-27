Imports LibraryDAL
Imports System.Messaging
Imports System.Configuration
Imports System.Data.SqlClient

Public Delegate Sub AddToDelegate(ByVal body As ForwardDNType)
Public Class UpdateOutboxDN
    Inherits System.ServiceProcess.ServiceBase

    Private del As AddToDelegate
    'Private Shared _qm As New QMgr

    Private _listen As Boolean
    Private _queue As MessageQueue

    Private update_queue As String = ConfigurationManager.AppSettings("EGG_DNUpdate_Queue")
    Private update_queue2 As String = ConfigurationManager.AppSettings("EGG_DNUpdate_Queue2")

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        log4net.Config.XmlConfigurator.Configure()

        Try
            del = New AddToDelegate(AddressOf UpdateDB)
            StartListen()
        Catch ex As Exception
            logger.Fatal("FATAL", ex)
        End Try

    End Sub

    Protected Overrides Sub OnStop()
        StopListen()
    End Sub

    Private Sub StartListen()
        Try
            _queue = New MessageQueue(update_queue)
            MessageQueue.EnableConnectionCache = False

            _listen = True
            AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
            _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(ForwardDNType)})
            StartListening()
        Catch ex As Exception
            logger.Fatal("FATAL", ex)
        End Try

    End Sub

    Private Sub StopListen()
        _listen = False
        RemoveHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
    End Sub

    Private Sub StartListening()
        Try
            If Not _listen Then
                Exit Sub
            End If
            _queue.BeginPeek()
        Catch ex As Exception
            logger.Fatal("FATAL", ex)
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
            logger.Fatal("FATAL", ex)
        End Try

    End Sub

    Private Sub UpdateDB(ByVal body As ForwardDNType)
        Try
            UpdateOutbox(body)
        Catch ex As Exception
            logger.Fatal("ERROR", ex)
            sendToQueue(update_queue2, body)
        End Try
    End Sub

    Private Function ConvertStrToDateTime(ByVal pDateTime As String) As DateTime
        Try
            Return DateTime.Parse(pDateTime)
        Catch ex As Exception
            Return Date.Now
        End Try
    End Function

    Private Sub UpdateOutbox(ByVal body As ForwardDNType)

        Try
            Dim Sql As String = ""
            Dim tmpDNStatus As String = ""
            Sql = "UPDATE outbox SET StatusCode=@dnStatusCode,DNStatusCode=@dnStatusCode,DNStatus=@dnStatus where MTID =@msgMTid and TelcoID=@telcoid " & _
            "AND sendDate >='" & Date.Now.AddDays(-3).ToString("yyyy-MM-dd") & "'"

            Dim pStatusCode As String = ""
            Select Case body.status_code
                Case "200"
                    pStatusCode = "SUCCESS"
                Case "1"
                    pStatusCode = "SUCCESS"
                Case Else
                    pStatusCode = "FAIL"
            End Select

            logger.Info("[INFO] query=" & Sql & ";dnstatuscode=" & body.status_code & "-" & pStatusCode & ";MTID=" & body.msgMTid & ";telcoID=" & body.TelcoID)
            Dim par(3) As SqlParameter

            par(0) = New SqlParameter("@dnStatusCode", SqlDbType.VarChar)
            par(0).Value = pStatusCode

            par(1) = New SqlParameter("@msgMTid", SqlDbType.VarChar)
            par(1).Value = body.msgMTid

            par(2) = New SqlParameter("@telcoid", SqlDbType.Int)
            par(2).Value = body.TelcoID

            par(3) = New SqlParameter("@dnStatus", SqlDbType.VarChar)
            par(3).Value = body.status_code
            Try
                Using conn As New SqlConnection(SQLHelper.GetConnectionString())
                    SQLHelper.ExecuteNonQuery(conn, CommandType.Text, Sql, par)
                End Using
            Catch ex As Exception
                Throw ex
            End Try

        Catch ex As Exception
            Throw ex
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
