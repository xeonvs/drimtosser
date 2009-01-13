'Based on FBRToss code
'Copyright R. Demidov & M. Irgiznov 2006-2007

Option Strict Off
Option Explicit On

Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Text


Public Module PKT

#Region "Structures"

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure Addr
        Dim Zone As Short
        Dim Net As Short
        Dim Node As Short
        Dim Point As Short
        Public Overrides Function ToString() As String
            Return Me.Zone & ":" & Me.Net & "/" & Me.Node & "." & Me.Point
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure Packet
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=23)> Public From As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=23)> Public [To] As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=20)> Public [Date] As String
        Dim PType As Short
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=8)> Public Password As String
        Dim Size As Integer
        Dim Records As Short

        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(Packet))) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As Packet
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(Packet)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As Packet = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(Packet)), Packet)
            handle.Free()
            Return st
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure MsgInfo
        Dim FromAdr As Addr
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=36)> Public FromName As String
        Dim ToAdr As Addr
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=36)> Public ToName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=72)> Public Subject As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=20)> Public [Date] As String '?? ??? ??  ??:??:??

        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(MsgInfo))) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As MsgInfo
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(MsgInfo)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As MsgInfo = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(MsgInfo)), MsgInfo)
            handle.Free()
            Return st
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure PktHeader
        Dim oNode As Short
        Dim dNode As Short
        Dim Year As Short
        Dim Month As Short
        Dim Day As Short
        Dim Hour As Short
        Dim Minute As Short
        Dim Second As Short
        Dim Baud As Short
        Dim PacketType As Short
        Dim oNet As Short
        Dim dNet As Short
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=1)> Public prodCode As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=1)> Public serNo As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=8)> Public Password As String
        Dim QoZone As Short
        Dim QdZone As Short
        Dim AuxNet As Short
        Dim cw1 As Short
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=1)> Public pCode As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=1)> Public PRMinor As String
        Dim CW2 As Short
        Dim oZone As Short
        Dim dZone As Short
        Dim oPoint As Short
        Dim dPoint As Short
        Dim LongData As Integer

        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(PktHeader))) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As PktHeader
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(PktHeader)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As PktHeader = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(PktHeader)), PktHeader)
            handle.Free()
            Return st
        End Function

        Public Function GetFromAddr() As String
            Return Me.oZone & ":" & Me.oNet & "/" & Me.oNode & "." & Me.oPoint
        End Function

        Public Function GetToAddr() As String
            Return Me.dZone & ":" & Me.dNet & "/" & Me.dNode & "." & Me.dPoint
        End Function

    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure PKThdr20Type
        Dim OrigNode As Short '0
        Dim DestNode As Short '2
        Dim Year As Short '4
        Dim Month As Short '6
        Dim Day As Short '8
        Dim Hour As Short '10
        Dim Minute As Short '12
        Dim Second As Short '14
        Dim Baud As Short '16
        Dim NoName As Short '18
        Dim OrigNet As Short '20  -  Set to -1 if from point
        Dim DestNet As Short '22
        Dim ProductCode As Short '24  -  ProductCode(low order) | Revision         (major) |
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=8)> Public Password As String '26  - password
        Dim OrigZone As Short '34
        Dim DestZone As Short '36
        Dim AuxNet As Short '38
        Dim CWvalidationCopy As Short '40
        Dim ProductCode1 As Short '42  - ProductCode (high order) | Revision         (minor) |
        Dim CapabilWord As Short '44
        Dim OrigZone1 As Short '46
        Dim DestZone1 As Short '48
        Dim OrigPoint As Short '50
        Dim DestPoint As Short '52
        Dim ProductSpecificData As Integer '54  4 Bytes

        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(PKThdr20Type))) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As PKThdr20Type
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(PKThdr20Type)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As PKThdr20Type = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(PKThdr20Type)), PKThdr20Type)
            handle.Free()
            Return st
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure PktMsgHeader
        Dim PType As Short
        Dim oNode As Short
        Dim dNode As Short
        Dim oNet As Short
        Dim dNet As Short
        Dim Attr As Short
        Dim cost As Short
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=20)> Public datetime As String
        Public Function ToByteArray() As Byte()
            Dim buff(Marshal.SizeOf(GetType(PktMsgHeader))) As Byte
            Dim Handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Marshal.StructureToPtr(Me, Handle.AddrOfPinnedObject(), False)
            Handle.Free()
            Return buff
        End Function

        Public Function FromBinaryReaderBlock(ByVal br As BinaryReader) As PktMsgHeader
            Dim buff() As Byte = br.ReadBytes(Marshal.SizeOf(GetType(PktMsgHeader)))
            Dim handle As GCHandle = GCHandle.Alloc(buff, GCHandleType.Pinned)
            Dim st As PktMsgHeader = CType(Marshal.PtrToStructure(handle.AddrOfPinnedObject(), GetType(PktMsgHeader)), PktMsgHeader)
            handle.Free()
            Return st
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
    Public Structure MessStruct
        Dim Pkt As PktHeader
        Dim Hdr As MsgInfo
        Dim Msg() As String
    End Structure
