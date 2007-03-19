Option Explicit On
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
            Return IDatabasesTypes.enmBaseType.Jam
        End Get
    End Property

    ''' <summary>
    ''' Тоссит пакеты в Inbound
    ''' </summary>
    ''' <remarks>В inbound уже должны находится *.pkt</remarks>
    Public Sub Toss() Implements IModule.TossInto
        '
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


#Region "ModuleInfo"
    Public Function GetModuleIcon() As System.Drawing.Image Implements IModule.GetModuleIcon
        Return My.Resources.ModuleIcon.ToBitmap
    End Function

    Public ReadOnly Property Description() As String Implements IModuleInfo.Description
        Get
            Return "JAM Toss Module"
        End Get
    End Property

    Public ReadOnly Property Name() As String Implements IModuleInfo.Name
        Get
            Return "jamTosser"
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
