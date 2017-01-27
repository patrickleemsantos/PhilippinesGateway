'Imports Library
Imports System.Messaging
Imports LibraryDAL.General
Imports LibraryDAL

Partial Public Class EggReceiveDN_2910
    Inherits System.Web.UI.Page

    Private EGGDN_Queue As String = System.Web.Configuration.WebConfigurationManager.AppSettings("EGGDN_Queue")
    Private Shared ReadOnly logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString)
    Private Shared ReadOnly loggerDN As log4net.ILog = log4net.LogManager.GetLogger("ReceiveDN")

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim ReceiveParam As String = ""
        Dim msisdn As String = Request("msisdn")
        Dim transid As String = Request("rcvd_transid")
        Dim status As String = Request("status")

        'msisdn  - mobile number to charged and received sms message
        'rcvd_transid  -  received transaction id.
        'Status – status of transaction: 1-Successfully Charged, 2-Server Error, 3-Insufficient Bal.


        ReceiveParam = "msisdn=" & msisdn & ";transid=" & transid & ";status=" & status

        Try

            If String.IsNullOrEmpty(msisdn) Or String.IsNullOrEmpty(transid) Or String.IsNullOrEmpty(status) Then
                Throw New Exception("Empty Param")
            End If

            Dim EGGDNStrc As New EGGDNStrc
            Dim boolValid As Boolean = False

            With EGGDNStrc
                .ReceiveDate = DateTime.Now
                .TransID = transid
                .Status = status
                .ShortCode = "2910"
            End With

            loggerDN.Info("[DN]" & ReceiveParam & ";ReceiveDate=" & EGGDNStrc.ReceiveDate.ToString)
            sendToQueue(EGGDNStrc, EGGDN_Queue)

            Response.Write("200")
        Catch ex As Exception
            logger.Fatal("[FATAL]" & ReceiveParam)
            logger.Fatal("[FATAL]", ex)
            Response.Write("ERROR")
        End Try

    End Sub

    Private Sub sendToQueue(ByVal obj As Object, ByVal str_q As String)
        Using q As New MessageQueue(str_q, True)
            MessageQueue.EnableConnectionCache = False
            Using qtrans As New MessageQueueTransaction
                qtrans.Begin()
                q.Send(obj, qtrans)
                qtrans.Commit()
            End Using
        End Using
    End Sub

End Class