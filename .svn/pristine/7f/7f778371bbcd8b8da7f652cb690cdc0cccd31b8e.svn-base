Public Partial Class testsend
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub Button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Button1.Click
        Dim MKCollection As New NameValueCollection
        Dim strColl As String = ""
        Dim strWebSendRes As String
        Dim strResultpost As String = ""
        Dim res As Byte()
        Dim strResultArray As String()
        Dim url As String = "http://localhost:52173/SmartReceiveAWSMO.aspx"


        Dim loop1, loop2 As Integer
        Dim arr1(), arr2() As String
        Dim coll As NameValueCollection


        ' Load Header collection into NameValueCollection object.
        coll = Request.Headers

        ' Put the names of all keys into a string array.
        arr1 = coll.AllKeys
        For loop1 = 0 To arr1.GetUpperBound(0)
            Response.Write("Key: " & arr1(loop1) & "<br>")
            arr2 = coll.GetValues(loop1)
            ' Get all values under this key.
            For loop2 = 0 To arr2.GetUpperBound(0)
                Response.Write("Value " & CStr(loop2) & ": " & Server.HtmlEncode(arr2(loop2)) & "<br>")
            Next loop2
        Next loop1

        Dim Header = Request.ServerVariables
        Response.Write("X-Nokia-MSISD[Get]=" & coll.[Get]("X-Nokia-MSISD"))
        Response.Write("X-Nokia-MSISDGet=" & coll.Get("X-Nokia-MSISD"))
        Response.Write("X-Nokia-MSISDHeader[Get]=" & Header.[Get]("X-Nokia-MSISD"))
        Response.Write("X-Nokia-MSISDHeader=" & Header.Get("X-Nokia-MSISD"))

        'Using web As New System.Net.WebClient
        '    web.Headers.Add("Content-Type", "application/x-www-form-urlencoded")
        '    web.Headers.Add("X-Nokia-MSISD", "60122299888")

        '    MKCollection.Add("accesscode", "testingaccesscode")
        '    MKCollection.Add("keyword", "Testingkeyword")

        '    For ai = 0 To MKCollection.Count - 1
        '        strColl = strColl & MKCollection.GetKey(ai) & "=" & MKCollection.Get(ai) & "&"
        '    Next

        '    strWebSendRes = "URL : " & url & "?" & strColl

        '    Try
        '        ' Parse the response and get the status code 200
        '        res = web.UploadValues(url, "POST", MKCollection)
        '        strResultpost = System.Text.Encoding.ASCII.GetString(res)
        '        strResultArray = Split(strResultpost, ",")

        '        strWebSendRes = strWebSendRes & " || RESULT : " & strResultpost

        '        Response.Write(strWebSendRes)
        '    Catch ex As Exception
        '    End Try
        'End Using
    End Sub
End Class