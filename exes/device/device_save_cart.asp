<!-- #include virtual ="/devin/core/core.inc" -->
<%
	response.contenttype = "text/html; charset=windows-1251"
	dim id : id = request.querystring("id")

	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim sql, text, name
	dim newID : newID = split(id, "-")
	dim isNewID : isNewID = false

	dim rs : set rs = server.createobject("ADODB.Recordset")
	rs.open "SELECT * FROM DEVICE WHERE (number_device = '" & id & "')", conn
	if rs.eof then
		response.write "error: ��� ������ �� ������� ID"
		response.end
	else
		dim i, tempValue, newValue, tempText
		for i = 0 to rs.fields.count - 1
			if instr(lcase(request.form), lcase(rs(i).name)) > 0 then
				tempValue = trim(rs(i))
				if isnull(tempValue) then tempValue = ""
				newValue = DecodeUTF8(request.form(rs(i).name))
				if rs(i).name = "name" then name = tempValue
				if rs(i).name = "install_date" then
					' �������� ���������� ���� � ������������ ����������� ���������� �������� � ����-������
					if not isdate(newValue) then
						response.write "error: ���� ������� � ������������ �������. ��������� ��.��.����"
						response.end
					else 
						if cdate(tempValue) <> cdate(newValue) then
							if sql <> "" then 
								sql = sql & ", "
								text = text & ", "
							end if
							sql = sql & rs(i).name & " = '" & DateToSql(newValue) & "'"
							text = text & "���� ��������� � [" & tempValue & "] �� [" & newValue & "]"
						end if
					end if				
				elseif (rs(i).name = "class_device") and (cstr(tempValue) <> cstr(newValue)) then
					' ������ ������ ID
					newID(1) = newValue
					isNewID = true
					if sql <> "" then 
						sql = sql & ", "
						text = text & ", "
					end if
					sql = sql & rs(i).name & " = '" & newValue & "'"
					text = text & "��� ���������� � [" & tempValue & "] �� [" & newValue & "]"
				elseif (rs(i).name = "inventory") and (cstr(tempValue) <> cstr(newValue)) then
					' ������ ������ ID
					newID(0) = newValue
					isNewID = true
					if sql <> "" then 
						sql = sql & ", "
						text = text & ", "
					end if
					sql = sql & rs(i).name & " = '" & newValue & "'"
					text = text & "����������� ����� � [" & tempValue & "] �� [" & newValue & "]"
				elseif cstr(tempValue) <> cstr(newValue) then
					' ������� ���������� ��������
					if sql <> "" then 
						sql = sql & ", "
						text = text & ", "
					end if
					sql = sql & rs(i).name & " = '" & newValue & "'"
					select case rs(i).name 
						case "name" 
							text = text & "������������ � [" & tempValue & "] �� [" & newValue & "]"

						case "number_comp" 
							text = text & "��������� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "description1C"
							text = text & "������� � 1� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "description"
							text = text & "�������� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "MOL"
							text = text & "�.�.�. � [" & tempValue & "] �� [" & newValue & "]"
						
						case "number_serial"
							text = text & "�������� ����� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "number_passport"
							text = text & "���������� ����� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "attribute"
							text = text & "������������ � [" & tempValue & "] �� [" & newValue & "]"
						
						case "OS"
							text = text & "�� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "OSKEY"
							text = text & "���� �� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "PRKEY"
							text = text & "���� ����� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "service_tag"
							text = text & "������-��� � [" & tempValue & "] �� [" & newValue & "]"
						
						case "ID_prn"
							text = text & "������� ������� (ID) � [" & tempValue & "] �� [" & newValue & "]"
						
						case "check1C"
							if cstr(newValue) = "1" then
								text = text & "������ � 1� � [���] �� [��]"
							else
								text = text & "������ � 1� � [��] �� [���]"
							end if
						
						case "checkEverest"
							if cstr(newValue) = "1" then
								text = text & "������ � AIDA � [���] �� [��]"
							else
								text = text & "������ � AIDA � [��] �� [���]"
							end if
						
						case "DMI_UUID"
							text = text & "AIDA UUID � [" & tempValue & "] �� [" & newValue & "]"
						
						case "G_ID"
							text = text & "����� (ID) � [" & tempValue & "] �� [" & newValue & "]"
						
						case "used"
							if cstr(newValue) = "1" then
								text = text & "������������ � [���] �� [��]"
							else
								text = text & "������������ � [��] �� [���]"
							end if
					end select
				end if
			end if
		next
	end if
	rs.close

	if isNewID then
		rs.open "SELECT TOP (1) number_device FROM DEVICE WHERE (inventory LIKE '%" & newID(0) & "%') AND (class_device = '" & newID(1) & "') ORDER BY number_device DESC", conn
		if sql <> "" then 
			sql = sql & ", "
			text = text & ", "
		end if
		dim newIDtext
		if not rs.eof then
			newID = split(rs(0), "-")
			newIDtext = newID(0) & "-" & newID(1) & "-"
			if cstr(cint(newID(2)) + 1) < 10 then newIDtext = newIDtext & "0"
			newIDtext = newIDtext & cstr(cint(newID(2)) + 1)
		else
			newIDtext = newID(0) & "-" & newID(1) & "-01"			
		end if
		rs.close
		sql = sql & "number_device = '" & newIDtext & "'"
		conn.execute "UPDATE ELMEVENTS SET CNAME = '" & newIDtext & "' WHERE CNAME = '" & id & "'"
		conn.execute "UPDATE REMONT SET ID_D = '" & newIDtext & "' WHERE ID_D = '" & id & "'"
		text = text & "ID ���������� ������� c [" & id & "] �� [" & newIDtext & "]"
	end if

	if sql <> "" then 		
		sql = "UPDATE DEVICE SET " & sql & " WHERE (number_device = '" & id & "')"
		text = "��������� �������� ���������� " & name & " [" & id & "], ��������: " & text
		conn.execute sql
		sql = log(id, text)
		response.write text
		if isNewID then response.write "<br /><a href='/devin/device/?id=" & newIDtext & "'>�������� ��������, ����� �������� ID ���������� � ����� ������</a>"
	else 
		response.write "��������� �� ����"
	end if

	set rs = nothing
	conn.close	
	set conn = nothing
%>