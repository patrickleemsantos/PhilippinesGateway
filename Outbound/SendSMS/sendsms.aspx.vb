Imports System.Messaging
Imports LibraryDAL
Imports LibraryDAL.General
Imports MySql.Data.MySqlClient

Partial Public Class sendsms
    Inherits System.Web.UI.Page

    Private strUserName, strPassword, strMSISDN, strShortCode, strKeywordID, strType, strMessage, strCharge, strUrl, strTelcoID, _
                strMessageMOID, strInfo, strIPAddress, strQueue, strMsgGUID, strOther As String

    Private CPID, KeywordType As Integer
    Private keyword As String

    Private ReceiveDate As DateTime

    Private pre_footer_Nor_Msg As String = System.Web.Configuration.WebConfigurationManager.AppSettings("NormalFooterMsg")
    Private pre_footer_Fri_Msg As String = System.Web.Configuration.WebConfigurationManager.AppSettings("FriFooterMsg")
    Private mySqlConStr As String = System.Web.Configuration.WebConfigurationManager.AppSettings("mySqlConn")

    Private EGG_EWKeywordID As Integer = CInt(System.Web.Configuration.WebConfigurationManager.AppSettings("EGG_EW_KewordID"))
    Private EGGTelcoID As Integer = CInt(System.Web.Configuration.WebConfigurationManager.AppSettings("EGG_TelcoID"))
    Private EGG_EW_MTQueue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("EGG_EW_MTQueue")

    Private SUN_QUEUE As String = System.Web.Configuration.WebConfigurationManager.AppSettings("sun_queue")
    Private SUN_QUEUE_0 As String = System.Web.Configuration.WebConfigurationManager.AppSettings("sun_queue_0")
    Private SUN_QUEUE_1 As String = System.Web.Configuration.WebConfigurationManager.AppSettings("sun_queue_1")
    Private SUN_QUEUE_2 As String = System.Web.Configuration.WebConfigurationManager.AppSettings("sun_queue_2")
    Private SUN_QUEUE_3 As String = System.Web.Configuration.WebConfigurationManager.AppSettings("sun_queue_3")
    Private SUN_QUEUE_DEATH As String = System.Web.Configuration.WebConfigurationManager.AppSettings("sun_queue_death")

    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        strUserName = RequestData("user")
        strPassword = RequestData("pass")
        strMSISDN = RequestData("to")
        strShortCode = RequestData("sc")
        strKeywordID = RequestData("serviceid")
        strType = RequestData("type")
        strMessage = RequestData("msg")
        strCharge = Trim(RequestData("charge"))
        strUrl = RequestData("url")
        strTelcoID = RequestData("telcoid")
        strMessageMOID = RequestData("msgid")
        strMsgGUID = RequestData("msgguid")
        strOther = RequestData("other")
        ReceiveDate = Date.Now
        strIPAddress = Request.ServerVariables("REMOTE_ADDR")

        strInfo = strIPAddress & "; " & _
                     "user=" & strUserName & _
                     "&pass=" & strPassword & _
                     "&to=" & strMSISDN & _
                     "&sc=" & strShortCode & _
                     "&serviceid=" & strKeywordID & _
                     "&type=" & strType & _
                     "&msg=" & strMessage & _
                     "&charge=" & strCharge & _
                     "&url=" & strUrl & _
                     "&telcoid=" & strTelcoID & _
                     "&msgid=" & strMessageMOID & _
                     "&other=" & strOther
        Try
            ''checking for empty infor or wrong info
            'If strUserName = "" Or strPassword = "" Or strMSISDN = "" Or strShortCode = "" Or strMessage = "" Or _
            '(strKeywordID = "" AndAlso Not IsNumeric(strKeywordID)) Or _
            '(strType = "" AndAlso Not IsNumeric(strType)) Or _
            '(strCharge = "" AndAlso Not IsNumeric(strCharge)) Or _
            '(strTelcoID = "" AndAlso Not IsNumeric(strTelcoID)) Then
            '    logger.Error("[ERROR]" & strInfo & "; " & MissingParameterField & ",")
            '    Response.Write(MissingParameterField & ",")
            '    Exit Sub
            'End If

            ''check data in databases store procedure will perform all the databases checking
            'Dim OtherCURD As New OtherCRUD

            'Dim Result As String = OtherCURD.CheckSendSMSApi(strUserName, strPassword, strKeywordID, strShortCode, strType, strMSISDN, strTelcoID, strCharge, strMessageMOID)
            'Dim TempResult() As String
            'If Result <> "" Then
            '    TempResult = Result.Split(",")
            '    If TempResult(0) = "0" Then
            '        CPID = TempResult(1)
            '        strQueue = TempResult(2)
            '        KeywordType = TempResult(3)
            '    Else
            '        logger.Error("[ERROR]" & strInfo & "; " & TempResult(0) & ",")
            '        Response.Write(TempResult(0) & ",")
            '        Exit Sub
            '    End If
            'End If

            'strMsgGUID = System.Guid.NewGuid.ToString

            ''Configure Message Format
            ''<keyword header>:<space><content><space>P<price>/txt
            ''Example:
            ''PFC: <content>. P2.50/txt
            'Dim KeywordCRUD As New KeywordCRUD
            'Dim KeywordInfo As Tbl_Keyword = KeywordCRUD.GetKeywordInfo_byKeywordID(strKeywordID)
            'If strTelcoID = General.TelcoID._Smart Or strTelcoID = General.TelcoID._Globe Then
            '    If strOther.ToLower = "help" Then ' User to reply invalid keyword
            '        'strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
            '        If strCharge = "0" Then
            '            strMessage = strMessage
            '        Else
            '            strMessage = strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
            '        End If
            '    Else
            '        If strOther.ToLower = "stop" Then ' User to reply invalid keyword
            '            'strMessage = strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
            '        Else
            '            'Added by Patrick 2013/09/24
            '            'Description: To remove price tag(P0.00/txt) for 0 charges (ex. double opt-out & double opt-in/out)  
            '            If strCharge = "0" Then
            '                strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
            '            Else
            '                strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
            '            End If
            '        End If
            '    End If
            'Else
            '    If strTelcoID = General.TelcoID._Sun Then
            '        Dim dayIndex As Integer = Today.DayOfWeek
            '        If KeywordInfo.Keyword.ToUpper = "MYSUBS" Then ' User to reply invalid keyword
            '            'strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
            '        Else
            '            'If strOther.ToLower = "help" Then
            '            'Else
            '            '    If strCharge = "0" Then
            '            '        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
            '            '    Else
            '            '        If strOther.ToUpper = "DOUBLE" Then
            '            '            strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage '& " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
            '            '        Else
            '            '            If strKeywordID = "50" Then
            '            '                strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
            '            '            Else
            '            '                If dayIndex = DayOfWeek.Friday Then
            '            '                    strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt" & pre_footer_Fri_Msg
            '            '                Else
            '            '                    strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt" & pre_footer_Nor_Msg
            '            '                End If
            '            '            End If


            '            '        End If

            '            '    End If
            '            'End If

            '            If strOther.ToLower = "help" Then
            '            ElseIf strOther.ToLower = "welcome" Then 'Added by Patrick: Welcome message is chargeable, add P2.00/txt at the end of the msg.
            '                strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
            '            ElseIf strOther.ToLower = "content" Then
            '                If strCharge = "0" Then
            '                    If dayIndex = DayOfWeek.Friday Then
            '                        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & pre_footer_Fri_Msg
            '                    Else
            '                        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & pre_footer_Nor_Msg
            '                    End If
            '                Else
            '                    If dayIndex = DayOfWeek.Friday Then
            '                        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt" & pre_footer_Fri_Msg
            '                    Else
            '                        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt" & pre_footer_Nor_Msg
            '                    End If
            '                End If
            '            ElseIf strKeywordID = "50" Then
            '                strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
            '            ElseIf strCharge = "0" Or strOther.ToLower = "double" Then
            '                strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
            '            End If
            '            'logger.Info("[Checking...]" & KeywordInfo.Keyword.ToUpper & ";" & strOther)
            '            logger.Info("Checking MSG: " & strMSISDN & "; [MSG]: " & strMessage)
            '        End If
            '    End If
            'End If

            ''25/04/2013,A, Add a function to devide MT to 10 queue to speed up the process
            'Dim pQueueNum As String = ""

            'If Not (strShortCode = "2910" Or strShortCode = "2488") Then
            '    'Detect queue number based on the msisdn last digit
            '    pQueueNum = strMSISDN.Substring(strMSISDN.Length - 1)
            '    'Modified By:SYL, 07/06/2013, move all the smartMT content to smartMT0 and move all 
            '    'If pQueueNum = "0" Then
            '    '    pQueueNum = ""
            '    'End If
            '    If strShortCode = "2656" Then
            '        pQueueNum = "1"
            '    End If
            '    If strOther.ToUpper = "WELCOME" Or strOther.ToUpper = "FIRST" Or strOther.ToUpper = "DOUBLE" Or strOther.ToUpper = "HELP" Or strOther.ToUpper = "STOP" Or strOther.ToUpper = "OFF" Or strKeywordID = "3" Then
            '        pQueueNum = ""
            '    End If
            'Else
            '    If strShortCode = "2488" Then
            '        If strOther.ToLower <> "content" Then
            '            pQueueNum = "0"
            '        End If
            '    End If
            'End If

            Dim sql As String = "SELECT cp_id,keyword FROM premium_sms_db.keyword WHERE keyword_id = '" & strKeywordID & "'"
            Dim ds_keyword As DataSet = getQuery(sql, "keyword")
            If ds_keyword.Tables("keyword").Rows.Count > 0 Then
                CPID = ds_keyword.Tables("keyword").Rows(0).Item("cp_id").ToString
                keyword = ds_keyword.Tables("keyword").Rows(0).Item("keyword").ToString
            End If

            Dim dayIndex As Integer = Today.DayOfWeek

            'Commented by Patrick - Footer message will be handle by blast 2015-04-08
            'If strCharge = "0" Then
            '    If dayIndex = DayOfWeek.Friday Then
            '        strMessage = keyword.ToUpper & ": " & strMessage & pre_footer_Fri_Msg
            '    Else
            '        strMessage = keyword.ToUpper & ": " & strMessage & pre_footer_Nor_Msg
            '    End If
            'Else
            '    If dayIndex = DayOfWeek.Friday Then
            '        strMessage = keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt" & pre_footer_Fri_Msg
            '    Else
            '        strMessage = keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt" & pre_footer_Nor_Msg
            '    End If
            'End If

            'Dim pQueueNum As String = ""
            If strOther = "daily" Then
                Dim lastDigit As String
                lastDigit = strMSISDN.Substring(strMSISDN.Length - 1)
                If lastDigit = "0" Or lastDigit = "1" Or lastDigit = "2" Then
                    'strQueue = SUN_QUEUE_0
                    strQueue = SUN_QUEUE
                ElseIf lastDigit = "3" Or lastDigit = "4" Or lastDigit = "5" Then
                    'strQueue = SUN_QUEUE_1
                    'strQueue = SUN_QUEUE_0
                    strQueue = SUN_QUEUE
                ElseIf lastDigit = "6" Or lastDigit = "7" Or lastDigit = "8" Then
                    'strQueue = SUN_QUEUE_2
                    'strQueue = SUN_QUEUE_0
                    strQueue = SUN_QUEUE
                Else
                    'strQueue = SUN_QUEUE_3
                    'strQueue = SUN_QUEUE_0
                    strQueue = SUN_QUEUE
                End If

                ''Cross Sell Part added by Patrick 2015-03-09
                'Dim currentDate As String = Format(Now, "yyyy-MM-dd")
                'Dim crossSellMsg As String = ""

                'If currentDate = "2015-04-04" Or currentDate = "2015-04-08" Then
                '    If strKeywordID <> "289" And strKeywordID <> "290" And strKeywordID <> "291" And strKeywordID <> "292" Then
                '        crossSellMsg = "Know various life hacks for women daily. Subscribe to Women 101 by sending WMN ON to 2488. P2.00/txt."
                '    End If
                'ElseIf currentDate = "2015-04-01" Or currentDate = "2015-04-05" Then
                '    If strKeywordID <> "270" And strKeywordID <> "271" Then
                '        crossSellMsg = "Get your daily sports bulletin on the go. Subscribe to Sports Headlines by sending SPORT ON to 2488. P2.00/txt."
                '    End If
                'ElseIf currentDate = "2015-04-02" Then
                '    If strKeywordID <   > "268" And strKeywordID <> "269" Then
                '        crossSellMsg = "Get daily info on how to be fit and healthy. Subscribe to Health Tips by sending HTIP ON to 2488. P2.00/txt."
                '    End If
                'ElseIf currentDate = "2015-04-03" Then
                '    If strKeywordID <> "285" And strKeywordID <> "286" And strKeywordID <> "287" And strKeywordID <> "288" Then
                '        crossSellMsg = "Know interesting facts and trivial informations daily. Subscribe to Facts and Trivia by sending FNT ON to 2488. P2.00/txt."
                '    End If
                'End If

                'strMessage = strMessage & " " & crossSellMsg
                ''Cross Sell Part added by Patrick 2015-03-09
            Else
                strQueue = SUN_QUEUE
            End If
            'Dim pQueueNum As String = ""

            'put data to structure and send to queue
            Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc
            With strcSendSMSApi
                .Charge = strCharge
                .CPID = CPID
                .Message = strMessage
                .TransactionID = strMessageMOID
                .URL = .URL
                .TelcoID = strTelcoID
                .MsgType = strType
                .MSISDN = strMSISDN
                .Queue = strQueue
                .ReceiveDate = ReceiveDate
                .RetryCount = 0
                .MsgGUID = strMsgGUID
                .ShortCode = strShortCode
                .KeywordID = strKeywordID
                .KeywordType = KeywordType
                .MsgCount = SMSMessageCount(strMessage, strType)
                .Other = strOther
            End With

            SendToQueue(strcSendSMSApi.Queue.Trim, strcSendSMSApi)

            strInfo = strInfo & "; CPID=" & strcSendSMSApi.CPID & _
                              ";Queue=" & strcSendSMSApi.Queue & _
                              ";ReceiveDate=" & strcSendSMSApi.ReceiveDate.ToString & _
                              ";MsgGUID=" & strMsgGUID & _
                              ";Queue=" & strcSendSMSApi.Queue

            logger.Info("[SMS]" & strInfo & ";" & SuccessSend)
            Response.Write(SuccessSend & "," & strMsgGUID)
        Catch ex As Exception
            logger.Fatal("[FATAL]" & strInfo)
            logger.Fatal("[FATAL]", ex)
            Response.Write(ServerError & ",")
        End Try

    End Sub

    Public Function RequestData(ByVal strParameter As String) As String
        Dim strReturn As String = ""
        Try
            strReturn = Request.QueryString(strParameter)
            If strReturn = "" Then
                strReturn = System.Web.HttpUtility.UrlDecode(Request.Form(strParameter))
            End If
            If strReturn <> "" Then
                strReturn = strReturn.Trim
                Return strReturn
            Else
                Return ""
            End If
        Catch
            Return ""
        End Try
    End Function

    Private Function getQuery(ByVal sql As String, ByVal tbl_db As String) As DataSet
        Dim con As MySqlConnection = New MySqlConnection(mySqlConStr)
        Dim cmd As New MySqlCommand
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
            logger.Info("[Get Keyword Details]: " & sql)
        Catch ex As Exception
            logger.Info("[Get Keyword Details]: " & ex.Message & "|" & sql)
            con.Close()
            ds.Dispose()
        End Try
        Return ds
    End Function

#Region "Dump Queue"
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

#End Region

End Class