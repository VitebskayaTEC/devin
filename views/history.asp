<!-- #include virtual ="/devin/core/core.inc" -->
<div class='cart-header'>История событий DEVIN</div>

<div class='cart-overflow'><table class='cart-history small-text'>
<col width="140"><col width="120"><col width="70"><col width="700">
<thead>
	<tr>
		<th data-type='date' onclick='_sort(this)'>Дата
		<th data-type='string' onclick='_sort(this)'>Объект
		<th data-type='string' onclick='_sort(this)'>Юзер
		<th data-type='string' onclick='_sort(this)'>Событие
	</tr>
	<tr>
		<td><input onkeyup='_search(this)' />
		<td><input onkeyup='_search(this)' />
		<td><input onkeyup='_search(this)' />
		<td><input onkeyup='_search(this)' />
	</tr>
</thead>
<tbody>
	<%
		dim key : key = request.queryString("key")
		dim view : view = request.queryString("view")
		dim sql
		select case view
			case "cname"
				sql = "SELECT EDATE, CNAME, CUSER, EVENTS FROM ELMEVENTS WHERE (EVGR = 'Администратор DEVIN') AND (CName = '" & key & "') ORDER BY EDATE DESC"
			case "like"
				sql = "SELECT EDATE, CNAME, CUSER, EVENTS FROM ELMEVENTS WHERE (EVGR = 'Администратор DEVIN') AND (EVENTS LIKE '%" & key & "%') ORDER BY EDATE DESC"
			case else
				sql = "SELECT EDATE, CNAME, CUSER, EVENTS FROM ELMEVENTS WHERE (EVGR = 'Администратор DEVIN') ORDER BY EDATE DESC"
		end select

		dim conn : set conn = server.createObject("ADODB.Connection")
		dim rs : set rs = server.CreateObject("ADODB.Recordset")

		conn.open everest
		rs.open sql, conn
		if not rs.eof then
			do while not rs.eof
				response.write "<tr><td>" & rs(0) & "<td>" & S_D(rs(1)) & "<td>" & rs(2) & "<td>" & S_D(rs(3)) & "</tr>"
				rs.movenext
			loop
			'response.write rs.getString(, , "<td>", "</tr><tr><td>", "")
		else
			response.write "<tr><th colspan='4'>Нет данных</tr>"
		end if
		rs.close
		conn.close
		set rs = nothing
		set conn = nothing
	%>
</tbody>
</table></div>

<table class='cart-menu'><tr>
	<td onclick='cartClose()'>Закрыть</td>
</tr></table>