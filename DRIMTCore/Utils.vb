'Based on FBRToss code
'Copyright R. Demidov & M. Irgiznov 2006-2007
Imports System.IO

Public Module Utils
    Public dtUNIX_DATE As Date = DateTime.Parse("01.01.1970 00:00:00")
    Public arrMonth() As String = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}

    Public sAppInfoString As String
    Public strLogFileName As String
    Public IsMonoRun As Boolean = False 'установлено в True если запуск кода произошел под mono
    Public IsUnixRun As Boolean = False 'установлено в True если запуск кода произошел на платформе Unix

    Public Sub Initialize()
        Dim t As Type, p As Integer, tmpVer As String

        t = Type.GetType("Mono.Runtime")
        If IsNothing(t) Then
            'Console.WriteLine("No mono!")
            IsMonoRun = False
        Else
            'Console.WriteLine("Mono!")
            IsMonoRun = True
        End If

        tmpVer = System.Reflection.Assembly.GetExecutingAssembly().FullName
        tmpVer = Mid$(tmpVer, InStr(tmpVer, "Version=") + 8, InStr(InStr(tmpVer, "Version="), tmpVer, ",") - (InStr(tmpVer, "Version=") + 8))

        p = Environment.OSVersion.Platform
        If (p = 4 Or p = 128) Then
            sAppInfoString = "DRIMTosser for Unix v" & tmpVer 'My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Revision
            IsUnixRun = True
        Else
            sAppInfoString = "DRIMTosser for windows v" & tmpVer 'My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Revision            
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
        'tmpDate = CDate(Format(sTime, "dd mm yy hh:mm:ss"))        
        Try
            Return CDate(sTime).Subtract(dtUNIX_DATE).TotalSeconds
        Catch ex As Exception
            Return Now.Subtract(dtUNIX_DATE).TotalSeconds
        End Try
    End Function

    ''' <summary>
    ''' Функция возвращает элемент адреса из строки адреса
    ''' </summary>
    ''' <param name="sAdress"></param>
    ''' <param name="Element"></param>
    ''' <returns></returns>
    ''' <remarks>требуется доработка</remarks>
    Public Function GetAdressElement(ByVal sAdress As String, ByVal Element As AdressElement) As Short        
        If sAdress.Length = 0 Then Exit Function 'если строка адреса нулевой длины, тогда на выход
        Dim StartPos, EndPos As Short

        'узнаем какой элемент нужно получить, если это
        Select Case Element
            Case AdressElement.iZONE  'Зона, тогда
                StartPos = CShort(InStr(1, sAdress, ":") - 1)
                If StartPos >= 0 Then
                    Return CShort(Strings.Left(sAdress, StartPos))
                End If

            Case AdressElement.iNET  'Сеть, тогда
                StartPos = CShort(InStr(1, sAdress, ":") + 1)
                EndPos = CShort(InStr(1, sAdress, "/"))
                If StartPos > 1 Then
                    Return CShort(Mid(sAdress, StartPos, EndPos - StartPos))
                End If

            Case AdressElement.INode  'Нода, тогда
                StartPos = CShort(InStr(1, sAdress, "/") + 1)
                EndPos = CShort(InStr(1, sAdress, "."))
                If StartPos > 1 Then
                    If EndPos <= StartPos Then
                        Return CShort(Mid(sAdress, StartPos))
                    Else
                        Return CShort(Mid(sAdress, StartPos, EndPos - StartPos))
                    End If
                End If

            Case AdressElement.iPoint  'Поинт, тогда
                If InStrRev(sAdress, "@", sAdress.Length) <> 0 Then
                    If IsNumeric(Strings.Mid(sAdress, InStr(sAdress, ".") + 1, InStrRev(sAdress, "@", sAdress.Length) - InStr(sAdress, ".") - 1)) Then
                        Return CShort(Strings.Mid(sAdress, InStr(sAdress, ".") + 1, InStrRev(sAdress, "@", sAdress.Length) - InStr(sAdress, ".") - 1))
                    End If
                Else
                    If IsNumeric(Strings.Right(sAdress, sAdress.Length - InStrRev(sAdress, ".", sAdress.Length))) Then
                        Return CShort(Strings.Right(sAdress, sAdress.Length - InStrRev(sAdress, ".", sAdress.Length)))
                    End If
                End If

        End Select

        Return 0

    End Function

    Public Sub SaveLog(ByVal sMsg As String, Optional ByVal bOnError As Boolean = False)
        Try
            Dim fs As New FileStream(GetValidPath(Environment.CurrentDirectory) & strLogFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
            Dim bw As New BinaryWriter(fs)

            With bw
                fs.Seek(0, SeekOrigin.End)
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
        'Select Case IsUnixRun
        '    Case True                
        'If Right(tmpPath, 1) <> "/" Then
        ' tmpPath = tmpPath & "/"
        'End If
        '    Case False
        'If Right(tmpPath, 1) <> "\" Then
        ' tmpPath = tmpPath & "\"
        'End If
        'End Select
        If Right(tmpPath, 1) <> "/" Then
            Return sPath & Path.DirectorySeparatorChar
        ElseIf Right(tmpPath, 1) <> "\" Then
            Return sPath & Path.DirectorySeparatorChar
        End If
        Return sPath
    End Function

    Public Sub UnPack(ByVal sInboundDir As String, ByVal sExtendUnPackCommand As String, ByVal sArcParam As String)
        Dim arrArcExt() As String = {"MO", "TH", "WE", "TU", "FR", "SA", "SU"}
        Dim CmdLine, tmpSp As String

        If IsNothing(sInboundDir) Then
            Exit Sub
        End If

        Dim di As New DirectoryInfo(sInboundDir)

        Console.WriteLine("Start Unpacking...")

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
