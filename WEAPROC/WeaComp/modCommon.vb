Imports System.Windows.Forms

''' <summary>
''' Commonly used routines in several applications
''' </summary>
Module modCommon
    Friend Sub OnThreadException(sender As Object, e As System.Threading.ThreadExceptionEventArgs)
        ' This is where you handle the exception
        MessageBox.Show(e.Exception.ToString, "Unhanded Exception", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ''' <summary>
    ''' Provide string.split functionality where separator is not a character but is a string. 
    ''' Note: individual elements are trimmed of spaces
    ''' </summary>
    ''' <param name="StringToSplit">Original string</param>
    ''' <param name="SeparatorString">String to use as separator</param>
    ''' <param name="RemoveEmptyEntries">If true, will remove empty entries in array</param>
    ''' <returns>Array of components</returns>
    ''' <remarks>For example, if you want to split "A - B" call SplitString("A - B"," - ") to get A,B</remarks>

    Friend Function StringSplit(StringToSplit As String, SeparatorString As String, Optional RemoveEmptyEntries As Boolean = False) As String()
        Dim ar() As String = StringToSplit.Replace(SeparatorString, "`").Split(New Char() {"`"}, IIf(RemoveEmptyEntries, StringSplitOptions.RemoveEmptyEntries, StringSplitOptions.None))
        For i As Integer = 0 To ar.Length - 1
            ar(i) = ar(i).Trim
        Next
        Return ar
    End Function

    ''' <summary>
    ''' Name of directory where data will be stored by default (My Documents>\WRDB)
    ''' </summary>
    ''' <remarks></remarks>
    Friend Function MyDataPath() As String
        Dim s As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\LSPC"
        If Not My.Computer.FileSystem.DirectoryExists(s) Then My.Computer.FileSystem.CreateDirectory(s)
        Return s
    End Function

    ''' <summary>
    ''' Wrapper for String.Compare to test if two strings are equal, ignoring case
    ''' </summary>
    Public Function StringsEqual(String1 As String, String2 As String) As Boolean
        Return String.Compare(String1, String2, True) = 0
    End Function

#Region "Warning and error message dialogs"

    'Private Sub LaunchErrorForm(Args() As Object)
    'With New frmError(Args(0), Args(1))
    '.BringToFront()
    '.ShowDialog()
    'End With
    'End Sub

#End Region

    ''' <summary>
    ''' Test object; if dbnull, nothing, empty string, or minimum value, return default value
    ''' </summary>
    Friend Function TestNull(Value As Object, DefaultValue As Object) As Object
        If IsDBNull(Value) OrElse
            IsNothing(Value) OrElse
            (TypeOf Value Is String AndAlso String.IsNullOrEmpty(Value)) OrElse
            (TypeOf Value Is Date AndAlso Value = DateTime.MinValue) OrElse
            (IsNumeric(Value) AndAlso Value = Single.MinValue) Then Return DefaultValue Else Return Value
    End Function

