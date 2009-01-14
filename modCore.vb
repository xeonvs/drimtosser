'Copyright Max 'Xeon' Irgiznov 2005-2009
Option Explicit On
Option Strict Off

Imports System.Reflection
Imports System.IO

Module modCore

    ''' <summary>
    ''' Поддерживаемые базы
    ''' </summary>
    ''' <remarks></remarks>
    Public Bases As New Dictionary(Of IDatabasesTypes.enmBaseType, System.Type)

    ''' <summary>
    ''' Загружает Плагины для работы с базами сообщений.
    ''' </summary>
    ''' <remarks>Загружаются только те сборки внутри которых определен класс Tosser</remarks>
    Public Sub LoadTossersModules()
        Dim di As New DirectoryInfo(AppRun)
        Dim modules() As FileInfo = di.GetFiles("*Base.dll")
        Dim a As Assembly

        For Each fModule As FileInfo In modules
            Dim cModule As String = fModule.Name
            Dim ModuleName As String = cModule.Replace(".dll", "")

            Try
                a = Assembly.LoadFrom(cModule)
                Dim mytypes As Type() = a.GetTypes()
                Dim flags As BindingFlags = BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Static Or _
                                            BindingFlags.Instance Or BindingFlags.DeclaredOnly
                Dim dbtype As IDatabasesTypes.enmBaseType = IDatabasesTypes.enmBaseType.Unknown

                For Each t As Type In mytypes
                    If t.FullName.IndexOf(ModuleName + ".Tosser") <> -1 Then
                        Dim mi As MethodInfo() = t.GetMethods(flags)
                        Dim obj As Object = Activator.CreateInstance(t)

                        For Each m As MethodInfo In mi
                            If m.Name.IndexOf("get_BaseType") <> -1 Then
                                dbtype = m.Invoke(obj, Nothing)
                            End If
                        Next
                        'возможно тут понадобиться проверка на поддержку одновременно нескольких sql-баз
                        If dbtype <> IDatabasesTypes.enmBaseType.Unknown And t.IsPublic = True Then
                            Bases.Add(dbtype, t)
                        Else
                            'errroooorrrr
                        End If
                    End If
                Next
            Catch ex As FileLoadException
                Console.WriteLine("Ошибка при загрузке модуля базы " & cModule & vbCrLf & "Проверьте его тип.")
            End Try
        Next
    End Sub

    Private Sub PrintUsage()
        Console.WriteLine(sAppInfoString)
        Console.WriteLine(" Usage")
        Console.WriteLine("-------------")
        Console.WriteLine("DRIMTosser.exe options")
        Console.WriteLine("options:")
        Console.WriteLine("-V" & vbTab & "for version")
        Console.WriteLine("-M" & vbTab & "show loaded modules")
        Console.WriteLine("-F:" & vbTab & "patch to config file")
        Console.WriteLine("-TOSS" & vbTab & "start tossing process")
        Console.WriteLine("-GUI" & vbTab & "gui frontend(not implemented")
        Console.WriteLine()
    End Sub

    Sub Main()
        Dim sPos, EndPos As Short, blnAltConfig As Boolean
        Dim commands As String = Microsoft.VisualBasic.Interaction.Command()
        Dim args() As String = commands.Split(" ".ToCharArray)
        Dim mdx As IModule

        DRIMTCore.Utils.Initialize()

        'Показываем версию и выходим
        If InStr(1, commands.ToUpper, "-V") <> 0 Then
            Console.WriteLine(sAppInfoString)
            Exit Sub
        End If

        If InStr(1, commands.ToUpper, "-?") <> 0 Then
            PrintUsage()
            Exit Sub
        End If


        LoadTossersModules()

        If InStr(1, commands.ToUpper, "-M") <> 0 Then
            Console.WriteLine(sAppInfoString)
            Console.WriteLine("=======================")
            Console.WriteLine("Loaded Modules:")
            For Each item As [Enum] In Bases.Keys
                mdx = Activator.CreateInstance(Bases.Item(item))
                Console.WriteLine(mdx.Name & vbTab & mdx.Description)
                mdx.Dispose()
            Next
            Exit Sub
        End If

        sPos = InStr(1, commands.ToUpper, "-F:")
        If sPos <> 0 Then
            blnAltConfig = True
            EndPos = InStr(sPos + 6, commands.ToUpper, "-")
            If EndPos = 0 Then
                If Mid(commands.ToUpper, sPos + 3, 1) = Chr(34) Then
                    EndPos = InStr(sPos + 4, commands.ToUpper, Chr(34))
                Else
                    EndPos = InStr(sPos, commands.ToUpper, Chr(32))
                End If
                sConfig = Replace(Right(commands, CInt(IIf(EndPos > 0, EndPos - sPos - 3, commands.Length - sPos - 3))), Chr(34), vbNullString)
            Else
                sConfig = Replace(commands.ToUpper, "-V", vbNullString)
                sConfig = Replace(sConfig.ToUpper, "-TOSS", vbNullString)
                sConfig = Replace(sConfig.ToUpper, "-GUI", vbNullString)
                sConfig = Replace(sConfig.ToUpper, "-F:", vbNullString)
                sConfig = Trim(Replace(sConfig.ToUpper, Chr(34), vbNullString))
            End If

            'Debug.Print strConfPath
        End If

        If Not blnAltConfig Then
            sConfig = GetValidPath(Environment.CurrentDirectory) & "DRIMToss.ini"
        End If

        cIni.FileName = sConfig

        If Not modSettings.LoadAppSettings() Then
            Exit Sub
        End If

        'запуск гуи для отображения работы
        If InStr(1, UCase(commands), "-GUI") <> 0 Then
            ''note: при выходе из Sub Main будет закрыто приложение!
        End If

        'запуск тоссинга
        If InStr(1, commands.ToUpper, "-TOSS") <> 0 Then
            'Перед тоссингом выполняется распаковка и запуск внешних комманд.
            Console.WriteLine(sAppInfoString)
            Call DRIMTCore.Utils.UnPack(sInboundDir, sExtendUnPackCommand, sArcParam)

            If bPreTossCmd Then
                If Trim(sPreTossCmd).Length = 0 Then
                    Call CmdShell(sPreTossCmd, sInboundDir, AppWinStyle.Hide)
                End If
            End If

            Try
                mdx = Activator.CreateInstance(Bases.Item(TosserType))
            Catch ex As KeyNotFoundException
                Console.WriteLine("Module TosserType=" & TosserType & " not implemented!")
                Exit Sub
            End Try

            With mdx
                .InboundDir = sInboundDir
                .BasePath = sBasePath
                .PointName = sPointName
                Call .TossInto()
            End With

        End If

    End Sub

End Module
