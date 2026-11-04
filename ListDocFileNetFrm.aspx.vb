Imports System.Globalization
Imports WebApplication.localhost2

Partial Public Class ListDocFileNetFrm
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
    End Sub

    Private Sub Page_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete
        If Not IsPostBack Then

            Dim PolicyNo As String = tostr(Request.QueryString("PolicyNo"))
            Dim CertificateNo As String = tostr(Request.QueryString("CertificateNo"))

            TxtPolicyNo.Text = PolicyNo

            Master.Title = "Document List"


            Load_Item(PolicyNo, CertificateNo)
        End If
    End Sub

    Private Sub Load_Item(ByVal PolicyNo As String, ByVal CertificateNo As String)
        Try

            'Dim wsb2b As New localhost10.WSB2B(GetAppSetting("CareWSB2B"))
            Dim wsb2b As New localhost10.WSB2B("http://localhost/WSB2B/WSB2B.asmx")

            Dim inparam(2) As Object
            Dim outparam(2) As Object

            inparam(0) = PolicyNo
            inparam(1) = CertificateNo
            inparam(2) = "AvristGroup"

            Dim ds As New DataSet()
            Dim dt As New DataTable("Items")

            ' Define your columns based on what you expect
            dt.Columns.Add("URL", GetType(String))
            ' Add more columns if the string contains more data

            If Not wsb2b.SendRequestToFilenet(inparam, outparam, ErrMsg) Then
                Throw New Exception(ErrMsg)
            Else
                ' Loop through the Object array
                For Each item As Object In outparam
                    If item IsNot Nothing Then
                        Dim row As DataRow = dt.NewRow()
                        row("URL") = item.ToString()
                        ' If you need to split the string, do it here:
                        ' Dim parts() As String = item.ToString().Split(","c)
                        ' row("Name") = parts(0)

                        dt.Rows.Add(row)
                    End If
                Next

                ds.Tables.Add(dt)
            End If

            'UP_DataGrid.DataSource = ds
            Master.MasterViewState("dsfilenet") = ds
            DataGrid.DataSource = ds
            DataGrid.DataBind()
            UP_DataGrid.DataBind()

        Catch ex As Exception
            Master.Status = ex.Message
        Finally
            ExecuteScript(Me, "Updated()")
        End Try
    End Sub

    Private Sub DataGrid_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataGrid.Load
        Dim Dataset As DataSet = CType(Master.MasterViewState("dsfilenet"), DataSet)
        If Not Dataset Is Nothing Then
            Master.MasterViewState("dsfilenet") = Dataset
            DataGrid.DataSource = Dataset
            DataGrid.DataBind()
        End If
    End Sub

    Private Sub DataGrid_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles DataGrid.DataBound
        Dim Row As System.Web.UI.WebControls.GridViewRow
        Dim DataGrid As System.Web.UI.WebControls.GridView
        Dim LnkButton As System.Web.UI.WebControls.LinkButton
        Dim TxtURL As System.Web.UI.WebControls.TextBox
        Dim ws As New localhost1.WSCommon

        DataGrid = CType(sender, GridView)


        Dim NumRows As Integer = DataGrid.Rows.Count
        Dim i As Integer
        Dim fname As String = "DocViewer.aspx"

        For i = 0 To NumRows - 1
            Row = DataGrid.Rows(i)

            LnkButton = CType(Row.FindControl("LnkBtnDocument"), System.Web.UI.WebControls.LinkButton)
            TxtURL = CType(Row.FindControl("TxtURL"), System.Web.UI.WebControls.TextBox)

            'If LnkButton IsNot Nothing Then
            '    ' Get the URL from the row (adjust column index / control name as needed)
            '    Dim fileUrl As String = TxtURL.text ' assuming URL is in column 1
            '    ' If you have a hidden field or control, use Row.FindControl instead

            '    ' Build a friendly filename (extract from URL or use PolicyNo)
            '    Dim fileName As String = "Document_" & i & ".pdf"

            '    ' Encode URL so query string works
            '    Dim downloadUrl As String = "DownloadFileNet.aspx?url=" & HttpUtility.UrlEncode(fileUrl) & "&fname=" & HttpUtility.UrlEncode(fileName)

            '    ' Force the LinkButton to post to the download page
            '    LnkButton.PostBackUrl = downloadUrl
            'End If

            If LnkButton IsNot Nothing Then
                Dim fileUrl As String = TxtURL.Text

                ' Append download=1 to force direct download
                Dim sep As String = If(fileUrl.Contains("?"), "&", "?")
                Dim downloadUrl As String = fileUrl & sep & "download=1"

                LnkButton.Attributes.Add("onclick",
                    "window.open('" & downloadUrl & "','_blank'); return false;")
            End If

        Next
    End Sub


End Class