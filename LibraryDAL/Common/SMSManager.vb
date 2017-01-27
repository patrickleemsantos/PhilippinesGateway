Public Class SMSManager


    Public Function CheckTextLength(ByVal strMessage As String) As Int32

        Dim intWord As Int32 = 0
        intWord = strMessage.Length
        If intWord <= 160 Then
            Return 1
        ElseIf intWord > 160 And intWord <= 320 Then
            Return 2
        ElseIf intWord > 320 And intWord <= 480 Then
            Return 3
        ElseIf intWord > 480 And intWord <= 640 Then
            Return 4
        ElseIf intWord > 640 And intWord <= 800 Then
            Return 5
        ElseIf intWord > 800 And intWord <= 960 Then
            Return 6
        End If
    End Function

    Public Function CheckHexLength(ByVal strMessage As String) As Int32
        Dim intWord As Int32 = 0
        intWord = strMessage.Length
        If intWord <= 280 Then
            Return 1
        ElseIf intWord > 280 And intWord <= 560 Then
            Return 2
        ElseIf intWord > 560 And intWord <= 840 Then
            Return 3
        ElseIf intWord > 840 And intWord <= 1120 Then
            Return 4
        ElseIf intWord > 1120 And intWord <= 1400 Then
            Return 5
        End If
    End Function

    Public Shared Function CheckMsgMaxContent(ByVal msgContent As String, ByVal maxCharacterPerMsg As Integer) As Integer
        Dim msgBytesCount As Integer = CInt(System.Text.Encoding.ASCII.GetByteCount(msgContent))

        ' Calculate the required msg to be sent.
        Dim msgCount As Integer = 1
        msgCount = msgCount + (msgBytesCount \ maxCharacterPerMsg)
        CheckMsgMaxContent = msgCount
    End Function

    Public Function SplitTextByLength(ByVal s As String, ByVal len As Integer) As String()

        Dim MsgCount As Integer = CheckMsgMaxContent(s, len)
        Dim MessageArray(MsgCount - 1) As String
        'Dim i As Integer = 0

        'While s.Length > len
        '    MessageArray(i) = s.Substring(0, len)
        '    s.Remove(0, len)
        '    i = +1
        'End While

        'If s.Length > 0 Then
        '    MessageArray(MsgCount - 1) = s
        'End If

        For i As Integer = 0 To MsgCount - 1
            If s.Length > len Then
                MessageArray(i) = s.Substring(0, len)
                s = s.Remove(0, len)
            Else
                MessageArray(i) = s
            End If
        Next

        Return MessageArray
    End Function
End Class
