<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	dim table
	select case request.form("app")
		case "device"
			table = "DEVICE"
		case "storage"
			table = "SKLAD"
		case "repair"
			table = "writeoff"
	end select

	conn.execute "UPDATE " & table & " SET G_ID = 0 WHERE (G_ID = '" & request.form("gid") & "')"

	conn.execute "DELETE FROM [GROUP] WHERE (G_ID = '" & request.form("gid") & "')"
	
	conn.close
	set conn = nothing
%>