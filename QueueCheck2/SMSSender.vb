Imports System.Collections.Specialized
Imports System.Text
Imports System.Configuration
Imports System.Web

Public Class SMSSender

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Shared Function deliverpost(ByVal msg As String) As String

        Try
            Dim url_str As String = ConfigurationManager.AppSettings("url_str")
            Dim phone_to As String = ConfigurationManager.AppSettings("phone_to")
            Dim username As String = ConfigurationManager.AppSettings("username")
            Dim password As String = ConfigurationManager.AppSettings("password")

            Dim phones As String() = phone_to.Split(CChar(" "))

            For Each phone As String In phones
                Dim strMsgData As New StringBuilder
                Dim B As Byte() = Nothing
                Dim response As Byte() = Nothing
                Dim strRes As String = ""

                strMsgData.Append("user=")
                strMsgData.Append(username)

                strMsgData.Append("&pass=")
                strMsgData.Append(password)

                strMsgData.Append("&to=")
                strMsgData.Append(phone)

                strMsgData.Append("&msg=")
                strMsgData.Append(HttpUtility.UrlEncode(msg))

                strMsgData.Append("&server=")
                strMsgData.Append("17")

                strMsgData.Append("&type=")
                strMsgData.Append("1")

                Using web As New System.Net.WebClient
                    web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
                    B = System.Text.Encoding.ASCII.GetBytes(strMsgData.ToString())
                    response = web.UploadData(url_str, "POST", B)
                    strRes = "MESSAGE INFO:URL=" & url_str & "?" & strMsgData.ToString & ";RESULT=" & System.Text.Encoding.ASCII.GetString(response) & ";"
                    log.Info(strRes)
                End Using
            Next

            Return "Success"
        Catch ex As Exception
            Return "Fail"
        End Try

    End Function
End Class
