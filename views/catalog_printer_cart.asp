<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")		
	dim rs 		: set rs = server.createObject("ADODB.Recordset")
	dim id 		: id = replace(request.queryString("id"), "prn", "")
	dim sql

	sub drop(str)
		on error resume next
		rs.close
		conn.close
		set rs = nothing
		set conn = nothing
		if str <> "" then response.write "<div class='error'>" & str & "</div>"
		response.end
	end sub

	conn.open everest

	' Получаем название принтера
	rs.open "SELECT Caption, Description FROM PRINTER WHERE N = " & id, conn
	if rs.eof then drop("Нет данных по данному ID")
	dim caption: caption = rs(0)
	response.write "<div class='cart-header'>" & caption & "</div>" _
		& "<div class='cart-overflow'><form id='form'><table class='cart-table'>" _
		& "<tr><td>Наименование<td><input name='Caption' value='" & caption & "' /></tr>" _
		& "<tr><td>Описание<td><textarea name='Description'>" & rs(1) & "</textarea></tr>" _
		& "</table></form>"
	rs.close

	' Связи
	response.write "<table class='cart-table catalog-compares'><tr><td><br /><b>Связи с типовыми картриждами:</b><td></tr>"
	
	sql = "SELECT CARTRIDGE.N, CARTRIDGE.Caption FROM OFFICE INNER JOIN PRINTER ON OFFICE.Printer = PRINTER.N INNER JOIN CARTRIDGE ON OFFICE.Cartridge = CARTRIDGE.N WHERE (PRINTER.N = " & id & ") ORDER BY CARTRIDGE.Caption"
	'response.write sql
	rs.open sql, conn
	if rs.eof then
		response.write "<tr><td>Не установлено ни одной связи</tr>"
	else
		dim tempN
		do while not rs.eof
			tempN = rs(0)
			response.write "<tr><td><a href='/devin/catalog/##cart" & tempN & "'>" & rs(1) & "</a><td width='30px' data-id='" & tempN & "'><div class='icon ic-clear' onclick='delCompare(this)'></div></tr>"
			rs.moveNext
		loop
	end if
	rs.close

	' Получаем вариант для связей
	response.write "<tr><td>Добавить связь: <select id='new-compare'><option value='0'>?"
	sql = "SELECT CARTRIDGE.N, CARTRIDGE.Caption " _
		& "FROM CARTRIDGE  " _
		& "WHERE CARTRIDGE.N NOT IN ( " _
			& "SELECT CARTRIDGE.N  " _
			& "FROM CARTRIDGE  " _
			& "INNER JOIN OFFICE ON OFFICE.Cartridge = CARTRIDGE.N " _
			& "INNER JOIN PRINTER ON OFFICE.Printer = PRINTER.N " _
			& "WHERE (PRINTER.N = " & id & ") " _
			& "GROUP BY CARTRIDGE.N " _
		& ") " _
		& "ORDER BY CARTRIDGE.Caption"
	'response.write sql
	rs.open sql, conn
	do while not rs.eof
		response.write "<option value='" & rs(0) & "'>" & rs(1)
		rs.moveNext
	loop
	rs.close
	response.write "</select><td width='30px'><div class='icon ic-add' onclick='addCompare()'></div></tr></table>"
	
	' Получаем список всех устройств, имеющих привязку к этому принтеру

	sql = "SELECT DEVICE.number_device, DEVICE.name, DEVICE.description, DEVICE.attribute FROM DEVICE INNER JOIN PRINTER ON DEVICE.ID_prn = PRINTER.N WHERE (PRINTER.N = " & id & ")"
	'response.write sql
	rs.open sql, conn
	if not rs.eof then
		response.write "<br /><b>Связанные устройства:</b><div class='cart-links'>"
		do while not rs.eof
			response.write "<div><a href='/devin/device/##" & trim(rs(0)) & "'>" & trim(rs(1)) & ": " & trim(rs(2)) & "</a><br />" _
			& "Расположен: " & trim(rs(3)) & "</div>"
			rs.moveNext
		loop
		response.write "</div>"
	end if
	rs.close

	response.write "</div></div>" _
		& "<div id='console'></div>" _
		& "<table class='cart-menu'><tr>" _
		& "<td onclick='cartSave()'>Сохранить" _
		& "<td onclick='cartDelete()'>Удалить" _
		& "<td onclick='cartClose()'>Закрыть" _ 
		& "</tr></table>"

	drop("")
%>