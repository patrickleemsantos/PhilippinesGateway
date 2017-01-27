Imports System.Runtime.Serialization

<System.Runtime.Serialization.DataContractAttribute()> _
Public Class SmartMOStr
    'structure
    Public Sub New()
    End Sub

    Private _MSISDN As String      'PhoneNumber
    <DataMember()> _
 Public Property MSISDN() As String
        Get
            Return _MSISDN
        End Get
        Set(ByVal value As String)
            _MSISDN = value
        End Set
    End Property

    Private _ShortCode As String   ' AccessCode = ShortCode
    <DataMember()> _
    Public Property ShortCode() As String
        Get
            Return _ShortCode
        End Get
        Set(ByVal value As String)
            _ShortCode = value
        End Set
    End Property

    Private _Message As String      ' ContentSendByUser
    <DataMember()> _
   Public Property Message() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property

    Private _TransactionID As String  'RPN MOID
    <DataMember()> _
     Public Property TransactionID() As String
        Get
            Return _TransactionID
        End Get
        Set(ByVal value As String)
            _TransactionID = value
        End Set
    End Property

    Private _ServiceID As String
    <DataMember()> _
    Public Property ServiceID() As String
        Get
            Return _ServiceID
        End Get
        Set(ByVal value As String)
            _ServiceID = value
        End Set
    End Property

    Private _TelcoID As Integer
    <DataMember()> _
    Public Property TelcoID() As Integer
        Get
            Return _TelcoID
        End Get
        Set(ByVal value As Integer)
            _TelcoID = value
        End Set
    End Property

    Private _ReceiveDate As DateTime
    <DataMember()> _
    Public Property ReceiveDate() As DateTime
        Get
            Return _ReceiveDate
        End Get
        Set(ByVal value As DateTime)
            _ReceiveDate = value
        End Set
    End Property

    Private _RetryCount As Integer = 0
    <DataMember()> _
    Public Property RetryCount() As Integer
        Get
            Return _RetryCount
        End Get
        Set(ByVal value As Integer)
            _RetryCount = value
        End Set
    End Property


End Class
