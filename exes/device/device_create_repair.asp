<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim user : user = replace(Request.ServerVariables("REMOTE_USER"), "VST\", "")
	dim id : id = request.querystring("id")

	dim data(2)
	dim n : n = 0
	dim key, insert, update

	dim conn : set CONN = Server.CreateObject("ADODB.Connection")
	conn.open everest

	dim writeoff : writeoff = "0"
	if request.querystring("won") = "on" then
		dim text : text = request.querystring("writeoff")
		if text = "" or isnull(text) then text = "Ремонты от " & date
		conn.execute "INSERT INTO [writeoff] (W_Name, W_Type, W_Date, G_ID) VALUES ('" & text & "', 'mat', GETDATE(), 0)"
		dim rs : set rs = server.createobject("ADODB.Recordset")
		rs.open "SELECT TOP (1) W_ID FROM [writeoff] ORDER BY W_ID DESC", conn
		writeoff = rs(0)
		rs.close
		set rs = nothing
		response.write log("off" & writeoff, "Автоматическое создание списания при оформлении ремонта на устройство [" & id & "]")
	end if

	for each key in request.form
		if instr(key, "count") = 0 and instr(key, "virtual") = 0 and request.form(key) = "on" then
			' Инвентарный номер детали
			data(0) = key
			' Введенное кол-во деталей для ремонта
			data(1) = request.form(key & "count")
			' Виртуальный делать ремонт или нет
			data(2) = request.form(key & "virtual")
			if data(2) = "on" then
				data(2) = "1"
				update = update & "UPDATE SKLAD SET nuse = nuse + " & data(1) & " WHERE (ncard = '" & data(0) & "') " & chr(13)
				response.write log(id, "Ремонт: использована позиция с инвентарным № [" & data(0) & "] в количестве [" & data(1) & "] шт. (виртуальный)")
			else
				data(2) = "0"
				update = update & "UPDATE SKLAD SET nuse = nuse + " & data(1) & ", nis = nis - " & data(1) & " WHERE (ncard = '" & data(0) & "') " & chr(13)
				response.write log(id, "Ремонт: использована позиция с инвентарным № [" & data(0) & "] в количестве [" & data(1) & "] шт.")
			end if
			insert = insert & "INSERT INTO REMONT (ID_D, ID_U, Units, Date, Author, IfSpis, Virtual, W_ID, G_ID) VALUES ('" & id & "', '" & data(0) & "', '" & data(1) & "', GETDATE(), '" & user & "', '0', '" & data(2) & "', '" & writeoff & "', 0) " & chr(13)

			n = n + 1
		end if
	next

	conn.execute insert
	conn.execute update

	response.write "Создано ремонтов: " & n
	if writeoff <> "0" then
		response.write " <a href='/devin/repair/##off" & writeoff & "'>перейти к созданной группе</a>"
	end if

	conn.close
	set conn = nothing
%>