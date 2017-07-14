<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	conn.execute "UPDATE [GROUP] SET G_Title = '" & DecodeUTF8(request.form("title")) & "' WHERE (G_ID = '" & request.form("gid") & "')"
	'conn.execute "UPDATE [GROUP] SET G_Title = '" & request.form("title") & "' WHERE (G_ID = '" & request.form("gid") & "')"

	conn.close
	set conn = nothing
%>