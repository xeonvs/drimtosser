Imports System.IO

Public Module Utils
    Public dtUNIX_DATE As Date = DateTime.Parse("01.01.1970 00:00:00")
    Public arrMonth() As String = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}

    Public sAppInfoString As String
    Public strLogFileName As String
    Public IsMonoRun As Boolean = False 'установлено в True если запуск кода произошел под mono
    Public IsUnixRun As Boolean = False 'установлено в True если запуск кода произошел на платформе Unix

    Public Sub Initialize()
        Dim t As Type, p As Integer

        t = Type.GetType("Mono.Runtime")
        If IsNothing(t) Then
            'Console.WriteLine("No mono!")
            IsMonoRun = False
        Else
            'Console.WriteLine("Mono!")
            IsMonoRun = True
        End If

        p = Environment.OSVersion.Platform
        If (p = 4 Or p = 128) Then
            sAppInfoString = "DRIMToss for Unix v" & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Revision
            IsUnixRun = True
        Else
            sAppInfoString = "DRIMToss tosser for windows v" & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Revision
            IsUnixRun = False
        End If

    End Sub

    Public Enum AdressElement
        iZONE = 0
        iNET = 1
        INode = 2
        iPoint = 3
    End Enum

    Public Function CutOfNullChar(ByVal sString As String, Optional ByRef bNotTrimString As Boolean = False) As String
        If InStr(1, sString, vbNullChar) <> 0 Then
            sString = Left(sString, InStr(1, sString, vbNullChar) - 1)
        End If
        Return IIf(IsNothing(bNotTrimString), Trim(sString), sString)
    End Function

    Public Function ParseTime(ByVal sTime As String) As Integer
        On Error GoTo ParseTime_Err

        Dim tmpDate As Date
        Dim tmpStr, tmpStrDate As String
        Dim iMonth, i As Short
        tmpStrDate = sTime
        sTime = CutOfNullChar(sTime)

        tmpStr = Mid(sTime, InStr(1, sTime, Chr(32)) + 1, 3)
        For i = 0 To 11
            If arrMonth(i) = tmpStr Then
                iMonth = i + 1
                Exit For
            End If
        Next i

        sTime = Replace(sTime, tmpStr, IIf(iMonth < 10, "0" + iMonth.ToString, iMonth.ToString))
        'tmpDate = CDate(Format(sTime, "dd mm yy hh:mm:ss"))
        tmpDate = CDate(sTime)

        'Return DateDiff(VB.DateInterval.Second, dtUNIX_DATE, tmpDate)
        Return tmpDate.Subtract(dtUNIX_DATE).TotalSeconds

        Exit Function

ParseTime_Err:

        tmpDate = DateTime.Now
        tmpDate = CDate(tmpDate)

        'Return DateDiff(VB.DateInterval.Second, dtUNIX_DATE, tmpDate)        
        Return tmpDate.Subtract(dtUNIX_DATE).TotalSeconds

    End Function

    Public Function ParseStdTime(ByVal sTime As String) As Integer
        Dim tmpDate As Date
        On Error GoTo errHandler
        'tmpDate = CDate(Format(sTime, "dd mm yy hh:mm:ss"))
        tmpDate = CDate(sTime)

        'Return DateDiff(VB.DateInterval.Second, dtUNIX_DATE, tmpDate)
        Return tmpDate.Subtract(dtUNIX_DATE).TotalSeconds

        Exit Function

