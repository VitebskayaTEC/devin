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
	<title>DEVIN | Устройства</title>
</head>

<body>

	<%
		dim search: search = request.querystring("search")
		menu("<li><form method='get' action='./'><input name='search' id='search' placeholder='Поиск' value='" & search & "'/></form>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")
	%>

	<div id='selected' class='panel'>
		<div>Выбрано элементов: <b></b></div>
		<div><div id='move_select'><select id='moveKey'></select></div><a onclick='moveSelectedDevices()'>Переместить</a></div>
		<div><a onclick='removeAllSelection()'>Сбросить выбор</a></div>
	</div>

	<div id="excl" class='panel'>
		<form method="POST" enctype="multipart/form-data" action="/devin/asp/device_1c_compare.asp?typeof=0">
			<select onchange="parentNode.action='/devin/asp/device_1c_compare.asp?typeof='+this.value;" name='typeof'>
				<option value='0'>Компьютеры</option>
				<option value='1'>Принтеры</option>
				<option value='2'>Мониторы</option>
				<option value='3'>Модемы</option>
				<option value='4'>Сканеры</option>
				<option value='5'>ИБП</option>
				<option value='6'>Другое</option>
			</select>
			<input type='file' name="FILE1" />
            <input type='submit' value="Загрузить" />
		</form>
	</div>

	<script src='/devin/content/lib/jquery-1.12.4.min.js'></script>

	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id='cart' class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='cartCreate()'>Создать карточку устройства</li>
		<li onclick='groupCreate()'>Создать группу</li>
        <li class="passive"><hr /></li>
		<li onclick='history()'>История всех событий DEVIN</li>
        <li class="passive"><hr /></li>
		<li onclick='location="/devin/analyze/"'>Анализ расхода картриджей</li>
        <li class="passive"><hr /></li>
        <li onclick="document.location = '/devin/devices/table';">Табличный просмотр Devin</li>
        <li onclick="document.location = '/devin/devices/table1C';">Просмотр данных 1С</li>
        <li onclick="document.location = '/devin/devices/compare1C';">Сверка с 1С</li>
	</ul>
	<ul class='context-menu' id='computer'>
		<li onclick='computerOpen()'>Открыть карточку</li>
		<li onclick='computerAida()'>Отчет из AIDA</li>
		<li onclick='groupBeforeMove()'>Переместить</li>
		<li onclick='computerDelete()'>Удалить</li>
	</ul>
	<ul class='context-menu' id='group'>
		<li onclick='groupEdit()'>Переименовать
		<li onclick='groupBeforeMove()'>Переместить</li>
		<li onclick='groupCreateInner()'>Создать вложенную группу</li>
		<li onclick='groupErase()'>Очистить</li>
		<li onclick='groupDelete()'>Удалить</li>
	</ul>
	<ul class='context-menu' id='modal'>
		<li></li>
		<li>Ок</li>
		<li onclick='$(this).parent().fadeOut(100)'>Отмена</li>
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