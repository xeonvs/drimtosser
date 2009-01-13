'Copyright Max 'Xeon' Irgiznov 2005-2009

''' <summary>
''' Модуль для доступа к глобальным настройкам
''' </summary> 
Public Module modSettings

    ''' <summary>
    ''' Полный пусть запуска 
    ''' </summary>    
    Public AppRun As String = System.Environment.CurrentDirectory

    Public sPreTossCmd As String
    Public sArcParam As String
    Public sExecOnExit As String
    Public sExtendUnPackCommand As String
    Public sConfig As String
    Public sInboundDir As String = ""
    Public sBasePath As String
    Public sPointName As String
    Public strEchoListFileName As String
    Public bExtendUnPack As Boolean
    Public bPreTossCmd As Boolean
    Public cIni As New clsINI

    ''' <summary>
    ''' Тип тосера в соответствии со значением Core.IDatabasesTypes.enmBaseType
    ''' </summary>
    Public TosserType As IDatabasesTypes.enmBaseType = IDatabasesTypes.enmBaseType.Unknown

    Public Function LoadAppSettings() As Boolean
        If Not My.Computer.FileSystem.FileExists(sConfig) Then
            Console.WriteLine("Ini file in path " & sConfig & " not found...")
            Return False
        End If
        sInboundDir = CutOfNullChar(cIni.Value("Paths", "Inbound"))

        sPointName = CutOfNullChar(cIni.Value("Common", "PointName"))
        bExtendUnPack = System.Math.Abs(Val(CutOfNullChar(cIni.Value("Common", "UseExtendUnPack"))))
        sExtendUnPackCommand = CutOfNullChar(cIni.Value("Paths", "ExtendUnPack"))
        bPreTossCmd = System.Math.Abs(Val(CutOfNullChar(cIni.Value("Common", "ExecPreTossCmd"))))
        sPreTossCmd = CutOfNullChar(cIni.Value("Paths", "PreTossCmd"))
        sArcParam = CutOfNullChar(cIni.Value("Common", "ArcParam"))
        strLogFileName = CutOfNullChar(cIni.Value("Common", "LogFileName"))
        strEchoListFileName = CutOfNullChar(cIni.Value("Paths", "EchoListName"))

        If CutOfNullChar(cIni.Value("Common", "TosserType")).Length <> 0 Then
            TosserType = CType(CutOfNullChar(cIni.Value("Common", "TosserType")), IDatabasesTypes.enmBaseType)
        End If

        sBasePath = CutOfNullChar(cIni.Value("Paths", "Msgbase"))

        If TosserType <> IDatabasesTypes.enmBaseType.SQL Then
            Select Case IsUnixRun
                Case True
                    If Right(sBasePath, 1) <> "/" Then
                        sBasePath = sBasePath & "/"
                    End If
                Case False
                    If Right(sBasePath, 1) <> "\" Then
                        sBasePath = sBasePath & "\"
                    End If
            End Select
        End If

        If sExtendUnPackCommand = "0" Then sExtendUnPackCommand = vbNullString
        If strEchoListFileName = "0" Then strEchoListFileName = vbNullString
        Return True
    End Function

End Module
