<%@ Page EnableEventValidation="false"  UICulture="auto" Culture="auto" Title="CareWebApplication | Home" Language="vb" AutoEventWireup="false" MasterPageFile="~/WebMaster.Master" CodeBehind="WebAppMain.aspx.vb" Inherits="WebApplication.WebAppMain" %>
<%@ MasterType virtualpath="~/WebMaster.master" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="ajaxToolkit" %>  
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<style type="text/css">
        .style3
        {
            padding-left: 5px;
            width: 130px;
            text-align: right;
            font-family: Calibri;
            font-size: 10pt;
        }
        #logo { 
           float: left; 
           width: 10.5em; 
           background: #CCCCCC; 
           padding: 0.5em 0.5em 0.5em 1em;
           margin: 0;        
        }
        .primaryStaticMenu
        {
           background-color: transparent;
           float: right;
        }
        .primaryStaticMenuItem
        {
           width: 15em;
           background-color: #F7F6F3; 
           border-width: 1px;
           border-color: #efefef #aaab9c #ccc #efefef; 
           border-style: solid;
           color: #333333;
           padding: 0.5em 0 0.5em 1em;
        }
        .primaryStaticHover
        {
           color: #ffffff;
           background: #91abc8;
        }

        .primaryDynamicMenu
        {   
           background-color: #f7f2ea;               
           border-bottom: solid 1px #ccc;    
        }  
        .primaryDynamicMenuItem
        {
           width: 15em;
           background-color: #f7f2ea; 
           color: #333333;
           padding: 0.5em 0 0.5em 1em;
           border-width: 1px;
           border-color: #f7f2ea #aaab9c #f7f2ea #efefef;  
           border-style: solid;         
        }    
        .primaryDynamicHover
        {
           color: #ffffff;
           background: #91abc8;
        }            

        .secondaryLevelOne
        {
           background-color: transparent;
           background-repeat: repeat-x;
           margin: 1.5em 0 0 0;
           padding: 5px 0 0 5px;
           width: 17em; 
           height: 35px;
        }
        .secondaryLevelTwo
        {
           background: #F7F6F3;
           padding: 5px 0 5px 5px;         
        }
        .secondaryStaticHover
        {
           color: #800000;
        }
    </style>
    </asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:UpdatePanel ID="UpdatePanelTimer" runat="server" 
    UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Timer ID="Timer1" runat="server" Enabled="False">
            </asp:Timer>
            <asp:Timer ID="Timer2" runat="server" Enabled="False">
            </asp:Timer>
            <asp:Timer ID="Timer3" runat="server" Enabled="False">
            </asp:Timer>
            <asp:Timer ID="Timer4" runat="server" Enabled="False" Interval="100">
            </asp:Timer>
        </ContentTemplate>
