'Imports Library
Imports System.Messaging
Imports LibraryDAL.General


Partial Public Class EggReceiveMO
    Inherits System.Web.UI.Page

    Private MODeath_Queue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("MODeath_queue")
    Private ShortCode As String = System.Web.Configuration.WebConfigurationManager.AppSettings("ShortCode")
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim ReceiveParam As String = ""
        Dim Number As String = Request("number")
        Dim Message As String = Request("message")
        Dim RRN As String = Request("rrn")  'moid
        Dim Telco As String = Request("telco")

        ReceiveParam = "number=" & Number & _
                                  "message=" & Message & _
                                  "rrn=" & RRN & _
                                  "telco=" & Telco

        Try

            If String.IsNullOrEmpty(Telco) Or String.IsNullOrEmpty(Number) Then
                Throw New Exception("Empty Param")
            End If

            Dim SmartMOStr As New ConMO.SmartMOStr


            With SmartMOStr
                .MSISDN = Number
                .ShortCode = ShortCode
                .ReceiveDate = Date.Now
                .ServiceID = 0
                .TelcoID = EGG_GetTelcoID(Telco)
                .TransactionID = RRN
                .Message = Message
                .RetryCount = 0
            End With

            Try
                Using MOsvc As New ConMO.ConMO

                    'post to WCF
                    MOsvc.PostSmartMO(SmartMOStr)

                    logger.Info("[MO] Message=" & SmartMOStr.Message & _
                       ";TransactionID=" & SmartMOStr.TransactionID & _
                       ";ShortCode=" & SmartMOStr.ShortCode & _
                       ";MSISDN=" & SmartMOStr.MSISDN & _
                       ";ReceiveDate=" & SmartMOStr.ReceiveDate.ToString() & _
                       ";RetryCount=" & SmartMOStr.RetryCount & _
                       ";TelcoID=" & SmartMOStr.TelcoID.ToString)
                End Using
            Catch ex As Exception
                sendToQueue(SmartMOStr, MODeath_Queue)
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
            Not body.ShortCode = String.Empty Then
            Return True
        Else
            Return False
        End If
    End Function



End Class