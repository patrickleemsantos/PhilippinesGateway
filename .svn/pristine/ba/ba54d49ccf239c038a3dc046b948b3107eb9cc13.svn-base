Imports System.Web.Mail
Imports System
Imports System.Configuration
Imports System.Web

Public Class MailSender
    Public Shared Function SendEmail(ByVal body As String) As Boolean

        Try
            Dim email_id As String = ConfigurationManager.AppSettings("email_id")
            Dim email_password As String = ConfigurationManager.AppSettings("email_password")
            Dim email_to As String = ConfigurationManager.AppSettings("email_to")
            Dim email_subject As String = ConfigurationManager.AppSettings("email_subject")
            Dim email_message1 As String = ConfigurationManager.AppSettings("email_message1")

            Dim config_smtpserver As String = ConfigurationManager.AppSettings("config_smtpserver")
            Dim config_smtpserverport As String = ConfigurationManager.AppSettings("config_smtpserverport")
            Dim config_sendusing As String = ConfigurationManager.AppSettings("config_sendusing")
            Dim config_smtpauthenticate As String = ConfigurationManager.AppSettings("config_smtpauthenticate")
            Dim config_smtpusessl As String = ConfigurationManager.AppSettings("config_smtpusessl")

            body = email_message1 & "<br/><br />" & body & "</b><br />Thank you"

            Dim emails As String() = email_to.Split(CChar(" "))
            For Each email As String In emails

                Dim myMail As New Mail.MailMessage()

                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserver", config_smtpserver)
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", config_smtpserverport)
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusing", config_sendusing)
                
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", config_smtpauthenticate)
                'Use 0 for anonymous
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", email_id)
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", email_password)
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", config_smtpusessl)
                myMail.From = email_id
                myMail.[To] = email
                myMail.Subject = email_subject
                myMail.BodyFormat = MailFormat.Html
                myMail.Body = "<html><head></head><body>" & body & "</body></html>"

                System.Web.Mail.SmtpMail.SmtpServer = config_smtpserver & ":" & config_smtpserverport
                System.Web.Mail.SmtpMail.Send(myMail)

            Next
            Return True
        Catch ex As Exception
            Throw
        End Try
    End Function
End Class

