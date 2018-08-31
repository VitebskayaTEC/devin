<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs : set rs = server.createObject("ADODB.Recordset")
	dim id : id = replace(request.queryString("id"), "prn", "")
	conn.open everest

	rs.open "SELECT Caption, Description FROM PRINTER WHERE N = " & id, conn
	if rs.eof then
		response.write "<div class='error'>Не найдены данные по данному ID</div>"
	else
		dim val, sql

		val = DecodeUTF8(request.form("Caption"))
		if rs(0) <> val or isnull(rs(0)) then
			if sql <> "" then sql = sql & ", "
			sql = sql & "Caption = '" & val & "'"
		end if

		val = DecodeUTF8(request.form("Description"))
		if rs(1) <> val or isnull(rs(1)) then
			if sql <> "" then sql = sql & ", "
			sql = sql & "Description = '" & val & "'"
		end if

		if sql <> "" then
			conn.execute "UPDATE PRINTER SET " & sql & " WHERE N = " & id
			response.write "<div class='done'>Сохранено</div>"
		else
			response.write "<div class='done'>Нет изменений</div>"
		end if
	end if
	rs.close
	conn.close
	set rs = nothing
	set conn = nothing
%>