<!-- #include virtual ="/devin/core/core.inc" -->
<!DOCTYPE html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<link href="/cdn/jquery-ui.min.css" rel="stylesheet" />
	<link href="/devin/css/core.css" rel="stylesheet" type="text/css" />
	<link href="/devin/css/repair.css" rel="stylesheet" type="text/css" />
	<link href="/devin/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <title>DEVIN | �������</title>
</HEAD>

<BODY>

	<%
		dim search : search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' def='�����' class='def' value='" & search & "'/>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")%>

	<div id='selected' class='panel'>
		<div>������� ���������: <b></b></div>
		<div><div id='move_select'><select id='moveKey'></select></div><a onclick='moveSelectedRepairs()'>�����������</a></div>
		<div><a actions='offSelectedRepairs' onclick='offSelectedRepairs()'>�������� ��� ������� ��� ���������</a></div>
		<div><a actions='onSelectedRepairs' onclick='onSelectedRepairs()'>�������� ��� ������� ��� ��������</a></div>
		<div><a actions='deleteSelectedRepairs' onclick='deleteSelectedRepairs()'>�������� ��� �������</a></div>
		<div><a onclick='removeAllSelection()'>�������� �����</a></div>
	</div>
	<div id='excelExports' class='panel' onclick='closeExportsPanel()'>
		<div id='excelExportsLink' ></div>
		<div><a>�������</a></div>
	</div>

	<script src='/cdn/jquery-1.12.4.min.js'></script>

	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id="cart" class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='location="/devin/analyze/"'>������ ����������
		<li onclick='location="/devin/repair/report_year/"'>������� ����� �� ��������
		<li onclick='groupCreate()'>������� ������
		<li onclick='writeoffCreate()'>������� ��������
		<li onclick='writeoffSetup()'>������� ��������
	</ul>
	<ul class='context-menu' id='writeoff'>
		<li onclick='writeoffOpen()'>������� ��������
		<li onclick='writeoffPrint()'>������
		<li onclick='groupBeforeMove()'>�����������
		<li onclick='offMenuRepairs()'>�������� ��� ������� ��� ��������
		<li onclick='onMenuRepairs()'>�������� ��� ������� ��� ��������
		<li onclick='deleteMenuRepairs()'>�������� ��� �������
		<li onclick='writeoffDelete()'>������� ��������
	</ul>
	<ul class='context-menu' id='group'>
		<li onclick='groupEdit()'>�������������
		<li onclick='groupBeforeMove()'>�����������
		<li onclick='groupCreateInner()'>������� ��������� ������
		<li onclick='groupErase()'>��������
		<li onclick='groupDelete()'>�������
	</ul>
	<ul class='context-menu' id='modal'>
		<li>
		<li>��
		<li onclick='$(this).parent().fadeOut(100)'>������
	</ul>

	<script src="/cdn/jquery-ui.min.js"></script>
	<script src="/devin/js/core.js"></script>
	<script src='/devin/js/repair.js?v=1'></script>

</BODY>

</HTML>