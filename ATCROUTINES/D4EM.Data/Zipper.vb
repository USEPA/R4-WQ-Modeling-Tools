Imports MapWinUtility
Imports Ionic.Zip

''' <summary>
''' Wrapper class around DotNetZip for unziping files
''' </summary>
''' <remarks>requires Ionic.Zip.dll</remarks>
Public Class Zipper

    ''' <summary>
    ''' Unzip aZipFilename into aDestinationFolder
    ''' </summary>
    ''' <param name="aZipFilename"></param>
    ''' <param name="aDestinationFolder"></param>
    ''' <param name="aIncrementProgressAfter"></param>
    ''' <param name="aProgressSameLevel"></param>
    ''' <remarks>
    ''' If file extension is .exe, attempt to self-extract first.
    ''' Progress available using MapWinUtility.Logger.
    ''' </remarks>
    Public Shared Sub UnzipFile(ByVal aZipFilename As String, _
                                ByVal aDestinationFolder As String, _
                       Optional ByVal aIncrementProgressAfter As Boolean = False, _
                       Optional ByVal aProgressSameLevel As Boolean = False)
        Using lLevel As New MapWinUtility.ProgressLevel(aIncrementProgressAfter, aProgressSameLevel)
            Try
                If IO.Path.GetExtension(aZipFilename).ToLower.EndsWith(".exe") Then
                    Logger.Status("Self-Extract " & aZipFilename & " to " & aDestinationFolder, True)

                    Dim lFile As New System.IO.FileStream(aZipFilename, IO.FileMode.Open, IO.FileAccess.Read)
                    Dim lIsExecutable As Boolean = lFile.ReadByte = Asc("M") AndAlso lFile.ReadByte = Asc("Z")
                    lFile.Close()
                    If lIsExecutable Then
                        Try
                            Shell("""" & aZipFilename + """ /auto """ + aDestinationFolder & """", AppWinStyle.Hide, True, 20000)
                        Catch e As Exception
                            Logger.Dbg("Failed to self-extract: " & e.Message)
                            GoTo UnZipIt
                        End Try
                    Else
                        Throw New ApplicationException("Self-extracting file '" & aZipFilename & "' is not EXE format. This is probably because download was unsuccessful.")
                    End If
                Else
UnZipIt:
                    Logger.Status("Unzip " & aZipFilename & " to " & aDestinationFolder, True)
                    Using lUnzipper As ZipFile = ZipFile.Read(aZipFilename)
                        Dim lUnzipTotal As Long = 0
                        For Each lZipEntry As ZipEntry In lUnzipper
                            lUnzipTotal += lZipEntry.UncompressedSize
                        Next
                        pUnzipShift = 0
                        While lUnzipTotal > Integer.MaxValue
                            lUnzipTotal <<= 1
                            pUnzipShift += 1
                        End While
                        pUnzipTotal = CInt(lUnzipTotal)
                        pUnzippedSoFar = 0
                        AddHandler lUnzipper.ExtractProgress, AddressOf MyExtractProgress
                        lUnzipper.ExtractAll(aDestinationFolder, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently)
                    End Using
                End If
            Catch lException As Exception
                Logger.Dbg("Error unzipping:" & lException.Message)
                Throw New ApplicationException("Unable to unzip '" & aZipFilename _
                                                        & "' to '" & aDestinationFolder _
                                                           & "': " & lException.Message, lException)
            End Try
        End Using
    End Sub

    'Private variables to keep track of total zipfile progress since Ionic.Zip progress messages only keep track per entry
    ' and we need Integers for Logger.Progress. Not thread safe if more than one unzip is in progress at the same time
    Private Shared pUnzippedSoFar As Integer = 0
    Private Shared pUnzipTotal As Integer = 0
    Private Shared pUnzipShift As Integer = 0 'Number of bits to shift to move Long progress into Integer range

    Private Shared Sub MyExtractProgress(ByVal sender As Object, ByVal e As ExtractProgressEventArgs)
        Dim lLastByte As Long = e.TotalBytesToTransfer
        If lLastByte > 0 Then
            Dim lCurrentByte As Long = e.BytesTransferred
            Dim lCurrentInt As Integer = CInt(lCurrentByte << pUnzipShift)
            Logger.Progress(pUnzippedSoFar + lCurrentInt, pUnzipTotal)
            If lCurrentByte = lLastByte Then
                pUnzippedSoFar += lCurrentInt
            End If
        End If
    End Sub

End Class
