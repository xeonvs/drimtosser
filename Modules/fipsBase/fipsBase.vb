Option Explicit On

Imports VB = Microsoft.VisualBasic
Imports System.Runtime.InteropServices
Imports System.IO
Imports DRIMTCore.Utils
Imports DRIMTCore.PKT

Public Class Tosser
    Implements IModule

    Private sPointName As String = ""
    Private sBasePath As String = ""
    Private sInboundDir As String = ""
    Private strEchoListFileName As String

    ''' <summary>
    ''' Тип базы поддерживаемый данным модулем тоссера
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property BaseType() As IDatabasesTypes.enmBaseType Implements IModule.BaseType
        Get
            Return IDatabasesTypes.enmBaseType.FIPS
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

        Me.LoadEchoList(strEchoListFileName)
        If Trim(sBasePath).Length > 0 Then
            If Not Me.LoadAreas(sBasePath) Then
                Me.CreateBaseAreas()
            End If
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
            sBasePath = value
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

#Region "Fips Bases functions"
#Region "Structures"
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Private Structure AreaRecord
        Dim index As Integer 'Порядковый номер области в списке (уникален)
        Dim StructLen As Integer 'Длина записи (всегда равна 562)
        Dim Status As Integer 'Статус области
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=129)> Public Description As String 'Описание области
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=101)> Public Echotag As String 'Название области
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)> Public FileName As String 'Имя файла заголовков и писем без расширения
        Dim LastRead As Integer 'Индекс последнего прочитанного письма
        Dim Hheader As Integer 'Указатель на файл заголовков (используется во время работы программы)
        Dim Hmessage As Integer 'Указатель на файл базы писем (используется во время работы программы)
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public AKA As String 'Ваш АКА для этой области
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=31)> Public UpLink As String 'Ваш аплинк для этой области
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=12)> Public Group As String 'Группа, к которой относится эта область
        Dim Membership As Integer '
        Dim PurgeAgeCreate As Integer 'Данные, используемые пуржером при работе
        Dim PurgeAgeRecipt As Integer
        Dim PurgeMaxNum As Integer
        Dim PurgeFlagCreate As Integer
        Dim PurgeFlagAgeRecipt As Integer
        Dim PurgeFlagNrmails As Integer
        Dim NumberOfMails As Integer 'Число сообщений в базе писем этой области
        Dim NumberOfAlreadyRead As Integer 'Число уже прочитанных сообщений
        Dim LocalMail As Integer 'Флаг неэкспортируемой области
        Dim AdditionalDays As Integer 'Дополнительное число дней для непрочитанных писем, используемое пуржером для автоматической чистки базы
        Dim AreaCreationDate As Integer 'Дата создания области
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=67)> Dim NoName() As Byte 'Внутренние неспользуемые данные

        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(AreaRecord))) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As AreaRecord
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(AreaRecord)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As AreaRecord = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(AreaRecord)), AreaRecord)
            handle.Free()
            Return st
        End Function

        'Длина одной записи = 562
        Public Function Length() As Integer
            Return 562
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Private Structure SabjRecord
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=72)> Public Name As String 'Поле "Тема" заголовка письма
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=20)> Public DateTime As String 'Дата и время создания письма в виде символьной строки в формате, принятом в сети FidoNet
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=36)> Public ToName As String 'Поле "Кому" заголовка письма
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=36)> Public FromName As String 'Поле "От кого" заголовка письма
        Dim StrucLen As Integer 'Размер структуры файла заголовков и должен быть равен 238
        Dim Status As Integer ' Статус письма (см. константы статуса письма)
        Dim MailID As Integer ' Идентификатор письма, используемый тоссером
        Dim ReciptTime As Integer 'Дата и время тоссинга
        Dim offset As Integer 'Смещение  первого символа текста сообщения (кладж MagicID) в файле базы писем
        Dim TextLen As Integer 'Длина текста сообщения в файле базы писем (включая все кладжи)
        Dim index As Integer 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
        Dim Filler As Short 'Пустое поле
        Dim Attrib As Short 'Атрибуты письма (см. константы атрибутов письма)
        Dim cost As Short 'Пустое поле
        Dim OrigZone As Short 'Номер зоны отправителя (обычно 2)
        Dim OrigNet As Short 'Номер сети отправителя
        Dim OrigNode As Short 'Номер узла отправителя
        Dim OrigPoint As Short 'Номер поинта отправителя
        Dim DestZone As Short 'Номер зоны получателя (обычно 2)
        Dim DestNet As Short 'Номер сети получателя
        Dim DestNode As Short 'Номер узла получателя
        Dim DestPoint As Short 'Номер поинта получателя
        Dim ReplyID As Integer 'Идентификатор ответа  ReplyID (устанавливается и используется тоссером)
        Dim NestLevel As Integer 'Временное поле, используемое для построения связанного списка ответов
        Dim UnixTime As Integer 'Время создания письма в формате, принятом в UNIX
        Dim ZoneNet As Integer 'Старшие 16 бит содержат номер зоны, младшие 16 бит - номер сети получателя
        Dim node As Integer 'Номер узла получателя (для тех писем, у которых установлен флаг письма DB_Mail_Route_to_Boss)
        Dim MailText As Integer 'String * 4 'Указатель на начало текста (может быть nil)

        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(SabjRecord)) - 1) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As SabjRecord
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(SabjRecord)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As SabjRecord = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(SabjRecord)), SabjRecord)
            handle.Free()
            Return st
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Private Structure MessRecord
        'MagicID(15) As Byte 'Это идентификатор начала заголовка нового сообщения, представляет собой
        'массив из 16 байт, содержащий постоянную последовательность
        '0xfe, 0xaf, 0xfe, 0xaf, 0xfe, 0xaf, 0xfe, 0xaf, 0x04, 0x03, 0x02, 0x01, 0x01, 0x02, 0x03, 0x04
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=15)> Public MagicID() As Byte
        Dim version As Integer 'Номер версии формата базы и должен быть равен 0х01
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=20)> Public Echotag As String 'Название области
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=72)> Public Subject As String 'Поле "Тема" заголовка письма
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=20)> Public datetime As String 'Дата и время создания письма в виде символьной строки в формате, принятом в сети FidoNet
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=36)> Public toname As String 'Поле "Кому" заголовка письма
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=36)> Public fromname As String 'Поле "От кого" заголовка письма
        Dim StrucLen As Integer 'Размер структуры файла заголовков и должен быть равен 238
        Dim Status As Integer 'Статус письма (см. константы статуса письма)
        Dim MailID As Integer 'Идентификатор письма, используемый тосеером
        Dim ReciptTime As Integer 'Дата и время тоссинга
        Dim offset As Integer 'Смещение  первого символа текста сообщения (кладж MagicID) в файле базы писем
        Dim TextLen As Integer 'Длина текста сообщения в файле базы писем (включая все кладжи)
        Dim index As Integer 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
        Dim Filler As Short 'Пустое поле
        Dim Attrib As Short 'Атрибуты письма (см. константы аттрибутов письма)
        Dim cost As Short 'Пустое поле
        Dim OrigZone As Short 'Номер зоны отправителя (обычно 2)
        Dim OrigNet As Short 'Номер сети отправителя
        Dim OrigNode As Short 'Номер узла отправителя
        Dim OrigPoint As Short 'Номер поинта отправителя
        Dim DestZone As Short 'Номер зоны получателя (обычно 2)
        Dim DestNet As Short 'Номер сети получателя
        Dim DestNode As Short 'Номер узла получателя
        Dim DestPoint As Short 'Номер поинта получателя
        Dim ReplyID As Integer 'Идентификатор ответа  ReplyID (устанавливается и используется тоссером)
        Dim NestLevel As Integer 'Временное поле, используемое для построения связанного списка ответов
        Dim UnixTime As Integer 'Время создания письма в формате, принятом в UNIX
        Dim ZoneNet As Integer 'Старшие 16 бит содержат номер зоны, младшие 16 бит - номер сети получателя
        Dim node As Integer 'Номер узла получателя (для тех писем, у которых установлен флаг письма DB_Mail_Route_to_Boss)
        Dim MailText As Integer 'Указатель на начало текста (может быть nil)

        Public Sub Initialize()
            ReDim MagicID(15)
        End Sub

        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(MessRecord)) - 1) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As MessRecord
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(SabjRecord)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As MessRecord = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(MessRecord)), MessRecord)
            handle.Free()
            Return st
        End Function
    End Structure

    Private Enum MessStatus
        stDB_DELETED = &H1 'Письмо помечено на удаление
        stDB_MARKED_FOR_PURGE = &H2 'Письмо помечено для пурженья
        stDB_AREA_LOCKED = &H4 'Область заблокирована
        stDB_MAIL_STATUS_READ = &H8 'Письмо прочитано
        stDB_AREA_MODIFIED = &H10 'Область необходимо пересканировать на предмет обнаружения новых сообщений
        stDB_MAIL_STATUS_CREATED_MAIL = &H20 'Созданное сообщение
        stDB_MAIL_STATUS_SCANNED = &H40S 'Письмо просканировано
        stDB_MAIL_NEVER_DELETE = &H80 'Письмо помечено как неудаляемое
        stDB_MAIL_ROUTE_TO_BOSS = &H100 'Отправить письмо прямо боссу
        stDB_DUPE_MAIL = &H200 'Это копия имеющегося письма (дуп)
        stDB_FROZEN_MAIL = &H400 'Письмо помечено как "Отложенное"
        stDB_USERMARKED = &H800 'Письмо помечено пользователем
        stDB_ANSWERED = &H1000 'На это письмо есть ответ
        stDB_CONVERTED = &H2000 'Письмо было сконвертировано
    End Enum

    Private Enum MessAttrib
        atMSGPRIVATE = &H1 'Частное письмо
        atMSGCRASH = &H2 'Отправить немедленно
        atMSGRECD = &H4 'Письмо получено
        atMSGSENT = &H8 'Отправлено удачно
        atMSGFILE = &H10 'К письму прикреплен аттач
        atMSGFWD = &H20 'Письмо было отфорваржено
        atMSGORPHAN = &H40 'Неизвестна нода-получатель
        atMSGKILL = &H80 'Удалить после отправки
        atMSGLOCAL = &H100 'Сообщение написано на Вашей станции
        atMSGXX1 = &H200 '
        atMSGXX2 = &H400 '
        atMSGFRQ = &H800 'Файловый запрос
        atMSGRRQ = &H1000 'Запрос принят
        atMSGCPT = &H2000 'Письмо - подтверждение приема
        atMSGARQ = &H4000 'Запрос пути следования
        atMSGURQ = &H8000 'Обновление запроса
    End Enum


