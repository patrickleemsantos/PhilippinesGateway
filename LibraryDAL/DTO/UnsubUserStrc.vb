Public Class UnsubUserStrc

    Public Sub New()
    End Sub

    Private _KeywordID As Integer
    Public Overridable Property KeywordID() As Integer
        Get
            Return _KeywordID
        End Get
        Set(ByVal value As Integer)
            _KeywordID = value
        End Set
    End Property

    Private _TransactionID As String
    Public Overridable Property TransactionID() As String
        Get
            Return _TransactionID
        End Get
        Set(ByVal value As String)
            _TransactionID = value
        End Set
    End Property

    Private _MSISDN As String
    Public Overridable Property MSISDN() As String
        Get
            Return _MSISDN
        End Get
        Set(ByVal value As String)
            _MSISDN = value
        End Set
    End Property

End Class
