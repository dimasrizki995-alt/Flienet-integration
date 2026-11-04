<%@ Page Trace="false" Language="vb" UICulture="auto" Culture="auto" AutoEventWireup="false" MasterPageFile="~/WebMaster.Master" CodeBehind="ListDocFileNetFrm.aspx.vb" Inherits="WebApplication.ListDocFileNetFrm" Title="ListDocFileNetFrm | File Net Attachments List"%>
<%@ MasterType virtualpath="~/WebMaster.master" %>
 
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="ajaxToolkit" %>  
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style12
        {
            text-align: right;
            width: 150px;
            padding-right: 5px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    
    <table style="width:100%; text-align:left;">
        <tr>
            <td style="text-align:right;">            
                <asp:LinkButton ID="LnkBtnClose" runat="server" Text="Close This Page" OnClientClick="CloseWindow();return false"/>
            </td>
        </tr>
    </table>
    
    <div align="center">
        <asp:Panel ID="PnlMain" runat="server" Width="450px">
            <asp:UpdatePanel ID="UpdatePanel12" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <table class="TableTitle" style="width: 100%;">
                        <tr>
                            <td class="TableTitle" style="text-align:center;">
                                Documents List</td>
                        </tr>
                    </table>
                    <table style="width:100%; text-align:left; background:#f4f3fe; border: solid 1px #EAEAEA;">
                        <tr>
                            <td class="style12">
                                <asp:label ID="LblPolicyNo" runat="server" Text="Policy No." />
                            </td>
                            <td>
                                <asp:TextBox ID="TxtPolicyNo" runat="server" CssClass="TextBox" Width="150px" Enabled="false"/>
                            </td>
                        </tr>

                        <tr><td colspan="2">&nbsp;</td></tr>

                    </table>
                </ContentTemplate>
            </asp:UpdatePanel>
            
        </asp:Panel>
    </div>
 
    <br />
    
    <asp:UpdatePanel ID="UP_DataGrid" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="PnlForm" runat="server">
                
                <table style="width:100%;">
                    <tr>
                        <td align="left" style="width:980px; vertical-align:top;">
                            <asp:GridView ID="DataGrid" runat="server" AllowPaging="False" AutoGenerateColumns="False" AllowSorting="false"
                            CaptionAlign="Top" CellPadding="4" EnableSortingAndPagingCallbacks="True" EnableViewState="false" 
                            Font-Names="Calibri" Font-Size="10pt" ForeColor="#333333" GridLines="Both" HorizontalAlign="Left" style="margin-left: 0px" Width="100%">
                            <PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" />
                            <FooterStyle BackColor="#5D7B9D" Font-Size="10pt" ForeColor="White" Font-Bold="True" />
                            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" HorizontalAlign="Left" />
                                <Columns>                                    
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="LnkBtnDocument" runat="server" Text="View" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                 
                                    <asp:TemplateField HeaderText="Document URL" ItemStyle-Width="750px" >
                                        <ItemTemplate>
                                            <asp:TextBox ID="TxtURL" runat="server" Text = '<%# Eval("URL") %>' Width="98%" CssClass="TextBox" MaxLength="255" Enabled ="false" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                  
                                </Columns>
                                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                <SelectedRowStyle BackColor="#CC3333" Font-Bold="False" ForeColor="White" />
                                <HeaderStyle BackColor="#5D7B9D" Font-Size="8pt" ForeColor="White" HorizontalAlign="Left"
                                    />
                                <EditRowStyle BackColor="#999999" />
                                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                            </asp:GridView>
                        </td>                        
                    </tr>
                </table>    
            </asp:Panel>
        </ContentTemplate>
        
    </asp:UpdatePanel>
    <ajaxToolkit:UpdatePanelAnimationExtender ID="upae" BehaviorID="Animation1" runat="server" 
        TargetControlID="UP_DataGrid">
        <Animations>
            <OnUpdated>
                <Parallel duration="0">
                    <ScriptAction Script="Updated();" /> 
                </Parallel> 
            </OnUpdated>
        </Animations>
    </ajaxToolkit:UpdatePanelAnimationExtender>     
    
    
            
</asp:Content>