#End Region

    Private arrTossMessText() As String
    Private arrTossMessHdr() As SabjRecord
    Private arrAreasToss() As AreaRecord
    Private PKThdr As New PKThdr20Type
    Private colAreas As New Collection
    Private iMessStatus As Integer
    Private iMessAttrib As Integer
    Private TotalMessCount As Integer
    Private lngNetMailCount As Integer
    Private lngCarbonCopyCount As Integer
    Private sReciptTime As String
    Private MsgArray() As String

    Private Function GetFreeAreaIndex(Optional ByRef BaseArea As Boolean = False) As Integer
        Dim i, num As Integer
        If BaseArea = False Then
            num = 3
        Else
            num = 0
        End If

Begin:
        For i = LBound(arrAreasToss) To UBound(arrAreasToss)
            If CInt(Val(CStr(arrAreasToss(i).index))) = num Then
                num = num + 1
                GoTo Begin
                Exit For
            End If
        Next

        Return num

    End Function

    Private Sub SaveEventMessage(ByVal strFromName As String, ByVal strSabj As String, ByVal sText As String, ByVal sAreaName As String)
        Dim TossMess As New SabjRecord
        Dim TossMsgText As String = "", tmpText As String = "", strTemp As String = ""
        Dim strData, strToName As String
        Dim strArea As String = ""
        Dim NewSabj As SabjRecord, NewMess As New MessRecord, tmpAreaRec As New AreaRecord
        Dim NewIndex, lAreaIndex As Integer

        strData = Format(Now, "dd mmm yy hh:mm:ss")

        tmpAreaRec = arrAreasToss(GetAreaByName(sAreaName))
        NewIndex = tmpAreaRec.NumberOfMails + 1

        strToName = sPointName
        iMessStatus = ReSetByteSelected(iMessStatus, MessStatus.stDB_MAIL_STATUS_READ)
        iMessStatus = SetByteSelected(iMessStatus, MessStatus.stDB_MAIL_STATUS_SCANNED)
        iMessAttrib = SetMessStatus(iMessAttrib, MessAttrib.atMSGLOCAL)

        With NewSabj 'заполним поля сообщения
            .Name = strSabj & vbNullChar 'Поле "Тема" заголовка письма
            .DateTime = strData & vbNullChar 'Дата и время создания письма в виде символьной строки в формате, принятом в сети FidoNet
            .ToName = strToName & vbNullChar 'Поле "Кому" заголовка письма
            .FromName = strFromName & vbNullChar 'Поле "От кого" заголовка письма
            .StrucLen = 238 'Размер структуры файла заголовков и должен быть равен 238
            .Status = iMessStatus 'Статус письма
            .Attrib = CShort(iMessAttrib) 'Атрибуты письма
            .MailID = ParseStdTime(CStr(Now))
            .ReciptTime = ParseStdTime(CStr(Now)) '.MailID 'DateDiff("s", CDate(Format("01.01.1970 00:00:00", "dd.mm.yy hh:mm:ss")), DateAdd("h", 0, Now))
            .offset = CInt(My.Computer.FileSystem.GetFileInfo(sBasePath & CutOfNullChar(tmpAreaRec.FileName) & ".mes").Length) 'Смещение в файле сообщений первого символа сообщения
            .TextLen = sText.Length  'Длина текста сообщения в файле базы писем (включая все кладжи)
            .index = NewIndex - 1 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
            'If UCase$(ConvOemToAnsi(m_Area.Echotag)) <> "NETMAIL" Then
            .OrigZone = 0 'GetAdressElement(sAKA, iZONE) ' 2 'Номер зоны отправителя (обычно 2)
            .OrigNet = 0 'GetAdressElement(sAKA, iNET) '5015 'Номер сети отправителя
            .OrigNode = 0 'GetAdressElement(sAKA, iNode) '112 'Номер узла отправителя
            .OrigPoint = 0 'GetAdressElement(sAKA, iPoint) 'intPoint ' 35 'Номер поинта отправителя
            .DestNet = 0 'GetAdressElement(sUpLink, iNET) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNet, GetAdressElement(sUpLink, iNET))
            .DestNode = 0 'GetAdressElement(sUpLink, iNode) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNode, GetAdressElement(sUpLink, iNode))
            .DestPoint = 0 'GetAdressElement(sUpLink, iPoint) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigPoint, GetAdressElement(sUpLink, iPoint))
            .DestZone = 0 'GetAdressElement(sUpLink, iZONE) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigZone, GetAdressElement(sUpLink, iZONE))
            'Else
            '    .OrigZone = GetAdressElement(sPointAdress, iZONE) ' 2 'Номер зоны отправителя (обычно 2)
            '    .OrigNet = GetAdressElement(sPointAdress, iNET) '5015 'Номер сети отправителя
            '    .OrigNode = GetAdressElement(sPointAdress, iNode) '112 'Номер узла отправителя
            '    .OrigPoint = GetAdressElement(sPointAdress, iPoint) 'intPoint ' 35 'Номер поинта отправителя
            '    .DestNet = GetAdressElement(sToAdress, iNET) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNet, GetAdressElement(sUpLink, iNET))
            '    .DestNode = GetAdressElement(sToAdress, iNode) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNode, GetAdressElement(sUpLink, iNode))
            '    .DestPoint = GetAdressElement(sToAdress, iPoint) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigPoint, GetAdressElement(sUpLink, iPoint))
            '    .DestZone = GetAdressElement(sToAdress, iZONE) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigZone, GetAdressElement(sUpLink, iZONE))
            'End If
            .ReplyID = 0 'Установим идентификатор письма на которое отвечаем
            .NestLevel = 0 ' не будем заполнять - это внутреннее поле используемое в ФИПС
            .UnixTime = .MailID '.ReciptTime 'Время создания письма в формате, принятом в UNIX
        End With


        With NewMess 'заолним поля сообщения
            .Initialize()
            .MagicID(0) = 254
            .MagicID(1) = 175
            .MagicID(2) = 254
            .MagicID(3) = 175
            .MagicID(4) = 254
            .MagicID(5) = 175
            .MagicID(6) = 254
            .MagicID(7) = 175
            .MagicID(8) = 4
            .MagicID(9) = 3
            .MagicID(10) = 2
            .MagicID(11) = 1
            .MagicID(12) = 1
            .MagicID(13) = 2
            .MagicID(14) = 3
            .MagicID(15) = 4
            .version = &H1 'Номер версии формата базы и должен быть равен 0х01
            .Echotag = tmpAreaRec.Echotag & vbNullChar 'Название области
            '        .Sabject = sSabj 'Поле "Тема" заголовка письма
            .Subject = strSabj & vbNullChar 'Поле "Тема" заголовка письма
            .datetime = strData & vbNullChar 'Дата и время создания письма в виде символьной строки в формате, принятом в сети FidoNet
            '        .ToName = sToName 'Поле "Кому" заголовка письма
            .toname = strToName & vbNullChar 'Поле "Кому" заголовка письма
            '        .FromName = sFromName 'Поле "От кого" заголовка письма
            .fromname = strFromName & vbNullChar 'Поле "От кого" заголовка письма
            .StrucLen = 238 'Размер структуры файла заголовков и должен быть равен 238
            .Status = iMessStatus 'установим статус сообщения
            .Attrib = CShort(iMessAttrib) 'установим атрибуты сообщения
            .MailID = NewSabj.MailID 'DateDiff("s", CDate(Format("01.01.1970 00:00:00", "dd.mm.yy hh:mm:ss")), Now) 'Идентификатор письма, используемый тосеером
            .ReplyID = 0 'установим идентификатор сообщения на которое отвечаем
            .ReciptTime = ParseStdTime(CStr(Now)) '.MailID 'DateDiff("s", CDate(Format("01.01.1970 00:00:00", "dd.mm.yy hh:mm:ss")), DateAdd("h", 0, Now)) '-frmReadAreas.GetTimeShift("2:5015/112.35"), Now))
            .offset = CInt(My.Computer.FileSystem.GetFileInfo(sBasePath & CutOfNullChar(tmpAreaRec.FileName) & ".mes").Length) 'Смещение  первого символа текста сообщения (кладж MagicID) в файле базы писем
            .TextLen = sText.Length  'Длина текста сообщения в файле базы писем (включая все кладжи)
            .index = NewIndex - 1 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
            '        If UCase$(ConvOemToAnsi(m_Area.Echotag)) <> "NETMAIL" Then
            .OrigZone = 0 'GetAdressElement(sAKA, iZONE) ' 2 'Номер зоны отправителя (обычно 2)
            .OrigNet = 0 'GetAdressElement(sAKA, iNET) '5015 'Номер сети отправителя
            .OrigNode = 0 'GetAdressElement(sAKA, iNode) '112 'Номер узла отправителя
            .OrigPoint = 0 'GetAdressElement(sAKA, iPoint) 'intPoint ' 35 'Номер поинта отправителя
            .DestNet = 0 'GetAdressElement(sUpLink, iNET) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNet, GetAdressElement(sUpLink, iNET))
            .DestNode = 0 'GetAdressElement(sUpLink, iNode) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNode, GetAdressElement(sUpLink, iNode))
            .DestPoint = 0 'GetAdressElement(sUpLink, iPoint) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigPoint, GetAdressElement(sUpLink, iPoint))
            .DestZone = 0 'GetAdressElement(sUpLink, iZONE) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigZone, GetAdressElement(sUpLink, iZONE))'        Else
            '            .OrigZone = GetAdressElement(sPointAdress, iZONE) ' 2 'Номер зоны отправителя (обычно 2)
            '            .OrigNet = GetAdressElement(sPointAdress, iNET) '5015 'Номер сети отправителя
            '            .OrigNode = GetAdressElement(sPointAdress, iNode) '112 'Номер узла отправителя
            '            .OrigPoint = GetAdressElement(sPointAdress, iPoint) 'intPoint ' 35 'Номер поинта отправителя
            '            .DestNet = GetAdressElement(sToAdress, iNET) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNet, GetAdressElement(sUpLink, iNET))
            '            .DestNode = GetAdressElement(sToAdress, iNode) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNode, GetAdressElement(sUpLink, iNode))
            '            .DestPoint = GetAdressElement(sToAdress, iPoint) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigPoint, GetAdressElement(sUpLink, iPoint))
            '            .DestZone = GetAdressElement(sToAdress, iZONE) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigZone, GetAdressElement(sUpLink, iZONE))
            '        End If
            .UnixTime = .MailID 'Время создания письма в формате, принятом в UNIX
        End With

        If SaveMessageEx(arrAreasToss(GetAreaByName(sAreaName)), NewSabj, NewMess, sText) Then
            arrAreasToss(GetAreaByName(sAreaName)).NumberOfMails = arrAreasToss(GetAreaByName(sAreaName)).NumberOfMails + 1
            Dim fsA As New FileStream(sBasePath & "areas.wwd", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            Dim bwA As New BinaryWriter(fsA), buff() As Byte = arrAreasToss(GetAreaByName(sAreaName)).ToByteArray

            fsA.Seek(CInt(IIf(lAreaIndex > 0, lAreaIndex, 1)) * 562 - 562, SeekOrigin.Begin) 'позиционируемся на нужную запись

            With bwA
                .Write(buff) 'записываем
                .Flush()
                .Close()
            End With
            fsA.Close() 'закрываем файл

        End If

    End Sub

    Public Sub CreateArea(ByVal strAreaName As String, Optional ByRef LocalMail As Integer = 0, Optional ByRef BaseArea As Boolean = False, Optional ByRef BaseAreaIndex As Integer = 0)
        Dim tmpAreaRec As New AreaRecord
        Dim strTmpFileName, strMessText As String
        Dim fs As FileStream

        If Not BaseArea Then
            ReDim Preserve arrAreasToss(UBound(arrAreasToss) + 1)
            tmpAreaRec = arrAreasToss(UBound(arrAreasToss))
        Else
            tmpAreaRec = arrAreasToss(BaseAreaIndex + 1)
        End If

        With tmpAreaRec
            .AdditionalDays = 0
            .AKA = PKThdr.DestZone & ":" & PKThdr.DestNet & "/" & PKThdr.DestNode & "." & PKThdr.DestPoint & vbNullChar
            .AreaCreationDate = CInt(Now.Subtract(dtUNIX_DATE).TotalSeconds)
            .Description = strGetAreaDescription(strAreaName)
            .Echotag = strAreaName & vbNullChar
            .index = CInt(IIf(Not BaseArea, GetFreeAreaIndex, BaseAreaIndex))
            strTmpFileName = Format(.index, "00000000") 'String$(8 - Len(CStr(.index)), "0") & CStr(.index)
            .FileName = strTmpFileName & vbNullChar
            .Group = "FIDO" & vbNullChar
            .LastRead = 0
            .LocalMail = LocalMail
            .Membership = 0
            .NumberOfAlreadyRead = 0
            .NumberOfMails = 0
            .PurgeAgeCreate = 0
            .PurgeAgeRecipt = 0
            .PurgeFlagAgeRecipt = 0
            .PurgeFlagCreate = 0
            .PurgeFlagNrmails = 0
            .PurgeMaxNum = 0
            .Status = 0
            .StructLen = 562
            .UpLink = PKThdr.DestZone & ":" & PKThdr.DestNet & "/" & PKThdr.DestNode & vbNullChar
        End With

        If Not BaseArea Then
            arrAreasToss(UBound(arrAreasToss)) = tmpAreaRec
        Else
            arrAreasToss(BaseAreaIndex + 1) = tmpAreaRec
        End If

        If Not My.Computer.FileSystem.FileExists(sBasePath & strTmpFileName & ".hdr") Then
            fs = New FileStream(sBasePath & strTmpFileName & ".hdr", FileMode.CreateNew)
            fs.Close()
            fs.Dispose()
        End If

        If Not My.Computer.FileSystem.FileExists(sBasePath & strTmpFileName & ".mes") Then
            fs = New FileStream(sBasePath & strTmpFileName & ".mes", FileMode.CreateNew)
            fs.Close()
            fs.Dispose()
        End If

        Console.WriteLine("Created area " & strAreaName & " ...")
        SaveLog(Now & vbTab & "Created area " & strAreaName & " ...")

        If Not BaseArea Then
            strMessText = "Создана новая конференция " & strAreaName & " ..." & vbCr & "_Проверьте настройки конференции._" & vbCr & "Если вы используете пользовательские настройки списка конференций, " & vbCr & "то для её отображения добавьте её в пользовательский список вручную."
            SaveEventMessage("DRIM Tosser", "Created new Area...", strMessText, "NETMAIL")
        End If

    End Sub

    Private Function ComposeMessage(ByVal lMessIndex As Integer) As Boolean
        Dim strToAdres As String
        Dim TossMess As New SabjRecord
        Dim tmpText As String = "", strTemp As String
        Dim strFromName As String = "", strFromAdress As String = "", strData, strToName, strSabj As String
        Dim strArea As String
        Dim NewSabj As SabjRecord
        Dim NewMess As New MessRecord
        Dim tmpAreaRec As AreaRecord
        Dim NewIndex, lCurMailID, lReplyID As Integer
        Dim lAreaIndex As Integer
        Dim tmpString() As String
        Dim i As Integer

        Dim fsHdr, fsMes As FileStream, bwHdr, bwMes As BinaryWriter


        ReDim tmpString(0)

        With Messages(lMessIndex).Hdr
            strData = .Date  'Left$(TmpText, EndPos - 1)
            strToName = Trim(.ToName)
            strFromName = Trim(.FromName)
            strSabj = RTrim(.Subject)
        End With

        tmpString = CType(Messages(lMessIndex).Msg.Clone, String())

        If Strings.Left(tmpString(0), 5) <> "AREA:" Then
            strArea = "NETMAIL"
            lngNetMailCount = lngNetMailCount + 1
        Else
            strArea = CutOfNullChar(Replace(tmpString(0), "AREA:", vbNullString))
        End If
        If strArea <> vbNullString Then
            lAreaIndex = GetAreaIndex(strArea)
            'sBlockedAreaName = strArea
            'lBlockedAreaIndex = lAreaIndex
            If lAreaIndex > 0 Then
                ' If CheckAreaLock(lAreaIndex) Then
                'strStateInfo = "Ожидание освобождения" & vbCrLf & "заблокированной конференции " & sBlockedAreaName
                'bAreaIsLock = True
                'tmrCheckLock.Enabled = True
                'Me.Hide()
                'End If
            End If
        End If

        lCurMailID = ParseTime(strData)
        lReplyID = 0
        For i = CInt(IIf(strArea = "NETMAIL", 0, 1)) To UBound(tmpString)
            If Strings.Left(tmpString(i), 7) = Chr(1) & "MSGID:" Then
                strFromAdress = Replace(tmpString(i), Chr(1) & "MSGID:", vbNullString)
                strFromAdress = Strings.Left(LTrim(strFromAdress), InStr(1, LTrim(strFromAdress), Chr(32)))
                strFromAdress = Trim(Replace(UCase(strFromAdress), "@FIDONET", vbNullString))
                strTemp = Trim(Mid(tmpString(i), InStrRev(tmpString(i), Chr(32))))
                lCurMailID = CInt(Val("&H" & strTemp))
            End If

            If Strings.Left(tmpString(i), 7) = Chr(1) & "REPLY:" Then
                'strTemp = Mid$(TmpText, tmpBegPos, tmpEndPos - tmpBegPos)
                strTemp = Replace(tmpString(i), Chr(1) & "REPLY:", vbNullString)
                strTemp = Trim(Mid(strTemp, InStrRev(strTemp, Chr(32))))
                lReplyID = CInt(Val("&H" & strTemp))
            End If

            tmpText = tmpText & tmpString(i) & IIf(i < UBound(tmpString), vbCr, "").ToString
            'Debug.Print Asc(Right$(tmpString(i), 1))
        Next  'i

        iMessAttrib = SetMessStatus(iMessAttrib, MessAttrib.atMSGRECD)
        iMessStatus = 0
        iMessStatus = ReSetByteSelected(iMessStatus, MessStatus.stDB_MAIL_STATUS_READ)

        lAreaIndex = GetAreaByName(strArea)
        'Debug.Print lAreaIndex & " - " & strArea
        If lAreaIndex = -1 Then
            CreateArea(strArea)
            lAreaIndex = UBound(arrAreasToss)
        End If

        tmpAreaRec = arrAreasToss(lAreaIndex)
        NewIndex = tmpAreaRec.NumberOfMails + 1

        'файл заголовков сообщений
        fsHdr = New FileStream(sBasePath & CutOfNullChar(tmpAreaRec.FileName) & ".hdr", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
        bwHdr = New BinaryWriter(fsHdr)

        'файл сообщений
        fsMes = New FileStream(sBasePath & CutOfNullChar(tmpAreaRec.FileName) & ".mes", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
        bwMes = New BinaryWriter(fsMes)

        With NewSabj 'заполним поля сообщения
            .Name = strSabj & vbNullChar 'Поле "Тема" заголовка письма
            .DateTime = strData & vbNullChar 'Дата и время создания письма в виде символьной строки в формате, принятом в сети FidoNet
            .ToName = strToName & vbNullChar 'Поле "Кому" заголовка письма
            .FromName = strFromName & vbNullChar 'Поле "От кого" заголовка письма
            .StrucLen = 238 'Размер структуры файла заголовков и должен быть равен 238
            .Status = iMessStatus 'Статус письма
            .Attrib = CShort(iMessAttrib) 'Атрибуты письма
            .MailID = lCurMailID
            .ReciptTime = ParseStdTime(CStr(Now)) '(sReciptTime) '.MailID 'DateDiff("s", CDate(Format("01.01.1970 00:00:00", "dd.mm.yy hh:mm:ss")), DateAdd("h", 0, Now))
            .offset = CInt(fsMes.Length + 1) 'Смещение в файле сообщений первого символа сообщения
            .TextLen = tmpText.Length  'Длина текста сообщения в файле базы писем (включая все кладжи)
            .index = NewIndex - 1 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
            .OrigZone = GetAdressElement(strFromAdress, AdressElement.iZONE) ' 2 'Номер зоны отправителя (обычно 2)
            .OrigNet = GetAdressElement(strFromAdress, AdressElement.iNET) '5015 'Номер сети отправителя
            .OrigNode = GetAdressElement(strFromAdress, AdressElement.INode) '112 'Номер узла отправителя
            .OrigPoint = GetAdressElement(strFromAdress, AdressElement.iPoint) 'intPoint ' 35 'Номер поинта отправителя
            .DestNet = PKThdr.DestNet 'GetAdressElement(sUpLink, iNET) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNet, GetAdressElement(sUpLink, iNET))
            .DestNode = PKThdr.DestNode 'GetAdressElement(sUpLink, iNode) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNode, GetAdressElement(sUpLink, iNode))
            .DestPoint = PKThdr.DestPoint 'GetAdressElement(sUpLink, iPoint) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigPoint, GetAdressElement(sUpLink, iPoint))
            .DestZone = PKThdr.DestZone 'GetAdressElement(sUpLink, iZONE) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigZone, GetAdressElement(sUpLink, iZONE))
            .ReplyID = lReplyID 'Установим идентификатор письма на которое отвечаем
            .NestLevel = 0 ' не будем заполнять - это внутреннее поле используемое в ФИПС
            .UnixTime = .MailID '.ReciptTime 'Время создания письма в формате, принятом в UNIX
        End With

        With NewMess 'заолним поля сообщения
            'Это идентификатор начала заголовка нового сообщения
            .Initialize()
            .MagicID(0) = 254
            .MagicID(1) = 175
            .MagicID(2) = 254
            .MagicID(3) = 175
            .MagicID(4) = 254
            .MagicID(5) = 175
            .MagicID(6) = 254
            .MagicID(7) = 175
            .MagicID(8) = 4
            .MagicID(9) = 3
            .MagicID(10) = 2
            .MagicID(11) = 1
            .MagicID(12) = 1
            .MagicID(13) = 2
            .MagicID(14) = 3
            .MagicID(15) = 4
            .version = &H1 'Номер версии формата базы и должен быть равен 0х01
            .Echotag = tmpAreaRec.Echotag & vbNullChar 'Название области
            '        .Sabject = sSabj 'Поле "Тема" заголовка письма
            .Subject = strSabj & vbNullChar 'Поле "Тема" заголовка письма
            .datetime = strData & vbNullChar 'Дата и время создания письма в виде символьной строки в формате, принятом в сети FidoNet
            '        .ToName = sToName 'Поле "Кому" заголовка письма
            .toname = strToName & vbNullChar 'Поле "Кому" заголовка письма
            '        .FromName = sFromName 'Поле "От кого" заголовка письма
            .fromname = strFromName & vbNullChar 'Поле "От кого" заголовка письма
            .StrucLen = 238 'Размер структуры файла заголовков и должен быть равен 238
            .Status = iMessStatus 'установим статус сообщения
            .Attrib = CShort(iMessAttrib) 'установим атрибуты сообщения
            .MailID = lCurMailID 'DateDiff("s", CDate(Format("01.01.1970 00:00:00", "dd.mm.yy hh:mm:ss")), Now) 'Идентификатор письма, используемый тосеером
            .ReplyID = lReplyID 'установим идентификатор сообщения на которое отвечаем
            .ReciptTime = ParseStdTime(CStr(Now)) 'ParseStdTime(sReciptTime) '.MailID 'DateDiff("s", CDate(Format("01.01.1970 00:00:00", "dd.mm.yy hh:mm:ss")), DateAdd("h", 0, Now)) '-frmReadAreas.GetTimeShift("2:5015/112.35"), Now))
            .offset = CInt(fsMes.Length + 1) 'Смещение  первого символа текста сообщения (кладж MagicID) в файле базы писем
            .TextLen = tmpText.Length  'Длина текста сообщения в файле базы писем (включая все кладжи)
            .index = NewIndex - 1 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
            '        If UCase$(ConvOemToAnsi(m_Area.Echotag)) <> "NETMAIL" Then
            .OrigZone = GetAdressElement(strFromAdress, AdressElement.iZONE) ' 2 'Номер зоны отправителя (обычно 2)
            .OrigNet = GetAdressElement(strFromAdress, AdressElement.iNET) '5015 'Номер сети отправителя
            .OrigNode = GetAdressElement(strFromAdress, AdressElement.INode) '112 'Номер узла отправителя
            .OrigPoint = GetAdressElement(strFromAdress, AdressElement.iPoint) 'intPoint ' 35 'Номер поинта отправителя
            .DestNet = PKThdr.DestNet 'GetAdressElement(sUpLink, iNET) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNet, GetAdressElement(sUpLink, iNET))
            .DestNode = PKThdr.DestNode 'GetAdressElement(sUpLink, iNode) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigNode, GetAdressElement(sUpLink, iNode))
            .DestPoint = PKThdr.DestPoint 'GetAdressElement(sUpLink, iPoint) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigPoint, GetAdressElement(sUpLink, iPoint))
            .DestZone = PKThdr.DestZone 'GetAdressElement(sUpLink, iZONE) ' IIf(OpenEditorMode = REPLY_MESSAGE, m_Sabj.OrigZone, GetAdressElement(sUpLink, iZONE))
            .UnixTime = .MailID 'Время создания письма в формате, принятом в UNIX
        End With

        If SaveMessageEx(arrAreasToss(lAreaIndex), NewSabj, NewMess, tmpText) Then
            arrAreasToss(lAreaIndex).NumberOfMails = arrAreasToss(lAreaIndex).NumberOfMails + 1

            Dim fsA As New FileStream(sBasePath & "areas.wwd", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            Dim bwA As New BinaryWriter(fsA), buff() As Byte = arrAreasToss(lAreaIndex).ToByteArray

            'открываем файл описания областей как бинарный
            'позиционируемся на нужную запись
            'Seek(AreasFile, lAreaIndex * 562 - 561) файлы начинаются с 0 а не с 1 как в VB6
            fsA.Seek(lAreaIndex * 562 - 562, SeekOrigin.Begin)
            With bwA
                .Write(buff)
                .Flush()
                .Close()
            End With
            fsA.Close()

        End If

        With NewSabj
            strToAdres = .DestZone & ":" & .DestNet & "/" & .DestNode & "." & .DestPoint
        End With

        If strToAdres = CutOfNullChar(arrAreasToss(lAreaIndex).AKA) Then
            If UCase(strToName) = UCase(sPointName) Then
                If arrAreasToss(lAreaIndex).LocalMail = 0 Then
                    If strArea <> "NETMAIL" Then
                        SaveCarbonCopy(NewSabj, NewMess, tmpText, strArea)
                        lngCarbonCopyCount = lngCarbonCopyCount + 1
                    End If
                End If
            End If
        End If

        TotalMessCount = TotalMessCount + 1
        Return True

        Exit Function

ErrHandle:
        ComposeMessage = False
    End Function

    Public Sub CreateBaseAreas()
        Dim fs As FileStream, bw As BinaryWriter, buff() As Byte

        If My.Computer.FileSystem.DirectoryExists(sBasePath) Then
            If My.Computer.FileSystem.FileExists(sBasePath & "areas.wwd") Then
                fs = New FileStream(sBasePath & "areas.wwd", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            Else
                fs = New FileStream(sBasePath & "areas.wwd", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
            End If
        Else
            Exit Sub
        End If

        ReDim Preserve arrAreasToss(2)
        bw = New BinaryWriter(fs)

        CreateArea("NETMAIL", , True, 0)
        CreateArea("LOCALMAIL", 1, True, 1)
        For i As Integer = 1 To 2
            'Seek(AreasFile, i * 562 - 562) 'позиционируемся на нужную запись
            buff = arrAreasToss(i).ToByteArray
            bw.Write(buff)
        Next i

        bw.Flush()
        bw.Close()
        fs.Close()
        fs.Dispose()

    End Sub

    Private Function LoadAreas(ByVal sPath As String) As Boolean
        Dim intAreasCnt As Short, areas As String = GetValidPath(sPath) & "areas.wwd"
        Dim rec As New AreaRecord
        Dim fs As FileStream, br As BinaryReader

        Try
            fs = New FileStream(areas, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            br = New BinaryReader(fs)
        Catch ex As FileNotFoundException
            Return False
        Catch ex As DirectoryNotFoundException
            Return False
        End Try

        intAreasCnt = CShort(fs.Length / rec.Length)
        ReDim arrAreasToss(0)
        If intAreasCnt = 0 Then
            CreateBaseAreas()
            intAreasCnt = 2
        End If

        ReDim Preserve arrAreasToss(intAreasCnt)
        For i As Integer = 1 To intAreasCnt
            arrAreasToss(i) = rec.FromBinaryReaderBlock(br)
        Next

        br.Close()
        fs.Close()
        fs.Dispose()

        Return True

    End Function

    Private Function SaveMessageEx(ByRef Area As AreaRecord, ByVal NewSabj As SabjRecord, ByVal NewMess As MessRecord, ByVal OEMText As String) As Boolean
        Dim fsHdr, fsMes As FileStream, bwHdr, bwMes As BinaryWriter, buff() As Byte

        Try
            'файл заголовков сообщений
            fsHdr = New FileStream(sBasePath & CutOfNullChar(Area.FileName) & ".hdr", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            bwHdr = New BinaryWriter(fsHdr)

            'файл сообщений
            fsMes = New FileStream(sBasePath & CutOfNullChar(Area.FileName) & ".mes", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            bwMes = New BinaryWriter(fsMes)

            If NewSabj.index <= 0 Then
                fsHdr.Seek(0, SeekOrigin.Begin)
            Else
                'установим указатель на текущее сообщение
                'fsHdr.Seek((NewSabj.index + 1) * 238 - 238, SeekOrigin.Begin)
                fsHdr.Seek((NewSabj.index) * 238, SeekOrigin.Begin)
            End If

            'запишем в файл заголовок сообщения
            buff = NewSabj.ToByteArray
            With bwHdr
                .Write(buff)
                .Flush()
                .Close()
            End With
            fsHdr.Close() 'закрываем файл заголовков
            fsHdr.Dispose()

            fsMes.Seek(NewSabj.offset, SeekOrigin.Begin) 'установим указатель на текущее сообщение        
            buff = NewMess.ToByteArray
            With bwMes
                .Write(buff) 'запишем заголовок сообщения        
                '.Write(OEMText) ' поубивал бы за неестественный интелект, кто вот просит строку писать в юникоде внутрь базы.                
                .Write(System.Text.Encoding.GetEncoding(866).GetBytes(OEMText)) 'запишем текст сообщения
                .Flush()
                .Close()
            End With
            fsMes.Close() 'закроем файл сообщений
            fsMes.Dispose()

            Return True

        Catch
            'ошибка
            Return False
        End Try

    End Function

    Private Sub SaveCarbonCopy(ByRef NewSabj As SabjRecord, ByRef NewMess As MessRecord, ByVal tmpText As String, ByVal strAreaName As String)
        Dim tmpArray(), strNewText As String
        Dim tmpArrayMin, tmpArrayMax As Integer
        Dim blnFirstLine As Boolean
        Dim tmpAreaRec As AreaRecord
        Dim fsMes As FileStream

        tmpAreaRec = arrAreasToss(GetAreaByName("LOCALMAIL"))

        'файл сообщений
        fsMes = New FileStream(sBasePath & CutOfNullChar(tmpAreaRec.FileName) & ".mes", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)

        blnFirstLine = True
        tmpArray = Split(tmpText, vbCr)
        tmpArrayMin = LBound(tmpArray)
        tmpArrayMax = UBound(tmpArray)

        strNewText = ""

        For i As Integer = tmpArrayMin To tmpArrayMax
            If Left(tmpArray(i), 1) <> Chr(1) Then
                If blnFirstLine Then
                    strNewText = strNewText & "Копия из области " & UCase(strAreaName) & vbCrLf & tmpArray(i) & vbCr
                    blnFirstLine = False
                Else
                    strNewText = strNewText & tmpArray(i) & vbCr
                End If
            Else
                strNewText = strNewText & tmpArray(i) & vbCr
            End If
        Next i

        With NewSabj
            .offset = fsMes.Length      'Смещение в файле сообщений первого символа сообщения
            .TextLen = strNewText.Length   'Длина текста сообщения в файле базы писем (включая все кладжи)
            .index = tmpAreaRec.NumberOfMails 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
        End With

        With NewMess 'заолним поля сообщения
            .Echotag = tmpAreaRec.Echotag & vbNullChar 'Название области
            .offset = fsMes.Length      'Смещение в файле сообщений первого символа сообщения
            .TextLen = strNewText.Length   'Длина текста сообщения в файле базы писем (включая все кладжи)
            .index = tmpAreaRec.NumberOfMails 'Порядковый номер этого сообщения в базе писем (0, 1, 2 и т. д.)
        End With

        If SaveMessageEx(tmpAreaRec, NewSabj, NewMess, strNewText) Then
            tmpAreaRec.NumberOfMails = tmpAreaRec.NumberOfMails + 1
            Dim fsA As New FileStream(sBasePath & "areas.wwd", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            Dim bwA As New BinaryWriter(fsA)

            'позиционируемся на нужную запись
            fsA.Seek(GetAreaByName("LOCALMAIL") * 562 - 562, SeekOrigin.Begin)
            With bwA
                .Write(tmpAreaRec.ToByteArray) 'записываем
                .Flush()
                .Close()
            End With
            fsA.Close() 'закрываем файл
            arrAreasToss(GetAreaByName("LOCALMAIL")) = tmpAreaRec
        End If

        SaveLog(Now & vbTab & "Saved CarbonCopy from area " & strAreaName & " ...")

        Exit Sub

    End Sub

    Private Function LoadEchoList(ByVal sFileName As String) As Boolean
        Dim sLoadLine As String
        Dim splitPos As Short
        Dim sr As StreamReader

        colAreas = Nothing
        If Trim(sFileName) = vbNullString Then Exit Function

        If Not My.Computer.FileSystem.FileExists(sFileName) Then
            MsgBox("Не найден эхолист-файл." & vbCrLf & sFileName, MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly, "Загрузка списка конференций")
            Return False
        End If

        sr = My.Computer.FileSystem.OpenTextFileReader(sFileName)

        Do While Not sr.EndOfStream
            sLoadLine = sr.ReadLine()
            sLoadLine = Replace(sLoadLine, vbTab, Chr(32))
            splitPos = InStr(1, sLoadLine, Chr(32))
            If splitPos = 0 Then
                colAreas.Add(UCase(sLoadLine), sLoadLine)
            Else
                colAreas.Add(Mid(sLoadLine, splitPos), UCase(Left(sLoadLine, splitPos - 1)))
            End If
        Loop

        sr.Close()

        Return True

    End Function

    Private Function strGetAreaDescription(ByVal strAreaName As String) As String

        If strAreaName <> vbNullString Then
            If Not IsNothing(colAreas) Then
                Return Trim(CStr(colAreas.Item(UCase(Trim(strAreaName)))))
            End If
        End If

        Return vbNullString

    End Function

    Private Function GetAreaIndex(ByVal sAreaName As String) As Integer 'возвращает индекс элемента массива конференций
        'c имененем sAreaName
        Dim i As Integer
        Dim ArrMax, ArrMin As Integer

        GetAreaIndex = -1 ' заранее установим признак того, что не нашли нужную конференцию
        ArrMin = LBound(arrAreasToss) : ArrMax = UBound(arrAreasToss) 'получим минимальное и максимальный размеры массива
        For i = ArrMin To ArrMax 'пройдёмся по массиву конференций
            If arrAreasToss(i).Echotag = sAreaName Then ' sCopyAreaName Then 'если имя конференции, то что нам нужно, тогда
                GetAreaIndex = i ' получим индекс элемента массива
                Exit For 'выйдем из массива
            End If
        Next i
        '    GetAreaIndex = GetAreaIndex 'вернём нужный индекс (ИСПРАВИТЬ)
    End Function

    Private Function GetAreaByName(ByVal sAreaName As String, Optional ByRef strUplink As String = "") As Integer
        Dim i, iAreaIndex As Short

        iAreaIndex = -1
        For i = LBound(arrAreasToss) To UBound(arrAreasToss)
            If UCase(CutOfNullChar(sAreaName)) = UCase(CutOfNullChar(arrAreasToss(i).Echotag)) Then
                If IsNothing(strUplink) Then
                    iAreaIndex = i
                    Exit For
                Else
                    If InStr(1, arrAreasToss(i).UpLink, Trim(strUplink)) <> 0 Then
                        iAreaIndex = i
                        Exit For
                    End If
                End If
            End If
        Next i

        Return iAreaIndex

    End Function

    Private Function SetByteSelected(ByVal Value As Integer, ByVal Mask As MessStatus) As Integer
        SetByteSelected = Value Or Mask
    End Function

    Private Function ReSetByteSelected(ByVal Value As Integer, ByVal Mask As MessStatus) As Integer
        ReSetByteSelected = Value And (Not Mask)
    End Function

    Private Function SetMessStatus(ByVal Value As Integer, ByVal Mask As MessAttrib) As Integer
        SetMessStatus = Value Or Mask
    End Function

    Private Function IsByteSelected(ByVal Value As Integer, ByVal Mask As MessStatus) As Boolean
        IsByteSelected = (Value And Mask)
    End Function

    Private Function ReSetMessStatus(ByVal Value As Integer, ByVal Mask As MessAttrib) As Integer
        ReSetMessStatus = Value And (Not Mask)
    End Function

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

        Erase MsgArray

        If Trim(sFName).Length = 0 Then Exit Function

        SaveLog(Now & vbTab & sFName)

        PktTossPacket(sFName)
        ArrMax = UBound(Messages) 'nPacket.Records 'UBound(MsgArray)

        For i = 1 To ArrMax
            'System.Windows.Forms.Application.DoEvents()
            If Not ComposeMessage(i) Then
                Return False                
            End If
            'System.Windows.Forms.Application.DoEvents()
        Next i

        Return True
    End Function

#End Region

#Region "ModuleInfo"
    Public Function GetModuleIcon() As System.Drawing.Image Implements IModule.GetModuleIcon
        Return My.Resources.ModuleIcon.ToBitmap
    End Function

    Public ReadOnly Property Description() As String Implements IModuleInfo.Description
        Get
            Return "FIPS Toss Module"
        End Get
    End Property

    Public ReadOnly Property Name() As String Implements IModuleInfo.Name
        Get
            Return "fipsTosser"
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
