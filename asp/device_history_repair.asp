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
				<th data-type='date' onclick='_sort(this)'>Дата
				<th data-type='string' onclick='_sort(this)'>Создал
				<th data-type='string' onclick='_sort(this)'>Установленная деталь
				<th data-type='number' onclick='_sort(this)'>Кол-во
			</tr>
		</thead>
		<tbody>
		<%
			dim conn : set conn = server.createobject("ADODB.Connection")
			dim rs : set rs = server.createobject("ADODB.Recordset")
			dim sql : sql = "SELECT REMONT.Date, REMONT.Author, SKLAD.Name, REMONT.Units FROM REMONT INNER JOIN SKLAD ON REMONT.ID_U = SKLAD.NCard WHERE (REMONT.ID_D = '" & request.querystring("id") & "') ORDER BY REMONT.Date DESC"

			conn.open everest
			rs.open sql, conn
				if not rs.eof then
					do while not rs.eof
						response.write "<tr><td>" & rs("Date") & "<td>" & trim(rs("Author")) & "<td>" & trim(rs("Name")) & "<td>" & trim(rs("Units")) & "</tr>"
						rs.movenext
					loop
				else
					response.write "<tr><td colspan='4'>Не найдено ни одного ремонта, оформленного на данное устройство</tr>"
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
		<td onclick='cartBack()'>Назад
		<td onclick='cartClose()'>Закрыть
	</tr>
</table>