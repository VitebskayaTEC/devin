<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/storage.css" rel="stylesheet" />
	<link href="/devin/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | Приход принтеров</title>
</HEAD>

<BODY>

	<% menu("") %>

	<div id='selected' class='panel'>
		<div>Выбрано позиций: <b></b></div>
		<div><a onclick='excelExports()'>Распечатать бирки</a></div>
		<div><a onclick='removeAllSelection()'>Сбросить выбор</a></div>
	</div>

	<div id='excelExports' class='panel'>
		<div id='excelExportsLink'></div>
		<div><a onclick='closeExportsPanel()'>Закрыть</a></div>
	</div>

	<div class='view' id='view'>
		<% server.execute "view.asp" %>
	</div>

	<%
		if false then
			dim excelApp   : set excelApp   = server.createobject("Excel.Application")
			excelApp.application.enableEvents  = false
			excelApp.application.displayAlerts = false
			dim excelBook  : set excelBook  = excelApp.workbooks.open("D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\exl\labels.xls")
			dim excelCells : set excelCells = excelBook.activesheet.cells

			excelBook.close
			set excelCells = nothing
			set excelBook  = nothing
			set excelApp   = nothing
		end if
	%>

	<div id='cart' class='cart-new'></div>

	<script src="/cdn/jquery-1.12.4.min.js"></script>
	<script src="/cdn/jquery-ui.min.js"></script>
	<script src="/devin/js/core.js"></script>
	<script src="/devin/js/storage.js"></script>
	<script>
		$(".view")
			.on("mousedown", ".items thead th:not(:first-child)", function() { _sort(this) })
			.on("mousedown", ".items tbody td", function() { cartOpen(this) })
	</script>

</BODY>

</HTML>