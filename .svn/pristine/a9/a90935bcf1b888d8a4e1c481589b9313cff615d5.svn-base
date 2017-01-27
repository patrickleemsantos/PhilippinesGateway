Imports LibraryDAL
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security
Imports System.Net

Partial Public Class smsin_stop_all
    Inherits System.Web.UI.Page

    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim message As String = RequestData("message")
        Dim MSISDN As String = RequestData("from")
        Dim TransactionID As String = RequestData("msgid")
        Dim shortcode As String = RequestData("sc")
        Dim serviceID As String = RequestData("serviceid")
        Dim strTelcoID As String = RequestData("telcoid")

        Dim UserCRUD As New UserCRUD
        Dim KeywordCRUD As New KeywordCRUD
        Dim UserStatus As Integer = 0
        Dim Result As Integer = 0
        Dim PostResponse As String = ""
        Dim msgData As String = ""
        Dim URLData As String = ""
        Dim URLPostResult As String = ""

        Dim dt As DataSet

        Dim objInfo As String = "Message=" & message & ";" & _
                                          "From=" & MSISDN & ";" & _
                                          "MsgID=" & TransactionID & ";" & _
                                          "ShortCode=" & shortcode & ";" & _
                                          "ServiceID=" & serviceID & ";" & _
                                          "TelcoID=" & strTelcoID & ";"

        Try

            If serviceID = "" Or shortcode = "" Or strTelcoID = "" Or MSISDN = "" Or message = "" Then
                Throw New Exception("Empty Info")
            End If

            Dim strKeywordList As String = ""
            dt = UserCRUD.GetUserSubsKeyword(MSISDN)

            If dt.Tables(0).Rows.Count > 0 Then
                Dim Count As Integer = 1
                For Each r As DataRow In dt.Tables(0).Rows
                    strKeywordList = strKeywordList & vbCr & r.Item("Keyword") & ","
                    Count = Count + 1
                Next
            End If
       


            dt = KeywordCRUD.GetKeywordURL_byMSISDN(MSISDN) ' get user keyword info
            If dt.Tables(0).Rows.Count > 0 Then

                Result = UserCRUD.UpdateAllUserSubs(MSISDN, General.UserStatus._Inactive)  'inactive all service

                If dt.Tables(0).Rows.Count > 0 Then
                    For Each r As DataRow In dt.Tables(0).Rows

                        If r.Item("MOUrl").ToString <> "" Then
                            msgData = "msgid=" & System.Web.HttpUtility.UrlDecode(TransactionID) & _
                                              "&from=" & MSISDN & _
                                              "&message=" & System.Web.HttpUtility.UrlDecode(message) & _
                                              "&sc=" & shortcode & _
                                               "&telcoid=" & strTelcoID & _
                                               "&serviceid=0"

                            'forward to MO Forward Queue
                            Dim MOForwardStrc As New LibraryDAL.MOForwardStrc
                            With MOForwardStrc
                                .URL = r.Item("MOUrl").ToString
                                .URLData = msgData
                            End With

                            dumpToQueue(MOForwardQueue, MOForwardStrc)
                        End If

                        logger.Info("[STOP ALL]" & objInfo & "; " & PostResponse.Trim)

                    Next

                    If strTelcoID = TelcoID._Smart Then  ' only smart reply the stop all message
                        URLData = "user=" & APIUserName & _
                                        "&pass=" & APIPass & _
                                        "&to=" & MSISDN & _
                                        "&sc=" & shortcode & _
                                        "&type=" & CStr(MsgType.TextSMS) & _
                                        "&msg=" & System.Web.HttpUtility.UrlEncode(StopAllMessage) & _
                                        "&charge=0" & _
                                        "&telcoid=" & strTelcoID & _
                                        "&msgid=" & TransactionID & _
                                        "&serviceid=" & SmartInvalidReplyKW & _
                                        "&other=STOP"

                        URLPostResult = WebURLPost(APIUrl, URLData, TimeOutPostToAPI)
                        logger.Info("[STOP ALL]" & objInfo & ";" & APIUrl & "?" & URLData & ";Result=" & URLPostResult)
                    End If

                    If strTelcoID = TelcoID._Sun Then  ' only Sun reply the stop all message
                        Dim strChkMsg = SunStopAllMessageHeader & vbCr & _
                                               strKeywordList & vbCr & _
                                               SunStopAllMessageBtm


                        URLData = "user=" & SunAPIUserName & _
                                        "&pass=" & SunAPIPass & _
                                        "&to=" & MSISDN & _
                                        "&sc=" & shortcode & _
                                        "&type=" & CStr(MsgType.TextSMS) & _
                                        "&msg=" & System.Web.HttpUtility.UrlEncode(strChkMsg) & _
                                        "&charge=0" & _
                                        "&telcoid=" & strTelcoID & _
                                        "&msgid=" & TransactionID & _
                                        "&serviceid=" & SunInvalidReplyKW & _
                                        "&other=STOP"

                        URLPostResult = WebURLPost(APIUrl, URLData, TimeOutPostToAPI)
                        logger.Info("[STOP ALL]" & objInfo & ";" & APIUrl & "?" & URLData & ";Result=" & URLPostResult)
                    End If

                End If
            Else
                If strTelcoID = TelcoID._Smart Then  ' only smart reply the stop all message
                    URLData = "user=" & APIUserName & _
                                    "&pass=" & APIPass & _
                                    "&to=" & MSISDN & _
                                    "&sc=" & shortcode & _
                                    "&type=" & CStr(MsgType.TextSMS) & _
                                    "&msg=" & System.Web.HttpUtility.UrlEncode(StopAllMessageNotSubs) & _
                                    "&charge=0" & _
                                    "&telcoid=" & strTelcoID & _
                                    "&msgid=" & TransactionID & _
                                    "&serviceid=" & SmartInvalidReplyKW & _
                                    "&other=STOP"

                    URLPostResult = WebURLPost(APIUrl, URLData, TimeOutPostToAPI)
                    logger.Info("[STOP ALL]" & objInfo & ";UserNotFound ;" & APIUrl & "?" & URLData & ";Result=" & URLPostResult)
                Else
                    If strTelcoID = TelcoID._Sun Then  ' only Sun reply the stop all message
                        URLData = "user=" & SunAPIUserName & _
                                        "&pass=" & SunAPIPass & _
                                        "&to=" & MSISDN & _
                                        "&sc=" & shortcode & _
                                        "&type=" & CStr(MsgType.TextSMS) & _
                                        "&msg=" & System.Web.HttpUtility.UrlEncode(SunStopAllMessageNotSubs) & _
                                        "&charge=0" & _
                                        "&telcoid=" & strTelcoID & _
                                        "&msgid=" & TransactionID & _
                                        "&serviceid=" & SunInvalidReplyKW & _
                                        "&other=STOP"

                        URLPostResult = WebURLPost(APIUrl, URLData, TimeOutPostToAPI)
                        logger.Info("[STOP ALL]" & objInfo & ";" & APIUrl & "?" & URLData & ";Result=" & URLPostResult)
                    End If
                    logger.Info("[STOP ALL]" & objInfo & "; User Not Found")
                End If


            End If

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