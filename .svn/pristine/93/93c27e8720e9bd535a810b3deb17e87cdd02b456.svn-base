Imports System.Configuration
Imports System.Messaging

Module QueueCheck

    Private queue_str As String = ConfigurationManager.AppSettings("queue_str")
    Private queue_lmt As String = ConfigurationManager.AppSettings("queue_lmt")
    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Sub Main()
        log4net.Config.XmlConfigurator.Configure()

        Try
            Dim arr As Integer = 0
            Dim queues As String() = queue_str.Split("|")
            For Each q_str As String In queues
                Using q As New MessageQueue(q_str)
                    arr = GetMessageCount(q)
                    If arr >= queue_lmt Then
                        MailSender.SendEmail("Queue : <b>" & q_str & "</b>, No. of Message : <b>" & arr & "</b><br />", "", "")
                        SMSSender.deliverpost("Queue " & q_str & " have exceeded the preset limit of " & queue_lmt & ". Total message in queue now is " & arr & ". Please recheck the application/system")
                    End If

                    Console.WriteLine("Queue=" & q_str & ";No. of Message=" & arr)
                    logger.Info("Queue=" & q_str & ";No. of Message=" & arr)
                    q.Close()
                End Using
            Next

            logger.Info("===========================================================================")
        Catch ex As Exception
            logger.Fatal("[FATAL] Message : " & ex.Message)
            logger.Fatal("[FATAL] Source : " & ex.Source)
            logger.Fatal("[FATAL] StackTrace : " & ex.StackTrace)
        End Try
    End Sub

    Private Function PeekWithoutTimeout(ByVal q As MessageQueue, ByVal cursor As Cursor, ByVal action As PeekAction) As Message
        Dim ret As Message = Nothing
        Try
            ret = q.Peek(New TimeSpan(1), cursor, action)
        Catch mqe As MessageQueueException
            If Not mqe.Message.ToLower().Contains("timeout") Then
                Throw
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
