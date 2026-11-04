Imports System.Globalization
Imports System.Runtime.InteropServices
Imports ComponentSpace.SAML2

Partial Public Class WebAppMain
    Inherits System.Web.UI.Page
    Dim UserID As String
    Dim Password As String
    Dim ErrMsg As String = ""
    Dim UserRights As Integer = 31
    Shadows Culture As CultureInfo

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Don't Change
        If Session("UserID") Is Nothing Then
            Session("ErrMsg") = "No Application Session !"
            Response.Redirect("Errors.aspx", True) : Exit Sub
        Else
            UserID = Session("UserID")
            Password = Session("Password")
        End If

        If ViewState("UserRights") Is Nothing Then
            UserRights = (New localhost.DataAccess(GetAppSetting("CareWebService1"))).GetUserRights(UserID, Password, System.Web.VirtualPathUtility.GetFileName(Request.Path).Replace(".aspx", ""), ErrMsg)
            ViewState("UserRights") = UserRights
        Else
            UserRights = ViewState("UserRights")
        End If
        If UserRights = 0 Then
            Session("ErrMsg") = "No Access Privilege " & " (Please Check Form : " & System.Web.VirtualPathUtility.GetFileName(Request.Path).Replace(".aspx", "") & " )"
            Response.Redirect("Errors.aspx", True) : Exit Sub
        End If
        Master.Status = ""
        Culture = System.Threading.Thread.CurrentThread.CurrentCulture

        ' Compatibility View Setting
        Response.AppendHeader("X-UA-Compatible", "IE=9")

        If Not IsPostBack Then
            Initialize()
        End If
    End Sub
    Private Sub Page_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete

        If Not Request.QueryString("URLForm") Is Nothing Then
            Dim CurrentURL As String
            Dim NewURL As String
            Dim Position As Integer
            Dim URLForm As String

            CurrentURL = HttpContext.Current.Request.Url.AbsoluteUri
            Position = InStr(InStr(CurrentURL, "?") + 1, CurrentURL, "?")

            If Position <> 0 Then
                NewURL = Left(CurrentURL, Position - 1) & "&" & Right(CurrentURL, Len(CurrentURL) - Position)
                Response.Redirect(NewURL, True)
            End If

            URLForm = Request.QueryString("URLForm")
            Server.Transfer(URLForm, True)
        End If

        If toval(GetAppSetting("ForceLogOutTimeOut")) <> 0 Then
            If tostr(Request.Form("__EVENTTARGET")) <> "ctl00$ContentPlaceHolder1$Timer1" AndAlso tostr(Request.Form("__EVENTTARGET")) <> "ctl00$ContentPlaceHolder1$Timer2" AndAlso tostr(Request.Form("__EVENTTARGET")) <> "ctl00$ContentPlaceHolder1$Timer3" Then
                AddUserPreferencesCookies(Me.Page, "CAREAPP_TimeStamp", GetTimeStamp)
                Session("ts") = GetUserPreferencesCookies(Me, "CAREAPP_TimeStamp")
            End If
        End If

        If Not IsPostBack Then

            DisplayDefault()
            Initialize_Task()
            Master.Title = "Care Web Application - Main Menu"

            If Not Session("Errmsg") Is Nothing Then
                Master.Status = Session("Errmsg")
                Session.Remove("Errmsg")
            End If

            Me.TreeView.ExpandAll()
            Me.TreeRpt.ExpandAll()

            Timer1.Interval = toval(IIf(Session.Timeout = 0, 20, Session.Timeout) / 2) * 60000
            Timer1.Enabled = True

            'If toval(GetAppSetting("RefreshTaskList")) <> 0 Then
            '    ' RefreshTaskList in MInutes
            '    Timer2.Enabled = False
            '    Timer2.Interval = 100
            '    Timer2.Enabled = True
            'End If

            If toval(GetAppSetting("ForceLogOutTimeOut")) <> 0 Then
                ' ForceLogOutTimeOut in MInutes
                Timer3.Enabled = False
                Timer3.Interval = toval(GetAppSetting("ForceLogOutTimeOut")) * 60000
                Timer3.Enabled = True
            End If


            InitializeAttributes()
        End If




    End Sub

    Private Sub Initialize()

        Dim SQL As String = ""
        Dim DataSet As DataSet = Nothing
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        ws1.Timeout = System.Threading.Timeout.Infinite

        SQL = "Select S1.Branch, S1.CT, S2.Currency, S2.Company From SysUser S1, SysTab S2 Where S1.ID = '" & UserID & "'"
        If ws1.GetDataSet(UserID, Password, DataSet, SQL, ErrMsg) Then
            If DataSet.Tables(0).Rows.Count <> 0 Then
                Session("Branch") = tostr(DataSet.Tables(0).Rows(0)("Branch"))
                Session("CT") = tostr(DataSet.Tables(0).Rows(0)("CT"))
                Session("BaseCCY") = tostr(DataSet.Tables(0).Rows(0)("Currency"))
                Session("Company") = tostr(DataSet.Tables(0).Rows(0)("Company"))
            Else
                Session("Branch") = ""
                Session("CT") = ""
                Session("BaseCCY") = ""
                Session("Company") = ""
            End If
        End If

    End Sub

    Private Sub Initialize_Task()
        Dim TaskType As String = tostr(GetAppSetting("DefaultTaskType"))
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim DataSet As DataSet = Nothing
        Dim TaskGroup As String = tostr(LstTaskGroup.SelectedValue)
        Dim UserAuth As Boolean = True
        Dim Sql As String = ""
        Dim findex() As String = Nothing
        Dim fbuffer() As String = Nothing
        ws1.Timeout = System.Threading.Timeout.Infinite
        LstTaskType.SelectedValue = TaskType
        ViewState("TaskType") = TaskType
        Sql = "Select top 1 ActionCode from ActionUser where ActionUserAuth_1 = '" & UserID & "' or ActionUserAuth_2 = '" & UserID & "' or ActionUserAuth_3 = '" & UserID & "'  or ActionUserAuth_4 = '" & UserID & "'  or ActionUserAuth_5 = '" & UserID & "'  or ActionUserAuth_6 = '" & UserID & "'  or ActionUserAuth_7 = '" & UserID & "'  or ActionUserAuth_8 = '" & UserID & "'  or ActionUserAuth_9 = '" & UserID & "'  or ActionUserAuth_10 = '" & UserID & "'"
        If ws1.GetRecord(UserID, Password, findex, fbuffer, Sql, ErrMsg) Then
            If tostr(GetFieldValue(fbuffer(GetFieldIndex("ActionCode", findex)))) = "" Then UserAuth = False
        End If
        If TaskGroup = "" Then
            If UserAuth Then
                Sql = "SELECT 'MY TASK' as 'Group' UNION ALL SELECT 'MY SUBORDINATE TASK' as 'Group' UNION ALL SELECT 'GROUP TASK' as 'Group'"
            Else
                Sql = "SELECT 'MY TASK' as 'Group'"
            End If
            If Not ws1.GetDataSet(UserID, Password, DataSet, Sql, ErrMsg) Then GoTo Error_Message
            For i As Integer = 0 To DataSet.Tables(0).Rows.Count - 1
                LstTaskGroup.Items.Add(i)
                LstTaskGroup.Items(i).Text = tostr(DataSet.Tables(0).Rows(i)("Group"))
            Next i
        End If

        Exit Sub
