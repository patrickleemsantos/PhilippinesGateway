Imports Library
Imports System.Messaging
Imports LibraryDAL.General

Partial Public Class SmartReceiveAWSMO
    Inherits System.Web.UI.Page

    Private SmartMODeath_Queue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("SmartMODeath_queue")
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Dim Header = Request.ServerVariables
        Dim Header As NameValueCollection = Request.Headers

        'Dim MSISDN As String = Header.Get("X-Nokia-MSISD")
        Dim MSISDN As String = Header.[Get]("X-Nokia-MSISD")
        Dim ReceiveParam As String = "msisdn=" & MSISDN & " accessCode=" & Request("accesscode") & " keyword=" & Request("keyword")

        Try
            Dim SmartMOStr As New ConMO.SmartMOStr
            Dim SmartAWS As New SmartAWS.VaeWebService

            Dim boolValid As Boolean = False
            'Dim Header As NameValueCollection = Request.Headers
            'MSISDN = Header.[Get]("msisdn")

            Dim presult = SmartAWS.ticket(MSISDN, Request("accesscode"), Request("keyword"))


            If presult.responseCode = "0000" Then

                Select Case presult.txnCode.ToUpper
                    Case "SBS_ON"

                    Case "SBS_OFF"

                    Case Else
                        With SmartMOStr
                            .MSISDN = MSISDN
                            .ShortCode = Request("accesscode")
                            .ReceiveDate = Date.Now
                            .ServiceID = presult.svcID
                            .TelcoID = TelcoID._Smart
                            .TransactionID = presult.vaeRRN
                            .Message = Request("keyword")
                            .RetryCount = 0
                        End With
                End Select



                boolValid = checkInvalidRequest(SmartMOStr)
                If boolValid = False Then
                    logger.Fatal("[FATAL]" & ReceiveParam)
                    Response.Write("ERROR")
                    Return
                End If
            Else
                Response.Write(presult.responseCode)
                Return
            End If

            Try

                Using MOsvc As New ConMO.ConMO
                    MOsvc.PostSmartMO(SmartMOStr)

                    logger.Info("[SmartMO] Message=" & SmartMOStr.Message & _
                       ";TransactionID=" & SmartMOStr.TransactionID & _
                       ";ShortCode=" & SmartMOStr.ShortCode & _
                       ";MSISDN=" & SmartMOStr.MSISDN & _
                       ";ReceiveDate=" & SmartMOStr.ReceiveDate.ToString() & _
                       ";ServiceID=" & SmartMOStr.ServiceID & _
                       ";RetryCount=" & SmartMOStr.RetryCount & _
                       ";TelcoID=" & SmartMOStr.TelcoID)
                End Using
            Catch ex As Exception
                sendToQueue(SmartMOStr, SmartMODeath_Queue)
                logger.Fatal("[FATAL]", ex)
            End Try

            Response.Write("200")
        Catch ex As Exception
            logger.Fatal("[FATAL]" & ReceiveParam)
            logger.Fatal("[FATAL]", ex)
            Response.Write("ERROR")
        End Try

    End Sub

    Private Sub sendToQueue(ByVal obj As ConMO.SmartMOStr, ByVal str_q As String)
        Using q As New MessageQueue(str_q, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

    Private Function checkInvalidRequest(ByVal body As ConMO.SmartMOStr) As Boolean
        If Not body.MSISDN = String.Empty And _
            Not body.ShortCode = String.Empty And _
            Not body.TransactionID = String.Empty Then
            Return True
        Else
            Return False
        End If
    End Function

    'Private Function CheckKeyword(ByVal pFirstKeyword As String, ByVal pSecondKeyword As String, ByVal pShortCode As String, ByVal pTelcoID As Integer) As DataSet
    '    Dim KeywordCRUD As New LibraryDAL.KeywordCRUD
    '    Dim dt As DataSet
    '    dt = KeywordCRUD.ChecKeyword(pFirstKeyword, pSecondKeyword, pTelcoID, pShortCode)
    '    Return dt
    'End Function
End Class