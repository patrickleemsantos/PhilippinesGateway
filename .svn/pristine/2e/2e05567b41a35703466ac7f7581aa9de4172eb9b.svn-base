Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security
Imports System.Net
Imports System.Messaging

Public Module GeneralLocal

    Public MOForwardQueue As String = ConfigurationManager.AppSettings("MOForward_queue")
    Public APIUrl As String = ConfigurationManager.AppSettings("APIUrl")
    Public APIPass As String = ConfigurationManager.AppSettings("APIPass")
    Public APIUserName As String = ConfigurationManager.AppSettings("APIUserName")
    Public SmartInvalidReplyKW As String = ConfigurationManager.AppSettings("SmartInvalidReplyKW")
    Public SmartHelpReplyKW As String = ConfigurationManager.AppSettings("SmartHelpReplyKW")

    Public SunAPIPass As String = ConfigurationManager.AppSettings("SunAPIPass")
    Public SunAPIUserName As String = ConfigurationManager.AppSettings("SunAPIUserName")
    Public SunInvalidReplyKW As String = ConfigurationManager.AppSettings("SunInvalidReplyKW")

    Public SunRegAlreadySubsService As String = ConfigurationManager.AppSettings("SunRegAlreadySubsService")
    Public SunUnSubsMessage As String = ConfigurationManager.AppSettings("SunUnSubsMsg")
    Public SunStopAlreadyUnSubsService As String = ConfigurationManager.AppSettings("SunAlreadyUnSubsMsg")
    Public CheckServiceMessageBottomSun As String = ConfigurationManager.AppSettings("CheckServiceMsgBottomSun")
    Public CheckServiceMessageHeaderSun As String = ConfigurationManager.AppSettings("CheckServiceMsgHeaderSun")
    Public SunCheckNotSubsMessage As String = ConfigurationManager.AppSettings("SunCheckNotSubsMsg")
    Public SunStopAllMessage As String = ConfigurationManager.AppSettings("SunStopAllMsg")
    Public SunStopAllMessageHeader As String = ConfigurationManager.AppSettings("SunStopAllMsgHeader")
    Public SunStopAllMessageBtm As String = ConfigurationManager.AppSettings("SunStopAllMsgBtm")
    Public SunStopAllMessageNotSubs As String = ConfigurationManager.AppSettings("SunStopAllMsgNotSubs")
    Public SunInvalidKeywordMessage As String = ConfigurationManager.AppSettings("SunInvalidKeywordMsg")

    Public InvalidKeywordMessage As String = ConfigurationManager.AppSettings("InvalidKeywordMsg")
    Public HelpMessage As String = ConfigurationManager.AppSettings("HelpMsg")
    Public StopAllMessage As String = ConfigurationManager.AppSettings("StopAllMsg")
    Public StopAllMessageNotSubs As String = ConfigurationManager.AppSettings("StopAllMsgNotSubs")
    Public StopMessageNotSubs As String = ConfigurationManager.AppSettings("StopNotSubsMsg")
    Public CheckServiceMessageHeader As String = ConfigurationManager.AppSettings("CheckServiceMsgHeader")
    Public CheckServiceMessageBottom As String = ConfigurationManager.AppSettings("CheckServiceMsgBottom")

    Public CheckNotSubsMessage As String = ConfigurationManager.AppSettings("CheckNotSubsMsg")
    Public RegAlreadySubsService As String = ConfigurationManager.AppSettings("RegAlreadySubsService")
    Public StopAlreadyUnSubsService As String = ConfigurationManager.AppSettings("AlreadyUnSubsMsg")
    Public UnSubsMessage As String = ConfigurationManager.AppSettings("UnSubsMsg")
    Public TimeOutPostToAPI As String = ConfigurationManager.AppSettings("TimeOutPostToAPI")


    Public Sub dumpToQueue(ByVal destination As String, ByVal obj As Object)
        Using q As New MessageQueue(destination, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

End Module

