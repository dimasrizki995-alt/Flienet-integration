Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Net.Security
Imports System.Net.WebRequestMethods
Imports System.Reflection.Emit
Imports System.Security
Imports System.Security.Authentication
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading.Tasks
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports CareWebLibrary
Imports CareWebLibrary.CursorTypeEnum
Imports Newtonsoft.Json.Linq
Imports File = System.IO.File

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class WSB2B
    Inherits System.Web.Services.WebService
    Dim db As ClassLib
    Dim dbName As String
    Dim dbServer As String
    Dim dbSysUser As String
    Dim dbSysPassword As String
    Dim dbUser As String
    Dim dbUserType As String
    Dim dbUserCCode As String
    Dim dbPassword As String
    Dim dbTimeOut As Integer
    Dim dbTrusted As Boolean
    Dim StrMsg As String
    Dim WI As System.Security.Principal.WindowsIdentity
    Dim WIC As System.Security.Principal.WindowsImpersonationContext
    Public Sub New()
        db = Nothing
        dbUser = ""
        dbPassword = ""
    End Sub
    Private Function OpenConnection(ByRef _dbUser As String, ByRef _dbPassword As String, ByRef ErrMsg As String) As Boolean
        Try
            If db Is Nothing Then
                db = New ClassLib
                dbUser = _dbUser
                dbPassword = _dbPassword
                dbName = GetAppSetting("dbName")
                dbServer = GetAppSetting("dbServer")
                dbSysUser = GetAppSetting("dbSysUser")
                dbSysPassword = GetAppSetting("dbSysPassword")
                dbTimeOut = toval(GetAppSetting("dbTimeout"))
                dbTrusted = toval(GetAppSetting("dbTrusted"))
                If dbTrusted Then
                    WI = CType(HttpContext.Current.User.Identity, System.Security.Principal.WindowsIdentity)
                    WIC = WI.Impersonate
                End If
                If Not db.OpenSession("", _dbUser, _dbPassword, dbName, dbServer, dbSysUser, dbSysPassword, dbTimeOut, False, dbTrusted) Then Throw New Exception("Opening Session Failed !")
                dbTrusted = db.Get_DbTrusted
                dbUserType = db.Var_db_User_Type
                dbUserCCode = db.Var_db_User_CCode
                If dbTrusted Then
                    dbUser = db.Var_db_user
                    dbPassword = db.Var_db_Password
                    _dbUser = dbUser
                    _dbPassword = dbPassword
                End If
            End If

            Const _Tls12 As SslProtocols = DirectCast(&HC00, SslProtocols)
            Const Tls12 As SecurityProtocolType = DirectCast(_Tls12, SecurityProtocolType)
            ServicePointManager.SecurityProtocol = Tls12
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            db = Nothing
            Return False
        End Try
    End Function
    Private Function CloseConnection() As Boolean
        Try
            If Not db Is Nothing Then
                db.CloseSession()
                db = Nothing
                dbUser = ""
                dbPassword = ""
            End If
        Catch ex As Exception
            db = Nothing
        End Try
        Return True
    End Function

    <WebMethod(EnableSession:=False)>
    Public Function UpdateLogFileH(ByVal dbUser As String, dbPassword As String, ByRef Dataset As DataSet, ByRef Outparam() As Object, ByRef ErrMsg As String) As Boolean

        Try

            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            Dim SQL As String = ""
            Dim rs As RemoteSet

            db.Begintrans()

            For i = 0 To Dataset.Tables(0).Rows.Count - 1
                'upload
                If tostr(Dataset.Tables(0).Rows(i)("Status")) = "N" And tostr(Dataset.Tables(0).Rows(i)("UploadDate")) <> "" And tostr(Dataset.Tables(0).Rows(i)("UploadTime")) <> "" And Dataset.Tables(0).Rows(i)("totalUpload") <> "0" And tostr(Dataset.Tables(0).Rows(i)("BatchNo")) = "" And tostr(Dataset.Tables(0).Rows(i)("errmsg")) <> "" Then
                    If Not db.Execute("Update LOGFILEH set status='R' , errmsg = '', UploadDate = '' , uploadtime='' where BID = '" & Dataset.Tables(0).Rows(i)("BID") & "'") Then
                        Throw New Exception(db.ErrDesc)
                    End If
                End If

                'download
                If tostr(Dataset.Tables(0).Rows(i)("Status")) = "N" And (tostr(Dataset.Tables(0).Rows(i)("PutfileDate")) = "" And tostr(Dataset.Tables(0).Rows(i)("PutfileTime")) = "") And tostr(Dataset.Tables(0).Rows(i)("BatchNo") <> "", ) And (tostr(Dataset.Tables(0).Rows(i)("UploadDate")) <> "" And tostr(Dataset.Tables(0).Rows(i)("UploadTime")) <> "") And (tostr(Dataset.Tables(0).Rows(i)("Totalcompleted")) = "0" And tostr(Dataset.Tables(0).Rows(i)("Totalfailed")) = "0") And tostr(Dataset.Tables(0).Rows(i)("errmsg")) <> "" Then
                    If Not db.Execute("Update LOGFILEH set ErrMsg = '', status='P' where BID = '" & Dataset.Tables(0).Rows(i)("BID") & "'") Then
                        Throw New Exception(db.ErrDesc)
                    End If
                End If

                'putfile
                If tostr(Dataset.Tables(0).Rows(i)("Status")) = "N" And tostr(Dataset.Tables(0).Rows(i)("PutfileDate")) = "" And tostr(Dataset.Tables(0).Rows(i)("BatchNo") <> "", ) And (tostr(Dataset.Tables(0).Rows(i)("UploadDate")) <> "" Or tostr(Dataset.Tables(0).Rows(i)("UploadTime")) <> "") And (tostr(Dataset.Tables(0).Rows(i)("Totalcompleted")) <> "0" Or tostr(Dataset.Tables(0).Rows(i)("Totalfailed")) <> "0") And tostr(Dataset.Tables(0).Rows(i)("errmsg")) <> "" Then
                    If Not db.Execute("Update LOGFILEH set ErrMsg = '', status='C'  where BID = '" & Dataset.Tables(0).Rows(i)("BID") & "'") Then
                        Throw New Exception(db.ErrDesc)
                    End If
                End If

                'email
                If tostr(Dataset.Tables(0).Rows(i)("Status")) = "N" And tostr(Dataset.Tables(0).Rows(i)("PutfileDate")) <> "" And tostr(Dataset.Tables(0).Rows(i)("BatchNo") <> "") And (tostr(Dataset.Tables(0).Rows(i)("UploadDate")) <> "" And tostr(Dataset.Tables(0).Rows(i)("UploadTime")) <> "") And tostr(Dataset.Tables(0).Rows(i)("errmsg")) <> "" Then
                    If Not db.Execute("Update LOGFILEH set status='C', ErrMsg = '' where BID = '" & Dataset.Tables(0).Rows(i)("BID") & "'") Then
                        Throw New Exception(db.ErrDesc)
                    End If
                End If


            Next

            db.Committrans()


            Return True
        Catch ex As Exception
            ErrMsg = ex.Message
            db.RollBack()
            CloseConnection()

            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function SearchEmailRule(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef Dataset As DataSet, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim Category As String
            Dim ERule As String
            Dim SQL As String

            Category = getInParamValue(InParam, 0)
            ERule = getInParamValue(InParam, 1)

            Dim rs As RemoteSet

            SQL = "Select Cast(0 as bit) as ProcessFlag, * From HEmail_Rule where 1=1"
            If Category <> "" Then
                SQL = SQL & " and Category Like '" & Category & "%'"
            End If

            If ERule <> "" Then
                SQL = SQL & " and ERule Like '" & ERule & "%'"
            End If

            rs = db.OpenRemoteSet(SQL, adOpenKeyset)
            Dataset = rs.GetDataSet


            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function GetEmailRule(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef findex() As String, ByRef fbuffer() As String, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim Category As String 'ProductType
            Dim Erule As String

            Category = getInParamValue(InParam, 0)
            Erule = getInParamValue(InParam, 1)
            Dim rs As RemoteSet
            rs = db.OpenRemoteSet("Select * from HEmail_Rule Where Category='" & Category & "' and ERule='" & Erule & "'")
            'rs = db.OpenRemoteSet(SQL)
            findex = rs.fields_index()
            fbuffer = rs.GetRecord()

            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function SaveEmailRule(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef OutParam() As Object, ByVal findex() As String, ByVal fbuffer() As String, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim Category As String
            Dim ERule As String
            Category = getInParamValue(InParam, 0)
            ERule = getInParamValue(InParam, 1)

            Dim rs As RemoteSet
            Dim EditMode As Integer
            rs = db.OpenRemoteSet("Select * from HEmail_Rule Where Category='" & Category & "' and ERule='" & ERule & "'")

            If rs.Eof Then
                rs.AddNew()
            Else
                rs.Edit()
            End If
            EditMode = rs.editmode

            ' Check Status
            Dim i As Integer
            Dim j As Integer
            i = 0
            j = UBound(findex)
            Do While i < j
                rs.fields(findex(i)) = GetFieldValue(fbuffer(i))
                i = i + 1
            Loop
            rs.fields("Last_Opr") = dbUser
            rs.fields("Last_Updated") = Today.Date
            If Not rs.Update Then Throw New Exception(db.ErrDesc)

            ' Set Output Parameter
            ReDim OutParam(1)
            OutParam(0) = rs.fields("Category")
            OutParam(1) = rs.fields("ERule")

            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function DropEmailRule(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim Category As String
            Dim ERule As String
            Category = getInParamValue(InParam, 0)
            ERule = getInParamValue(InParam, 1)
            Dim rs As RemoteSet


            rs = db.OpenRemoteSet("Select * from HEmail_Rule Where Category='" & Category & "' and ERule='" & ERule & "'")
            If rs.Eof Then
                ErrMsg = "No Email Rule Data !"
                Throw New Exception(ErrMsg)
            End If
            If Not rs.Delete Then Throw New Exception(db.ErrDesc)

            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function SearchLogProcess(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef Dataset As DataSet, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim CCODE As String
            Dim ERule As String
            Dim SQL As String

            CCODE = getInParamValue(InParam, 0)
            Dim rs As RemoteSet

            SQL = "Select Cast(0 as bit) as ProcessFlag, * From LogProcess where 1=1"
            If CCODE <> "" Then
                SQL = SQL & " and Ccode Like '" & CCODE & "%'"
            End If

            rs = db.OpenRemoteSet(SQL, adOpenKeyset)
            Dataset = rs.GetDataSet


            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function SearchLogFile(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef dt As DataTable, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            dt = New DataTable("LogFile")
            Dim Path As String = InParam(0)
            dt.Columns.Add("FileName")
            dt.Columns.Add("Size")
            dt.Columns.Add("Path")


            For Each file As String In Directory.GetFiles(Path)

                Dim fi As New FileInfo(file)

                dt.Rows.Add(
            fi.Name,
            Math.Round(fi.Length / 1024, 2),
            fi.FullName
            )

            Next

            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function GetLogProcess(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef findex() As String, ByRef fbuffer() As String, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim CCODE As String

            CCODE = getInParamValue(InParam, 0)
            Dim rs As RemoteSet
            rs = db.OpenRemoteSet("Select * from LogProcess Where CCODE='" & CCODE & "'")
            'rs = db.OpenRemoteSet(SQL)
            findex = rs.fields_index()
            fbuffer = rs.GetRecord()

            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function SaveLogProcess(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef OutParam() As Object, ByVal findex() As String, ByVal fbuffer() As String, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim CCODE As String
            CCODE = getInParamValue(InParam, 0)

            Dim rs As RemoteSet
            Dim EditMode As Integer
            rs = db.OpenRemoteSet("Select * from LogProcess Where CCODE='" & CCODE & "'")

            If rs.Eof Then
                rs.AddNew()
            Else
                rs.Edit()
            End If
            EditMode = rs.editmode

            ' Check Status
            Dim i As Integer
            Dim j As Integer
            i = 0
            j = UBound(findex)
            Do While i < j
                rs.fields(findex(i)) = GetFieldValue(fbuffer(i))
                i = i + 1
            Loop
            rs.fields("Last_Opr") = dbUser
            rs.fields("Last_Updated") = Today.Date
            If Not rs.Update Then Throw New Exception(db.ErrDesc)

            ' Set Output Parameter
            ReDim OutParam(1)
            OutParam(0) = rs.fields("CCODE")

            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function GetLogFile(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef Files As Byte(), ByRef ErrMsg As String) As Boolean
        Try


            Dim fileBytes As Byte() = File.ReadAllBytes(InParam(0))
            Files = fileBytes
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function

    <WebMethod(EnableSession:=False)>
    Public Function DropLogProcess(ByVal dbUser As String, ByVal dbPassword As String, ByVal InParam() As Object, ByRef ErrMsg As String) As Boolean
        Try
            If Not OpenConnection(dbUser, dbPassword, ErrMsg) Then
                Return False
            End If

            db.Begintrans()

            ' Get Input Parameter
            Dim CCODE As String
            CCODE = getInParamValue(InParam, 0)
            Dim rs As RemoteSet


            rs = db.OpenRemoteSet("Select * from LogProcess Where CCODE='" & CCODE & "'")
            If rs.Eof Then
                ErrMsg = "No Schedule Process !"
                Throw New Exception(ErrMsg)
            End If
            If Not rs.Delete Then Throw New Exception(db.ErrDesc)

            db.Committrans()
            CloseConnection()
            Return True
        Catch ex As Exception
            ErrMsg = db.ErrDesc
            db.RollBack()
            CloseConnection()
            If ErrMsg = "" Then ErrMsg = ex.Message
            Return False
        End Try
    End Function

    Public Shared Function ValidateRemoteCertificate(ByVal sender As Object, ByVal certificate As X509Certificate, ByVal chain As X509Chain, ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
        Return True
    End Function
    <WebMethod(EnableSession:=False)>
    Public Function SendRequestToFilenet(ByVal InParam() As Object, ByRef Outparam() As Object, ByRef ErrMsg As String)
        Dim sourceError As String = ""
        Try
            Dim ApiUrl As String = "https://sandbox-middleware.care.co.id:9096/API_AVRIST/Avr/getDocumentLink"
            Dim bearerToken As String = "6abc195b5ea1c9ff02306f6736195c0541517ad4"
            Using client As New HttpClient()
                Try
                    ' 1. Set Header Authorization Bearer
                    client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", bearerToken)

                    ' 2. Buat Payload / Request Body dalam bentuk Anonymous Object atau Class
                    Dim requestPayload = New With {
                    .policyNo = InParam(0),
                    .certificateNo = InParam(1),
                    .objectStore = InParam(2)
                }

                    ' 3. Serialize object ke format JSON
                    Dim jsonContent As String = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload)
                    Dim httpContent As New StringContent(jsonContent, Encoding.UTF8, "application/json")

                    ' 4. Lakukan HTTP POST secara synchronous menggunakan .Result
                    Dim response As HttpResponseMessage = client.PostAsync(ApiUrl, httpContent).Result

                    ' 5. Cek apakah status sukses (200 OK)
                    If response.IsSuccessStatusCode Then
                        ' Baca response string (JSON) secara synchronous
                        Dim jsonResponse As String = response.Content.ReadAsStringAsync().Result
                        Dim jsonObject As JObject = JObject.Parse(jsonResponse)

                        Dim i As Integer = 0
                        For Each item In jsonObject("data")("content")
                            ' Langsung ambil nilainya satu per satu secara otomatis
                            Outparam(i) = item("documentLink").ToString()
                            i += 1
                        Next

                        Return True
                    Else
                        Return "Error: " & response.StatusCode.ToString()
                    End If

                Catch ex As Exception
                    Return "Exception: " & ex.Message
                End Try
            End Using

            Return True
        Catch ex As Exception
            ErrMsg = ex.Message & " - " & sourceError
            Return False
        End Try
    End Function

End Class