</asp:UpdatePanel>
	<asp:UpdatePanel ID="UP_Menu" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Menu id="MnuApp" runat="server" Orientation="Horizontal" StaticEnableDefaultPopOutImage="false"
                StaticDisplayLevels="1" MaximumDynamicDisplayLevels="1">
            <StaticMenuStyle CssClass="primaryStaticMenu"/>
            <StaticMenuItemStyle CssClass="primaryStaticMenuItem"/>
            <StaticHoverStyle CssClass="primaryStaticHover"/>   
            <DynamicMenuStyle CssClass="primaryDynamicMenu" />     
            <DynamicMenuItemStyle CssClass="primaryDynamicMenuItem"/>
            <DynamicHoverStyle CssClass="primaryDynamicHover"/>
            <Items>
                <asp:MenuItem Text="System" Selectable="false">
                    <asp:MenuItem Text="Change Password" Value="ChangePassword.aspx" />
                    <asp:MenuItem Text="Versioning Application" Value="VersionApp.aspx" />
                </asp:MenuItem>
                <asp:MenuItem Text="Management" Selectable="false">
                    <asp:MenuItem Text="Report Log" Value="WebRptLog.aspx" /> 
                    <asp:MenuItem Text="Report Portal" Value="PortalFrm.aspx" />                          
                </asp:MenuItem>
                <asp:MenuItem Text="Data Maintenance" Selectable="false">
                    <asp:MenuItem Text="Profile" Value="ProfileFrm.aspx" />
                    <asp:MenuItem Text="Location" Value="LocationFrm.aspx" />
                    <asp:MenuItem Text="Province" Value="ProvinceFrm.aspx" />
                    <asp:MenuItem Text="District" Value="DistrictFrm.aspx" />
                    <asp:MenuItem Text="City" Value="CityFrm.aspx" />
                    <asp:MenuItem Text="SubDistrict" Value="SubDistrictFrm.aspx" />
                    <asp:MenuItem Text="Village" Value="VillageFrm.aspx" />
                    <asp:MenuItem Text="ID Type" Value="IDTypeFrm.aspx" />
                    <asp:MenuItem Text="Gender" Value="GenderFrm.aspx" />
                    <asp:MenuItem Text="Income Source" Value="PIncomeSourceFrm.aspx" />
                </asp:MenuItem>
                <asp:MenuItem Text="Log Out" Value="LogOff.aspx" />
            </Items>         
          </asp:Menu>
          
        </ContentTemplate>
    </asp:UpdatePanel>
    
    <br />
    
    <asp:Panel ID="PnlForm" runat="server">
        <table style="width:100%;background:#ffffff; ">
            <tr>
                <td style="vertical-align:top; width:27%;">
                    <div>
                        <ajaxToolkit:TabContainer ID="TabContainer" runat="server" Enabled="true" ActiveTabIndex="0" AutoPostBack="false" BorderStyle="None" CssClass="ajax__tab_yuitabview-theme">
                            <ajaxToolkit:TabPanel runat="server" ID="TPApp">
                                <HeaderTemplate>Apps</HeaderTemplate>
                                <ContentTemplate>
                                    <asp:TreeView ID="TreeView" runat="server" NodeWrap="True" ImageSet="Arrows">
                                         <ParentNodeStyle Font-Bold="False" />
                                         <HoverNodeStyle Font-Underline="True" ForeColor="#5585DF" />
                                         <SelectedNodeStyle Font-Underline="True" ForeColor="#5585DF" 
                                             HorizontalPadding="0px" VerticalPadding="0px" />
                                         <RootNodeStyle ForeColor="#454545" />
                                         <NodeStyle Font-Names="Calibri" Font-Size="10pt" ForeColor="Black" HorizontalPadding="5px" NodeSpacing="0px" VerticalPadding="0px" />
                                        <LeafNodeStyle Font-Bold="False" ForeColor="#454545" />
                                    </asp:TreeView>   
                                </ContentTemplate>
                            </ajaxToolkit:TabPanel>
                            <ajaxToolkit:TabPanel runat="server" ID="TPRpt">
                                <HeaderTemplate>Reports</HeaderTemplate>
                                <ContentTemplate>
                                    <asp:Panel ID="PnlRpt" runat="server" Height="500px" ScrollBars="Auto">
                                    <asp:TreeView ID="TreeRpt" runat="server" NodeWrap="True" ImageSet="Arrows">
                                         <ParentNodeStyle Font-Bold="false" />
                                         <HoverNodeStyle Font-Underline="True" ForeColor="#5585df" />
                                         <SelectedNodeStyle Font-Underline="True" ForeColor="#5585df" 
                                             HorizontalPadding="0px" VerticalPadding="0px" />
                                         <RootNodeStyle ForeColor="#454545" />
                                         <NodeStyle Font-Names="Calibri" Font-Size="10pt" ForeColor="Black" 
                                             HorizontalPadding="5px" NodeSpacing="0px" VerticalPadding="0px" />
                                        <LeafNodeStyle Font-Bold="False" ForeColor="#454545" />
                                    </asp:TreeView>    
                                    </asp:Panel>
                                </ContentTemplate>
                            </ajaxToolkit:TabPanel>
                        </ajaxToolkit:TabContainer>        
                    </div>
                </td>
                <td style="vertical-align:top; width:73%;">
                    <asp:UpdatePanel ID="UP_TaskInquiry" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <table style="width:100%;">
                            <tr>
                                    <td class="style3">
                                        Task Due :
                                    </td>
                                    <td>
                                        <asp:TextBox ID="TxtSDueDate" runat="server" AutoPostBack="false" CssClass="DateField"/>
                                            <ajaxToolkit:CalendarExtender ID="TxtSDueDate_CalendarExtender" runat="server" Enabled="True" TargetControlID="TxtSDueDate" />
                                        <asp:Label ID="LblDueDate" runat="server" Text=" - " />
                                        <asp:TextBox ID="TxtEDueDate" runat="server" CssClass="DateField" />
                                            <ajaxToolkit:CalendarExtender ID="TxtEDueDate_CalendarExtender" runat="server" Enabled="True" TargetControlID="TxtEDueDate" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="style3">
                                        Task Lapse :
                                    </td>
                                    <td>
                                        <asp:ListBox ID="LstTaskLapse" Rows="1" runat="server" CssClass="ListBox">
                                            <asp:ListItem Value="" Text="" />
                                            <asp:ListItem Text="1 to 3 Days" Value="D1"></asp:ListItem>
                                            <asp:ListItem Text="4 to 7 Days" Value="D2"></asp:ListItem>
                                            <asp:ListItem Text="More Than 7 Days" Value="D3"></asp:ListItem>
                                        </asp:ListBox>                                   
                                    </td>
                                </tr>
                                <tr>
                                    <td class="style3">
                                        Task Type :
                                    </td>
                                    <td>
                                        <asp:ListBox ID="LstTaskGroup" runat="server" AutoPostBack="true" Rows="1" CssClass="ListBox" />
                                        <asp:ListBox ID="LstTaskType" runat="server" AutoPostBack="true" Rows="1" CssClass="ListBox" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="style3">
                                        Task Category :
                                    </td>
                                    <td>
                 
                                        <asp:ListBox ID="LstTaskCategory" runat="server" AutoPostBack="true" Rows="1" CssClass="ListBox" />  
                     
                                    </td>
                                </tr>
                                <tr>
                                    <td class="style3">
                                        Order By :
                                    </td>
                                    <td>
                                        <asp:ListBox ID="LstOrderBy" Rows="1" runat="server" CssClass="ListBox">  
                                            <asp:ListItem Text="DESC" Value="desc"></asp:ListItem>
                                            <asp:ListItem Text="ASC" Value="asc"></asp:ListItem>
                                        </asp:ListBox> 
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">   
                                    </td>
                                </tr>
                                <tr>
                                    <td class="style3">
                                        Type of Cover :  </td>
                                    <td>
                                        <asp:TextBox ID="TxtTOC" runat="server" CssClass="F2Items" AutoPostBack="false" ToolTip="Press (F2) to see list"/>
                                        <asp:TextBox ID="TxtTOCDesc" runat="server" CssClass="TextBox" Enabled="False" Width="300px" />                                                
                                    </td>
                                </tr>
                                 <tr>
                                    <td class="style3">
                                        Branch : </td>
                                    <td>
                                        <asp:TextBox ID="TxtBranchID" runat="server" CssClass="F2Items" AutoPostBack="false" ToolTip="Press (F2) to see list"/>
                                        <asp:TextBox ID="TxtBranchName" runat="server" CssClass="TextBox" Enabled="False"  Width ="300px" />
                                    </td>
                                </tr>
                                 <tr>
                                    <td class="style3">
                                        Sender ID : </td>
                                    <td>
                                        <asp:TextBox ID="TxtSenderID" runat="server" CssClass="F2Items" AutoPostBack="false" ToolTip="Press (F2) to see list"/>
                                        <asp:TextBox ID="TxtSenderName" runat="server" CssClass="TextBox" Enabled="False"  Width ="300px" />
                                    </td>
                                </tr>
                                  <tr>
                                    <td class="style3">
                                        Business Source ID :  </td>
                                    <td>
                                        <asp:TextBox ID="TxtSourceID" runat="server" CssClass="F2Items" AutoPostBack="false" ToolTip="Press (F2) to see list"/>
                                        <asp:TextBox ID="TxtSourceName" runat="server" CssClass="TextBox" Enabled="False"  Width ="300px" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="style3">
                                         <asp:Label ID="LblDocno" runat="server" Width="122px" Text="CI No. :"></asp:Label>

                                    </td>
                                    <td>
                                        <asp:TextBox ID="TxtCINo" runat="server" CssClass="TextBox" Width="400px" />
                                          
                                    </td>
                                </tr>
                                <tr>
                                    <td class="style3">
                                        Remarks :
                                    </td>
                                    <td>
                                        <asp:TextBox ID="TxtRemarks" runat="server" CssClass="TextBox" Width="400px" />

                                      

                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="2" align="center" >

                                          <asp:Button ID="BtnFind" runat="server" Font-Names="Calibri" Font-Size="10pt" Text="Find Task" onclientclick="Updating()" Width="200px" />      
 <asp:Button ID="BtnRefresh" runat="server" Font-Names="Calibri" Font-Size="10pt" Text="Refresh Task" onclientclick="Updating()" Width="200px" />      

                                    </td>
                                    
                                </tr>
                                 
                            </table>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div>
                        <asp:UpdatePanel ID="UP_DataGrid" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>                                
                                    <asp:GridView ID="DataGrid" runat="server" EnableViewState="false"
                                        AutoGenerateColumns="False" GridLines="Horizontal" AllowPaging="True" PageSize="5" 
                                        CaptionAlign="Top" CellPadding="3" EnableSortingAndPagingCallbacks="True" 
                                        Font-Names="Calibri" Font-Size="10pt"  ShowHeader="False"
                                        ForeColor="#333333" HorizontalAlign="Left" Width="100%" >
                                        <FooterStyle BackColor="#5D7B9D" Font-Size="8pt" ForeColor="White" Font-Bold="True" />
                                        <AlternatingRowStyle BackColor="#F4F3FE" ForeColor="Black" />
                                        <Columns>            
                                            <asp:TemplateField>
                                                <ItemTemplate>
                                                    <table style="width:660px;">
                                                        <tr>
                                                            <td style="width:20px; vertical-align:top;" >
                                                                <asp:Label ID="LblMessageNo" runat="server" Text="" />
                                                            </td>
                                                            <td style="vertical-align:top;text-align:right; width:90px;">
                                                                Message No. 
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;">:</td>
                                                            <td style="vertical-align:top; " colspan ="2">
                                                                <asp:Label ID="LblID" runat="server" Text='<%# Eval("ID") %>' ForeColor="Black" Font-Bold="true" />.<asp:Label ID="LblParam3" runat="server" Text='<%# Eval("PARAM3") %>' ForeColor="Black" />.<asp:Label ID="LblParam4" runat="server" Text='<%# Eval("PARAM4") %>' ForeColor="Black" /><asp:Label ID="LblParam6" runat="server" Text='<%# Eval("PARAM6") %>' ForeColor="Black" Visible="false" /> (<asp:Label ID="LblParam2" runat="server" Text='<%# Eval("AppName") %>' ForeColor="Black" />)
                                                            </td>
                                                        </tr>
                                                       
                                                        <tr>
                                                            <td style="width:20px; vertical-align:top;">&nbsp;</td>
                                                            <td style="vertical-align:top; text-align:right;width:90px;">
                                                                <asp:Label ID="LblSentTitle" runat="server" />
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;">:</td>
                                                            <td style="vertical-align:top; " colspan ="2">
                                                                <asp:Label ID="LblSentRemark" runat="server" ForeColor="Black" />
                                                            </td>
                                                        </tr>
                                                         <tr>
                                                            <td style="width:20px; vertical-align:top;">&nbsp;</td>
                                                            <td style="vertical-align:top;text-align:right; width:90px;">
                                                                 User Branch 
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;">:</td>
                                                            <td style="vertical-align:top; " colspan ="2">
                                                                <asp:Label ID="LblBranchDesc" runat="server" ForeColor="Black" />
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style="width:20px; vertical-align:top;">&nbsp;</td>
                                                            <td style="vertical-align:top;text-align:right; width:90px;">
                                                                <asp:Label ID="LblDataNoDesc" runat="server" Text="" />
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;">:</td>
                                                            <td style="vertical-align:top; width:150px;" colspan="2">
                                                                <asp:Label ID="LblDataNo" runat="server" Text='<%# Eval("DataNo") %>' ForeColor="Black" /> 


