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
    <title>DEVIN | Ремонты</title>
</HEAD>

<BODY>

	<%
		dim search : search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' placeholder='Поиск' value='" & search & "'/>" _
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

	<script src='/devin/content/lib/jquery-1.12.4.min.js'></script>

	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id="cart" class='cart-new'></div>

	<ul class='context-menu' id='main'>
		<li onclick='location="/devin/analyze/"'>Закупка картриджей</li>
		<li onclick='location="/devin/repair/report_year/"'>Годовой отчет по ремонтам</li>
		<li onclick='location="/devin/repair/cartridges_usage/"'>Годовой отчет по картриджам</li>
		<li onclick='groupCreate()'>Создать группу</li>
		<li onclick='writeoffCreate()'>Создать списание</li>
		<li onclick='writeoffSetup()'>Шаблоны списаний</li>
	</ul>
	<ul class='context-menu' id='writeoff'>
		<li onclick='writeoffOpen()'>Открыть карточку</li>
		<li onclick='writeoffPrint()'>Печать</li>
		<li onclick='groupBeforeMove()'>Переместить</li>
		<li onclick='offMenuRepairs()'>Отметить все ремонты как списаные</li>
		<li onclick='onMenuRepairs()'>Отметить все ремонты как активные</li>
		<li onclick='deleteMenuRepairs()'>Отменить все ремонты</li>
		<li onclick='writeoffDelete()'>Удалить списание</li>
	</ul>
	<ul class='context-menu' id='group'>
		<li onclick='groupEdit()'>Переименовать</li>
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
	<script src="/devin/content/js/core.js"></script>
	<script src='/devin/content/js/repair.js?v=1'></script>

</BODY>

</HTML>