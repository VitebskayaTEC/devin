<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim id : id = request.querystring("id")
	dim conn : set conn = server.createobject("ADODB.Connection")
	dim rs : set rs = server.createobject("ADODB.Recordset")
	dim sql : sql = ""
	dim text : text = ""

	conn.open everest
	rs.open "SELECT TOP (1) REMONT.Units, REMONT.IfSpis, REMONT.Virtual, SKLAD.NCard, REMONT.Date FROM REMONT LEFT OUTER JOIN SKLAD ON SKLAD.NCard = REMONT.ID_U WHERE (REMONT.INum = '" & id & "')", conn
	dim repair(4), i
	for i = 0 to 4
		repair(i) = rs(i)
	next
	rs.close

	dim ifspis : ifspis = request.form("ifspis")
	if ifspis <> cstr(repair(1)) then

		if text <> "" then text = text & ", "
		if ifspis = "1" then
			text = text & "������ ������� ��� ���������"
			if not isnull(repair(3)) then sql = sql & "UPDATE SKLAD SET Nbreak = Nbreak + " & repair(0) & ", Nuse = Nuse - " & repair(0) & " WHERE (NCard = '" & repair(3) & "') " & chr(13)

		else
			text = text & "������ ������� ��� ��������"
			if not isnull(repair(3)) then sql = sql & "UPDATE SKLAD SET Nbreak = Nbreak - " & repair(0) & ", Nuse = Nuse + " & repair(0) & " WHERE (NCard = '" & repair(3) & "') " & chr(13)

		end if
	end if

	dim virtual : virtual = request.form("virtual")
	if virtual <> cstr(repair(2)) then

		if text <> "" then text = text & ", "
		if virtual = "1" then
			if not isnull(repair(3)) then sql = sql & "UPDATE SKLAD SET Nis = Nis + " & repair(0) & " WHERE (NCard = '" & repair(3) & "') " & chr(13)
			text = text & "������ ������� ��� �����������"
		else
			if not isnull(repair(3)) then sql = sql & "UPDATE SKLAD SET Nis = Nis - " & repair(0) & " WHERE (NCard = '" & repair(3) & "') " & chr(13)
			text = text & "������ ������� ��� �� �����������"
		end if

	end if

	dim dateVal : dateVal = request.form("date")
	if isdate(dateVal) then
		if datevalue(dateVal) <> datevalue(repair(4)) then
			if text <> "" then text = text & ", "
			text = text & "���� ������� � " & datevalue(repair(4)) & " �� " & datevalue(dateVal)
		end if
	end if

	dim units : units = request.form("units")
	if isnumeric(units) then
		if cint(units) <> cint(repair(0)) then
			if text <> "" then text = text & ", "
			text = text & "���������� � [" & repair(0) & "] �� [" & units & "]"
			if not isnull(repair(3)) then
				if ifspis = "1" then
					if virtual = "1" then
						sql = sql & "UPDATE SKLAD SET Nbreak = Nbreak - " & repair(0) & " + " & units & " WHERE (NCard = '" & repair(3) & "')" & chr(13)
					else
						sql = sql & "UPDATE SKLAD SET Nis = Nis - " & units & " + " & repair(0) & ", Nbreak = Nbreak - " & repair(0) & " + " & units & " WHERE (NCard = '" & repair(3) & "')" & chr(13)
					end if
				else
					if virtual = "1" then
						sql = sql & "UPDATE SKLAD SET Nuse = Nuse - " & repair(0) & " + " & units & " WHERE (NCard = '" & repair(3) & "')" & chr(13)
					else
						sql = sql & "UPDATE SKLAD SET Nis = Nis - " & units & " + " & repair(0) & ", Nuse = Nuse - " & repair(0) & " + " & units & " WHERE (NCard = '" & repair(3) & "')" & chr(13)
					end if
				end if
			end if
		end if
	end if

	if text <> "" then

		sql = sql & "UPDATE REMONT SET Date = '" & DateToSql(request.form("date")) & "',  Units = " & units & ", IfSpis = " & ifspis & ", Virtual = " & virtual & " WHERE (INum = '" & id & "')" & chr(13)

		dim user : user = replace(request.servervariables("REMOTE_USER"), "VST\", "")
		text = "�������� ������ [repair" & id & "], ��������: " & text
		sql = sql & "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), 'repair" & id & "', '" & user & "', '������������� DEVIN', '" & text & "')"

		conn.execute sql
		response.write "<div class='done'>" & text & "</div>"
	else
		response.write "<div class='done'>��������� ���</div>"
	end if

	conn.close
	set rs = nothing
	set conn = nothing
%>