<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	conn.open everest
	conn.execute "DELETE FROM PRINTER WHERE N = " & replace(request.queryString("id"), "prn", "") 
	conn.close
	set conn = nothing
%>