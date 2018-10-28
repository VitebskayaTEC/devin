<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	select case request.form("app")
		case "device"
            conn.execute "UPDATE Devices SET FolderId = 0 WHERE FolderId = '" & request.form("gid") & "'"
		case "storage"
            conn.execute "UPDATE Storages SET FolderId = 0 WHERE FolderId = '" & request.form("gid") & "'"
		case "repair"
            conn.execute "UPDATE Writeoffs SET FolderId = 0 WHERE FolderId = '" & request.form("gid") & "'"
	end select

	conn.close
	set conn = nothing
%>