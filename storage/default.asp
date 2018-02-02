<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<link href="/cdn/jquery-ui.min.css" rel="stylesheet" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/storage.css" rel="stylesheet" />
	<link href="/devin/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | Склад</title>
</HEAD>

<BODY>

	<%
		dim search: search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' def='Поиск' class='def' value='" & search & "'/>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")
	%>

	<div id="excl" class="panel">
		<form id='compare-form' method="POST" enctype="multipart/form-data" action="/devin/views/storage_1c_compare.asp">
    	<table>
			<tr><th>Импорт данных из файла</tr>
			<tr><td><input type="file" name="FILE1" /></tr>
			<tr><td>
				<select name="saveto" onchange="compareSetSource(this)">
					<option value="compare">Выполнить сверку
					<option value="in">Загрузить приход
					<!--<option value="out">Загрузить остатки-->
				</select></tr>
			<tr><td>
				<input type="submit" value="Загрузить" />
				<input type="button" value="Закрыть" onclick="compare()" />
			</tr>
		</table>
		</form>
	</div>

	<div id='selected' class='panel'>
		<div>Выбрано позиций: <b></b></div>
		<div><a onclick='allStoragesToRepair()'>Оформить ремонты</a></div>
		<div><div id='move_select'></div><a onclick='storagesToGroup()'>Переместить в группу</a></div>
		<div><a onclick='excelExports()'>Распечатать бирки</a></div>
		<div><a onclick='removeAllSelection()'>Сбросить выбор</a></div>
	</div>

	<div id='excelExports' class='panel'>
		<div id='excelExportsLink'></div>
		<div><a onclick='closeExportsPanel()'>Закрыть</a></div>
	</div>

	<script src="/cdn/jquery-1.12.4.min.js"></script>

	<div class='view' id='view'>
		<% server.execute "view.asp" %>
	</div>

	<div id='cart' class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='compare()'>Импорт из 1C
		<li onclick='groupCreate()'>Создать группу
		<li onclick='cartCreate()'>Создать позицию на складе
		<li onclick='location="/devin/storage/labels/"'>Просмотр прихода картриджей
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
	<script src="/devin/js/core.js"></script>
	<script src="/devin/js/storage.js"></script>
	<script>
		var sortText = "<%=search%>";
		$(".view")
			.on("mousedown", ".unit:not(#solo) .caption th", function() { toggle(this) })
			.on("mousedown", ".items thead th:not(:first-child)", function() { _sort(this) })
			.on("mousedown", ".items tbody td", function() { cartOpen(this) })
	</script>

</BODY>

</HTML>