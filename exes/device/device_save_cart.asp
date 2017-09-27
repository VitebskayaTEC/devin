<!-- #include virtual ="/devin/core/core.inc" -->
<%
	response.contenttype = "text/html; charset=windows-1251"
	dim id : id = request.querystring("id")

	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim sql, text, name
	dim newID : newID = split(id, "-")
	dim isNewID : isNewID = false

	dim rs : set rs = server.createobject("ADODB.Recordset")
	rs.open "SELECT * FROM DEVICE WHERE (number_device = '" & id & "')", conn
	if rs.eof then
		response.write "error: Нет данных по данному ID"
		response.end
	else
		dim i, tempValue, newValue, tempText
		for i = 0 to rs.fields.count - 1
			if instr(lcase(request.form), lcase(rs(i).name)) > 0 then
				tempValue = trim(rs(i))
				if isnull(tempValue) then tempValue = ""
				newValue = DecodeUTF8(request.form(rs(i).name))
				if rs(i).name = "name" then name = tempValue
				if rs(i).name = "install_date" then
					' Проверка обновления даты и корректности конвертации введенного значения в дата-формат
					if not isdate(newValue) then
						response.write "error: Дата введена в некорректном формате. Ожидается дд.мм.гггг"
						response.end
					else 
						if cdate(tempValue) <> cdate(newValue) then
							if sql <> "" then 
								sql = sql & ", "
								text = text & ", "
							end if
							sql = sql & rs(i).name & " = '" & DateToSql(newValue) & "'"
							text = text & "дата установки с [" & tempValue & "] на [" & newValue & "]"
						end if
					end if				
				elseif (rs(i).name = "class_device") and (cstr(tempValue) <> cstr(newValue)) then
					' Подбор нового ID
					newID(1) = newValue
					isNewID = true
					if sql <> "" then 
						sql = sql & ", "
						text = text & ", "
					end if
					sql = sql & rs(i).name & " = '" & newValue & "'"
					text = text & "тип устройства с [" & tempValue & "] на [" & newValue & "]"
				elseif (rs(i).name = "inventory") and (cstr(tempValue) <> cstr(newValue)) then
					' Подбор нового ID
					newID(0) = newValue
					isNewID = true
					if sql <> "" then 
						sql = sql & ", "
						text = text & ", "
					end if
					sql = sql & rs(i).name & " = '" & newValue & "'"
					text = text & "инвентарный номер с [" & tempValue & "] на [" & newValue & "]"
				elseif cstr(tempValue) <> cstr(newValue) then
					' Простое обновление значения
					if sql <> "" then 
						sql = sql & ", "
						text = text & ", "
					end if
					sql = sql & rs(i).name & " = '" & newValue & "'"
					select case rs(i).name 
						case "name" 
							text = text & "наименование с [" & tempValue & "] на [" & newValue & "]"

						case "number_comp" 
							text = text & "компьютер с [" & tempValue & "] на [" & newValue & "]"
						
						case "description1C"
							text = text & "подпись в 1С с [" & tempValue & "] на [" & newValue & "]"
						
						case "description"
							text = text & "описание с [" & tempValue & "] на [" & newValue & "]"
						
						case "MOL"
							text = text & "м.о.л. с [" & tempValue & "] на [" & newValue & "]"
						
						case "number_serial"
							text = text & "серийный номер с [" & tempValue & "] на [" & newValue & "]"
						
						case "number_passport"
							text = text & "паспортный номер с [" & tempValue & "] на [" & newValue & "]"
						
						case "attribute"
							text = text & "расположение с [" & tempValue & "] на [" & newValue & "]"
						
						case "OS"
							text = text & "ОС с [" & tempValue & "] на [" & newValue & "]"
						
						case "OSKEY"
							text = text & "ключ ОС с [" & tempValue & "] на [" & newValue & "]"
						
						case "PRKEY"
							text = text & "ключ софта с [" & tempValue & "] на [" & newValue & "]"
						
						case "service_tag"
							text = text & "сервис-тег с [" & tempValue & "] на [" & newValue & "]"
						
						case "ID_prn"
							text = text & "типовой принтер (ID) с [" & tempValue & "] на [" & newValue & "]"
						
						case "check1C"
							if cstr(newValue) = "1" then
								text = text & "сверен с 1С с [нет] на [да]"
							else
								text = text & "сверен с 1С с [да] на [нет]"
							end if
						
						case "checkEverest"
							if cstr(newValue) = "1" then
								text = text & "сверен с AIDA с [нет] на [да]"
							else
								text = text & "сверен с AIDA с [да] на [нет]"
							end if
						
						case "DMI_UUID"
							text = text & "AIDA UUID с [" & tempValue & "] на [" & newValue & "]"
						
						case "G_ID"
							text = text & "папка (ID) с [" & tempValue & "] на [" & newValue & "]"
						
						case "used"
							if cstr(newValue) = "1" then
								text = text & "используется с [нет] на [да]"
							else
								text = text & "используется с [да] на [нет]"
							end if
					end select
				end if
			end if
		next
	end if
	rs.close

	if isNewID then
		rs.open "SELECT TOP (1) number_device FROM DEVICE WHERE (inventory LIKE '%" & newID(0) & "%') AND (class_device = '" & newID(1) & "') ORDER BY number_device DESC", conn
		if sql <> "" then 
			sql = sql & ", "
			text = text & ", "
		end if
		dim newIDtext
		if not rs.eof then
			newID = split(rs(0), "-")
			newIDtext = newID(0) & "-" & newID(1) & "-"
			if cstr(cint(newID(2)) + 1) < 10 then newIDtext = newIDtext & "0"
			newIDtext = newIDtext & cstr(cint(newID(2)) + 1)
		else
			newIDtext = newID(0) & "-" & newID(1) & "-01"			
		end if
		rs.close
		sql = sql & "number_device = '" & newIDtext & "'"
		conn.execute "UPDATE ELMEVENTS SET CNAME = '" & newIDtext & "' WHERE CNAME = '" & id & "'"
		conn.execute "UPDATE REMONT SET ID_D = '" & newIDtext & "' WHERE ID_D = '" & id & "'"
		text = text & "ID устройства изменен c [" & id & "] на [" & newIDtext & "]"
	end if

	if sql <> "" then 		
		sql = "UPDATE DEVICE SET " & sql & " WHERE (number_device = '" & id & "')"
		text = "Обновлена карточка устройства " & name & " [" & id & "], изменены: " & text
		conn.execute sql
		sql = log(id, text)
		response.write text
		if isNewID then response.write "<br /><a href='/devin/device/?id=" & newIDtext & "'>Обновить страницу, чтобы обновить ID устройства в общем списке</a>"
	else 
		response.write "Изменений не было"
	end if

	set rs = nothing
	conn.close	
	set conn = nothing
%>