Imports LibraryDAL
Imports System.Web.Configuration

Partial Public Class smsin_junk
    Inherits System.Web.UI.Page

    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim message As String = RequestData("message")
        Dim MSISDN As String = RequestData("from")
        Dim TransactionID As String = RequestData("msgid")
        Dim shortcode As String = RequestData("sc")
        Dim serviceID As String = RequestData("serviceid")
        Dim strTelcoID As String = RequestData("telcoid")


        Dim objInfo As String = "Message=" & message & ";" & _
                                          "From=" & MSISDN & ";" & _
                                          "MsgID=" & TransactionID & ";" & _
                                          "ShortCode=" & shortcode & ";" & _
                                          "ServiceID=" & serviceID & ";" & _
                                          "TelcoID=" & strTelcoID & ";"

        Try

            If serviceID = "" Or shortcode = "" Or strTelcoID = "" Or MSISDN = "" Then
                Throw New Exception("Empty Info")
            End If

            Dim URLData As String = ""
            Dim Result As String = ""


            If strTelcoID = TelcoID._Smart Then  ' only smart reply the invalid keyword message 
                URLData = "user=" & APIUserName & _
                                "&pass=" & APIPass & _
                                "&to=" & MSISDN & _
                                "&sc=" & shortcode & _
                                "&type=" & CStr(MsgType.TextSMS) & _
                                "&msg=" & System.Web.HttpUtility.UrlEncode(InvalidKeywordMessage) & _
                                "&charge=250" & _
                                "&telcoid=" & strTelcoID & _
                                "&msgid=" & TransactionID & _
                                "&serviceid=" & SmartInvalidReplyKW & _
                                "&other=help"

                Result = WebURLPost(APIUrl, URLData, TimeOutPostToAPI)
            End If

            If strTelcoID = TelcoID._Sun Then  ' only smart reply the invalid keyword message 
                URLData = "user=" & SunAPIUserName & _
                                "&pass=" & SunAPIPass & _
                                "&to=" & MSISDN & _
                                "&sc=" & shortcode & _
                                "&type=" & CStr(MsgType.TextSMS) & _
                                "&msg=" & System.Web.HttpUtility.UrlEncode(SunInvalidKeywordMessage) & _
                                "&charge=200" & _
                                "&telcoid=" & strTelcoID & _
                                "&msgid=" & TransactionID & _
                                "&serviceid=" & SunInvalidReplyKW & _
                                "&other=help"

                Result = WebURLPost(APIUrl, URLData, TimeOutPostToAPI)
            End If



            logger.Info("[JUNK]" & objInfo & ";" & APIUrl & "?" & URLData & ";Result=" & Result)

        Catch ex As Exception
            logger.Fatal("[FATAL]" & objInfo)
            logger.Fatal("[FATAL]", ex)
        End Try

    End Sub

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