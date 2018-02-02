<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs : set rs = server.createObject("ADODB.Recordset")
	conn.open everest
	select case request.form("active")
		case "printer"
			conn.execute "DELETE FROM OFFICE WHERE (Printer = '" & replace(request.form("id"), "prn", "") & "' AND Cartridge = '" & request.form("compare") & "')"
			response.write "<div class='done'>Связь удалена</div>"
		case "cartridge"
			conn.execute "DELETE FROM OFFICE WHERE (Cartridge = '" & replace(request.form("id"), "cart", "") & "' AND Printer = '" & request.form("compare") & "')"
			response.write "<div class='done'>Связь удалена</div>"
		case else
			response.write "<div class='error'>Получено недостаточно данных</div>"
	end select
	conn.close
	set rs = nothing
	set conn = nothing
%>