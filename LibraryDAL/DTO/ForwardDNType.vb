Public Class ForwardDNType

    Private _msgMTid As String
    Public Overridable Property msgMTid() As String
        Get
            Return _msgMTid
        End Get
        Set(ByVal value As String)
            _msgMTid = value
        End Set
    End Property

    Private _status_code As String
    Public Overridable Property status_code() As String
        Get
            Return _status_code
        End Get
        Set(ByVal value As String)
            _status_code = value
        End Set
    End Property

    Private _TelcoID As Integer
    Public Overridable Property TelcoID() As Integer
        Get
            Return _TelcoID
        End Get
        Set(ByVal value As Integer)
            _TelcoID = value
        End Set
    End Property

    Private _DnDate As DateTime
    Public Overridable Property DnDate() As DateTime
        Get
            Return _DnDate
        End Get
        Set(ByVal value As DateTime)
            _DnDate = value
        End Set
    End Property


End Class
