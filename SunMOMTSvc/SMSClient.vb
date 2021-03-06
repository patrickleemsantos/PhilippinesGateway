Imports System.Threading

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Class		: SMSClient
'''
'''-----------------------------------------------------------------------------
''' <summary>To simply thins use this class to easyly send and recieve SMS messages</summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Class SMSClient
    '''-----------------------------------------------------------------------------
    ''' <summary>This event triggers when these debug information. use when looking for errors</summary>
    ''' <param name="msg">debug messages</param>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event onDebug(ByVal msg As String)

    '''-----------------------------------------------------------------------------
    ''' <summary>most SMSC dont allow continually connections, and will disconnect
    ''' clients after 5-10 minuts. this options lets SMSClient handle this for you
    ''' by automatily reconnect if we have been disconnected.
    ''' this value will be automatily set to false if we get an authentication error </summary>
    '''-----------------------------------------------------------------------------
    Public autoReconnect As Boolean = True
    Private priv_username As String = ""
    Private priv_password As String = ""
    Private pre_SMSC_Host As String = ""
    Private priv_SMSC_port As Integer = 0

    '''-----------------------------------------------------------------------------
    ''' <summary>This event fires if we get disconnected from the SMSC</summary>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event onDisconnect()
    '''-----------------------------------------------------------------------------
    ''' <summary>This event fires when we have successfully connected to the SMSC</summary>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event onConnect()
    '''-----------------------------------------------------------------------------
    ''' <summary>This event fires, if SMSC accepts login our login credentials</summary>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event login_success()
    '''-----------------------------------------------------------------------------
    ''' <summary>This event fires if we gets rejected with our credentials</summary>
    ''' <param name="reason"></param>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event login_failed(ByVal reason As String)

    Public Event msg_recv(ByVal packet As UCPPacket)

    ''' <summary>This event fires, if SMSC accepts login our login credentials</summary>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event send_keep_alive()

    '''-----------------------------------------------------------------------------
    ''' <summary>Have we ben authenticatede by the SMSC successfully ?</summary>
    ''' <value></value>
    ''' <remarks>ignore this, if your account/smsc does not need to be authenticated</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public ReadOnly Property isAuthenticated() As Boolean
        Get
            Return authenticated
        End Get
    End Property

    Private WithEvents ClientSocket As UCPSocket
    Private WithEvents ClientListener As UCPListener
    Private authenticated As Boolean = False

    '''-----------------------------------------------------------------------------
    ''' <summary>Are we connected to the SMSC. Dont use externally </summary>
    ''' <value></value>
    ''' <remarks>this property automaticly resumes connection with SMSC if
    ''' you have set autoReconnect to True ( its the default )</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public ReadOnly Property connected() As Boolean
        Get
            If Not ClientSocket.connected And autoReconnect Then
                Reconnect(pre_SMSC_Host, priv_SMSC_port)
                'ClientSocket.disconnect()
                If priv_username <> "" And priv_password <> "" Then
                    login(priv_username, priv_password)
                    RaiseEvent onDebug("disconnected, so reconningting, with login")
                Else
                    RaiseEvent onDebug("disconnected, so reconningting")
                End If
            End If
            Return ClientSocket.connected
        End Get
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>converts and ip and port, to something the smsc understands</summary>
    ''' <param name="Ip">the ip address</param>
    ''' <param name="port">the port</param>
    ''' <returns>an encoded string of ip and port</returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	09-02-2004	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Function encodeIpAndPort(ByVal Ip As String, ByVal port As Integer) As String
        Return encodeIpPort(Ip, port)
    End Function

    '''-----------------------------------------------------------------------------
    ''' <summary>Takes an SMSC formatet ip and port, and converts it to and easy
    ''' readeble ip address and port</summary>
    ''' <param name="IpAndPort">SMSC port and ip</param>
    ''' <param name="ip">extractet ip address</param>
    ''' <param name="port">extractet port</param>
    ''' <remarks>return "" in ip and 0 in port, if input was not valid</remarks>
    ''' <history>
    ''' 	[bofh] 	09-02-2004	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub decodeIpAndPort(ByVal IpAndPort As String, ByRef ip As String, ByRef port As Integer)
        decodeIpPort(IpAndPort, ip, port)
    End Sub


    '''-----------------------------------------------------------------------------
    ''' <summary>Reconnects to SMSC</summary>
    ''' <param name="SMSC_Host"></param>
    ''' <param name="SMSC_port"></param>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub Reconnect(ByVal SMSC_Host As String, ByVal SMSC_port As Integer)
        ' Added by Patrick, to disconnect first before connecting again 2016-09-21
        ClientSocket.disconnect()
        ClientSocket.connect(SMSC_Host, SMSC_port)
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>Send login informations to SMSC</summary>
    ''' <param name="username">your account ID</param>
    ''' <param name="password">your password</param>
    ''' <remarks>if login fails, autoReconnect will be set to False</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub login(ByVal username As String, ByVal password As String)
        If ClientSocket.connected Then
            priv_username = username
            priv_password = password
            If Not connected Then Exit Sub
            Dim login_p As New UCPPacket
            login_p.CreateLoginPacket(username, password)
            ClientSocket.SendData(login_p.createpacket)
        Else
            RaiseEvent login_failed("Not connected")
        End If
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>If you have public IP, you can make the SMSC send receipts and SMS 
    ''' messages to your maskine. Use bind to make this class pickup the messages
    ''' from the smsc, and then you can react on those</summary>
    ''' <param name="LocalIP">sms clients internal IP or fqdn</param>
    ''' <param name="localport">port to listen on</param>
    ''' <remarks>This ip should be the maskines IP. if the IP is on an rfc1918 net
    ''' make sure you pat or nat the extern ip and port to this. then use
    ''' encodeIpAndPort to construct the ip and port smses should be sendt too</remarks>
    ''' <history>
    ''' 	[bofh] 	09-02-2004	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub bind(ByVal LocalIP As String, ByVal localport As Integer)
        If ClientListener Is Nothing Then
            ClientListener = New UCPListener
            AddHandler ClientListener.Packet_recieved, AddressOf Packet_recieved
            AddHandler ClientListener.onDebug, AddressOf listener_onDebug
        End If
        ClientListener.bind(LocalIP, localport)
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>Creates an new instans of the SMS client and connects to the SMSC</summary>
    ''' <param name="SMSC_Host">host or IP of SMSC</param>
    ''' <param name="SMSC_port">port number on SMSC</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Sub New(ByVal SMSC_Host As String, ByVal SMSC_port As Integer)
        pre_SMSC_Host = SMSC_Host
        priv_SMSC_port = SMSC_port
        TransActionNumber = Int(98 * Rnd() + 1)
        ClientSocket = New UCPSocket
        AddHandler ClientSocket.Packet_recieved, AddressOf Packet_recieved
        AddHandler ClientSocket.onDebug, AddressOf socket_onDebug
        AddHandler ClientSocket.onConnect, AddressOf socket_onConnect
        AddHandler ClientSocket.onDisconnect, AddressOf socket_onDisconnect
        ClientSocket.connect(SMSC_Host, SMSC_port)
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>Creates an new instans of the SMS client and connects to the SMSC
    ''' and sends authentication packets ( logs on )</summary>
    ''' <param name="SMSC_Host">host or IP of SMSC</param>
    ''' <param name="SMSC_port">port number on SMSC</param>
    ''' <param name="username">user account ID</param>
    ''' <param name="password">user account password</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Sub New(ByVal SMSC_Host As String, ByVal SMSC_port As Integer, ByVal username As String, ByVal password As String)
        pre_SMSC_Host = SMSC_Host
        priv_SMSC_port = SMSC_port
        TransActionNumber = Int(98 * Rnd() + 1)
        ClientSocket = New UCPSocket
        ClientSocket.connect(SMSC_Host, SMSC_port)
        AddHandler ClientSocket.Packet_recieved, AddressOf Packet_recieved
        AddHandler ClientSocket.onDebug, AddressOf socket_onDebug
        AddHandler ClientSocket.onConnect, AddressOf socket_onConnect
        AddHandler ClientSocket.onDisconnect, AddressOf socket_onDisconnect
        ClientListener = New UCPListener
    End Sub

    Private Sub Packet_recieved(ByVal packet As UCPPacket)


        If packet.Operation = enumOT.Session_management_operation Then
            If packet.success() Then
                authenticated = True
                RaiseEvent login_success()
            Else
                authenticated = False
                autoReconnect = False
                RaiseEvent login_failed("{" & packet.last_errorcode.ToString & "} " & packet.error_message)
                ClientSocket.disconnect()
            End If
        ElseIf Not packet.success And packet.is_Operation_or_Request = enumOR.Request Then
            If packet.last_errorcode = Error_code.Authentication_failure Then
                authenticated = False
                autoReconnect = False
                ClientSocket.disconnect()
            End If
            RaiseEvent onDebug("Packet_recieved: Error: {" & packet.last_errorcode.ToString & "} " & packet.error_message)
        Else
            If packet.is_Operation_or_Request = enumOR.Operation Then
                ' We need to respond to this packets.
                If packet.Message <> "" Then
                    packet.acknowledgement(True, "")
                    ClientSocket.SendData(packet.createpacket)
                    'sendKeepAlive()
                Else
                    '2015/02/04 Patrick: To reply success even blank messages
                    'packet.acknowledgement(False, "Not understood", Error_code.Message_type_not_supported_by_system)
                    packet.acknowledgement(True, "")
                    ClientSocket.SendData(packet.createpacket)
                    'sendKeepAlive()
                End If
            Else
                ' SMSC is just telling us something ( went right )
            End If

            If packet.Message <> "" Then
                RaiseEvent onDebug("Packet_recieved: " & packet.Originator & "->" & packet.receiver & ": " & packet.Message)
                RaiseEvent msg_recv(packet)
            Else
                RaiseEvent onDebug("Packet_recieved: " & packet.createpacket)
            End If
        End If
    End Sub

    Private Sub socket_onDebug(ByVal msg As String)
        RaiseEvent onDebug("UCPSocket." & msg)
    End Sub

    Private Sub listener_onDebug(ByVal msg As String)
        RaiseEvent onDebug("UCPListener." & msg)
    End Sub

    Private Sub socket_onConnect()
        authenticated = False
        RaiseEvent onConnect()
    End Sub

    Private Sub socket_onDisconnect()
        authenticated = False
        RaiseEvent onDisconnect()
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>a VERY simple way of sending sms messages via UCP</summary>
    ''' <param name="from">who is SMS from</param>
    ''' <param name="sto">who is SMS to</param>
    ''' <param name="msg">The alpha numeric messages</param>
    ''' <returns>false if messages could not be sendt</returns>
    ''' <remarks>by this function return true does not mean that the sms actually was sendt.
    ''' you need to use notifications requests  for this</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Function SendSimpleSMS(ByVal from As String, ByVal sto As String, ByVal msg As String) As Boolean
        If Not connected Then : Return False : Exit Function : End If
        Dim sms_p As New UCPPacket
        sms_p.CreateSimpleSMSpacket(from, sto, msg)
        ClientSocket.SendData(sms_p.createpacket)
        Return True
    End Function

    '''-----------------------------------------------------------------------------
    ''' <summary>send a normal and simple sms messages</summary>
    ''' <param name="Public"></param>
    ''' <param name="from">who is SMS from</param>
    ''' <param name="sto">who is SMS to</param>
    ''' <param name="msg">The alpha numeric messages</param>
    ''' <returns>false if messages could not be sendt</returns>
    ''' <remarks>by this function return true does not mean that the sms actually was sendt.
    ''' you need to use notifications requests  for this</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Function SubmitSMS(ByVal from As String, ByVal sto As String, _
                    ByVal msg As String, ByVal charge As String, ByVal page As String, ByVal keywordID As String) As Boolean

        If Not connected Then : Return False : Exit Function : End If
        Dim sms_p As New UCPPacket
        sms_p.Create_Submit_AlphaNum_SMS(from, sto, msg, charge, page, keywordID)

        Return ClientSocket.SendData(sms_p.createpacket)
        'Return True
    End Function

    Public t = ""
    'Added by Patrick 2016-10-13 
    'Keep alive function
    Public Sub StartKeepAlive()
        'If Not connected Then : Return False : Exit Sub : End If
        'Dim sms_p As New UCPPacket
        'Return ClientSocket.SendData(sms_p.createpacket)
        'sendKeepAlive()
        'Do While connected
        '    'If Not connected Then : Exit Sub : End If
        '    Threading.Thread.Sleep(10000)
        '    RaiseEvent send_keep_alive()
        '    Dim p As New UCPPacket("02/00035/O/31/2488/0139/A0")
        '    ClientSocket.SendData(p.createpacket)
        '    'sendKeepAlive()
        '    BackgroundWorker1.RunWorkerAsync()
        'Loop
        t = New Thread(AddressOf Me.BackgroundProcess)
        t.Priority = ThreadPriority.BelowNormal ' This will push the subroutine even further into the background
        t.Start()
    End Sub

    Private Sub BackgroundProcess()
        Do While connected
            If Not connected Then : Exit Sub : End If
            'Threading.Thread.Sleep(60000)
            Threading.Thread.Sleep(30000)
            RaiseEvent send_keep_alive()
            'Dim p As New UCPPacket("02/00035/O/31/2488/0139/A0")
            'ClientSocket.SendData(p.createpacket)z
            Dim sms_p As New UCPPacket
            sms_p.CreateKeepAlivePacket()

            ClientSocket.SendData(sms_p.createpacket)
        Loop
    End Sub

    'Public Sub sendKeepAlive()
    '    If Not connected Then : Exit Sub : End If
    '    Threading.Thread.Sleep(10000)
    '    RaiseEvent send_keep_alive()
    '    Dim p As New UCPPacket("02/00035/O/31/2488/0139/A0")
    '    ClientSocket.SendData(p.createpacket)
    '    StartKeepAlive()
    'End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Public"></param>
    ''' <param name="from">who is SMS from</param>
    ''' <param name="sto">who is SMS to</param>
    ''' <param name="msg">The alpha numeric messages</param>
    ''' <param name="notificationAddr">mobil number, or ipaddress and port to send notifications too</param>
    ''' <param name="notify_over_ip">is the notification adddress and internet addresse. Use false for mobil numbers.</param>
    ''' <returns>false if messages could not be sendt</returns>
    ''' <remarks>by this function return true does not mean that the sms actually was sendt.
    ''' you need to use notifications requests  for this</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    'Public Function SubmitSMS(ByVal from As String, ByVal sto As String, _
    'ByVal msg As String, _
    ' ByVal charge As String, ByVal notificationAddr As String, Optional ByVal notify_over_ip As Boolean = False) As Boolean
    'If Not connected Then : Return False : Exit Function : End If

    ' Dim sms_p As New UCPPacket
    'Dim notifcationtype As Notification_Type
    'notifcationtype.Delivery_Notification = True
    'notifcationtype.Non_delivery_notification = True
    'If notify_over_ip Then
    'sms_p.Create_Submit_AlphaNum_SMS(from, sto, msg, charge, True, notificationAddr, notifcationtype, Notification_PID.PC_appl_over_TCP_IP)
    'Else
    'sms_p.Create_Submit_AlphaNum_SMS(from, sto, msg, charge, True, notificationAddr, notifcationtype, Notification_PID.Mobile_Station)
    'End If

    'ClientSocket.SendData(sms_p.createpacket)
    'Return True
    'End Function

    Public Sub test()
        Dim p As New UCPPacket("70/00019/R/60/A//74")
        Dim s As String = p.createpacket
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>To proberly close down listner, call this function before disposing object</summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub destroy()
        If Not ClientListener Is Nothing Then ClientListener.StopListener = True
    End Sub
End Class
