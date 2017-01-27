Imports System.Messaging
Imports System.Configuration
Imports System.Data.SqlClient
Imports LibraryDAL

Public Delegate Sub AddToDelegate(ByVal body As LibraryDAL.Tbl_MOStr)

Public Class SunUpdateOutbox
    Inherits System.ServiceProcess.ServiceBase

    Private del As AddToDelegate

    Private _listen As Boolean
    Private _queue As MessageQueue

    'Private death_queue As String = ConfigurationManager.AppSettings("unsub_queue")
    Private update_queue As String = ConfigurationManager.AppSettings("update_queue")
    Private death_queue As String = ConfigurationManager.AppSettings("death_queue")

    Private conn As String = ConfigurationManager.AppSettings("ConnectionString")

    Private MT_URL As String = ConfigurationManager.AppSettings("MT_URL")

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
        log4net.Config.XmlConfigurator.Configure()

        del = New AddToDelegate(AddressOf ProcessUpdate) 'ProcessUpdateOutbox

        StartListen()

        'StartConnecting()
        'StartSMPPConnect()
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        StopListen()
    End Sub



    Private Sub StartListen()
        Try
            _queue = New MessageQueue(update_queue)
            MessageQueue.EnableConnectionCache = False

            _listen = True
            AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
            _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(LibraryDAL.Tbl_MOStr)})

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
            del.Invoke(CType(msg.Body, LibraryDAL.Tbl_MOStr))
            StartListening()
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Function update2SunOutbox(ByVal objMessage As LibraryDAL.Tbl_MOStr) As Boolean
        Dim strWebSendRes As String = ""

        Try
            Dim time As DateTime = DateTime.Now
            Dim format As String = "yyyyMM"
            Dim tname As String = "premium_sms_cp.mt_sun_" & time.ToString(format)

            'Dim sQuery As String = "insert into " & tname & " values (0,'" & objMessage.TelcoID & "','" & _
            '   objMessage.ShortCode & "','" & objMessage.CPID & "','" & objMessage.KeywordID & "','" & objMessage.MSISDN & "','" & objMessage.Charge & "','" & _
            '   objMessage.Message.Replace("'", "") & "','1','" & objMessage.ContentURL & "','" & _
            '    "0','" & objMessage.RetryCount & "','" & objMessage.TransactionID & "','" & objMessage.MTID & "','" & objMessage.Status & "','" & objMessage.StatusCode & "','" & objMessage.StatusCode & "',now(),'','','0000-00-00 00:00:00', '');"

            Dim sQuery As String = "update " & tname & " set dn_code = 'SUCCESS', dn_desc='SUCCESS' " & _
                                " WHERE id in (SELECT id " & _
                                " FROM " & tname & _
                                " WHERE msisdn='" & objMessage.MSISDN & "' and dn_code=''" & _
                                " and telco_id = 2 and (timestamp BETWEEN DATEADD(d, -1, GETDATE()) AND GETDATE()) ORDER BY id DESC limit 1) and (timestamp BETWEEN DATEADD(d, -1, GETDATE()) AND GETDATE())"

            sQuery = "update " & tname & " set dn_code = 'SUCCESS', dn_desc='SUCCESS' " & _
                                " WHERE msisdn='" & objMessage.MSISDN & "' and dn_code=''" & _
                                " ORDER BY id DESC limit 1"



            'logger.FATAL("Entering " & sQuery)
            'Dim result As Integer = ExecuteCommand(sQuery)
            Dim result As String = ""
            strWebSendRes = MT_URL & "?telco=" & System.Web.HttpUtility.UrlEncode("SUN UPDATE") & "&query=" & System.Web.HttpUtility.UrlEncode(sQuery)

            logger.Info("URL: " & strWebSendRes)
            Using web As New System.Net.WebClient
                result = web.DownloadString(strWebSendRes)
                'Dim res As Byte() = web.UploadValues(MT_URL, "POST", MKCollection)

                'result = System.Text.Encoding.ASCII.GetString(res)
                Try
                    web.Dispose()

                Catch ex As Exception
                    'logger.FATAL("Error Closing WebClient")
                End Try
            End Using

            logger.Info(result & "|" & sQuery)
            logger.Info(result & "|" & strWebSendRes)

            If result = "" Then
                'sendToQueue(insert_queue2, objMessage)
                SendQueue(death_queue, objMessage)
                Return False
            End If

            If result = "0" Then
                'sendToQueue(insert_queue2, objMessage)
                Return False
            End If

            If result = "-1" Then
                SendQueue(death_queue, objMessage)
                Return False
            End If

            Return True
        Catch ex As Exception
            'sendToQueue(insert_queue2, objMessage)
            SendQueue(death_queue, objMessage)

            logger.Info("[FATAL]" & strWebSendRes)
            logger.Fatal("[FATAL]", ex)

        End Try
        Return False
    End Function

    Private Sub ProcessUpdate(ByVal body As LibraryDAL.Tbl_MOStr)
        Dim updBol As Boolean = update2SunOutbox(body)
        If updBol = False Then
            ProcessUpdateOutbox(body)
        End If
    End Sub

    Private Sub ProcessUpdateOutbox(ByVal body As LibraryDAL.Tbl_MOStr)
        Dim result As Integer = 0
        Dim sql As String = "update [PremiumSMS].[dbo].[Outbox] set statuscode = 'SUCCESS', dnstatuscode='SUCCESS', dnstatus='200' " & _
                                "WHERE OutboxID in (SELECT TOP (1) OutboxID " & _
                                "FROM [PremiumSMS].[dbo].[Outbox] WITH(NOLOCK) " & _
                                "WHERE MSISDN='" & body.MSISDN & "' and statuscode=''" & _
                                " and [TelcoID] = 2 and ([SendDate] BETWEEN DATEADD(d, -1, GETDATE()) AND GETDATE()) ORDER BY OutboxID DESC) and ([SendDate] BETWEEN DATEADD(d, -1, GETDATE()) AND GETDATE())"
        Dim intLoginCount As Integer = 0
        Dim ds As New DataSet

        Dim userid As String = ""
        Dim telcoid As String = ""
        Try
            result = sendToDB(sql)
            logger.Info("[Update Outbox]: TelcoID=" & telcoid & "; UserID=" & body.MSISDN & "; Result=" & result & "; SQL= " & sql)

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
            logger.Info("[Error Query]: TelcoID=" & telcoid & "; UserID=" & body.MSISDN & "; Error Result=" & result & "; SQL= " & sql)
            SendQueue(death_queue, body)
        End Try
    End Sub


    Private Function sendToDB(ByVal Sql As String) As Integer
        Dim result As Integer = 0
        Using magentConn As New SqlConnection(conn)
            magentConn.Open()
            Using objCommand As New SqlCommand(Sql, magentConn)
                objCommand.CommandTimeout = 5 * 60 * 1000
                result = objCommand.ExecuteNonQuery()
                objCommand.Connection.Close()
            End Using
            magentConn.Close()
        End Using

        Return result
    End Function

    Private Function dbmgmt(ByVal sql_str As String, ByVal tbl_db As String) As DataSet
        Dim dsResult As New DataSet
        Using magentConn As New SqlConnection(conn)
            magentConn.Open()
            Using Adapter As New SqlDataAdapter
                Adapter.SelectCommand = New SqlCommand(sql_str, magentConn)
                Adapter.SelectCommand.CommandTimeout = 3800
                Adapter.Fill(dsResult, tbl_db)
                Adapter.SelectCommand.Connection.Close()
            End Using
            magentConn.Close()
        End Using
        Return dsResult
    End Function

    Private Sub SendQueue(ByVal destination As String, ByVal obj As LibraryDAL.Tbl_MOStr)
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