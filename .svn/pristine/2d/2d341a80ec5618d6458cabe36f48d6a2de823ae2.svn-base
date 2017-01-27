Imports System.Messaging
Imports LibraryDAL
Imports LibraryDAL.General
Partial Public Class sendsms
    Inherits System.Web.UI.Page

    Private strUserName, strPassword, strMSISDN, strShortCode, strKeywordID, strType, strMessage, strCharge, strUrl, strTelcoID, _
                strMessageMOID, strInfo, strIPAddress, strQueue, strMsgGUID, strOther As String

    Private CPID, KeywordType As Integer

    Private ReceiveDate As DateTime

    'Private EGG_EWKeywordID As Integer = CInt(System.Web.Configuration.WebConfigurationManager.AppSettings("EGG_EW_KewordID"))
    'Private EGGTelcoID As Integer = CInt(System.Web.Configuration.WebConfigurationManager.AppSettings("EGG_TelcoID"))
    Private EGG_MTQueue_Bulk As String = System.Web.Configuration.WebConfigurationManager.AppSettings("EGG_MTQueue_Bulk")
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim validate As Boolean = False
        Dim pResult As String = ""
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
            'checking for empty infor or wrong info
            If strUserName = "" Or strPassword = "" Or strMSISDN = "" Or strShortCode = "" Or strMessage = "" Or _
            (strKeywordID = "" AndAlso Not IsNumeric(strKeywordID)) Or _
            (strType = "" AndAlso Not IsNumeric(strType)) Or _
            (strCharge = "" AndAlso Not IsNumeric(strCharge)) Or _
            (strTelcoID = "" AndAlso Not IsNumeric(strTelcoID)) Then
                logger.Error("[ERROR]" & strInfo & "; " & MissingParameterField & ",")
                Response.Write(MissingParameterField & ",")
                Exit Sub
            End If

            '23/04/2013,A, MSISDN will be posted with up to 20 at once.
            Dim pMSISDNList As String() = strMSISDN.Split(",")
            Dim pMSISDNList_New As String = ""
            For Each _List In pMSISDNList
                'check data in databases store procedure will perform all the databases checking
                Dim OtherCURD As New OtherCRUD
                Dim Result As String = OtherCURD.CheckSendSMSApi(strUserName, strPassword, strKeywordID, strShortCode, strType, _List.ToString.Trim, strTelcoID, strCharge, strMessageMOID)
                Dim TempResult() As String
                If Result <> "" Then
                    TempResult = Result.Split(",")
                    If TempResult(0) = "0" Then
                        CPID = TempResult(1)
                        strQueue = TempResult(2)
                        KeywordType = TempResult(3)
                        validate = True
                    Else
                        logger.Error("[ERROR]" & strInfo & "; " & TempResult(0) & ",")
                        pResult = pResult & TempResult(0) & ",," & _List.ToString.Trim & ":"
                        'Response.Write(TempResult(0) & ",," & _List.ToString.Trim)
                        'Exit Sub

                    End If
                End If
                strMsgGUID = System.Guid.NewGuid.ToString
                'Response.Write(SuccessSend & "," & strMsgGUID & "," & _List.ToString.Trim)
                pResult = pResult & SuccessSend & "," & strMsgGUID & "," & _List.Trim & ":"
                pMSISDNList_New = pMSISDNList_New & _List.Trim & ","
            Next
            If validate Then
                'strMsgGUID = System.Guid.NewGuid.ToString

                'Configure Message Format
                '<keyword header>:<space><content><space>P<price>/txt
                'Example:
                'PFC: <content>. P2.50/txt
                Dim KeywordCRUD As New KeywordCRUD
                Dim KeywordInfo As Tbl_Keyword = KeywordCRUD.GetKeywordInfo_byKeywordID(strKeywordID)
                If strTelcoID = General.TelcoID._Smart Or strTelcoID = General.TelcoID._Globe Then
                    If strOther.ToLower = "help" Then ' User to reply invalid keyword
                        strMessage = strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
                    Else
                        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
                    End If
                Else
                    If strTelcoID = General.TelcoID._Sun Then
                        If KeywordInfo.Keyword.ToUpper = "MYSUBS" Then ' User to reply invalid keyword
                            'strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
                        Else
                            If strOther.ToLower = "help" Then
                            Else
                                If strCharge = "0" Then
                                    strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
                                Else
                                    If strOther.ToUpper = "DOUBLE" Then
                                        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage
                                    Else
                                        strMessage = KeywordInfo.Keyword.ToUpper & ": " & strMessage & " P" & (CDbl(strCharge) / 100).ToString("0.00") & "/txt"
                                    End If

                                End If
                            End If
                        End If
                        'logger.Info("[Checking...]" & KeywordInfo.Keyword.ToUpper & ";" & strOther)
                    End If

                End If

                logger.Info("[SMS]" & strInfo & ";" & SuccessSend)

                ''Response.Write(SuccessSend & "," & strMsgGUID & "," & _List.ToString.Trim)
                'pResult = pResult & SuccessSend & "," & strMsgGUID & "," & strMSISDN.Trim & ":"
                'pMSISDNList_New = pMSISDNList_New & strMSISDN.Trim & ","
            End If

            '25/04/2013,A, Add a function to devide MT to 10 queue to speed up the process
            Dim pQueueNum As String = "1"

            'If Not (strShortCode = "2910" Or strShortCode = "2488") Then
            '    'Detect queue number based on the msisdn last digit
            '    pQueueNum = strMSISDN.Substring(strMSISDN.Length - 1)
            '    'Modified By:SYL, 07/06/2013, move all the smartMT content to smartMT0 and move all 
            '    'If pQueueNum = "0" Then
            '    '    pQueueNum = ""
            '    'End If
            '    If strOther.ToUpper = "WELCOME" Or strOther.ToUpper = "FIRST" Then
            '        pQueueNum = ""
            '    End If
            'End If

            'Trim the end ","
            pMSISDNList_New = pMSISDNList_New.Substring(0, pMSISDNList_New.Length - 1)
            pResult = pResult.Substring(0, pResult.Length - 1)
            'Response.Write(pMSISDNList_New.ToString)

            ' put data to structure and send to queue
            Dim strcSendSMSApi As New LibraryDAL.SendSMSAPIStrc

            With strcSendSMSApi
                .Charge = strCharge
                .CPID = CPID
                .Message = strMessage
                .TransactionID = strMessageMOID
                .URL = .URL
                .TelcoID = strTelcoID
                .MsgType = strType
                .MSISDN = pMSISDNList_New
                .Queue = EGG_MTQueue_Bulk & pQueueNum
                .ReceiveDate = ReceiveDate
                .RetryCount = 0
                .MsgGUID = strMsgGUID
                .ShortCode = strShortCode
                .KeywordID = strKeywordID
                .KeywordType = KeywordType
                .MsgCount = SMSMessageCount(strMessage, strType)
                .Other = strOther
            End With

            SendToQueue(EGG_MTQueue_Bulk & pQueueNum, strcSendSMSApi)

            strInfo = strInfo & "; CPID=" & strcSendSMSApi.CPID & _
                          ";Queue=" & EGG_MTQueue_Bulk & _
                          ";ReceiveDate=" & strcSendSMSApi.ReceiveDate.ToString & _
                          ";MsgGUID=" & strMsgGUID

            Response.Write(pResult)
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