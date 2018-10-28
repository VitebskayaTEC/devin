<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createobject("ADODB.Connection")
	dim rs 		: set rs = server.createobject("ADODB.Recordset")
	dim id 		: id = replace(request.querystring("id"), "off", "")

	conn.open everest
	rs.open "SELECT Name, Type, Date, Params, Description, FolderId, CostArticle FROM Writeoffs WHERE Id = '" & id & "'", conn
	if rs.eof then
		response.write "<div class='error'>Нет данных по данному ID</div>"
	else
		dim writeoff(6), i, temp

		for i = 0 to 6
			writeoff(i) = rs(i)
		next

		dim sql 	: sql = ""
		dim text 	: text = ""

		temp = DecodeUTF8(request.form("Name"))
		if temp <> writeoff(0) then
			text = "наименование с [" & writeoff(0) & "] на [" & temp & "]"
			sql = "W_Name = '" & temp & "'"
		end if

		temp = request.form("Type")
		if temp <> writeoff(1) or isnull(writeoff(1)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "тип с [" & writeoff(1) & "] на [" & temp & "]"
			sql = sql & "Type = '" & temp & "'"
		end if

		temp = request.form("Date")
		if not isdate(temp) then
			response.write "<div class='error'>Введено некорректное значение даты. Ожидается формат дд.мм.гггг</div>"
			response.end
		end if
		if datevalue(temp) <> datevalue(writeoff(2)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "дата создания с [" & datevalue(writeoff(2)) & "] на [" & datevalue(temp) & "]"
			sql = sql & "Date = '" & DateToSql(temp) & "'"
		end if

		dim key
		temp = ""
		dim first: first = true
		for i = 0 to 20
			if not isNull(request.form("Params" & i)) and not isEmpty(request.form("Params" & i)) then
				if first then
					temp = temp & DecodeUTF8(request.form("Params" & i))
				else
					temp = temp & ";;" & DecodeUTF8(request.form("Params" & i))
				end if
				first = false
			end if
		next
		if temp <> writeoff(3) or isNull(writeoff(3)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "параметры экспорта с [" & writeoff(3) & "] на [" & temp & "]"
			sql = sql & "Params = '" & temp & "'"
		end if

		temp = DecodeUTF8(request.form("Description"))
		if temp <> writeoff(4) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "описание с [" & writeoff(4) & "] на [" & temp & "]"
			sql = sql & "Description = '" & temp & "'"
		end if

		temp = request.form("CostArticle")
		if temp <> writeoff(6) or isNull(writeoff(6)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "статья расходов с [" & writeoff(6) & "] на [" & temp & "]"
			sql = sql & "CostArticle = '" & temp & "'"
		end if

		temp = request.form("FolderId")
		if isnull(writeoff(5)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "папка с [group" & writeoff(5) & "] на [group" & temp & "]"
			sql = sql & "FolderId = '" & temp & "'"
		elseif cstr(temp) <> cstr(writeoff(5)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "папка с [group" & writeoff(5) & "] на [group" & temp & "]"
			sql = sql & "FolderId = '" & temp & "'"
		end if

		if text <> "" then
			conn.execute "UPDATE Writeoffs SET " & sql & " WHERE Id = '" & id & "'"
			text = "Обновлена карточка списания [off" & id & "], изменены: " & text
			response.write log("off" & id, text) & "<div class='done'>" & text & "</div>"
		else
			response.write "<div class='done'>Изменений нет</div>"
		end if
	end if

	rs.close
	conn.close
	set rs = nothing
	set conn = nothing
%>