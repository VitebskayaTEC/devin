<!-- #include virtual ="/devin/core/core.inc" -->
<div class='cart-overflow cart-history-box'>
	<table class='cart-history'>
		<thead>
			<tr>
				<td width='140px'><input type="text" onkeyup='_search(this)' />
				<td width='110px'><input type="text" onkeyup='_search(this)' />
				<td width='60px'><input type="text" onkeyup='_search(this)' />
				<td><input type="text" onkeyup='_search(this)' />
			</tr>
			<tr>
				<th data-type='date' onclick='_sort(this)'>Дата
				<th data-type='string' onclick='_sort(this)'>Объект
				<th data-type='string' onclick='_sort(this)'>Юзер
				<th data-type='string' onclick='_sort(this)'>Событие
			</tr>
		</thead>
		<tbody>
		<%
			dim conn : set conn = server.createobject("ADODB.Connection")
			dim rs   : set rs   = server.createobject("ADODB.Recordset")
			dim id   : id       = UCASE(request.querystring("id"))
			dim sql  : sql      = "SELECT EDATE, CUSER, EVENTS FROM ELMEVENTS WHERE UPPER(CName) = '" & id & "' ORDER BY EDATE DESC"

			conn.open everest
			rs.open sql, conn
				if not rs.eof then
					do while not rs.eof
						response.write "<tr><td>" & rs("EDATE") & "<td>" & trim(rs("CUSER")) & "<td>" & S_D(trim(rs("EVENTS"))) & "</tr>"
						rs.movenext
					loop
				else
					response.write "<tr><td colspan='4'>Нет данных</tr>"
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