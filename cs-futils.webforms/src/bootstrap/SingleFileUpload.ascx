<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SingleFileUpload.ascx.cs" Inherits="JohamWeb.Controls.Common.SingleFileUpload" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxTK" %>
<asp:UpdatePanel runat="server" ID="pnlFileUpload1" RenderMode="Block" UpdateMode="Conditional">
    <ContentTemplate>
        <AjaxTK:AsyncFileUpload ID="AsyncFileUpload1" runat="server" ClientIDMode="AutoID" 
            PersistFile="true"
            UploaderStyle="Traditional" 
            UploadingBackColor="Yellow" 
            CompleteBackColor="Lime" 
            ErrorBackColor="Red" 
            Width="100%" CssClass="form-control"  
            OnClientUploadStarted="SingleFileUpload_uploadStarted"
            OnClientUploadComplete="SingleFileUpload_uploadComplete" />
        <asp:Panel runat="server" ID="pnlFileList" CssClass="form-group input-group-sm">
            <asp:Button ID="txtFileName1" runat="server" 
                Text="Verwijderen" 
                CssClass="form-control" 
                CausesValidation="false" 
                UseSubmitBehavior="false" 
                OnClick="txtFileName1_Click" 
                title="Verwijderen" />
        </asp:Panel>
        <asp:RequiredFieldValidator runat="server" ID="AsyncFileUpload1Validator" 
            ControlToValidate="txtFileUploadHelper" 
            ErrorMessage="U moet een bestand opgeven!" 
            Display="Dynamic" SetFocusOnError="false" CssClass="alert-text" />
        <asp:TextBox runat="server" ID="txtFileUploadHelper" style="display:none" MaxLength="0" />
    </ContentTemplate>
</asp:UpdatePanel>