#End Region

    Public Messages() As MessStruct
    Public Months() As String = {" ", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}

    ''' <summary>
    ''' 'Процедура удаляет заданную мессагу из заданного файла.
    ''' </summary>
    ''' <param name="Wich"></param>
    ''' <param name="FileName"></param>
    ''' <remarks>'Если она последняя, просто получается пустой пакет!!!</remarks>
    Public Sub DelPKT(ByRef Wich As Short, ByRef FileName As String)
        Dim Head As New PktHeader
        Dim tmp As Short
        Dim Tmp2 As String, Tmp3() As Byte
        Dim MsgHead As New PktMsgHeader
        Dim m, i As Integer
        Dim Offset1, Offset2, LastSeek As Integer
        Dim TmpFile As String = ""
        Dim fs1, fs2 As FileStream, br As BinaryReader, bw As BinaryWriter

        Tmp2 = Space$(1)
        fs1 = New FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
        br = New BinaryReader(fs1)

        'FileOpen(fNum, FileName, OpenMode.Binary)
        'FileGet(fNum, Head)
        Head = Head.FromBinaryReaderBlock(br)

        'Проверка на тип пакета
        If Head.PacketType <> 2 Or Head.cw1 <> 256 Or Head.CW2 <> 1 Then
            fs1.Close()
            Exit Sub
        End If

        'FileGet(fNum, tmp)
        tmp = br.ReadInt16()
        If tmp = 0 Then
            fs1.Close()
            Exit Sub
        End If

        'Seek(fNum, Seek(1) - 2)
        fs1.Seek(-2, SeekOrigin.Current)

        For i = 1 To Wich
            If i = Wich Then
                Offset1 = fs1.Position
            End If

            'FileGet(fNum, MsgHead)
            MsgHead = MsgHead.FromBinaryReaderBlock(br)
            If MsgHead.PType <> 2 Then
                fs1.Close()
                Exit Sub
            End If
            'Прокручиваем имена, сабж и текст сообщения...
            For j As Integer = 1 To 4
                ReDim Tmp3(1024)
                Do
                    LastSeek = fs1.Position
                    'FileGet(fNum, Tmp3)
                    Tmp3 = br.ReadBytes(1024)
                    m = InStr(Encoding.GetEncoding(866).GetString(Tmp3), Chr(0))
                Loop Until m <> 0
                'Seek(fNum, LastSeek + m)
                fs1.Seek(-1024 + m, SeekOrigin.Current)
            Next
        Next

        'Теперь указатель указывает на начало следующей мессаги.
        'Типа... Круто! ;-)
        Offset2 = fs1.Position

        'Ну а теперь, перекидываем все байтики
        'Поздняя приписка: мляяяяяяяяя.... Бейсик - лажа! Нельзя даже файл
        'закрыть на любой позиции... Короче: перекидываем всю инфу в новый файл,
        'старый удаляем и пишем поверх него. Иначе пока нельзя. :-(((

        For i = FileName.Length To 1 Step -1
            If Mid(FileName, i, 1) = "." Then TmpFile = Left(FileName, i) & "k-q" : Exit For
        Next

        'FileOpen(fNum2, TmpFile, OpenMode.Binary)
        fs2 = New FileStream(TmpFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
        bw = New BinaryWriter(fs2)

        'Тягаем куски блоками по 16 кб.
        ReDim Tmp3(16384)
        fs1.Seek(0, SeekOrigin.Begin) 'Seek(fNum, 1)

        For i = 1 To ((Offset1 - 1) \ 16384)
            Tmp3 = br.ReadBytes(16384)
            bw.Write(Tmp3)
        Next

        ReDim Tmp3((Offset1 - 1) Mod 16384)
        Tmp3 = br.ReadBytes(Tmp3.Length) 'FileGet(fNum, Tmp3)
        bw.Write(Tmp3)

        'Теперь тянем все тоже самое до конца файла, но со следующего смещения...
        ReDim Tmp3(16384)

        fs1.Seek(Offset2, SeekOrigin.Begin) 'Seek(fNum, Offset2)

        For i = 1 To ((fs1.Length - Offset2 + 1) \ 16384)
            Tmp3 = br.ReadBytes(16384)
            bw.Write(Tmp3)
        Next

        'Перекидываем остатки
        ReDim Tmp3((fs1.Length - Offset2 + 1) Mod 16384)
        Tmp3 = br.ReadBytes(Tmp3.Length) 'FileGet(fNum, Tmp3)
        bw.Write(Tmp3)

        bw.Flush()
        bw.Close()
        fs2.Close()
        br.Close()
        fs1.Close()

        'Теперь просто удаляем первый файл и переименовываем второй...
        My.Computer.FileSystem.DeleteFile(FileName)
        My.Computer.FileSystem.MoveFile(TmpFile, FileName)

    End Sub

    ''' <summary>
    ''' 'Вытаскивает и текстовой строки адрес и раскидывает в переменную
    ''' </summary>
    Public Sub GetAddress(ByRef AdrString As String, ByRef Adres As Addr)
        AdrString = LTrim(RTrim(AdrString))
        If AdrString.Length = 0 Then Exit Sub
        If InStr(AdrString, ":") = 0 Or InStr(AdrString, "/") = 0 Or InStr(AdrString, "/") < InStr(AdrString, ":") Or ((InStr(AdrString, ".") < InStr(AdrString, "/")) And InStr(AdrString, ".")) Then Exit Sub

        Adres.Zone = CShort(Val(Left(AdrString, InStr(AdrString, ":") - 1)))
        Adres.Net = CShort(Val(Mid(AdrString, InStr(AdrString, ":") + 1, InStr(AdrString, "/") - InStr(AdrString, ":") - 1)))

        If InStr(AdrString, ".") <> 0 Then
            Adres.Node = CShort(Val(Mid(AdrString, InStr(AdrString, "/") + 1, InStr(AdrString, ".") - InStr(AdrString, "/") - 1)))
            Adres.Point = CShort(Val(Right(AdrString, Len(AdrString) - InStr(AdrString, "."))))
        Else
            Adres.Node = CShort(Val(Right(AdrString, Len(AdrString) - InStr(AdrString, "/"))))
            Adres.Point = 0
        End If

    End Sub

    ''' <summary>
    ''' Выводит инфу по пакету.
    ''' </summary>
    ''' <param name="pkt"></param>
    ''' <param name="FileName"></param>
    ''' <remarks>Поддерживает только пакеты типа 2+</remarks>
    Public Sub PktInfo(ByRef pkt As Packet, ByRef FileName As String)
        Dim m As Integer
        Dim Head As New PktHeader
        Dim tmp, i As Short
        Dim Tmp2, Tmp3 As String
        Dim MsgHead As New PktMsgHeader
        Dim fs As New FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Dim br As New BinaryReader(fs)

        Tmp2 = Space$(1)
        pkt.Size = fs.Length

        Head = Head.FromBinaryReaderBlock(br)

        'Проверка на тип пакета
        If Head.PacketType <> 2 Or Head.cw1 <> 256 Or Head.CW2 <> 1 Then
            fs.Close()
            Exit Sub
        End If

        If Head.oPoint <> 0 And Head.oNet = -1 Then
            Head.oNet = Head.AuxNet
        End If

        'Собирание инфы
        Tmp3 = LTrim(Str(Head.oZone)) & ":" & LTrim(Str(Head.oNet)) & "/" & LTrim(Str(Head.oNode))
        If Head.oPoint <> 0 Then Tmp3 = Tmp3 & "." & LTrim(Str(Head.oPoint))

        pkt.From = Tmp3
        Tmp3 = LTrim(Str(Head.dZone)) & ":" & LTrim(Str(Head.dNet)) & "/" & LTrim(Str(Head.dNode))

        If Head.dPoint <> 0 Then Tmp3 = Tmp3 & "." & LTrim(Str(Head.dPoint))

        With pkt
            .To = Tmp3
            .Date = LTrim(Str(Head.Day)) & " " & _
                    Months(Head.Month) & " " & _
                    LTrim(Str(Head.Year)) & ", " & _
                    LTrim(Str(Head.Hour)) & ":" & _
                    LTrim(Str(Head.Minute))
            .PType = Head.PacketType
            .Password = Head.Password
        End With

        tmp = br.ReadInt16

        If tmp = 0 Then
            pkt.Records = 0
            br.Close()
            fs.Close()
            Exit Sub
        End If

        'Seek(ff, Seek(ff) - 2)
        fs.Seek(-2, SeekOrigin.Current)

        Do
            MsgHead = MsgHead.FromBinaryReaderBlock(br)
            If MsgHead.PType <> 2 Then
                fs.Close()
                Exit Sub
            End If

            pkt.Records += 1

            'Прокручиваем имена, сабж и текст сообщения...
            For i = 1 To 4
                Tmp3 = Space$(1024)
                Do
                    'FileGet(ff, Tmp3)
                    Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))
                    m = InStr(Tmp3, Chr(0))
                Loop Until m <> 0
                fs.Seek(-1024 + m, SeekOrigin.Current)
            Next

        Loop

        br.Close()
        fs.Close()

    End Sub

    ''' <summary>
    ''' Выводит список заголовков мессаг в пакете.
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="FileName"></param>
    ''' <remarks>Поддерживает только пакеты типа 2+</remarks>
    Public Sub PktMsgList(ByRef a() As MsgInfo, ByRef FileName As String)
        Dim Head As New PktHeader
        Dim tmp As Short, LastSeek As Integer
        Dim Tmp2 As Char, Tmp3 As String
        Dim MsgHead As New PktMsgHeader
        Dim Id, m As Integer
        Dim MsgId As String
        Dim fs As New FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Dim br As New BinaryReader(fs)

        Tmp2 = Space$(1)

        Head = Head.FromBinaryReaderBlock(br)

        'Проверка на тип пакета
        If Head.PacketType <> 2 Or Head.cw1 <> 256 Or Head.CW2 <> 1 Then
            fs.Close()
            Exit Sub
        End If

        If Head.oPoint <> 0 And Head.oNet = -1 Then Head.oNet = Head.AuxNet

        tmp = br.ReadInt16
        If tmp = 0 Then
            fs.Close()
            Exit Sub
        End If

        fs.Seek(-2, SeekOrigin.Current)

        Do
            MsgHead = MsgHead.FromBinaryReaderBlock(br)
            If MsgHead.PType <> 2 Then
                fs.Close()
                Exit Sub
            End If

            ReDim Preserve a(UBound(a) + 1)

            'Это все ЛАЖОВЫЕ имена. Настоящие берутся только в MSGID!!!!!!
            a(UBound(a)).Date = MsgHead.datetime

            'Теперь потихоньку читаем имена и сабж...
            Tmp3 = ""
            Do
                'FileGet(fNum, Tmp2)
                Tmp2 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1))
                If Tmp2 <> Chr(0) Then Tmp3 &= Tmp2 Else Exit Do
            Loop
            a(UBound(a)).ToName = Tmp3

            Tmp3 = ""
            Do
                Tmp2 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1))
                If Tmp2 <> Chr(0) Then Tmp3 &= Tmp2 Else Exit Do
            Loop
            a(UBound(a)).FromName = Tmp3

            Tmp3 = ""
            Do
                Tmp2 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1))
                If Tmp2 <> Chr(0) Then Tmp3 &= Tmp2 Else Exit Do
            Loop
            a(UBound(a)).Subject = Tmp3

            'Прокручиваем текст сообщения...
            'Ищем MSGID и REPLY и смотрим реальные адреса
            'С вероятностью сто процентов в первых 64 байтах будет
            Tmp3 = Space(1024)
            Do
                LastSeek = fs.Position 'Смещение начала этой мессаги
                'FileGet(fNum, Tmp3)
                Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))

                'Определаем полный адрес From
                Id = InStr(Left(Tmp3, 128), Chr(1) & "MSGID: ")
                If Id <> 0 Then 'Выкусываем имя из MSGID
                    MsgId = Mid(Tmp3, Id + 8, InStr(Id + 8, Tmp3, " ") - (Id + 8))
                    GetAddress(MsgId, a(UBound(a)).FromAdr)
                End If

                'Определаем полный адрес To
                Id = InStr(Left(Tmp3, 64), Chr(1) & "REPLY: ")
                If Id <> 0 Then 'Выкусываем имя из REPLY
                    MsgId = Mid(Tmp3, Id + 8, InStr(Id + 8, Tmp3, " ") - (Id + 8))
                    GetAddress(MsgId, a(UBound(a)).ToAdr)
                Else
                    Id = InStr(Left(Tmp3, 64), Chr(1) & "INTL ")
                    If Id <> 0 Then 'Выкусываем имя из INTL
                        MsgId = Mid(Tmp3, Id + 6, InStr(Id + 6, Tmp3, " ") - (Id + 6))
                        Id = InStr(Left(Tmp3, 64), Chr(1) & "TOPT ")
                        If Id <> 0 Then MsgId = MsgId & "." & Mid(Tmp3, Id + 6, InStr(Id + 6, Tmp3, " ") - (Id + 6))
                        GetAddress(MsgId, a(UBound(a)).ToAdr)
                    End If
                End If

                m = InStr(Tmp3, Chr(0))
            Loop Until m <> 0

            'Seek(fNum, LastSeek + m)
            fs.Seek(LastSeek + m, SeekOrigin.Begin)

        Loop

        br.Close()
        fs.Close()

    End Sub

    ''' <summary>
    ''' Читает сообщение с номером Wich из pkt'шника
    ''' </summary>
    ''' <param name="Wich"></param>
    ''' <param name="Message"></param>
    ''' <param name="FileName"></param>
    ''' <remarks></remarks>
    Public Sub PktMsgRead(ByRef Wich As Integer, ByRef Message() As String, ByRef FileName As String)
        Dim Head As New PktHeader
        Dim tmp As Short, Tmp3 As String
        Dim MsgHead As New PktMsgHeader
        Dim m, i, m1 As Integer, LastSeek As Integer
        Dim fs As New FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Dim br As New BinaryReader(fs)

        Head = Head.FromBinaryReaderBlock(br)

        'Проверка на тип пакета
        If Head.PacketType <> 2 Or Head.cw1 <> 256 Or Head.CW2 <> 1 Then
            fs.Close()
            Exit Sub
        End If

        tmp = br.ReadInt16()
        If tmp = 0 Then
            fs.Close()
            Exit Sub
        End If

        fs.Seek(-2, SeekOrigin.Current)

        For i = 1 To Wich
            MsgHead = MsgHead.FromBinaryReaderBlock(br) 'FileGet(fNum, MsgHead)
            If MsgHead.PType <> 2 Then
                fs.Close()
                Exit Sub
            End If

            'Прокручиваем имена, сабж и текст сообщения...
            For j As Integer = 1 To 4
                Tmp3 = Space(1024)
                Do
                    Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))
                    m = InStr(Tmp3, Chr(0))
                Loop Until m <> 0

                fs.Seek(m, SeekOrigin.Current) 'Seek(fNum, LastSeek + m)

                If i = Wich And j = 3 Then
                    Exit For
                End If
            Next
        Next

        'Читаем само сообщение...
        Tmp3 = Space(1024)
        Do
            LastSeek = fs.Position
            'FileGet(fNum, Tmp3)
            Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))
            m = InStr(Tmp3, Chr(13))
            m1 = InStr(Tmp3, Chr(0))
            If (m <> 0 And m1 = 0) Or (m < m1 And m1 <> 0 And m <> 0) Then 'Читаем очередную строчку
                ReDim Preserve Message(UBound(Message) + 1)
                Message(UBound(Message)) = Left(Tmp3, m - 1)
                'Seek(fNum, LastSeek + m)
                fs.Seek(-LastSeek + m, SeekOrigin.Current)
            ElseIf m1 <> 0 Then  'Последняя строчка
                ReDim Preserve Message(UBound(Message) + 1)
                Message(UBound(Message)) = Left(Tmp3, m1 - 1)
                Exit Do
            End If
        Loop

        br.Close()
        fs.Close()

    End Sub

    ''' <summary>
    ''' Тоссинг пакета
    ''' </summary>
    ''' <param name="FileName">полный путь к pkt файлу</param>
    ''' <remarks></remarks>
    Public Sub PktTossPacket(ByRef FileName As String)
        Dim Head As New PktHeader
        Dim tmp, m, i As Integer
        Dim Tmp3 As String = "", tmpText As String
        Dim MsgHead As New PktMsgHeader
        Dim LastSeek As Integer
        Dim Message() As String
        Dim tmpMess As New MessStruct
        Dim nPacket As New Packet
        Dim PktMsgInfo() As MsgInfo

        Dim fs As New FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read)
        Dim br As New BinaryReader(fs)

        'Читает сообщение с номером Wich из pkt'шника

        PktInfo(nPacket, FileName)

        ReDim Messages(nPacket.Records)
        ReDim PktMsgInfo(0)
        PktMsgList(PktMsgInfo, FileName)

        Head = Head.FromBinaryReaderBlock(br)
        tmpMess.Pkt = Head

        'Проверка на тип пакета
        If Head.PacketType <> 2 Or Head.cw1 <> 256 Or Head.CW2 <> 1 Then
            fs.Close()
            Exit Sub
        End If

        tmp = br.ReadInt16
        If tmp = 0 Then
            fs.Close()
            Exit Sub
        End If

        'Seek(fNum, Seek(fNum) - 2)
        fs.Seek(-2, SeekOrigin.Current)

        MsgHead = MsgHead.FromBinaryReaderBlock(br)

        If MsgHead.PType <> 2 Then
            fs.Close()
            Exit Sub
        End If


        For i = 1 To UBound(Messages)
            'Прокручиваем имена, сабж и текст сообщения...
            For j As Integer = 1 To 4
                Tmp3 = Space(1024)
                Do
                    LastSeek = fs.Position
                    Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))

                    m = InStr(Tmp3, Chr(0))

                    If j = 3 Then
                        'LastSeek = LastSeek + m
                        'Get FNum, , Tmp3
                        Exit Do
                    End If
                    'fs.Seek(LastSeek + m, SeekOrigin.Begin)
                Loop Until m <> 0

                If j < 3 Then
                    'Seek(fNum, LastSeek + m)
                    fs.Seek(LastSeek + m, SeekOrigin.Begin)
                Else
                    fs.Seek(LastSeek, SeekOrigin.Begin)
                    Exit For
                End If
            Next

            tmpMess.Hdr = PktMsgInfo(i)

            ReDim Message(0)

            'Читаем само сообщение...
            tmpText = vbNullString
            m = InStr(Tmp3, Chr(0))
            LastSeek = LastSeek + m
            'Seek(fNum, LastSeek)
            fs.Seek(LastSeek, SeekOrigin.Begin)
            Tmp3 = Space(1024)

            Do
                LastSeek = fs.Position
                'FileGet(fNum, Tmp3)
                Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))
                m = InStr(1, Tmp3, Chr(0))
                If m > 0 Then
                    tmpText = tmpText & Left(Tmp3, m - 1)
                    LastSeek += Left(Tmp3, m - 1).Length
                    Exit Do
                Else
                    tmpText &= Tmp3
                    LastSeek += Tmp3.Length
                End If

            Loop

            tmpMess.Msg = Split(tmpText, Chr(13))
            Messages(i) = tmpMess

            'Seek(fNum, LastSeek)
            fs.Seek(LastSeek, SeekOrigin.Begin)
            'FileGet(fNum, Tmp3)
            Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))

            m = InStr(1, Tmp3, New String(Chr(0), 2))

            If InStr(1, Tmp3, New String(Chr(0), 2)) <> m Then
                m += 1
            End If

            If InStr(1, Tmp3, New String(Chr(0), 4)) = m Then
                m += 1
            End If

            If m > 0 Then
                LastSeek += +m + 3

                'Seek(fNum, LastSeek)
                'FileGet(fNum, Tmp3)
                fs.Seek(LastSeek, SeekOrigin.Begin)
                Tmp3 = Encoding.GetEncoding(866).GetString(br.ReadBytes(1024))

                m = InStr(1, Tmp3, Chr(0))
                If m > 0 Then
                    LastSeek += m
                End If
            End If
            'Seek(fNum, LastSeek)
            fs.Seek(LastSeek, SeekOrigin.Begin)
        Next  'i

        br.Close()
        fs.Close()

    End Sub

End Module