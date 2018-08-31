<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")
	dim rs 		: set rs = server.createObject("ADODB.Recordset")
	dim id 		: id = replace(request.queryString("id"), "cart", "")

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

	' Получаем данные о картридже
	dim cartridge(4), i
	rs.open "SELECT Caption, Price, Type, Color, Description FROM CARTRIDGE WHERE N = " & id, conn
		if rs.eof then
			drop("Нет данных по данному ID")
		else
			for i = 0 to 4
				cartridge(i) = rs(i)
			next
		end if
	rs.close

	' Получаем справочник для типов и цветов
	dim colors(100, 1), types(100, 1), Ncolors, Ntypes
	rs.open "SELECT C_type, C_alias, C_name FROM catalog_cartridge_types ORDER BY C_Type, C_Name", conn
	if rs.eof then
		drop("Ошибка при загрузке справочника типовых параметров")
	else
		Ncolors = 0
		Ntypes = 0
		do while not rs.eof
			if rs("C_type") = "Type" then
				types(Ntypes, 0) = rs("C_alias")
				types(Ntypes, 1) = rs("C_name")
				Ntypes = Ntypes + 1
			else
				colors(Ncolors, 0) = rs("C_alias")
				colors(Ncolors, 1) = rs("C_name")
				Ncolors = Ncolors + 1
			end if
			rs.moveNext
		loop
	end if
	rs.close

	' Форма с данными о картридже и титульная строка
	response.write "<div class='cart-header'>" & cartridge(0) & "</div>" _
		& "<div class='cart-overflow'><form id='form'><table class='cart-table'>" _
		& "<tr><td>Наименование<td><input name='Caption' value='" & cartridge(0) & "' /></tr>" _
		& "<tr><td>Типовая стоимость<td><input name='Price' type='number' style='width:100px' value='" & cartridge(1) & "' />&nbsp;&nbsp;"

	dim prices(100), uchet, price, fixcost, Nprices
	dim minBaseValue: minBaseValue = 23
	rs.open "SELECT uchet, Price FROM SKLAD WHERE (ID_cart = " & id & ")", conn
	if not rs.eof then
		response.write ""
		Nprices = 0
		do while not rs.eof
			uchet = rs(0)
			price = rs(1)


			' Если счет учета - материалы, то стоимость берется с половинным коэффициентом, т.к. часть стоимости возмещена
			if uchet = "10.5.1" then fixcost = 1.2 else fixcost = 2.4
			' Если стоимость нулевая, то берется базовая величина
			if price = 0 then price = 27.6 else price = price * fixcost

			prices(Nprices) = round(price, 2)

			Nprices = Nprices + 1

			rs.moveNext
		loop

		price = 0
		for i = 0 to Nprices
			price = price + prices(i)
		next

		price = round(( price / Nprices ), 2)
		response.write "Вычислено: ~" & price & " б.р."
	else
		response.write "Данных о стоимости нет"
	end if
	rs.close



	response.write "</tr><tr><td>Тип<td><select name='Type'><option value='0'>?"
	for i = 0 to Ntypes - 1
		if cartridge(2) = types(i, 0) then
			response.write "<option value='" & types(i, 0) & "' selected>" & types(i, 1)
		else
			response.write "<option value='" & types(i, 0) & "'>" & types(i, 1)
		end if
	next
	response.write "</select></tr><tr><td>Цвет<td><select name='Color'><option value='0'>?"
	for i = 0 to Ncolors - 1
		if cartridge(3) = colors(i, 0) then
			response.write "<option value='" & colors(i, 0) & "' selected>" & colors(i, 1)
		else
			response.write "<option value='" & colors(i, 0) & "'>" & colors(i, 1)
		end if
	next
	response.write "</select></tr>" _
	& "<tr><td>Описание<td><textarea name='Description'>" & cartridge(4) & "</textarea></tr>" _
	& "</table></form><div class='cart-links'>"

	' Связи
	response.write "<table class='cart-table catalog-compares'><tr><td><br /><b>Связи с типовыми принтерами:</b><td></tr>"
	rs.open "SELECT PRINTER.N, PRINTER.Caption FROM OFFICE INNER JOIN PRINTER ON OFFICE.Printer = PRINTER.N INNER JOIN CARTRIDGE ON OFFICE.Cartridge = CARTRIDGE.N WHERE (CARTRIDGE.N = " & id & ") ORDER BY PRINTER.Caption", conn
	if rs.eof then
		response.write "<tr><td>Не установлено ни одной связи</tr>"
	else
		dim tempN
		do while not rs.eof
			tempN = rs(0)
			response.write "<tr><td><a href='/devin/catalog/##prn" & tempN & "'>" & rs(1) & "</a><td width='30px' data-id='" & tempN & "'><div class='icon ic-clear' onclick='delCompare(this)'></div></tr>"
			rs.moveNext
		loop
	end if
	rs.close

	' Получаем вариант для связей
	response.write "<tr><td>Добавить связь: <select id='new-compare'><option value='0'>?"
	rs.open "SELECT PRINTER.N, PRINTER.Caption " _
		& "FROM PRINTER  " _
		& "WHERE PRINTER.N NOT IN ( " _
			& "SELECT PRINTER.N  " _
			& "FROM PRINTER  " _
			& "INNER JOIN OFFICE ON OFFICE.Printer = PRINTER.N " _
			& "INNER JOIN CARTRIDGE ON OFFICE.Cartridge = CARTRIDGE.N " _
			& "WHERE (CARTRIDGE.N = " & id & ") " _
			& "GROUP BY PRINTER.N " _
		& ") " _
		& "ORDER BY PRINTER.Caption"
	do while not rs.eof
		response.write "<option value='" & rs(0) & "'>" & rs(1)
		rs.moveNext
	loop
	rs.close
	response.write "</select><td width='30px'><div class='icon ic-add' onclick='addCompare()'></div></tr></table>"

	' Получаем список позиций на складе, связанных с этим картриджем
	rs.open "SELECT SKLAD.NCard AS ID, SKLAD.NCard, SKLAD.Name, SKLAD.Nis FROM CARTRIDGE LEFT OUTER JOIN SKLAD ON CARTRIDGE.N = SKLAD.ID_cart WHERE (SKLAD.delit = 1) AND (SKLAD.Nis > 0) AND (CARTRIDGE.N = " & id & ") ORDER BY SKLAD.NCard, SKLAD.Name", conn
	if not rs.eof then
		response.write "<br /><b>Позиции со склада, связанные с этим картриджем:</b>"
		do while not rs.eof
			response.write "<div><a href='/devin/storage/##" & trim(rs(0)) & "'>" & trim(rs(1)) & ": " & trim(rs(2)) & "</a><br />" _
				& trim(rs(2)) & "</div>"
			rs.moveNext
		loop
	end if
	rs.close

	response.write "</div></div>" _
		& "<div id='console'></div>" _
		& "<table class='cart-menu'><tr>" _
		& "<td onclick='cartSave()'>Сохранить" _
		& "<td onclick='cartDelete()'>Удалить" _
		& "<td onclick='cartClose()'>Закрыть" _
		& "</tr></table>"

	conn.close
	set rs = nothing
	set conn = nothing
%>