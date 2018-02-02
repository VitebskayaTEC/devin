<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs : set rs = server.createObject("ADODB.Recordset")
	conn.open everest
	conn.execute "INSERT INTO PRINTER (Caption) VALUES ('Новый типовой принтер')"
	rs.open "SELECT MAX(N) FROM PRINTER", conn
		response.write rs(0)
	rs.close
	conn.close
	set rs = nothing
	set conn = nothing
%>