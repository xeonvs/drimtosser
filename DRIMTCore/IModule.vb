'Copyright Max 'Xeon' Irgiznov 2005-2009

''' <summary>
''' Интерфейс Динамического Модуля
''' </summary>
''' <remarks></remarks>
Public Interface IModule
    Inherits IDisposable
    Inherits IModuleInfo
    Inherits IDatabasesTypes

    ''' <summary>
    ''' Событие генерируемое после тоссинга каждого пакета
    ''' </summary>
    ''' <param name="sender">Имточник события</param>
    ''' <param name="Percentage">Процент выполнения</param>
    ''' <remarks></remarks>
    Event TossingProgress(ByVal sender As Object, ByVal Percentage As Integer)

    ''' <summary>
    ''' Тоссит pkt в заданную базу
    ''' </summary>
    Sub TossInto()

    ''' <summary>
    ''' Сканирует базу на предмет новых сообщений.
    ''' </summary>
    Sub ScanBase()

    ''' <summary>
    ''' Полное имя поинта
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property PointName() As String
    ''' <summary>
    ''' Путь к базе
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property BasePath() As String
    ''' <summary>
    ''' Путь к каталогу с PKT файлами
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property InboundDir() As String
    ''' <summary>
    ''' Путь к эхолисту
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property EchoListFileName() As String

    ''' <summary>
    ''' Тип базы поддерживаемый данным модулем тоссера
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ReadOnly Property BaseType() As IModule.enmBaseType

End Interface

''' <summary>
''' Интерфейс Информации о Модуле
''' </summary>
Public Interface IModuleInfo

    ''' <summary>
    ''' Ворвращает иконку ассоциированную с модулем
    ''' </summary>
    Function GetModuleIcon() As System.Drawing.Image
    ''' <summary>
    ''' Возвращает описание модуля
    ''' </summary>
    ReadOnly Property Description() As String
    ''' <summary>
    ''' Возвращает Имя модуля
    ''' </summary>
    ReadOnly Property Name() As String

End Interface