<asp:TextBox ID="TxtDocNo" runat="server" CssClass="TextBox" Width="0px" Height="0px" BorderWidth="0px" Text='<%# Eval("DocNo") %>' />
<asp:TextBox ID="TxtDocType" runat="server" CssClass="TextBox" Width="0px" Height="0px" BorderWidth="0px" Text='<%# Eval("DocType") %>' />
                                                            </td>
                                                        </tr>
<tr>
                                                            <td style="width:20px; vertical-align:top;">&nbsp;</td>
                                                            <td style="vertical-align:top;text-align:right; width:90px;">
                                                                <asp:Label ID="LblTitleStatus" runat="server" Text="Status " />
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;"><asp:Label ID="Label2" runat="server" Text=":" /></td>
                                                            <td style="vertical-align:top; width:150px;" >
                                                                 <asp:Label ID="lblStatus_Transaction" runat="server" Text='<%# Eval("Status_Transasction") %>' ForeColor="Black" />  
                                                            </td>
                                                            <td style="text-align:right; padding-right:5px;">

                                                            </td>
                                                        </tr>

                                                        <tr>
                                                            <td style="width:20px; vertical-align:top;">&nbsp;</td>
                                                            <td style="vertical-align:top;text-align:right; width:90px;">
                                                                <asp:Label ID="LblPolicy" runat="server" Text="Policy No." />
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;"><asp:Label ID="LblSeparator1" runat="server" Text=":" /></td>
                                                            <td style="vertical-align:top; width:150px;" >
                                                                <asp:Label ID="LblPolicyNo" runat="server" ForeColor="Black" />
                                                            </td>
                                                            <td style="text-align:right; padding-right:5px;">
                                                                 <asp:LinkButton ID="LnkbtnViewDocFileNet" runat="server" Text="View Doc FileNet" onclick="LnkBtnViewDocFileNet_Click" />&nbsp;◦
                                                                <asp:LinkButton ID="LnkBtnViewPic" runat="server" Text="View Image" onclick="LnkBtnViewPic_Click" />&nbsp;◦
                                                                <asp:LinkButton ID="LnkBtnViewDoc" runat="server" Text="View Doc" onclick="LnkBtnViewDoc_Click" />
                                                            </td>
                                                        </tr>
                                                          <tr>
                                                            <td style="width:20px; vertical-align:top;" >
                                                                <asp:Label ID="Lbl_TOC" runat="server" Text="" />
                                                            </td>
                                                            <td style="vertical-align:top;text-align:right; width:90px;">
                                                                TOC. 
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;">:</td>
                                                            <td style="vertical-align:top; " colspan ="2">
                                                                <asp:Label ID="LblTOC" runat="server" Text='<%# Eval("TOC") %>' ForeColor="Black" Font-Bold="true" />.
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style="width:20px; vertical-align:top;">&nbsp;</td>
                                                            <td style="vertical-align:top;text-align:right; width:90px;">
                                                                 <asp:Label ID="LblMark" runat="server" />
                                                            </td>
                                                            <td style="width:10px; vertical-align:top;"><asp:Label ID="LblSeparator2" runat="server" Text=":" /></td>
                                                            <td style="vertical-align:top; width:150px;" >
                                                                <asp:Label ID="LblParam5" runat="server" ForeColor="Black" />
                                                            </td>
                                                            <td style="text-align:right; padding-right:5px;">
                                                              <asp:UpdatePanel ID="UP_LinkBtn" runat="server" UpdateMode="Conditional">
                                                                <ContentTemplate>
                                                                    <asp:LinkButton ID="LnkBtnPickUp" runat="server" Text="Pick Up" onclick="LnkBtnPickUp_Click" />&nbsp;◦
                                                                    <asp:LinkButton ID="LnkBtnReply" runat="server" Text="Reply" onclick="LnkBtnReply_Click" />&nbsp;◦
                                                                    <asp:LinkButton ID="LnkBtnViewLog" runat="server" Text="View Log" onclick="LnkBtnViewLog_Click" />&nbsp;◦
                                                                    <asp:LinkButton ID="LnkBtnAssign" runat="server" Text="Assign" onclick="LnkBtnAssign_Click" />&nbsp;◦
                                                                    <asp:LinkButton ID="LnkBtnDone" runat="server" Text="Mark as Done" onclick="LnkBtnDone_Click" />&nbsp;◦
                                                                    <asp:LinkButton ID="LnkBtnDrop" runat="server" Text="Decline" onclick="LnkBtnDrop_Click" />
                                                                        <ajaxToolkit:ConfirmButtonExtender ID="LnkBtnDrop_ConfirmButtonExtender" runat="server" ConfirmText="Decline This Task ?" Enabled="True" TargetControlID="LnkBtnDrop" />
                                                                 </ContentTemplate>
                                                            </asp:UpdatePanel>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style="width:20px; vertical-align:top;">&nbsp;</td>
                                                            <td colspan="4">
                                                                <asp:TextBox ID="TxtMessage" runat="server" Text='<%# Eval("REMARKS") %>' ForeColor="Black" TextMode="MultiLine" CssClass="TextBox" Width="100%" Height="75px" ReadOnly="true" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <EditRowStyle BackColor="#999999" />
                                        <HeaderStyle BackColor="#F7F6F3" Font-Names="Calibri" Font-Size="10pt" 
                                            ForeColor="#333333" HorizontalAlign="Left" Font-Bold="False" />
                                        <PagerStyle BackColor="#5D7B9D" ForeColor="White" HorizontalAlign="Left" 
                                            Font-Names="Calibri" Font-Size="10pt" />
                                        <RowStyle BackColor="#F4F3FE" ForeColor="Black" HorizontalAlign="Left" />
                                        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                                    </asp:GridView>    
                                
                            </ContentTemplate>
                        </asp:UpdatePanel>      
                    </div>
                </td>
            </tr>
        </table>    
    </asp:Panel>
</asp:Content>
