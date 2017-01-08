Imports System.Data.SqlClient
Imports System.Data
Public Class AlgoSelectForm
    Private Sub UserControl1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' when our component is loaded, we check for the existence of the database
        'Dim con As New SqlConnection
        'Dim cmd As New SqlCommand
        'cmd.Connection = con
        'cmd.CommandText = "IF table does not exists then create table named classifier_db"
        'Try
        'cmd.ExecuteNonQuery()
        'Catch ex As Exception
        'MsgBox("The Classifier Database Does Not exist in the system : " + ex.Message)
        'con.Close()
        'Application.Exit()
        'End Try
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If RadioButton4.Checked = True Then
            ClassifierTrainningForm.ShowDialog()
        End If
        If RadioButton5.Checked = True Then
            ClassifierForm.ShowDialog()
        End If
        Me.SuspendLayout()
    End Sub
End Class