Imports System.Threading
Imports System.Configuration

Public Class EGGMT_2910
    Inherits System.ServiceProcess.ServiceBase

    Private timer As System.Timers.Timer

    Public Sub New()
        CanPauseAndContinue = True
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnStart(ByVal args() As String)

        log4net.Config.XmlConfigurator.Configure()
        Dim _listener(CInt(ConfigurationManager.AppSettings("maxthread"))) As MSMQListener

        Dim threads(CInt(ConfigurationManager.AppSettings("maxthread"))) As Thread
        Dim i As Integer

        For i = 0 To CInt(ConfigurationManager.AppSettings("maxthread"))
            _listener(i) = New MSMQListener(ConfigurationManager.AppSettings("gateway_queue"))
            Dim st As New ThreadStart(AddressOf _listener(i).Start)

            threads(i) = New Thread(st)
        Next

        For Each t As Thread In threads
            t.Start()
        Next
    End Sub

    Protected Overrides Sub OnStop()
        Me.Dispose()
        Me.Finalize()
        GC.Collect()
    End Sub

End Class
