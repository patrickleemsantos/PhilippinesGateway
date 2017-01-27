Imports System.Configuration

Public Class SunMOTesting

    Private Sub txtSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSend.Click
        Dim firstKeyword As String = txtFirstKeyword.Text
        Dim secondKeyword As String = txtSecondKeyword.Text

        Dim clsSunMO As New SunMO
        With clsSunMO
            .firstKeyword = firstKeyword
            .secondKeyword = secondKeyword
            .msisdn = "09229948061"
            .msgid = ""
            .shortcode = "2488"
            .message = firstKeyword & " " & secondKeyword
            .telcoID = 2

            If .getKeywordDetails = True Then
            Else
                If .getFirstKeywordDetails = False Then
                    .keywordID = 106
                    .sendInvalidMessage()
                    .insertInbox()
                    Exit Sub
                End If
            End If

            If .keywordType = 1 Then
                .postMOToCP()
            ElseIf .keywordType = 2 Then
                If .reserveKeywordType = 1 Then
                    If .isSubscriptionExist = True Then
                        If .isSubscriberActive = True Then
                            .sendDoubleOptInMessage()
                            .postMOToCP()
                        Else
                            .sendOptInMessage()
                            .activateSubscriber()
                            .postMOToCP()
                        End If
                    Else
                        .sendOptInMessage()
                        .addSubscriber()
                        .postMOToCP()
                    End If
                ElseIf .reserveKeywordType = 2 Then
                    If .isSubscriptionExist = True Then
                        If .isSubscriberActive = True Then
                            .sendOptOutMessage()
                            .deactivateSubscriber()
                            .postMOToCP()
                        Else
                            .sendDoubleOptOutMessage()
                            .postMOToCP()
                        End If
                    Else
                        .sendDoubleOptOutMessage()
                        .postMOToCP()
                    End If
                ElseIf .reserveKeywordType = 5 Then
                    .sendStopAllMessage()
                    .deactivateAllSubscription()
                    .postStopAllMOToCP()
                ElseIf .reserveKeywordType = 3 Then
                    .sendHelpMessage()
                ElseIf .reserveKeywordType = 4 Then
                    .sendCheckMessage()
                End If
            End If
            .insertInbox()
        End With
    End Sub

End Class