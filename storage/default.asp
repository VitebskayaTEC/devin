<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<link href="/devin/content/lib/jquery-ui.min.css" rel="stylesheet" />
	<link href="/devin/content/css/core.css" rel="stylesheet" />
	<link href="/devin/content/css/storage.css" rel="stylesheet" />
	<link href="/devin/content/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | �����</title>
</HEAD>

<BODY>

	<%
		dim search: search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' placeholder='�����' value='" & search & "'/>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")
	%>

	<div id="excl" class="panel">
		<form id='compare-form' method="POST" enctype="multipart/form-data" action="/devin/storages/import">
    	    <b>��������� �������� �� 1�</b>
			<div>
                <input type="file" name="excel" />
			</div>
			<div>
                <input type="submit" value="���������" />
				<input type="button" value="�������" onclick="compare()" />
			</div>
		</form>
	</div>

	<div id='selected' class='panel'>
		<div>������� �������: <b></b></div>
		<div><a onclick='allStoragesToRepair()'>�������� �������</a></div>
		<div><div id='move_select'></div><a onclick='storagesToGroup()'>����������� � ������</a></div>
		<div><a onclick='excelExports()'>����������� �����</a></div>
		<div><a onclick='removeAllSelection()'>�������� �����</a></div>
	</div>

	<div id='excelExports' class='panel'>
		<div id='excelExportsLink'></div>
		<div><a onclick='closeExportsPanel()'>�������</a></div>
	</div>

	<script src="/devin/content/lib/jquery-3.3.1.min.js"></script>

	<div class='view' id='view'>
		<% server.execute "view.asp" %>
	</div>

	<div id='cart' class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='compare()'>������ �� 1C
		<li onclick='groupCreate()'>������� ������
		<li onclick='cartCreate()'>������� ������� �� ������
		<li onclick='location="/devin/storage/labels/"'>�������� ������� ����������
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

    <script src="/devin/content/lib/jquery-ui.min.js"></script>
	<script src="/devin/content/js/core.js"></script>
	<script src="/devin/content/js/storage.js"></script>
	<script>
		var sortText = "<%=search%>";
		$(".view")
			.on("mousedown", ".unit:not(#solo) .caption th", function() { toggle(this) })
			.on("mousedown", ".items thead th:not(:first-child)", function() { _sort(this) })
			.on("mousedown", ".items tbody td", function() { cartOpen(this) })
	</script>

</BODY>

</HTML>