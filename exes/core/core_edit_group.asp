<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	conn.execute "UPDATE Folders SET Name = '" & DecodeUTF8(request.form("title")) & "' WHERE (Id = '" & request.form("gid") & "')"

	conn.close
	set conn = nothing
%>