<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn, rs, sql, i

	dim id : id = request.querystring("id")

	set conn = server.createObject("ADODB.Connection")
	set rs = server.createObject("ADODB.Recordset")


	' ������ � ������ � ��������
	sql = "SELECT Name, Type, Date, Params, Description, FolderId, LastExcel, LastExcelDate, CostArticle FROM Writeoffs WHERE (Id = " & id & ")"

	conn.open everest
	rs.open sql, conn

	if rs.eof then
		' �������� �� �������
		response.write "<div class='cart-header'>������ �� ������!</div>" _
		& "<table class='cart-menu'><tr><td onclick='cartClose()'>�������</td></tr></table>"
		rs.close
	else
		' ������ ������ � ������ ��� ���������� ���������
		dim writeoff(8)
		for i = 0 to 8
			writeoff(i) = rs(i)
		next
		rs.close

		dim defParams : defParams = ""
		dim defNParams : defNParams = 0

		' ����� ��������
		response.write "<div class='cart-header'>" & writeoff(0) & "</div><form id='form' method='post'><table class='cart-table'>"

		' ���������������� ����� ������ � �������
		' ������������
		response.write "<tr><td>������������<td><input name='Name' value='" & writeoff(0) & "' /></tr>"

		' ��� ��������
		response.write "<tr><td>���<td>"
		rs.open "SELECT O_Alias AS O_Index, O_Alias, O_Name, O_Data FROM catalog_writeoffs ORDER BY O_Name", conn
		if rs.eof then
			response.write "��� ������� � �����������"
		else
			response.write "<select name='Type'><option value='0'>?"
			do while not rs.eof
				if rs("O_Index") = writeoff(1) then
					response.write "<option selected value='" & rs("O_Alias") & "'>" & rs("O_Name")
					defParams = rs("O_Data")
				else
					response.write "<option value='" & rs("O_Alias") & "'>" & rs("O_Name")
				end if
				rs.movenext
			loop
			response.write "</select>"
		end if
		rs.close
		response.write "</tr>"

		' ���� �������� ��������
		response.write "<tr><td>���� ��������<td><input name='Date' value='" & datevalue(writeoff(2)) & "' style='width: 100px' /></tr>"

		' ��������� ��� �������� � Excel, ���� �� ���, ����� ����� ����������� �� �����������
		dim params, Nparams
		if isnull(writeoff(3)) then
			Nparams = 0
		elseif instr(writeoff(3), ";;") = 0 then
			Nparams = 0
		else
			params = split(writeoff(3), ";;")
			Nparams = ubound(params)
		end if
		if not isnull(defParams) then
			if instr(defParams, ";;") > 0 then
				defParams = split(defParams, ";;")
				defNParams = ubound(defParams)
			end if
		end if
		if defNParams > 0 then
			for i = 0 to defNParams
				if defParams(i) <> "" then response.write "<tr><td>" & defParams(i) else response.write "<tr><td>{����������� ��������}"
				if not (i > Nparams) and (Nparams <> 0) then
					response.write "<td><input name='params" & i & "' value='" & params(i) & "' /></tr>"
				else
					response.write "<td><input name='params" & i & "' value='' /></tr>"
				end if
			next
		else
			response.write "<tr><td>��������� ��������<td>� ����������� �� ���������� ���� ��� ����������</tr>"
		end if

		' �������� ��������
		response.write "<tr><td>��������<td><textarea name='Description'>" & writeoff(4) & "</textarea></tr>"

		' ������ ��������
		if writeoff(1) <> "expl" then
			response.write "<tr><td>������ ��������</td><td><select name='CostArticle'><option value='0'>�� �������</option>"
			if writeoff(8) = 1 then response.write "<option value='1' selected>���������������� �������</option>" else response.write "<option value='1'>���������������� �������</option>"
			if writeoff(8) = 2 then response.write "<option value='2' selected>���. �������</option>" else response.write "<option value='2'>���. �������</option>"
			if writeoff(8) = 3 then response.write "<option value='3' selected>��� ���</option>" else response.write "<option value='3'>��� ���</option>"
		end if

		' ������
		response.write "<tr><td>������<td>"
		rs.open "SELECT Id AS O_Index, Id, Name FROM Folders WHERE (Type = 'repair') ORDER BY Name", conn
		if rs.eof then
			response.write "��� ������� � �����������"
		else
			response.write "<select name='FolderId'><option value='0'>?"
			do while not rs.eof
				if rs(0) = writeoff(5) then
					response.write "<option selected value='" & rs(1) & "'>" & rs(2)
				else
					response.write "<option value='" & rs(1) & "'>" & rs(2)
				end if
				rs.movenext
			loop
			response.write "</select>"
		end if
		rs.close
		response.write "</tr>"

		if writeoff(7) <> "" and not isnull(writeoff(7)) then
			' ������ �� ��������� ��������� ��� �������� Excel ����
			response.write "<tr><td>������ �� ����������<td><a href='/devin/excels/" & writeoff(6) & "'>" & writeoff(6) & "</a></tr>"

			' ���� ���������� �������� � Excel
			response.write "<tr><td>��������� ������<td>" & writeoff(7) & "</tr>"
		end if

		' ���� �������� � ���������� ��������
		response.write "</table></form><div id='console'></div>" _
		& "<table class='cart-menu'><tr>" _
		& "<td onclick='cartDelete()'>�������</td>" _
		& "<td onclick='cartSave()'>���������</td>" _
		& "<td onclick='writeoffExport()'>������</td>" _
		& "<td onclick='cartClose()'>�������</td>" _
		& "</tr></table>"

	end if

	conn.close
	set rs = nothing
	set conn = nothing
%>