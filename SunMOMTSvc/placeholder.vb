'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Module		: placeholder
'''
'''-----------------------------------------------------------------------------
''' <summary>
'''     Placeholder for enumerations and general functions
''' </summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Module placeholder
    '''-----------------------------------------------------------------------------
    ''' <summary>Start of packets contain this</summary>
    '''-----------------------------------------------------------------------------
    Public Const UCP_STX As Char = Chr(2)
    '''-----------------------------------------------------------------------------
    ''' <summary>End of packets must contain this</summary>
    '''-----------------------------------------------------------------------------
    Public Const UCP_ETX As Char = Chr(3)
    '''-----------------------------------------------------------------------------
    ''' <summary>Global counter for transaction ids for packets</summary>
    '''-----------------------------------------------------------------------------
    Public TransActionNumber As Integer = 0

    '''-----------------------------------------------------------------------------
    ''' <summary>Checks if an string contains an integer value</summary>
    ''' <param name="value">string containing integer</param>
    ''' <returns>true if contains and integer, else returns false</returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	09-02-2004	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Function IsInteger(ByVal value As String) As Boolean
        Dim result As Boolean = True
        Try
            System.Int32.Parse(value)
        Catch ex As Exception
            result = False
        End Try
        Return result
    End Function

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
    Public Function encodeIpPort(ByVal Ip As String, ByVal port As Integer) As String
        Dim result As String, ips() As String = Split(Ip, ".")
        If ips.GetLength(0) = 4 Then
            For i As Integer = 0 To 3
                If ips(i).Length = 1 Then
                    ips(i) = "00" & ips(i)
                ElseIf ips(i).Length = 2 Then
                    ips(i) = "0" & ips(i)
                End If
                result = result & ips(i)
            Next
            Return result & port.ToString
        Else
            ' Illegal ip number
            Return ""
        End If
    End Function

    '''-----------------------------------------------------------------------------
    ''' <summary>Takes an SMSC formatet ip and port, and converts it to and easy
    ''' readeble ip address and port</summary>
    ''' <param name="Ipandport">SMSC port and ip</param>
    ''' <param name="ip">extractet ip address</param>
    ''' <param name="port">extractet port</param>
    ''' <remarks>return "" in ip and 0 in port, if input was not valid</remarks>
    ''' <history>
    ''' 	[bofh] 	09-02-2004	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Sub decodeIpPort(ByVal Ipandport As String, ByRef ip As String, ByRef port As Integer)
        port = 0 : ip = ""
        If Ipandport.Length > 14 Then
            If IsInteger(Mid(Ipandport, 13)) Then
                port = System.Int32.Parse(Mid(Ipandport, 13))
            Else : Exit Sub : End If
            ip = Mid(Ipandport, 1, 3) & "." & _
            Mid(Ipandport, 4, 3) & "." & _
            Mid(Ipandport, 7, 3) & "." & _
            Mid(Ipandport, 10, 3)
        End If
    End Sub

    '''-----------------------------------------------------------------------------
    ''' <summary>The checksum is the 8 LSB (least significant bits) of a simple addition of all octets (note that a message type '30' is two octets with the values 33 hex and 30 hex) following the STX character and until the checksum field. Note that all separators will be included in the checksum, but the STX, checksum and the ETX will not be included</summary>
    ''' <param name="TheText">packets to generate checksum for</param>
    ''' <returns>checksom for packets</returns>
    ''' <remarks>This code i found on news.
    ''' See: http://groups.google.dk/groups?hl=da&lr=&ie=UTF-8&oe=UTF-8&frame=right&th=d005b5dafb6a130f&seekm=%25Glsa.32182%24D%254.31009%40nwrdny03.gnilink.net#link7
    ''' </remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Function CheckSum(ByVal TheText As String) As String
        Dim s As Long
        Dim x As Long
        For x = 1 To Len(TheText)
            s = (s + Asc(Mid$(TheText, x, 1))) And 255
        Next
        CheckSum = Right$("0" & Hex$(s), 2)
    End Function

    '''-----------------------------------------------------------------------------
    ''' <summary>Converts ascii strings containing alphanumeric letters to an , IA5en coded encoded string</summary>
    ''' <param name="str">Ascii string to convert</param>
    ''' <returns>IA5 encoded string</returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Function StrToIA5(ByVal str As String) As String
        Dim result As String
        For i As Integer = 0 To (str.Length - 1)
            Dim tmpResult As String
            tmpResult = Hex(Asc(str.Substring(i, 1)))
            If tmpResult.Length = 1 Then tmpResult = "0" & tmpResult
            result = result & tmpResult
        Next
        Return result
    End Function

    '''-----------------------------------------------------------------------------
    ''' <summary>Convert an IA5 encoded string, to an alphanumeric ascii string</summary>
    ''' <param name="str">IA5 encoded string</param>
    ''' <returns>alphanumeric ascii string</returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' 	[bofh] 	06-02-2003	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Function IA5ToStr(ByVal str As String) As String
        Dim result As String
        For i As Integer = 0 To (str.Length - 1) Step 2
            Dim tmpResult As String = str.Substring(i, 2)
            tmpResult = CLng("&H" & tmpResult)
            result = result & Chr(tmpResult)
        Next
        Return result
    End Function