errHandler:
        'Return DateDiff(VB.DateInterval.Second, dtUNIX_DATE, Now)
        Return Now.Subtract(dtUNIX_DATE).TotalSeconds
    End Function

    Public Function GetAdressElement(ByVal sAdress As String, ByVal Element As AdressElement) As Short
        'Функция возвращает элемент адреса из строки адреса
        If sAdress.Length = 0 Then Exit Function 'если строка адреса нулевой длины, тогда на выход
        Dim StartPos, EndPos As Short

        On Error GoTo ErrSub
        Select Case Element 'узнаем какой элемент нужно получить, если это
            Case AdressElement.iZONE  'Зона, тогда
                StartPos = CShort(InStr(1, sAdress, ":") - 1)
                GetAdressElement = CShort(Strings.Left(sAdress, StartPos))

            Case AdressElement.iNET  'Сеть, тогда
                StartPos = CShort(InStr(1, sAdress, ":") + 1)
                EndPos = CShort(InStr(1, sAdress, "/"))
                GetAdressElement = CShort(Mid(sAdress, StartPos, EndPos - StartPos))

            Case AdressElement.INode  'Нода, тогда
                StartPos = CShort(InStr(1, sAdress, "/") + 1)
                EndPos = CShort(InStr(1, sAdress, "."))
                If EndPos <= StartPos Then
                    GetAdressElement = CShort(Mid(sAdress, StartPos))
                Else
                    GetAdressElement = CShort(Mid(sAdress, StartPos, EndPos - StartPos))
                End If

            Case AdressElement.iPoint  'Поинт, тогда
                If InStrRev(sAdress, "@", sAdress.Length) <> 0 Then
                    GetAdressElement = CShort(Strings.Mid(sAdress, InStr(sAdress, ".") + 1, InStrRev(sAdress, "@", sAdress.Length) - InStr(sAdress, ".") - 1))
                Else
                    GetAdressElement = CShort(Strings.Right(sAdress, sAdress.Length - InStrRev(sAdress, ".", sAdress.Length)))
                End If

        End Select
        Exit Function
ErrSub:

    End Function

    Public Sub SaveLog(ByVal sMsg As String, Optional ByVal bOnError As Boolean = False)
        Try
            Dim fs As New FileStream(GetValidPath(Environment.CurrentDirectory) & strLogFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
            Dim bw As New BinaryWriter(fs)

            With bw
                .Write(System.Text.Encoding.Default.GetBytes(sMsg & vbCrLf))
                .Flush()
                .Close()
            End With

            fs.Close()
            fs.Dispose()
        Catch
            '
        End Try
    End Sub

    Public Function GetValidPath(ByVal sPath As String) As String
        Dim tmpPath As String = sPath
        Select Case IsUnixRun
            Case True
                If Right(tmpPath, 1) <> "/" Then
                    tmpPath = tmpPath & "/"
                End If
            Case False
                If Right(tmpPath, 1) <> "\" Then
                    tmpPath = tmpPath & "\"
                End If
        End Select
        Return tmpPath
    End Function

    Public Sub UnPack(ByVal sInboundDir As String, ByVal sExtendUnPackCommand As String, ByVal sArcParam As String)
        Dim arrArcExt() As String = {"MO", "TH", "WE", "TU", "FR", "SA", "SU"}
        Dim CmdLine, tmpSp As String

        If IsNothing(sInboundDir) Then
            Exit Sub
        End If

        Dim di As New DirectoryInfo(sInboundDir)

        Console.WriteLine("Unpacking...")

        Try
            For Each ext As String In arrArcExt
                For Each fln As FileInfo In di.GetFiles("*." + ext + "?")
                    tmpSp = fln.FullName
                    CmdLine = Chr(34) & sExtendUnPackCommand & Chr(34) & Chr(32) & Replace(sArcParam, "%f", Chr(34) & tmpSp & Chr(34))
                    'Debug.Print CmdLine
                    Call CmdShell(CmdLine, sInboundDir, AppWinStyle.Hide)
                    fln.Delete()
                Next
            Next
        Catch dnf As DirectoryNotFoundException
            Console.WriteLine("Путь: " & sInboundDir & " не существует или не найден")
        End Try
    End Sub

    Public Function CmdShell(ByRef ComLine As String, ByRef DefaultDir As String, ByRef ShowFlag As AppWinStyle) As Boolean
        Dim proc As New Diagnostics.Process
        Try
            proc.StartInfo.FileName = ComLine
            proc.StartInfo.WorkingDirectory = DefaultDir
            proc.StartInfo.WindowStyle = ShowFlag
            proc.Start()
            proc.WaitForExit(600000)
            Return True
        Catch
            Return False
        End Try
    End Function

End Module