#Region " Min/Max Routines "

    Friend Function Min(ParamArray List() As Int32) As Int32
        Dim MinItem As Int32
        Dim i As Short
        MinItem = List(0)
        For i = 1 To UBound(List)
            If List(i) < MinItem Then MinItem = List(i)
        Next i
        Return MinItem
    End Function

    Friend Function Min(ParamArray List() As Double) As Double
        Dim MinItem As Double
        Dim i As Short
        MinItem = List(0)
        For i = 1 To UBound(List)
            If List(i) < MinItem Then MinItem = List(i)
        Next i
        Return MinItem
    End Function

    Friend Function Min(ParamArray List() As Single) As Single
        Dim MinItem As Double
        Dim i As Short
        MinItem = List(0)
        For i = 1 To UBound(List)
            If List(i) < MinItem Then MinItem = List(i)
        Next i
        Return MinItem
    End Function

    Friend Function Min(ParamArray List() As String) As String
        Dim MinItem As String
        Dim i As Short
        MinItem = List(0)
        For i = 1 To UBound(List)
            If List(i) < MinItem Then MinItem = List(i)
        Next i
        Return MinItem
    End Function

    Friend Function Min(ParamArray List() As Date) As Date
        Dim MinItem As String
        Dim i As Short
        MinItem = List(0)
        For i = 1 To UBound(List)
            If List(i) < MinItem Then MinItem = List(i)
        Next i
        Return MinItem
    End Function

    Friend Function Max(ParamArray List() As Int32) As Int32
        Dim MaxItem As Int32
        Dim i As Short
        MaxItem = List(0)
        For i = 1 To UBound(List)
            If List(i) > MaxItem Then MaxItem = List(i)
        Next i
        Return MaxItem
    End Function

    Friend Function Max(ParamArray List() As Double) As Double
        Dim MaxItem As Double
        Dim i As Short
        MaxItem = List(0)
        For i = 1 To UBound(List)
            If List(i) > MaxItem Then MaxItem = List(i)
        Next i
        Return MaxItem
    End Function

    Friend Function Max(ParamArray List() As Single) As Single
        Dim MaxItem As Single
        Dim i As Short
        MaxItem = List(0)
        For i = 1 To UBound(List)
            If List(i) > MaxItem Then MaxItem = List(i)
        Next i
        Return MaxItem
    End Function

    Friend Function Max(ParamArray List() As String) As String
        Dim MaxItem As String
        Dim i As Short
        MaxItem = List(0)
        For i = 1 To UBound(List)
            If List(i) > MaxItem Then MaxItem = List(i)
        Next i
        Return MaxItem
    End Function

    Friend Function Max(ParamArray List() As Date) As Date
        Dim MaxItem As String
        Dim i As Short
        MaxItem = List(0)
        For i = 1 To UBound(List)
            If List(i) > MaxItem Then MaxItem = List(i)
        Next i
        Return MaxItem
    End Function

