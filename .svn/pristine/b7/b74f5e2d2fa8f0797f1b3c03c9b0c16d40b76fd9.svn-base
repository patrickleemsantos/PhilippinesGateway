Imports System.Net
Imports System.Messaging
Imports System.ServiceModel
Imports MySql.Data.MySqlClient
Imports System.IO
Imports LibraryDAL
Imports LibraryDAL.General
Imports LibraryDAL.Extensions

<ServiceBehavior(InstanceContextMode:=InstanceContextMode.PerCall, ConcurrencyMode:=ConcurrencyMode.Multiple)> _
Public Class ConMO
    Implements IMO

    Private EGGMODeath_Queue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("EGGMODeath_queue")
    Private EGGMOInsert_queue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("EGGMOInsert_queue")
    Private EGGMOForward_queue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("EGGMOForward_queue")

    Private MOType As New LibraryDAL.Tbl_MOStr

    Private ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Public Sub New()
    End Sub

    Public Sub PostSmartMO(ByVal Body As SmartMOStr) Implements IMO.PostSmartMO

        Dim TempMessage() As String
        Dim MOUrl As String = ""
        Dim sendString As String = ""
        Dim KeywordID As Integer = 0

        Dim dt As DataSet
        Dim FirstKeyword As String = ""
        Dim SecondKeyword As String = ""
        Dim strResult As String = ""
        Dim TempMessage1 As String = ""

        Try

            TempMessage1 = Body.Message
            TempMessage = Split(TempMessage1, " ").RemoveGap  ' split the message by spacing

            If TempMessage.Count > 0 Then
                FirstKeyword = UCase(TempMessage(0)) ' get first keyword and upper case the keyword 
            End If

            If TempMessage.Count > 1 Then
                SecondKeyword = UCase(TempMessage(1)) ' get second keyword upper case the keyword 
            End If

            dt = CheckKeyword(FirstKeyword, SecondKeyword, Body.ShortCode, Body.TelcoID) ' check for first and second keyword


            If dt.Tables(0).Rows.Count <= 0 Then
                dt = CheckKeyword(FirstKeyword, "", Body.ShortCode, Body.TelcoID)  ' check for first keyword
                If dt.Tables(0).Rows.Count <= 0 Then
                    Dim JunkKeyword As String = "JUNK_" & Body.ShortCode
                    dt = CheckKeyword(JunkKeyword, "", Body.ShortCode, Body.TelcoID) ' check for junk keyword 
                End If
            End If

            If dt.Tables(0).Rows.Count > 0 Then
                For Each r As DataRow In dt.Tables(0).Rows
                    KeywordID = r.Item("KeywordID")
                    MOUrl = r.Item("rMOUrl")
                    If MOUrl = "" Then ' if no reserve keyword url  then get keyword url (cp apps url)
                        MOUrl = r.Item("kMOUrl")
                    End If
                Next
            End If

            'post content to URL
            If MOUrl <> "" Then
                sendString = "from=" & System.Web.HttpUtility.UrlEncode(Body.MSISDN) _
                & "&msgid=" & System.Web.HttpUtility.UrlEncode(Body.TransactionID) _
                & "&message=" & System.Web.HttpUtility.UrlEncode(Body.Message) _
                & "&sc=" & System.Web.HttpUtility.UrlEncode(Body.ShortCode) _
                & "&telcoid=" & System.Web.HttpUtility.UrlEncode(Body.TelcoID) _
                & "&serviceid=" & System.Web.HttpUtility.UrlEncode(KeywordID)

                'forward to MO Forward Queue
                Dim MOForwardStrc As New LibraryDAL.MOForwardStrc
                With MOForwardStrc
                    .URL = MOUrl
                    .URLData = sendString
                End With
                dumpToQueue(EGGMOForward_queue, MOForwardStrc)

            End If

            'insert inbox mo
            With MOType
                .TelcoID = Body.TelcoID
                .MSISDN = Body.MSISDN
                .TransactionID = Body.TransactionID
                .KeywordID = KeywordID
                .DataCoding = 0
                .ReceiveDate = Body.ReceiveDate
                .Message = Body.Message
            End With

            dumpToQueue(EGGMOInsert_queue, MOType)


            logger.Info("EGG_MO]" & getObjInfo(MOType, Body.RetryCount))

        Catch ex As Exception
            logger.Fatal("[FATAL]", ex)
            dumpToQueue(EGGMODeath_Queue, Body)
        End Try

    End Sub

#Region "Function"
    Private Function CheckKeyword(ByVal pFirstKeyword As String, ByVal pSecondKeyword As String, ByVal pShortCode As String, ByVal pTelcoID As Integer) As DataSet
        Dim KeywordCRUD As New KeywordCRUD
        Dim dt As DataSet
        dt = KeywordCRUD.ChecKeyword(pFirstKeyword, pSecondKeyword, pTelcoID, pShortCode)
        Return dt
    End Function

    Private Function getObjInfo(ByVal objMessage As LibraryDAL.Tbl_MOStr, ByVal RetryCount As Integer) As String
        Dim myText As String = ""

        myText = "KeywordID=" + objMessage.KeywordID.ToString & ";" & _
                "MSISDN=" + objMessage.MSISDN + ";" & _
                "TransactionID=" + objMessage.TransactionID + ";" & _
                "DataCoding=" + objMessage.DataCoding.ToString + ";" & _
                "TelcoID=" + objMessage.TelcoID.ToString + ";" & _
                "ReceiveDate=" + objMessage.ReceiveDate.ToString + ";" & _
                "RetryCount=" + RetryCount.ToString

        Return myText
    End Function
#End Region

#Region "Dump Queue"
    Private Sub dumpToQueue(ByVal destination As String, ByVal obj As Object)
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

