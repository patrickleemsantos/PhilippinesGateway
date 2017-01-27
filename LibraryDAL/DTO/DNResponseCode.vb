Public Class ResponseCode
    Private _ResponseID As Integer
    Private _TelcoID As Integer
    Private _StatusCode As String
    Private _StatusDesc As String
    Private _ResponseCode As String
    Private _ResponseDesc As String

    Public Property ResponseID() As Integer
        Get
            Return _ResponseID
        End Get
        Set(ByVal value As Integer)
            _ResponseID = value
        End Set
    End Property
    Public Property TelcoID() As Integer
        Get
            Return _TelcoID
        End Get
        Set(ByVal value As Integer)
            _TelcoID = value
        End Set
    End Property
    Public Property StatusCode() As String
        Get
            Return _StatusCode
        End Get
        Set(ByVal value As String)
            _StatusCode = value
        End Set
    End Property
    Public Property StatusDesc() As String
        Get
            Return _StatusDesc
        End Get
        Set(ByVal value As String)
            _StatusDesc = value
        End Set
    End Property
    Public Property ResponseCode() As String
        Get
            Return _ResponseCode
        End Get
        Set(ByVal value As String)
            _ResponseCode = value
        End Set
    End Property
    Public Property ResponseDesc() As String
        Get
            Return _ResponseDesc
        End Get
        Set(ByVal value As String)
            _ResponseDesc = value
        End Set
    End Property


End Class
