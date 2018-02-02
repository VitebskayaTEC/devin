<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	conn.open everest
	conn.execute "DELETE FROM CARTRIDGE WHERE N = " & replace(request.queryString("id"), "cart", "") 
	conn.close
	set conn = nothing
%>