Error_Message:
        Master.Status = ErrMsg
    End Sub

    Private Sub InitializeAttributes()
        Dim SQL As String = ""

        SQL = "Select toc, DESCRIPTION from FL_SR_TOC(\'" & UserID & "\',\'%\',\'%\') T where T.NOINPUTF=0"
        TxtTOC.Attributes.Add("onkeydown", "ListData(event,'TOC List','" & EncryptSQL(SQL) & "','" & TxtTOC.ClientID & "','" & TxtTOCDesc.ClientID & "','TOC','Description','1',false);")
        SQL = "Select toc, DESCRIPTION from FL_SR_TOC(\'" & UserID & "\',\''+getFilteredTextByID('" & TxtTOC.ClientID & "').value+'\',\'%\') T where T.NOINPUTF=0 and T.TOC=\''+getFilteredTextByID('" & TxtTOC.ClientID & "').value+'\'"
        TxtTOC.Attributes.Add("onchange", "GetWSData('','','" & EncryptScriptPath(Me) & "','" & EncryptSQL(SQL) & "','" & TxtTOC.ClientID & "','" & TxtTOCDesc.ClientID & "');")

        SQL = "Select Branch, Name, cast(ShariaF as varchar(1)) as ShariaF from FL_UserBranch (\'" & UserID & "\', \'\')"
        TxtBranchID.Attributes.Add("onkeydown", "ListData(event,'Branch List','" & EncryptSQL(SQL) & "','" & TxtBranchID.ClientID & "','" & TxtBranchName.ClientID & "','Branch','Name','1',false)")
        SQL = "Select Top 1 Branch, Name, cast(ShariaF as varchar(1)) as ShariaF from FL_UserBranch(\'" & UserID & "\',\'\') B Where B.Branch=\''+getFilteredTextByID('" & TxtBranchID.ClientID & "').value+'\'"
        TxtBranchID.Attributes.Add("onchange", "GetWSData('','','" & EncryptScriptPath(Me) & "','" & EncryptSQL(SQL) & "','" & TxtBranchID.ClientID & "','" & TxtBranchName.ClientID & "')")

        SQL = "Select ID, Name from Sysuser where AllowedF = 1"
        TxtSenderID.Attributes.Add("onkeydown", "ListData(event,'ID List','" & EncryptSQL(SQL) & "','" & TxtSenderID.ClientID & "','" & TxtSenderName.ClientID & "','ID','Name','1',false)")
        SQL = "Select Top 1 ID, Name from Sysuser where ID like\'%'+getFilteredTextByID('" & TxtSenderID.ClientID & "').value+'%\ AND AllowedF = 1"
        TxtSenderID.Attributes.Add("onchange", "GetWSData('','','" & EncryptScriptPath(Me) & "','" & EncryptSQL(SQL) & "','" & TxtSenderID.ClientID & "','" & TxtSenderName.ClientID & "')")

        SQL = "Select ID, Name from Profile where RestrictedF = 0"
        TxtSourceID.Attributes.Add("onkeydown", "ListData(event,'ID List','" & EncryptSQL(SQL) & "','" & TxtSourceID.ClientID & "','" & TxtSourceName.ClientID & "','ID','Name','1',false)")
        SQL = "Select Top 1 ID, Name from Profile where ID like\'%'+getFilteredTextByID('" & TxtSourceID.ClientID & "').value+'%\ AND RestrictedF = 0"
        TxtSourceID.Attributes.Add("onchange", "GetWSData('','','" & EncryptScriptPath(Me) & "','" & EncryptSQL(SQL) & "','" & TxtSourceID.ClientID & "','" & TxtSourceName.ClientID & "')")

        TxtSDueDate.Attributes.Add("onchange", "doPostBack('" & TxtSDueDate.ClientID & "')")
        TxtEDueDate.Attributes.Add("onchange", "doPostBack('" & TxtEDueDate.ClientID & "')")
        LstTaskType.Attributes.Add("onchange", "doPostBack('" & LstTaskType.ClientID & "')")
        LstTaskCategory.Attributes.Add("onchange", "doPostBack('" & LstTaskCategory.ClientID & "')")

        TxtSDueDate_CalendarExtender.Format = Culture.DateTimeFormat.ShortDatePattern
        TxtSDueDate.Attributes.Add("onblur", "Date_LostFocus('" & TxtSDueDate.ClientID & "','" & Culture.DateTimeFormat.ShortDatePattern & "')")
        TxtEDueDate_CalendarExtender.Format = Culture.DateTimeFormat.ShortDatePattern
        TxtEDueDate.Attributes.Add("onblur", "Date_LostFocus('" & TxtEDueDate.ClientID & "','" & Culture.DateTimeFormat.ShortDatePattern & "')")
    End Sub

    Private Sub DisplayDefault()
        Dim DefaultTaskSDate As String = tostr(GetAppSetting("DefaultTaskSDate"))

        If DefaultTaskSDate <> "" Then
            TxtSDueDate.Text = todate(DefaultTaskSDate, Culture).ToString("d", Culture)
        Else
            TxtSDueDate.Text = Today.Date.ToString("d", Culture)
        End If
        TxtEDueDate.Text = DateAdd(DateInterval.Day, 7, Today.Date).ToString("d", Culture)

        LblDocno.Text = tostr(GetAppSetting("DefaultLabelDocno")) & ":"

        Load_MenuApp()
        Load_MenuRpt()
        Load_Task()

        LstTaskLapse.Attributes.Add("onchange", "LstTaskLapse_onTextChanged('" & LstTaskLapse.ClientID & "', '" & TxtSDueDate.ClientID & "', '" & TxtEDueDate.ClientID & "');doPostBack('" & TxtSDueDate.ClientID & "')")
    End Sub

    Private Sub Load_MenuApp()
        Dim ws As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim findex() As String = Nothing
        Dim fbuffer() As String = Nothing
        Dim Arrfbuffer() As Object = Nothing
        Dim SQL As String
        Dim i As Integer
        Dim Category As String
        Dim CClass As String
        Dim ProcID As String
        Dim CatNode As System.Web.UI.WebControls.TreeNode = Nothing
        Dim ClaNode As System.Web.UI.WebControls.TreeNode = Nothing
        Dim PNode As System.Web.UI.WebControls.TreeNode = Nothing

        SQL = "Select * from ("
        SQL = SQL & " Select Case When Coalesce(F.FCode,'')='REPORT' Then 'REPORT' Else 'NON-REPORT' End as Code, C2.Category, C2.Description as CatDesc, C1.Class, C1.Description as ClassDesc, P.ProcID, P.Description, P.ExeName, P.RepName"
        SQL = SQL & " from MenuSetD M, SysUser S, SysProcID P Left Join SysForm F on (P.PROCID=F.FName), SysClass C1, SysCategory C2 "
        SQL = SQL & " Where S.ID='" & UserID & "' and P.WebVersionF=1 and M.MenuID=S.MenuID and P.ProcID=M.ProcID and C1.Class=P.Class and C2.Category=C1.Category and P.Status = 1"
        SQL = SQL & " ) A Where Code='NON-REPORT'"
        SQL = SQL & " Order By Code, CATEGORY, CatDesc, Class, ClassDesc, PROCID, DESCRIPTION, EXENAME"

        If ws.GetArrayRecord(UserID, Password, findex, Arrfbuffer, SQL, ErrMsg) Then
            If Arrfbuffer.Length > 0 Then
                Category = ""
                CClass = ""
                ProcID = ""
                Do While i < Arrfbuffer.Length
                    fbuffer = Arrfbuffer(i)
                    If Category <> GetFieldValue(fbuffer(GetFieldIndex("Category", findex))) Then
                        ' Create New Category Node
                        Category = GetFieldValue(fbuffer(GetFieldIndex("Category", findex)))
                        CatNode = New System.Web.UI.WebControls.TreeNode
                        CatNode.Value = ""
                        CatNode.Text = GetFieldValue(fbuffer(GetFieldIndex("CatDesc", findex)))
                        CatNode.SelectAction = TreeNodeSelectAction.Expand
                        TreeView.Nodes.Add(CatNode)
                    End If
                    If CClass <> GetFieldValue(fbuffer(GetFieldIndex("Class", findex))) Then
                        ' Create New Class Node
                        CClass = GetFieldValue(fbuffer(GetFieldIndex("Class", findex)))
                        ClaNode = New System.Web.UI.WebControls.TreeNode
                        ClaNode.Value = ""
                        ClaNode.Text = GetFieldValue(fbuffer(GetFieldIndex("ClassDesc", findex)))
                        ClaNode.SelectAction = TreeNodeSelectAction.Expand
                        CatNode.ChildNodes.Add(ClaNode)
                    End If

                    PNode = New System.Web.UI.WebControls.TreeNode
                    Dim URLString As String
                    Dim QueryString As String

                    URLString = GetFieldValue(fbuffer(GetFieldIndex("ExeName", findex)))
                    QueryString = "?ProcID=" & GetFieldValue(fbuffer(GetFieldIndex("ProcID", findex)))
                    QueryString = QueryString & "&ExeName=" & GetFieldValue(fbuffer(GetFieldIndex("ExeName", findex)))
                    QueryString = QueryString & "&RepName=" & GetFieldValue(fbuffer(GetFieldIndex("RepName", findex)))
                    QueryString = QueryString & "&AuthKey=" & AuthKey(tostr(GetFieldValue(fbuffer(GetFieldIndex("ProcID", findex)))), UserID)
                    'QueryString = QueryString & "&UserID=" & EncodeURL( Encrypt(UserID, GenerateEncryptionKey(Me)))
                    'QueryString = QueryString & "&Password=" & EncodeURL( Encrypt(Password, GenerateEncryptionKey(Me)))
                    PNode.NavigateUrl = URLString + QueryString
                    PNode.Target = URLString + QueryString
                    PNode.Value = URLString + QueryString
                    PNode.Text = GetFieldValue(fbuffer(GetFieldIndex("Description", findex)))
                    PNode.SelectAction = TreeNodeSelectAction.Select
                    ClaNode.ChildNodes.Add(PNode)
                    i = i + 1
                Loop
                TreeView.CollapseAll()
            End If
        End If
    End Sub

    Private Sub Load_MenuRpt()
        Dim ws As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim findex() As String = Nothing
        Dim fbuffer() As String = Nothing
        Dim Arrfbuffer() As Object = Nothing
        Dim SQL As String
        Dim i As Integer
        Dim Category As String
        Dim CClass As String
        Dim ProcID As String
        Dim CatNode As System.Web.UI.WebControls.TreeNode = Nothing
        Dim ClaNode As System.Web.UI.WebControls.TreeNode = Nothing
        Dim PNode As System.Web.UI.WebControls.TreeNode = Nothing

        SQL = "Select * from ("
        SQL = SQL & " Select Case When Coalesce(F.FCode,'')='REPORT' Then 'REPORT' Else 'NON-REPORT' End as Code, C2.Category, C2.Description as CatDesc, C1.Class, C1.Description as ClassDesc, P.ProcID, P.Description, P.ExeName, P.RepName"
        SQL = SQL & " from MenuSetD M, SysUser S, SysProcID P Left Join SysForm F on (P.PROCID=F.FName), SysClass C1, SysCategory C2 "
        SQL = SQL & " Where S.ID='" & UserID & "' and P.WebVersionF=1 and M.MenuID=S.MenuID and P.ProcID=M.ProcID and C1.Class=P.Class and C2.Category=C1.Category and P.Status = 1 and C1.documentF = 0"
        SQL = SQL & " ) A Where Code='REPORT'"
        SQL = SQL & " Order By Code, CATEGORY, CatDesc, Class, ClassDesc, PROCID, DESCRIPTION, EXENAME"

        If ws.GetArrayRecord(UserID, Password, findex, Arrfbuffer, SQL, ErrMsg) Then
            If Arrfbuffer.Length > 0 Then
                Category = ""
                CClass = ""
                ProcID = ""
                Do While i < Arrfbuffer.Length
                    fbuffer = Arrfbuffer(i)
                    If Category <> GetFieldValue(fbuffer(GetFieldIndex("Category", findex))) Then
                        ' Create New Category Node
                        Category = GetFieldValue(fbuffer(GetFieldIndex("Category", findex)))
                        CatNode = New System.Web.UI.WebControls.TreeNode
                        CatNode.Value = ""
                        CatNode.Text = GetFieldValue(fbuffer(GetFieldIndex("CatDesc", findex)))
                        CatNode.SelectAction = TreeNodeSelectAction.Expand
                        TreeRpt.Nodes.Add(CatNode)
                    End If
                    If CClass <> GetFieldValue(fbuffer(GetFieldIndex("Class", findex))) Then
                        ' Create New Class Node
                        CClass = GetFieldValue(fbuffer(GetFieldIndex("Class", findex)))
                        ClaNode = New System.Web.UI.WebControls.TreeNode
                        ClaNode.Value = ""
                        ClaNode.Text = GetFieldValue(fbuffer(GetFieldIndex("ClassDesc", findex)))
                        ClaNode.SelectAction = TreeNodeSelectAction.Expand
                        CatNode.ChildNodes.Add(ClaNode)
                    End If

                    PNode = New System.Web.UI.WebControls.TreeNode
                    Dim URLString As String
                    Dim QueryString As String

                    URLString = GetFieldValue(fbuffer(GetFieldIndex("ExeName", findex)))
                    QueryString = "?ProcID=" & GetFieldValue(fbuffer(GetFieldIndex("ProcID", findex)))
                    QueryString = QueryString & "&ExeName=" & GetFieldValue(fbuffer(GetFieldIndex("ExeName", findex)))
                    QueryString = QueryString & "&RepName=" & GetFieldValue(fbuffer(GetFieldIndex("RepName", findex)))
                    PNode.NavigateUrl = URLString + QueryString
                    PNode.Target = URLString + QueryString
                    PNode.Value = URLString + QueryString
                    PNode.Text = GetFieldValue(fbuffer(GetFieldIndex("Description", findex))) & " (" & GetFieldValue(fbuffer(GetFieldIndex("procid", findex))) & ")"
                    PNode.SelectAction = TreeNodeSelectAction.Select
                    ClaNode.ChildNodes.Add(PNode)
                    i = i + 1
                Loop
                TreeRpt.CollapseAll()
            End If
        End If
    End Sub

    Private Sub Load_Task()
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim DataSet As DataSet = Nothing
        Dim SQL As String = ""
        Dim findex() As String = Nothing
        Dim fbuffer() As String = Nothing
        Dim UserAuth As Boolean = True
        Dim TaskCategory As String = tostr(LstTaskCategory.SelectedValue)
        Dim ADataSet(2) As DataSet
        Dim SQL2(2) As String

        ws1.Timeout = System.Threading.Timeout.Infinite

        SQL = "Select top 1 ActionCode from ActionUser where ActionUserAuth_1 = '" & UserID & "' or ActionUserAuth_2 = '" & UserID & "' or ActionUserAuth_3 = '" & UserID & "'  or ActionUserAuth_4 = '" & UserID & "'  or ActionUserAuth_5 = '" & UserID & "'  or ActionUserAuth_6 = '" & UserID & "'  or ActionUserAuth_7 = '" & UserID & "'  or ActionUserAuth_8 = '" & UserID & "'  or ActionUserAuth_9 = '" & UserID & "'  or ActionUserAuth_10 = '" & UserID & "' or ActionUser in (Select [Group] from SysGroup)"
        If ws1.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
            If tostr(GetFieldValue(fbuffer(GetFieldIndex("ActionCode", findex)))) = "" Then UserAuth = False
        End If
        Dim TaskGroup As String = tostr(LstTaskGroup.SelectedValue)
        If TaskGroup = "" Then
            If UserAuth Then
                SQL = "SELECT 'MY TASK' as 'Group' UNION ALL SELECT 'MY SUBORDINATE TASK' as 'Group' UNION ALL SELECT 'GROUP TASK' as 'Group'"
            Else
                SQL = "SELECT 'MY TASK' as 'Group'"
            End If

            If Not ws1.GetDataSet(UserID, Password, DataSet, SQL, ErrMsg) Then GoTo Error_Message
            FillListBox("1", UserID, Password, LstTaskGroup, SQL, ErrMsg)
        End If

        If Not CBool(toval(GetAppSetting("CareHidePickUpCount"))) Then
            If UCase(TaskGroup) = "MY TASK" Then
                'SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                'SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                'SQL = SQL & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                'SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                'SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

                SQL = "SELECT 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from ("
                SQL = SQL & " Select S1.ID, S1.Remarks, S1.UserDate, S1.UserTime, S1.USERID,S1.Status, S1.Param1, S1.Param2, S1.Param3, S1.Param4, S1.Param5, S1.Param6, S1.ActionCode, S1.DueDate, SU.Name , SU.Branch from SysRequest S1"
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S1.Param5=SU.ID or S1.Param5 = SU.Type)) "
                SQL = SQL & " union "
                SQL = SQL & " Select S1.ID, S1.Remarks, S1.UserDate, S1.UserTime, S1.USERID,S1.Status, S1.Param1, S1.Param2, S1.Param3, S1.Param4, S1.Param5, S1.Param6, S1.ActionCode, S1.DueDate, SU.Name , SU.Branch from SysRequest S1"
                SQL = SQL & " Inner Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S1.Param5 = S5.[Group])"
                SQL = SQL & " Inner Join SysUser SU on (SU.ID=S5.ID) ) S "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            ElseIf TaskGroup = "MY SUBORDINATE TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (ActionUser.ActionUserAuth_1=SU.ID Or ActionUser.ActionUserAuth_2=SU.ID Or ActionUser.ActionUserAuth_3=SU.ID Or ActionUser.ActionUserAuth_4=SU.ID Or ActionUser.ActionUserAuth_5=SU.ID Or ActionUser.ActionUserAuth_6=SU.ID Or ActionUser.ActionUserAuth_7=SU.ID Or ActionUser.ActionUserAuth_8=SU.ID Or ActionUser.ActionUserAuth_9=SU.ID Or ActionUser.ActionUserAuth_10=SU.ID)) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH')  and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
            ElseIf TaskGroup = "GROUP TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH')  and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            End If

            SQL = SQL & " UNION ALL"
            SQL = SQL & " Select 'PENDING' as 'Category', 'PENDING' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
            SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
            SQL = SQL & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
            SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            FillListBox("1", UserID, Password, LstTaskType, SQL, ErrMsg)
        Else
            If TaskGroup = "MY TASK" Then
                'SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                'SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                'SQL = SQL & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                'SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                'SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

                SQL = "SELECT 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from ("
                SQL = SQL & " Select S1.ID, S1.Remarks, S1.UserDate, S1.UserTime, S1.USERID,S1.Status, S1.Param1, S1.Param2, S1.Param3, S1.Param4, S1.Param5, S1.Param6, S1.ActionCode, S1.DueDate, SU.Name , SU.Branch from SysRequest S1"
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S1.Param5=SU.ID or S1.Param5 = SU.Type)) "
                SQL = SQL & " union "
                SQL = SQL & " Select S1.ID, S1.Remarks, S1.UserDate, S1.UserTime, S1.USERID,S1.Status, S1.Param1, S1.Param2, S1.Param3, S1.Param4, S1.Param5, S1.Param6, S1.ActionCode, S1.DueDate, SU.Name , SU.Branch from SysRequest S1"
                SQL = SQL & " Inner Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S1.Param5 = S5.[Group])"
                SQL = SQL & " Inner Join SysUser SU on (SU.ID=S5.ID) ) S "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            ElseIf TaskGroup = "MY SUBORDINATE TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (ActionUser.ActionUserAuth_1=SU.ID Or ActionUser.ActionUserAuth_2=SU.ID Or ActionUser.ActionUserAuth_3=SU.ID Or ActionUser.ActionUserAuth_4=SU.ID Or ActionUser.ActionUserAuth_5=SU.ID Or ActionUser.ActionUserAuth_6=SU.ID Or ActionUser.ActionUserAuth_7=SU.ID Or ActionUser.ActionUserAuth_8=SU.ID Or ActionUser.ActionUserAuth_9=SU.ID Or ActionUser.ActionUserAuth_10=SU.ID)) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH')  and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
            ElseIf TaskGroup = "GROUP TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH')  and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
            End If

            SQL = SQL & " UNION ALL"
            SQL = SQL & " Select 'PENDING' as 'Category', 'PENDING' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
            SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
            SQL = SQL & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
            SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            Dim itemvalue As Object = LstTaskType.Items.FindByValue("PICKUP TASK")
            Dim itemvalue2 As Object = LstTaskType.Items.FindByValue("PENDING")

            If itemvalue Is Nothing AndAlso itemvalue2 Is Nothing Then
                LstTaskType.Items.Add("PICKUP TASK")
                LstTaskType.Items.Add("PENDING")
            End If
        End If

        Dim TaskType As String = tostr(ViewState("TaskType"))
        SQL = ""
        Select Case True
            Case TaskType.ToUpper().Contains("PICKUP")
                If TaskGroup = "MY TASK" Then
                    SQL = "Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count'  from SysRequest S  "
                    SQL = SQL & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                    SQL = SQL & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                ElseIf TaskGroup = "MY SUBORDINATE TASK" Then
                    SQL = "Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count'  from SysRequest S  "
                    SQL = SQL & " Left Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5) "
                    SQL = SQL & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5=SU.Type or ActionUser.ActionUserAuth_1=SU.ID or ActionUser.ActionUserAuth_2=SU.ID or ActionUser.ActionUserAuth_3=SU.ID or ActionUser.ActionUserAuth_4=SU.ID or ActionUser.ActionUserAuth_5=SU.ID or ActionUser.ActionUserAuth_6=SU.ID or ActionUser.ActionUserAuth_7=SU.ID or ActionUser.ActionUserAuth_8=SU.ID or ActionUser.ActionUserAuth_9=SU.ID or ActionUser.ActionUserAuth_10=SU.ID)) "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                ElseIf TaskGroup = "GROUP TASK" Then
                    SQL = "Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count'  from SysRequest S  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                    SQL = SQL & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                    SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                    SQL = SQL & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                    SQL = SQL & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                End If
            Case TaskType.ToUpper().Contains("PENDING")
                SQL = " Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                SQL = SQL & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
                SQL = SQL & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                SQL = SQL & " Group By S.ActionCode, AL.Description "

        End Select


        FillListBox("1", UserID, Password, LstTaskCategory, SQL, ErrMsg)

        ViewState("TaskGroup") = IIf(TaskGroup <> "", TaskGroup, tostr(LstTaskGroup.SelectedValue))

        ViewState("TaskCategory") = IIf(TaskCategory <> "", TaskCategory, tostr(LstTaskCategory.SelectedValue))
        'LstTaskGroup.Items.Clear()
        ViewState("TaskType") = IIf(TaskCategory <> "", TaskCategory, tostr(LstTaskType.SelectedValue))


        Exit Sub
