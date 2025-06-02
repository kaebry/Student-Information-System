<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ManageStudents.aspx.vb" Inherits="StudentInformationSystem.ManageStudents" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="text-center mb-4">Manage Students</h2>

    <div class="row mb-3">
        <div class="col-md-3">
            <asp:Label AssociatedControlID="txtFirstName" runat="server" Text="First Name:" />
            <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" placeholder="Enter First Name" />
        </div>
        <div class="col-md-3">
            <asp:Label AssociatedControlID="txtLastName" runat="server" Text="Last Name:" />
            <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" placeholder="Enter Last Name" />
        </div>
        <div class="col-md-3">
            <asp:Label AssociatedControlID="txtEmail" runat="server" Text="Email:" />
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter Email" />
        </div>
        <div class="col-md-3">
            <asp:Label AssociatedControlID="txtEnrollmentDate" runat="server" Text="Enrollment Date:" />
            <asp:TextBox ID="txtEnrollmentDate" runat="server" TextMode="Date" CssClass="form-control" placeholder="dd/mm/yyyy" />
        </div>
    </div>

    <div class="mb-4">
        <asp:Button ID="btnAdd" runat="server" Text="Add Student" CssClass="btn btn-primary me-2" OnClick="btnCreate_Click" OnClientClick="return confirm('Are you sure you want to add this student?');" />
        <asp:Button ID="btnUpdate" runat="server" Text="Update Student" CssClass="btn btn-warning me-2" OnClick="btnUpdate_Click" Enabled="False" />
        <asp:Button ID="btnDelete" runat="server" Text="Delete Student" CssClass="btn btn-danger me-2" OnClick="btnDelete_Click" Enabled="False" OnClientClick="return confirm('Are you sure you want to delete this student?');"/>
        <asp:Button ID="btnClear" runat="server" Text="Clear Fields" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="alert mt-3 d-block" Visible="False" />

    <asp:GridView ID="gvStudents" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped"
        DataKeyNames="ID" OnSelectedIndexChanged="gvStudents_SelectedIndexChanged">
        <Columns>
            <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" />
            <asp:BoundField DataField="FirstName" HeaderText="First Name" />
            <asp:BoundField DataField="LastName" HeaderText="Last Name" />
            <asp:BoundField DataField="Email" HeaderText="Email" />
            <asp:BoundField DataField="EnrollmentDate" HeaderText="Enrollment Date" DataFormatString="{0:dd/MM/yyyy}" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="lnkSelect" runat="server" Text="Select" CommandName="Select" CssClass="btn btn-link" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
