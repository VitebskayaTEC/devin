<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs : set rs = server.createObject("ADODB.Recordset")
	conn.open everest
	select case request.form("active")
		case "printer"
			conn.execute "INSERT INTO OFFICE (Printer, Cartridge) VALUES ('" & replace(request.form("id"), "prn", "") & "','" & request.form("compare") & "')"
			response.write "<div class='done'>Связь добавлена</div>"
		case "cartridge"
			conn.execute "INSERT INTO OFFICE (Cartridge, Printer) VALUES ('" & replace(request.form("id"), "cart", "") & "','" & request.form("compare") & "')"
			response.write "<div class='done'>Связь добавлена</div>"
		case else
			response.write "<div class='error'>Получено недостаточно данных</div>"
	end select
	conn.close
	set rs = nothing
	set conn = nothing
%>