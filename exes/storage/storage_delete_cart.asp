<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim ncard : ncard = request.querystring("id")

	conn.execute "UPDATE SKLAD SET delit = 0 WHERE (NCard = '" & ncard & "')"
	response.write log(ncard, "Удалена карточка позиции [" & ncard & "]")
	
	conn.close
	set conn = nothing
%>