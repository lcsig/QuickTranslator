Namespace My

    Partial Friend Class MyApplication

        Private Sub MyApplication_UnhandledException(sender As Object, e As ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            If MessageBox.Show(e.Exception.Message + vbCrLf + vbCrLf + "Do you want to continue ?", "UN::ERROR", MessageBoxButtons.YesNo, MessageBoxIcon.Error) = DialogResult.No Then
                End
            End If
        End Sub
    End Class


End Namespace

