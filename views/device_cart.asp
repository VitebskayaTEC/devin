<!-- #include virtual ="/devin/core/core.inc" -->
<%
	' ���������� ���������� � ����������� � ���� ������
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs:   set rs   = server.createObject("ADODB.Recordset")
	dim keys: set keys = server.createObject("Scripting.Dictionary")
	dim id:   id = request.queryString("id")
	dim sql:  sql = ""

	conn.open everest

	dim size: size = 22
	dim device(22, 2), temp, cls

	'�������� ������ �� ����������
	sql = "SELECT * FROM DEVICE WHERE (number_device = '" & id & "')"
	rs.open sql, conn
	if not rs.eof then
		dim i
		for i = 0 to rs.fields.count - 1
			keys.add rs(i).name, trim(rs(i))
		next
	else
		response.write "���������� �� �������!"
		response.end
	end if
	rs.close

	cls = keys("class_device")
	'������ header � ����������� �� ���� � ������� � ���� ����������
	response.write "<div class='cart-header'>"
	if cls = "CMP" then
		response.write "��������� " & keys("name")
	else
		if id = "000000-000-00" then
		response.write "����� ����������"
		else
			rs.open "SELECT name FROM DEVICE WHERE (number_device = '"& keys("number_comp") &"')", conn
			if not rs.eof then
				response.write keys("name") & " �� " & trim(rs(0))
			else
				response.write keys("name")
			end if
			rs.close
		end if
	end if

	response.write "</div><form id='cart-form'><table class='cart-table'>" _
	& "<tr><td>������������<td><input name='name' value='" & keys("name") & "' /></tr>" _
	& "<tr><td>����������� �����<td><input name='inventory' value='" & keys("inventory") & "' /></tr>" _
	& "<tr><td>��� ����������<td><select name='class_device'><option value='0'>?"

	rs.open "SELECT T_alias, T_alias, T_name FROM catalog_device_types ORDER BY T_name", conn
	do while not rs.eof
		if cls = rs(0) then
			response.write "<option value='" & rs(1) & "' selected>" & rs(2)
		else
			response.write "<option value='" & rs(1) & "'>" & rs(2)
		end if
		rs.movenext
	loop
	rs.close

	response.write "</select></tr>" _
	& "<tr><td>����� ����������<td><i><small>" & keys("number_device") & "</small></i></tr>"

	if cls <> "CMP" then
		response.write "<tr><td>���������<td><select name='number_comp'><option value='0'>?"
		rs.Open "SELECT RTRIM(number_device), RTRIM(number_device), RTRIM(name) FROM DEVICE WHERE (class_device = 'CMP') ORDER BY name", conn
		temp = keys("number_comp")
		do while not rs.eof
			if temp = rs(0) then
				Response.Write "<option selected value='" & rs(1) & "'>" & rs(2)
			else
				Response.Write "<option value='" & rs(1) & "'>" & rs(2)
			end if
			rs.MoveNext
		loop
		rs.Close
		response.write "</select> <a onclick='cartOpen(document.getElementById(" & chr(34) & temp & chr(34) & "))'>��������</a></tr>"
	end if

	response.write "<tr><td>������� � 1�<td><input name='description1C' value='" & keys("description1C") & "' /></tr>" _
	& "<tr><td>��������<td><textarea name='description'>" & keys("description") & "</textarea></tr>" _
	& "<tr><td>�.�.�.<td><input name='mol' value='" & keys("MOL") & "' /></tr>" _
	& "<tr><td>������������<td><input name='attribute' value='" & keys("attribute") & "' /></tr>" _
	& "<tr><td>�������� �����<td><input name='number_serial' value='" & keys("number_serial") & "' /></tr>" _
	& "<tr><td>���������� �����<td><input name='number_passport' value='" & keys("number_passport") & "' /></tr>" _
	& "<tr><td>������-���<td><input name='service_tag' value='" & keys("service_tag") & "' /></tr>" _
	& "<tr><td>���� ���������<td><input name='install_date' style='width: 100px' value='" & datevalue(keys("install_date")) & "' /> <a><b>" & datediff("h", datevalue(keys("install_date")), datevalue(date)) & " �����</b></a></tr>"

	if cls = "CMP" then
		response.write "<tr><td>��<td><select name='OS'><option value='0'>?"
		rs.Open "Select T_alias, T_alias,T_name From catalog_os Order by T_name", CONN
		do while not rs.eof
			if keys("os") = trim(rs(0)) then
				response.write "<option selected value='" & trim(rs(1)) & "'>" & trim(rs(2))
			else
				response.write "<option value='" & trim(rs(1)) & "'>" & trim(rs(2))
			end if
			rs.MoveNext
		loop
		rs.Close
		response.write "</select></tr>"
	end if

	if cls = "CMP" then
		response.write "<tr><td>���� ��<td><input name='OSKEY' value='" & keys("oskey") & "' /></tr>" _
		& "<tr><td>���� ��� �����<td><input name='PRKEY' value='" & keys("prkey") & "' /></tr>"
	end if

	response.write "<tr><td>������ �� ��������</td><td><input class='numbers' name='passportgold' value='" & keys("PassportGold") & "' /></td></tr>" _
	& "<tr><td>������� �� ��������</td><td><input class='numbers' name='passportsilver' value='" & keys("PassportSilver") & "' /></td></tr>" _
	& "<tr><td>������� �� ��������</td><td><input class='numbers' name='passportplatinum' value='" & keys("PassportPlatinum") & "' /></td></tr>" _
	& "<tr><td>��� �� ��������</td><td><input class='numbers' name='passportmpg' value='" & keys("PassportMPG") & "' /></td></tr>"

	if cls = "PRN" then
		response.write "<tr><td>������� �������<td><select name='ID_prn'><option value='0'>?"
		rs.Open "Select N, Caption From PRINTER Order by Caption", CONN
		dim temp_n
		do while not rs.eof
			temp_n = rs(0)
			'response.write "<option>1: [" & keys("ID_prn") & "] 2: [" & RS(0) & "]"
			if cstr(keys("ID_prn")) = cstr(temp_n) then
				response.write "<option selected value='" & temp_n & "'>" & rs(1)
			else
				response.write "<option value='" & temp_n & "'>" & rs(1)
			end if
			rs.MoveNext
		loop
		rs.Close
		response.write "</select></tr>"
	end if

	Response.Write "<tr><td>������������<td><select name='used'>"
	if keys("used") = "1" then
		Response.Write "<option value='0'>���<option selected value='1'>��"
	else
		Response.Write "<option selected value='0'>���<option value='1'>��"
	end if
	Response.Write "</select></tr>"

	Response.Write "<tr><td>������ � 1�<td><select name='check1C'>"
	if keys("check1C") = "1" then
		Response.Write "<option value='0'>���<option selected value='1'>��"
	else
		Response.Write "<option selected value='0'>���<option value='1'>��"
	end if
	Response.Write "</select></tr>"

	if cls = "CMP" then
		Response.Write "<tr><td>������ � AIDA<td><select name='checkEverest'><option value='0'>���<option"
		if keys("checkEverest") = "1" then response.write " selected"
		Response.Write " value='1'>��</select></tr>"
	end if

	Response.Write "<tr><td>������<td><select name='G_ID'><option value='0'>?"
	rs.Open "Select G_ID, G_ID, G_Title From [GROUP] Where (G_Type = 'device') Order by G_Title", CONN
	do while not rs.eof
		if keys("G_ID") = cstr(rs(0)) then
			Response.Write "<option value='" & trim(rs(1)) & "' selected>" & trim(rs(2))
		else
			Response.Write "<option value='" & trim(rs(1)) & "'>" & trim(rs(2))
		end if
		rs.movenext
	loop

	response.write "</select></tr></table></form>"

	rs.close:   set rs = nothing
	conn.close: set conn = nothing
	set keys = nothing

	response.write "<div class='cart-links'><a onclick='cartHistoryRepair()'>������� ��������</a><br/>"
	if cls = "CMP" then
		response.write "<a onclick='cartAidaAutorun()'>������������</a><br/><a onclick='cartAidaDevices()'>������������</a><br/>"
	end if
	if cls = "CMP" or cls = "DIS" or cls = "PRN" then
		response.write "<a onclick='cartDefect()'>��������� ���</a>"
	end if

	response.write "</div><div id='console'></div><table class='cart-menu'><tr>" _
	& "<td onclick='cartSave()'>���������</td>" _
	& "<td onclick='deviceToRepair()'>������</td>"
	if cls = "CMP" then
		response.write "<td onclick='cartAida()'>Everest</td>" _
		& "<td onclick='cartHistory(""t=name"")'>������� �� �����</td>"
	end if
	response.write "<td onclick='cartHistory(""t=id"")'>������� �� ���.�.</td>" _
	& "<td onclick='cartDelete()'>�������</td>" _
	& "<td onclick='cartCopy()'>�����.</td>" _
	& "<td onclick='cartClose()'>�������</td>" _
	& "</tr></table>"
%>