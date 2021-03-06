﻿Imports LibraryDAL
Imports System.Web.Configuration
Imports LibraryDAL.Extensions

Partial Public Class smsin_help
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

            If serviceID = "" Or shortcode = "" Or strTelcoID = "" Or MSISDN = "" Or message = "" Then
                Throw New Exception("Empty Info")
            End If

            Dim URLData As String = ""
            Dim Result As String = ""
            Dim PostResult As String = ""
            Dim msgData As String = ""
            Dim CPUserName As String = ""
            Dim CPPassword As String = ""
            Dim HelpKwMessage As String = ""
            Dim CPKeywordID As String = ""
            Dim CPMsgType As String = ""
            Dim Charge As String = ""

            Dim KeywordCRUD As New KeywordCRUD
            Dim KeywordInfo As Tbl_Keyword
            Dim KwSetMsgCRUD As New KeywordMsgSettingCRUD



            If strTelcoID = TelcoID._Smart Then  ' only smart reply the help message

                KeywordInfo = KeywordCRUD.GetKeywordInfo_byKeywordID(serviceID)

                'If KeywordInfo.TypeID = KeywordType._IOD Then
                If KeywordInfo.TypeID = KeywordType._IOD Or KeywordInfo.TypeID = KeywordType._ContentBasedSubs Or KeywordInfo.TypeID = KeywordType._TimeBasedSubs Or KeywordInfo.TypeID = KeywordType._Conditional Then

                    Dim MessageList() As String = Split(message, " ").RemoveGap

                    If MessageList.Count > 1 Then ' if they is help for some keyword like HELP CNEWS, need to determine the keyword message
                        'get help keyword message
                        Dim dt As DataSet = KwSetMsgCRUD.GetHelpMsgByKeywordShortCode(MessageList(1), shortcode)
                        If dt.Tables(0).Rows.Count > 0 Then ' if able to find the keyword 

                            For Each r As DataRow In dt.Tables(0).Rows
                                CPUserName = r.Item("UserName")
                                CPPassword = r.Item("Password")
                                HelpKwMessage = r.Item("HelpMsg")
                                CPKeywordID = r.Item("keywordid")
                                CPMsgType = r.Item("MsgType")
                                Charge = r.Item("charge")
                            Next

                            'post to api
                            PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, CPUserName, CPPassword, MSISDN, shortcode, CPMsgType, _
                                HelpKwMessage.Replace("#Keyword", KeywordInfo.Keyword), Charge, strTelcoID, SmartHelpReplyKW, "help", TransactionID)

                            logger.Info("[HELP]Keyword;" & objInfo & ";" & PostResult)
                        Else ' invalid send normal help sms

                            'Added by Patrick 09/16/2013
                            'To get help message from database
                            Dim dtHelpMsg As DataSet = KwSetMsgCRUD.GetHelpMsgByKeywordShortCode(MessageList(0), shortcode)
                            If dtHelpMsg.Tables(0).Rows.Count Then
                                For Each r As DataRow In dtHelpMsg.Tables(0).Rows
                                    HelpKwMessage = r.Item("HelpMsg")
                                Next
                            End If
                            '''''''''''''''''''''''''''''''End

                            PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, APIUserName, APIPass, MSISDN, shortcode, CStr(MsgType.TextSMS), _
                                        HelpKwMessage.Replace("#Keyword", KeywordInfo.Keyword), "250", strTelcoID, SmartHelpReplyKW, "help", TransactionID)
                            'commented by Patrick 09/17/2013
                            'PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, APIUserName, APIPass, MSISDN, shortcode, CStr(MsgType.TextSMS), _
                            '            HelpKwMessage.Replace("#Keyword", KeywordInfo.Keyword), "250", strTelcoID, SmartInvalidReplyKW, "help", TransactionID)

                            logger.Info("[HELP]General;" & objInfo & ";" & PostResult)
                        End If

                    Else ' send general help keyword message if it just send in help keyword

                        'Added by Patrick 09/16/2013
                        'To get help message from database
                        Dim dtHelpMsg As DataSet = KwSetMsgCRUD.GetHelpMsgByKeywordShortCode(MessageList(0), shortcode)
                        If dtHelpMsg.Tables(0).Rows.Count Then
                            For Each r As DataRow In dtHelpMsg.Tables(0).Rows
                                HelpKwMessage = r.Item("HelpMsg")
                            Next
                        End If

                        'PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, APIUserName, APIPass, MSISDN, shortcode, CStr(MsgType.TextSMS), _
                        '                     HelpMessage.Replace("#Keyword", KeywordInfo.Keyword), "250", strTelcoID, SmartInvalidReplyKW, "help", TransactionID)
                        PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, APIUserName, APIPass, MSISDN, shortcode, CStr(MsgType.TextSMS), _
                                            HelpKwMessage.Replace("#Keyword", KeywordInfo.Keyword), "250", strTelcoID, SmartHelpReplyKW, "help", TransactionID)

                        logger.Info("[HELP]General;" & objInfo & ";" & PostResult)
                    End If

                End If

            End If


            If strTelcoID = TelcoID._Sun Then  ' only Sun reply the help message

                KeywordInfo = KeywordCRUD.GetKeywordInfo_byKeywordID(serviceID)
                Dim MessageList() As String = Split(message, " ").RemoveGap

                If MessageList.Count > 1 Then ' if they is help for some keyword like HELP CNEWS, need to determine the keyword message
                    'get help keyword message
                    Dim dt As DataSet = KwSetMsgCRUD.GetHelpMsgByKeywordShortCode(MessageList(0), shortcode)
                    If dt.Tables(0).Rows.Count > 0 Then ' if able to find the keyword 

                        For Each r As DataRow In dt.Tables(0).Rows
                            CPUserName = r.Item("UserName")
                            CPPassword = r.Item("Password")
                            HelpKwMessage = r.Item("HelpMsg")
                            CPKeywordID = r.Item("keywordid")
                            CPMsgType = r.Item("MsgType")
                            Charge = r.Item("charge")
                        Next

                        'post to api
                        PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, CPUserName, CPPassword, MSISDN, shortcode, CPMsgType, _
                        HelpKwMessage, "200", strTelcoID, "108", "help", TransactionID)

                        logger.Info("[HELP SUN]Keyword;" & objInfo & ";" & PostResult)
                    Else ' invalid send normal help sms

                        PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, SunAPIUserName, SunAPIPass, MSISDN, shortcode, CStr(MsgType.TextSMS), _
                                   HelpMessage, "200", strTelcoID, "108", "help", TransactionID)

                        logger.Info("[HELP]General;" & objInfo & ";" & PostResult)
                    End If

                Else ' send general help keyword message if it just send in help keyword
                    PostResult = SendToAPI(APIUrl, TimeOutPostToAPI, SunAPIUserName, SunAPIPass, MSISDN, shortcode, CStr(MsgType.TextSMS), _
                                        HelpMessage, "200", strTelcoID, "108", "help", TransactionID)

                    logger.Info("[HELP]General;" & objInfo & ";" & PostResult)
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