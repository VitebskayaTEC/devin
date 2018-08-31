<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/devin/content/lib/jquery-ui.min.css" rel="stylesheet" type="text/css" />
	<link href="/devin/content/css/core.css" rel="stylesheet" />
	<link href="/devin/content/css/aida.css" rel="stylesheet" />
	<link href="/devin/content/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | AIDA64</title>
</HEAD>

<BODY>

	<%
		dim search: search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' placeholder='Поиск' value='" & search & "'/>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")
	%>
	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id='cart' class='cart-new'></div>

	<script src="/devin/content/lib/jquery-1.12.4.min.js"></script>
	<script src="/devin/content/lib/jquery-ui.min.js"></script>
	<script src='/devin/content/js/core.js'></script>
	<script src='/devin/content/js/aida.js'></script>
	<script>
		$(".view")
			.on("mousedown", ".caption", function() { toggle(this) })
			.on("mousedown", ".title", function() { cartOpen(this) })
	</script>

</BODY>

</HTML>