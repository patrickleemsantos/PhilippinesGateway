'Imports Library
Imports System.Messaging
Imports LibraryDAL.General

Partial Public Class SmartReceiveMO
    Inherits System.Web.UI.Page

    Private SmartMODeath_Queue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("SmartMODeath_queue")
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim ReceiveParam As String = "UD=" & Request("UD") & " OA=" & Request("OA") & " DA=" & Request("DA")

        Try
            Dim SmartMOStr As New ConMO.SmartMOStr
            Dim SmartAWS As New SmartAWS.VaeWebService

            Dim boolValid As Boolean = False
            'Smart UD format  ;LUCK ON;123456789012;1
            Dim UDArray As String() = Request("UD").Split(";")

            UDArray(1) = UDArray(1).Replace("_Oa", "@")

            With SmartMOStr
                .MSISDN = Request("OA")
                .ShortCode = Request("DA")
                .ReceiveDate = Date.Now
                .ServiceID = UDArray(3)
                .TelcoID = TelcoID._Smart
                .TransactionID = UDArray(2)
                .Message = UDArray(1)
                .RetryCount = 0
            End With


            boolValid = checkInvalidRequest(SmartMOStr)
            If boolValid = False Then
                logger.Fatal("[FATAL]" & ReceiveParam)
                Response.Write("ERROR")
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
End Class