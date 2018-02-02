<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim rs : set rs = server.createobject("ADODB.Recordset")
	rs.open "SELECT MIN(CAST(NCard as int)) - 1 FROM SKLAD", conn

	dim ncard : ncard = rs(0)

	rs.close
	set rs = nothing

	conn.execute "INSERT INTO SKLAD (Name, NCard, Nadd, Nis, Nuse, Nbreak, Price, Date, ID_cart, G_ID, delit) VALUES ('Новая позиция', '" & ncard & "', 0, 1, 0, 0, 0, '" & DateToSql(date) & "', 0, 0, 1)"
	response.write log(ncard, "Создана карточка новой позиции, сгенерированный инвентарный номер [" & ncard & "]") & ncard
	
	conn.close
	set conn = nothing
%>