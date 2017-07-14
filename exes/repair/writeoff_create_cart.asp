<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createobject("ADODB.Connection")
	dim rs : set rs = server.createobject("ADODB.Recordset")

	conn.open everest
	conn.execute "INSERT INTO writeoff (W_Name, W_Type, G_ID, W_Date, W_Description) VALUES ('Новое списание', '0', 0, '" & DateToSql(date) & "', '')"

	rs.open "SELECT MAX(W_ID) FROM writeoff", conn
		dim id : id = "off" & rs(0)
	rs.close
	
	response.write log(id, "Создано списание [" & id & "]") & id

	conn.close
	set rs = nothing
	set conn = nothing
%>