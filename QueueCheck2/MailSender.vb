Imports System.Web.Mail
Imports System
Imports System.Configuration

Public Class MailSender
    Public Shared Function SendEmail(ByVal body As String, ByVal attch As String, ByVal attch2 As String) As String

        Try
            'Dim pAttachmentPath As String = ""
            'Dim body As String = ""
            Dim email_id As String = ConfigurationManager.AppSettings("email_id")
            Dim email_password As String = ConfigurationManager.AppSettings("email_password")
            Dim email_to As String = ConfigurationManager.AppSettings("email_to")
            Dim email_subject As String = ConfigurationManager.AppSettings("email_subject")
            Dim email_message1 As String = ConfigurationManager.AppSettings("email_message1")
            Dim email_message2 As String = ConfigurationManager.AppSettings("email_message2")

            Dim config_smtpserver As String = ConfigurationManager.AppSettings("config_smtpserver")
            Dim config_smtpserverport As String = ConfigurationManager.AppSettings("config_smtpserverport")
            Dim config_sendusing As String = ConfigurationManager.AppSettings("config_sendusing")
            Dim config_smtpauthenticate As String = ConfigurationManager.AppSettings("config_smtpauthenticate")
            Dim config_smtpusessl As String = ConfigurationManager.AppSettings("config_smtpusessl")

            body = email_message1 & "<br/><br />" & email_message2 & "<br /><br />" & body & "<br />Thank you"

            Dim emails As String() = email_to.Split(CChar(" "))
            For Each email As String In emails

                Dim myMail As New System.Web.Mail.MailMessage()

                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserver", config_smtpserver)
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", config_smtpserverport)
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusing", config_sendusing)
                'sendusing: cdoSendUsingPort, value 2, for sending the message using 
                'the network.

                'smtpauthenticate: Specifies the mechanism used when authenticating 
                'to an SMTP 
                'service over the network. Possible values are:
                '- cdoAnonymous, value 0. Do not authenticate.
                '- cdoBasic, value 1. Use basic clear-text authentication. 
                'When using this option you have to provide the user name and password 
                'through the sendusername and sendpassword fields.
                '- cdoNTLM, value 2. The current process security context is used to 
                ' authenticate with the service.
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
                If attch.Trim() <> "" Then
                    Dim MyAttachment As New MailAttachment(attch)
                    myMail.Attachments.Add(MyAttachment)
                    myMail.Priority = System.Web.Mail.MailPriority.High
                End If
                If attch2.Trim() <> "" Then
                    Dim MyAttachment As New MailAttachment(attch2)
                    myMail.Attachments.Add(MyAttachment)
                    myMail.Priority = System.Web.Mail.MailPriority.High
                End If

                System.Web.Mail.SmtpMail.SmtpServer = config_smtpserver & ":" & config_smtpserverport
                System.Web.Mail.SmtpMail.Send(myMail)

            Next
            Return "Success"
        Catch ex As Exception
            Return "Fail"
        End Try
    End Function
End Class

