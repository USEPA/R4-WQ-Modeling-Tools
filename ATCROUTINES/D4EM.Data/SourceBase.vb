Public MustInherit Class SourceBase
    Implements ISource
    'Implements MapWindow.Interfaces.IPlugin

    Public Overridable Function Execute(ByVal aQuerySchema As String) As String Implements ISource.Execute
        Throw New ApplicationException("Execute not defined")
    End Function

    Public Overridable ReadOnly Property QuerySchema() As String Implements ISource.QuerySchema
        Get
            Throw New ArgumentException("QuerySchema not defined")
        End Get
    End Property

    Public Overridable ReadOnly Property Author() As String Implements ISource.Author
        Get
            Return "AQUA TERRA Consultants"
        End Get
    End Property

    Public Overridable ReadOnly Property Description() As String Implements ISource.Description
        Get
            Return ""
        End Get
    End Property

    Public Overridable ReadOnly Property Name() As String Implements ISource.Name
        Get
            Return ""
        End Get
    End Property

    Public Overridable ReadOnly Property Version() As String Implements ISource.Version
        Get
            Return System.Diagnostics.FileVersionInfo.GetVersionInfo(Me.GetType().Assembly.Location).FileVersion
        End Get
    End Property

    'Public Overridable ReadOnly Property BuildDate() As String Implements MapWindow.Interfaces.IPlugin.BuildDate
    '    Get
    '        Return System.IO.File.GetLastWriteTime(Me.GetType().Assembly.Location).ToString()
    '    End Get
    'End Property

End Class