''' <summary>
''' Описание доступных типов баз и атрибутов писем
''' </summary>
''' <remarks></remarks>
Public Interface IDatabasesTypes
    ''' <summary>
    ''' Перечисление доступных типов баз
    ''' </summary>
    Enum enmBaseType
        QBBS = 0        'notfound descr.
        Fido = 1        'MSG base
        Squish = 2      'Squish base
        Jam = 3         'JAM base
        Passthru = 7    'Pasthru base
        FIPS = 9        'FIPS BASE
        SQL = 10        'SQL Support
        Unknown = -1    'Unknown base
    End Enum

    ''' <summary>
    ''' Атрибуты Netmail сообщения
    ''' </summary>    
    Enum MSG_ATTRIBUTE
        Msg_Private = &H1S      '/*+Pvt - Личное                        */
        Msg_Crash = &H2S        '/*+Cra - Высокопpиоpитетное            */
        Msg_Read = &H4S         '/* Rec - Пpочитано полyчателем         */
        Msg_Sent = &H8S         '/* Snt - Послано                       */
        Msg_File = &H10S        '/*+Att - Файлатач                      */
        Msg_Transit = &H20S     '/* Trs - Тpанзитное                    */
        Msg_Orphan = &H40S      '/* Orp - Полyчатель не сyществyет      */
        Msg_Kill = &H80S        '/* K/s - Удалить после отсылки         */
        Msg_Local = &H100S      '/* Loc - Создано на данном yзле        */
        Msg_Hold = &H200S       '/* Hld - Отсылка по входящемy звонкy   */
        Msg_FReq = &H800S       '/* FRq - Запpос файла                  */
        Msg_RReq = &H1000S      '/*+RRq - Запpос подтвеpждения пpиема   */
        Msg_IsRR = &H2000S      '/*+RRc - Подтвеpждение пpиема          */
        Msg_AReq = &H4000S      '/*+ARq - Audit request ???             */
        Msg_FUpsReq = &H8000S   '/* URq - Запpос обновления файла       */
    End Enum

    ''' <summary>
    ''' Атрибуты сообщения в JAM базе
    ''' </summary>
    Enum JAM_ATTRIBUTE
        MSG_Local = &H1S            ' Msg created locally
        MSG_Intransit = &H2S        ' Msg is in-transit
        MSG_Private = &H4S          ' Private
        MSG_Read = &H8S             ' Read by addressee
        MSG_Sent = &H10S            ' Sent to remote
        MSG_Killsent = &H20S        ' Kill when sent
        MSG_Archivesent = &H40S     ' Archive when sent
        MSG_Hold = &H80S            ' Hold for pick-up
        MSG_Crash = &H100S          ' Crash
        MSG_Immediate = &H200S      ' Send Msg now, ignore restrictions
        MSG_Direct = &H400S         ' Send directly to destination
        MSG_Gate = &H800S           ' Send via gateway
        MSG_Filerequest = &H1000S   ' File request
        MSG_Fileattach = &H2000S    ' File(s) attached to Msg
        MSG_Truncfile = &H4000S     ' Truncate file(s) when sent
        MSG_Killfile = &H8000S      ' Delete file(s) when sent
        MSG_Receiptreq = &H10000    ' Return receipt requested
        MSG_Confirmreq = &H20000    ' Confirmation receipt requested
        MSG_Orphan = &H40000        ' Unknown destination
        MSG_Encrypt = &H80000       ' Msg text is encrypted
        MSG_Сompress = &H100000     ' Msg text is compressed
        MSG_Escaped = &H200000      ' Msg text is seven bit ASCII
        MSG_Fpu = &H400000          ' Force pickup
        MSG_Typelocal = &H800000    ' Msg is for local use only
        MSG_Typeecho = &H1000000    ' Msg is for conference distribution
        MSG_Typenet = &H2000000     ' Msg is direct network mail
        MSG_Nodisp = &H20000000     ' Msg may not be displayed to user
        MSG_Locked = &H40000000     ' Msg is locked, no editing possible
        MSG_Deleted = &H80000000    ' Msg is deleted
    End Enum

    ''' <summary>
    ''' Атрибуты сообщения в FIPS базе
    ''' </summary>
    Enum FIPS_ATTRIBUTE
        MSG_PRIVATE = &H1   'Частное письмо
        MSG_CRASH = &H2     'Отправить немедленно
        MSG_RECD = &H4      'Письмо получено
        MSG_SENT = &H8      'Отправлено удачно
        MSG_FILE = &H10     'К письму прикреплен аттач
        MSG_FWD = &H20      'Письмо было отфорваржено
        MSG_ORPHAN = &H40   'Неизвестна нода-получатель
        MSG_KILL = &H80     'Удалить после отправки
        MSG_LOCAL = &H100   'Сообщение написано на Вашей станции
        MSG_XX1 = &H200     'Hold
        MSG_XX2 = &H400     'Reserved
        MSG_FRQ = &H800     'Файловый запрос
        MSG_RRQ = &H1000    'Запрос принят
        MSG_CPT = &H2000    'Письмо - подтверждение приема
        MSG_ARQ = &H4000    'Запрос пути следования
        MSG_URQ = &H8000    'Обновление запроса
    End Enum

    ''' <summary>
    ''' Атрибуты сообщения в Sqish базе
    ''' </summary>
    Enum SQ_ATTRIBUTE
        MSG_PRIVATE = &H1   'Частное письмо
        MSG_CRASH = &H2     'Отправить немедленно
        MSG_RECD = &H4      'Письмо получено
        MSG_SENT = &H8      'Отправлено удачно
        MSG_FILE = &H10     'К письму прикреплен аттач
        MSG_FWD = &H20      'Письмо было отфорваржено
        MSG_ORPHAN = &H40   'Неизвестна нода-получатель
        MSG_KILL = &H80     'Удалить после отправки
        MSG_LOCAL = &H100   'Сообщение написано на Вашей станции
        MSG_XX1 = &H200     'Hold
        MSG_XX2 = &H400     'Reserved
        MSG_FRQ = &H800     'Файловый запрос
        MSG_RRQ = &H1000    'Запрос принят
        MSG_CPT = &H2000    'Письмо - подтверждение приема
        MSG_ARQ = &H4000    'Запрос пути следования
        MSG_URQ = &H8000    'Обновление запроса
        MSG_SCAN = &H10000L 'Squish scanned
        MSG_UMSG = &H20000L 'Valid xmsg.umsg
    End Enum

End Interface

