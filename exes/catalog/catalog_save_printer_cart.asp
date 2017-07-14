<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs : set rs = server.createObject("ADODB.Recordset")
	dim id : id = replace(request.queryString("id"), "prn", "")
	conn.open everest

	rs.open "SELECT Caption FROM PRINTER WHERE N = " & id, conn
	if rs.eof then
		response.write "<div class='error'>Не найдены данные по данному ID</div>"
	else
		dim caption : caption = rs(0)
		dim newCaption : newCaption = DecodeUTF8(request.form("Caption"))
		if newCaption <> caption then
			conn.execute "UPDATE PRINTER SET Caption = '" & newCaption & "' WHERE N = " & id
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