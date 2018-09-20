<!-- #include virtual ="/devin/core/core.inc" -->
<div class='cart-overflow cart-history-box'>
	<table class='cart-history' style="table-layout: inherit">
		<thead>
			<tr>
				<td colspan="3"><input type="text" onkeyup='_search(this, true)' /></td>
			</tr>
			<tr>
				<th width="90px" data-type='date' onclick='_sort(this)'>Дата</th>
				<th width="60px" data-type='string' onclick='_sort(this)'>Юзер</th>
				<th data-type='string' onclick='_sort(this)'>Событие</th>
			</tr>
		</thead>
		<tbody>
		<%
			dim conn : set conn = server.createobject("ADODB.Connection")
			dim rs   : set rs   = server.createobject("ADODB.Recordset")
			dim id   : id       = UCASE(request.querystring("id"))
			dim sql  : sql      = "SELECT EDATE, CUSER, EVENTS FROM ELMEVENTS WHERE CName = '" & UCase(id) & "' ORDER BY EDATE DESC"

			conn.open everest
			rs.open sql, conn
				if not rs.eof then
					do while not rs.eof
						response.write "<tr><td>" & rs("EDATE") & "<td>" & trim(rs("CUSER")) & "<td>" & S_D(trim(rs("EVENTS"))) & "</tr>"
						rs.movenext
					loop
				else
					response.write "<tr><td colspan='3'>Нет данных</tr>"
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