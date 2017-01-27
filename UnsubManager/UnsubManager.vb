Imports System.Messaging
Imports System.Configuration
Imports System.Data.SqlClient
Imports LibraryDAL

Public Delegate Sub AddToDelegate(ByVal body As UnsubUserStrc)

Public Class UnsubManager
    Inherits System.ServiceProcess.ServiceBase

    Private del As AddToDelegate

    Private _listen As Boolean
    Private _queue As MessageQueue

    Private death_queue As String = ConfigurationManager.AppSettings("unsub_death")
    Private mo_queue As String = ConfigurationManager.AppSettings("unsub_queue")

    Private conn As String = ConfigurationManager.AppSettings("ConnectionString")

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
        log4net.Config.XmlConfigurator.Configure()

        del = New AddToDelegate(AddressOf ProcessUnsub)

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
            _queue = New MessageQueue(mo_queue)
            MessageQueue.EnableConnectionCache = False

            _listen = True
            AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
            _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(UnsubUserStrc)})

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
            del.Invoke(CType(msg.Body, UnsubUserStrc))
            StartListening()
        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

    Private Sub ProcessUnsub(ByVal body As UnsubUserStrc)
        Dim result As Integer = 0
        Dim sql As String = "UPDATE [PremiumSMS].[dbo].[User] SET [StatusID]=" & UserStatus._Inactive & ", [TransactionID]='" & body.TransactionID & "', " & _
        "[DateTerminate]=CURRENT_TIMESTAMP WHERE [MSISDN]='" & body.MSISDN & "' AND [KeywordID]='" & body.KeywordID & "'"
        Dim intLoginCount As Integer = 0
        Dim ds As New DataSet

        Dim userid As String = ""
        Dim telcoid As String = ""
        Try
            sql = "SELECT * FROM [PremiumSMS].[dbo].[User] WHERE [KeywordID]='" & body.KeywordID & "'"

            ds = dbmgmt(sql, "tbl_user")

            intLoginCount = ds.Tables("tbl_user").Rows.Count()

            If intLoginCount > 0 Then
                userid = Convert.ToString(ds.Tables("tbl_user").Rows(0)("UserID"))
                telcoid = Convert.ToString(ds.Tables("tbl_user").Rows(0)("TelcoID"))
            End If
            ds.Dispose()
 

            result = 0
            sql = "UPDATE [PremiumSMS].[dbo].[User] SET [StatusID]=" & UserStatus._Inactive & ", [TransactionID]='" & body.TransactionID & "', " & _
            "[DateTerminate]=CURRENT_TIMESTAMP WHERE [MSISDN]='" & body.MSISDN & "' AND [KeywordID]='" & body.KeywordID & "'"

            result = sendToDB(sql)

            logger.Info("[Process Unsub User]: TelcoID=" & telcoid & "; UserID=" & userid & "; Result=" & result & "; SQL= " & sql)
     
            Try
                result = 0
                sql = "INSERT INTO [PremiumSMS].[dbo].[userhistory] VALUES(" & userid & ", " & body.KeywordID & _
                ", " & telcoid & ", 'Unsubcribed: SMART 951', CURRENT_TIMESTAMP)"

                result = sendToDB(sql)

                logger.Info("[Insert User History]: TelcoID=" & telcoid & "; UserID=" & userid & "; Result=" & result & "; SQL= " & sql)
            Catch ex As Exception
                logger.Fatal("[FATAL]", ex)
                logger.Info("[Error Query]: TelcoID=" & telcoid & "; UserID=" & userid & "; Error Result=" & result & "; SQL= " & sql)
            End Try

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
            logger.Info("[Error Query]: TelcoID=" & telcoid & "; UserID=" & userid & "; Error Result=" & result & "; SQL= " & sql)
            SendQueue(death_queue, body)
        End Try
    End Sub


    Private Function sendToDB(ByVal Sql As String) As Integer
        Dim result As Integer = 0
        Using magentConn As New SqlConnection(conn)
            magentConn.Open()
            Using objCommand As New SqlCommand(Sql, magentConn)
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

    Private Sub SendQueue(ByVal destination As String, ByVal obj As UnsubUserStrc)
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
