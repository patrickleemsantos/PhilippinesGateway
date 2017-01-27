Imports System.Net.Sockets
Imports System.Threading

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Class		: UCPListener
'''
'''-----------------------------------------------------------------------------
''' <summary>And simple listner to be used for picking up sms messages and delivery repports from SMSC</summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Class UCPListener
    '''-----------------------------------------------------------------------------
    ''' <summary>Event that fires when the socket has debug information</summary>
    ''' <param name="msg">debug messages</param>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event onDebug(ByVal msg As String)
    '''-----------------------------------------------------------------------------
    ''' <summary>Fires when we have recieved an packet from SMSC</summary>
    ''' <param name="packet">An parsed packet</param>
    ''' <remarks></remarks>
    '''-----------------------------------------------------------------------------
    Public Event Packet_recieved(ByVal packet As UCPPacket)
    '''-----------------------------------------------------------------------------
    ''' <summary>Set to True, to make listner close down</summary>
    '''-----------------------------------------------------------------------------
    Public StopListener As Boolean = False

    Private ListnerThread As Thread
    Private sck As Socket

    '''-----------------------------------------------------------------------------
    ''' <summary>Binds to IP address and port</summary>
    ''' <param name="hostname">ip adddresse to listen on</param>
    ''' <param name="port">port to listen on, typpacly 5000 or 6000</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub bind(ByVal hostname As String, ByVal port As Integer)
        Try
            sck = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            Dim EP As New Net.IPEndPoint(Net.Dns.Resolve(hostname).AddressList(0), port)
            sck.Bind(EP)
            sck.Listen(10)

            RaiseEvent onDebug("bind: Starting listner thread")
            Dim ListnerThreadStart As ThreadStart
            ListnerThreadStart = New ThreadStart(AddressOf WaitForConnection)
            ListnerThread = New Thread(ListnerThreadStart)
            ListnerThread.Name = "WebServer_Listner"
            ListnerThread.Start()
        Catch ex As Exception
            sck = Nothing
            RaiseEvent onDebug("bind: " & ex.Message)
        End Try
    End Sub

    Private Sub WaitForConnection()
        While Not StopListener
            If sck.Poll(200, SelectMode.SelectRead) Then
                Dim mySck As Socket = sck.Accept
                Dim endReq As Boolean = False
                Dim iEmptyLoopCounter As Integer = 0
                Dim sckDataBuffer As String
                While Not endReq
                    If Not mySck.Connected Then endReq = True
                    If mySck.Poll(200, SelectMode.SelectError) Then endReq = True
                    If mySck.Available > 0 Then

                        Dim Buffer(mySck.Available) As Byte
                        Dim bytes As Integer = mySck.Receive(Buffer, mySck.Available, 0)
                        sckDataBuffer = sckDataBuffer & System.Text.Encoding.ASCII.GetString(Buffer, 0, bytes)
                    End If
                    iEmptyLoopCounter += 1
                    If (iEmptyLoopCounter Mod 10) = 9 Then _
                        System.Threading.Thread.CurrentThread().Sleep(100)
                    If iEmptyLoopCounter > 200 Then
                        endReq = True
                    End If

                    Dim tmpData As String
                    While InStr(sckDataBuffer, UCP_ETX)
                        tmpData = Mid(sckDataBuffer, 1, InStr(sckDataBuffer, UCP_ETX) - 1)
                        sckDataBuffer = Mid(sckDataBuffer, InStr(sckDataBuffer, UCP_ETX) + 1)
                        RaiseEvent onDebug("OnDataReceived: " & tmpData)
                        Try
                            Dim tmp_p As New UCPPacket(tmpData)
                            RaiseEvent Packet_recieved(tmp_p)
                        Catch ex As Exception
                            Dim current As Exception, errMsg As String
                            current = ex
                            While Not (current Is Nothing)
                                errMsg = current.Message
                                current = current.InnerException
                            End While
                            RaiseEvent onDebug("OnDataReceived: " & errMsg)
                        End Try
                    End While
                End While
            End If
        End While
    End Sub
End Class
