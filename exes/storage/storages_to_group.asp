<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim storages : storages = split(request.form("select"), ";")

	dim storage, sql
	sql = ""
	for each storage in storages
		if storage <> "" then
			if sql <> "" then sql = sql & " OR "
			sql = sql & "Ncard = '" & storage & "'"
		end if
	next

	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	conn.execute "UPDATE Storages SET FolderId = '" & request.form("gid") & "' WHERE (" & sql & ")"

	conn.close
	set conn = nothing
%>