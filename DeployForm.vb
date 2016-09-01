Imports System.ComponentModel
Imports System.Threading
Imports Renci.SshNet

Delegate Sub DeleHandler()

Public Class DeployForm

    Private IP As String
    Private Port As String
    Private User As String
    Private Passwd As String
    Private Param As String = ""

    Private Sub DeleFunc()
        Me.Invoke(New DeleHandler(AddressOf Processor))
    End Sub

    Private Sub DeployForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Get Server Info
        IP = MainForm.txtIP.Value
        Port = MainForm.txtPort.Text
        User = MainForm.txtUser.Text
        Passwd = MainForm.txtPasswd.Text
        Dim thProcess As New System.Threading.Thread(AddressOf DeleFunc)
        thProcess.Start()
    End Sub

    Private Sub Loggin(ByVal str As String)
        txtMain.AppendText(str)
    End Sub

    Private Sub Processor()
        'Get Params If Needed
        Dim strInput As String
        If IO.File.Exists(Application.StartupPath & "\Temp\param") = True Then
            Dim sr As New IO.StreamReader(Application.StartupPath & "\Temp\param", System.Text.Encoding.UTF8)
            Dim vars As String
            Do
                vars = sr.ReadLine()
                If "" = vars Then Exit Do
InputParam:
                strInput = InputBox(vars)
                If "" <> strInput Then
                    Param = Param & " " & InputBox(vars)
                Else
                    MessageBox.Show("���벻��Ϊ�ա�", "", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    GoTo InputParam
                End If
            Loop
        End If


        'Create SFTP Connection
        Dim sftpc As SftpClient
        Try
            Loggin("����SFTP����...")
            sftpc = New SftpClient(IP, Port, User, Passwd)
            Loggin("���" & vbCrLf)
            proMain.Value = 5
        Catch ex As Exception
            Loggin("ʧ�ܣ�" & ex.Message & vbCrLf)
            Exit Sub
        End Try


        'Establish SFTP Connection
        Try
            Loggin("����������SFTP������...")
            sftpc.Connect()
            Loggin("���" & vbCrLf)
            proMain.Value = 10
        Catch ex As Exception
            Loggin("ʧ�ܣ�" & ex.Message & vbCrLf)
            Exit Sub
        End Try


        'Upload INSTALL.SH
        Try
            Try
                Loggin("�л���/root/boomvm...")
                sftpc.ChangeDirectory("/root/boomvm")
                Loggin("���" & vbCrLf)
                proMain.Value = 20
            Catch ex0 As Exception
                Loggin("ʧ�ܣ�" & ex0.Message & vbCrLf)
                Try
                    Loggin("����Ŀ¼/root/boomvm...")
                    sftpc.CreateDirectory("/root/boomvm")
                    Loggin("���" & vbCrLf)
                    proMain.Value = 15
                    Loggin("�л���/root/boomvm...")
                    sftpc.ChangeDirectory("/root/boomvm")
                    Loggin("���" & vbCrLf)
                    proMain.Value = 20
                Catch ex1 As Exception
                    Loggin("ʧ�ܣ�" & ex1.Message & vbCrLf)
                    Exit Sub
                End Try
            End Try
            Loggin("�ϴ�install.sh...")
            Dim stm As New IO.FileStream(Application.StartupPath & "\Temp\install.sh", IO.FileMode.Open)
            sftpc.UploadFile(stm, "/root/boomvm/install.sh", True)
            Loggin("���" & vbCrLf)
            proMain.Value = 50
        Catch ex As Exception
            Loggin("ʧ�ܣ�" & ex.Message & vbCrLf)
            Exit Sub
        End Try


        'Create SSH Connection
        Dim sshc As SshClient
        Try
            Loggin("����SSH����...")
            sshc = New SshClient(IP, Port, User, Passwd)
            Loggin("���" & vbCrLf)
            proMain.Value = 55
        Catch ex As Exception
            Loggin("ʧ�ܣ�" & ex.Message & vbCrLf)
            Exit Sub
        End Try


        'Establish SSH Connection
        Try
            Loggin("����������������...")
            sshc.Connect()
            Loggin("���" & vbCrLf)
            proMain.Value = 60
        Catch ex As Exception
            Loggin("ʧ�ܣ�" & ex.Message & vbCrLf)
            Exit Sub
        End Try


        'Grant Execution Permission
        Dim cmd As SshCommand
        Try
            Loggin("��install.sh��ӿ�ִ��Ȩ��...")
            cmd = sshc.CreateCommand("chmod +x /root/boomvm/install.sh")
            cmd.Execute()
            Loggin("���" & vbCrLf)
            proMain.Value = 65
        Catch ex As Exception
            Loggin("ʧ�ܣ�" & ex.Message & vbCrLf)
            Exit Sub
        End Try

        'Process Installation
        Try
            Loggin("���ڽ��а�װ...")
            cmd = sshc.CreateCommand("bash /root/boomvm/install.sh" & Param)
            Dim result As String = cmd.Execute()
            Loggin("���" & vbCrLf)
            proMain.Value = 100
            Loggin(vbCrLf)
            Loggin("�����������ϸ��Ϣ����ʾ���·���" & vbCrLf)
            Loggin(vbCrLf & "====================" & vbCrLf)
            result.Replace(vbLf, vbCrLf)
            txtMain.AppendText(result)
        Catch ex As Exception
            Loggin("ʧ�ܣ�" & ex.Message & vbCrLf)
            Exit Sub
        End Try

    End Sub

    'Closing the window
    Private Sub DeployForm_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        txtMain.Clear()
    End Sub
End Class