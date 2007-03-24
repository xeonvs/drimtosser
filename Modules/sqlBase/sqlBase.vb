Option Explicit On

Imports DRIMTCore.Utils
Imports DRIMTCore.PKT
Imports Npgsql
Imports System.Data
Imports System.IO
Imports System.Text

Public Class Tosser
    Implements IModule

    Private sPointName As String = ""
    Private sBasePath As String = ""
    Private sInboundDir As String = ""
    Private strEchoListFileName As String
    Private TotalMessCount As Integer
    Private lngNetMailCount As Integer
    Private lngCarbonCopyCount As Integer
    Private PKThdr As New PKThdr20Type
    Private sReciptTime As String

    Private sqlConn As NpgsqlConnection
    Private dsAreas As New DataSet

    Private setMessage As New NpgsqlCommand("""StoreMessage""(:fromN, :fromA, :toN, :toA, :echo, :subj, :msgtxt, :date, :attr, :aka, :msgid);")
    Private fromNParam As New NpgsqlParameter("fromN", DbType.String)
    Private fromAParam As New NpgsqlParameter("fromA", DbType.String)
    Private toNParam As New NpgsqlParameter("toN", DbType.String)
    Private toAParam As New NpgsqlParameter("toA", DbType.String)
    Private echoNameParam As New NpgsqlParameter("echo", DbType.String)
    Private subjParam As New NpgsqlParameter("subj", DbType.String)
    Private msgTextParam As New NpgsqlParameter("msgtxt", DbType.String)
    Private dateParam As New NpgsqlParameter("date", DbType.String)
    Private attrParam As New NpgsqlParameter("attr", DbType.String)
    Private akaParam As New NpgsqlParameter("aka", DbType.String)
    Private msgidParam As New NpgsqlParameter("msgid", DbType.String)

    ''' <summary>
    ''' Тип базы поддерживаемый данным модулем тоссера
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property BaseType() As IDatabasesTypes.enmBaseType Implements IModule.BaseType
        Get
            Return IDatabasesTypes.enmBaseType.SQL
        End Get
    End Property

    ''' <summary>
    ''' Тоссит пакеты в Inbound
    ''' </summary>
    ''' <remarks>В inbound уже должны находится *.pkt</remarks>
    Public Sub Toss() Implements IModule.TossInto
        Dim t, T1, fc As Integer
        Dim FileCount As Integer
        Dim sBaloonInfoString As String
        Dim di As DirectoryInfo

        Try
            di = New DirectoryInfo(sInboundDir)
        Catch dnf As DirectoryNotFoundException
            Console.WriteLine("Путь: " & sInboundDir & " не существует или не найден")
            Exit Sub
        End Try

        Try
            sqlConn = New NpgsqlConnection(sBasePath)
            sqlConn.Open()
        Catch ex As Exception
            SaveLog("Ошибка подключение к SQL серверу!" & vbCrLf & ex.Message)
            Exit Sub
        End Try

        If Not Me.LoadAreas() Then
            CreateArea("NETMAIL", "Netmail Area")
        End If

        Console.WriteLine("Start Tossing ...")

        RaiseEvent TossingProgress(Me, 0)

        t = Environment.TickCount()
        Try
            fc = di.GetFiles("*.pkt").Length
            For Each fln As FileInfo In di.GetFiles("*.pkt")
                Console.WriteLine("Processing file..." & fln.Name)
                OpenPkt(fln.FullName)
                If OpenNextFile(fln.FullName) Then
                    fln.Delete()
                    RaiseEvent TossingProgress(Me, FileCount \ fc * 100)
                    'If blnStopToss Then
                    'SaveLog(Now & "Тоссинг отменён пользователем..." & vbCrLf & "==========================" & vbCrLf)
                    'Exit Sub
                    'End If
                    FileCount += 1
                End If
            Next

        Catch dnf As DirectoryNotFoundException
            Console.WriteLine("Путь: " & sInboundDir & " не существует или не найден")
        End Try

        T1 = Environment.TickCount()

        RaiseEvent TossingProgress(Me, 100)

        Console.WriteLine("Тоссинг закончен")
        Console.WriteLine("Обработанные пакеты")

        'Console.WriteLine(TotalMessCount & " сообщений за " & (T1 - T) / 1000 & "сек.")
        Console.WriteLine("Всего сообщений : " & TotalMessCount)
        Console.WriteLine("Личных : " & lngNetMailCount)
        Console.WriteLine("Копий  : " & lngCarbonCopyCount)
        Console.WriteLine("==========================")
        sBaloonInfoString = "Всего сообщений : " & TotalMessCount & vbCrLf & "Личных : " & lngNetMailCount & vbCrLf & "Копий  : " & lngCarbonCopyCount

        SaveLog(Now & vbTab & sBaloonInfoString & vbCrLf & "==========================" & vbCrLf)

        If FileCount > 0 Then
            Console.WriteLine(TotalMessCount & " сообщений за " & (T1 - t) / 1000 & "сек.")
            Console.WriteLine("Average: " & System.Math.Round(TotalMessCount / ((T1 - t) / 1000), 2) & " mess/sec")
            SaveLog(TotalMessCount & " сообщений за " & (T1 - t) / 1000 & "сек." & vbCrLf & "Average: " & System.Math.Round(TotalMessCount / ((T1 - t) / 1000), 2) & " mess/sec" & vbCrLf & "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -" & vbCrLf & vbCrLf)
        End If

        sqlConn.Close()

    End Sub

    ''' <summary>
    ''' Сканирует базу на предмет новых сообщений
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Scan() Implements IModule.ScanBase
        '
    End Sub

    Public Event TossingProgress(ByVal sender As Object, ByVal Percentage As Integer) Implements IModule.TossingProgress

    Public Property PointName() As String Implements IModule.PointName
        Get
            Return sPointName
        End Get
        Set(ByVal value As String)
            sPointName = value
        End Set
    End Property
    Public Property BasePath() As String Implements IModule.BasePath
        Get
            Return sBasePath
        End Get
        Set(ByVal value As String)
            sBasePath = value.Replace("|", ";")
        End Set
    End Property
    Public Property InboundDir() As String Implements IModule.InboundDir
        Get
            Return sInboundDir
        End Get
        Set(ByVal value As String)
            sInboundDir = value
        End Set
    End Property
    Public Property EchoListFileName() As String Implements IModule.EchoListFileName
        Get
            Return strEchoListFileName
        End Get
        Set(ByVal value As String)
            strEchoListFileName = value
        End Set
    End Property

    Private Function LoadAreas() As Boolean
        Dim da As New NpgsqlDataAdapter("SELECT *  FROM ""Areas"";", sqlConn)
        da.Fill(dsAreas)
        If dsAreas.Tables(0).Rows.Count <> 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub CreateArea(ByVal strAreaName As String, ByVal strAreaDescription As String)
        Try
            Dim Command As New NpgsqlCommand("INSERT INTO ""Areas""( ""AreaName"", ""AKA"", ""Description"") VALUES (:name, :aka, :desc);", sqlConn)
            Command.Parameters.Add(New NpgsqlParameter("name", DbType.String))
            Command.Parameters.Add(New NpgsqlParameter("aka", DbType.String))
            Command.Parameters.Add(New NpgsqlParameter("desc", DbType.String))
            Command.Parameters(0).Value = strAreaName
            Command.Parameters(1).Value = "0:0/0.0"
            Command.Parameters(2).Value = strAreaDescription
            Command.ExecuteNonQuery()

            Console.WriteLine("Created area " & strAreaName & " ...")
            SaveLog(Now & vbTab & "Created area " & strAreaName & " ...")

            SaveEventMessage("DRIM Tosser", _
                             "Created new Area...", _
                             "Создана новая конференция " & strAreaName & " ..." & vbCr & _
                             "_Проверьте настройки конференции._" & vbCr & _
                             "Если вы используете пользовательские настройки списка конференций, " & vbCr & _
                             "то для её отображения добавьте её в пользовательский список вручную.", "NETMAIL", "0:0/0.0")
        Catch ex As Exception
            '
        End Try
    End Sub

    Private Sub SaveEventMessage(ByVal strFromName As String, ByVal strSabj As String, ByVal sText As String, ByVal sAreaName As String, ByVal sysOpAka As String)
        Try
            With setMessage
                .Connection = sqlConn
                .CommandType = CommandType.StoredProcedure
                .Parameters.Add(fromNParam)     '0
                .Parameters.Add(fromAParam)     '1
                .Parameters.Add(toNParam)       '2
                .Parameters.Add(toAParam)       '3
                .Parameters.Add(echoNameParam)  '4
                .Parameters.Add(subjParam)      '5
                .Parameters.Add(msgTextParam)   '6
                .Parameters.Add(dateParam)      '7
                .Parameters.Add(attrParam)      '8
                .Parameters.Add(akaParam)       '9
                .Parameters.Add(msgidParam)     '10
                .Parameters(0).Value = strFromName
                .Parameters(1).Value = sysOpAka
                .Parameters(2).Value = sPointName
                .Parameters(3).Value = sysOpAka
                .Parameters(4).Value = sAreaName
                .Parameters(5).Value = strSabj
                .Parameters(6).Value = sText
                .Parameters(7).Value = Now
                .Parameters(8).Value = "0"
                .Parameters(9).Value = sysOpAka
                .Parameters(10).Value = ""
                .ExecuteNonQuery()
            End With

        Catch ex As Exception

        End Try
    End Sub

    Private Sub OpenPkt(ByRef sPath As String)
        Dim fs As New FileStream(sPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Dim br As New BinaryReader(fs)

        PKThdr = PKThdr.FromBinaryReaderBlock(br)
        br.Close()
        fs.Close()
        fs.Dispose()

        sReciptTime = CStr(PKThdr.Day) & "." & _
                      CStr(PKThdr.Month) & "." & _
                      CStr(PKThdr.Year) & " " & _
                      CStr(PKThdr.Hour) & ":" & _
                      CStr(PKThdr.Minute) & ":" & _
                      CStr(PKThdr.Second)

    End Sub

    Private Function OpenNextFile(ByVal sFName As String) As Boolean
        Dim i, ArrMax As Integer
        Dim nPacket As New Packet

        If Trim(sFName).Length = 0 Then Exit Function

        SaveLog(Now & vbTab & sFName)

        PktTossPacket(sFName)
        ArrMax = UBound(Messages) 'nPacket.Records

        For i = 1 To ArrMax
            'System.Windows.Forms.Application.DoEvents()
            If Not ComposeMessage(i) Then
                Return False                
            End If
            'System.Windows.Forms.Application.DoEvents()
        Next i

        Return True

    End Function

    Private Function ComposeMessage(ByVal lMessIndex As Integer) As Boolean
        Dim strArea, strFromName, strData, strToName, strSabj, strTemp As String
        Dim strFromAdress As String = "", strToAdress As String = ""
        Dim tmpBody() As String, strText As String = "", strMsgid As String = ""

        With Messages(lMessIndex).Hdr
            strData = .Date
            strToName = Trim(.ToName)
            strFromName = Trim(.FromName)
            strSabj = RTrim(.Subject)
        End With

        tmpBody = CType(Messages(lMessIndex).Msg.Clone, String())

        If Strings.Left(tmpBody(0), 5) <> "AREA:" Then
            strArea = "NETMAIL"
            lngNetMailCount = lngNetMailCount + 1
        Else
            strArea = CutOfNullChar(Replace(tmpBody(0), "AREA:", vbNullString))
        End If


        For i As Integer = CInt(IIf(strArea = "NETMAIL", 0, 1)) To UBound(tmpBody)
            If Strings.Left(tmpBody(i), 7) = Chr(1) & "MSGID:" Then
                strFromAdress = Replace(tmpBody(i), Chr(1) & "MSGID:", vbNullString)
                strMsgid = "MSGID:" & strFromAdress
                strFromAdress = Strings.Left(LTrim(strFromAdress), InStr(1, LTrim(strFromAdress), Chr(32)))
                strFromAdress = Trim(Replace(UCase(strFromAdress), "@FIDONET", vbNullString))                
            End If

            If Strings.Left(tmpBody(i), 7) = Chr(1) & "REPLY:" Then
                strToAdress = Replace(tmpBody(i), Chr(1) & "REPLY:", vbNullString)
                strToAdress = Strings.Left(LTrim(strToAdress), InStr(1, LTrim(strToAdress), Chr(32)))
                strToAdress = Trim(Replace(UCase(strToAdress), "@FIDONET", vbNullString))
            End If

            strText = strText & tmpBody(i) & vbCr
        Next

        With setMessage
            .Connection = sqlConn
            .CommandType = CommandType.StoredProcedure
            .Parameters.Add(fromNParam)     '0
            .Parameters.Add(fromAParam)     '1
            .Parameters.Add(toNParam)       '2
            .Parameters.Add(toAParam)       '3
            .Parameters.Add(echoNameParam)  '4
            .Parameters.Add(subjParam)      '5
            .Parameters.Add(msgTextParam)   '6
            .Parameters.Add(dateParam)      '7
            .Parameters.Add(attrParam)      '8
            .Parameters.Add(akaParam)       '9
            .Parameters.Add(msgidParam)     '10
            .Parameters(0).Value = strFromName
            .Parameters(1).Value = strFromAdress
            .Parameters(2).Value = strToName
            .Parameters(3).Value = strToAdress
            .Parameters(4).Value = strArea
            .Parameters(5).Value = strSabj
            .Parameters(6).Value = strText
            .Parameters(7).Value = strData
            .Parameters(8).Value = "0"
            .Parameters(9).Value = Messages(lMessIndex).Pkt.GetFromAddr
            .Parameters(10).Value = strMsgid

            Try
                .ExecuteNonQuery()
            Catch ex As Exception
                Return False
            End Try

            TotalMessCount = TotalMessCount + 1
            Return True
        End With

    End Function

#Region "ModuleInfo"
    Public Function GetModuleIcon() As System.Drawing.Image Implements IModule.GetModuleIcon
        Return My.Resources.ModuleIcon.ToBitmap
    End Function

    Public ReadOnly Property Description() As String Implements IModuleInfo.Description
        Get
            Return "PostgreSQL Toss Module"
        End Get
    End Property

    Public ReadOnly Property Name() As String Implements IModuleInfo.Name
        Get
            Return "sqlTosser"
        End Get
    End Property
#End Region
#Region " IDisposable Support "
    Private disposed As Boolean = False

    ' IDisposable
    Private Overloads Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposed Then
            If disposing Then
                ' TODO: put code to dispose managed resources
            End If

            ' TODO: put code to free unmanaged resources here
        End If
        Me.disposed = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(False)
        MyBase.Finalize()
    End Sub
#End Region

End Class
