<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim id : id = request.querystring("id")

	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim sql, key, Vold, Vnew, text, name, cls, ncard
	text = ""

	' ��������� ��������
	if not isnumeric(request.form("NCard")) then text = "���������� ����������� ����� �� �������� �������� ���������"
	if not isnumeric(request.form("Price")) then text = "���������� ��������� �� �������� �������� ���������"
	if not isnumeric(request.form("Nadd")) then text = "���������� ���-�� ��������� ������� �� �������� �������� ���������"
	if not isnumeric(request.form("Nis")) then text = "���������� ���-�� ������� �� ������ �� �������� �������� ���������"
	if not isnumeric(request.form("Nuse")) then text = "���������� ���-�� ������������ ������� �� �������� �������� ���������"
	if not isnumeric(request.form("Nbreak")) then text = "���������� ���-�� ��������� ������� �� �������� �������� ���������"
	if not isdate(request.form("Date")) then text = "������� ������������ ���� �������. ��������� ������ ��.��.����"
	if text <> "" then
		response.write "<div class='error'>" & text & "</div>"
		response.end
	end if

	' �������� ������ ������
	dim rs : set rs = server.createobject("ADODB.Recordset")
	rs.open "SELECT Name, NCard, class_name, Price, Nadd, Nis, Nuse, Nbreak, Date, uchet, ID_cart, G_ID FROM SKLAD WHERE (NCard = '" & id & "')", conn
	if not rs.eof then
		for each key in rs.fields
			Vold = key
			if isnull(Vold) then Vold = "" else Vold = trim(cstr(Vold))
			if key.name = "class_name" then cls = Vold
			if key.name = "Name" then name = Vold
			if (key.name <> "ID_cart") or (key.name = "ID_cart" and cls = "PRN") then
				Vnew = cstr(DecodeUTF8(request.form(key.name)))
				if key.name = "NCard" then ncard = Vnew
				if Vnew <> Vold then
					if text <> "" then
						text = text & ", "
						sql = sql & ", "
					end if
					text = text & key.name & " � [" & Vold & "] �� [" & Vnew & "]"
					if (key.name = "Date") then
						sql = sql & key.name & " = '" & DateToSql(Vnew) & "'"
					else
						sql = sql & key.name & " = '" & Vnew & "'"
					end if
				end if
			end if
		next
	else
		response.write "<div class='error'>��� ������ �� ������� ID</div>"
		response.end
	end if
	rs.close

	' �������� �� ������������ ������������ ������
	if ncard <> id then
		rs.open "SELECT NCard FROM SKLAD WHERE (NCard = '" & ncard & "')"
		if not rs.eof then
			rs.close
			set rs = nothing
			conn.close
			set conn = nothing
			response.write "<div class='error'>��������� ����������� ����� �� �������� ����������</div>"
			response.end
		end if
		rs.close
	end if

	' ���������� ������ ��� �������������
	if text <> "" then
		text = "��������� �������� ������� " & name & " [" & id & "], ��������: " & text
		response.write "<div class='done'>" & text & "</div>"
		'response.write "UPDATE SKLAD SET " & sql & " WHERE (Ncard = '" & id & "')"
		conn.execute "UPDATE SKLAD SET " & sql & " WHERE (Ncard = '" & id & "')"
		response.write log(id, text)
	else
		response.write "<div class='done'>��������� �� ����</div>"
	end if

	set rs = nothing
	conn.close
	set conn = nothing
%>