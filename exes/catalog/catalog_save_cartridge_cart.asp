<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs : set rs = server.createObject("ADODB.Recordset")
	dim id : id = replace(request.queryString("id"), "cart", "")
	conn.open everest

	rs.open "SELECT Caption, Price, Type, Color FROM CARTRIDGE WHERE N = " & id, conn
	if rs.eof then
		response.write "<div class='error'>�� ������� ������ �� ������� ID</div>"
	else
		dim val
		dim sql : sql = ""
		dim ifErr : ifErr = false

		' ������������ 
		val = DecodeUTF8(request.form("Caption"))
		if rs(0) <> val then
			sql = sql & "Caption = '" & val & "'"
		end if

		' ���������
		val = DecodeUTF8(request.form("Price"))
		if isnumeric(val) then
			if CCur(val) >= 0 then
				if rs(1) <> val or isnull(rs(1)) then
					if sql <> "" then sql = sql & ", "
					sql = sql & "Price = '" & val & "'"
				end if
			else 
				response.write "<div class='error'>������� ������������� �������� ���������. ��������� ������������� �����</div>"
				ifErr = true
			end if
		elseif val = "" then
			if sql <> "" then sql = sql & ", "
			sql = sql & "Price = NULL"
		else
			response.write "<div class='error'>������� ������������ �������� ���������. ��������� ������������� �����</div>"
			ifErr = true
		end if

		' ��� 
		val = request.form("Type")
		if rs(2) <> val or isnull(rs(2)) then
			if sql <> "" then sql = sql & ", "
			sql = sql & "Type = '" & val & "'"
		end if

		' ����
		val = request.form("Color")
		if rs(3) <> val or isnull(rs(3)) then
			if sql <> "" then sql = sql & ", "
			sql = sql & "Color = '" & val & "'"
		end if

		if not ifErr then
			if sql <> "" then
				conn.execute "UPDATE CARTRIDGE SET " & sql & " WHERE N = " & id
				response.write "<div class='done'>���������</div>"
			else
				response.write "<div class='done'>��� ���������</div>"
			end if
		end if		
	end if
	rs.close
	conn.close
	set rs = nothing
	set conn = nothing
%>