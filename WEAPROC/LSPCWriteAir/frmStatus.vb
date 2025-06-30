Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms

Public Class frmStatus
    Private fMain As frmMain

    Public Sub New(ByRef _frmMain As frmMain)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        fMain = _frmMain
    End Sub

    Public Sub SetStatus(ByVal msg)
        frmLabel.Text = msg
    End Sub

    Public Sub ProcessWeather()
        Try
            fMain.ReadDSNFile()
            fLabel.Text = "Processing " + fMain.NumAir.ToString() + " AirFiles"
            Debug.WriteLine("In User Form: Executed ReadDSNFile")
            fMain.ProcessingAirFiles()

        Catch ex As Exception
            MessageBox.Show("Error!" + vbCrLf + ex.Message + vbCrLf + ex.StackTrace)
        End Try
        Me.Dispose()
    End Sub

    Private Sub btnProcess_Click(sender As Object, e As EventArgs)

    End Sub
End Class