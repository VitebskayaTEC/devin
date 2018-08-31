<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createobject("ADODB.Connection")
	dim rs 		: set rs = server.createobject("ADODB.Recordset")
	dim id 		: id = replace(request.querystring("id"), "off", "")

	conn.open everest
	rs.open "SELECT W_Name, W_Type, W_Date, W_Params, W_Description, G_ID, W_Cost_Article FROM writeoff WHERE (W_ID = '" & id & "')", conn
	if rs.eof then
		response.write "<div class='error'>Нет данных по данному ID</div>"
	else
		dim writeoff(6), i, temp

		for i = 0 to 6
			writeoff(i) = rs(i)
		next

		dim sql 	: sql = ""
		dim text 	: text = ""

		temp = DecodeUTF8(request.form("W_Name"))
		if temp <> writeoff(0) then
			text = "наименование с [" & writeoff(0) & "] на [" & temp & "]"
			sql = "W_Name = '" & temp & "'"
		end if

		temp = request.form("W_Type")
		if temp <> writeoff(1) or isnull(writeoff(1)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "тип с [" & writeoff(1) & "] на [" & temp & "]"
			sql = sql & "W_Type = '" & temp & "'"
		end if

		temp = request.form("W_Date")
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
			sql = sql & "W_Date = '" & DateToSql(temp) & "'"
		end if

		dim key
		temp = ""
		dim first: first = true
		for i = 0 to 20
			if not isNull(request.form("params" & i)) and not isEmpty(request.form("params" & i)) then
				if first then
					temp = temp & DecodeUTF8(request.form("params" & i))
				else
					temp = temp & ";;" & DecodeUTF8(request.form("params" & i))
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
			sql = sql & "W_Params = '" & temp & "'"
		end if

		temp = DecodeUTF8(request.form("W_Description"))
		if temp <> writeoff(4) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "описание с [" & writeoff(4) & "] на [" & temp & "]"
			sql = sql & "W_Description = '" & temp & "'"
		end if

		temp = request.form("W_Cost_Article")
		if temp <> writeoff(6) or isNull(writeoff(6)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "статья расходов с [" & writeoff(6) & "] на [" & temp & "]"
			sql = sql & "W_Cost_Article = '" & temp & "'"
		end if

		temp = request.form("G_ID")
		if isnull(writeoff(5)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "папка с [group" & writeoff(5) & "] на [group" & temp & "]"
			sql = sql & "G_ID = '" & temp & "'"
		elseif cstr(temp) <> cstr(writeoff(5)) then
			if text <> "" then
				text = text & ", "
				sql = sql & ", "
			end if
			text = text & "папка с [group" & writeoff(5) & "] на [group" & temp & "]"
			sql = sql & "G_ID = '" & temp & "'"
		end if

		if text <> "" then
			conn.execute "UPDATE writeoff SET " & sql & " WHERE (W_ID = '" & id & "')"
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