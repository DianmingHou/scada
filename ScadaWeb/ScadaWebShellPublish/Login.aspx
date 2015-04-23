<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Scada.Web.WFrmLogin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rapid SCADA - Вход в систему</title>
</head>
<body>
    <form id="LoginForm" runat="server">
        <div>
            <asp:Label ID="lblErrMsg" runat="server" Text="Ошибка"></asp:Label>
        </div>
        <div>
            <asp:Label ID="lblLogin" runat="server" Text="Имя пользователя"></asp:Label>
        </div>
        <div>
            <asp:TextBox ID="txtLogin" runat="server" MaxLength="50"></asp:TextBox>
        </div>
        <div>
            <asp:Label ID="lblPassword" runat="server" Text="Пароль"></asp:Label>
        </div>
        <div>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" MaxLength="20"></asp:TextBox>
        </div>
        <div>
            <asp:CheckBox ID="chkRememberUser" runat="server" Text="Запомнить меня" /><asp:Button ID="btnLogin" runat="server" Text="Войти" OnClick="btnLogin_Click" />
        </div>
    </form>
</body>
</html>
