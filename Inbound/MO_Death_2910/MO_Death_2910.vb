Imports System.Timers
Imports System.Configuration
Imports System.Messaging

Public Class MO_Death_2910
    Inherits System.ServiceProcess.ServiceBase

    Private atimer As Timer

    Private elapsed_time As String = ConfigurationManager.AppSettings("elapsed_time")
    Private EGGMO_2910_queue As String = ConfigurationManager.AppSettings("EGGMO_2910_queue")

    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        log4net.Config.XmlConfigurator.Configure()

        atimer = New System.Timers.Timer()
        AddHandler atimer.Elapsed, AddressOf SmartMORetry
        atimer.Interval = CDbl(elapsed_time) * 1000
        atimer.Enabled = True
        atimer.Start()
    End Sub

    Protected Overrides Sub OnStop()
        atimer.Stop()
        components.Dispose()
        Me.Dispose()
        Me.Finalize()
        GC.Collect()
    End Sub


#Region "Smart MO Retry Function"
    Public Sub SmartMORetry(ByVal source As Object, ByVal e As Timers.ElapsedEventArgs)
        Dim msgExists As Boolean = True
        Dim objMessage As New ConMO.SmartMOStr
        Dim bol As Boolean = False

        Do While msgExists = True
            objMessage = ReceiveEggMOQueue(EGGMO_2910_queue)

            Try
                If objMessage.TransactionID = String.Empty Then
                    msgExists = False
                Else
                    'atimer.Stop()
                    Using moserv As New ConMO.MOClient
                        moserv.PostSmartMO(objMessage)
                        moserv.Close()
                    End Using
                End If
            Catch ex As Exception
                ' atimer.Start()
                msgExists = False
                dumpToSmartQueue(objMessage, EGGMO_2910_queue)
                logger.Fatal("[FATAL]", ex)
            End Try
        Loop
    End Sub

    Private Function ReceiveEggMOQueue(ByVal queuestr As String) As ConMO.SmartMOStr
        Dim objSmsType As New ConMO.SmartMOStr
        Dim myMessage As New Message

        Try
            Using q As New MessageQueue(queuestr, False)
                MessageQueue.EnableConnectionCache = False
                Using qtrans As New MessageQueueTransaction

                    q.Formatter = New XmlMessageFormatter(New Type() {GetType(ConMO.SmartMOStr)})
                    qtrans.Begin()
                    myMessage = q.Receive(New TimeSpan(0, 0, 1), qtrans)
                    If myMessage Is Nothing Then
                        Return Nothing
                    Else
                        objSmsType = CType(myMessage.Body, ConMO.SmartMOStr)
                        qtrans.Commit()
                        Return objSmsType
                    End If
                End Using
            End Using

            'Catch ex As MessageQueueException
            '    If ex.MessageQueueErrorCode = MessageQueueErrorCode.TransactionUsage Then
            '        ' Queue is not transactional.
            '    Else
            '        ' Handle no message arriving in the queue.
            '        If ex.MessageQueueErrorCode = MessageQueueErrorCode.IOTimeout Then
            '            'queue timeout error
            '        End If
            '    End If
            '    Return Nothing
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub dumpToSmartQueue(ByVal obj As ConMO.SmartMOStr, ByVal str_que As String)
        Using q As New MessageQueue(str_que, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

#End Region


End Class
