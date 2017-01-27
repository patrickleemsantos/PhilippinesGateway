Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Globalization
Imports System.Web


Namespace Extensions
    Public Module StringExtension

        'Created by : Sum
        'Created Date : 28/12/2010
        'description : use to remove array gap
        <Extension()> _
    Public Function RemoveGap(ByVal pArray() As String) As String()
            Dim CountGap As Integer = 0
            For Each i In pArray
                i = i.Trim()
                If String.IsNullOrEmpty(i) Then
                    CountGap += 1
                End If
            Next
            Dim pArray2(pArray.Length - CountGap - 1) As String

            Dim index As Integer = 0
            For Each i In pArray
                i = i.Trim()
                If Not String.IsNullOrEmpty(i) Then
                    pArray2(index) = i
                    index += 1
                End If
            Next

            Return pArray2
        End Function

    End Module
End Namespace

