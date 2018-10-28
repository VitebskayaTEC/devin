<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn, rs, sql, repair(24), pos, i

	dim id : id = request.querystring("id")

	set conn = server.createObject("ADODB.Connection")
	set rs = server.createObject("ADODB.Recordset")

	conn.open everest

	'Получаем список всех ремонтов
	sql = "SELECT " _
	& "Repairs.DeviceId, " _
	& "Devices.Inventory, " _
	& "Devices.Name, " _
	& "Devices.Description, " _
	& "Devices.DateInstall, " _
	& "Devices.Mol, " _
	& "Devices.Location, " _
	& "Devices.IsOff, " _
	& "Printers.Name, " _
	& "Repairs.StorageInventory, " _
	& "Storages.Name AS Expr1, " _
	& "Storages.Inventory, " _
	& "Storages.Cost, " _
	& "Storages.Account, " _
	& "Cartridges.Name AS Expr2, " _
	& "Repairs.Number, " _
	& "Repairs.Date, " _
	& "Repairs.IsOff, " _
	& "Repairs.IsVirtual, " _
	& "Repairs.Author, " _
	& "Repairs.WriteoffId, " _
	& "Repairs.FolderId " _
	& "FROM Cartridges " _
	& "RIGHT OUTER JOIN Storages ON Cartridges.Id = Storages.CartridgeId " _
	& "RIGHT OUTER JOIN Repairs ON Storages.Inventory = Repairs.StorageInventory " _
	& "LEFT OUTER JOIN Devices ON Repairs.DeviceId = Devices.Id " _
	& "LEFT OUTER JOIN Printers ON DEVICE.PrinterId = Printers.Id " _
	& "LEFT OUTER JOIN Devices AS Computers ON DEVICE.ComputerId = Computers.Id " _
	& "WHERE (Repairs.Id = " & id & ")"
	rs.open sql, conn
	'response.write sql

	if rs.eof then
		response.write "<div class='cart-header'>Объект не найден!</div>" _
		& "<table class='cart-menu'><tr><td onclick='cartClose()'>Закрыть</td></tr></table>"
	else
		for i = 0 to 21
			repair(i) = trim(rs(i))
		next
		response.write "<div class='cart-header'>Ремонт № " & id & "</div>" _
		& "<form id='form' method='post'>" _
		& "<table class='cart-table'>" _
		& "<tr><th colspan='2'><a href='/devin/devices/##" & repair(0) & "'>Устройство</a></tr>" _
		& "<tr><td>Инвентарный №<td>" & repair(1) & "</tr>" _
		& "<tr><td>Наименование<td>" & repair(2) & "</tr>" _
		& "<tr><td>Описание<td>" & repair(3) & "</tr>" _
		& "<tr><td>Дата установки<td>" & repair(4) & "</tr>" _
		& "<tr><td>М.О.Л.<td>" & repair(5) & "</tr>" _
		& "<tr><td>Расположение<td>" & repair(6) & "</tr>"
		if instr(id, "PRN") > 0 then response.write "<tr><td>Типовой принтер<td>" & repair(8) & "</tr>"

		response.write "<tr><th colspan='2'><a href='/devin/storages/##" & repair(9) & "'>Деталь</a></tr>" _
		& "<tr><td>Наименование<td>" & repair(10) & "</tr>" _
		& "<tr><td>Инвентарный №<td>" & repair(11) & "</tr>" _
		& "<tr><td>Стоимость, шт.<td>" & repair(12) & "</tr>" _
		& "<tr><td>Счет учета<td>" & repair(13) & "</tr>"
		if instr(id, "PRN") > 0 then response.write "<tr><td>Типовой картридж<td>" & repair(14) & "</tr>"

		response.write "<tr><th colspan='2'>Инфо</tr>" _
		& "<tr><td>Кол-во деталей исп.<td><input name='Number' type='number' style='width: 100px' value='" & repair(15) & "' /></tr>" _
		& "<tr><td>Дата ремонта<td><input name='Date' value='" & datevalue(repair(16)) & "' style='width: 100px' /></tr>" _
		& "<tr><td>Списан<td><select name='IsOff'>" _
		& "<option value='0'>Нет" _
		& "<option value='1' "
		if repair(17) = "1" then response.write " selected"
		response.write ">Да" _
		& "</select></tr>" _
		& "<tr><td>Виртуальный<td><select name='IsVirtual'>" _
		& "<option value='0'>Нет" _
		& "<option value='1' "
		if repair(18) = "1" then response.write " selected"
		response.write ">Да" _
		& "</select></tr>" _
		& "<tr><td>Создал запись<td>" & repair(19) & "</tr>" _
		& "</table>" _
		& "</form>" _
		& "<div id='console'></div>" _
		& "<table class='cart-menu'><tr>" _
		& "<td onclick='cartDelete()'>Удалить</td>" _
		& "<td onclick='cartSave()'>Сохранить</td>" _
		& "<td onclick='cartClose()'>Закрыть</td>" _
		& "</tr></table>"
	end if

	rs.close
	conn.close
	set rs = nothing
	set conn = nothing
%>