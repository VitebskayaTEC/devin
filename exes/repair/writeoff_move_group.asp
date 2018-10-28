<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	dim wid: wid = replace(request.form("id"), "off", "")
	dim gid: gid = replace(request.form("in"), "rg",  "")

	conn.execute "UPDATE Writeoffs SET FolderId = '" & gid & "' WHERE Id = '" & wid & "'"

	conn.close
	set conn = nothing
%>