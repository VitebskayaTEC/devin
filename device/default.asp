<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<html>

<head>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/devin/content/lib/jquery-ui.min.css" rel="stylesheet" />
	<link href="/devin/content/css/core.css" rel="stylesheet" />
	<link href="/devin/content/css/device.css" rel="stylesheet" />
	<link href="/devin/content/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | ����������</title>
</head>

<body>

	<%
		dim search: search = request.querystring("search")
		menu("<li><form method='get' action='./'><input name='search' id='search' placeholder='�����' value='" & search & "'/></form>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")
	%>

	<div id='selected' class='panel'>
		<div>������� ���������: <b></b></div>
		<div><div id='move_select'><select id='moveKey'></select></div><a onclick='moveSelectedDevices()'>�����������</a></div>
		<div><a onclick='removeAllSelection()'>�������� �����</a></div>
	</div>

	<div id="excl" class='panel'>
		<form method="POST" enctype="multipart/form-data" action="/devin/asp/device_1c_compare.asp?typeof=0">
			<select onchange="parentNode.action='/devin/asp/device_1c_compare.asp?typeof='+this.value;" name='typeof'>
				<option value='0'>����������</option>
				<option value='1'>��������</option>
				<option value='2'>��������</option>
				<option value='3'>������</option>
				<option value='4'>�������</option>
				<option value='5'>���</option>
				<option value='6'>������</option>
			</select>
			<input type='file' name="FILE1" />
            <input type='submit' value="���������" />
		</form>
	</div>

	<script src='/devin/content/lib/jquery-1.12.4.min.js'></script>

	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id='cart' class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='cartCreate()'>������� �������� ����������</li>
		<li onclick='groupCreate()'>������� ������</li>
        <li class="passive"><hr /></li>
		<li onclick='history()'>������� ���� ������� DEVIN</li>
        <li class="passive"><hr /></li>
		<li onclick='location="/devin/analyze/"'>������ ������� ����������</li>
        <li class="passive"><hr /></li>
        <li onclick="document.location = '/devin/devices/table';">��������� �������� Devin</li>
        <li onclick="document.location = '/devin/devices/table1C';">�������� ������ 1�</li>
        <li onclick="document.location = '/devin/devices/compare1C';">������ � 1�</li>
	</ul>
	<ul class='context-menu' id='computer'>
		<li onclick='computerOpen()'>������� ��������</li>
		<li onclick='computerAida()'>����� �� AIDA</li>
		<li onclick='groupBeforeMove()'>�����������</li>
		<li onclick='computerDelete()'>�������</li>
	</ul>
	<ul class='context-menu' id='group'>
		<li onclick='groupEdit()'>�������������
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
	<script src='/devin/content/js/core.js'></script>
	<script src='/devin/content/js/device.js?v=2'></script>
	<script>
		var sortKey = "<%=request.queryString("key")%>",
			sortDesc = "<%=request.queryString("desc")%>",
			sortText = "<%=request.queryString("search")%>";

		$(".view")
			.on("mousedown", ".unit:not(#solo) .caption th,.unit:not(#solo) .caption td:not(:first-child)", function() { toggle(this) })
			.on("mousedown", ".items tbody td:not(:first-child), .title", function() { cartOpen(this); })
			.on("mousedown", ".main th", function() { sort(this) })
			.on("mousedown", ".items th", function() { _sort(this) })
		$("#cart")
			.on("submit", "form", function() { event.preventDefault() })
	</script>


</body>

</html>