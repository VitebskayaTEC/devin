<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim newID, id

	dim rs : set rs = server.createobject("ADODB.Recordset")
	rs.open "SELECT TOP 1 number_device FROM DEVICE WHERE (inventory = '000000' and class_device = 'CMP') ORDER BY number_device DESC", conn
	if rs.eof then
		id = "000000-CMP-00"
		newID = array("000000", "CMP", "00")
	else
		newID = split(rs(0), "-")
		id = newID(0) & "-" & newID(1) & "-"
		if (cint(newID(2)) + 1) < 10 then id = id & "0"
		id = id & (cint(newID(2)) + 1)
	end if
	rs.close

	conn.execute "INSERT INTO DEVICE (inventory, class_device, number_device, number_comp, name, description, install_date, g_id, id_prn, OS, check1C, checkEverest, used, deleted) VALUES ('" & newID(0) & "', '" & newID(1) & "', '" & id & "', '', 'Новое устройство', '', '" & DateToSql(date) & "', '0', '0', '0', '0', '0', '1', 0)"
	response.write log(id, "Создана карточка для устройства, сгенерированный ID [" & id & "]")
	response.write id

	set rs = nothing
	conn.close
	set conn = nothing
%>