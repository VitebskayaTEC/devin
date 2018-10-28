<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs : set rs = Server.CreateObject("ADODB.Recordset")
	dim sql

	dim id : id = request.querystring("id")
	dim only : only = Request.QueryString("only")
	dim group : group = Request.QueryString("gid")
	if only = "" or isnull(only) or only = "1" then only = true else only = false
	if group = "" or isnull(group)  then group = 0

	' Составление sql запроса
	sql = "SELECT Storages.Ncard AS Count, Storages.Nstorage, Storages.Ncard AS ID, Storages.Ncard, Storages.Type, Storages.Name, Storages.Account, Storages.Ncard AS V, Storages.Date "
	sql = sql & "FROM Storages "
	if only then
		sql = sql & "INNER JOIN Cartridges ON Storages.CartridgeId = Cartridges.Id "
		sql = sql & "INNER JOIN PrintersCartridges ON Cartridges.Id = PrintersCartridges.CartridgeId "
		sql = sql & "INNER JOIN Printers ON PrintersCartridges.PrinterId = Printers.Id "
		sql = sql & "RIGHT OUTER JOIN DEVICE ON PRINTER.N = DEVICE.ID_prn "
		sql = sql & "WHERE (DEVICE.number_device = '" & id & "') AND (Storages.Nstorage > 0) AND (Storages.IsDeleted = 0) "
	else
		sql = sql & "WHERE (Storages.Nstorage > 0) AND (Storages.IsDeleted = 0) "
	end if
	if group <> 0 then sql = sql & "AND (Storages.FolderId = " & group & ") "
	sql = sql & "ORDER BY Storages.Type, Storages.Account, Storages.Date ASC, Storages.Name"

	' Получение списка деталей
	conn.open everest
	rs.open sql, conn

	if rs.eof then
		response.write "Доступных материалов нет!"
	else
		response.write "<table><thead><tr><th><th><th width='84px'>Инв. №<th>Наименование<th width='75px'>Счет учета<th width='120px'>Дата<th width='35px'>Вирт.</tr></thead><tbody>"
		do while not rs.eof
			response.write "<tr><td><input type='number' name='" & rs("Count") & "count' value='0' onkeyup='setPosition(this)' onchange='setPosition(this)' /><td>" & rs("Nis") & "<td onclick='togglePosition(this)'><input name='" & rs("ID") & "' type='checkbox' />" & rs("NCard") & "<td onclick='togglePosition(this)'>" & rs("Name") & "<td onclick='togglePosition(this)'>" & rs("uchet") & "<td>" & rs("Date") & "<td><input type='checkbox' name='" & rs("V") & "virtual' /></tr>"
			rs.movenext
		loop
		response.write "</tbody></table>"
	end if

	rs.close
	set rs = nothing
	conn.close
	set conn = nothing
%>