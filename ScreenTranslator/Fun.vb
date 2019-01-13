Imports System.Runtime.InteropServices
Imports Patagames.Ocr
Public Module Fun


#Region "Translating OCR & WebRequest Stuff ..."
    Dim API As Patagames.Ocr.OcrApi
    Dim WhiteList As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz."

    Public Sub PrepareAPI()
        API = OcrApi.Create
        API.Init(Enums.Languages.English, Application.StartupPath, Enums.OcrEngineMode.OEM_DEFAULT)
        API.SetVariable("tessedit_char_whitelist", WhiteList) '// Case Sensitive
    End Sub

    Public Function GetTextFromImage(img As Bitmap, Optional percentOfZoom As Int32 = 4) As String
        Try
            img = img.Clone()

            '// Resize The Image To A Best OCR Processing
            Dim x As New Bitmap(img.Width * percentOfZoom, img.Height * percentOfZoom)
            Using G As Graphics = Graphics.FromImage(x)
                G.DrawImage(img, New Rectangle(0, 0, x.Width, x.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel)
            End Using

            '// Bypass Patagames OCR Limition ... 
            '// Width Must Be Smaller Than 500 || For other images, the allowed widths are 500 - 550 pixels; 600 - 650 pixels; 700 - 750 pixels and so on  
            '// As A Result Of That, We Will Increase Width 
            Dim Bit As Bitmap
            If x.Width >= 550 And ((x.Width \ 50) Mod 2 = 1 Or x.Width Mod 50 = 0) Then '// ### Between xx51 And xx99 OR 800,,600,,750,,650
                Dim NewWidth As Int32
                If x.Width Mod 50 = 0 AndAlso (x.Width \ 50) Mod 2 = 1 Then '// ### 850,,950,,750 ==After== 901,,1001,,801
                    NewWidth = 51
                ElseIf x.Width Mod 50 = 0 AndAlso (x.Width \ 50) Mod 2 = 0 Then '// ### 800,,900,,700 ==After== 801,,901,,701
                    NewWidth = 1
                Else '// ### Between x51 And x99 ,,, 651-699 ==After== 701-749
                    NewWidth = 50
                End If

                Bit = New Bitmap(x.Width + NewWidth, x.Height)
                Using G As Graphics = Graphics.FromImage(Bit)
                    G.DrawImage(x, New Rectangle(0, 0, x.Width, x.Height), 0, 0, x.Width, x.Height, GraphicsUnit.Pixel)
                End Using
                x = Bit
            End If
            Return API.GetTextFromImage(x).Trim
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error In OCR Process", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return String.Empty
        End Try

    End Function

    '// The HTML Code Can Be Changed By Google, Careful !
    'Const ClassName As String = "class=""" + "t0""" + ">"
    'Const Div As String = "</div><div dir=""" + "rtl""" + " class=""" + "o1""" + ">"
    '// Sorry about this stupid way, YES, you can get data by the class name as HTML Code, but I will use my own way

    Public Function Translate(ReqText As String, Optional ByVal Source As String = "en", Optional ByVal Target As String = "ar")
        '// URL::translate.google.com/m?hl=en&sl={Source}tl={Target}&ie=UTF-8&prev=_m&q={TextToTranslateIt}
        '// Classic Google Translator URL '// Must Be Classic !!! 
        Try
            '// Check The String
            If ReqText.Trim = "" Then Return String.Empty
            ReqText = ReqText.Replace("<", "xxx@@xxx1").Replace(">", "xxx@@xxx2")

            '// Get The HTML Code Which Contains Translated Text
            Dim HTM As String
            Dim x As New System.Net.WebClient
            x.Encoding = System.Text.Encoding.GetEncoding("windows-1256") '// Arabic Encoding, But Not UTF8
            HTM = x.DownloadString(String.Format("https://translate.google.com/m?hl=en&sl={0}&tl={1}&ie=UTF-8&prev=_m&q={2}", Source, Target, Web.HttpUtility.UrlDecode(ReqText)))
            x.Dispose()

            '// Extract Traslated Text From The HTML Page
            'Dim StartStr As Int32 = HTM.IndexOf(ClassName) + ClassName.Length + 1 '// +1 Rules of MID
            'Dim StrLen As Int32 = HTM.IndexOf(Div) - StartStr + 1
            'If StartStr < 100 Or StrLen = 0 Then Return String.Empty
            Dim start As Int32
            Dim ending As Int32
            Dim status As Boolean = True
            For i = 0 To HTM.Length - 1
                If AscW(HTM(i)) >= &H600 And AscW(HTM(i)) <= &H6FF Then
                    If status Then
                        status = False
                        '// start = i
                        For n = i To 0 Step -1
                            If (HTM(n) = ">") Then
                                start = n + 1
                                Exit For
                            End If
                        Next
                    Else
                        '// ending = i
                        For n = i To HTM.Length
                            If (HTM(n) = "<") Then
                                ending = n - 1
                                Exit For
                            End If
                        Next
                        Exit For
                    End If
                End If
            Next

            Dim finalStr As String = Web.HttpUtility.HtmlDecode(HTM.Substring(start, ending - start + 1))
            Return IIf(finalStr.Trim.Length >= 2, finalStr.Trim.Replace("xxx@@xxx1", "<").Replace("xxx@@xxx2", ">"), String.Empty)
        Catch
            Return String.Empty
        End Try
    End Function

#End Region

#Region "Windows API's Calling ..."

    <DllImport("wininet")> _
    Public Function InternetGetConnectedState(lpdwFlags As Int32, dwReserved As Int32) As Boolean
    End Function

    Public Sub checkStartup()
        Dim Exist As String
        Const _STR_NOT_IN_THE_AUTO_RUN_CASE = "NOT in the auto-run case."
        Const _STR_IN_THE_AUTO_RUN_CASE = "in the auto-run case."

        Dim x As Microsoft.Win32.RegistryKey
        x = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
        If x.GetValue(My.Application.Info.ProductName, "NotExist") <> ("""" + Application.ExecutablePath + """") Then
            Exist = _STR_NOT_IN_THE_AUTO_RUN_CASE
        Else
            Exist = _STR_IN_THE_AUTO_RUN_CASE
        End If

        If MessageBox.Show("This Program Is " + Exist + vbCrLf + vbCrLf + "Do you want to change this case ?", "Startup", MessageBoxButtons.YesNo, MessageBoxIcon.Information) = Windows.Forms.DialogResult.Yes Then
            If Exist = _STR_NOT_IN_THE_AUTO_RUN_CASE Then
                My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", My.Application.Info.ProductName, """" + Application.ExecutablePath + """")
            Else
                x.DeleteValue(My.Application.Info.ProductName)
            End If
        End If

        x.Close()
    End Sub

    '    <System.Runtime.InteropServices.DllImport("User32")> _
    '    Public Shared Function EmptyClipboard() As Boolean
    '    End Function

    'Public Const MOD_ALT As Int32 = &H1
    'Public Const MOD_CONTROL As Int32 = &H2
    'Public Const MOD_SHIFT As Int32 = &H4
    'Public Const WM_HOTKEY As Int32 = &H312
    '<StructLayout(LayoutKind.Sequential)> _
    'Public Structure MSG
    '    Public hwnd As IntPtr
    '    Public message As Integer
    '    Public wParam As IntPtr
    '    Public lParam As IntPtr
    '    Public time As Integer
    '    Public pt As Point
    'End Structure

    '<DllImport("User32")> _
    'Public Shared Function RegisterHotKey(hWnd As IntPtr, id As Int32, fsModifiers As UInt32, vk As UInt32) As Boolean
    'End Function

    '<DllImport("User32")> _
    'Public Shared Function GetMessage(ByRef lpMsg As MSG, hWnd As Int32, wMsgFilterMin As UInt32, wMsgFilterMax As UInt32) As Boolean
    'End Function

#End Region

#Region "Image Functions "

    Sub getGrayImg(ByRef img As Bitmap)
        Dim Clr As Color
        Dim R_G_B As Int32
        For xx = 0 To img.Width - 1
            For y = 0 To img.Height - 1
                Clr = img.GetPixel(xx, y)
                R_G_B = (Val(Clr.R) + Val(Clr.G) + Val(Clr.B)) / 3
                img.SetPixel(xx, y, Color.FromArgb(R_G_B, R_G_B, R_G_B))
            Next
        Next
    End Sub
    Public Function DrawRectangle(Img As Bitmap, X1 As Int32, Y1 As Int32, X2 As Int32, Y2 As Int32) As Bitmap
        Img = Img.Clone
        Try
            Using Graph As Graphics = Graphics.FromImage(Img)
                If X1 <= X2 And Y1 <= Y2 Then
                    Graph.DrawRectangle(Pens.Red, New Rectangle(X1, Y1, X2 - X1, Y2 - Y1))
                ElseIf X1 <= X2 And Y1 >= Y2 Then
                    Graph.DrawRectangle(Pens.Red, New Rectangle(X1, Y2, X2 - X1, Y1 - Y2))
                ElseIf X1 >= X2 And Y1 >= Y2 Then
                    Graph.DrawRectangle(Pens.Red, New Rectangle(X2, Y2, X1 - X2, Y1 - Y2))
                ElseIf X1 >= X2 And Y1 <= Y2 Then
                    Graph.DrawRectangle(Pens.Red, New Rectangle(X2, Y1, X1 - X2, Y2 - Y1))
                End If
            End Using
            Return Img
        Catch
            Return Img
        End Try
    End Function

    Public Function GetWorkingAreaScreen(Optional ByVal DrawLine As Boolean = True, Optional ByVal YourPenWidth As Single = 5) As Bitmap
        Dim ScreenSize As Size = My.Computer.Screen.WorkingArea.Size
        Dim Bit As New Bitmap(ScreenSize.Width, ScreenSize.Height)
        Using Graph As Graphics = Graphics.FromImage(Bit)
            Graph.CopyFromScreen(0, 0, 0, 0, ScreenSize, CopyPixelOperation.SourceCopy)
            If DrawLine Then Graph.DrawLine(New Pen(Brushes.Red, YourPenWidth), New Point(0, ScreenSize.Height - 2), New Point(ScreenSize.Width, ScreenSize.Height - 2))
        End Using
        Return Bit
    End Function

    Public Function CutImg(Img As Bitmap, RectSource As Rectangle) As Bitmap
        Dim Bit As New Bitmap(Img, RectSource.Width, RectSource.Height)
        Using Graph As Graphics = Graphics.FromImage(Bit)
            Graph.DrawImage(Img, New Rectangle(0, 0, RectSource.Width, RectSource.Height), RectSource.X, RectSource.Y, RectSource.Width, RectSource.Height, GraphicsUnit.Pixel)
        End Using
        Return Bit
    End Function

    '// Just Black And White
    'Using G As Graphics = Graphics.FromImage(BoxImage)
    '    Dim gray_matrix As Single()() = {
    '      New Single() {0.299F, 0.299F, 0.299F, 0, 0},
    '      New Single() {0.587F, 0.587F, 0.587F, 0, 0},
    '      New Single() {0.114F, 0.114F, 0.114F, 0, 0},
    '      New Single() {0, 0, 0, 1, 0},
    '      New Single() {0, 0, 0, 0, 1}
    '    }
    '    Dim ImageAttr As New System.Drawing.Imaging.ImageAttributes
    '    With ImageAttr
    '        .SetColorMatrix(New System.Drawing.Imaging.ColorMatrix(gray_matrix))
    '        .SetThreshold(0.8)
    '    End With
    '    G.DrawImage(BoxImage, New Rectangle(0, 0, BoxImage.Width, BoxImage.Height), 0, 0, BoxImage.Width, BoxImage.Height, GraphicsUnit.Pixel, ImageAttr)
    'End Using


#End Region

End Module
