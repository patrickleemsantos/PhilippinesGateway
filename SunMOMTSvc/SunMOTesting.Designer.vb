<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SunMOTesting
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.txtFirstKeyword = New System.Windows.Forms.TextBox
        Me.txtSecondKeyword = New System.Windows.Forms.TextBox
        Me.txtSend = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'txtFirstKeyword
        '
        Me.txtFirstKeyword.Location = New System.Drawing.Point(12, 20)
        Me.txtFirstKeyword.Name = "txtFirstKeyword"
        Me.txtFirstKeyword.Size = New System.Drawing.Size(100, 20)
        Me.txtFirstKeyword.TabIndex = 0
        '
        'txtSecondKeyword
        '
        Me.txtSecondKeyword.Location = New System.Drawing.Point(12, 60)
        Me.txtSecondKeyword.Name = "txtSecondKeyword"
        Me.txtSecondKeyword.Size = New System.Drawing.Size(100, 20)
        Me.txtSecondKeyword.TabIndex = 1
        '
        'txtSend
        '
        Me.txtSend.Location = New System.Drawing.Point(36, 99)
        Me.txtSend.Name = "txtSend"
        Me.txtSend.Size = New System.Drawing.Size(75, 23)
        Me.txtSend.TabIndex = 2
        Me.txtSend.Text = "Send"
        Me.txtSend.UseVisualStyleBackColor = True
        '
        'SunMOTesting
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(132, 182)
        Me.Controls.Add(Me.txtSend)
        Me.Controls.Add(Me.txtSecondKeyword)
        Me.Controls.Add(Me.txtFirstKeyword)
        Me.Name = "SunMOTesting"
        Me.Text = "SunMOTesting"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtFirstKeyword As System.Windows.Forms.TextBox
    Friend WithEvents txtSecondKeyword As System.Windows.Forms.TextBox
    Friend WithEvents txtSend As System.Windows.Forms.Button
End Class
