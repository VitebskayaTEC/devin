<!-- #include virtual ="/devin/core/core.inc" -->
<div class='cart-overflow cart-history-box'>
	<table class='cart-history'>
		<thead>
			<tr>
				<td width='140px'><input type="text" onkeyup='_find()' />
				<td width='110px'><input type="text" onkeyup='_find()' />
				<td><input type="text" onkeyup='_find()' />
				<td width='60px'><input type="text" onkeyup='_find()' />
			</tr>
			<tr>
				<th data-type='date' onclick='_sort(this)'>����
				<th data-type='string' onclick='_sort(this)'>������
				<th data-type='string' onclick='_sort(this)'>������������� ������
				<th data-type='number' onclick='_sort(this)'>���-��
			</tr>
		</thead>
		<tbody>
		<%
			dim conn : set conn = server.createobject("ADODB.Connection")
			dim rs : set rs = server.createobject("ADODB.Recordset")
			dim sql : sql = "SELECT Repairs.Date, Repairs.Author, Storages.Name, Repairs.Number FROM Repairs INNER JOIN Storages ON Repairs.StorageInventory = Storages.Inventory WHERE (Repairs.DeviceId = '" & request.querystring("id") & "') ORDER BY Repairs.Date DESC"

			conn.open everest
			rs.open sql, conn
				if not rs.eof then
					do while not rs.eof
						response.write "<tr><td>" & rs("Date") & "<td>" & trim(rs("Author")) & "<td>" & trim(rs("Name")) & "<td>" & trim(rs("Number")) & "</tr>"
						rs.movenext
					loop
				else
					response.write "<tr><td colspan='4'>�� ������� �� ������ �������, ������������ �� ������ ����������</tr>"
				end if
			rs.close
			set rs = nothing
			conn.close
			set conn = nothing
		%>
		</tbody>
	</table>
</div>

<table class='cart-menu'>
	<tr>
		<td onclick='cartBack()'>�����
		<td onclick='cartClose()'>�������
	</tr>
</table>