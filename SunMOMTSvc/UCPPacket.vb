#Region " simple exception handler "

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Class		: UCPPacketException
'''
'''-----------------------------------------------------------------------------
''' <summary>Simple exception for packet errors</summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	07-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Class UCPPacketException
    Inherits Exception
    '''-----------------------------------------------------------------------------
    ''' <summary>The packets that threw the exception</summary>
    '''-----------------------------------------------------------------------------
    Public ReadOnly rawPacket As String

    Public Sub New(ByVal message As String, ByVal inner As Exception, ByVal theRawPacket As String)
        MyBase.New(message, inner)
        rawPacket = theRawPacket
    End Sub ' New
End Class

#End Region


#Region " Header class "

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Class		: header
'''
'''-----------------------------------------------------------------------------
''' <summary>All UCP packets consist of an header byt this specefiktaion</summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Class header
    Private packet_header_TRN As String = "00"
    Private packet_header_LEN As String = "00000"
    Private packet_header_OR As String = "O"
    Private packet_header_OT As String = "30"

    '''-----------------------------------------------------------------------------
    ''' <summary>Takes an raw UCP packets and parses the headere so they corrospond</summary>
    ''' <param name="dump">Raw packets</param>
    ''' <remarks>use this to identify the type of packets</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub parseHeader(ByVal dump As String)
        packet_header_TRN = "00"
        packet_header_LEN = "00000"
        packet_header_OR = "O"
        packet_header_OT = "30"
        If InStr(dump, "/") > 0 Then
            Dim aDump() As String = Split(dump, "/")
            If aDump.GetLength(0) >= 4 Then
                packet_header_TRN = aDump(0)
                packet_header_LEN = aDump(1)
                packet_header_OR = aDump(2)
                packet_header_OT = aDump(3)
            Else
                Throw New UCPPacketException("packets does not contain a full header", New Exception, dump)
            End If
        Else
            Throw New UCPPacketException("packets is not an UDP packets", New Exception, dump)
        End If
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>Returns and string representing the header of the packet</summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public ReadOnly Property topacket() As String
        Get
            Return packet_header_TRN & "/" & packet_header_LEN & "/" & _
                packet_header_OR & "/" & packet_header_OT
        End Get
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>Gets or sets the transaction number of the packets</summary>
    ''' <value>new transaction id, 2 octets</value>
    ''' <remarks>dont modify this manualy</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Property transaction_number() As Integer
        Get
            Return packet_header_TRN
        End Get
        Set(ByVal Value As Integer)
            If Value > 0 And Value < 100 Then
                packet_header_TRN = Value.ToString
                If packet_header_TRN < 10 Then
                    packet_header_TRN = "0" & packet_header_TRN
                End If
            End If
        End Set
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>
    '''     Length of whole packets
    ''' </summary>
    ''' <value>Length, in 5 octets</value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Property LEN() As Integer
        Get
            Return packet_header_LEN
        End Get
        Set(ByVal Value As Integer)
            If Value > 1 And Value < 10000 Then
                packet_header_LEN = Value.ToString
                If packet_header_LEN.Length < 5 Then
                    packet_header_LEN = Space(5 - packet_header_LEN.Length) & packet_header_LEN
                    packet_header_LEN = Replace(packet_header_LEN, " ", "0")
                End If
            End If
        End Set
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>Type of operation this pacets represents</summary>
    ''' <value>Sets the operation type.</value>
    ''' <remarks>Dont modify this manualy</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Property Operation() As enumOT
        Get
            Return packet_header_OT
        End Get
        Set(ByVal Value As enumOT)
            packet_header_OT = Value
            If packet_header_OT.Length < 2 Then
                packet_header_OT = "0" & packet_header_OT
            End If
        End Set
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>Is this an operation go request ( acknowledgement ) packet </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Property is_Operation_or_Request() As enumOR
        Get
            If packet_header_OR = "O" Then
                Return enumOR.Operation
            Else
                Return enumOR.Request
            End If
        End Get
        Set(ByVal Value As enumOR)
            If Value = enumOR.Operation Then
                packet_header_OR = "O"
            Else
                packet_header_OR = "R"
            End If
        End Set
    End Property

    Public Sub New()
        transaction_number = TransActionNumber
        TransActionNumber += 1
        If TransActionNumber > 99 Then
            TransActionNumber = 1
        End If
    End Sub
End Class

