<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<html>

<head>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/cdn/jquery-ui.min.css" rel="stylesheet" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/device.css" rel="stylesheet" />
	<link href="/devin/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | Устройства</title>
</head>

<body>

	<%
		dim search: search = DecodeUTF8(request.querystring("text"))
		menu("<li><input def='Поиск' class='def' value='" & search & "'/>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")
		'onkeyup='search(this)'
	%>

	<div id='selected' class='panel'>
		<div>Выбрано элементов: <b></b></div>
		<div><div id='move_select'><select id='moveKey'></select></div><a onclick='moveSelectedDevices()'>Переместить</a></div>
		<div><a onclick='removeAllSelection()'>Сбросить выбор</a></div>
	</div>

	<div id="excl" class='panel'>
		<form method="POST" enctype="multipart/form-data" action="/devin/views/device_1c_compare.asp?typeof=0">
			<select onchange="parentNode.action='/devin/views/device_1c_compare.asp?typeof='+this.value;" name='typeof'>
				<option value='0'>Компьютеры
				<option value='1'>Принтеры
				<option value='2'>Мониторы
				<option value='3'>Модемы
				<option value='4'>Сканеры
				<option value='5'>ИБП
				<option value='6'>Другое
			</select>
			<input type='file' size='50' name="FILE1" /><input actions='openCartridgeCart' type='submit' value="Загрузить" />
		</form>
	</div>

	<script src='/cdn/jquery-1.12.4.min.js'></script>

	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id='cart' class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='cartCreate()'>Создать карточку устройства
		<li onclick='groupCreate()'>Создать группу
		<li onclick='history()'>История всех событий DEVIN
		<li onclick='location="/devin/analyze/"'>Анализ расхода картриджей
		<li onclick='loadCompare()'>Сверка с 1С
	</ul>
	<ul class='context-menu' id='computer'>
		<li onclick='computerOpen()'>Открыть карточку
		<li onclick='computerAida()'>Отчет из AIDA
		<li onclick='groupBeforeMove()'>Переместить
		<li onclick='computerDelete()'>Удалить
	</ul>
	<ul class='context-menu' id='group'>
		<li onclick='groupEdit()'>Переименовать
		<li onclick='groupBeforeMove()'>Переместить
		<li onclick='groupCreateInner()'>Создать вложенную группу
		<li onclick='groupErase()'>Очистить
		<li onclick='groupDelete()'>Удалить
	</ul>
	<ul class='context-menu' id='modal'>
		<li>
		<li>Ок
		<li onclick='$(this).parent().fadeOut(100)'>Отмена
	</ul>

	<script src="/cdn/jquery-ui.min.js"></script>
	<script src='/devin/js/core.js'></script>
	<script src='/devin/js/device.js'></script>
	<script>
		var sortKey = "<%=request.queryString("key")%>",
			sortDesc = "<%=request.queryString("desc")%>",
			sortText = "<%=request.queryString("text")%>";

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