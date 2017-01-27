Module Module1

    Sub Main()

        Dim TestXML As String = "<HTML><BODY><DA>639473290572</DA><br><EC>975</EC><br><ECT>null</ECT><br></BODY></HTML>"

        Dim DA, EC, ECT As String

        TestXML = TestXML.Replace("<br>", "")

        Dim xml As XDocument = XDocument.Parse(TestXML)

        Dim ChildQuery = From c In xml.Element("HTML").Descendants("BODY") Select c

        For Each d As XElement In ChildQuery
            DA = d.Element("DA").Value.ToString
            EC = d.Attribute("EC").Value.ToString  'Response Code
            ECT = d.Attribute("ECT").Value.ToString  'Response Description
        Next
    End Sub

End Module
