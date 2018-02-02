<!-- #include virtual ="/devin/core/core.inc" -->
<%
	function S_D(ss)
		dim b, i, s, s1, j, k, l
		dim classes : classes = array("CMP", "PRN", "DIS", "MOD", "SWT", "SCN", "UPS", "TO")
		for l = 0 to 7
			b = true
			s = ss
			ss = ""
			do while b
				i = InStr(s, "-" & classes(l) & "-")
				if i <> 0 then
					s1 = left(s, i + 6)
					j = i - 1
					do while (ascw(mid(s, j, 1)) <= 57) and (ascw(mid(s, j, 1)) >= 48) and (j > 1)
						j = j - 1
					loop
					s1 = mid(s1, j + 1, i + 6)
					ss = ss & left(s, j) & "<a href='/devin/device/?id=" & s1 & "'>" & s1 & "</a>"
					s = mid(s, i + 7, len(s))
				else
					b = false
				end if
			loop
			if ss="" then ss = s else ss = ss & s
		next
		S_D = s
	end function
%>
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
			conn.open everest
			dim rs : set rs = server.createobject("ADODB.Recordset")
			dim sql
			dim id : id = request.querystring("id")
			if request.querystring("t") = "id" then 
				sql = "SELECT EDATE, CNAME, CUSER, EVENTS FROM ELMEVENTS WHERE (EVGR = 'Администратор DEVIN') AND (CName = '" & id & "') ORDER BY EDATE DESC"
			else 
				sql = "SELECT EDATE, CNAME, CUSER, EVENTS FROM ELMEVENTS WHERE (CName = '" & id & "') ORDER BY EDATE DESC"
			end if
			rs.open sql, conn
			
				if not rs.eof then
					do while not rs.eof
						response.write "<tr><td>" & rs("EDATE") & "<td>" & trim(rs("CNAME")) & "<td>" & trim(rs("CUSER")) & "<td>" & S_D(trim(rs("EVENTS"))) & "</tr>"
						rs.movenext
					loop
				else
					response.write "<tr><th colspan='4'>Нет данных</tr>"
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