Error_Message:
        Master.Status = ErrMsg
    End Sub

    Private Sub LstTaskGroup_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstTaskGroup.SelectedIndexChanged
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim DataSet As DataSet = Nothing
        Dim SQL As String = ""
        Dim TaskGroup As String = tostr(LstTaskGroup.SelectedValue)
        Dim TaskCategory As String = tostr(LstTaskCategory.SelectedValue)

        If Not CBool(toval(GetAppSetting("CareHidePickUpCount"))) Then
            If TaskGroup = "MY TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                SQL = SQL & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            ElseIf TaskGroup = "MY SUBORDINATE TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Left Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (ActionUser.ActionUserAuth_1=SU.ID Or ActionUser.ActionUserAuth_2=SU.ID Or ActionUser.ActionUserAuth_3=SU.ID Or ActionUser.ActionUserAuth_4=SU.ID Or ActionUser.ActionUserAuth_5=SU.ID Or ActionUser.ActionUserAuth_6=SU.ID Or ActionUser.ActionUserAuth_7=SU.ID Or ActionUser.ActionUserAuth_8=SU.ID Or ActionUser.ActionUserAuth_9=SU.ID Or ActionUser.ActionUserAuth_10=SU.ID)) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "

                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and Coalesce(S.Actioncode,'')='' and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
            ElseIf TaskGroup = "GROUP TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "

                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and Coalesce(S.Actioncode,'')='' and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
            End If

            SQL = SQL & " UNION ALL"
            SQL = SQL & " Select 'PENDING' as 'Category', 'PENDING' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
            SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
            SQL = SQL & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
            SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
            FillListBox("1", UserID, Password, LstTaskType, SQL, ErrMsg)
        Else
            If TaskGroup = "MY TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                SQL = SQL & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            ElseIf TaskGroup = "MY SUBORDINATE TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Left Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (ActionUser.ActionUserAuth_1=SU.ID Or ActionUser.ActionUserAuth_2=SU.ID Or ActionUser.ActionUserAuth_3=SU.ID Or ActionUser.ActionUserAuth_4=SU.ID Or ActionUser.ActionUserAuth_5=SU.ID Or ActionUser.ActionUserAuth_6=SU.ID Or ActionUser.ActionUserAuth_7=SU.ID Or ActionUser.ActionUserAuth_8=SU.ID Or ActionUser.ActionUserAuth_9=SU.ID Or ActionUser.ActionUserAuth_10=SU.ID)) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "

                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and Coalesce(S.Actioncode,'')='' and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
            ElseIf TaskGroup = "GROUP TASK" Then
                SQL = "Select 'PICKUP TASK' as 'Category', 'PICKUP TASK' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                SQL = SQL & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                SQL = SQL & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                SQL = SQL & " Left Join ActionList AL On (AL.ActionCode=S.ActionCode And (AL.ActionNotification=0  Or (AL.ActionNotification>0 And datediff(day, S.UserDate, cast(Convert(varchar(10), getdate(), 1) As smalldatetime))>AL.ActionNotification))) "

                SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and Coalesce(S.Actioncode,'')='' and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            End If

            SQL = SQL & " UNION ALL"
            SQL = SQL & " Select 'PENDING' as 'Category', 'PENDING' + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count', Count(S.ID) as Total from SysRequest S  "
            SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
            SQL = SQL & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
            SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "

            Dim itemvalue As Object = LstTaskType.Items.FindByValue("PICKUP TASK")
            Dim itemvalue2 As Object = LstTaskType.Items.FindByValue("PENDING")

            If itemvalue Is Nothing AndAlso itemvalue2 Is Nothing Then
                LstTaskType.Items.Add("PICKUP TASK")
                LstTaskType.Items.Add("PENDING")
            End If
        End If

        Dim TaskType As String = tostr(LstTaskType.SelectedIndex)

        Select Case TaskType
            Case TaskType.ToUpper().Contains("PICKUP")
                If TaskGroup = "MY TASK" Then
                    SQL = "Select '' as ActionCode, '' 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count' from SysRequest S  "
                    SQL = SQL & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                    SQL = SQL & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                ElseIf TaskGroup = "MY SUBORDINATE TASK" Then
                    SQL = "Select '' as ActionCode, '' 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count' from SysRequest S  "
                    SQL = SQL & " Left Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5) "
                    SQL = SQL & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5=SU.Type or ActionUser.ActionUserAuth_1=SU.ID or ActionUser.ActionUserAuth_2=SU.ID or ActionUser.ActionUserAuth_3=SU.ID or ActionUser.ActionUserAuth_4=SU.ID or ActionUser.ActionUserAuth_5=SU.ID or ActionUser.ActionUserAuth_6=SU.ID or ActionUser.ActionUserAuth_7=SU.ID or ActionUser.ActionUserAuth_8=SU.ID or ActionUser.ActionUserAuth_9=SU.ID or ActionUser.ActionUserAuth_10=SU.ID)) "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                ElseIf TaskGroup = "GROUP TASK" Then
                    SQL = "Select '' as ActionCode, '' 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count' from SysRequest S  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                    SQL = SQL & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                    SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                    SQL = SQL & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                    SQL = SQL & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "

                End If
            Case TaskType.ToUpper().Contains("PENDING")

                SQL = " Select '' as ActionCode, '' 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count' from SysRequest S  "
                SQL = SQL & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
                SQL = SQL & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                SQL = SQL & " Group By S.ActionCode, AL.Description "
        End Select

        If TaskCategory <> "" Then
            FillListBox("1", UserID, Password, LstTaskCategory, SQL, ErrMsg)
        End If

        ViewState("TaskGroup") = IIf(TaskGroup <> "", TaskGroup, tostr(LstTaskGroup.SelectedValue))
        ViewState("TaskCategory") = IIf(TaskCategory <> "", TaskCategory, tostr(LstTaskCategory.SelectedValue))
    End Sub

    Private Sub LstTaskType_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles LstTaskType.SelectedIndexChanged
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim DataSet As DataSet = Nothing
        Dim SQL As String = ""

        Dim TaskGroup As String = tostr(LstTaskGroup.SelectedValue)
        Dim TaskType As String = tostr(LstTaskType.SelectedValue)
        Dim TaskCategory As String = tostr(LstTaskCategory.SelectedValue)

        'If TaskType = "1" Then TaskType = "0"
        Select Case True
            Case TaskType.ToUpper().Contains("PICKUP")
                If TaskGroup = "MY TASK" Then
                    SQL = "Select '' as ActionCode, '' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count'  from SysRequest S  "
                    SQL = SQL & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                    SQL = SQL & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                ElseIf TaskGroup = "MY SUBORDINATE TASK" Then
                    SQL = "Select '' as ActionCode, '' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count'  from SysRequest S  "
                    SQL = SQL & " Left Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5) "
                    SQL = SQL & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5=SU.Type or ActionUser.ActionUserAuth_1=SU.ID or ActionUser.ActionUserAuth_2=SU.ID or ActionUser.ActionUserAuth_3=SU.ID or ActionUser.ActionUserAuth_4=SU.ID or ActionUser.ActionUserAuth_5=SU.ID or ActionUser.ActionUserAuth_6=SU.ID or ActionUser.ActionUserAuth_7=SU.ID or ActionUser.ActionUserAuth_8=SU.ID or ActionUser.ActionUserAuth_9=SU.ID or ActionUser.ActionUserAuth_10=SU.ID)) "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                ElseIf TaskGroup = "GROUP TASK" Then
                    SQL = "Select '' as ActionCode, '' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count'  from SysRequest S  "
                    SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                    SQL = SQL & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                    SQL = SQL & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                    SQL = SQL & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                    SQL = SQL & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    SQL = SQL & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    SQL = SQL & " Group By S.ActionCode, AL.Description "
                End If
            Case TaskType.ToUpper() = "PENDING"
                SQL = " Select '' as ActionCode, '' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' 'Count' from SysRequest S  "
                SQL = SQL & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                SQL = SQL & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
                SQL = SQL & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRF','PRPSL','PO','PRH') and S.Status='' "
                SQL = SQL & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                SQL = SQL & " Group By S.ActionCode, AL.Description "
        End Select


        FillListBox("1", UserID, Password, LstTaskCategory, SQL, ErrMsg)
        ViewState("TaskGroup") = IIf(TaskGroup <> "", TaskGroup, tostr(LstTaskGroup.SelectedValue))
        ViewState("TaskCategory") = IIf(TaskCategory <> "", TaskCategory, tostr(LstTaskCategory.SelectedValue))
    End Sub

    Private Sub BtnFind_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnFind.Click
        Try
            RetrieveTaskList(LstTaskType.SelectedValue.ToUpper, tostr(LstTaskCategory.SelectedValue), tostr(LstTaskGroup.SelectedValue))
            Load_Task()
            UP_DataGrid.Update()
            UP_TaskInquiry.Update()

        Catch ex As Exception
            Master.Status = ex.Message
        Finally
            ExecuteScript(Me, "Updated()")
        End Try
    End Sub

    Private Sub RetrieveTaskList(ByVal TaskType As String, ByVal TaskCategory As String, ByVal TaskGroupIndex As String)
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim DataSet As DataSet = Nothing
        Dim SQL As String = ""
        ws1.Timeout = System.Threading.Timeout.Infinite
        Dim TxtOrderBy As String = tostr(LstOrderBy.SelectedValue)
        Dim ActionCode As String = ""

        Select Case True
            Case TaskType.ToUpper().Contains("PICKUP")
                If tostr(TaskCategory) <> "" Then
                    SQL = "Select * from WF_PICKUPTASK('" & tostr(UserID) & "', '" & tostr(TaskGroupIndex) & "','" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "', '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' ) Where ActionCode='" & TaskCategory & "' AND STATUS='' and (Remarks like '%' + '" & tostr(TxtRemarks.Text) & "' + '%' or DataNo like '%' + '" & tostr(TxtRemarks.Text) & "' + '%')  AND TOC like '%' + '" & tostr(TxtTOC.Text) & "' + '%' AND BRANCH like "
                    SQL = SQL & "'%' + '" & tostr(TxtBranchID.Text) & "' + '%' AND SentBy like '%' + '" & tostr(TxtSenderID.Text) & "' + '%'  AND Datano like '%' + '" & tostr(TxtCINo.Text) & "' + '%'  AND SOURCE LIKE '%' + '" & tostr(TxtSourceID.Text) & "' + '%' "
                    SQL = SQL & "order by UserDate  " & TxtOrderBy & " , UserTime  " & TxtOrderBy & "  "
                Else
                    SQL = "Select * from WF_PICKUPTASK('" & tostr(UserID) & "', '" & tostr(TaskGroupIndex) & "','" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "', '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' ) Where STATUS='' and (Remarks like '%' + '" & tostr(TxtRemarks.Text) & "' + '%' or DataNo like '%' + '" & tostr(TxtRemarks.Text) & "' + '%')  AND TOC like '%' + '" & tostr(TxtTOC.Text) & "' + '%' AND BRANCH like "
                    SQL = SQL & "'%' + '" & tostr(TxtBranchID.Text) & "' + '%' AND SentBy like '%' + '" & tostr(TxtSenderID.Text) & "' + '%'  AND Datano like '%' + '" & tostr(TxtCINo.Text) & "' + '%'  AND SOURCE LIKE '%' + '" & tostr(TxtSourceID.Text) & "' + '%' "
                    SQL = SQL & "order by UserDate  " & TxtOrderBy & " , UserTime  " & TxtOrderBy & "  "
                End If
                ActionCode = tostr(TaskCategory)
            Case TaskType.ToUpper().Contains("PENDING")
                If tostr(TaskCategory) <> "" Then
                    SQL = "Select * from WF_PENDING('" & tostr(UserID) & "', '','" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "', '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' ) Where ActionCode='" & TaskCategory & "' AND STATUS='' and (Remarks like '%' + '" & tostr(TxtRemarks.Text) & "' + '%' or DataNo like '%' + '" & tostr(TxtRemarks.Text) & "' + '%')  AND TOC like '%' + '" & tostr(TxtTOC.Text) & "' + '%' AND BRANCH like "
                    SQL = SQL & "'%' + '" & tostr(TxtBranchID.Text) & "' + '%' AND SentBy like '%' + '" & tostr(TxtSenderID.Text) & "' + '%'  AND Datano like '%' + '" & tostr(TxtCINo.Text) & "' + '%'  AND SOURCE LIKE '%' + '" & tostr(TxtSourceID.Text) & "' + '%' "
                    SQL = SQL & "order by UserDate  " & TxtOrderBy & " , UserTime  " & TxtOrderBy & "  "
                Else
                    SQL = "Select * from WF_PENDING('" & tostr(UserID) & "', '','" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "', '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' ) Where STATUS='' and (Remarks like '%' + '" & tostr(TxtRemarks.Text) & "' + '%' or DataNo like '%' + '" & tostr(TxtRemarks.Text) & "' + '%')  AND TOC like '%' + '" & tostr(TxtTOC.Text) & "' + '%' AND BRANCH like "
                    SQL = SQL & "'%' + '" & tostr(TxtBranchID.Text) & "' + '%' AND SentBy like '%' + '" & tostr(TxtSenderID.Text) & "' + '%'  AND Datano like '%' + '" & tostr(TxtCINo.Text) & "' + '%'  AND SOURCE LIKE '%' + '" & tostr(TxtSourceID.Text) & "' + '%' "
                    SQL = SQL & "order by UserDate  " & TxtOrderBy & " , UserTime  " & TxtOrderBy & "  "
                End If


                ActionCode = TaskCategory
            Case Else
                If tostr(TaskCategory) <> "" Then
                    SQL = "Select * from WF_PICKUPTASK('" & tostr(UserID) & "', '" & tostr(TaskGroupIndex) & "','" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "', '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' ) Where ActionCode='" & TaskCategory & "' AND STATUS='' and (Remarks like '%' + '" & tostr(TxtRemarks.Text) & "' + '%' or DataNo like '%' + '" & tostr(TxtRemarks.Text) & "' + '%')  AND TOC like '%' + '" & tostr(TxtTOC.Text) & "' + '%' AND BRANCH like "
                    SQL = SQL & "'%' + '" & tostr(TxtBranchID.Text) & "' + '%' AND SentBy like '%' + '" & tostr(TxtSenderID.Text) & "' + '%'  AND Datano like '%' + '" & tostr(TxtCINo.Text) & "' + '%'  AND SOURCE LIKE '%' + '" & tostr(TxtSourceID.Text) & "' + '%' "
                    SQL = SQL & "order by UserDate  " & TxtOrderBy & " , UserTime  " & TxtOrderBy & "  "
                Else
                    SQL = "Select * from WF_PICKUPTASK('" & tostr(UserID) & "', '" & tostr(TaskGroupIndex) & "','" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "', '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' ) Where STATUS='' and (Remarks like '%' + '" & tostr(TxtRemarks.Text) & "' + '%' or DataNo like '%' + '" & tostr(TxtRemarks.Text) & "' + '%')  AND TOC like '%' + '" & tostr(TxtTOC.Text) & "' + '%' AND BRANCH like "
                    SQL = SQL & "'%' + '" & tostr(TxtBranchID.Text) & "' + '%' AND SentBy like '%' + '" & tostr(TxtSenderID.Text) & "' + '%'  AND Datano like '%' + '" & tostr(TxtCINo.Text) & "' + '%'  AND SOURCE LIKE '%' + '" & tostr(TxtSourceID.Text) & "' + '%' "
                    SQL = SQL & "order by UserDate  " & TxtOrderBy & " , UserTime  " & TxtOrderBy & "  "
                End If


                ActionCode = TaskCategory
        End Select

        ViewState("TaskType") = TaskType
        ViewState("TaskCategory") = TaskCategory

        ws1.Timeout = System.Threading.Timeout.Infinite
        If (Master.MasterViewState("AsyncStatus") Is Nothing OrElse Master.MasterViewState("AsyncStatus") = "Done") Then
            Master.MasterViewState("AsyncStatus") = "PENDING"
            ws1.BeginGetDataSet(UserID, Password, DataSet, SQL, ErrMsg, New AsyncCallback(AddressOf WSCallback), ws1)
            Master.Status = "Retrieve Data Task on progress, please wait..."
            Timer4.Enabled = True
            BtnFind.Enabled = False
            BtnRefresh.Enabled = False
            DataGrid.Enabled = False
        Else
            Master.Status = "PENDING Asynchronous Process..."
        End If
        If ViewState("TaskCategory") <> "" Then
            LstTaskCategory.SelectedValue = ViewState("TaskCategory")
        End If
        If ViewState("TaskType") <> "" Then
            LstTaskType.SelectedValue = ViewState("TaskType")
        End If
        UP_DataGrid.Update()
        UP_TaskInquiry.Update()
        UpdatePanelTimer.Update()
        Exit Sub

