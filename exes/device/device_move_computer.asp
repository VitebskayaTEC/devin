<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createobject("ADODB.Connection")
	dim id:   id       = request.form("id")
	dim key:  key      = request.form("key")

	conn.open everest
	conn.execute "UPDATE DEVICE SET G_ID = '" & key & "' WHERE (number_device = '" & id & "')"
	conn.close

	set conn = nothing
%>