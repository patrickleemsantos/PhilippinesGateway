Module Module1


    Private Declare Function QueryPerformanceCounter Lib "Kernel32" (ByRef X As Long) As Short
    Private Declare Function QueryPerformanceFrequency Lib "Kernel32" (ByRef X As Long) As Short

    Sub Main()



        Console.WriteLine(GetUniqueNumber())

        Console.WriteLine(GetUniqueNumber())
        'Console.WriteLine(GenerateUniqueNumber.ToString)
        Console.ReadKey()
    End Sub

    Private Function GenerateId() As String
        Dim i As Long = 1
        For Each b As Byte In Guid.NewGuid().ToByteArray()
            i *= (CInt(b) + 1)
        Next
        Return String.Format("{0:x}", i - DateTime.Now.Ticks)
    End Function

    Private Function GenerateId1() As String
        Dim i As Long = 1
        For Each b As Byte In Guid.NewGuid().ToByteArray()
            i *= (CLng(b) + 1)
        Next
        Return String.Format("{0:x}", i - DateTime.Now.Ticks)
    End Function

    Private Function GenerateUniqueNumber() As String
        Dim buffer As Byte() = Guid.NewGuid().ToByteArray()
        Return BitConverter.ToInt64(buffer, 0)
    End Function

    Private Function TestUniqueID() As String
        Return DateTime.Now.Ticks.ToString("x") + Guid.NewGuid().ToString().GetHashCode().ToString("x")
    End Function


    Private Function UniqueID2() As String
        Dim test As Long = ((DateTime.Now.Ticks / 12) Mod 100000000000)
        Dim number As String = String.Format("{0:D11}", test)

        Return number
    End Function

    Public Function TraceTime() As String
        Dim cnt As Long = 0
        Dim frq As Long = 0
        QueryPerformanceCounter(cnt)
        QueryPerformanceFrequency(frq)
        Dim c As Double = 1000 * CDbl(cnt) / CDbl(frq)
        TraceTime = c.ToString("#0.0000")
    End Function


    Public Function GetUniqueNumber() As String
        Dim cputime As String = TraceTime()
        Dim test As Long = ((DateTime.Now.Ticks / 12) Mod 100000000000)
        Dim number As String = String.Format("{0:D11}", test)

        Return number & Mid(cputime.Replace(".", "").Replace(",", ""), 8, 5)
    End Function

End Module
