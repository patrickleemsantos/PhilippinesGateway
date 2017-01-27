Imports System.Data.SqlClient
Imports System.Xml
Imports System.IO
Imports System.Data
Imports System.Configuration



Public Class CRUD

    Dim pKeyStr As String = ConfigurationManager.AppSettings("Conn").ToString
    Dim sqlConn As SqlConnection = New SqlConnection(pKeyStr)

    Public Function getMT() As DataSet

        Dim strQuery As String = "select MSISDN,[Message],Charge,MsgGUID,KeywordID,MTID,shortcode from outbox " & _
                                            "where senddate >='2013-03-20' and senddate <'2013-03-21 '" & _
                                            "and telcoid = 3 " & _
                                            "and statuscode = '309'"

        Dim DSRoot As Data.DataSet

        sqlConn.Open()

        Dim myCommand As New SqlCommand()
        myCommand.Connection = sqlConn
        myCommand.CommandText = strQuery
        Dim adap As SqlDataAdapter = New SqlDataAdapter(myCommand.CommandText, myCommand.Connection)
        DSRoot = New DataSet("roots")
        adap.Fill(DSRoot)
        sqlConn.Close()

        Return DSRoot
    End Function

    Public Function GetAllFromEggDN(ByVal pDatefrom As String, ByVal pDateTo As String) As DataSet
        Dim strQuery As String = ""

        Try

            strQuery = "SELECT Transid,Status,ReceiveDate FROM eggdn WHERE ReceiveDate >='" & pDatefrom & "' and ReceiveDate <'" & pDateTo & "' and status = 200"

            Dim DSRoot As Data.DataSet

            sqlConn.Open()

            Dim myCommand As New SqlCommand()
            myCommand.Connection = sqlConn
            myCommand.CommandText = strQuery
            Dim adap As SqlDataAdapter = New SqlDataAdapter(myCommand.CommandText, myCommand.Connection)
            DSRoot = New DataSet("roots")
            adap.Fill(DSRoot)
            sqlConn.Close()

            Return DSRoot
        Catch ex As Exception
            Throw ex
        End Try
    End Function
End Class