Error_Message:
        Master.Status = ErrMsg
    End Sub


    Private Sub DataGrid_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataGrid.DataBound
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim NumRows As Integer = DataGrid.Rows.Count
        Dim StartRow As Integer = DataGrid.PageSize * DataGrid.PageIndex
        Dim i As Integer
        Dim Dataset As DataSet = Master.MasterViewState("TASK_DATASET")
        Dim LnkBtnReply, LnkBtnPickup, LnkBtnAssign, LnkBtnDone, LnkBtnViewLog, LnkBtnDrop, LnkBtnViewPic As System.Web.UI.WebControls.LinkButton
        Dim LblMessageNo, LblSentRemark, LblSentTitle, LblParam2, LblParam3, LblParam4, LblParam5, LblParam6, LblMark As System.Web.UI.WebControls.Label
        Dim LblDataNoDesc, LblDataNo, LblSeparator1, LblSeparator2, LblBranchDesc, LblPolicy, LblPolicyNo, LblTOC As System.Web.UI.WebControls.Label
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim findex() As String = Nothing
        Dim fbuffer() As String = Nothing
        Dim LoadRowPickupTask As Double = toval(GetAppSetting("LoadRowPickupTask"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing

        Dim TaskCategory As String = tostr(LstTaskType.SelectedValue)

        If LoadRowPickupTask <> 0 Then
            DataGrid.PageSize = LoadRowPickupTask
            UP_DataGrid.Update()
        Else
            DataGrid.PageSize = 5
            UP_DataGrid.Update()
        End If

        For i = 0 To NumRows - 1
            Row = DataGrid.Rows(i)
            LnkBtnReply = CType(Row.FindControl("LnkBtnReply"), System.Web.UI.WebControls.LinkButton)
            LnkBtnPickup = CType(Row.FindControl("LnkBtnPickup"), System.Web.UI.WebControls.LinkButton)
            LnkBtnAssign = CType(Row.FindControl("LnkBtnAssign"), System.Web.UI.WebControls.LinkButton)
            LnkBtnDone = CType(Row.FindControl("LnkBtnDone"), System.Web.UI.WebControls.LinkButton)
            LnkBtnViewLog = CType(Row.FindControl("LnkBtnViewLog"), System.Web.UI.WebControls.LinkButton)
            LnkBtnDrop = CType(Row.FindControl("LnkBtnDrop"), System.Web.UI.WebControls.LinkButton)
            LnkBtnViewPic = CType(Row.FindControl("LnkBtnViewPic"), System.Web.UI.WebControls.LinkButton)

            LblSentTitle = CType(Row.FindControl("LblSentTitle"), System.Web.UI.WebControls.Label)
            LblSentRemark = CType(Row.FindControl("LblSentRemark"), System.Web.UI.WebControls.Label)
            LblMessageNo = CType(Row.FindControl("LblMessageNo"), System.Web.UI.WebControls.Label)
            LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
            LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
            LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
            LblParam5 = CType(Row.FindControl("LblParam5"), System.Web.UI.WebControls.Label)
            LblParam6 = CType(Row.FindControl("LblParam6"), System.Web.UI.WebControls.Label)
            LblMark = CType(Row.FindControl("LblMark"), System.Web.UI.WebControls.Label)
            LblDataNoDesc = CType(Row.FindControl("LblDataNoDesc"), System.Web.UI.WebControls.Label)
            LblDataNo = CType(Row.FindControl("LblDataNo"), System.Web.UI.WebControls.Label)
            LblSeparator1 = CType(Row.FindControl("LblSeparator1"), System.Web.UI.WebControls.Label)
            LblSeparator2 = CType(Row.FindControl("LblSeparator2"), System.Web.UI.WebControls.Label)
            LblBranchDesc = CType(Row.FindControl("LblBranchDesc"), System.Web.UI.WebControls.Label)
            LblPolicyNo = CType(Row.FindControl("LblPolicyNo"), System.Web.UI.WebControls.Label)
            LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)
            LblPolicy = CType(Row.FindControl("LblPolicy"), System.Web.UI.WebControls.Label)
            LblMessageNo.Text = tostr(StartRow + i + 1) + ". " + LblMessageNo.Text

            If CareAsperdagF Then
                If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
                    If DataSetTOC.Tables(0).Rows.Count <> 0 Then
                        LnkBtnReply.Enabled = False
                    End If
                End If
            End If

            Select Case LblParam2.Text
                Case "Policy Processing"
                    LblDataNoDesc.Text = "C/I No."
                Case "Policy Processing MW"
                    LblDataNoDesc.Text = "C/I No."
                Case "Inward Treaty"
                    LblDataNoDesc.Text = "C/I No."
                Case "Claim"
                    LblDataNoDesc.Text = "Claim No."
                Case "Servicing"
                    LblDataNoDesc.Text = "Servicing No."
                Case "Quotation"
                    LblDataNoDesc.Text = "Quotation No."
                Case "Finance"
                    LblDataNoDesc.Text = "Voucher No."
                    LnkBtnViewPic.Enabled = False
                Case "Request"
                    LblDataNoDesc.Text = "Request No."
                    LnkBtnViewPic.Enabled = False
                Case "Policy Health"
                    LblDataNoDesc.Text = "Policy No."
                Case "Claim Health"
                    LblDataNoDesc.Text = "Claim No."
                Case "Profile"
                    LblDataNoDesc.Text = "Profile No."
                Case "Guarantee Letter"
                    LblDataNoDesc.Text = "Guarantee No."
                Case "Quotation Health"
                    LblDataNoDesc.Text = "Quotation No."
                    LnkBtnDrop.Enabled = False
                    Dim DropConfirmButton As AjaxControlToolkit.ConfirmButtonExtender = CType(Row.FindControl("LnkBtnDrop_ConfirmButtonExtender"), AjaxControlToolkit.ConfirmButtonExtender)
                    DropConfirmButton.Enabled = False
                Case "Proposal"
                    LblDataNoDesc.Text = "Proposal No."
                Case "Purchase Order"
                    LblDataNoDesc.Text = "Doc No."
                Case "Realization Proposal"
                    LblDataNoDesc.Text = "Doc No."
                Case Else
                    LblDataNoDesc.Text = ""
                    LblSeparator1.Text = ""
                    LblSeparator2.Text = ""
                    LnkBtnReply.Enabled = False
                    LnkBtnPickup.Enabled = False
                    LnkBtnAssign.Enabled = False
                    LnkBtnViewLog.Enabled = False
                    LnkBtnDrop.Enabled = False
                    LnkBtnViewPic.Enabled = False
            End Select

            Select Case UCase(TaskCategory)
                Case "PICKUP TASK"
                    LblBranchDesc.Text = tostr(Dataset.Tables(0).Rows(StartRow + i)("SentByBranchID")) & " - " & tostr(Dataset.Tables(0).Rows(StartRow + i)("SentByBranchName"))
                    LblSentTitle.Text = "Sent By"
                    LblSentRemark.Text = (UCase(tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTBY")))) & " - " & tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTBYName"))
                    LblMark.Text = "Assigned User"
                    LblParam5.Text = (UCase(tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTTO")))) & " - " & tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTToName"))

                    If Not CareAsperdagF Then
                        LnkBtnReply.Enabled = (LblParam2.Text <> "(None)")
                    End If

                    LnkBtnAssign.Enabled = False
                Case "PENDING"
                    LblBranchDesc.Text = tostr(Dataset.Tables(0).Rows(StartRow + i)("SentToBranchID")) & " - " & tostr(Dataset.Tables(0).Rows(StartRow + i)("SentToBranchName"))
                    LblSentTitle.Text = "Sent To"
                    LblSentRemark.Text = (UCase(tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTTO")))) & " - " & tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTToName"))
                    LblMark.Visible = False
                    LblSeparator2.Visible = False
                    LblParam5.Visible = False

                    LnkBtnReply.Enabled = False
                    LnkBtnPickup.Enabled = False
                    LnkBtnAssign.Enabled = False
                    LnkBtnDone.Enabled = False
                Case "OTHER"
                    LblBranchDesc.Text = tostr(Dataset.Tables(0).Rows(StartRow + i)("SentByBranchID")) & " - " & tostr(Dataset.Tables(0).Rows(StartRow + i)("SentByBranchName"))
                    LblSentTitle.Text = "Sent By"
                    LblSentRemark.Text = (UCase(tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTBY")))) & " - " & tostr(Dataset.Tables(0).Rows(StartRow + i)("SENTBYName"))
                    LblMark.Visible = False
                    LblSeparator2.Visible = False
                    LblParam5.Visible = False

                    LnkBtnAssign.Enabled = False
            End Select

            If Not LnkBtnAssign.Enabled Then
                Dim qparam() As Object = {UserID, tostr(ViewState("TaskCategory"))}
                Dim DataSetA As DataSet = Nothing
                If Not ws1.GetQueryDataSet(UserID, Password, DataSetA, "QWF_ASSIGNUSER", qparam, ErrMsg) Then GoTo Error_Message
                LnkBtnAssign.Enabled = CBool(DataSetA.Tables(0).Rows.Count <> 0)
            End If

            LblSentRemark.Text = LblSentRemark.Text & " (" & tostr(Format(todate(Dataset.Tables(0).Rows(StartRow + i)("UserDate")), "dd mmm yyyy")) & " " & tostr(Dataset.Tables(0).Rows(StartRow + i)("UserTime")) & ") "
            LblParam3.Text = IIf(toval(LblParam3.Text) = 0, "", LblParam3.Text)
            LblParam4.Text = IIf(toval(LblParam4.Text) = 0, "", LblParam4.Text)

            If tostr(Dataset.Tables(0).Rows(StartRow + i)("CertificateNo")) <> "" Then
                LblPolicyNo.Text = tostr(Dataset.Tables(0).Rows(StartRow + i)("PolicyNo")) & "-" & tostr(Dataset.Tables(0).Rows(StartRow + i)("CertificateNo"))
            Else
                LblPolicyNo.Text = tostr(Dataset.Tables(0).Rows(StartRow + i)("PolicyNo"))
            End If
            If UCase(LblParam2.Text) = "POLICY HEALTH" Or UCase(LblParam2.Text) = "CLAIM HEALTH" Or UCase(LblParam2.Text) = "GUARANTEE LETTER" Or UCase(LblParam2.Text) = "QUOTATION HEALTH" Then
                LblPolicy.Visible = False
                LblPolicyNo.Visible = False
            End If
        Next

        Exit Sub
Error_Message:
        Master.Status = ErrMsg
    End Sub

    Private Sub DataGrid_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataGrid.Load
        DataGrid.DataSource = Master.MasterViewState("TASK_DATASET")
        DataGrid.DataBind()
    End Sub

    Private Sub DataGrid_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles DataGrid.PageIndexChanging
        DataGrid.PageIndex = e.NewPageIndex
        Dim DataSet As DataSet
        DataSet = Master.MasterViewState("TASK_DATASET")
        DataGrid.DataSource = DataSet
        DataGrid.DataBind()
    End Sub

    Protected Sub MnuApp_MenuItemClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.MenuEventArgs) Handles MnuApp.MenuItemClick
        Select Case e.Item.Value
            Case "LogOff.aspx"
                'OpenForm(Me, "PolInfFrm.aspx", "ANO", "14522", "UserID", Encrypt(UserID, GenerateEncryptionKey(Me)), "Password", Encrypt(Password, GenerateEncryptionKey(Me)))
                'OpenForm(Me, "QuoQryFrm.aspx", "UserID", Encrypt(UserID, GenerateEncryptionKey(Me)), "Password", Encrypt(Password, GenerateEncryptionKey(Me)))
                'OpenForm(Me, "PolQryFrm.aspx", "UserID", Encrypt(UserID, GenerateEncryptionKey(Me)), "Password", Encrypt(Password, GenerateEncryptionKey(Me)))

                Dim ws As New localhost.DataAccess(GetAppSetting("CareWebService1"))
                If ws.LogOut(UserID, Password, Request.UserHostName, ErrMsg) Then
                    AddUserPreferencesCookies(Me, "CAREAPP_UserID", "")
                    AddUserPreferencesCookies(Me, "CAREAPP_Password", "")
                    Session.Abandon()

                    'FormsAuthentication.SignOut()
                    Response.Cookies.Add(New HttpCookie("ASP.NET_SessionId", ""))
                    Response.Redirect("WebSignOn.aspx")

                End If
            Case "ChangePassword.aspx"
                If Not CBool(toval(GetAppSetting("SSOOktaF"))) Or Not CBool(toval(GetAppSetting("SSOAzureF"))) Then
                    Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
                    Dim findex() As String = Nothing
                    Dim fbuffer() As String = Nothing

                    Dim SQL As String = "Select PWD_History from SysTab"
                    If ws1.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                        Dim Password_History As Integer = toval(GetFieldValue(fbuffer(GetFieldIndex("PWD_History", findex))))
                        If Password_History <> 0 Then
                            SQL = "Select Password_Changed,Password_ResetF from SysUserActivity Where ID = '" & UserID & "'"
                            If ws1.GetRecord(UserID, Password, findex, fbuffer, SQL, ErrMsg) Then
                                If DateDiff(DateInterval.Day, todate(GetFieldValue(fbuffer(GetFieldIndex("Password_Changed", findex)))), Today) = 0 And Not toval(GetFieldValue(fbuffer(GetFieldIndex("Password_ResetF", findex)))) Then
                                    Master.Status = "You have changed your Password Today, Please Change Your Password on the Next Day !"
                                    Exit Sub
                                Else
                                    OpenDialog(MnuApp, "ChangePassword.aspx")
                                End If
                            Else
                                Master.Status = ErrMsg
                                Exit Sub
                            End If
                        Else
                            OpenDialog(MnuApp, "ChangePassword.aspx")
                        End If
                    Else
                        Master.Status = ErrMsg
                        Exit Sub
                    End If
                Else
                    Master.Status = "Change Password Is Not Available For Single Sign On Login !"
                End If
            Case Else
                OpenForm(MnuApp, e.Item.Value)
        End Select
        UP_Menu.Update()
        UP_DataGrid.Update()
    End Sub

    Protected Sub ImgBtnViewMsg_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim ImgBtnViewMsg As System.Web.UI.WebControls.ImageButton
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label


        ImgBtnViewMsg = sender
        Row = CType(sender, System.Web.UI.WebControls.ImageButton).NamingContainer
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                ImgBtnViewMsg.Enabled = False
            End If
        End If
        If ImgBtnViewMsg.Enabled Then OpenForm(Me, "WebMsgDlg.aspx", "ID", Row.Cells(3).Text)

        UP_DataGrid.Update()
        UP_LinkBtn.Update()
    End Sub

    Protected Sub LnkBtnViewLog_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LblParam2, LblParam3, LblParam4, LblParam6 As System.Web.UI.WebControls.Label
        Dim FName As String = ""
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label
        Dim LnkBtnViewLog As System.Web.UI.WebControls.LinkButton

        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer

        LnkBtnViewLog = sender
        LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
        LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
        LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
        LblParam6 = CType(Row.FindControl("LblParam6"), System.Web.UI.WebControls.Label)
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnViewLog.Enabled = False
            End If
        End If

        If LnkBtnViewLog.Enabled Then OpenForm(LnkBtnViewLog, "ActionLogDlg.aspx", "Param2", LblParam2.Text, "Param3", LblParam3.Text, "Param4", LblParam4.Text, "Param6", LblParam6.Text)
        UP_LinkBtn.Update()
    End Sub

    Protected Sub LnkBtnPickUp_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim LnkBtnPickUp As System.Web.UI.WebControls.LinkButton
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LblID, LblParam2, LblParam3, LblParam4 As System.Web.UI.WebControls.Label
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim InParam(3) As Object
        Dim FName As String = ""
        Dim OutParam(1) As Object

        Dim wsc As New localhost1.WSCommon(GetAppSetting("CareWSCommon"))
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim SQL As String = ""
        Dim findex() As String = Nothing
        Dim fbuffer() As String = Nothing
        Dim ANO As Double = -1

        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label

        LnkBtnPickUp = sender
        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer
        LblID = CType(Row.FindControl("LblID"), System.Web.UI.WebControls.Label)
        LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
        LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
        LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnPickUp.Enabled = False
            End If
        End If


        InParam(0) = tostr(LblID.Text)
        Select Case UCase(tostr(LblParam2.Text))
            Case "SERVICING"
                InParam(1) = "SVC"
            Case "POLICY PROCESSING"
                InParam(1) = "UND"
            Case "POLICY PROCESSING MW"
                InParam(1) = "UND-MW"
            Case "INWARD TREATY"
                InParam(1) = "IT"
            Case "CLAIM"
                InParam(1) = "CLM"
            Case "QUOTATION"
                InParam(1) = "MAR"
            Case "FINANCE"
                InParam(1) = "FA"
            Case "REQUEST"
                InParam(1) = "RQ"
            Case "POLICY HEALTH"
                InParam(1) = "POLH"
            Case "CLAIM HEALTH"
                InParam(1) = "CLMH"
            Case "PROFILE"
                InParam(1) = "PRF"
            Case "GUARANTEE LETTER"
                InParam(1) = "GLH"
            Case "QUOTATION HEALTH"
                InParam(1) = "QUOT"
            Case "PROPOSAL"
                InParam(1) = "PRPSL"
            Case "PURCHASE ORDER"
                InParam(1) = "PO"
            Case "REALIZATION PROPOSAL"
                InParam(1) = "PRH"
            Case Else
                InParam(1) = ""
        End Select

        InParam(2) = toval(LblParam3.Text)
        InParam(3) = toval(LblParam4.Text)

        If wsc.GetPickUpListName(UserID, Password, InParam, OutParam, ErrMsg) Then
            FName = OutParam(0)
            If FName <> "" Then
                'If OutParam(1) = 1 Then0rd
                '    OpenForm(Me, FName, "SNO", tostr(InParam(2)), "Mode", "ADD")
                'Else
                If LnkBtnPickUp.Enabled Then OpenForm(LnkBtnPickUp, FName, "Mode", "PickUp", "Param", tostr(InParam(0)), "AuthKey", AuthKey(tostr(InParam(0)), UserID))
                'End If
            End If
        Else
            Master.Status = ErrMsg
            Exit Sub
        End If
        UP_LinkBtn.Update()
    End Sub

    Protected Sub LnkBtnReply_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LnkBtnReply As System.Web.UI.WebControls.LinkButton
        Dim LblID, LblParam2, LblParam3, LblParam4 As System.Web.UI.WebControls.Label
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel

        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label

        LnkBtnReply = sender
        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer
        LblID = CType(Row.FindControl("LblID"), System.Web.UI.WebControls.Label)
        LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
        LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
        LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnReply.Enabled = False
            End If
        End If

        If LnkBtnReply.Enabled Then OpenForm(LnkBtnReply, "WebMsgReplyDlg.aspx", "ID", LblID.Text, "Param2", tostr(LblParam2.Text), "Param3", LblParam3.Text, "Param4", LblParam4.Text)
        UP_DataGrid.Update()
        UP_LinkBtn.Update()
    End Sub

    Protected Sub LnkBtnAssign_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LnkBtnAssign As System.Web.UI.WebControls.LinkButton
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim LblID, LblParam2, LblParam3, LblParam4 As System.Web.UI.WebControls.Label

        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label

        LnkBtnAssign = sender
        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer
        LblID = CType(Row.FindControl("LblID"), System.Web.UI.WebControls.Label)
        LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
        LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
        LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnAssign.Enabled = False
            End If
        End If

        If LnkBtnAssign.Enabled Then OpenDialog(LnkBtnAssign, "SRFlowAssDlg.aspx", "ID", LblID.Text, "Param2", LblParam2.Text, "Param3", LblParam3.Text, "Param4", LblParam4.Text)
        UP_DataGrid.Update()
        UP_LinkBtn.Update()

    End Sub

    Protected Sub LnkBtnDone_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LnkBtnDone As System.Web.UI.WebControls.LinkButton
        Dim LblID, LblParam2, LblParam3, LblParam4, LblParam6 As System.Web.UI.WebControls.Label
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label
        Dim TxtDocType, TxtDocNo As System.Web.UI.WebControls.TextBox

        LnkBtnDone = sender
        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer
        LblID = CType(Row.FindControl("LblID"), System.Web.UI.WebControls.Label)
        LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
        LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
        LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
        LblParam6 = CType(Row.FindControl("LblParam6"), System.Web.UI.WebControls.Label)
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)
        TxtDocType = CType(Row.FindControl("TxtDocType"), System.Web.UI.WebControls.TextBox)
        TxtDocNo = CType(Row.FindControl("TxtDocNo"), System.Web.UI.WebControls.TextBox)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnDone.Enabled = False
            End If
        End If

        If LnkBtnDone.Enabled Then OpenDialog2(BtnFind, "SRFlowDoneDlg.aspx", "ID", LblID.Text, "Param2", LblParam2.Text, "Param3", LblParam3.Text, "Param4", LblParam4.Text, "Param6", LblParam6.Text, "DocType", TxtDocType.Text, "DocNo", TxtDocNo.Text)
        UP_DataGrid.Update()
        UP_LinkBtn.Update()
    End Sub

    Protected Sub LnkBtnViewReport_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim LnkBtnViewReport As System.Web.UI.WebControls.LinkButton
        LnkBtnViewReport = sender
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label

        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer

        Dim fname As String = "DocViewer.aspx"
        Dim LblProcID, LblImageID, LblFileType, LblStatus As System.Web.UI.WebControls.Label
        LblProcID = CType(Row.FindControl("LblProcID"), System.Web.UI.WebControls.Label)
        LblImageID = CType(Row.FindControl("LblImageID"), System.Web.UI.WebControls.Label)
        LblFileType = CType(Row.FindControl("LblFileType"), System.Web.UI.WebControls.Label)
        LblStatus = CType(Row.FindControl("LblStatus"), System.Web.UI.WebControls.Label)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnViewReport.Enabled = False
            End If
        End If

        Dim ProcID, ImageID, FileType, Status As String
        ProcID = LblProcID.Text
        ImageID = LblImageID.Text
        FileType = LblFileType.Text
        Status = LblStatus.Text

        Dim wsc As New localhost1.WSCommon(GetAppSetting("CareWSCommon"))
        wsc.Timeout = System.Threading.Timeout.Infinite

        If LnkBtnViewReport.Enabled Then
            If Status = "N" Or Status = "New" Then
                Dim InParam() As Object = {SetFieldValue(ProcID), SetFieldValue(toval(ImageID))}
                If Not wsc.UpdateReportLog(UserID, Password, InParam, ErrMsg) Then
                    Master.Status = ErrMsg
                    Exit Sub
                End If
            End If
        End If

    End Sub

    Protected Sub LnkBtnDrop_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim LnkBtnDrop As System.Web.UI.WebControls.LinkButton
        LnkBtnDrop = sender
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LblID As System.Web.UI.WebControls.Label
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label

        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer
        LblID = CType(Row.FindControl("LblID"), System.Web.UI.WebControls.Label)
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)

        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnDrop.Enabled = False
            End If
        End If

        If LnkBtnDrop.Enabled Then
            Dim wsc As New localhost1.WSCommon(GetAppSetting("CareWSCommon"))
            Dim InParam() As Object = {LblID.Text}
            Dim OutParam() As Object = Nothing
            If wsc.DropTask(UserID, Password, InParam, OutParam, ErrMsg) Then
                RetrieveTaskList(LstTaskType.SelectedValue.ToUpper, LstTaskCategory.SelectedValue, tostr(LstTaskGroup.SelectedIndex + 1))
                Master.Status = "Data deleted successfully"
            Else
                Master.Status = ErrMsg
                Exit Sub
            End If
        End If

        UP_LinkBtn.Update()
    End Sub

    Protected Sub LnkBtnViewPic_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim LnkBtnViewPic As System.Web.UI.WebControls.LinkButton
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LblID As System.Web.UI.WebControls.Label
        Dim LblParam2 As System.Web.UI.WebControls.Label
        Dim LblParam3 As System.Web.UI.WebControls.Label
        Dim LblParam4 As System.Web.UI.WebControls.Label
        Dim LblParam5 As System.Web.UI.WebControls.Label
        Dim LblParam6 As System.Web.UI.WebControls.Label
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label
        Dim TxtDocType, TxtDocNo As System.Web.UI.WebControls.TextBox

        LnkBtnViewPic = sender
        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer

        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblID = CType(Row.FindControl("LblID"), System.Web.UI.WebControls.Label)
        LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
        LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
        LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
        LblParam5 = CType(Row.FindControl("LblParam5"), System.Web.UI.WebControls.Label)
        LblParam6 = CType(Row.FindControl("LblParam6"), System.Web.UI.WebControls.Label)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)
        TxtDocType = CType(Row.FindControl("TxtDocType"), System.Web.UI.WebControls.TextBox)
        TxtDocNo = CType(Row.FindControl("TxtDocNo"), System.Web.UI.WebControls.TextBox)


        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnViewPic.Enabled = False
            End If
        End If

        If LnkBtnViewPic.Enabled Then
            OpenDialog(Me, "ListImageFrm.aspx", "ID", tostr(LblID.Text), "Param2", tostr(LblParam2.Text), "Param3", tostr(LblParam3.Text), "Param4", tostr(LblParam4.Text), "Param5", tostr(LblParam5.Text), "Param6", LblParam6.Text, "DocType", TxtDocType.Text, "DocNo", TxtDocNo.Text)
        End If
        UP_LinkBtn.Update()
    End Sub

    Protected Sub LnkBtnViewDoc_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim LnkBtnViewDoc_ As System.Web.UI.WebControls.LinkButton
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim LblID As System.Web.UI.WebControls.Label
        Dim LblParam2 As System.Web.UI.WebControls.Label
        Dim LblParam3 As System.Web.UI.WebControls.Label
        Dim LblParam4 As System.Web.UI.WebControls.Label
        Dim LblParam5 As System.Web.UI.WebControls.Label
        Dim LblParam6 As System.Web.UI.WebControls.Label
        Dim UP_LinkBtn As System.Web.UI.UpdatePanel
        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim CareAsperdagF As Boolean = CBool(toval(GetAppSetting("CareAsperdagF")))
        Dim PolicyType As String = tostr(GetAppSetting("PolicyType"))
        Dim DataSetTOC As DataSet = Nothing
        Dim LblTOC As System.Web.UI.WebControls.Label
        Dim TxtDocType, TxtDocNo As System.Web.UI.WebControls.TextBox

        LnkBtnViewDoc_ = sender
        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer
        LblID = CType(Row.FindControl("LblID"), System.Web.UI.WebControls.Label)
        LblParam2 = CType(Row.FindControl("LblParam2"), System.Web.UI.WebControls.Label)
        LblParam3 = CType(Row.FindControl("LblParam3"), System.Web.UI.WebControls.Label)
        LblParam4 = CType(Row.FindControl("LblParam4"), System.Web.UI.WebControls.Label)
        LblParam5 = CType(Row.FindControl("LblParam5"), System.Web.UI.WebControls.Label)
        LblParam6 = CType(Row.FindControl("LblParam6"), System.Web.UI.WebControls.Label)
        UP_LinkBtn = CType(Row.FindControl("UP_LinkBtn"), System.Web.UI.UpdatePanel)
        LblTOC = CType(Row.FindControl("LblTOC"), System.Web.UI.WebControls.Label)
        TxtDocType = CType(Row.FindControl("TxtDocType"), System.Web.UI.WebControls.TextBox)
        TxtDocNo = CType(Row.FindControl("TxtDocNo"), System.Web.UI.WebControls.TextBox)

        If ws1.GetDataSet(UserID, Password, DataSetTOC, "select TOC from PTClass where policytype= '" & tostr(PolicyType) & "' and TOC= '" & tostr(LblTOC.Text) & "'", ErrMsg) Then
            If CareAsperdagF And DataSetTOC.Tables(0).Rows.Count <> 0 Then
                LnkBtnViewDoc_.Enabled = False
            End If
        End If
        If LnkBtnViewDoc_.Enabled Then
            OpenDialog(Me, "ListDocFrm.aspx", "ID", tostr(LblID.Text), "Param2", tostr(LblParam2.Text), "Param3", tostr(LblParam3.Text), "Param4", tostr(LblParam4.Text), "Param5", tostr(LblParam5.Text), "Param6", LblParam6.Text, "DocType", TxtDocType.Text, "DocNo", TxtDocNo.Text)
        End If
        UP_LinkBtn.Update()
    End Sub
    Private Sub Timer4_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer4.Tick

        If Not Master.MasterViewState("AsyncStatus") Is Nothing Then
            If Master.MasterViewState("AsyncStatus") = "Done" Then
                Timer4.Enabled = False


                Master.MasterViewState("AsyncStatus") = Nothing
                If Master.MasterViewState("AsyncResult") = "" Then    ' No Error
                    Dim DataSet As DataSet = Nothing
                    DataSet = Master.MasterViewState("AsyncDataSet")

                    If Not DataSet Is Nothing Then
                        If DataSet.Tables(0).Rows.Count > 0 Then
                            Master.Status = DataSet.Tables(0).Rows.Count & " Task(s) "

                            If DataSet.Tables(0).Rows.Count <> 0 Then
                                Master.MasterViewState("TASK_DATASET") = DataSet
                            Else
                                Master.MasterViewState("TASK_DATASET") = Nothing
                                Master.Status = "No Data Available. Please Check Period Task Due !"
                            End If

                        Else
                            Master.MasterViewState("TASK_DATASET") = Nothing
                            Master.Status = "No Data Available !"
                        End If

                        If tostr(ViewState("TaskCategory")) <> "" Then
                            LstTaskCategory.SelectedValue = tostr(Session("TaskCategory"))
                        End If
                        If tostr(ViewState("TaskGroup")) <> "" Then
                            LstTaskGroup.SelectedValue = tostr(ViewState("TaskGroup"))
                        End If
                        'If ViewState("TaskType") <> "" Then
                        '	LstTaskType.SelectedValue = tostr(ViewState("TaskGroup"))
                        'End If
                        Session("TaskCategory") = Nothing
                        DataGrid.DataSource = DataSet
                        DataGrid.DataBind()

                        BtnFind.Enabled = True
                        BtnRefresh.Enabled = True
                        DataGrid.Enabled = True
                        UP_DataGrid.Update()
                        UP_TaskInquiry.Update()
                    End If
                Else
                    ErrMsg = Master.MasterViewState("AsyncResult")
                    Master.Status = ErrMsg

                    BtnFind.Enabled = True
                    BtnRefresh.Enabled = True
                    DataGrid.Enabled = True
                    UP_DataGrid.Update()
                    UP_TaskInquiry.Update()
                End If

            Else    'PENDING
                BtnFind.Enabled = False
                BtnRefresh.Enabled = False
                DataGrid.Enabled = False

                Master.Status = "Retrieve Data Task on progress, please wait..."
                UP_DataGrid.Update()
                UP_TaskInquiry.Update()
            End If
        End If
    End Sub


    Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Master.MasterViewState("UserID") = UCase(Session("UserID"))
    End Sub

    Private Sub Timer2_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer2.Tick

    End Sub

    Private Sub Timer3_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer3.Tick
        Dim ws As New localhost.DataAccess(GetAppSetting("CareWebService1"))

        Timer3.Enabled = False
        If toval(GetAppSetting("ForceLogOutTimeOut")) <> 0 Then
            Dim LogOffF As Boolean = False
            Dim MinLapse As Integer = 0
            Dim ts As String = GetUserPreferencesCookies(Me, "CAREAPP_TimeStamp")

            If tostr(ts) <> "" Then
                Dim dt As Date = TimeStampToDate(Session("ts"))
                ' MinLapse = DateDiff(DateInterval.Minute, dt, Date.Now)
                MinLapse = toval(GetAppSetting("ForceLogOutTimeOut")) + 1
                If MinLapse > 0 Then
                    If MinLapse > toval(GetAppSetting("ForceLogOutTimeOut")) Or MinLapse = toval(GetAppSetting("ForceLogOutTimeOut")) Then
                        LogOffF = True
                    Else
                        Timer3.Enabled = False
                        Timer3.Interval = (toval(GetAppSetting("ForceLogOutTimeOut")) - MinLapse) * 60000
                        Timer3.Enabled = True
                    End If
                End If
            Else
                Timer3.Enabled = False
                Timer3.Interval = (toval(GetAppSetting("ForceLogOutTimeOut")) - MinLapse) * 60000
                Timer3.Enabled = True
                LogOffF = True
            End If

            If LogOffF Then
                Timer1.Enabled = False
                Timer2.Enabled = False
                Timer3.Enabled = False
                Timer4.Enabled = False
                ws.LogOut(UserID, Password, Request.UserHostName, ErrMsg)
                AddUserPreferencesCookies(Me, "CAREAPP_UserID", "")
                AddUserPreferencesCookies(Me, "CAREAPP_Password", "")
                Session.Abandon()
                Session.RemoveAll()
                Session.Clear()
                FormsAuthentication.SignOut()
                Response.Cookies.Add(New HttpCookie("ASP.NET_SessionId", ""))
                Response.Redirect("WebSignOn.aspx")
            End If
        End If

    End Sub

    Private Sub WebAppMain_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        Page.ClientTarget = "uplevel"
    End Sub


    Private Sub WSCallback(ByVal ar As IAsyncResult)
        Try
            Dim rst As Boolean = False
            Dim DataSet As DataSet = Nothing
            Dim ws1 As localhost.DataAccess = ar.AsyncState
            Master.MasterViewState("AsyncStatus") = "Done"
            rst = ws1.EndGetDataSet(ar, DataSet, ErrMsg)
            If rst Then

                Master.MasterViewState("AsyncResult") = ""
                Master.MasterViewState("AsyncDataSet") = DataSet
            Else
                Master.MasterViewState("AsyncResult") = ErrMsg
                Master.MasterViewState("AsyncDataSet") = Nothing
            End If
        Catch ex As Exception
            Master.MasterViewState("AsyncStatus") = "Done"
            Master.MasterViewState("AsyncResult") = ex.Message
            Master.MasterViewState("AsyncDataSet") = Nothing
        End Try
    End Sub

    Private Sub BtnRefresh_Click(sender As Object, e As EventArgs) Handles BtnRefresh.Click
        Try
            RetrieveTaskList(LstTaskType.SelectedValue.ToUpper, LstTaskCategory.SelectedValue, tostr(LstTaskGroup.SelectedValue))
            Load_Task()
            UP_DataGrid.Update()
            UP_TaskInquiry.Update()
        Catch ex As Exception
            Master.Status = ex.Message
        Finally
            ExecuteScript(Me, "Updated()")
        End Try
    End Sub


    Private Sub LstTaskCategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LstTaskCategory.SelectedIndexChanged
        Dim sql As String
        Dim TaskType As String = tostr(LstTaskType.SelectedValue)



        Select Case True
            Case TaskType.ToUpper().Contains("PICKUP")
                If UCase(LstTaskGroup.SelectedValue) = "MY TASK" OrElse LstTaskGroup.SelectedValue = "2" Then
                    sql = "Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count'  from SysRequest S  "
                    sql = sql & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    sql = sql & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5 = SU.Type)) "
                    sql = sql & " Left Join SysUserGroup S5 on (S5.ID='" & UserID & "' and S.Param5 = S5.[Group])  "
                    sql = sql & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    sql = sql & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    sql = sql & " Group By S.ActionCode, AL.Description "
                ElseIf UCase(LstTaskGroup.SelectedValue) = "MY SUBORDINATE TASK" OrElse LstTaskGroup.SelectedValue = "1" Then
                    sql = "Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count'  from SysRequest S  "
                    sql = sql & " Left Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5) "
                    sql = sql & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    sql = sql & " Inner Join SysUser SU on (SU.ID='" & UserID & "' and (S.Param5=SU.ID or S.Param5=SU.Type or ActionUser.ActionUserAuth_1=SU.ID or ActionUser.ActionUserAuth_2=SU.ID or ActionUser.ActionUserAuth_3=SU.ID or ActionUser.ActionUserAuth_4=SU.ID or ActionUser.ActionUserAuth_5=SU.ID or ActionUser.ActionUserAuth_6=SU.ID or ActionUser.ActionUserAuth_7=SU.ID or ActionUser.ActionUserAuth_8=SU.ID or ActionUser.ActionUserAuth_9=SU.ID or ActionUser.ActionUserAuth_10=SU.ID)) "
                    sql = sql & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    sql = sql & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    sql = sql & " Group By S.ActionCode, AL.Description "
                ElseIf UCase(LstTaskGroup.SelectedValue) = "GROUP TASK" OrElse LstTaskGroup.SelectedValue = "3" Then
                    sql = "Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count'  from SysRequest S  "
                    sql = sql & " Inner Join SysUser SU on (SU.ID='" & UserID & "') "
                    sql = sql & " Inner Join SysUserGroup S5 on (S5.[Group]=S.Param5) "
                    sql = sql & " Inner Join ActionUser on (ActionUser.ActionCode=S.ActionCode And ActionUser.ActionUser=S.Param5 "
                    sql = sql & " And (S.Param5=SU.ID or S.Param5=S5.[Group] or ActionUser.ActionUserAuth_1=S5.[Group] Or ActionUser.ActionUserAuth_2=S5.[Group] Or ActionUser.ActionUserAuth_3=S5.[Group] Or ActionUser.ActionUserAuth_4=S5.[Group] Or ActionUser.ActionUserAuth_5=S5.[Group] Or ActionUser.ActionUserAuth_6=S5.[Group] Or ActionUser.ActionUserAuth_7=S5.[Group] Or ActionUser.ActionUserAuth_8=S5.[Group] Or ActionUser.ActionUserAuth_9=S5.[Group] Or ActionUser.ActionUserAuth_10=S5.[Group])) "
                    sql = sql & " Left Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                    sql = sql & " Where S.Param1='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                    sql = sql & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                    sql = sql & " Group By S.ActionCode, AL.Description "
                End If
            Case TaskType.ToUpper().Contains("PENDING")
                sql = " Select '' as ActionCode, '' as 'Count' UNION ALL Select Distinct S.ActionCode, AL.Description + ' (' + Cast(Count(S.ID) as varchar) + ')' as 'Count' from SysRequest S  "
                sql = sql & " Inner Join ActionList AL on (AL.ActionCode=S.ActionCode)  "
                sql = sql & " Inner Join SysUser SU on (SU.ID='" & UserID & "' And S.UserID=SU.ID ) "
                sql = sql & " Where S.Param1 ='SEND' and S.Param2 in ('MAR','UND','UND-MW','IT','CLM','SVC','FA','RQ','POLH','CLMH','GLH','PRPSL','PO','PRH') and S.Status='' "
                sql = sql & " And S.DueDate Between '" & Format(todate(TxtSDueDate.Text), "MM/DD/YYYY") & "' And '" & Format(todate(TxtEDueDate.Text), "MM/DD/YYYY") & "' "
                sql = sql & " Group By S.ActionCode, AL.Description "
        End Select

        If tostr(LstTaskCategory.SelectedValue) = "" Then
            FillListBox("1", UserID, Password, LstTaskCategory, sql, ErrMsg)
        End If

        Session("TaskGroup") = tostr(LstTaskGroup.SelectedValue)
        Session("TaskCategory") = tostr(LstTaskCategory.SelectedValue)
    End Sub
    Protected Sub LnkbtnViewDocFileNet_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim LnkbtnViewDocFileNet_ As System.Web.UI.WebControls.LinkButton
        Dim Row As System.Web.UI.WebControls.GridViewRow

        Dim LblPolicyNo As System.Web.UI.WebControls.Label


        LnkbtnViewDocFileNet_ = sender
        Row = CType(sender, System.Web.UI.WebControls.LinkButton).NamingContainer

        LblPolicyNo = CType(Row.FindControl("LblPolicyNo"), System.Web.UI.WebControls.Label)

        If LblPolicyNo Is Nothing Then
            Master.Status = "No PolicyNo Avalaible"
            Exit Sub
        End If

        Dim ws1 As New localhost.DataAccess(GetAppSetting("CareWebService1"))
        Dim datasetfilenet As DataSet = Nothing
        Dim CertificateNo As String = ""

        If ws1.GetDataSet(UserID, Password, datasetfilenet, "Select CertificateNo From Acceptance where PolicyNo= '" & tostr(LblPolicyNo.Text) & "'", ErrMsg) Then
            If datasetfilenet.Tables(0).Rows.Count <> 0 Then
                CertificateNo = tostr(datasetfilenet.Tables(0).Rows(0)("CertificateNo"))
            End If
        End If

        If LnkbtnViewDocFileNet_.Enabled Then
            OpenDialog(Me, "ListDocFileNetFrm.aspx", "PolicyNo", tostr(LblPolicyNo.Text), "CertificateNo", tostr(CertificateNo))
        End If
        'UP_LinkBtn.Update()
    End Sub
End Class