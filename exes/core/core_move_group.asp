<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createObject("ADODB.Connection")
	conn.open everest

	conn.execute "UPDATE Folders SET FolderId = '" & request.form("in") & "' WHERE Id = '" & request.form("gid") & "'"

	conn.close
	set conn = nothing
%>