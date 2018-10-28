<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn, rs, sql, i

	dim id : id = request.querystring("id")

	set conn = server.createObject("ADODB.Connection")
	set rs = server.createObject("ADODB.Recordset")


	' Запрос к данным о списании
	sql = "SELECT Name, Type, Date, Params, Description, FolderId, LastExcel, LastExcelDate, CostArticle FROM Writeoffs WHERE (Id = " & id & ")"

	conn.open everest
	rs.open sql, conn

	if rs.eof then
		' Списание не найдено
		response.write "<div class='cart-header'>Объект не найден!</div>" _
		& "<table class='cart-menu'><tr><td onclick='cartClose()'>Закрыть</td></tr></table>"
		rs.close
	else
		' Запись данных в массив для дальнейшей обработки
		dim writeoff(8)
		for i = 0 to 8
			writeoff(i) = rs(i)
		next
		rs.close

		dim defParams : defParams = ""
		dim defNParams : defNParams = 0

		' Шапка карточки
		response.write "<div class='cart-header'>" & writeoff(0) & "</div><form id='form' method='post'><table class='cart-table'>"

		' Последовательный вывод данных в таблицу
		' Наименование
		response.write "<tr><td>Наименование<td><input name='Name' value='" & writeoff(0) & "' /></tr>"

		' Тип списания
		response.write "<tr><td>Тип<td>"
		rs.open "SELECT O_Alias AS O_Index, O_Alias, O_Name, O_Data FROM catalog_writeoffs ORDER BY O_Name", conn
		if rs.eof then
			response.write "Нет доступа к справочнику"
		else
			response.write "<select name='Type'><option value='0'>?"
			do while not rs.eof
				if rs("O_Index") = writeoff(1) then
					response.write "<option selected value='" & rs("O_Alias") & "'>" & rs("O_Name")
					defParams = rs("O_Data")
				else
					response.write "<option value='" & rs("O_Alias") & "'>" & rs("O_Name")
				end if
				rs.movenext
			loop
			response.write "</select>"
		end if
		rs.close
		response.write "</tr>"

		' Дата создания списания
		response.write "<tr><td>Дата создания<td><input name='Date' value='" & datevalue(writeoff(2)) & "' style='width: 100px' /></tr>"

		' Параметры для экспорта в Excel, если их нет, будут взяты стандартные из справочника
		dim params, Nparams
		if isnull(writeoff(3)) then
			Nparams = 0
		elseif instr(writeoff(3), ";;") = 0 then
			Nparams = 0
		else
			params = split(writeoff(3), ";;")
			Nparams = ubound(params)
		end if
		if not isnull(defParams) then
			if instr(defParams, ";;") > 0 then
				defParams = split(defParams, ";;")
				defNParams = ubound(defParams)
			end if
		end if
		if defNParams > 0 then
			for i = 0 to defNParams
				if defParams(i) <> "" then response.write "<tr><td>" & defParams(i) else response.write "<tr><td>{неизвестный параметр}"
				if not (i > Nparams) and (Nparams <> 0) then
					response.write "<td><input name='params" & i & "' value='" & params(i) & "' /></tr>"
				else
					response.write "<td><input name='params" & i & "' value='' /></tr>"
				end if
			next
		else
			response.write "<tr><td>Параметры экспорта<td>В справочнике не определены поля для параметров</tr>"
		end if

		' Описание списания
		response.write "<tr><td>Описание<td><textarea name='Description'>" & writeoff(4) & "</textarea></tr>"

		' Статьи расходов
		if writeoff(1) <> "expl" then
			response.write "<tr><td>Статья расходов</td><td><select name='CostArticle'><option value='0'>Не выбрана</option>"
			if writeoff(8) = 1 then response.write "<option value='1' selected>Эксплуатационные расходы</option>" else response.write "<option value='1'>Эксплуатационные расходы</option>"
			if writeoff(8) = 2 then response.write "<option value='2' selected>Орг. техника</option>" else response.write "<option value='2'>Орг. техника</option>"
			if writeoff(8) = 3 then response.write "<option value='3' selected>ПТК АСУ</option>" else response.write "<option value='3'>ПТК АСУ</option>"
		end if

		' Группа
		response.write "<tr><td>Группа<td>"
		rs.open "SELECT Id AS O_Index, Id, Name FROM Folders WHERE (Type = 'repair') ORDER BY Name", conn
		if rs.eof then
			response.write "Нет доступа к справочнику"
		else
			response.write "<select name='FolderId'><option value='0'>?"
			do while not rs.eof
				if rs(0) = writeoff(5) then
					response.write "<option selected value='" & rs(1) & "'>" & rs(2)
				else
					response.write "<option value='" & rs(1) & "'>" & rs(2)
				end if
				rs.movenext
			loop
			response.write "</select>"
		end if
		rs.close
		response.write "</tr>"

		if writeoff(7) <> "" and not isnull(writeoff(7)) then
			' Ссылка на последний созданный при экспорте Excel файл
			response.write "<tr><td>Ссылка на распечатку<td><a href='/devin/excels/" & writeoff(6) & "'>" & writeoff(6) & "</a></tr>"

			' Дата последнего экспорта в Excel
			response.write "<tr><td>Последняя печать<td>" & writeoff(7) & "</tr>"
		end if

		' Меню операций и завершение карточки
		response.write "</table></form><div id='console'></div>" _
		& "<table class='cart-menu'><tr>" _
		& "<td onclick='cartDelete()'>Удалить</td>" _
		& "<td onclick='cartSave()'>Сохранить</td>" _
		& "<td onclick='writeoffExport()'>Печать</td>" _
		& "<td onclick='cartClose()'>Закрыть</td>" _
		& "</tr></table>"

	end if

	conn.close
	set rs = nothing
	set conn = nothing
%>