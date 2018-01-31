Public Class Form1

    Dim OrgImage As Bitmap
    Dim WidthOfImage As Int32, HeightOfImage As Int32
    Private Sub Form1_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        OrgImage = GetWorkingAreaScreen()
        PictureBox1.Image = OrgImage.Clone
        WidthOfImage = OrgImage.Width
        HeightOfImage = OrgImage.Height

        'Me.WindowState = FormWindowState.Maximized
        Me.Location = New Point(0, 0)
        Me.Size = OrgImage.Size

        Me.ShowInTaskbar = False
        Me.TopLevel = True
        Me.TopMost = True
    End Sub

    Dim First As Boolean
    Dim FirstPoint As Point
    Private Sub PictureBox1_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseDown
        FirstPoint = e.Location
        First = True
        Call PictureBox1_MouseMove(Nothing, e)
    End Sub

    Dim x As New Object
    Private Sub PictureBox1_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseMove
        SyncLock x
            If First Then
                PictureBox1.Image = DrawRectangle(OrgImage.Clone, FirstPoint.X, FirstPoint.Y, e.Location.X, e.Location.Y)
                GC.Collect()
                GC.WaitForPendingFinalizers()
                'Application.DoEvents()
            End If
        End SyncLock
    End Sub

    Private Sub PictureBox1_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseUp
        First = False
        Try
            Dim X1 As Int32 = FirstPoint.X, X2 As Int32 = e.Location.X
            Dim Y1 As Int32 = FirstPoint.Y, Y2 As Int32 = e.Location.Y

            Dim Rect As Rectangle
            If X1 < X2 And Y1 < Y2 Then
                Rect = New Rectangle(X1, Y1, X2 - X1, Y2 - Y1)
            ElseIf X1 < X2 And Y1 > Y2 Then
                Rect = New Rectangle(X1, Y2, X2 - X1, Y1 - Y2)
            ElseIf X1 > X2 And Y1 > Y2 Then
                Rect = New Rectangle(X2, Y2, X1 - X2, Y1 - Y2)
            ElseIf X1 > X2 And Y1 < Y2 Then
                Rect = New Rectangle(X2, Y1, X1 - X2, Y2 - Y1)
            End If

            If Rect.Width <= 10 Or Rect.Height <= 10 Then
                Me.Hide()
                Me.Close()
            Else
                If MessageBox.Show("Do you want to translate this area of that image ?", "Are You Sure ?", MessageBoxButtons.YesNo, MessageBoxIcon.Information) = Windows.Forms.DialogResult.Yes Then
                    Me.Hide()
                    Dim Required As Bitmap = CutImg(OrgImage, Rect)

                    Form2.Show()
                    Form2.Visible = True
                    Form2.WindowState = FormWindowState.Normal
                    Form2.StartPosition = FormStartPosition.CenterScreen
                    AppActivate(Form2.Text)
                    Form2.orgImg = Required.Clone()
                    Form2.OCR_Btn.PerformClick()

                    Me.Close()
                Else
                    PictureBox1.Image = OrgImage.Clone()
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error Occurred", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Hide()
            Me.Close()
        End Try
    End Sub


End Class