End Module

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Structure		: Notification_Type
'''
'''-----------------------------------------------------------------------------
''' <summary>
'''     Types of notifications that can be requestet for sendt messages
''' </summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Structure Notification_Type
    '''-----------------------------------------------------------------------------
    ''' <summary>send notification when SMS could not be delivered but will tryed again later</summary>
    '''-----------------------------------------------------------------------------
    Public Buffered_message_notification As Boolean
    '''-----------------------------------------------------------------------------
    ''' <summary>send notification when SMS messages has been delivered</summary>
    '''-----------------------------------------------------------------------------
    Public Delivery_Notification As Boolean
    '''-----------------------------------------------------------------------------
    ''' <summary>send notification if SMS messages could not be delivered</summary>
    '''-----------------------------------------------------------------------------
    Public Non_delivery_notification As Boolean

    Function value() As Integer
        If Delivery_Notification And Not Buffered_message_notification And Not Non_delivery_notification Then
            Return 1
        ElseIf Not Delivery_Notification And Not Buffered_message_notification And Non_delivery_notification Then
            Return 2
        ElseIf Delivery_Notification And Not Buffered_message_notification And Non_delivery_notification Then
            Return 3
        ElseIf Not Delivery_Notification And Buffered_message_notification And Not Non_delivery_notification Then
            Return 4
        ElseIf Delivery_Notification And Buffered_message_notification And Not Non_delivery_notification Then
            Return 5
        ElseIf Not Delivery_Notification And Buffered_message_notification And Non_delivery_notification Then
            Return 6
        Else
            Return 7
        End If
    End Function
End Structure

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Enum		: Notification_PID
'''
'''-----------------------------------------------------------------------------
''' <summary>Different protocols notifications can be delivered over</summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Enum Notification_PID
    Mobile_Station = 100
    Fax_Group_3 = 122
    X_400 = 131
    Menu_over_PSTN = 138
    PC_appl_over_PSTN = 139
    PC_appl_over_X_25 = 339
    PC_appl_over_ISDN = 439
    PC_appl_over_TCP_IP = 539
End Enum

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Enum		: enumOR
'''
'''-----------------------------------------------------------------------------
''' <summary>Types of messages that can be sent and received.
''' Operations often is something we need to do action on, and we will reseive and
''' "request" when the operation has been accepted
''' 
''' Requests are ofthen, responses to and operation.
''' 
''' f.eks Send "operation" to SMSC that we want to send an SMS
''' we then receive and "request" telling if the sms was accepted</summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Enum enumOR
    Operation
    Request
End Enum

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Enum		: enumOT
'''
'''-----------------------------------------------------------------------------
''' <summary>
''' Different types of operations. 
''' Note that SMSC operations can only be sendt FROM an smsc, to an SMT
''' ( so we will receive these as operations, and we reply with an "request" )
''' SMT operations can only be sendt TO an sms
''' ( we send this to an SMSC, and in return we get an status "request" )
''' </summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Enum enumOT
    Call_Input_Operation = 1
    Multiple_address_call_input_operation = 2
    Call_input_with_supplementary_services_operation = 3
    SMS_message_transfer_operation = 30 ' Standart UCP way, allways work
    SMT_alert_operation = 31

    Submit_short_message = 51 ' SMT sends this ( 50 series ) - client
    Deliver_short_message = 52 ' SMSC sends this ( 50 series ) - server
    Deliver_notification = 53 ' SMSC sends this ( 50 Series )  server
    Modify_message = 54 ' SMT sends this ( 50 series ) - client
    Inquiry_message = 55 ' SMT sends this ( 50 series ) - client
    Delete_message = 56 ' SMT sends this ( 50 series ) - client
    Response_inquiry_message = 57 ' SMSC sends this ( 50 Series )  server
    Response_delete_message = 58 ' SMSC sends this ( 50 Series )  server

    Session_management_operation = 60 ' ( 60 series ) Session management. User auth
    Provisioning_actions_operation = 61 ' ( 60 series ) Provisioning interface. not sup.
End Enum

'''-----------------------------------------------------------------------------
''' Project		: DOTUCPEMI
''' Enum		: Error_code
'''
'''-----------------------------------------------------------------------------
''' <summary>
''' Known error messages codes, regarding SMSC 4.6 EMI - UCP interface
''' </summary>
''' <remarks></remarks>
''' <history>
''' 	[bofh] 	06-02-2003	Created
''' </history>
'''-----------------------------------------------------------------------------
Public Enum Error_code
    No_error = 0
    Checksum_error = 1
    Syntax_error = 2
    Operation_not_supported_by_system = 3
    Operation_not_allowed = 4
    Call_barring_active = 5
    AdC_invalid = 6
    Authentication_failure = 7
    Legitimisation_code_for_all_calls_failure = 8
    GA_not_valid = 9
    Repetition_not_allowed = 10
    Legitimisation_code_for_repetition_failure = 11
    Priority_call_not_allowed = 12
    Legitimisation_code_for_priority_call_failure = 13
    Urgent_message_not_allowed = 14
    Legitimisation_code_for_urgent_message_failure = 15
    Reverse_charging_not_allowed = 16
    Legitimisation_code_for_rev_charging_failure = 17
    Deferred_delivery_not_allowed = 18
    New_AC_not_valid = 19
    New_legitimisation_code_not_valid = 20
    Standard_text_not_valid = 21
    Time_period_not_valid = 22
    Message_type_not_supported_by_system = 23
    Message_too_long = 24
    Requested_standard_text_not_valid = 25
    Message_type_not_valid_for_the_pager_type = 26
    Message_not_found_in_smsc = 27
    Subscriber_hang_up = 30
    Fax_group_not_supported = 31
    Fax_message_type_not_supported = 32
    Address_already_in_list = 33 '(60 series)
    Address_not_in_list = 34  ' (60 series)
    List_full_cannot_add_address_to_list = 35 ' (60 series)
    RPID_already_in_use = 36
    Delivery_in_progress = 37
    unknown_error_code = 1000
End Enum
