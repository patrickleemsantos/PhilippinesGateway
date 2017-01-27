Imports System.Text
Imports LibraryDAL
Imports System.Messaging

Module Module1

    Sub Main()

        Try

            Dim MT As New SendSMSAPIStrc

            With MT
                .MSISDN = "1234567890"
                .Charge = 250
                .CPID = 1
                .KeywordID = 6
                .KeywordType = 1
                .Message = "testing123456"
                .MsgCount = 1
                .MsgGUID = "fjasfjijijfkjdklajfoaifa"
                .ReceiveDate = DateTime.Now
                .RetryCount = 0
                .ShortCode = "2910"
                .TelcoID = 3
                .TransactionID = "abc2134"
                .MsgType = 1
            End With

            dumpToQueue(".\Private$\smppmt", MT)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try







        ''http://64.49.216.30/services/egg/apps/listener.jsp?number=09052911412&rrn=3167948532073461&message=test1234567&telco=GLOBE&tariff=2.50'
        ''http://64.49.216.30/services/egg/apps/listener.jsp?number=09052911412&rrn=4494249257687835&message=test20111128&telco=GLOBE&tariff=2.50
        'Dim B As Byte() = Nothing
        'Dim response As Byte() = Nothing
        'Dim strRes As String = ""
        'Dim strMsgData As New StringBuilder
        'strMsgData.Append("number=")
        'strMsgData.Append("09052911412")
        'strMsgData.Append("&rrn=")
        'strMsgData.Append("4494249257687835")
        'strMsgData.Append("&message=")
        'strMsgData.Append("test 28/11/2011")
        'strMsgData.Append("&telco=")
        'strMsgData.Append("GLOBE")
        'strMsgData.Append("&tariff=")
        'strMsgData.Append("2.50")
        'Dim url = "http://64.49.216.30/services/egg/apps/listener.jsp?" & strMsgData.ToString()
        'Try
        '    Using web As New System.Net.WebClient
        '        web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
        '        ' web.Headers.Add("Content-Type", " ")
        '        B = System.Text.Encoding.ASCII.GetBytes(strMsgData.ToString())
        '        ' response = web.UploadData("http://64.49.216.30/services/egg/apps/listener.jsp", "POST", B)
        '        response = web.DownloadData(url)

        '        '   response = web.UploadValues(url_str_premium_umobile, MKCollection)
        '        strRes = System.Text.Encoding.ASCII.GetString(response)

        '        ' strRes = "[SEND TO API]:URL=" & url_str_premium_umobile & "?" & strMsgData.ToString & ";RESULT=" & System.Text.Encoding.ASCII.GetString(response) & ";"
        '    End Using



        'Catch ex As Exception


        'End Try
    End Sub


    Private Sub dumpToQueue(ByVal destination As String, ByVal obj As Object)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
                q.Close()
            End Using
        End Using
    End Sub



End Module
