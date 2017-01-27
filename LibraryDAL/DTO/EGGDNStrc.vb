Public Class EGGDNStrc

    Public Sub New()
    End Sub

    Private _TransID As String = ""
    Public Overridable Property TransID() As String
        Get
            Return _TransID
        End Get
        Set(ByVal value As String)
            _TransID = value
        End Set
    End Property

    Private _Status As String = ""
    Public Overridable Property Status() As String
        Get
            Return _Status
        End Get
        Set(ByVal value As String)
            _Status = value
        End Set
    End Property

    Private _ShortCode As String = ""
    Public Overridable Property ShortCode() As String
        Get
            Return _ShortCode
        End Get
        Set(ByVal value As String)
            _ShortCode = value
        End Set
    End Property


    Private _ReceiveDate As DateTime
    Public Overridable Property ReceiveDate() As DateTime
        Get
            Return _ReceiveDate
        End Get
        Set(ByVal value As DateTime)
            _ReceiveDate = value
        End Set
    End Property


End Class
