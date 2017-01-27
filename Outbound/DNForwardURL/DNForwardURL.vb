Imports System.Messaging
Imports System.Configuration
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security
Imports System.Net
Imports System.Web
Imports System.IO


Public Delegate Sub AddToDelegate(ByVal body As LibraryDAL.DNForwardStrc)
Public Class DNForwardURL
    Inherits System.ServiceProcess.ServiceBase

    Private del As AddToDelegate

    Private _listen As Boolean
    Private _queue As MessageQueue

    Private ForwardURL_Queue As String = ConfigurationManager.AppSettings("ForwardURL_Queue")
    Private ForwardURL_Queue2 As String = ConfigurationManager.AppSettings("ForwardURL_Queue2")
    Private RetryCount As Integer = ConfigurationManager.AppSettings("RetryCount")
    Private PostURLTimeOut As Integer = ConfigurationManager.AppSettings("PostUrlTimeOut")

    Public Delegate Function ServerCertificateValidationCallback(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Overrides Sub OnStart(ByVal args() As String)
        log4net.Config.XmlConfigurator.Configure()

        del = New AddToDelegate(AddressOf PostToCP)

        StartListen()
    End Sub

    Protected Overrides Sub OnStop()
        StopListen()
    End Sub

    Private Sub StartListen()
        _queue = New MessageQueue(ForwardURL_Queue)
        MessageQueue.EnableConnectionCache = False

        _listen = True
        AddHandler _queue.PeekCompleted, AddressOf OnPeekCompleted
        _queue.Formatter = New XmlMessageFormatter(New System.Type() {GetType(LibraryDAL.DNForwardStrc)})

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
        del.Invoke(CType(msg.Body, LibraryDAL.DNForwardStrc))
        StartListening()
    End Sub

    Private Sub PostToCP(ByVal body As LibraryDAL.DNForwardStrc)
        Dim ds As New DataSet
        Try
            Dim sendString As String = ""
            Dim strResult As String = ""

            'post to cp
            strResult = WebRequest(body.URL, body.URLData)

            ' if fail retry the count 
            If strResult = "Fail" And body.RetryCount < RetryCount Then
                body.RetryCount = body.RetryCount + 1
                sendToQueue(body, ForwardURL_Queue2)
                Exit Sub
            End If

            'log the post
            Dim strUrlResponse As String = body.URL & "?" & body.URLData & ";RetryCount=" & body.RetryCount.ToString & ";" & strResult
            logger.Info(strUrlResponse)

        Catch ex As Exception
            sendToQueue(body, ForwardURL_Queue2)
            logger.Fatal("[FATAL]", ex)
        End Try
    End Sub


    Public Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
        Try
            If Left(URL.ToLower, 5) = "https" Then
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls
                ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate)
            End If

            Dim encoding As New System.Text.ASCIIEncoding
            Dim Data() As Byte = encoding.GetBytes(PostData)
            Dim LoginReq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(URL), Net.HttpWebRequest)
            With LoginReq
                .KeepAlive = False
                .Method = "POST"
                .ContentType = "application/x-www-form-urlencoded"
                .ContentLength = Data.Length
                .Timeout = CInt(PostURLTimeOut) * 1000
            End With

            Dim SendReq As Stream = LoginReq.GetRequestStream
            SendReq.Write(Data, 0, Data.Length)
            SendReq.Close()

            Dim LoginRes As System.Net.HttpWebResponse = CType(LoginReq.GetResponse(), Net.HttpWebResponse)
            Dim HTML As String = ""
            Using sReader As StreamReader = New StreamReader(LoginRes.GetResponseStream)
                HTML = sReader.ReadToEnd
            End Using

            HTML = HTML.Trim & " (" & LoginRes.StatusCode.ToString & ")"

            LoginRes.Close()
            LoginRes = Nothing

            Return HTML.Trim
        Catch ex As Exception
            logger.Fatal("URL=" & URL & "?" & PostData)
            logger.Fatal("[FATAL]", ex)
            Return "Fail"
        End Try
    End Function

    Private Sub sendToQueue(ByVal objQueueType As Object, ByVal str_q As String)
        Using q As New MessageQueue(str_q, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(objQueueType, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

    Public Function ValidateServerCertificate(ByVal sender As Object, ByVal cert As X509Certificate, ByVal chain As X509Chain, _
    ByVal ssl As SslPolicyErrors) As Boolean
        Return True
    End Function

End Class
