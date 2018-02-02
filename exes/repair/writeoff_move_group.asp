<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	dim wid: wid = replace(request.form("id"), "off", "")
	dim gid: gid = replace(request.form("in"), "rg",  "")

	conn.execute "UPDATE [writeoff] SET G_ID = '" & gid & "' WHERE (W_ID = '" & wid & "')"

	conn.close
	set conn = nothing
%>