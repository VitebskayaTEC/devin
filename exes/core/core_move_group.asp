<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createObject("ADODB.Connection")
	conn.open everest

	conn.execute "UPDATE [GROUP] SET G_Inside = '" & request.form("in") & "' WHERE (G_ID = '" & request.form("gid") & "')"

	conn.close
	set conn = nothing
%>