<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn, rs, sql, i

	set conn = server.createObject("ADODB.Connection")
	set rs = server.createObject("ADODB.Recordset")


	' ������ � ������ � ��������
	sql = "SELECT O_ID, O_Alias, O_Name, O_Data, O_Excel FROM catalog_writeoffs ORDER BY O_Name"

	conn.open everest
	rs.open sql, conn

	if rs.eof then
		' �������� �� �������
		response.write "<div class='cart-header'>������ �� ������!</div>" _
		& "<table class='cart-menu'><tr><td onclick='cartClose()'>�������</td></tr></table>"

	else
		' ����� ��������
		response.write "<div class='cart-header'>��������� �������� ��������</div><form id='form' method='post'><table class='cart-table'>"

		dim writeoff(4)
		do while not rs.eof
			for i = 0 to 4
				writeoff(i) = rs(i)
			next
			response.write "<tr><td>��������<td><input name='off" & writeoff(0) & "' value='" & writeoff(1) & "' /></tr>"
			response.write "<tr><td>���� �������<td><input name='off" & writeoff(0) & "' value='" & writeoff(1) & "' /></tr>"

		loop

		' ���� �������� � ���������� ��������
		response.write "</table></form><div id='console'></div>" _
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