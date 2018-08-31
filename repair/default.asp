<!-- #include virtual ="/devin/core/core.inc" -->
<!DOCTYPE html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<link href="/devin/content/lib/jquery-ui.min.css" rel="stylesheet" />
	<link href="/devin/content/css/core.css" rel="stylesheet" type="text/css" />
	<link href="/devin/content/css/repair.css" rel="stylesheet" type="text/css" />
	<link href="/devin/content/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <title>DEVIN | �������</title>
</HEAD>

<BODY>

	<%
		dim search : search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' placeholder='�����' value='" & search & "'/>" _
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

	<script src='/devin/content/lib/jquery-1.12.4.min.js'></script>

	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id="cart" class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='location="/devin/analyze/"'>������� ����������</li>
		<li onclick='location="/devin/repair/report_year/"'>������� ����� �� ��������</li>
		<li onclick='location="/devin/repair/cartridges_usage/"'>������� ����� �� ����������</li>
		<li onclick='groupCreate()'>������� ������</li>
		<li onclick='writeoffCreate()'>������� ��������</li>
		<li onclick='writeoffSetup()'>������� ��������</li>
	</ul>
	<ul class='context-menu' id='writeoff'>
		<li onclick='writeoffOpen()'>������� ��������</li>
		<li onclick='writeoffPrint()'>������</li>
		<li onclick='groupBeforeMove()'>�����������</li>
		<li onclick='offMenuRepairs()'>�������� ��� ������� ��� ��������</li>
		<li onclick='onMenuRepairs()'>�������� ��� ������� ��� ��������</li>
		<li onclick='deleteMenuRepairs()'>�������� ��� �������</li>
		<li onclick='writeoffDelete()'>������� ��������</li>
	</ul>
	<ul class='context-menu' id='group'>
		<li onclick='groupEdit()'>�������������</li>
		<li onclick='groupBeforeMove()'>�����������</li>
		<li onclick='groupCreateInner()'>������� ��������� ������</li>
		<li onclick='groupErase()'>��������</li>
		<li onclick='groupDelete()'>�������</li>
	</ul>
	<ul class='context-menu' id='modal'>
		<li></li>
		<li>��</li>
		<li onclick='$(this).parent().fadeOut(100)'>������</li>
	</ul>

	<script src="/devin/content/lib/jquery-ui.min.js"></script>
	<script src="/devin/content/js/core.js"></script>
	<script src='/devin/content/js/repair.js?v=1'></script>

</BODY>

</HTML>