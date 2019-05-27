<%@ Control Language="C#" AutoEventWireup="true" ClientIDMode="AutoID" CodeBehind="ModalBootstrapDialog.ascx.cs" Inherits="JohamWeb.Controls.Common.ModalBootstrapDialog" %>
<%@ Import Namespace="JohamWeb.Controls" %>
<asp:Literal ID="ltrModalTag" runat="server" Text="<div class='modal fade' id='{0}' role='dialog' aria-labelledby='myModalLabel' aria-hidden='true'>" />
    <asp:Literal ID="ltrDialogTag" runat="server" Text="<div class='modal-dialog {0}'>" />

        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                <h4 class="modal-title">
            <asp:UpdatePanel ID="pnlItemHeader" runat="server" 
                UpdateMode="Conditional"
                RenderMode="Inline"
                OnInit="pnlItemHeader_Init">
            </asp:UpdatePanel>
                </h4>
            </div>
            <asp:UpdatePanel ID="pnlItemBody" runat="server" 
                UpdateMode="Conditional" 
                RenderMode="Block"
                class="modal-body" 
                OnInit="pnlItemBody_Init">
            </asp:UpdatePanel>
            <asp:UpdatePanel ID="pnlItemFooter" runat="server" 
                UpdateMode="Conditional" 
                RenderMode="Block"
                class="modal-footer" 
                OnInit="pnlItemFooter_Init">
            </asp:UpdatePanel>
        </div>
    <asp:Literal runat="server" Text="</div>" />
<asp:Literal runat="server" Text="</div>" />
