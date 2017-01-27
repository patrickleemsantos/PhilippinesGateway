Partial Public Class _Default
    Inherits System.Web.UI.Page


    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Dim message As String = RequestData("message")
            Dim MSISDN As String = RequestData("from")
            Dim TransactionID As String = RequestData("msgid")
            Dim shortcode As String = RequestData("sc")
            Dim serviceID As String = RequestData("serviceid")
            Dim telcoID As String = RequestData("telcoid")

            logger.Info("[MO]message=" & message & ";from" & MSISDN & ";msgid=" & TransactionID & ";sc=" & shortcode & ";serviceid=" & serviceID & ";telcoid=" & telcoID)

            Dim BodyMessage As String

            If message.ToUpper = "PNH ON" Then
                BodyMessage = System.Web.HttpUtility.UrlEncode("Thank you for subscribing to Daily News Headline.ph2.50")
            ElseIf message.ToUpper = "PNH STOP" Then
                BodyMessage = System.Web.HttpUtility.UrlEncode("You have unsubscribed from Daily News Headlin. TQ for your support. To join again, send PNH ON to 2610.ph0.00")
            End If

            Dim PostData As String = "user=Test&pass=Test1234&to=" & MSISDN & "&sc=" & shortcode & "&serviceid=" & serviceID & "&type=1&msg=" & BodyMessage & "&charge=250&telcoid=" & telcoID & "&msgid=" & TransactionID
            Dim PostUrl As String = "http://115.85.17.59:8001/sendsms.aspx"




            Dim MTResponse As String = WebRequest(PostUrl, PostData)

            Dim MTPush As String = PostUrl & "?" & PostData & ";Response=" & MTResponse

            logger.Info("[MT]" & MTPush)
        Catch ex As Exception
            logger.Fatal("FATAL", ex)
        End Try

    End Sub


    Public Function WebRequest(ByVal URL As String, ByVal PostData As String) As String
        Try
            Dim encoding As New System.Text.ASCIIEncoding
            Dim B As Byte() = Nothing
            Dim response As Byte() = Nothing
            Dim strRes As String = ""
        
            Using web As New System.Net.WebClient
                Dim postURL = URL & "?" & PostData.ToString()
                web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
                B = System.Text.Encoding.ASCII.GetBytes(PostData.ToString())
                ' logger.Info("[URL called]-" & postURL)
                response = web.UploadData(URL, "POST", B)
                strRes = System.Text.Encoding.ASCII.GetString(response)
                Return strRes
            End Using
        Catch ex As Exception
            logger.Fatal("URL=" & URL & "?" & PostData)
            logger.Fatal("[FATAL]", ex)
            Return "Fail"
        End Try
    End Function


    Public Function RequestData(ByVal strName As String) As String
        Dim strReturn As String = ""
        Try

            strReturn = Request.QueryString(strName)
            If strReturn = "" Then
                strReturn = System.Web.HttpUtility.UrlDecode(Request.Form(strName))
            End If
            If strReturn <> "" Then
                strReturn = strReturn.Trim
                Return strReturn
            Else
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try
    End Function

End Class