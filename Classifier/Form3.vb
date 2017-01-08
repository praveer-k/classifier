Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.Data
Public Class ClassifierTrainningForm
    Dim dir_path As String = "c:\"
    Dim textBox_Str As String = ""
    Dim totalNumOfFile As Integer = 0
    Dim con As New SqlConnection

    Private Sub But_Browse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles But_Browse.Click
        FolderBrowserDialog1.ShowDialog()
        ComboBox1.Text = FolderBrowserDialog1.SelectedPath()
        dir_path = FolderBrowserDialog1.SelectedPath()
        Dim strArr As String() = dir_path.Split("\"c)
        Dim dir As String = strArr(strArr.Length - 1)
        Dim mRootNode As New TreeNode
        mRootNode.Text = dir
        mRootNode.Tag = dir_path
        mRootNode.Nodes.Add("*DUMMY*")
        TreeView1.Nodes.Clear()
        TreeView1.Nodes.Add(mRootNode)


        Label6.Text = "Showing files from the first sub directory of the directory selected"
        Label7.Text = ""
        If dir_path <> "" Then
            ListView1.Items.Clear()
            Dim DirObj As New DirectoryInfo(dir_path)
            Dim Dirs As DirectoryInfo() = DirObj.GetDirectories
            If Dirs.Length > 0 Then
                Dim di As New IO.DirectoryInfo(Dirs(0).FullName)
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
                        totalNumOfFile += 1
                    End If
                Next
            Else
                MsgBox("No folders in the given Directory")
            End If
        End If
    End Sub
    Private Sub UserControl1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' when our component is loaded, we initialize the TreeView by  adding  the root node
        TreeView1.Nodes.Clear()
        Dim mRootNode As New TreeNode
        mRootNode.Text = dir_path
        mRootNode.Tag = dir_path
        mRootNode.Nodes.Add("*DUMMY*")
        TreeView1.Nodes.Add(mRootNode)
        But_abort.Enabled = False
    End Sub
    Private Sub TreeView1_BeforeCollapse(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) Handles TreeView1.BeforeCollapse
        ' clear the node that is being collapsed
        e.Node.Nodes.Clear()
        ' add a dummy TreeNode to the node being collapsed so it is expandable
        e.Node.Nodes.Add("*DUMMY*")
    End Sub
    Private Sub TreeView1_BeforeExpand(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) Handles TreeView1.BeforeExpand
        ' clear the expanding node so we can re-populate it, or else we end up with duplicate nodes
        e.Node.Nodes.Clear()
        ' get the directory representing this node
        Dim mNodeDirectory As IO.DirectoryInfo
        mNodeDirectory = New IO.DirectoryInfo(e.Node.Tag.ToString)
        ' add each subdirectory from the file system to the expanding node as a child node
        Try
            For Each mDirectory As IO.DirectoryInfo In mNodeDirectory.GetDirectories("*.*")
                ' declare a child TreeNode for the next subdirectory
                Dim mDirectoryNode As New TreeNode
                ' store the full path to this directory in the child TreeNode's Tag property
                mDirectoryNode.Tag = mDirectory.FullName
                ' set the child TreeNodes's display text
                mDirectoryNode.Text = mDirectory.Name
                ' add a dummy TreeNode to this child TreeNode to make it expandable
                mDirectoryNode.Nodes.Add("*DUMMY*")
                ' add this child TreeNode to the expanding TreeNode
                e.Node.Nodes.Add(mDirectoryNode)
            Next
        Catch
            MessageBox.Show("Error accessing " & mNodeDirectory.FullName)
        End Try
    End Sub
    Private Sub Label5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label5.Click

    End Sub
    Private Sub ListView1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListView1.SelectedIndexChanged

    End Sub
    Private Sub ComboBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.TextChanged
        If ComboBox1.Text <> "" Then
            Try
                Dim chdir As New DirectoryInfo(ComboBox1.Text)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            Dim checkdir As New DirectoryInfo(ComboBox1.Text)
            If checkdir.Exists And (ComboBox1.Text).Length > 3 Then
                'MsgBox(dir_path)
                dir_path = ComboBox1.Text
                Dim strArr As String() = dir_path.Split("\"c)
                Dim dir As String = strArr(strArr.Length - 1)
                Dim mRootNode As New TreeNode
                mRootNode.Text = dir
                mRootNode.Tag = dir_path
                mRootNode.Nodes.Add("*DUMMY*")
                TreeView1.Nodes.Clear()
                TreeView1.Nodes.Add(mRootNode)
                Label6.Text = "Showing files from the first sub directory of the directory selected"
                Label7.Text = ""
                If dir_path <> "" Then
                    ListView1.Items.Clear()
                    Dim DirObj As New DirectoryInfo(dir_path)
                    Dim Dirs As DirectoryInfo() = DirObj.GetDirectories
                    If Dirs.Length > 0 Then
                        Dim di As New IO.DirectoryInfo(Dirs(0).FullName)
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
                                totalNumOfFile += 1
                            End If
                        Next
                    Else
                        MsgBox("No folders in the given Directory")
                    End If
                End If
            End If
        End If
    End Sub
    Private Sub But_abort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles But_abort.Click
        But_Start.Enabled = True
        But_abort.Enabled = False
        MsgBox("The Process will aborted and connection will be closed abruptly !!!")
        con.Close()
        ProgressBar1.Value = 0
        Label7.Text = ""
        Label6.Text = "Connection was closed ... you may re-enter the path"
    End Sub

    Private Sub But_Start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles But_Start.Click
        Label6.Text = "Starting trainning process..."
        Me.Label6.Refresh()
        'initiallise Data Connection
        Dim cmd As New SqlCommand
        Try
            con.ConnectionString = "Data Source=PRAVEER-PC\SQLEXPRESS;Initial Catalog=classifier_db;Integrated Security=True"
            con.Open()
            Label6.Text = "Connection to databse was Successfull..."
            Me.Label6.Refresh()
            ProgressBar1.Maximum = 0
            ProgressBar1.Maximum = 100
            ProgressBar1.Value = 0
        Catch ex As Exception
            MessageBox.Show("Error while connecting to SQL Server." & ex.Message)
        End Try
        Label7.Text = ""
        If dir_path <> "" Then
            Dim DirObj As New DirectoryInfo(dir_path)
            Dim Dirs As DirectoryInfo() = DirObj.GetDirectories
            Dim DirectoryName As DirectoryInfo
            Dim lv As ListViewItem
            Dim i As Integer = 0
            Dim j As Integer = 0
            But_abort.Enabled = True
            But_Start.Enabled = False
            For Each DirectoryName In Dirs
                Dim di As New IO.DirectoryInfo(DirectoryName.FullName)
                Dim diar1 As IO.FileInfo() = di.GetFiles()
                Dim dra As IO.FileInfo
                Try
                    Dim restrictions(3) As String
                    restrictions(2) = DirectoryName.ToString
                    Dim dbTbl As DataTable = con.GetSchema("Tables", restrictions)
                    'list the names of all files in the specified directory
                    If j > 0 Then
                        For Each dra In diar1
                            'ListView1.Items.Add(dra.ToString)
                            If dra.Extension = ".txt" Then
                                lv = ListView1.Items.Add(dra.ToString)
                                'The remaining columns are subitems  
                                lv.SubItems.Add("0%")
                                totalNumOfFile += 1
                            End If
                        Next
                    End If
                    If dbTbl.Rows.Count = 0 Then
                        'Table does not exist thus create Table
                        'MsgBox("Table does not exists")
                        Label6.Text = "Table with name: " + DirectoryName.ToString + " does not exist !!!"
                        Me.Label6.Refresh()
                        cmd.Connection = con
                        cmd.CommandText = "CREATE TABLE " + DirectoryName.ToString + "(wordList NVarChar(40) NOT NULL)"
                        cmd.ExecuteNonQuery()
                        'MsgBox("Table created")
                        Label6.Text = "Table with name: " + DirectoryName.ToString + " created ."
                        Me.Label6.Refresh()
                    End If
                    'Table exists
                    'MsgBox("Table exists")
                    Label6.Text = "Table with name: " + DirectoryName.ToString + " already exists !!! ... thus continuing process."
                    Me.Label6.Refresh()
                    'Take the values from the list and store
                    Dim underProcesOfFile As Integer = 0
                    For Each dra In diar1
                        If dra.Extension = ".txt" Then
                            'MsgBox("Path : " + dra.FullName.ToString)
                            Label6.Text = "Path of the file : " + dra.FullName.ToString
                            Me.Label6.Refresh()
                            ListView1.Items(underProcesOfFile).Selected = True
                            lv = ListView1.SelectedItems(0)
                            lv.EnsureVisible()
                            Me.ListView1.Refresh()
                            'Specify file
                            Dim myfile As String = dra.FullName.ToString
                            'Check if file exists
                            If System.IO.File.Exists(myfile) = True Then
                                'Read the file
                                Dim objReader As New System.IO.StreamReader(myfile)
                                'Save file contents to textbox
                                Label7.Text = objReader.ReadToEnd
                                textBox_Str = Label7.Text.ToString
                                textBox_Str = textBox_Str.Trim()
                                objReader.Close()
                                Tokenize()
                                Label6.Text = "Tokenization process completed. "
                                Me.Label7.Refresh()
                                Me.Label6.Refresh()
                                RemoveStopWords()
                                Label6.Text = "Stop words removal process completed."
                                Me.Label7.Refresh()
                                Me.Label6.Refresh()
                                Stemming()
                                Label6.Text = "Stemming process completed."
                                Me.Label7.Refresh()
                                Me.Label6.Refresh()
                                RemoveDuplicates()
                                Label6.Text = "Removal of duplicate words completed"
                                Me.Label7.Refresh()
                                Me.Label6.Refresh()
                                'MsgBox("Done Removing duplicate words")
                                'Now save the contents into the database with a check that the value is not in the database
                                'check if the words exists
                                Dim strArr As String() = textBox_Str.Split(New Char() {" "c})
                                Dim str As String
                                'MsgBox("words in file : " + dra.ToString)
                                For Each str In strArr
                                    cmd.Connection = con
                                    cmd.CommandText = "SELECT * FROM " + DirectoryName.ToString + " WHERE wordList='" + str + "'"
                                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                                    If reader.HasRows Then
                                        'The record exists
                                        'MsgBox("word exist in the table of file : " + dra.ToString)
                                        Label6.Text = "Word exist in the table ... ignoring from inserting into the table"
                                        Me.Label6.Refresh()
                                    Else
                                        'the data does not exist.
                                        reader.Close()
                                        'MsgBox("word does not exist in the table")
                                        Label6.Text = "Word does not exist in the table...inserting it in the table"
                                        Me.Label6.Refresh()
                                        If str.Length < 40 Then
                                            cmd.Connection = con
                                            cmd.CommandText = "INSERT INTO " + DirectoryName.ToString + " (wordList) VALUES('" + str + "')"
                                            cmd.ExecuteNonQuery()
                                            'MsgBox("word inserted into the table")
                                            Label6.Text = "word inserted into the table"
                                            Me.Label6.Refresh()
                                        Else
                                            Label6.Text = "word ignored as it is more than 40 characters"
                                            Me.Label6.Refresh()
                                        End If
                                    End If
                                    reader.Close()
                                    Application.DoEvents()
                                Next
                            Else
                                MsgBox("File not found!")
                            End If
                            lv.SubItems(1).Text = "100%"
                            underProcesOfFile += 1
                            ProgressBar1.Value = Int((underProcesOfFile / totalNumOfFile) * 100)
                            'MsgBox("Done one file")
                            Me.Label6.Refresh()
                        End If
                        Application.DoEvents()
                    Next
                    i += 1
                    j += 1
                    'MsgBox("Done")
                    Label6.Text = "All text files in the directory parsed... Moving to the next directory"

                Catch ex As Exception
                    MsgBox(ex.Message)
                    con.Close()
                End Try
                ListView1.Items.Clear()
                Application.DoEvents()
            Next
            If i <= 0 Then
                MsgBox("No Folders in the Directory")
            Else
                Label6.Text = "Congratulations !!! All directories have been parsed...DONE TRAINING PROCESS !!!"
                But_abort.Enabled = False
                But_Start.Enabled = False
                con.Close()
                ProgressBar1.Value = 100
                MsgBox("Training Process Compeleted and connections closed properly.")
            End If
        End If
    End Sub

    Private Sub ProgressBar1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ProgressBar1.Click


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
        'textBox_Str = textBox_Str.Trim()
        'MsgBox(str)
        Dim words As String() = textBox_Str.Split(New Char() {" "c})
        Dim word As String
        Label7.Text = "Tokenizied Output :" + vbNewLine + "+----------------------------------+" + vbNewLine
        Label7.Text += "Number of Tokens : " + words.Length.ToString + vbNewLine + "----------------------" + vbNewLine
        For Each word In words
            Label7.Text += word + vbNewLine
        Next
    End Sub
    Private Sub RemoveStopWords()
        Dim stopwords As String() = {"a", "able", "about", "above", "according", "accordingly", "across", "actually", "after", "afterwards", "again", "against", "ain", "all", "allow", "allows", "almost", "alone", "along", "already", "also", "although", "always", "am", "among", "amongst", "an", "and", "another", "any", "anybody", "anyhow", "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "appreciate", "appropriate", "are", "aren", "around", "as", "aside", "ask", "asking", "associated", "at", "available", "away", "awfully", "b", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "behind", "being", "believe", "below", "beside", "besides", "best", "better", "between", "beyond", "both", "brief", "but", "by", "c", "mon", "came", "can", "cannot", "cant", "cause", "causes", "certain", "certainly", "changes", "clearly", "co", "com", "come", "comes", "concerning", "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could", "couldn", "course", "currently", "d", "definitely", "described", "despite", "did", "didn", "different", "do", "does", "doesn", "doing", "don", "done", "down", "downwards", "during", "e", "each", "edu", "eg", "eight", "either", "else", "ere", "enough", "entirely", "especially", "et", "etc", "even", "ever", "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except", "f", "far", "few", "fifth", "first", "five", "followed", "following", "follows", "for", "former", "formerly", "forth", "four", "from", "further", "furthermore", "g", "get", "gets", "getting", "given", "gives", "go", "goes", "going", "gone", "got", "gotten", "greetings", "h", "had", "hadn", "happens", "hardly", "has", "hasn", "have", "haven", "having", "he", "hello", "help", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "hi", "him", "himself", "his", "hither", "hopefully", "how", "howbeit", "however", "i", "ll", "ve", "ie", "if", "ignored", "immediate", "in", "inasmuch", "inc", "indeed", "indicate", "indicated", "indicates", "inner", "insofar", "instead", "into", "inward", "is", "isn", "it", "its", "itself", "j", "just", "k", "keep", "keeps", "kept", "know", "knows", "known", "l", "last", "lately", "later", "latter", "latterly", "least", "less", "lest", "let", "like", "liked", "likely", "little", "look", "looking", "looks", "ltd", "m", "mainly", "many", "may", "maybe", "me", "mean", "meanwhile", "merely", "might", "moreover", "most", "mostly", "much", "must", "my", "myself", "n", "name", "namely", "nd", "near", "nearly", "necessary", "need", "needs", "neither", "never", "nevertheless", "new", "next", "nine", "no", "nobody", "non", "none", "noone", "nor", "normally", "not", "nothing", "novel", "now", "nowhere", "o", "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "only", "onto", "or", "other", "others", "otherwise", "ought", "our", "ours", "ourselves", "out", "outside", "over", "overall", "own", "p", "particular", "particularly", "per", "perhaps", "placed", "please", "plus", "possible", "presumably", "probably", "provides", "q", "que", "quite", "qv", "r", "rather", "rd", "re", "really", "reasonably", "regarding", "regardless", "regards", "relatively", "respectively", "right", "s", "said", "same", "saw", "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed", "seeming", "seems", "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "shall", "she", "should", "shouldn", "since", "six", "so", "some", "somebody", "somehow", "someone", "something", "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "specified", "specify", "specifying", "still", "sub", "such", "sup", "sure", "t", "take", "taken", "tell", "tends", "th", "than", "thank", "thanks", "thanx", "that", "thats", "the", "their", "theirs", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "therefore", "therein", "theres", "thereupon", "these", "they", "re", "ve", "think", "third", "this", "thorough", "thoroughly", "those", "though", "three", "through", "throughout", "thus", "to", "together", "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying", "twice", "two", "u", "un", "under", "unfortunately", "unless", "unlikely", "until", "unto", "up", "upon", "us", "use", "used", "useful", "uses", "using", "usually", "uucp", "v", "value", "various", "very", "via", "viz", "vs", "w", "want", "wants", "was", "wasn", "way", "we", "welcome", "well", "went", "were", "weren", "what", "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever", "whether", "which", "while", "whither", "who", "whoever", "whole", "whom", "whose", "why", "will", "willing", "wish", "with", "within", "without", "won", "wonder", "would", "would", "wouldn", "x", "y", "yes", "yet", "you", "your", "yours", "yourself", "yourselves", "z", "zero"}
        Dim words As String() = textBox_Str.Split(New Char() {" "c})
        textBox_Str = ""
        Dim word As String
        Dim noOfWords As Integer = 0
        Label7.Text = "Stop Words Removal : " + vbNewLine + "+----------------------------------+" + vbNewLine
        For Each word In words
            If Array.IndexOf(stopwords, word) = -1 Then
                textBox_Str += word + " "
                noOfWords += 1
            End If
        Next
        textBox_Str = textBox_Str.Substring(0, textBox_Str.Length - 1)
        Label7.Text += "Number of filtered Words : " + noOfWords.ToString + vbNewLine + "----------------------" + vbNewLine
        words = textBox_Str.Split(New Char() {" "c})
        For Each word In words
            Label7.Text += word + vbNewLine
        Next
    End Sub
    Private Sub Stemming()
        Dim words As String() = textBox_Str.Split(New Char() {" "c})
        Dim word As String
        Dim stemword As PorterStemmerAlgorithm.StemmerInterface = New PorterStemmerAlgorithm.PorterStemmer
        textBox_Str = ""
        Label7.Text = "Stemmed Content Output :" + vbNewLine + "+----------------------------------+" + vbNewLine
        For Each word In words
            textBox_Str += stemword.stemTerm(word) + " "
        Next
        textBox_Str = textBox_Str.Substring(0, textBox_Str.Length - 1)
        Label7.Text += "Number of Tokens : " + words.Length.ToString + vbNewLine + "----------------------" + vbNewLine
        words = textBox_Str.Split(New Char() {" "c})
        For Each word In words
            Label7.Text += word + vbNewLine
        Next
    End Sub
    Private Sub RemoveDuplicates()
        Dim strArr As String() = textBox_Str.Split(New Char() {" "c})
        Dim str As String
        Dim k As Integer = 0
        Dim count As Integer = 0
        Label7.Text = ""
        textBox_Str = ""
        For i As Integer = 1 To strArr.Length - 1 Step 1
            count = 0
            For j As Integer = i + 1 To strArr.Length - 1 Step 1
                If strArr(i) = strArr(j) Then
                    count += 1
                End If
            Next
            If count = 0 Then
                textBox_Str += strArr(i) + " "
                k += 1
            End If
        Next
        Label7.Text = "After Removing non Unique Words" + vbNewLine
        strArr = textBox_Str.Split(New Char() {" "c})
        Label7.Text = "Number of words :" + strArr.Length.ToString + vbNewLine
        For Each str In strArr
            Label7.Text += str + vbNewLine
        Next
    End Sub
End Class