#End Region

    Private Declare Auto Function GetShortPathName Lib "kernel32.dll" (path As String, shortname As System.Text.StringBuilder, maxlen As Integer) As Integer
    Public Function GetShortName(path As String) As String
        Dim sb As New System.Text.StringBuilder(259)
        Dim retval As Integer = GetShortPathName(path, sb, 259)
        If retval = 0 Then Throw New IO.IOException("Could not retrieve short name", retval)
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Capitalize first character and any characters following a space,|,\,;,.,_,(
    ''' </summary>
    Public Function Proper(s As String) As String
        Return Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower)
    End Function

    Public Function GetUserName() As String
        Return Environment.UserName
    End Function

    Public Function ItemIn(Target As Object, ParamArray Items() As Object) As Boolean
        'see if target is in list of items
        'usage: ItemIn("B","A","B","C") results in True
        Dim lst As New Generic.List(Of Object)
        lst.AddRange(Items)
        Return lst.Contains(Target)

        'For Each Item As Object In Items
        '    If Target = Item Then Return True
        'Next Item
        'Return False
    End Function

    ''' <summary>
    ''' Cannot embed newline characters in tooltips and help text Import VB; 
    ''' Recursively look at all such text and replace \n with Newline and \t with tab
    ''' Will also do this for labels and textbox entries
    ''' </summary>
    ''' <param name="CtlParent">Parent control (start with form)</param>
    ''' <param name="Tool">Name of tooltip provider</param>
    ''' <param name="Help">Name of helpprovider</param>
    ''' <remarks></remarks>
    <System.Diagnostics.DebuggerStepThrough()>
    Friend Sub ReplaceTooltipAndHelpText(CtlParent As Control, Tool As ToolTip, Help As HelpProvider)

        For Each ctl As Windows.Forms.Control In CtlParent.Controls

            If Tool IsNot Nothing Then
                Dim s As String = Tool.GetToolTip(ctl)
                s = s.Replace("\n", Environment.NewLine)
                s = s.Replace("\t", vbTab)
                Tool.SetToolTip(ctl, s)
            End If

            If TypeOf ctl Is Label Then
                CType(ctl, Label).Text = CType(ctl, Label).Text.Replace("\n", Environment.NewLine)
            End If

            If TypeOf ctl Is TextBox Then
                CType(ctl, TextBox).Text = CType(ctl, TextBox).Text.Replace("\n", Environment.NewLine)
            End If

            If Help IsNot Nothing Then
                Dim s As String = Help.GetHelpString(ctl)
                If Not s Is Nothing Then
                    s = s.Replace("\n", Environment.NewLine)
                    s = s.Replace("\t", vbTab)
                    Help.SetHelpString(ctl, s)
                End If
            End If
            ReplaceTooltipAndHelpText(ctl, Tool, Help)
        Next
    End Sub

    Friend Function InterpolateY(TargetX As Single, X1 As Single, Y1 As Single, X2 As Single, Y2 As Single) As Single
        If X1 = X2 Then Return (Y1 + Y2) / 2
        Return Y1 + (TargetX - X1) * (Y2 - Y1) / (X2 - X1)
    End Function

    Friend Function InterpolateY(TargetT As Date, T1 As Date, Y1 As Single, T2 As Date, Y2 As Single) As Single
        If T1 = T2 Then Return (Y1 + Y2) / 2
        Return Y1 + (TargetT.Subtract(T1).TotalHours * (Y2 - Y1) / (T2.Subtract(T1).TotalHours))
    End Function

    ''' <summary>
    ''' Given real number string in culture-invariant format, return value (use in place of CDate)
    ''' </summary>
    ''' <param name="Number">Culture-invariant number format like 1,234.56</param>
    ''' <returns></returns>
    ''' <remarks>Useful when running in non-US culture</remarks>
    Friend Function MyNumber(Number As String) As Double
        Try
            Return Double.Parse(Number, System.Globalization.CultureInfo.InvariantCulture)
        Catch ex As Exception
            Return 0.0
        End Try
    End Function

    ''' <summary>
    ''' Given date-time string in culture-invariant format, return date value (use in place of CDate)
    ''' </summary>
    ''' <param name="DateString">Culture-invariant string format like MM/dd/yyyy HH:mm</param>
    ''' <returns></returns>
    ''' <remarks>Useful when running in non-US culture</remarks>
    Friend Function MyCDate(DateString As String) As Date
        Try
            Return Date.Parse(DateString, System.Globalization.CultureInfo.InvariantCulture)
        Catch ex As Exception
            Return Date.MinValue
        End Try
    End Function

    ''' <summary>
    ''' Given date-time value return short date-time format string that obeys culture preferences (for US, gives MM/dd/yyyy HH:mm)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Cache results to improve performance</remarks>
    Friend Function MyDateTimeFormat() As String
        Static CachedValue As String = Nothing
        If Not String.IsNullOrEmpty(CachedValue) Then Return CachedValue
        CachedValue = MyDateFormat() & " HH:mm"
        Return CachedValue
    End Function

    ''' <summary>
    ''' Given date-time value return short date format string that obeys culture preferences (for US, gives MM/dd/yyyy)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Cache results to improve performance</remarks>
    Friend Function MyDateFormat() As String
        Static CachedValue As String = Nothing
        If Not String.IsNullOrEmpty(CachedValue) Then Return CachedValue
        With Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat
            CachedValue = IIf(Threading.Thread.CurrentThread.CurrentCulture.Name = "en-US", "MM/dd/yyyy", .ShortDatePattern)
            Return CachedValue
        End With
    End Function

    ''' <summary>
    ''' Looks up chain to see if control (or a parent) is in design mode, so some initialization can be skipped to avoid designer errors
    ''' </summary>
    ''' <param name="Cntl">Control to check</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function IsDesignMode(Cntl As Control) As Boolean
        If Cntl Is Nothing Then Return False
        If Cntl.Site IsNot Nothing AndAlso Cntl.Site.DesignMode Then
            Return True
        Else
            Return IsDesignMode(Cntl.Parent)
        End If
    End Function

#Region "Encryption/Decryption Routines"

    ' Encrypt the text
    Friend Function EncryptText(strText As String) As String
        Return Encrypt(strText, "&%#@?,:*")
    End Function

    'Decrypt the text 
    <DebuggerStepThrough()>
    Friend Function DecryptText(strText As String) As String
        Return Decrypt(strText, "&%#@?,:*")
    End Function

    'The function used to encrypt the text
    Private Function Encrypt(strText As String, strEncrKey As String) As String
        If strText = "" Then Return ""
        Dim byKey() As Byte = {}
        Dim IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}

        byKey = System.Text.Encoding.UTF8.GetBytes(Left(strEncrKey, 8))

        Dim des As New System.Security.Cryptography.DESCryptoServiceProvider()
        Dim inputByteArray() As Byte = System.Text.Encoding.UTF8.GetBytes(strText)
        Dim ms As New IO.MemoryStream()
        Dim cs As New Security.Cryptography.CryptoStream(ms, des.CreateEncryptor(byKey, IV), Security.Cryptography.CryptoStreamMode.Write)
        cs.Write(inputByteArray, 0, inputByteArray.Length)
        cs.FlushFinalBlock()
        Return Convert.ToBase64String(ms.ToArray())
    End Function

    'The function used to decrypt the text
    <DebuggerStepThrough()>
    Private Function Decrypt(strText As String, sDecrKey As String) As String
        If strText = "" Then Return ""
        Dim byKey() As Byte = {}
        Dim IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}
        Dim inputByteArray(strText.Length) As Byte

        Try
            byKey = System.Text.Encoding.UTF8.GetBytes(Left(sDecrKey, 8))
            Dim des As New Security.Cryptography.DESCryptoServiceProvider()
            inputByteArray = Convert.FromBase64String(strText)
            Dim ms As New IO.MemoryStream()
            Dim cs As New Security.Cryptography.CryptoStream(ms, des.CreateDecryptor(byKey, IV), Security.Cryptography.CryptoStreamMode.Write)

            cs.Write(inputByteArray, 0, inputByteArray.Length)
            cs.FlushFinalBlock()
            Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8

            Return encoding.GetString(ms.ToArray())
        Catch ex As Exception
            Throw New ApplicationException("Unable to decrypt password; please respecify.")
            Return ""
        End Try
    End Function

