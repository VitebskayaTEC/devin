<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/jquery-ui.min.css" rel="stylesheet" type="text/css" />
	<link href="/devin/css/aida.css" rel="stylesheet" />
	<link href="/devin/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | AIDA64</title>
</HEAD>

<BODY>

	<% 
		dim search: search = DecodeUTF8(request.querystring("text"))
		menu("<li><input onkeyup='search(this)' def='Поиск' class='def' value='" & search & "'/>" _
		& "<li><a class='has-icon' onmousedown='_menu(this)' menu='main'><div class='icon ic-menu'></div></a>")
	%>
	<div id='view' class='view'><% server.execute "view.asp" %></div>

	<div id='cart' class='cart-new'></div>

	<script src="/devin/js/jquery-1.12.4.min.js"></script>
	<script src="/devin/js/jquery-ui.min.js"></script>
	<script src='/devin/js/core.js'></script>
	<script src='/devin/js/aida.js'></script>
	<script>
		$(".view")
			.on("mousedown", ".caption", function() { toggle(this) })
			.on("mousedown", ".title", function() { cartOpen(this) })
	</script>
	
</BODY>

</HTML>