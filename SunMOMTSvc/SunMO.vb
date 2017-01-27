Imports System.Configuration
Imports MySql.Data
Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Messaging
Imports LibraryDAL.General
Imports MySql.Data.MySqlClient

Public Class SunMO

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)
    Private MYSQL_DB_DEFAULT As String = ConfigurationManager.AppSettings("db_default")
    Private MYSQL_CON_STRING As String = ConfigurationManager.AppSettings("mySqlConn")
    Private WELCOME_QUEUE As String = ConfigurationManager.AppSettings("welcome_queue")
    Private TBL_SUBSCRIBER As String = "subscriber"
    Private TBL_INBOX As String = "inbox"
    Private TBL_MT As String = "mt_sun"
    Private con As MySqlConnection = New MySqlConnection(MYSQL_CON_STRING)
    Private cmd As New MySqlCommand
    Private result As Integer
    Private _messageType As String = "1"

    Private _msisdn As String
    Private _message As String
    Private _msgid As String
    Private _shortcode As Integer
    Private _telcoID As Integer
    Private _keywordID As Integer
    Private _firstKeyword As String
    Private _secondKeyword As String
    Private _cpID As Integer
    Private _moURL As String
    Private _charge As Integer
    Private _keyword As String
    Private _keywordStatus As Integer
    Private _keywordType As String
    Private _reserveKeywordID As Integer
    Private _reserveKeywordType As String

    Public Property msisdn() As String
        Get
            Return _msisdn
        End Get
        Set(ByVal value As String)
            _msisdn = value
        End Set
    End Property

    Public Property message() As String
        Get
            Return _message
        End Get
        Set(ByVal value As String)
            _message = value
        End Set
    End Property

    Public Property msgid() As String
        Get
            Return _msgid
        End Get
        Set(ByVal value As String)
            _msgid = value
        End Set
    End Property

    Public Property shortcode() As Integer
        Get
            Return _shortcode
        End Get
        Set(ByVal value As Integer)
            _shortcode = value
        End Set
    End Property

    Public Property telcoID() As Integer
        Get
            Return _telcoID
        End Get
        Set(ByVal value As Integer)
            _telcoID = value
        End Set
    End Property

    Public Property keywordID() As Integer
        Get
            Return _keywordID
        End Get
        Set(ByVal value As Integer)
            _keywordID = value
        End Set
    End Property

    Public Property firstKeyword() As String
        Get
            Return _firstKeyword
        End Get
        Set(ByVal value As String)
            _firstKeyword = value
        End Set
    End Property

    Public Property secondKeyword() As String
        Get
            Return _secondKeyword
        End Get
        Set(ByVal value As String)
            _secondKeyword = value
        End Set
    End Property

    Public Property reserveKeywordType() As String
        Get
            Return _reserveKeywordType
        End Get
        Set(ByVal value As String)
            _reserveKeywordType = value
        End Set
    End Property

    Public Property reserveKeywordID() As Integer
        Get
            Return _reserveKeywordID
        End Get
        Set(ByVal value As Integer)
            _reserveKeywordID = value
        End Set
    End Property

    Public Property keywordType() As String
        Get
            Return _keywordType
        End Get
        Set(ByVal value As String)
            _keywordType = value
        End Set
    End Property

    Public Property keywordStatus() As Integer
        Get
            Return _keywordStatus
        End Get
        Set(ByVal value As Integer)
            _keywordStatus = value
        End Set
    End Property

    Public Property keyword() As String
        Get
            Return _keyword
        End Get
        Set(ByVal value As String)
            _keyword = value
        End Set
    End Property

    Public Property cpID() As Integer
        Get
            Return _cpID
        End Get
        Set(ByVal value As Integer)
            _cpID = value
        End Set
    End Property

    Public Property moURL() As String
        Get
            Return _moURL
        End Get
        Set(ByVal value As String)
            _moURL = value
        End Set
    End Property

    Public Property charge() As Integer
        Get
            Return _charge
        End Get
        Set(ByVal value As Integer)
            _charge = value
        End Set
    End Property

    Public Sub activateSubscriber()
        Dim sql_activate_subscriber As String = "UPDATE " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " SET status = '1', date_joined = now(), transaction_id = '" & _msgid & "', timestamp = now() WHERE msisdn = '" & _msisdn & "' AND keyword_id = '" & _keywordID & "'"
        executeQuery(sql_activate_subscriber)
    End Sub

    Public Sub addSubscriber()
        Dim sql_add_subscriber As String = "INSERT INTO " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " (subscriber_id,msisdn,telco_id,keyword_id,status,transaction_id,date_joined,remark,timestamp) VALUES (0,'" & _msisdn & "','" & _telcoID & "','" & _keywordID & "','1','" & _msgid & "',now(),'',now())"
        executeQuery(sql_add_subscriber)
    End Sub

    Public Sub deactivateSubscriber()
        Dim sql_deactivate_subscriber As String = "UPDATE " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " SET status = '2', date_terminate = now(), timestamp = now() WHERE msisdn = '" & _msisdn & "' AND keyword_id = '" & _keywordID & "'"
        executeQuery(sql_deactivate_subscriber)
    End Sub

    Public Sub deactivateAllSubscription()
        Dim sql_deactivate_subscriber As String = "UPDATE " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " SET status = '2', date_terminate = now(), timestamp = now() WHERE msisdn = '" & _msisdn & "'"
        executeQuery(sql_deactivate_subscriber)
    End Sub

    Public Function getDoubleOptInMsg() As String
        Dim message As String = ""
        Dim sql_get_double_opt_in_message As String = "SELECT double_opt_in_msg FROM " & MYSQL_DB_DEFAULT & ".keyword_reply_message WHERE keyword_id = '" & _keywordID & "' LIMIT 1"
        Dim ds_message As DataSet = getQuery(sql_get_double_opt_in_message, "keyword_reply_message")
        If ds_message.Tables("keyword_reply_message").Rows.Count > 0 Then
            message = ds_message.Tables("keyword_reply_message").Rows(0).Item("double_opt_in_msg").ToString
        End If
        Return message
    End Function

    Public Function getDoubleOptOutMsg() As String
        Dim message As String = ""
        Dim sql_get_double_opt_out_message As String = "SELECT double_opt_out_msg FROM " & MYSQL_DB_DEFAULT & ".keyword_reply_message WHERE keyword_id = '" & _keywordID & "' LIMIT 1"
        Dim ds_message As DataSet = getQuery(sql_get_double_opt_out_message, "keyword_reply_message")
        If ds_message.Tables("keyword_reply_message").Rows.Count > 0 Then
            message = ds_message.Tables("keyword_reply_message").Rows(0).Item("double_opt_out_msg").ToString
        End If
        Return message
    End Function

    Public Function getHelpMsg() As String
        Dim message As String = ""
        Dim sql_get_help_message As String = "SELECT help_msg FROM " & MYSQL_DB_DEFAULT & ".keyword_reply_message WHERE keyword_id = '" & _keywordID & "' LIMIT 1"
        Dim ds_message As DataSet = getQuery(sql_get_help_message, "keyword_reply_message")
        If ds_message.Tables("keyword_reply_message").Rows.Count > 0 Then
            message = ds_message.Tables("keyword_reply_message").Rows(0).Item("help_msg").ToString
        End If
        Return message
    End Function

    Public Function getKeywordCharge() As String
        Dim charge As String = ""
        Dim sql_get_charge As String = "SELECT charge FROM " & MYSQL_DB_DEFAULT & ".keyword WHERE keyword_id = '" & _keywordID & "' LIMIT 1"
        Dim ds_keyword As DataSet = getQuery(sql_get_charge, "keyword")
        If ds_keyword.Tables("keyword").Rows.Count > 0 Then
            charge = ds_keyword.Tables("keyword").Rows(0).Item("charge").ToString
        Else
            charge = "0"
        End If
        Return charge
    End Function

    Public Function getKeywordDetails() As Boolean
        Dim sql_get_keyword_details As String = "SELECT k.cp_id, k.keyword_id, k.mo_url, k.charge, k.keyword_type, k.keyword, k.status, kd.reserve_keyword_id, rk.reserve_keyword_type " & _
                        "FROM " & MYSQL_DB_DEFAULT & ".keyword_detect as kd " & vbCrLf & _
                        "INNER JOIN " & MYSQL_DB_DEFAULT & ".keyword as k ON k.keyword_id = kd.keyword_id " & _
                        "INNER JOIN " & MYSQL_DB_DEFAULT & ".reserve_keyword as rk ON rk.reserve_keyword_id = kd.reserve_keyword_id " & _
                        "WHERE kd.first_keyword = '" & _firstKeyword & "' AND kd.second_keyword = '" & _secondKeyword & "' AND kd.telco_id = '" & _telcoID & "' LIMIT 1"
        Dim ds_keyword_detail As DataSet = getQuery(sql_get_keyword_details, "keyword_detail")
        If ds_keyword_detail.Tables("keyword_detail").Rows.Count > 0 Then
            cpID = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("cp_id").ToString)
            moURL = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("mo_url").ToString
            charge = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("charge").ToString)
            keyword = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("keyword").ToString.ToUpper
            keywordID = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("keyword_id").ToString)
            keywordStatus = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("status").ToString)
            keywordType = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("keyword_type").ToString
            reserveKeywordID = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("reserve_keyword_id").ToString)
            reserveKeywordType = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("reserve_keyword_type").ToString

            logger.Info("[Get keyword details] Keyword ID=" & keywordID & ";Keyword Type=" & keywordType & ";Reserve Keyword ID=" & reserveKeywordID & ";Keyword Status=" & keywordStatus & ";Reserve Keyword Type=" & reserveKeywordType)

            Return True
        Else
            Return False
        End If
        Return False
    End Function

    Public Function getFirstKeywordDetails() As Boolean
        Dim sql_get_keyword_details As String = "SELECT k.cp_id, k.keyword_id, k.mo_url, k.charge, k.keyword_type, k.keyword, k.status, kd.reserve_keyword_id, rk.reserve_keyword_type " & _
                        "FROM " & MYSQL_DB_DEFAULT & ".keyword_detect as kd " & vbCrLf & _
                        "INNER JOIN " & MYSQL_DB_DEFAULT & ".keyword as k ON k.keyword_id = kd.keyword_id " & _
                        "INNER JOIN " & MYSQL_DB_DEFAULT & ".reserve_keyword as rk ON rk.reserve_keyword_id = kd.reserve_keyword_id " & _
                        "WHERE kd.first_keyword = '" & _firstKeyword & "' AND kd.second_keyword = '' AND kd.telco_id = '" & _telcoID & "' LIMIT 1"
        Dim ds_keyword_detail As DataSet = getQuery(sql_get_keyword_details, "keyword_detail")
        If ds_keyword_detail.Tables("keyword_detail").Rows.Count > 0 Then
            cpID = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("cp_id").ToString)
            moURL = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("mo_url").ToString
            charge = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("charge").ToString)
            keyword = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("keyword").ToString.ToUpper
            keywordID = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("keyword_id").ToString)
            keywordStatus = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("status").ToString)
            keywordType = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("keyword_type").ToString
            reserveKeywordID = CInt(ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("reserve_keyword_id").ToString)
            reserveKeywordType = ds_keyword_detail.Tables("keyword_detail").Rows(0).Item("reserve_keyword_type").ToString

            logger.Info("[Get first keyword details] Keyword ID=" & keywordID & ";Keyword Type=" & keywordType & ";Reserve Keyword ID=" & reserveKeywordID & ";Keyword Status=" & keywordStatus & ";Reserve Keyword Type=" & reserveKeywordType)

            Return True
        Else
            Return False
        End If
        Return False
    End Function

    Public Function getOptInMsg() As String
        Dim message As String = ""
        Dim sql_get_opt_in_message As String = "SELECT opt_in_msg FROM " & MYSQL_DB_DEFAULT & ".keyword_reply_message WHERE keyword_id = '" & _keywordID & "' LIMIT 1"
        Dim ds_message As DataSet = getQuery(sql_get_opt_in_message, "keyword_reply_message")
        If ds_message.Tables("keyword_reply_message").Rows.Count > 0 Then
            message = ds_message.Tables("keyword_reply_message").Rows(0).Item("opt_in_msg").ToString
        End If
        Return message
    End Function

    Public Function getOptOutMsg() As String
        Dim message As String = ""
        Dim sql_get_opt_out_message As String = "SELECT opt_out_msg FROM " & MYSQL_DB_DEFAULT & ".keyword_reply_message WHERE keyword_id = '" & _keywordID & "' LIMIT 1"
        Dim ds_message As DataSet = getQuery(sql_get_opt_out_message, "keyword_reply_message")
        If ds_message.Tables("keyword_reply_message").Rows.Count > 0 Then
            message = ds_message.Tables("keyword_reply_message").Rows(0).Item("opt_out_msg").ToString
        End If
        Return message
    End Function

    Public Sub insertInbox()
        Dim sql_insert_inbox As String = "INSERT INTO " & MYSQL_DB_DEFAULT & "." & TBL_INBOX & " (inbox_id,keyword_id,msisdn,message,transaction_id,telco_id,date_received,timestamp) VALUES (0,'" & _keywordID & "','" & _msisdn & "','" & _message & "','" & _msgid & "','" & _telcoID & "',now(),now())"
        executeQuery(sql_insert_inbox)
    End Sub

    Public Function isSubscriberActive() As Boolean
        Dim sql_check_sub As String = "SELECT status FROM " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " WHERE msisdn = '" & _msisdn & "' AND keyword_id = '" & _keywordID & "' and status = '1'"
        Dim ds_status As DataSet = getQuery(sql_check_sub, "subscriber")
        If ds_status.Tables("subscriber").Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function isSubscriberWhiteListed() As Boolean
        Dim sql_check_sub As String = "SELECT * FROM " & MYSQL_DB_DEFAULT & ".whitelist WHERE msisdn = '" & _msisdn & "' AND keyword_id = '" & _keywordID & "' AND telco_id = '" & _telcoID & "' LIMIT 1"
        Dim ds_status As DataSet = getQuery(sql_check_sub, "whitelist")
        If ds_status.Tables("whitelist").Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function isSubscriptionExist() As Boolean
        Dim sql_check_sub As String = "SELECT * FROM " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " WHERE msisdn = '" & _msisdn & "' AND keyword_id = '" & _keywordID & "'"
        Dim ds_status As DataSet = getQuery(sql_check_sub, "subscriber")
        If ds_status.Tables("subscriber").Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function isSubGotSubscription() As Boolean
        Dim sql_check_sub As String = "SELECT * FROM " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " WHERE msisdn = '" & _msisdn & "' AND status = '1'"
        Dim ds_status As DataSet = getQuery(sql_check_sub, "subscriber")
        If ds_status.Tables("subscriber").Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub sendCheckMessage()
        Dim keywordList As String = ""
        Dim checkMessage As String = ""
        Dim sql_get_sub_services As String = "SELECT k.keyword as keyword FROM " & MYSQL_DB_DEFAULT & ".subscriber as s INNER JOIN keyword as k ON k.keyword_id = s.keyword_id  WHERE s.msisdn = '" & _msisdn & "' AND s.status ='1'"
        Dim ds_services As DataSet = getQuery(sql_get_sub_services, "services")
        If ds_services.Tables("services").Rows.Count > 0 Then
            For Each r As DataRow In ds_services.Tables(0).Rows
                keywordList = keywordList + r.Item("keyword") + ","
            Next
            keywordList = keywordList.Substring(0, keywordList.Length - 1)
            'keywordList = keywordList.Trim().Remove(keywordList.Length - 1)
            checkMessage = "Ur subscribed from d ff 2488 svcs: " & keywordList & ". To unsubscribe,txt <KEYWORD> OFF to 2488,free.To remove all svcs for free,txt STOP ALL to 2488.This msg s free."
        Else
            checkMessage = "You are not subscribed to any 2488 service. This message is free."
        End If
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        With strcSendSMSApi
            .Charge = "0"
            .CPID = _cpID
            .Message = checkMessage
            .TransactionID = _msgid
            .URL = ""
            .TelcoID = _telcoID
            .MsgType = _messageType
            .MSISDN = _msisdn
            .Queue = WELCOME_QUEUE
            .ReceiveDate = Date.Now
            .RetryCount = 0
            .MsgGUID = ""
            .ShortCode = _shortcode
            .KeywordID = _keywordID
            .KeywordType = keywordType
            .MsgCount = SMSMessageCount(checkMessage, _messageType)
            .Other = ""
        End With

        SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
    End Sub

    Public Sub sendDoubleOptInMessage()
        Dim doulbleOptInMsg As String = getDoubleOptInMsg()
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        With strcSendSMSApi
            .Charge = "0"
            .CPID = _cpID
            .Message = doulbleOptInMsg
            .TransactionID = _msgid
            .URL = ""
            .TelcoID = _telcoID
            .MsgType = _messageType
            .MSISDN = _msisdn
            .Queue = WELCOME_QUEUE
            .ReceiveDate = Date.Now
            .RetryCount = 0
            .MsgGUID = ""
            .ShortCode = _shortcode
            .KeywordID = _keywordID
            .KeywordType = keywordType
            .MsgCount = SMSMessageCount(doulbleOptInMsg, _messageType)
            .Other = ""
        End With

        SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
    End Sub

    Public Sub sendDoubleOptOutMessage()
        Dim doulbleOptOutMsg As String = getDoubleOptOutMsg()
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        With strcSendSMSApi
            .Charge = "0"
            .CPID = _cpID
            .Message = doulbleOptOutMsg
            .TransactionID = _msgid
            .URL = ""
            .TelcoID = _telcoID
            .MsgType = _messageType
            .MSISDN = _msisdn
            .Queue = WELCOME_QUEUE
            .ReceiveDate = Date.Now
            .RetryCount = 0
            .MsgGUID = ""
            .ShortCode = _shortcode
            .KeywordID = _keywordID
            .KeywordType = keywordType
            .MsgCount = SMSMessageCount(doulbleOptOutMsg, _messageType)
            .Other = ""
        End With

        SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
    End Sub

    Public Sub sendHelpMessage()
        Dim helpMessage As String = getHelpMsg()
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        With strcSendSMSApi
            .Charge = "0"
            .CPID = _cpID
            .Message = helpMessage
            .TransactionID = _msgid
            .URL = ""
            .TelcoID = _telcoID
            .MsgType = _messageType
            .MSISDN = _msisdn
            .Queue = WELCOME_QUEUE
            .ReceiveDate = Date.Now
            .RetryCount = 0
            .MsgGUID = ""
            .ShortCode = _shortcode
            .KeywordID = _keywordID
            .KeywordType = keywordType
            .MsgCount = SMSMessageCount(helpMessage, _messageType)
            .Other = ""
        End With

        SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
    End Sub

    Public Sub sendInvalidMessage()
        Dim invalidMessage As String = "You have sent an invalid keyword. You have been charged P2.00 for this message. Help? Call 3530170 M-F."
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        With strcSendSMSApi
            .Charge = "200"
            .CPID = _cpID
            .Message = invalidMessage
            .TransactionID = _msgid
            .URL = ""
            .TelcoID = _telcoID
            .MsgType = _messageType
            .MSISDN = _msisdn
            .Queue = WELCOME_QUEUE
            .ReceiveDate = Date.Now
            .RetryCount = 0
            .MsgGUID = ""
            .ShortCode = _shortcode
            .KeywordID = _keywordID
            .KeywordType = keywordType
            .MsgCount = SMSMessageCount(invalidMessage, _messageType)
            .Other = ""
        End With

        SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
    End Sub

    Public Sub sendOptInMessage()
        Dim optInMsg As String = getOptInMsg()
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        If strcSendSMSApi.KeywordID <> "305" Then
            With strcSendSMSApi
                '.Charge = getKeywordCharge()
                .Charge = "0"
                .CPID = _cpID
                .Message = optInMsg
                .TransactionID = _msgid
                .URL = ""
                .TelcoID = _telcoID
                .MsgType = _messageType
                .MSISDN = _msisdn
                .Queue = WELCOME_QUEUE
                .ReceiveDate = Date.Now
                .RetryCount = 0
                .MsgGUID = ""
                .ShortCode = _shortcode
                .KeywordID = _keywordID
                .KeywordType = keywordType
                .MsgCount = SMSMessageCount(optInMsg, _messageType)
                .Other = ""
            End With

            SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
        End If
    End Sub

    Public Sub sendOptOutMessage()
        Dim optOutMsg As String = getOptOutMsg()
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        With strcSendSMSApi
            .Charge = "0"
            .CPID = _cpID
            .Message = optOutMsg
            .TransactionID = _msgid
            .URL = ""
            .TelcoID = _telcoID
            .MsgType = _messageType
            .MSISDN = _msisdn
            .Queue = WELCOME_QUEUE
            .ReceiveDate = Date.Now
            .RetryCount = 0
            .MsgGUID = ""
            .ShortCode = _shortcode
            .KeywordID = _keywordID
            .KeywordType = keywordType
            .MsgCount = SMSMessageCount(optOutMsg, _messageType)
            .Other = ""
        End With

        SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
    End Sub

    Public Sub sendStopAllMessage()
        Dim keywordList As String = ""
        Dim stopAllMessage As String = ""
        Dim sql_get_sub_services As String = "SELECT k.keyword as keyword FROM " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " as s INNER JOIN keyword as k ON k.keyword_id = s.keyword_id  WHERE s.msisdn = '" & _msisdn & "' AND s.status ='1'"
        Dim ds_services As DataSet = getQuery(sql_get_sub_services, "services")
        If ds_services.Tables("services").Rows.Count > 0 Then
            For Each r As DataRow In ds_services.Tables(0).Rows
                keywordList = keywordList + r.Item("keyword") + ","
            Next
            keywordList = keywordList.Substring(0, keywordList.Length - 1)
            'keywordList = keywordList.Trim().Remove(keywordList.Length - 1)
            stopAllMessage = "COSMIC: You have been unsubscribed from the following 2488 services: " & keywordList & ". This message is free."
        Else
            stopAllMessage = "COSMIC: You are not subscribed to any 2488 service.This message is free."
        End If
        Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
        With strcSendSMSApi
            .Charge = "0"
            .CPID = _cpID
            .Message = stopAllMessage
            .TransactionID = _msgid
            .URL = ""
            .TelcoID = _telcoID
            .MsgType = _messageType
            .MSISDN = _msisdn
            .Queue = WELCOME_QUEUE
            .ReceiveDate = Date.Now
            .RetryCount = 0
            .MsgGUID = ""
            .ShortCode = _shortcode
            .KeywordID = _keywordID
            .KeywordType = keywordType
            .MsgCount = SMSMessageCount(stopAllMessage, _messageType)
            .Other = ""
        End With

        SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)
    End Sub

    Public Sub postMOToCP()
        Dim parameters As String = "from=" & System.Web.HttpUtility.UrlEncode(_msisdn) & "&msgid=" & _msgid & "&message=" & System.Web.HttpUtility.UrlEncode(_message) & "&sc=" & _shortcode & "&telcoid=" & _telcoID & "&serviceid=" & _keywordID
        Try
            Dim request As WebRequest = WebRequest.Create(_moURL)
            request.Method = "POST"
            Dim postData As String = parameters
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            dataStream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()
            reader.Close()
            dataStream.Close()
            response.Close()
            logger.Info("[FORWARD MO] RESPONSE=" & responseFromServer & ";" & _moURL & "?" & parameters)
        Catch ex As Exception
            logger.Info("[FORWARD MO ERROR] " & _moURL & "?" & parameters, ex)
        End Try
    End Sub

    Public Sub postStopAllMOToCP()
        Dim kMOURL As String = ""
        Dim kKeywordID As Integer
        Dim kShortcode As String
        Dim kTelcoID As Integer
        Dim parameters As String = ""
        Dim sql_get_mo_urls = "SELECT k.mo_url,k.keyword_id,k.shortcode,k.telco_id FROM " & MYSQL_DB_DEFAULT & "." & TBL_SUBSCRIBER & " AS s INNER JOIN keyword AS k ON k.keyword_id = s.keyword_id WHERE s.msisdn = '" & _msisdn & "' GROUP BY k.cp_id"
        Dim ds_urls As DataSet = getQuery(sql_get_mo_urls, "urls")
        If ds_urls.Tables("urls").Rows.Count > 0 Then
            For Each r As DataRow In ds_urls.Tables(0).Rows
                kMOURL = r.Item("mo_url")
                kKeywordID = r.Item("keyword_id")
                kShortcode = r.Item("shortcode")
                kTelcoID = r.Item("telco_id")
                parameters = "from=" & System.Web.HttpUtility.UrlEncode(_msisdn) & "&msgid=" & _msgid & "&message=" & System.Web.HttpUtility.UrlEncode("STOP ALL") & "&sc=" & kShortcode & "&telcoid=" & kTelcoID & "&serviceid=" & kKeywordID
                Try
                    Dim request As WebRequest = WebRequest.Create(kMOURL)
                    request.Method = "POST"
                    Dim postData As String = parameters
                    Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
                    request.ContentType = "application/x-www-form-urlencoded"
                    request.ContentLength = byteArray.Length
                    Dim dataStream As Stream = request.GetRequestStream()
                    dataStream.Write(byteArray, 0, byteArray.Length)
                    dataStream.Close()
                    Dim response As WebResponse = request.GetResponse()
                    dataStream = response.GetResponseStream()
                    Dim reader As New StreamReader(dataStream)
                    Dim responseFromServer As String = reader.ReadToEnd()
                    reader.Close()
                    dataStream.Close()
                    response.Close()
                    logger.Info("[FORWARD STOP ALL MO] " & kMOURL & "?" & parameters)
                Catch ex As Exception
                    logger.Info("[FORWARD STOP ALL MO] " & kMOURL & "?" & parameters, ex)
                End Try
            Next
        End If
    End Sub

    Public Sub executeQuery(ByVal sql As String)
        Try
            con.Open()
            cmd.Connection = con
            cmd.CommandText = sql
            result = cmd.ExecuteNonQuery
            con.Close()
            logger.Info("[SUN MO EXECUTE QUERY]: " & result & "|" & sql)
        Catch ex As Exception
            logger.Info("[SUN MO SQL EXCEPTION]: " & ex.Message & "|" & sql)
            con.Close()
        End Try
    End Sub

    Private Function getQuery(ByVal sql As String, ByVal tbl_db As String) As DataSet
        Dim ds As New DataSet
        Try
            con.Open()
            Using da As New MySqlDataAdapter
                da.SelectCommand = New MySqlCommand(sql, con)
                da.SelectCommand.CommandTimeout = 21600
                da.Fill(ds, tbl_db)
                da.SelectCommand.Connection.Close()
            End Using
            con.Close()
            logger.Info("[SUN MO GET QUERY]: " & sql)
        Catch ex As Exception
            logger.Info("[SUN MO SQL EXCEPTION]: " & ex.Message & "|" & sql)
            con.Close()
            ds.Dispose()
        End Try
        Return ds
    End Function

    Private Sub httpPost(ByVal url As String, ByVal parameters As String)

        Try
            Dim request As WebRequest = WebRequest.Create(url)
            request.Method = "POST"
            Dim postData As String = parameters
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = byteArray.Length
            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()
            Dim response As WebResponse = request.GetResponse()
            dataStream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()
            reader.Close()
            dataStream.Close()
            response.Close()
            logger.Info("[SUN MO POST TO 205] " & url & "?" & parameters)
        Catch ex As Exception
            logger.Info("[SUN MO FAILED POST TO 205] " & url & "?" & parameters, ex)
        End Try

    End Sub

    Private Sub SendToQueue(ByVal destination As String, ByVal obj As LibraryDAL.SendSMSAPIStrc)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

End Class
