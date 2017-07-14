<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim id : id  = Request.QueryString("id")

	dim conn : set conn = Server.CreateObject("ADODB.Connection")

	conn.open everest
	conn.execute "UPDATE DEVICE SET deleted = 1 WHERE (number_device = '" & id & "')"

	response.write log(id, "Устройство удалено")

	conn.close
	set conn = nothing
%>