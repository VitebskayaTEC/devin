<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn, rs, sql, repair(24), pos, i

	dim id : id = request.querystring("id")

	set conn = server.createObject("ADODB.Connection")
	set rs = server.createObject("ADODB.Recordset")

	conn.open everest

	'�������� ������ ���� ��������
	sql = "SELECT " _
	& "REMONT.ID_D, " _
	& "DEVICE.inventory, " _
	& "DEVICE.name, " _
	& "DEVICE.description, " _
	& "DEVICE.install_date, " _
	& "DEVICE.MOL, " _
	& "DEVICE.attribute, " _
	& "DEVICE.used, " _
	& "PRINTER.Caption, " _
	& "REMONT.ID_U, " _
	& "SKLAD.Name AS Expr1, " _
	& "SKLAD.NCard, " _
	& "SKLAD.Price, " _
	& "SKLAD.uchet, " _
	& "CARTRIDGE.Caption AS Expr2, " _
	& "REMONT.Units, " _
	& "REMONT.Date, " _
	& "REMONT.IfSpis, " _
	& "REMONT.Virtual, " _
	& "REMONT.Author, " _
	& "REMONT.W_ID, " _
	& "REMONT.G_ID " _
	& "FROM CARTRIDGE " _
	& "RIGHT OUTER JOIN SKLAD ON CARTRIDGE.N = SKLAD.ID_cart " _
	& "RIGHT OUTER JOIN REMONT ON SKLAD.NCard = REMONT.ID_U " _
	& "LEFT OUTER JOIN DEVICE ON REMONT.ID_D = DEVICE.number_device " _
	& "LEFT OUTER JOIN PRINTER ON DEVICE.ID_prn = PRINTER.N " _
	& "LEFT OUTER JOIN DEVICE AS DEVICE_1 ON DEVICE.number_comp = DEVICE_1.number_device " _
	& "WHERE (REMONT.INum = " & id & ")"
	rs.open sql, conn
	'response.write sql

	if rs.eof then
		response.write "<div class='cart-header'>������ �� ������!</div>" _
		& "<table class='cart-menu'><tr><td onclick='cartClose()'>�������</td></tr></table>"
	else
		for i = 0 to 21
			repair(i) = trim(rs(i))
		next
		response.write "<div class='cart-header'>������ � " & id & "</div>" _
		& "<form id='form' method='post'>" _
		& "<table class='cart-table'>" _
		& "<tr><th colspan='2'><a href='/devin/devices/##" & repair(0) & "'>����������</a></tr>" _
		& "<tr><td>����������� �<td>" & repair(1) & "</tr>" _
		& "<tr><td>������������<td>" & repair(2) & "</tr>" _
		& "<tr><td>��������<td>" & repair(3) & "</tr>" _
		& "<tr><td>���� ���������<td>" & repair(4) & "</tr>" _
		& "<tr><td>�.�.�.<td>" & repair(5) & "</tr>" _
		& "<tr><td>������������<td>" & repair(6) & "</tr>"
		if instr(id, "PRN") > 0 then response.write "<tr><td>������� �������<td>" & repair(8) & "</tr>"

		response.write "<tr><th colspan='2'><a href='/devin/storagess/##" & repair(9) & "'>������</a></tr>" _
		& "<tr><td>������������<td>" & repair(10) & "</tr>" _
		& "<tr><td>����������� �<td>" & repair(11) & "</tr>" _
		& "<tr><td>���������, ��.<td>" & repair(12) & "</tr>" _
		& "<tr><td>���� �����<td>" & repair(13) & "</tr>"
		if instr(id, "PRN") > 0 then response.write "<tr><td>������� ��������<td>" & repair(14) & "</tr>"

		response.write "<tr><th colspan='2'>����</tr>" _
		& "<tr><td>���-�� ������� ���.<td><input name='units' type='number' style='width: 100px' value='" & repair(15) & "' /></tr>" _
		& "<tr><td>���� �������<td><input name='date' value='" & datevalue(repair(16)) & "' style='width: 100px' /></tr>" _
		& "<tr><td>������<td><select name='ifspis'>" _
		& "<option value='0'>���" _
		& "<option value='1' "
		if repair(17) = "1" then response.write " selected"
		response.write ">��" _
		& "</select></tr>" _
		& "<tr><td>�����������<td><select name='virtual'>" _
		& "<option value='0'>���" _
		& "<option value='1' "
		if repair(18) = "1" then response.write " selected"
		response.write ">��" _
		& "</select></tr>" _
		& "<tr><td>������ ������<td>" & repair(19) & "</tr>" _
		& "</table>" _
		& "</form>" _
		& "<div id='console'></div>" _
		& "<table class='cart-menu'><tr>" _
		& "<td onclick='cartDelete()'>�������</td>" _
		& "<td onclick='cartSave()'>���������</td>" _
		& "<td onclick='cartClose()'>�������</td>" _
		& "</tr></table>"
	end if

	rs.close
	conn.close
	set rs = nothing
	set conn = nothing
%>