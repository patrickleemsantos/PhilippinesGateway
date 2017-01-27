Imports System.Configuration
Imports System.Messaging

Module QueueCheck

    Private queue_str As String = ConfigurationManager.AppSettings("queue_str")
    Private queue_lmt As String = ConfigurationManager.AppSettings("queue_lmt")
    Private ServerName As String = ConfigurationManager.AppSettings("ServerName")
    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Sub Main()
        log4net.Config.XmlConfigurator.Configure()

        Try
            Dim arr As Integer = 0
            Dim queues As String() = queue_str.Split("|")
            Dim QueueLimits As String() = queue_lmt.Split("|")
            Dim QueueName As String
            Dim QueueLimit As Integer
            Dim EmailResult As String = ""
            Dim SMSResult As String = ""

            For i As Integer = 0 To queues.Count - 1
                QueueName = queues(i)
                QueueLimit = QueueLimits(i)

                Using q As New MessageQueue(QueueName)
                    arr = GetMessageCount(q)
                    If arr >= QueueLimit Then
                        EmailResult = MailSender.SendEmail("Queue : <b>" & QueueName & "</b>, No. of Message : <b>" & arr & "</b><br />", "", "")
                        SMSResult = SMSSender.deliverpost("Philippine " & ServerName & " ;Queue:'" & QueueName & "' exceeded preset limit" & QueueLimit & ". No. msg in queue now is " & arr & ". Please Check System !")
                        logger.Info("Queue=" & QueueName & ";No. of Message=" & arr & ";Email=" & EmailResult & ";SMS=" & SMSResult)
                    End If

                    logger.Info("Checked:" & QueueName)
                    Console.WriteLine("Queue=" & QueueName & ";No. of Message=" & arr)
                    q.Close()
                End Using
            Next

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub

    Private Function PeekWithoutTimeout(ByVal q As MessageQueue, ByVal cursor As Cursor, ByVal action As PeekAction) As Message
        Dim ret As Message = Nothing
        Try
            ret = q.Peek(New TimeSpan(1), cursor, action)
        Catch ex As MessageQueueException
            If Not ex.Message.ToLower().Contains("timeout") Then
                Throw ex
            End If
        End Try
        Return ret
    End Function

    Private Function GetMessageCount(ByVal q As MessageQueue) As Integer
        Dim count As Integer = 0
        Dim cursor As Cursor = q.CreateCursor()

        Dim m As Message = PeekWithoutTimeout(q, cursor, PeekAction.Current)
        If m IsNot Nothing Then
            count = 1
            While (InlineAssignHelper(m, PeekWithoutTimeout(q, cursor, PeekAction.[Next]))) IsNot Nothing
                count += 1
            End While
        End If
        Return count
    End Function

    Public Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function
End Module
