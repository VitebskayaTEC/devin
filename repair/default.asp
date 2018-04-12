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
    <title>DEVIN | Ремонты</title>
</HEAD>

<BODY>

	<%
		dim search : search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' def='Поиск' class='def' value='" & search & "'/>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")%>

	<div id='selected' class='panel'>
		<div>Выбрано элементов: <b></b></div>
		<div><div id='move_select'><select id='moveKey'></select></div><a onclick='moveSelectedRepairs()'>Переместить</a></div>
		<div><a actions='offSelectedRepairs' onclick='offSelectedRepairs()'>Отметить все ремонты как списанные</a></div>
		<div><a actions='onSelectedRepairs' onclick='onSelectedRepairs()'>Отметить все ремонты как активные</a></div>
		<div><a actions='deleteSelectedRepairs' onclick='deleteSelectedRepairs()'>Отменить все ремонты</a></div>
		<div><a onclick='removeAllSelection()'>Сбросить выбор</a></div>
	</div>
	<div id='excelExports' class='panel' onclick='closeExportsPanel()'>
		<div id='excelExportsLink' ></div>
		<div><a>Закрыть</a></div>
	</div>

	<script src='/cdn/jquery-1.12.4.min.js'></script>

	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id="cart" class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='location="/devin/analyze/"'>Расход картриджей
		<li onclick='location="/devin/repair/report_year/"'>Годовой отчет по ремонтам
		<li onclick='groupCreate()'>Создать группу
		<li onclick='writeoffCreate()'>Создать списание
		<li onclick='writeoffSetup()'>Шаблоны списаний
	</ul>
	<ul class='context-menu' id='writeoff'>
		<li onclick='writeoffOpen()'>Открыть карточку
		<li onclick='writeoffPrint()'>Печать
		<li onclick='groupBeforeMove()'>Переместить
		<li onclick='offMenuRepairs()'>Отметить все ремонты как списаные
		<li onclick='onMenuRepairs()'>Отметить все ремонты как активные
		<li onclick='deleteMenuRepairs()'>Отменить все ремонты
		<li onclick='writeoffDelete()'>Удалить списание
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
	<script src='/devin/js/repair.js?v=1'></script>

</BODY>

</HTML>