#End Region

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Class		: UCPPacket
'''
'''-----------------------------------------------------------------------------
''' <summary>represents an single UCP packets.</summary>
''' <remarks>use create[type} to create packet types.
''' for acknowledgement of packets, use acknowledgement and resend packets back to smsc</remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Class UCPPacket
    Inherits header

    'Protected Friend Packet_header As New header
    Private packet_message As String
    Private packet_checksum As String = "00"

    '''-----------------------------------------------------------------------------
    ''' <summary>Creates an "empty" packets</summary>
    ''' <remarks>after this, use create[type} tp create the packets you need</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub New()
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>Parses raw ucp data, in to a UCP packets object.</summary>
    ''' <param name="packetData"></param>
    ''' <remarks>use this to "read" packets, and to send acknowledgements back to smsc</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub New(ByVal packetData As String)
        loadpacket = packetData
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>if this is an request packets, was the result an success ?</summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public ReadOnly Property success() As Boolean
        Get
            Return packet_message_ACK = "A"
        End Get
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>if not, what was the "custom" error messages from the SMSC</summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public ReadOnly Property error_message() As String
        Get
            If packet_message_ACK = "N" Then
                Return packet_message_SM : Else : Return "" : End If
        End Get
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>error number, in enumeration form, for easy look up of text and/or number</summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public ReadOnly Property last_errorcode() As Error_code
        Get
            If packet_message_ACK = "N" Then
                Try
                    Return packet_message_MVP
                Catch ex As Exception
                    Return Error_code.unknown_error_code
                End Try
            End If
            Return Error_code.No_error
        End Get
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>
    '''     who was this messages sendt to.
    '''     typically mobil number but can be ip_address-port or other
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Property receiver() As String
        Get
            Return packet_message_AdC
        End Get
        Set(ByVal Value As String)
            packet_message_AdC = Value
        End Set
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>
    '''     who was this messages designated for.
    '''     typically mobil number but can be ip_address-port or other
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Property Originator() As String
        Get
            Return packet_message_OAdC
        End Get
        Set(ByVal Value As String)
            packet_message_OAdC = Value
        End Set
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>Theres 3 types of messages. Numeric, AlphaNumeric and Binary messages. This finds the type in the packets and returns that.</summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Property Message() As String
        Get
            If packet_message_MT = "2" Then
                Return packet_message_NMsg
            ElseIf packet_message_MT = "3" Then
                Return IA5ToStr(packet_message_AMsg)
            ElseIf packet_message_MT = "4" Then
                Return IA5ToStr(packet_message_AMsg)
            Else
                If packet_message_AMsg <> "" Then
                    Return IA5ToStr(packet_message_AMsg)
                Else
                    Return ""
                End If
            End If
        End Get
        Set(ByVal Value As String)
            If Value.Length > 160 Then Value = Mid(Value, 1, 160)
            packet_message_AMsg = StrToIA5(Value)
        End Set
    End Property


    '''-----------------------------------------------------------------------------
    ''' <summary>Loads and RAW packets, and parses it to and UCP packets object</summary>
    ''' <value>The RAW packets</value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public WriteOnly Property loadpacket() As String
        Set(ByVal Value As String)
            Value = Replace(Value, UCP_STX, "")
            Value = Replace(Value, UCP_ETX, "")
            Try
                parseHeader(Value)
            Catch ex As Exception
                Throw New UCPPacketException("caught an exception parsing header", ex, Value)
            End Try
            Dim aPacket() As String = Split(Value, "/")
            If is_Operation_or_Request = enumOR.Request Then
                If (Operation = enumOT.Session_management_operation And aPacket.GetLength(0) <> 7) Or _
                    (Operation <> enumOT.Session_management_operation And aPacket.GetLength(0) <> 8) Then
                    Throw New UCPPacketException("packet has an invalid syntax", New Exception, Value)
                End If
                packet_message_ACK = aPacket(4)
                packet_message_MVP = aPacket(5)
                If Operation = enumOT.Session_management_operation And packet_message_ACK = "A" Then
                    packet_checksum = aPacket(6)
                Else
                    packet_message_SM = aPacket(6)
                    packet_checksum = aPacket(7)
                End If

                If InStr(packet_message_SM, ":") > 0 Then
                    packet_message_SM_AdC = packet_message_SM.Split(":")(0)
                    packet_message_SM_SCTC = packet_message_SM.Split(":")(1)
                    Try
                        packet_message_AMsg = StrToIA5("Messages to " & packet_message_SM_AdC & " was accepted at " & _
                        packet_message_SM_SCTC.Substring(0, 2) & "-" & _
                        packet_message_SM_SCTC.Substring(2, 2) & "-" & _
                        packet_message_SM_SCTC.Substring(4, 2) & " " & _
                        packet_message_SM_SCTC.Substring(6, 2) & ":" & _
                        packet_message_SM_SCTC.Substring(8, 2) & ":" & _
                        packet_message_SM_SCTC.Substring(10, 2))
                    Catch ex As Exception
                        ' dont bothere throwing an exception here.
                    End Try
                End If
            Else
                Select Case Operation
                    Case enumOT.Call_Input_Operation, enumOT.SMS_message_transfer_operation
                        If aPacket.GetLength(0) <> 10 Then
                            Throw New UCPPacketException("packet has an invalid syntax", New Exception, Value)
                        End If
                        packet_message_AdC = aPacket(4)
                        packet_message_OAdC = aPacket(5)
                        packet_message_AC = aPacket(6)
                        packet_message_MT = aPacket(7)
                        If packet_message_MT = 2 Then
                            ' Numeric message 
                            packet_message_NMsg = aPacket(8)
                        ElseIf packet_message_MT = 3 Then
                            ' Message is IA5
                            packet_message_AMsg = aPacket(8)
                        Else
                            ' What happend ?
                            packet_message_AMsg = aPacket(8)
                        End If
                        packet_checksum = aPacket(9)
                    Case enumOT.Deliver_notification, enumOT.Deliver_short_message
                        If aPacket.GetLength(0) <> 38 Then
                            Throw New UCPPacketException("packet has an invalid syntax", New Exception, Value)
                        End If
                        packet_message_AdC = aPacket(4)
                        packet_message_OAdC = aPacket(5)
                        packet_message_AC = aPacket(6)
                        packet_message_NRq = aPacket(7)
                        packet_message_NAdC = aPacket(8)
                        packet_message_NT = aPacket(9)
                        packet_message_NPID = aPacket(10)
                        packet_message_LRq = aPacket(11)
                        packet_message_LRAd = aPacket(12)
                        packet_message_LPID = aPacket(13)
                        packet_message_DD = aPacket(14)
                        packet_message_DDT = aPacket(15)
                        packet_message_VP = aPacket(16)
                        packet_message_RPID = aPacket(17)
                        packet_message_SCTS = aPacket(18)
                        packet_message_Dst = aPacket(19)
                        packet_message_Rsn = aPacket(20)
                        packet_message_DSCTS = aPacket(21)
                        packet_message_MT = aPacket(22)
                        If packet_message_MT = "2" Then
                            packet_message_NB = aPacket(23)
                            packet_message_NMsg = aPacket(24)
                        ElseIf packet_message_MT = "3" Then
                            packet_message_NB = aPacket(23)
                            packet_message_AMsg = aPacket(24)
                        Else ' if = "4"
                            packet_message_NB = aPacket(23)
                            packet_message_TMsg = aPacket(24)
                        End If
                        packet_message_MMS = aPacket(25)
                        packet_message_PR = aPacket(26)
                        packet_message_DCs = aPacket(27)
                        packet_message_MCLs = aPacket(28)
                        packet_message_RPI = aPacket(29)

                        packet_message_CPg = aPacket(30)
                        packet_message_RPLy = aPacket(31)
                        packet_message_OTOA = aPacket(32)
                        packet_message_HPLMN = aPacket(33)
                        packet_message_XSer = aPacket(34)
                        packet_message_RES4 = aPacket(35)
                        packet_message_RES5 = aPacket(36)
                        packet_checksum = aPacket(37)
                    Case Else
                        If aPacket.GetLength(0) <> 15 Then
                            Throw New UCPPacketException("packet has an invalid syntax", New Exception, Value)
                        End If
                        packet_message_AdC = aPacket(4)
                        packet_message_OAdC = aPacket(5)
                        packet_message_AC = aPacket(6)
                        packet_message_NRq = aPacket(7)
                        packet_message_NAd = aPacket(8)
                        packet_message_NPID = aPacket(9)
                        packet_message_DD = aPacket(10)
                        packet_message_DDT = aPacket(11)
                        packet_message_VP = aPacket(12)
                        packet_message_AMsg = aPacket(13)
                        packet_checksum = aPacket(14)
                End Select
            End If
        End Set
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>Creates and ready-to-send packets by the data specefied on object</summary>
    ''' <value>RAW UCP packets</value>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public ReadOnly Property createpacket() As String

        Get
            If is_Operation_or_Request = enumOR.Operation Then

                Select Case Operation
                    Case enumOT.Call_Input_Operation ' UCP 1.0 messages.
                        packet_message = packet_message_ACK
                        packet_message = packet_message & "/" & packet_message_MVP
                        packet_message = packet_message & "//" & packet_message_SM

                    Case enumOT.Submit_short_message, enumOT.Deliver_notification ' 50 Series way. More advanced.

                        packet_message = packet_message_AdC
                        packet_message = packet_message & "/" & packet_message_OAdC
                        If packet_message_AC.Length = 4 Then
                            packet_message = packet_message & "/" & packet_message_AC
                        Else
                            packet_message = packet_message & "/" & ""
                        End If
                        packet_message = packet_message & "/" & packet_message_NRq
                        packet_message = packet_message & "/" & packet_message_NAdC
                        packet_message = packet_message & "/" & packet_message_NT
                        packet_message = packet_message & "/" & packet_message_NPID
                        packet_message = packet_message & "/" & packet_message_LRq
                        packet_message = packet_message & "/" & packet_message_LRAd
                        packet_message = packet_message & "/" & packet_message_LPID
                        If packet_message_DD <> "" And packet_message_DDT <> "" Then
                            packet_message = packet_message & "/" & packet_message_DD
                            packet_message = packet_message & "/" & packet_message_DDT
                        Else
                            packet_message = packet_message & "/" & ""
                            packet_message = packet_message & "/" & ""
                        End If
                        packet_message = packet_message & "/" & packet_message_VP
                        packet_message = packet_message & "/" & packet_message_RPID
                        packet_message = packet_message & "/" & packet_message_SCTS
                        packet_message = packet_message & "/" & packet_message_Dst
                        packet_message = packet_message & "/" & packet_message_Rsn
                        packet_message = packet_message & "/" & packet_message_DSCTS
                        packet_message = packet_message & "/" & packet_message_MT
                        If packet_message_MT = "2" Then
                            packet_message = packet_message & "/" & packet_message_NB
                            packet_message = packet_message & "/" & packet_message_NMsg
                        ElseIf packet_message_MT = "3" Then
                            packet_message = packet_message & "/" & packet_message_NB
                            packet_message = packet_message & "/" & packet_message_AMsg
                        Else ' if = "4"
                            packet_message = packet_message & "/" & packet_message_NB
                            packet_message = packet_message & "/" & packet_message_TMsg
                        End If
                        packet_message = packet_message & "/" & packet_message_MMS
                        packet_message = packet_message & "/" & packet_message_PR
                        packet_message = packet_message & "/" & packet_message_DCs
                        packet_message = packet_message & "/" & packet_message_MCLs
                        packet_message = packet_message & "/" & packet_message_RPI

                        packet_message = packet_message & "/" & packet_message_CPg
                        packet_message = packet_message & "/" & packet_message_RPLy
                        packet_message = packet_message & "/" & packet_message_OTOA
                        packet_message = packet_message & "/" & packet_message_HPLMN
                        packet_message = packet_message & "/" & packet_message_XSer
                        packet_message = packet_message & "/" & packet_message_RES4
                        packet_message = packet_message & "/" & packet_message_RES5
                        If packet_message_RPID = "0127" Then
                            If packet_message_MT <> "4" Or packet_message_MCLs <> "2" Then
                                ' Messages will be rejected but let user find out the hard way.
                            End If
                        End If
                    Case enumOT.SMS_message_transfer_operation ' Old UCP way. allways work 
                        packet_message = packet_message_AdC
                        packet_message = packet_message & "/" & packet_message_OAdC
                        packet_message = packet_message & "/" & packet_message_AC
                        packet_message = packet_message & "/" & packet_message_NRq
                        packet_message = packet_message & "/" & packet_message_NAd
                        packet_message = packet_message & "/" & packet_message_NPID
                        packet_message = packet_message & "/" & packet_message_DD
                        packet_message = packet_message & "/" & packet_message_DDT
                        packet_message = packet_message & "/" & packet_message_VP
                        packet_message = packet_message & "/" & packet_message_AMsg
                    Case enumOT.Session_management_operation ' 60 series. Auth f.eks
                        packet_message = packet_message_OAdC & "/" & _
                        packet_message_OTON & "/" & _
                        packet_message_ONPI & "/" & _
                        packet_message_STYP & "/" & _
                        packet_message_PWD & "/" & _
                        packet_message_NPWD & "/" & _
                        packet_message_VERS & "/" & _
                        packet_message_LAdC & "/" & _
                        packet_message_LTON & "/" & _
                        packet_message_LNPI & "/" & _
                        packet_message_OPID & "/" & _
                        packet_message_RES1
                    Case enumOT.SMT_alert_operation
                        packet_message = packet_message_AdC & "/" & _
                        packet_message_PID
                        'packet_checksum = CheckSum(topacket & "/" & packet_message & "/")
                        'Return "02/00026/O/31/" & packet_message & "/" & packet_checksum & UCP_ETX
                        'Return "02/00035/O/31/0234765439845/0139/A0"
                    Case Else
                        ' not implementet yet
                        packet_message = ""
                End Select
            Else
                Select Case Operation
                    Case enumOT.Deliver_notification
                        If packet_message_ACK = "A" Then
                            packet_message = packet_message_ACK
                            packet_message = packet_message & "/" & packet_message_MVP
                            packet_message = packet_message & "//" & packet_message_SM
                        Else
                            packet_message = packet_message_ACK
                            packet_message = packet_message & "/" & packet_message_EC
                            packet_message = packet_message & "//" & packet_message_SM
                        End If
                    Case Else
                        packet_message = packet_message_ACK
                        If packet_message_ACK = "A" Then
                            packet_message = packet_message & "//" & packet_message_SM
                        Else
                            packet_message = packet_message & "/" & packet_message_MVP
                            packet_message = packet_message & "//" & packet_message_SM
                        End If
                End Select
            End If

            LEN = topacket.Length + packet_message.Length
            ' Add 1 for the leading / for checksum 
            ' and the one between header and massage
            ' plus 2 for check sum
            LEN += 4


            packet_checksum = CheckSum(topacket & "/" & packet_message & "/")
            Return UCP_STX & topacket & "/" & packet_message & "/" & packet_checksum & UCP_ETX
        End Get
    End Property

    '''-----------------------------------------------------------------------------
    ''' <summary>
    ''' modify packets to be and acknowledgement on an operation.
    ''' </summary>
    ''' <param name="posetive">Did we succesfullt understand and operated the operation =</param>
    ''' <param name="SystemMessage">Custom messages to system</param>
    ''' <param name="Errorcode">If an error, suply error number here</param>
    ''' <remarks>use this to reply to operation requests from SMSC. typecaly SMS receipts</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub acknowledgement(ByVal posetive As Boolean, ByVal SystemMessage As String, Optional ByVal Errorcode As Error_code = 0)
        is_Operation_or_Request = enumOR.Request
        If posetive Then
            packet_message_ACK = "A"
            packet_message_MVP = ""
            packet_message_SM = SystemMessage
        Else
            packet_message_ACK = "N"
            packet_message_EC = Errorcode
            packet_message_SM = SystemMessage
        End If
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>Most SMSC requere login. This creates and authentication packets by using management operations.</summary>
    ''' <param name="uid"></param>
    ''' <param name="password"></param>
    ''' <remarks>dont send this, id SMSC dont requere you to logon</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub CreateLoginPacket(ByVal uid As String, ByVal password As String)
        Operation = enumOT.Session_management_operation

        packet_message_OAdC = uid
        packet_message_OTON = "6"
        packet_message_ONPI = "5"
        packet_message_STYP = "1"
        '1 = open session
        '2 = reserved
        '3 = change password
        '4 = open provisioning session
        '5 = reserved
        '6 = change provisioning password
        packet_message_PWD = StrToIA5(password)
        packet_message_VERS = "0100" ' Version number ?100' i.flg doc
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>ALL UCP complaint devices understand this simple code 31 operation messages. use this to send simple sms messages.</summary>
    ''' <param name="Public"></param>
    ''' <param name="from">who is SMS from</param>
    ''' <param name="sto">who is SMS to</param>
    ''' <param name="msg">The alpha numeric messages </param>
    ''' <remarks>we allways presume aphanumeric. pages will not accept this, but hey. buy a new device then.</remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub CreateSimpleSMSpacket( _
                    ByVal from As String, ByVal sto As String, _
                ByVal msg As String)
        Operation = enumOT.SMS_message_transfer_operation

        packet_message_OAdC = from
        packet_message_AdC = sto
        packet_message_AMsg = StrToIA5(msg)
    End Sub

    'Added by Patrick 2016-10-13
    'Keep alive function
    Public Sub CreateKeepAlivePacket()
        Operation = enumOT.SMT_alert_operation

        packet_message_AdC = "2488"
        packet_message_PID = "0539"
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>Overload function to simplyfy sending SMS throug code 51 operation</summary>
    ''' <param name="Public"></param>
    ''' <param name="from">who is sms from</param>
    ''' <param name="sto">who is sms to</param>
    ''' <param name="msg">The alpha numeric messages</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub Create_Submit_AlphaNum_SMS( _
                ByVal from As String, ByVal sto As String, _
            ByVal msg As String, ByVal charge As String, ByVal page As String, ByVal keywordID As String)
        Create_Submit_AlphaNum_SMS(from, sto, msg, charge, True, "", New Notification_Type, Notification_PID.Mobile_Station, page, keywordID)
        'setPrice(charge)
    End Sub

    Private Sub setPrice(ByVal charge As String, ByVal page As String, ByVal keywordID As String)

        Select Case charge
            Case "0"
                If page = "0" Then
                    packet_message_XSer = ""
                Else
                    packet_message_XSer = "010605000326" & page 'page
                End If

            Case "100"
                If page = "0" Then
                    packet_message_XSer = "0C12303130303030303143313233303030303130"
                Else
                    packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030303130"
                    'packet_message_XSer = "0C12303130303030303143313233303030303230" & "000394" & page
                End If

            Case "200"
                If keywordID = "554" Or keywordID = "556" Or keywordID = "557" Then
                    If page = "0" Then
                        packet_message_XSer = "0C12303130303030303143313233303030303330"
                    Else
                        packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030303330"
                        'packet_message_XSer = "0C1233130303030303143313233303030303230" & "000394" & page
                    End If
                Else
                    If page = "0" Then
                        packet_message_XSer = "0C12303130303030303143313233303030303230"
                    Else
                        packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030303230"
                        'packet_message_XSer = "0C1233130303030303143313233303030303230" & "000394" & page
                    End If
                End If
            Case "500"
                If keywordID = "125" Or keywordID = "306" Then 'VID, VIDS
                    If page = "0" Then
                        packet_message_XSer = "0C12303130303030303143313233303030313730"
                    Else
                        packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030313730"
                        'packet_message_XSer = "0C123031303030303031433132333030303135300d0a" & "000394" & page
                    End If
                ElseIf keywordID = "122" Or keywordID = "304" Then 'WP, WPS
                    If page = "0" Then
                        packet_message_XSer = "0C12303130303030303143313233303030313530"
                    Else
                        packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030313530"
                    End If
                ElseIf keywordID = "305" Then 'CPLAY
                    If page = "0" Then
                        packet_message_XSer = "0C12303130303030303143313233303030303530"
                    Else
                        packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030303530"
                    End If
                Else ' TONE, TONES etc.
                    If page = "0" Then
                        packet_message_XSer = "0C12303130303030303143313233303030313630"
                    Else
                        packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030313630"
                        'packet_message_XSer = "0C123031303030303031433132333030303135300d0a" & "000394" & page
                    End If
                End If
            Case "1500"
                If page = "0" Then
                    packet_message_XSer = "0C123031303030303031433132333030303344300d0a"
                Else
                    packet_message_XSer = "010605000326" & page & "0C123031303030303031433132333030303344300d0a"
                    'packet_message_XSer = "0C123031303030303031433132333030303344300d0a" & "000394" & page
                End If

            Case "3000"
                If page = "0" Then
                    packet_message_XSer = "0C123031303030303031433132333030303739300d0a"
                Else
                    packet_message_XSer = "010605000326" & page & "0C123031303030303031433132333030303739300d0a"
                    'packet_message_XSer = "0C123031303030303031433132333030303739300d0a" & "000394" & page
                End If

            Case "5000"
                If page = "0" Then
                    packet_message_XSer = "0C123031303030303031433132333030304339300d0a"
                Else
                    packet_message_XSer = "010605000326" & page & "0C123031303030303031433132333030304339300d0a"
                    'packet_message_XSer = "0C123031303030303031433132333030304339300d0a" & "000394" & page
                End If

            Case Else
                If page = "0" Then
                    packet_message_XSer = "0C12303130303030303143313233303030303230"
                Else
                    packet_message_XSer = "010605000326" & page & "0C12303130303030303143313233303030303230"
                    'packet_message_XSer = "0C12303130303030303143313233303030303230" & "000394" & page
                End If

        End Select



    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Public"></param>
    ''' <param name="from">who is sms from</param>
    ''' <param name="sto">who is sms to</param>
    ''' <param name="msg">The alpha numeric messages</param>
    ''' <param name="notification">request notification on messages</param>
    ''' <param name="notificationAddr">leave blank for senders number</param>
    ''' <param name="NotificationType">what kind of mesages notifications do we want</param>
    ''' <param name="NotificationPID">what type of device is the receivere of the notifications</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    'Public Sub Create_Submit_AlphaNum_SMS( _
    '                ByVal from As String, ByVal sto As String, _
    '            ByVal msg As String, _
    '             ByVal charge As String, _
    '            ByVal notification As Boolean, _
    '            ByVal notificationAddr As String, _
    '            ByVal NotificationType As Notification_Type, _
    '            ByVal NotificationPID As Notification_PID, _
    '            ByVal page As String)
    '    Operation = enumOT.Submit_short_message
    '    'packet_message_
    '    packet_message_OAdC = from
    '    packet_message_AdC = sto
    '    packet_message_MT = "3"
    '    packet_message_NB = "" ' should this be set for non DT packets ?
    '    packet_message_AMsg = StrToIA5(msg)
    '    If notification Then
    '        packet_message_NT = NotificationType.value

    '        '0 = NAdC not used
    '        '1 = NAdC used
    '        If notificationAddr = "" Then
    '            packet_message_NRq = "0"
    '        Else
    '            packet_message_NRq = "1"
    '            packet_message_NAdC = notificationAddr
    '            packet_message_NPID = NotificationPID
    '            If packet_message_NPID.Length < 4 Then
    '                ' its allways 3, so ....
    '                packet_message_NPID = "0" & packet_message_NPID
    '            End If
    '        End If
    '    End If

    '    'Modified by Patrick to not send Notification Request if 2nd part of message 2014-04-11
    '    'Commented by Patrick 2015-04-08
    '    'If page = "0202" Then
    '    '    packet_message_NRq = ""
    '    'Else
    '    '    'Wesley testing for charging 
    '    '    packet_message_NRq = "1"
    '    'End If

    '    'Modified by Patrick to handle greater than 2 part message 2015-04-08
    '    If page >= "0202" And page < "0301" Then
    '        packet_message_NRq = ""
    '    ElseIf page >= "0302" And page < "0401" Then
    '        packet_message_NRq = ""
    '    ElseIf page >= "0402" Then
    '        packet_message_NRq = ""
    '    Else
    '        packet_message_NRq = "1"
    '    End If

    '    'packet_message_XSer = "0C12303130303030303143313233303030443230"
    '    '01000001C123000020
    '    'packet_message_XSer = "0C12303130303030303143313233303030303230"

    '    'packet_message_XSer = "0C12303130303030303143313233303030303230"
    '    setPrice(charge, page)
    'End Sub

    Public Sub Create_Submit_AlphaNum_SMS( _
                    ByVal from As String, ByVal sto As String, _
                ByVal msg As String, _
                 ByVal charge As String, _
                ByVal notification As Boolean, _
                ByVal notificationAddr As String, _
                ByVal NotificationType As Notification_Type, _
                ByVal NotificationPID As Notification_PID, _
                ByVal page As String, _
                ByVal keywordID As String)

        Operation = enumOT.Submit_short_message
        'packet_message_
        packet_message_OAdC = from
        packet_message_AdC = sto
        packet_message_MT = "3"
        packet_message_NB = "" ' should this be set for non DT packets ?
        packet_message_AMsg = StrToIA5(msg)
        If notification Then
            'packet_message_NT = NotificationType.value
            packet_message_NT = 3

            '0 = NAdC not used
            '1 = NAdC used
            If notificationAddr = "" Then
                packet_message_NRq = "0"
            Else
                packet_message_NRq = "1"
                packet_message_NAdC = notificationAddr
                packet_message_NPID = NotificationPID
                If packet_message_NPID.Length < 4 Then
                    ' its allways 3, so ....
                    packet_message_NPID = "0" & packet_message_NPID
                End If
            End If
        End If

        'Modified by Patrick to not send Notification Request if 2nd part of message 2014-04-11
        'Commented by Patrick 2015-04-08
        'If page = "0202" Then
        '    packet_message_NRq = ""
        'Else
        '    'Wesley testing for charging 
        '    packet_message_NRq = "1"
        'End If

        'Modified by Patrick to handle greater than 2 part message 2015-04-08
        If page >= "0202" And page < "0301" Then
            packet_message_NRq = ""
        ElseIf page >= "0302" And page < "0401" Then
            packet_message_NRq = ""
        ElseIf page >= "0402" Then
            packet_message_NRq = ""
        Else
            packet_message_NRq = "1"
        End If

        'packet_message_XSer = "0C12303130303030303143313233303030443230"
        '01000001C123000020
        'packet_message_XSer = "0C12303130303030303143313233303030303230"

        'packet_message_XSer = "0C12303130303030303143313233303030303230"
        setPrice(charge, page, keywordID)
    End Sub

    Public Function getSendPacketDef() As String
        Return "CODE: " & enumOT.Submit_short_message & _
        " 0AdC: " & packet_message_OAdC & _
        " AdC: " & packet_message_AdC & _
        " MT: " & packet_message_MT & _
        " NB: " & packet_message_NB & _
        " AMsg: " & packet_message_AMsg & _
        " NT: " & packet_message_NT & _
        " NRq: " & packet_message_NRq & _
        " NAdC: " & packet_message_NAdC & _
        " NPID: " & packet_message_NPID
    End Function


    ' Operation message fields
    Public packet_message_AdC As String = ""
    Public packet_message_OAdC As String = ""
    Public packet_message_AC As String = ""
    Public packet_message_NRq As String = ""
    Public packet_message_NAd As String = ""
    Public packet_message_NPID As String = ""
    Public packet_message_DD As String = ""
    Public packet_message_DDT As String = ""
    Public packet_message_VP As String = ""
    Public packet_message_AMsg As String = ""


    ' 51 Submit Short Messages operation specefik fields
    'Private packet_message_AdC As String = ""
    'Private packet_message_OAdC As String = ""
    'Private packet_message_AC As String = ""
    'Private packet_message_NRq As String = ""
    Private packet_message_NAdC As String = ""
    Private packet_message_NT As String = ""
    'Private packet_message_NPID As String = ""
    Private packet_message_LRq As String = ""
    Private packet_message_LRAd As String = ""
    Private packet_message_LPID As String = ""
    'Private packet_message_DD As String = ""
    'Private packet_message_DDT As String = ""
    'Private packet_message_VP As String = ""
    Private packet_message_RPID As String = ""
    Private packet_message_SCTS As String = ""
    Private packet_message_Dst As String = ""
    Private packet_message_Rsn As String = ""
    Private packet_message_DSCTS As String = ""
    Private packet_message_MT As String = ""
    Private packet_message_NB As String = ""
    Private packet_message_NMsg As String = "" ' Numeric message
    'Private packet_message_AMsg As String = "" ' Alpha numeric massega
    Private packet_message_TMsg As String = "" ' Transparent Data message
    Private packet_message_MMS As String = ""
    Private packet_message_PR As String = ""
    Private packet_message_DCs As String = ""
    Private packet_message_MCLs As String = ""
    Private packet_message_RPI As String = ""
    Private packet_message_CPg As String = ""
    Private packet_message_RPLy As String = ""
    Private packet_message_OTOA As String = ""
    Private packet_message_HPLMN As String = ""
    Public packet_message_XSer As String = ""
    Private packet_message_RES4 As String = ""
    Private packet_message_RES5 As String = ""



    ' &0 Series messages fields
    'OAdC   'Address code originator
    'OTON   ' Originator Type of Number
    Public packet_message_OTON As String = ""
    'ONPI   ' Originator Numbering Plan Id
    Public packet_message_ONPI As String = ""
    'STYP   ' Subtype of operation
    Public packet_message_STYP As String = ""
    'PWD    ' Current password encoded into IA5 characters
    Public packet_message_PWD As String = ""
    'NPWD   ' New password encoded into IA5 characters
    Public packet_message_NPWD As String = ""
    'VERS   ' Version number
    Public packet_message_VERS As String = ""
    'LAdC   ' Address for VSMSC list operation
    Public packet_message_LAdC As String = ""
    'LTON   ' Type of Number list address
    Public packet_message_LTON As String = ""
    'LNPI   ' Numbering Plan Id list address
    Public packet_message_LNPI As String = ""
    'OPID   ' Originator Protocol Identifier
    Public packet_message_OPID As String = ""
    'RES1   ' (reserved for future use)
    Public packet_message_RES1 As String = ""


    ' Response message fields
    Private packet_message_ACK As String = ""
    Private packet_message_MVP As String = ""

    ' Positive or negative acknowledgement text
    Private packet_message_SM As String = ""
    ' if ACK = A = Positive acknowledgement
    Private packet_message_SM_AdC As String = ""
    Private packet_message_SM_SCTC As String = ""
    Private packet_message_EC As String = ""

    ' Call input Operation
    'Private packet_message_MT As String = ""

    Private packet_message_PID As String = ""
End Class