#End Region

    ''' <summary>
    ''' Given a path or file, return the path relative to MyDocuments\WRDB
    ''' </summary>
    Public Function RelativePath(Path As String) As String
        Dim MyPath As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\WRDB\"
        If IO.Path.IsPathRooted(Path) AndAlso Path.StartsWith(MyPath, StringComparison.OrdinalIgnoreCase) AndAlso Not String.Equals(MyPath, Path, StringComparison.OrdinalIgnoreCase) Then
            Return Path.Substring(MyPath.Length)
        Else
            Return Path
        End If
    End Function

    ''' <summary>
    ''' Given a path or file possibly relative to MyDocuments\WRDB, return the absolute path 
    ''' </summary>
    Public Function AbsolutePath(Path As String, Optional Ext As String = "") As String
        If Path = "" Then Return ""
        Dim MyPath As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\WRDB"
        If Not IO.Path.IsPathRooted(Path) Then
            Return MyPath & "\" & Path & IIf(Ext <> "" AndAlso Path.EndsWith(Ext, StringComparison.OrdinalIgnoreCase), "", Ext)
        Else
            Return Path
        End If
    End Function

    Public Sub SetDefaultProxy()
        Dim Proxy As Net.IWebProxy
        Proxy = Net.WebRequest.GetSystemWebProxy
        Proxy.Credentials = Net.CredentialCache.DefaultCredentials
        Net.WebRequest.DefaultWebProxy = Proxy 'set defaults for most downloads
    End Sub

#Region "Message methods"

    ''' <summary>
    ''' Display warning message
    ''' </summary>
    ''' <param name="WarningText">Warning text to display</param>
    Friend Sub WarningMsg(ByVal WarningText As String)
        MapWinUtility.Logger.Message(WarningText, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, DialogResult.OK)
    End Sub

    ''' <summary>
    ''' Display warning message
    ''' </summary>
    ''' <param name="WarningTextformat">Warning text to display as string format</param>
    ''' <param name="Args">Arguments for string format</param>
    Friend Sub WarningMsg(ByVal WarningTextFormat As String, ByVal ParamArray Args() As Object)
        WarningMsg(StringFormat(WarningTextFormat, Args))
    End Sub

    ''' <summary>
    ''' Display error message
    ''' </summary>
    ''' <param name="ErrorText">Error text to display</param>
    ''' <param name="ex">Exception (will display traceback info)</param>
    Friend Sub ErrorMsg(Optional ByVal ErrorText As String = "", Optional ByVal ex As Exception = Nothing)
        If ErrorText = "" Then ErrorText = "An error has occurred in weather computation."
        If ex IsNot Nothing Then ErrorText &= String.Format("\n\n{0}\n\n\nThe detailed error message was:\n\n{1}", ex.Message, ex.ToString)
        MapWinUtility.Logger.Message(ErrorText.Replace("\n", vbCr), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK)
    End Sub

    ''' <summary>
    ''' Format string Import standard String.Format, except substitute \t and \n with tab and newline characters
    ''' </summary>
    Friend Function StringFormat(ByVal Format As String, ByVal ParamArray Args() As Object) As String
        Return String.Format(Format, Args).Replace("\t", vbTab).Replace("\n", vbNewLine)
    End Function

#End Region

End Module

