<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs : set rs = Server.CreateObject("ADODB.Recordset")
	dim sql

	dim id : id = request.querystring("id")
	dim only : only = Request.QueryString("only")
	dim classname : classname = Request.QueryString("classname")
	if only = "" or isnull(only) or only = "1" then only = true else only = false
	if classname = "" or isnull(classname)  then classname = "ALL"
	
	' Составление sql запроса
	sql = "SELECT SKLAD.NCard AS Count, SKLAD.Nis, SKLAD.NCard AS ID, SKLAD.NCard, SKLAD.class_name, SKLAD.Name, SKLAD.uchet, SKLAD.NCard AS V "
	sql = sql & "FROM SKLAD "
	if only then	
		sql = sql & "INNER JOIN CARTRIDGE ON SKLAD.ID_cart = CARTRIDGE.N "
		sql = sql & "INNER JOIN OFFICE ON CARTRIDGE.N = OFFICE.Cartridge "
		sql = sql & "INNER JOIN PRINTER ON OFFICE.Printer = PRINTER.N "
		sql = sql & "RIGHT OUTER JOIN DEVICE ON PRINTER.N = DEVICE.ID_prn "
		sql = sql & "WHERE (DEVICE.number_device = '" & id & "') AND (SKLAD.Nis > 0) AND (SKLAD.delit = 1) "
	else
		sql = sql & "WHERE (SKLAD.Nis > 0) AND (SKLAD.delit = 1) "
	end if
	if classname <> "ALL" then sql = sql & "AND (SKLAD.class_name = '" & classname & "') "
	sql = sql & "ORDER BY SKLAD.class_name, SKLAD.NCard, SKLAD.name"

	' Получение списка деталей 
	conn.open everest
	rs.open sql, conn

	if rs.eof then
		response.write "Доступных материалов нет!"
	else 
		response.write "<table><thead><tr><th><th><th width='84px'>Инв. №<th>Наименование<th width='75px'>Счет учета<th width='35px'>Вирт.</tr></thead><tbody>"
		do while not rs.eof
			response.write "<tr><td><input type='number' name='" & rs("Count") & "count' value='0' onkeyup='setPosition(this)' onchange='setPosition(this)' /><td>" & rs("Nis") & "<td onclick='togglePosition(this)'><input name='" & rs("ID") & "' type='checkbox' />" & rs("NCard") & "<td onclick='togglePosition(this)'>" & rs("Name") & "<td onclick='togglePosition(this)'>" & rs("uchet") & "<td><input type='checkbox' name='" & rs("V") & "virtual' /></tr>"
			rs.movenext
		loop
		response.write "</tbody></table>"
	end if

	rs.close
	set rs = nothing
	conn.close
	set conn = nothing
%>