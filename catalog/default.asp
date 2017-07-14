<!-- #include virtual ="/devin/core/core.inc" -->
<!doctype html>
<html>

<head>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/jquery-ui.min.css" rel="stylesheet" type="text/css" />
	<link href="/devin/css/catalog.css" rel="stylesheet" />
	<link href="/devin/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | Каталог</title>
</head>

<body>

<% 
	menu("<li><a href='/devin/analyze/'>Расход картриджей</a>" _
		& "<li><a onclick='createPrinter()' actions='createPrinter'>Создать принтер</a>" _
		& "<li><a onclick='createCartridge()' actions='createCartridge'>Создать картридж</a>")

	dim conn : 	set conn = server.createObject("ADODB.Connection")		
	dim rs : 	set rs = server.createObject("ADODB.Recordset")
	conn.open everest

	response.write "<div class='view'><table><tr><td><div class='unit'><table class='caption'><tr><th>Принтеры</tr></table>"	
		rs.open "SELECT N, Caption FROM PRINTER ORDER BY Caption", conn
		if not rs.eof then
			response.write "<table class='items'><thead><tr><th><input class='def' def='найти...' onkeyup='_search(this)' /></tr></thead><tbody>"
			do while not rs.eof
				response.write "<tr><td actions='openPrinterCart' id='prn" & rs(0) & "'>" & rs(1) & "</tr>"
				rs.moveNext
			loop
			response.write "</tbody></table>"
		else
			response.write "<div>Данные не получены.</div>"
		end if
		rs.close
	response.write "</div><td width='50%'><div class='unit'><table class='caption'><tr><th>Картриджи</tr></table>"
		rs.open "SELECT N, Caption FROM CARTRIDGE ORDER BY Caption", conn
		if not rs.eof then
			response.write "<table class='items'><thead><tr><th><input class='def' def='найти...' onkeyup='_search(this)' /></tr></thead><tbody>"
			do while not rs.eof
				response.write "<tr><td actions='openCartridgeCart' id='cart" & rs(0) & "'>" & rs(1) & "</tr>"
				rs.moveNext
			loop
			response.write "</tbody></table>"
		else
			response.write "<div>Данные не получены.</div>"
		end if
		rs.close
	response.write "</div></tr></table></div>"
	conn.close
	set rs = nothing
	set conn = nothing
%>
	<div id="cart" class='cart-new'></div>

	<script src='/devin/js/jquery-1.12.4.min.js'></script>
	<script src="/devin/js/jquery-ui.min.js"></script>
	<script src='/devin/js/core.js'></script>
	<script src='/devin/js/catalog.js'></script>
	<script>
		$(".unit").on("mousedown", ".items td", function() { cartOpen(this); })
	</script>

</body>

</html>