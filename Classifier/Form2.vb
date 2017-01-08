Imports System.Text.RegularExpressions
Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports System.Data
Public Class ClassifierForm
    Dim path As String = ""
    Dim textBox_Str As String = ""
    Dim distinct_Str As String = ""
    Dim count As Integer = 0
    Dim con As New SqlConnection
    Dim totalNumOfFile As Integer = 0
    Dim abort As Boolean = False
    ' This function Browses and lists the items in a list view
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles But_Browse.Click
        FolderBrowserDialog1.ShowDialog()
        ComboBox1.Text = FolderBrowserDialog1.SelectedPath()
        path = ComboBox1.Text
        TextBox1.Text = ""
        totalNumOfFile = 0
        If path <> "" Then
            abort = False
            ListView1.Items.Clear()
            Dim di As New IO.DirectoryInfo(path)
            Dim diar1 As IO.FileInfo() = di.GetFiles()
            Dim dra As IO.FileInfo
            Dim lv As ListViewItem

            'list the names of all files in the specified directory
            For Each dra In diar1
                'ListView1.Items.Add(dra.ToString)
                If dra.Extension = ".txt" Then
                    lv = ListView1.Items.Add(dra.ToString)
                    'The remaining columns are subitems  
                    lv.SubItems.Add("0%")
                    lv.SubItems.Add("None")
                    totalNumOfFile += 1
                End If
            Next
            'MsgBox(totalNumOfFile.ToString)
        End If
    End Sub

    Private Sub But_classify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles But_classify.Click
        TextBox1.Text = ""
        Dim file_name As String
        Dim prog_percent As Integer
        Dim KNN As Integer = 0
        If AlgoSelectForm.RadioButton2.Checked = True Then
            Me.SuspendLayout()
            Form4.ShowDialog()
            If Form4.ComboBox1.Text = "1" Then
                KNN = 20
            ElseIf Form4.ComboBox1.Text = "2" Then
                KNN = 40
            ElseIf Form4.ComboBox1.Text = "3" Then
                KNN = 60
            Else
                KNN = 100
            End If
        End If
        If ListView1.SelectedItems.Count > 0 And RadioButton2.Checked = True Then
            But_classify.Enabled = False
            'make sure there is a selected item to modify  
            file_name = ListView1.SelectedItems(0).Text
            Dim str As String = ListView1.SelectedItems(0).SubItems(1).Text.ToString
            Dim strParts() As String = str.Split("%")
            prog_percent = Int32.Parse(strParts(0))
            'MsgBox("Path=" + path + "\" + file_name + vbNewLine + "Prog_Percent=" + prog_percent.ToString + "%")

            'Specify file
            Dim myfile As String = path + "\" + file_name
            'Check if file exists
            If System.IO.File.Exists(myfile) = True Then
                'Read the file
                Dim objReader As New System.IO.StreamReader(myfile)
                'Save file contents to textbox
                TextBox1.Text = objReader.ReadToEnd
                textBox_Str = TextBox1.Text.ToString
                textBox_Str = textBox_Str.Trim()
                objReader.Close()
            Else
                MsgBox("File not found!")
            End If
            GroupBox1.Enabled = True
            GroupBox2.Enabled = False
            But_abort.Enabled = False
            But_continue.Enabled = True
            'do bnothing rest of the work shall be controlled by ...continue button
        ElseIf RadioButton1.Checked = True Then
            GroupBox1.Enabled = False
            GroupBox2.Enabled = True
            But_abort.Enabled = True
            But_continue.Enabled = False
            But_classify.Enabled = False
            'This means that the automatic mode is on 
            'Required processes shall go one after the other
            'i.e. 1) Tokenization
            '     2) Stop Words Removal
            '     3) Stemming
            ' Write functions to implement it below
            ProgressBar1.Minimum = 0
            ProgressBar1.Maximum = 100
            ProgressBar1.Value = 0
            Dim underProcessFile As Integer = 0
            Dim di As New IO.DirectoryInfo(path)
            Dim diar1 As IO.FileInfo() = di.GetFiles()
            Dim dra As IO.FileInfo
            Try
                con.ConnectionString = "Data Source=PRAVEER-PC\SQLEXPRESS;Initial Catalog=classifier_db;Integrated Security=True"
                con.Open()
                TextBox1.Text += "Connection to databse was Successfull..." + vbNewLine
                Me.TextBox1.Refresh()
            Catch ex As Exception
                MessageBox.Show("Error while connecting to SQL Server." & ex.Message)
            End Try
            For Each dra In diar1
                If abort = True Then
                    Continue For
                End If
                If dra.Extension = ".txt" Then
                    Dim myfile As String = dra.FullName
                    'Check if file exists
                    If System.IO.File.Exists(myfile) = True Then
                        'Read the file
                        Dim objReader As New System.IO.StreamReader(myfile)
                        'Save file contents to textbox
                        TextBox1.Text = objReader.ReadToEnd
                        textBox_Str = TextBox1.Text.ToString
                        textBox_Str = textBox_Str.Trim()
                        objReader.Close()
                    Else
                        MsgBox("File not found!")
                    End If
                    Tokenize()
                    Me.TextBox1.Refresh()
                    RemoveStopWords()
                    Me.TextBox1.Refresh()
                    Stemming()
                    Me.TextBox1.Refresh()
                    'check How much percentage of the selected words are iterated in the given file
                    TextBox1.Text = "Starting classification process..." + vbNewLine
                    'initiallise Data Connection
                    Dim cmd As New SqlCommand
                    'create a loop with having all the table name in loop
                    '**************************************************
                    'THIS PART IS SPECIFIC TO EACH ALGORITHM TO BE USED
                    '**************************************************
                    'removing duplicates
                    distinct_Str = ""
                    RemoveDuplicates()
                    textBox_Str = textBox_Str.Trim()
                    distinct_Str = distinct_Str.Trim()
                    ListView1.Items(underProcessFile).Selected = True
                    Dim lv As ListViewItem = ListView1.SelectedItems(0)
                    lv.EnsureVisible()
                    Me.ListView1.Refresh()
                    lv.SubItems(1).Text = "100%"
                    'Number of words in the distinct_str
                    Dim words As String() = textBox_Str.Split(New Char() {" "c})
                    Dim distinct_words As String() = distinct_Str.Split(New Char() {" "c})
                    Dim T As Integer = distinct_words.Length
                    cmd.Connection = con
                    cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"
                    Dim myCmd As SqlDataAdapter = New SqlDataAdapter(cmd)
                    Dim myData As New DataSet()
                    myCmd.Fill(myData)
                    Dim nexti As Integer = 0
                    'myData.Tables.Count
                    Dim mytable As String = ""
                    For Each table As DataTable In myData.Tables
                        For Each row As DataRow In table.Rows
                            mytable += row(0).ToString + " "
                            nexti += 1
                        Next
                    Next
                    mytable = mytable.Trim()
                    'mytable = mytable.Substring(0, mytable.Length - 1)
                    'mytable.Trim()
                    Dim tablename As String() = mytable.Split(New Char() {" "c})
                    Dim N(3) As Integer
                    If AlgoSelectForm.RadioButton1.Checked = True Then
                        'Calculate Frequency of each word
                        Dim fi(T) As Integer
                        Dim str As String
                        Dim k As Integer = 0
                        Dim count As Integer = 0
                        TextBox1.Text += "calculating Frequency of each word in the word set ..." + vbNewLine
                        TextBox1.Text += "+----------------------------------------+" + vbNewLine
                        TextBox1.Text += " Word - Frequency" + vbNewLine
                        TextBox1.Text += "----+--------------------------------+----" + vbNewLine
                        For myi As Integer = 0 To T - 1 Step 1
                            count = 0
                            For Each str In words
                                If distinct_words(myi) = str Then
                                    count += 1
                                End If
                            Next
                            fi(myi) = count
                            TextBox1.Text += " " + distinct_words(myi) + " - " + fi(myi).ToString + vbNewLine
                        Next
                        TextBox1.Text += "-------------------------------------" + vbNewLine
                        TextBox1.Text += "Number of matching in the classified files or tables are:" + vbNewLine
                        TextBox1.Text += "-------------------------------------" + vbNewLine
                        k = 0
                        For Each onetable In tablename
                            cmd.Connection = con
                            cmd.CommandText = "SELECT * From " + onetable
                            Dim myDataAdapter As SqlDataAdapter = New SqlDataAdapter(cmd)
                            Dim ds As DataSet = New DataSet()
                            myDataAdapter.Fill(ds, onetable)
                            Dim dt As DataTable = New DataTable()
                            dt = ds.Tables(onetable)
                            Dim row As DataRow
                            count = 0
                            For Each row In dt.Rows
                                For Each str In words
                                    If str = row("wordList").ToString Then
                                        count += 1
                                    End If
                                Next
                            Next
                            N(k) = count
                            TextBox1.Text += dt.TableName.ToString + "    " + N(k).ToString + vbNewLine
                            k += 1
                        Next
                    End If
                    If AlgoSelectForm.RadioButton2.Checked = True Then
                        'Calculate Frequency of each word
                        Dim fi(T) As Integer
                        Dim str As String
                        Dim k As Integer = 0
                        Dim count As Integer = 0
                        TextBox1.Text += "calculating Frequency of each word in the word set ..." + vbNewLine
                        TextBox1.Text += "+----------------------------------------+" + vbNewLine
                        TextBox1.Text += " Word - Frequency" + vbNewLine
                        TextBox1.Text += "----+--------------------------------+----" + vbNewLine
                        For myi As Integer = 0 To T - 1 Step 1
                            count = 0
                            For Each str In words
                                If distinct_words(myi) = str Then
                                    count += 1
                                End If
                            Next
                            fi(myi) = count
                            TextBox1.Text += " " + distinct_words(myi) + " - " + fi(myi).ToString + vbNewLine
                        Next
                        TextBox1.Text += "Calculating Rank of word in the word set ..." + vbNewLine
                        TextBox1.Text += "+----------------------------------------+" + vbNewLine
                        TextBox1.Text += " Word - Frequency" + vbNewLine
                        TextBox1.Text += "----+--------------------------------+----" + vbNewLine
                        Dim rank(T) As Array
                        rank = {fi, distinct_words}
                        Array.Sort(rank(0))
                        For myi As Integer = T - 1 To 0 Step -1
                            'display all values
                            TextBox1.Text += (T - myi).ToString + ")  " + distinct_words(myi) + " - " + fi(myi + 1).ToString + vbNewLine
                        Next
                        TextBox1.Text += "-------------------------------------" + vbNewLine
                        For Each onetable In tablename
                            cmd.Connection = con
                            cmd.CommandText = "SELECT * From " + onetable
                            Dim myDataAdapter As SqlDataAdapter = New SqlDataAdapter(cmd)
                            Dim ds As DataSet = New DataSet()
                            myDataAdapter.Fill(ds, onetable)
                            Dim dt As DataTable = New DataTable()
                            dt = ds.Tables(onetable)
                            Dim row As DataRow
                            count = 0
                            For myi As Integer = T - 1 To (T * KNN / 100) Step -1
                                For Each row In dt.Rows
                                    If distinct_words(myi) = row("wordList").ToString Then
                                        count += myi - (T * KNN / 100)
                                    End If
                                    Application.DoEvents()
                                Next
                                Application.DoEvents()
                            Next
                            N(k) = (count / (distinct_words.Length * distinct_words.Length)) * 100
                            TextBox1.Text += dt.TableName.ToString + "    " + N(k).ToString + "%" + vbNewLine
                            k += 1
                            Application.DoEvents()
                        Next
                    End If
                    If AlgoSelectForm.RadioButton3.Checked = True Then
                        MsgBox("Under Counstruction !!!" + vbNewLine + "...cannot continue")
                    End If
                    TextBox1.Text += "-------------------------------------" + vbNewLine
                    Dim max As Integer = 0
                    Dim ithIsMax As Integer = 0
                    For myi As Integer = 0 To 3
                        If N(myi) > max Then
                            max = N(myi)
                            ithIsMax = myi
                        End If
                    Next
                    'MsgBox("The File is Classifed As : " + tablename(ithIsMax))
                    '"max is N(" + ithIsMax.ToString + ") and its value is : " + max.ToString)
                    lv.SubItems(2).Text = tablename(ithIsMax)
                    '*****************************************************
                    '********************** END **************************
                    '*****************************************************
                    con.Close()
                    ProgressBar1.Value = ((underProcessFile + 1) / totalNumOfFile) * 100
                    Application.DoEvents()
                    underProcessFile += 1
                End If
            Next
            If abort = True Then
                con.Close()
                'MsgBox("proccess aborted !!!")
                abort = False
            Else
                ProgressBar1.Value = 100
                MsgBox("DONE !!! Classifying files.")
                But_abort.Enabled = False
                But_classify.Enabled = True
                GroupBox1.Enabled = True
                ProgressBar1.Value = 0
            End If
        End If
        count = 1
    End Sub

    Private Sub But_continue_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles But_continue.Click
        Dim total_step As Integer = 10
        'Select(count)
        'call fuctions related to all the required processeses one by one
        'i.e. increment count till total_step
        Select Case count
            Case 1
                'Tokenize elements
                Tokenize()
                count += 1
            Case 2
                'Remove Stop Words
                RemoveStopWords()
                count += 1
            Case 3
                'stemming process
                Stemming()
                count += 1
            Case 4
                'check How much percentage of the selected words are iterated in the given file
                TextBox1.Text = "Starting classification process..." + vbNewLine
                'initiallise Data Connection
                Dim cmd As New SqlCommand
                Try
                    con.ConnectionString = "Data Source=PRAVEER-PC\SQLEXPRESS;Initial Catalog=classifier_db;Integrated Security=True"
                    con.Open()
                    TextBox1.Text += "Connection to databse was Successfull..." + vbNewLine
                    Me.TextBox1.Refresh()
                Catch ex As Exception
                    MessageBox.Show("Error while connecting to SQL Server." & ex.Message)
                End Try
                'create a loop with having all the table name in loop
                '**************************************************
                'THIS PART IS SPECIFIC TO EACH ALGORITHM TO BE USED
                '**************************************************
                'removing duplicates
                distinct_Str = ""
                RemoveDuplicates()
                textBox_Str = textBox_Str.Trim()
                distinct_Str = distinct_Str.Trim()
                Dim lv As ListViewItem
                lv = ListView1.SelectedItems(0)
                lv.SubItems(1).Text = "100%"
                'Number of words in the distinct_str
                Dim words As String() = textBox_Str.Split(New Char() {" "c})
                Dim distinct_words As String() = distinct_Str.Split(New Char() {" "c})
                Dim T As Integer = distinct_words.Length
                Dim tablename As String() = {"alt_atheism", "comp_os_ms_windows_misc", "rec_motorcycles"}
                Dim onetable As String
                Dim N(3) As Integer
                If AlgoSelectForm.RadioButton1.Checked = True Then
                    'Calculate Frequency of each word
                    Dim fi(T) As Integer
                    Dim str As String
                    Dim k As Integer = 0
                    Dim count As Integer = 0
                    TextBox1.Text += "calculating Frequency of each word in the word set ..." + vbNewLine
                    TextBox1.Text += "+----------------------------------------+" + vbNewLine
                    TextBox1.Text += " Word - Frequency" + vbNewLine
                    TextBox1.Text += "----+--------------------------------+----" + vbNewLine
                    For myi As Integer = 0 To T - 1 Step 1
                        count = 0
                        For Each str In words
                            If distinct_words(myi) = str Then
                                count += 1
                            End If
                        Next
                        fi(myi) = count
                        TextBox1.Text += " " + distinct_words(myi) + " - " + fi(myi).ToString + vbNewLine
                    Next
                    TextBox1.Text += "-------------------------------------" + vbNewLine
                    TextBox1.Text += "Number of matching in the classified files or tables are:" + vbNewLine
                    TextBox1.Text += "-------------------------------------" + vbNewLine
                    k = 0
                    For Each onetable In tablename
                        cmd.Connection = con
                        cmd.CommandText = "SELECT * From " + onetable
                        Dim myDataAdapter As SqlDataAdapter = New SqlDataAdapter(cmd)
                        Dim ds As DataSet = New DataSet()
                        myDataAdapter.Fill(ds, onetable)
                        Dim dt As DataTable = New DataTable()
                        dt = ds.Tables(onetable)
                        Dim row As DataRow
                        count = 0
                        For Each row In dt.Rows
                            For Each str In words
                                If str = row("wordList").ToString Then
                                    count += 1
                                End If
                            Next
                        Next
                        N(k) = count
                        TextBox1.Text += dt.TableName.ToString + "    " + N(k).ToString + vbNewLine
                        k += 1
                    Next
                End If
                If AlgoSelectForm.RadioButton2.Checked = True Then
                    Me.SuspendLayout()
                    Form4.ShowDialog()
                    Dim KNN As Integer = 0
                    If Form4.ComboBox1.Text = "1" Then
                        KNN = 20
                    ElseIf Form4.ComboBox1.Text = "2" Then
                        KNN = 40
                    ElseIf Form4.ComboBox1.Text = "3" Then
                        KNN = 60
                    Else
                        KNN = 100
                    End If
                    'Calculate Frequency of each word
                    Dim fi(T) As Integer
                    Dim str As String
                    Dim k As Integer = 0
                    Dim count As Integer = 0
                    TextBox1.Text += "calculating Frequency of each word in the word set ..." + vbNewLine
                    TextBox1.Text += "+----------------------------------------+" + vbNewLine
                    TextBox1.Text += " Word - Frequency" + vbNewLine
                    TextBox1.Text += "----+--------------------------------+----" + vbNewLine
                    For myi As Integer = 0 To T - 1 Step 1
                        count = 0
                        For Each str In words
                            If distinct_words(myi) = str Then
                                count += 1
                            End If
                        Next
                        fi(myi) = count
                        TextBox1.Text += " " + distinct_words(myi) + " - " + fi(myi).ToString + vbNewLine
                    Next
                    TextBox1.Text += "Calculating Rank of word in the word set ..." + vbNewLine
                    TextBox1.Text += "+----------------------------------------+" + vbNewLine
                    TextBox1.Text += " Word - Frequency" + vbNewLine
                    TextBox1.Text += "----+--------------------------------+----" + vbNewLine
                    Dim rank(T) As Array
                    rank = {fi, distinct_words}
                    Array.Sort(rank(0))
                    For myi As Integer = T - 1 To 0 Step -1
                        'display all values
                        TextBox1.Text += (T - myi).ToString + ")  " + distinct_words(myi) + " - " + fi(myi + 1).ToString + vbNewLine
                    Next
                    TextBox1.Text += "-------------------------------------" + vbNewLine
                    For Each onetable In tablename
                        cmd.Connection = con
                        cmd.CommandText = "SELECT * From " + onetable
                        Dim myDataAdapter As SqlDataAdapter = New SqlDataAdapter(cmd)
                        Dim ds As DataSet = New DataSet()
                        myDataAdapter.Fill(ds, onetable)
                        Dim dt As DataTable = New DataTable()
                        dt = ds.Tables(onetable)
                        Dim row As DataRow
                        count = 0
                        For myi As Integer = T - 1 To (T * KNN / 100) Step -1
                            For Each row In dt.Rows
                                If distinct_words(myi) = row("wordList").ToString Then
                                    count += myi - (T * KNN / 100)
                                End If
                            Next
                        Next
                        N(k) = (count / (distinct_words.Length * distinct_words.Length)) * 100
                        TextBox1.Text += dt.TableName.ToString + "    " + N(k).ToString + "%" + vbNewLine
                        k += 1
                    Next
                End If
                If AlgoSelectForm.RadioButton3.Checked = True Then
                    MsgBox("Under Counstruction !!!" + vbNewLine + "...cannot continue")
                End If
                TextBox1.Text += "-------------------------------------" + vbNewLine
                Dim max As Integer = 0
                Dim ithIsMax As Integer = 0
                For myi As Integer = 0 To 3
                    If N(myi) > max Then
                        max = N(myi)
                        ithIsMax = myi
                    End If
                Next
                MsgBox("The File is Classifed As : " + tablename(ithIsMax))
                '"max is N(" + ithIsMax.ToString + ") and its value is : " + max.ToString)
                lv.SubItems(2).Text = tablename(ithIsMax)
                '*****************************************************
                '********************** END **************************
                '*****************************************************
                con.Close()
                But_classify.Enabled = True
                But_continue.Enabled = False
        End Select
    End Sub
    ' Automatic Radio Button
    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        If path = "" Then
            GroupBox2.Enabled = False
            But_abort.Enabled = False
            But_continue.Enabled = False
        End If
    End Sub
    ' Manual Radio Button
    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton2.CheckedChanged
        If path = "" Then
            GroupBox2.Enabled = False
            But_abort.Enabled = False
            But_continue.Enabled = False
        End If
    End Sub

    Private Sub But_abort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles But_abort.Click
        GroupBox1.Enabled = True
        'call function to discontinue the running process
        abort = True
        MsgBox("Process Aborted")
        But_abort.Enabled = False
    End Sub
    Private Sub Tokenize()
        Dim illegalChars As Char() = "~!@#$%^&*()_+{}:""<>?|`-=[];',./\1234567890".ToCharArray()
        textBox_Str = LCase(textBox_Str)
        For Each ch As Char In textBox_Str
            If Array.IndexOf(illegalChars, ch) <> -1 Then
                textBox_Str = textBox_Str.Replace(ch, " ")
            End If
        Next
        textBox_Str = Regex.Replace(textBox_Str, "\s+", " ")
        textBox_Str = textBox_Str.Trim()
        'MsgBox(str)
        Dim words As String() = textBox_Str.Split(New Char() {" "c})
        Dim word As String
        TextBox1.Text = "Tokenizied Output :" + vbNewLine + "+----------------------------------+" + vbNewLine
        TextBox1.Text += "Number of Tokens : " + words.Length.ToString + vbNewLine + "----------------------" + vbNewLine
        For Each word In words
            TextBox1.Text += word + vbNewLine
        Next
    End Sub
    Private Sub RemoveStopWords()
        Dim stopwords As String() = {"a", "able", "about", "above", "according", "accordingly", "across", "actually", "after", "afterwards", "again", "against", "ain", "all", "allow", "allows", "almost", "alone", "along", "already", "also", "although", "always", "am", "among", "amongst", "an", "and", "another", "any", "anybody", "anyhow", "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "appreciate", "appropriate", "are", "aren", "around", "as", "aside", "ask", "asking", "associated", "at", "available", "away", "awfully", "b", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "behind", "being", "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "both", "brief", "but", "by", "c", "mon", "came", "can", "cannot", "cant", "cause", "causes", "certain", "certainly", "changes", "clearly", "co", "com", "come", "comes", "concerning", "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could", "couldn", "course", "currently", "d", "definitely", "described", "despite", "did", "didn", "different", "do", "does", "doesn", "doing", "don", "done", "down", "downwards", "during", "e", "each", "edu", "eg", "eight", "either", "else", "ere", "enough", "entirely", "especially", "et", "etc", "even", "ever", "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except", "f", "far", "few", "fifth", "first", "five", "followed", "following", "follows", "for", "former", "formerly", "forth", "four", "from", "further", "furthermore", "g", "get", "gets", "getting", "given", "gives", "go", "goes", "going", "gone", "got", "gotten", "greetings", "h", "had", "hadn", "happens", "hardly", "has", "hasn", "have", "haven", "having", "he", "hello", "help", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "hi", "him", "himself", "his", "hither", "hopefully", "how", "howbeit", "however", "i", "ll", "ve", "ie", "if", "ignored", "immediate", "in", "inasmuch", "inc", "indeed", "indicate", "indicated", "indicates", "inner", "insofar", "instead", "into", "inward", "is", "isn", "it", "its", "itself", "j", "just", "k", "keep", "keeps", "kept", "know", "knows", "known", "l", "last", "lately", "later", "latter", "latterly", "least", "less", "lest", "let", "like", "liked", "likely", "little", "look", "looking", "looks", "ltd", "m", "mainly", "many", "may", "maybe", "me", "mean", "meanwhile", "merely", "might", "moreover", "most", "mostly", "much", "must", "my", "myself", "n", "name", "namely", "nd", "near", "nearly", "necessary", "need", "needs", "neither", "never", "nevertheless", "new", "next", "nine", "no", "nobody", "non", "none", "noone", "nor", "normally", "not", "nothing", "novel", "now", "nowhere", "o", "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "only", "onto", "or", "other", "others", "otherwise", "ought", "our", "ours", "ourselves", "out", "outside", "over", "overall", "own", "p", "particular", "particularly", "per", "perhaps", "placed", "please", "plus", "possible", "presumably", "probably", "provides", "q", "que", "quite", "qv", "r", "rather", "rd", "re", "really", "reasonably", "regarding", "regardless", "regards", "relatively", "respectively", "right", "s", "said", "same", "saw", "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed", "seeming", "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "shall", "she", "should", "shouldn", "since", "six", "so", "some", "somebody", "somehow", "someone", "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "specified", "specify", "specifying", "still", "sub", "such", "sup", "sure", "t", "take", "taken", "tell", "tends", "th", "than", "thank", "thanks", "thanx", "that", "thats", "the", "their", "theirs", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "therefore", "therein", "theres", "thereupon", "these", "they", "re", "ve", "think", "third", "this", "thorough", "thoroughly", "those", "though", "three", "through", "throughout", "thus", "to", "together", "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying", "twice", "two", "u", "un", "under", "unfortunately", "unless", "unlikely", "until", "unto", "up", "upon", "us", "use", "used", "useful", "uses", "using", "usually", "uucp", "v", "value", "various", "very", "via", "viz", "vs", "w", "want", "wants", "was", "wasn", "way", "we", "welcome", "well", "went", "were", "weren", "what", "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever", "whether", "which", "while", "whither", "who", "whoever", "whole", "whom", "whose", "why", "will", "willing", "wish", "with", "within", "without", "won", "wonder", "would", "would", "wouldn", "x", "y", "yes", "yet", "you", "your", "yours", "yourself", "yourselves", "z", "zero"}
        Dim words As String() = textBox_Str.Split(New Char() {" "c})
        textBox_Str = ""
        Dim word As String
        Dim noOfWords As Integer = 0
        TextBox1.Text = "Stop Words Removal : " + vbNewLine + "+----------------------------------+" + vbNewLine
        For Each word In words
            If Array.IndexOf(stopwords, word) = -1 Then
                textBox_Str += word + " "
                noOfWords += 1
            End If
        Next
        textBox_Str = textBox_Str.Trim()
        words = textBox_Str.Split(New Char() {" "c})
        TextBox1.Text += "Number of filtered Words : " + words.Length.ToString + vbNewLine + "----------------------" + vbNewLine
        For Each word In words
            TextBox1.Text += word + vbNewLine
        Next
    End Sub
    Private Sub Stemming()
        Dim words As String() = textBox_Str.Split(New Char() {" "c})
        Dim word As String
        Dim stemword As PorterStemmerAlgorithm.StemmerInterface = New PorterStemmerAlgorithm.PorterStemmer
        textBox_Str = ""
        TextBox1.Text = "Stemmed Content Output :" + vbNewLine + "+----------------------------------+" + vbNewLine
        For Each word In words
            textBox_Str += stemword.stemTerm(word) + " "
        Next
        textBox_Str = textBox_Str.Trim()
        words = textBox_Str.Split(New Char() {" "c})
        TextBox1.Text += "Number of Tokens : " + words.Length.ToString + vbNewLine + "----------------------" + vbNewLine
        For Each word In words
            TextBox1.Text += word + vbNewLine
        Next
    End Sub
    Private Sub RemoveDuplicates()
        Dim strArr As String() = textBox_Str.Split(New Char() {" "c})
        'Dim str As String
        Dim k As Integer = 0
        Dim count As Integer = 0
        TextBox1.Text += "calculating distinct words in the word set ..." + vbNewLine
        For i As Integer = 1 To strArr.Length - 1 Step 1
            count = 0
            For j As Integer = i + 1 To strArr.Length - 1 Step 1
                If strArr(i) = strArr(j) Then
                    count += 1
                End If
            Next
            If count = 0 Then
                distinct_Str += strArr(i) + " "
                k += 1
            End If
        Next
        'TextBox1.Text += "After Removing non Unique Words" + vbNewLine + "+----------------------------------+" + vbNewLine
        distinct_Str = distinct_Str.Trim()
        strArr = distinct_Str.Split(New Char() {" "c})
        TextBox1.Text += "Number Unique of words :" + strArr.Length.ToString + vbNewLine + "----------------------" + vbNewLine
        'For Each str In strArr
        'TextBox1.Text += str + vbNewLine
        'Next
    End Sub

    Private Sub ClassifierForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        AlgoSelectForm.ResumeLayout()
    End Sub

    Private Sub ComboBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.TextChanged
        If ComboBox1.Text <> "" Then
            Try
                Dim chdir As New DirectoryInfo(ComboBox1.Text)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            Dim di As New IO.DirectoryInfo(ComboBox1.Text)
            If di.Exists And (ComboBox1.Text).Length > 3 Then
                path = ComboBox1.Text
                TextBox1.Text = ""
                ListView1.Items.Clear()
                Dim diar1 As IO.FileInfo() = di.GetFiles()
                Dim dra As IO.FileInfo
                Dim lv As ListViewItem

                'list the names of all files in the specified directory
                For Each dra In diar1
                    'ListView1.Items.Add(dra.ToString)
                    If dra.Extension = ".txt" Then
                        lv = ListView1.Items.Add(dra.ToString)
                        'The remaining columns are subitems  
                        lv.SubItems.Add("0%")
                        lv.SubItems.Add("None")
                    End If
                Next
            End If
        End If
    End Sub
End Class

'<Assembly: AssemblyTitle("")>
'<Assembly: AssemblyDescription("Porter stemmer in VB.NET")>
'<Assembly: AssemblyConfiguration("")>
'<Assembly: AssemblyCompany("")>
'<Assembly: AssemblyProduct("")>
'<Assembly: AssemblyCopyright("")>
'<Assembly: AssemblyTrademark("")>
'<Assembly: AssemblyCulture("")>
'<Assembly: AssemblyVersion("1.4")>
'<Assembly: AssemblyKeyFile("keyfile.snk")>
'<Assembly: AssemblyDelaySign(False)>
'<Assembly: AssemblyKeyName("")>
Namespace PorterStemmerAlgorithm


    '

    '           Porter stemmer in VB.NET, translation of the CSharp port (csharp2.txt).

    '           The original paper is in

    '                   Porter, 1980, An algorithm for suffix stripping, Program, Vol. 14,
    '                   no. 3, pp 130-137,

    '           See also http://www.tartarus.org/~martin/PorterStemmer

    '           History:

    '           Release 1

    '           Bug 1 (reported by Gonzalo Parra 16/10/99) fixed as marked below.
    '           The words 'aed', 'eed', 'oed' leave k at 'a' for step 3, and b[k-1]
    '           is then out outside the bounds of b.

    '           Release 2

    '           Similarly,

    '           Bug 2 (reported by Steve Dyrdahl 22/2/00) fixed as marked below.
    '           'ion' by itself leaves j = -1 in the test for 'ion' in step 5, and
    '           b[j] is then outside the bounds of b.

    '           Release 3

    '           Considerably revised 4/9/00 in the light of many helpful suggestions
    '           from Brian Goetz of Quiotix Corporation (brian@quiotix.com).

    '           Release 4

    '           This revision allows the Porter Stemmer Algorithm to be exported via the
    '           .NET Framework. To facilate its use via .NET, the following commands need to be
    '           issued to the operating system to register the component so that it can be
    '           imported into .Net compatible languages, such as Delphi.NET, Visual C#.NET,
    '           Visual C++.NET, etc.

    '           1. Create a stong name:
    '                        sn -k Keyfile.snk
    '           2. Compile the VB.NET class, which creates an assembly PorterStemmerAlgorithm.dll
    '                        vbc /t:library PorterStemmerAlgorithm.vb
    '           3. Register the dll with the Windows Registry
    '                  and so expose the interface to COM Clients via the type library
    '                  ( PorterStemmerAlgorithm.tlb will be created)
    '                        regasm /tlb PorterStemmerAlgorithm.dll
    '           4. Load the component in the Global Assembly Cache
    '                        gacutil -i PorterStemmerAlgorithm.dll

    '           Note: You must have the .Net Studio installed.

    '           Once this process is performed you should be able to import the class
    '           via the appropiate mechanism in the language that you are using.

    '           i.e in Delphi 7 .NET this is simply a matter of selecting:
    '                        Project | Import Type Libary
    '           And then selecting Porter stemmer in VB.NET Version 1.4"!




    '            Stemmer, implementing the Porter Stemming Algorithm
    '
    '            The Stemmer class transforms a word into its root form.  The input
    '            word can be provided a character at time (by calling add()), or at once
    '            by calling one of the various stem(something) methods.
    '

    Public Interface StemmerInterface
        Function stemTerm(ByVal s As String) As String
    End Interface

    <ClassInterface(ClassInterfaceType.None)> Public Class PorterStemmer
        Implements StemmerInterface

        Public b As Char()

        Private i As Integer                  ' offset into b
        Private i_end As Integer                  ' offset to end of stemmed word
        Private j, k As Integer
        Private Shared INC As Integer = 200                  ' unit of size whereby b is increased

        Public Sub New()
            b = New Char(INC) {}
            i = 0
            i_end = 0
        End Sub

        ' Implementation of the .NET interface - added as part of release 4 (Leif)
        Public Function stemTerm(ByVal s As String) As String Implements StemmerInterface.stemTerm
            setTerm(s)
            stem()
            Return getTerm()
        End Function


        '        SetTerm and GetTerm have been simply added to ease the
        '        interface with other lanaguages. They replace the add functions
        '        and toString function. This was done because the original functions stored
        '        all stemmed words (and each time a new woprd was added, the buffer would be
        '        re-copied each time, making it quite slow). Now, The class interface
        '        that is provided simply accepts a term and returns its stem,
        '        instead of storing all stemmed words.
        '        (Leif)



        Private Sub setTerm(ByVal s As String)
            i = s.Length
            Dim new_b As Char() = New Char(i) {}
            Dim c As Integer
            For c = 0 To (i - 1)
                new_b(c) = s.Chars(c)
            Next
            b = new_b
        End Sub

        Private Function getTerm() As String
            Return New String(b, 0, i_end)
        End Function


        ' Old interface to the class - left for posterity. However, it is not
        ' used when accessing the class via .NET (Leif)*/

        '
        ' Add a character to the word being stemmed.  When you are finished
        ' adding characters, you can call stem(void) to stem the word.
        '
        Public Sub add(ByVal ch As Char)
            Dim c As Integer
            If (i = b.Length) Then
                Dim new_b As Char() = New Char(i + INC) {}
                For c = 0 To (i - 1) Step 1
                    new_b(c) = b(c)
                Next
                b = new_b
            End If
            b(i) = ch
            i = i + 1
        End Sub


        '  Adds wLen characters to the word being stemmed contained in a portion
        '  of a char[] array. This is like repeated calls of add(char ch), but
        '  faster.
        Public Sub add(ByVal w As Char(), ByVal wLen As Integer)
            Dim c As Integer
            If i + wLen >= b.Length Then
                Dim new_b As Char() = New Char(i + wLen + INC) {}
                For c = 0 To (i - 1) Step 1
                    new_b(c) = b(c)
                Next
                b = new_b
            End If
            For c = 0 To (wLen - 1) Step 1
                b(i) = w(c)
                i = i + 1
            Next
        End Sub

        '  After a word has been stemmed, it can be retrieved by toString(),
        '  or a reference to the internal buffer can be retrieved by getResultBuffer
        '  and getResultLength (which is generally more efficient.)
        Public Overrides Function ToString() As String
            Return New String(b, 0, i_end)
        End Function


        '  Returns the length of the word resulting from the stemming process.
        Public Function getResultLength() As Integer
            Return i_end
        End Function


        '  Returns a reference to a character buffer containing the results of
        '  the stemming process.  You also need to consult getResultLength()
        '  to determine the length of the result.
        Public Function getResultBuffer() As Char()
            Return b
        End Function


        '  cons(i) is true <=> b[i] is a consonant.
        Public Function cons(ByVal i As Integer) As Boolean
            Select Case b(i)
                Case "a"c                                  ' Cast string to char. Option Strict On.
                Case "e"c
                Case "i"c
                Case "o"c
                Case "u"c
                    Return False
                Case "y"c
                    If i = 0 Then
                        Return True
                    Else
                        Return Not (cons(i - 1))
                    End If
                Case Else
                    Return True
            End Select
            Return False
        End Function


        '  m() measures the number of consonant sequences between 0 and j. if c is
        '  a consonant sequence and v a vowel sequence, and <..> indicates arbitrary
        '  presence,
        '          <c><v>       gives 0
        '          <c>vc<v>     gives 1
        '          <c>vcvc<v>   gives 2
        '          <c>vcvcvc<v> gives 3
        '          ....
        '
        Private Function m() As Integer
            Dim n As Integer = 0
            Dim i As Integer = 0

            While True
                If (i > j) Then Return n
                If (Not cons(i)) Then Exit While
                i = i + 1
            End While
            i = i + 1
            While (True)
                While (True)
                    If (i > j) Then Return n
                    If (cons(i)) Then Exit While
                    i = i + 1
                End While
                i = i + 1
                n = n + 1
                While (True)
                    If (i > j) Then Return n
                    If (Not cons(i)) Then Exit While
                    i = i + 1
                End While
                i = i + 1
            End While
            Return 0
        End Function


        '  vowelinstem() is true <=> 0,...j contains a vowel
        Private Function vowelinstem() As Boolean
            Dim i As Integer
            For i = 0 To j Step 1                         '  i <= j
                If (Not cons(i)) Then Return True
            Next
            Return False
        End Function


        '  doublec(j) is true <=> j,(j-1) contain a double consonant.
        Private Function doublec(ByVal j As Integer) As Boolean
            If (j < 1) Then Return False
            If (b(j) <> b(j - 1)) Then Return False
            Return cons(j)
        End Function


        '  cvc(i) is true <=> i-2,i-1,i has the form consonant - vowel - consonant
        '  and also if the second c is not w,x or y. this is used when trying to
        '  restore an e at the end of a short word. e.g.
        '
        '          cav(e), lov(e), hop(e), crim(e), but
        '          snow, box, tray.
        '
        Private Function cvc(ByVal i As Integer) As Boolean
            If ((i < 2) OrElse (Not cons(i)) OrElse cons(i - 1) OrElse (Not cons(i - 2))) Then
                Return False
            End If
            Dim ch As Char = b(i)
            If (ch = "w"c OrElse ch = "x"c OrElse ch = "y"c) Then Return False
            Return True
        End Function


        Private Function ends(ByVal s As String) As Boolean
            Dim l As Integer = s.Length
            Dim o As Integer = k - l + 1

            If (o < 0) Then Return False

            Dim sc As Char() = s.ToCharArray
            Dim i As Integer

            For i = 0 To (l - 1) Step 1
                If (b(o + i) <> sc(i)) Then Return False
            Next
            j = k - l

            Return True
        End Function


        '  setto(s) sets (j+1),...k to the characters in the string s, readjusting
        '  k.
        Private Sub setto(ByVal s As String)
            Dim l As Integer = s.Length
            Dim o As Integer = j + 1
            Dim i As Integer = 0
            Dim sc As Char() = s.ToCharArray
            For i = 0 To (l - 1) Step 1
                b(o + i) = sc(i)
            Next
            k = j + l
        End Sub


        '  r(s) is used further down.
        Private Sub r(ByVal s As String)
            If (m() > 0) Then setto(s)
        End Sub


        '  step1() gets rid of plurals and -ed or -ing. e.g.
        '           caresses  ->  caress
        '           ponies    ->  poni
        '           ties      ->  ti
        '           caress    ->  caress
        '           cats      ->  cat
        '
        '           feed      ->  feed
        '           agreed    ->  agree
        '           disabled  ->  disable
        '
        '           matting   ->  mat
        '           mating    ->  mate
        '           meeting   ->  meet
        '           milling   ->  mill
        '           messing   ->  mess
        '
        '           meetings  ->  meet
        '
        Private Sub step1()
            If (b(k) = "s"c) Then
                If (ends("sses")) Then
                    k = k - 2
                ElseIf (ends("ies")) Then
                    setto("i")
                ElseIf (b(k - 1) <> "s"c) Then
                    k = k - 1
                End If
            End If
            If (ends("eed")) Then
                If (m() > 0) Then
                    k = k - 1
                End If
            ElseIf ((ends("ed") OrElse ends("ing")) AndAlso vowelinstem()) Then
                k = j
                If (ends("at")) Then
                    setto("ate")
                ElseIf (ends("bl")) Then
                    setto("ble")
                ElseIf (ends("iz")) Then
                    setto("ize")
                ElseIf (doublec(k)) Then
                    k = k - 1
                    Dim ch As Char = b(k)
                    If ((ch = "l"c) OrElse (ch = "s"c) OrElse (ch = "z"c)) Then
                        k = k + 1
                    End If
                ElseIf ((m() = 1) AndAlso cvc(k)) Then
                    setto("e")
                End If
            End If
        End Sub


        '  step2() turns terminal y to i when there is another vowel in the stem.
        Private Sub step2()
            If (ends("y") AndAlso vowelinstem()) Then
                b(k) = "i"c
            End If

        End Sub


        '  step3() maps double suffices to single ones. so -ization ( = -ize plus
        '  -ation) maps to -ize etc. note that the string before the suffix must give
        '  m() > 0.
        Private Sub step3()
            If (k = 0) Then Return

            'For Bug 1
            Select Case (b(k - 1))
                Case "a"c
                    If ends("ational") Then
                        r("ate")
                        Exit Select
                    End If
                    If ends("tional") Then
                        r("tion")
                        Exit Select
                    End If
                    Exit Select

                Case "c"c
                    If ends("enci") Then
                        r("ence")
                        Exit Select
                    End If
                    If ends("anci") Then
                        r("ance")
                        Exit Select
                    End If
                    Exit Select

                Case "e"c
                    If ends("izer") Then
                        r("ize")
                        Exit Select
                    End If
                    Exit Select

                Case "l"c
                    If ends("bli") Then
                        r("ble")
                        Exit Select
                    End If
                    If ends("alli") Then
                        r("al")
                        Exit Select
                    End If
                    If ends("entli") Then
                        r("ent")
                        Exit Select
                    End If
                    If ends("eli") Then
                        r("e")
                        Exit Select
                    End If
                    If ends("ousli") Then
                        r("ous")
                        Exit Select
                    End If
                    Exit Select

                Case "o"c
                    If ends("ization") Then
                        r("ize")
                        Exit Select
                    End If
                    If ends("ation") Then
                        r("ate")
                        Exit Select
                    End If
                    If ends("ator") Then
                        r("ate")
                        Exit Select
                    End If
                    Exit Select

                Case "s"c
                    If ends("alism") Then
                        r("al")
                        Exit Select
                    End If
                    If ends("iveness") Then
                        r("ive")
                        Exit Select
                    End If
                    If ends("fulness") Then
                        r("ful")
                        Exit Select
                    End If
                    If ends("ousness") Then
                        r("ous")
                        Exit Select
                    End If
                    Exit Select

                Case "t"c
                    If ends("aliti") Then
                        r("al")
                        Exit Select
                    End If
                    If ends("iviti") Then
                        r("ive")
                        Exit Select
                    End If
                    If ends("biliti") Then
                        r("ble")
                        Exit Select
                    End If
                    Exit Select

                Case "g"c
                    If ends("logi") Then
                        r("log")
                        Exit Select
                    End If
                    Exit Select

                Case Else
                    Exit Select
            End Select
        End Sub


        '  step4() deals with -ic-, -full, -ness etc. similar strategy to step3.
        Private Sub step4()
            Select Case (b(k))
                Case "e"c
                    If ends("icate") Then
                        r("ic")
                        Exit Select
                    End If
                    If ends("ative") Then
                        r("")
                        Exit Select
                    End If
                    If ends("alize") Then
                        r("al")
                        Exit Select
                    End If
                    Exit Select

                Case "i"c
                    If ends("iciti") Then
                        r("ic")
                        Exit Select
                    End If
                    Exit Select

                Case "l"c
                    If ends("ical") Then
                        r("ic")
                        Exit Select
                    End If
                    If ends("ful") Then
                        r("")
                        Exit Select
                    End If
                    Exit Select

                Case "s"c
                    If ends("ness") Then
                        r("")
                        Exit Select
                    End If
                    Exit Select
            End Select
        End Sub


        '  step5() takes off -ant, -ence etc., in context <c>vcvc<v>.
        Private Sub step5()
            If (k = 0) Then Return

            '  for Bug 1
            Select Case (b(k - 1))
                Case "a"c
                    If ends("al") Then
                        Exit Select
                    End If
                    Return

                Case "c"c
                    If ends("ance") Then
                        Exit Select
                    End If
                    If ends("ence") Then
                        Exit Select
                    End If
                    Return

                Case "e"c
                    If ends("er") Then
                        Exit Select
                    End If
                    Return

                Case "i"c
                    If ends("ic") Then
                        Exit Select
                    End If
                    Return

                Case "l"c
                    If ends("able") Then
                        Exit Select
                    End If
                    If ends("ible") Then
                        Exit Select
                    End If
                    Return

                Case "n"c
                    If ends("ant") Then
                        Exit Select
                    End If
                    If ends("ement") Then
                        Exit Select
                    End If
                    If ends("ment") Then
                        Exit Select
                    End If
                    '  element etc. not stripped before the m
                    If ends("ent") Then
                        Exit Select
                    End If
                    Return

                Case "o"c
                    If ends("ion") AndAlso (j >= 0) AndAlso (b(j) = "s"c OrElse b(j) = "t"c) Then
                        '  j >= 0 fixes Bug 2
                        Exit Select
                    End If
                    If ends("ou") Then
                        Exit Select
                    End If
                    Return
                    'takes care of -ous

                Case "s"c
                    If ends("ism") Then
                        Exit Select
                    End If
                    Return

                Case "t"c
                    If ends("ate") Then
                        Exit Select
                    End If
                    If ends("iti") Then
                        Exit Select
                    End If
                    Return

                Case "u"c
                    If ends("ous") Then
                        Exit Select
                    End If
                    Return

                Case "v"c
                    If ends("ive") Then
                        Exit Select
                    End If
                    Return

                Case "z"c
                    If ends("ize") Then
                        Exit Select
                    End If
                    Return

                Case Else
                    Return
            End Select
            If (m() > 1) Then k = j
        End Sub


        '  step6() removes a final -e if m() > 1.
        Private Sub step6()
            j = k

            If (b(k) = "e"c) Then
                Dim a As Integer = m()
                If (a > 1) OrElse ((a = 1) AndAlso (Not cvc(k - 1))) Then k = k - 1
            End If
            If (b(k) = "l"c) AndAlso doublec(k) AndAlso (m() > 1) Then k = k - 1

        End Sub


        '  Stem the word placed into the Stemmer buffer through calls to add().
        '  Returns true if the stemming process resulted in a word different
        '  from the input.  You can retrieve the result with
        '  getResultLength()/getResultBuffer() or toString().
        '
        Public Sub stem()
            k = i - 1
            If (k > 1) Then
                step1()
                step2()
                step3()
                step4()
                step5()
                step6()
            End If
            i_end = k + 1
            i = 0
        End Sub


    End Class
End Namespace
