Imports Patagames.Ocr
Public Class Form2

#Region "Auto Scaling ..."

    '    Dim ProportionsArray() As CtrlProportions

    '    Private Structure CtrlProportions
    '        Dim HeightProportions As Double
    '        Dim WidthProportions As Double
    '        Dim TopProportions As Double
    '        Dim LeftProportions As Double
    '    End Structure

    '    Private Sub Form2_HandleCreated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.HandleCreated
    '        On Error Resume Next
    '        Application.DoEvents()
    '        ReDim ProportionsArray(0 To Controls.Count - 1)
    '        For I As Integer = 0 To Controls.Count - 1
    '            With ProportionsArray(I)
    '                .HeightProportions = Controls(I).Height / Height
    '                .WidthProportions = Controls(I).Width / Width
    '                .TopProportions = Controls(I).Top / Height
    '                .LeftProportions = Controls(I).Left / Width
    '            End With
    '        Next
    '    End Sub

    '    Private Sub Form2_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
    '        On Error Resume Next
    '        For I As Integer = 0 To Controls.Count - 1
    '            If Controls(I).Name.Contains("Button") Or Controls(I).Name.Contains("Check") Then Continue For
    '            If Controls(I).Name.Contains("Picture") = False Then Controls(I).Left = Math.Ceiling(ProportionsArray(I).LeftProportions * Me.Width)
    '            Controls(I).Top = Math.Ceiling(ProportionsArray(I).TopProportions * Me.Height)
    '            Controls(I).Width = Math.Ceiling(ProportionsArray(I).WidthProportions * Me.Width)
    '            Controls(I).Height = Math.Ceiling(ProportionsArray(I).HeightProportions * Me.Height)
    '        Next
    '        
    '    End Sub

#End Region


#Region "Form Tools Events ..."

    Private Sub Form2_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        PictureBox1.Width = Me.Width - PictureBox1.Left - 20
        PictureBox1.Height = TextBox1.Top - 10
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        NotifyIcon1.Dispose()
        End
    End Sub

    Private Sub StartToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles StartToolStripMenuItem.Click
        Call checkStartup()
    End Sub

    Private Sub Form2_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        e.Cancel = True
        Me.Hide()
        Me.Visible = False
        Me.ShowInTaskbar = False
        Me.WindowState = FormWindowState.Minimized
        GC.Collect()
        GC.WaitForPendingFinalizers()
    End Sub

    Private Sub Form2_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        '// Set Program To Run At Startup
        Dim Path As String
        Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\ScreenTranslator\FirstStart.Txt"
        Dim FirstStart As Boolean
        FirstStart = Not My.Computer.FileSystem.FileExists(Path)
        If FirstStart Then
            Dim msg As String = "Please ,,, This program was developed to be a quick and simple translator from English language to Arabic language for special purpose." + vbCrLf + vbCrLf
            msg += "If you want to develop it, you can download TessData for the language you want and a little things in this source code ..." + vbCrLf + vbCrLf
            msg += "After you start the program, you need to press {Ctrl} & {Shift} to translate a text directly from screen,"
            msg += "or you can copy any text after selection by {Ctrl} + {C Key}, Then Press {Ctrl} + {Alt} To Show The Translation of the copied text."
            msg += vbCrLf + vbCrLf + "Do you want the system to run this program at startup ?"

            If MessageBox.Show(msg, "INFO ...", MessageBoxButtons.YesNo, MessageBoxIcon.Information) = Windows.Forms.DialogResult.Yes Then
                My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", My.Application.Info.ProductName, """" + Application.ExecutablePath + """")
            End If
            My.Computer.FileSystem.CreateDirectory(Mid(Path, 1, Path.IndexOf("\FirstStart.Txt") + 1))
            My.Computer.FileSystem.WriteAllText(Path, "Nothing", False)
        End If

        '// OCR
        Call PrepareAPI()

        '// Window Options
        Me.Hide()
        Me.Visible = False
        Me.ShowInTaskbar = False
        Me.TopMost = True
        Me.TopLevel = True

        'My.Computer.FileSystem.DeleteFile(Path)
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        TextBox2.Clear()
    End Sub

#End Region




    '// Hotkeys 
    '// You Can Use RegisterHotKey API !
    Dim x As New Object
    Private Sub Timer1_Tick(sender As System.Object, e As System.EventArgs) Handles Timer1.Tick
        SyncLock x
            Timer1.Interval = 1
            If My.Computer.Keyboard.ShiftKeyDown And My.Computer.Keyboard.CtrlKeyDown Then
                '// Translation --- Screen
                Timer1.Enabled = False
                If InternetGetConnectedState(0, 0) = False Then
                    MessageBox.Show("There Is No Internet Connection", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Else
                    Form1.Show()
                End If
                Timer1.Interval = 2000
                Timer1.Enabled = True


            ElseIf My.Computer.Keyboard.CtrlKeyDown And My.Computer.Keyboard.AltKeyDown Then
                '// Translation --- Copied Text
                Timer1.Enabled = False
                If InternetGetConnectedState(0, 0) = False Then
                    MessageBox.Show("There Is No Internet Connection", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Else
                    Dim str As String = ""

                    Try
                        str = Clipboard.GetText()
                        Clipboard.Clear()
                    Catch
                        Clipboard.Clear()
                    End Try

                    If str = "" Then
                    Else
                        Dim TranslatedText As String = Translate(str)
                        If TranslatedText.Trim = "" Then
                            MessageBox.Show("The text hasn't been translated ...", "Try Again ...", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Else
                            MessageBox.Show(str + vbCrLf + "--------------------------" + vbCrLf + vbCrLf + TranslatedText, "Done Successfully ...", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If
                    End If
                End If

                Timer1.Interval = 1000
                Timer1.Enabled = True
            End If
        End SyncLock
    End Sub

    Public orgImg As Bitmap
    Private Sub OCR_Btn_Click(sender As System.Object, e As System.EventArgs) Handles OCR_Btn.Click
        Dim BoxImage As Bitmap = orgImg.Clone()

        '// Gray Img
        If CheckBox1.Checked Then
            getGrayImg(BoxImage)
            PictureBox1.Image = BoxImage.Clone
        Else
            PictureBox1.Image = orgImg.Clone
        End If

        '// OCR
        Dim ReqStr As String = GetTextFromImage(BoxImage.Clone).Trim
        If ReqStr.Length <= 2 Then ReqStr = "Nothing has been found in the image." ' , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        TextBox1.Text = ReqStr
    End Sub

    Private Sub TranslateBtn_Click(sender As System.Object, e As System.EventArgs) Handles TranslateBtn.Click
        Dim Str As String = TextBox1.Text.Trim
        If Str.Length <= 2 Then
            MessageBox.Show("The Text Has Not Been Completed Yet ...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Else
            TextBox2.Text = Translate(Str)
            'If TextBox2.Text.Trim.Length <= 2 Then
            '    TextBox2.Text = "لم نستطع القيام بالترجمة"
            'End If
        End If
    End Sub
End Class

