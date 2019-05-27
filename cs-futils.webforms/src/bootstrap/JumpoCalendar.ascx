<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="JumpoCalendar.ascx.cs" Inherits="JohamWeb.Controls.Common.JumpoCalendar" %>
<%@ Import Namespace="JohamWeb.Controls" %>
<CalendarSingleDate ID="cdrSingleDate" CssClass="table table-bordered" runat="server"
    SelectionMode="Day" 
    OnDayRender="cdrSingleDate_DayRender" 
    TitleStyle-BackColor="#8195BA" 
    DayHeaderStyle-BackColor="#D2E0F2" 
    SelectedDayStyle-BackColor="#81BA95">
        <SelectedDayStyle />
</CalendarSingleDate>
<asp:CustomValidator ID="cdrSingleDateValidator" runat="server" ValidateEmptyText="true"
     ControlToValidate="cdrSingleDate"
    ClientValidationFunction="JumboCalendar_ClientValidate" EnableClientScript="true"  
    OnServerValidate="cdrSingleDateValidator_Validate" 
    ErrorMessage="Geen geldige datum geselecteerd!" 
    Display="Dynamic" SetFocusOnError="false" CssClass="alert-text" />

