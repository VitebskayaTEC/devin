<!-- #include virtual ="/devin/core/core.inc" -->
<%
	response.contenttype = "text/html; charset=windows-1251"
	dim id : id = request.querystring("id")

	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim rs : set rs = server.createobject("ADODB.Recordset")
	dim sql
	
	dim newID : newID = split(id, "-")
	sql = "SELECT TOP 1 number_device FROM DEVICE WHERE (inventory = '" & newID(0) & "') AND (class_device = '" & newID(1) & "') ORDER BY number_device DESC"
	'response.write sql
	rs.open sql, conn
		newID = split(rs(0), "-")
	rs.close
	
	dim idNew : idNew = newID(0) & "-" & newID(1) & "-"
	if (cint(newID(2)) + 1) < 10 then idNew = idNew & "0"
	idNew = idNew & (cint(newID(2)) + 1)
	
	sql = "SELECT * FROM DEVICE WHERE (number_device = '" & id & "')"
	rs.open sql, conn
		sql = "INSERT INTO DEVICE (inventory, class_device, number_device, name, number_comp, description1C, description, install_date, MOL, number_serial, number_passport, attribute, OS, OSKEY, PRKEY, ID_prn, check1C, checkEverest, DMI_UUID, G_ID, used, deleted) VALUES ('" & newID(0) & "', '" & newID(1) & "', '" & idNew & "', '" & trim(rs("name")) & "', '" & trim(rs("number_comp")) & "', '" & trim(rs("description1C")) & "', '" & trim(rs("description")) & "', '" & DateToSql(trim(rs("install_date"))) & "', '" & trim(rs("MOL")) & "', '" & trim(rs("number_serial")) & "', '" & trim(rs("number_passport")) & "', '" & trim(rs("attribute")) & "', '" & trim(rs("OS")) & "', '" & trim(rs("OSKEY")) & "', '" & trim(rs("PRKEY")) & "', '" & trim(rs("ID_prn")) & "', '" & trim(rs("check1C")) & "', '" & trim(rs("checkEverest")) & "', '" & trim(rs("DMI_UUID")) & "', '" & trim(rs("G_ID")) & "', '" & trim(rs("used")) & "', 0)"
	rs.close

	conn.execute sql
	response.write log(idNew, "На основе карточки [" & id & "] создана копия, сгенерированный ID [" & idNew & "]")
	response.write idNew

	set rs = nothing
	conn.close	
	set conn = nothing
%>