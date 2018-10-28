<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createobject("ADODB.Connection")
	dim rs : set rs = server.createobject("ADODB.Recordset")

	conn.open everest
	conn.execute "INSERT INTO Writeoffs (Name, Type, FolderId, Date, Description) VALUES ('Новое списание', '0', 0, '" & DateToSql(date) & "', '')"

	rs.open "SELECT MAX(Id) FROM Writeoffs", conn
		dim id : id = "off" & rs(0)
	rs.close

	response.write log(id, "Создано списание [" & id & "]") & id

	conn.close
	set rs = nothing
	set conn = nothing
%>