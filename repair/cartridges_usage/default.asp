<!-- #include virtual ="/devin/core/core.inc" -->
<!DOCTYPE html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<link href="/devin/content/css/core.css" rel="stylesheet" type="text/css" />
	<link href="/devin/content/css/jquery-ui.min.css" rel="stylesheet" type="text/css" />
	<link href="/devin/content/css/repair.css" rel="stylesheet" type="text/css" />
	<link href="/devin/content/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | Годовой отчет по ремонтам</title>
</HEAD>

<BODY>

	<%
		menu("")

		dim y: y = request.queryString("year")
		if isNull(y) or y = "" then y = year(date)

		dim i, j
	%>

	<div id='view' class='view'>
		<form action="/devin/repair/cartridges_usage/" method="get" class="filter">
			Сводная таблица по заменам картриджей на
			<select name="year">
				<%
					dim selected
					for i = 2016 to year(date)
						if CStr(i) = y then selected = "selected" else selected = ""
						response.write "<option value='" & i & "' " & selected & ">" & i & "</option>"
					next
				%>
			</select>
			 год &emsp;&emsp;&emsp;
			<input type="submit" value="Запрос данных" />
		</form>
		<br />
	<%
		dim conn: set conn = server.createObject("ADODB.Connection")
		dim rs:   set rs   = server.createObject("ADODB.Recordset")
		dim sql
		dim tempName, tempLocation, tempId, tempDate, tempCount, allCount
		dim currentName
		dim dataLength, data(1000, 13)

		conn.open everest

		sql = "SELECT c.Caption AS [Name], c.N AS [Id], r.Date AS [Date], r.Units AS [Count] FROM Remont r LEFT OUTER JOIN Sklad s LEFT OUTER JOIN Cartridge c ON c.N = s.ID_cart ON s.Ncard = r.ID_U WHERE s.class_name = 'PRN' AND Year(r.Date) = " & y & " ORDER BY c.Caption"

		rs.open sql, conn

		if not rs.eof then
	%>

		<table class='stats'>
			<thead>
				<tr>
					<th data-type='string'>Тип картриджа</th>
					<th data-type='number' width='50px'>Январь</th>
					<th data-type='number' width='50px'>Февраль</th>
					<th data-type='number' width='50px'>Март</th>
					<th data-type='number' width='50px'>Апрель</th>
					<th data-type='number' width='50px'>Май</th>
					<th data-type='number' width='50px'>Июнь</th>
					<th data-type='number' width='50px'>Июль</th>
					<th data-type='number' width='50px'>Август</th>
					<th data-type='number' width='50px'>Сентябрь</th>
					<th data-type='number' width='50px'>Октябрь</th>
					<th data-type='number' width='50px'>Ноябрь</th>
					<th data-type='number' width='50px'>Декабрь</th>
					<th data-type='number' width='50px'>Всего</th>
				</tr>
			</thead>
			<tbody>
			<%
				dataLength = -1
				do while not rs.eof

					tempName  = rs("Name")
					tempId    = rs("Id")
					tempDate  = rs("Date")
					tempCount = rs("Count")

					if tempName <> currentName then

						dataLength  = dataLength + 1
						currentName = tempName

						data(dataLength, 0)  = tempName
						data(dataLength, 13) = tempId

						for i = 1 to 12
							data(dataLength, i) = 0
						next

					end if

					data(dataLength, month(tempDate)) = data(dataLength, month(tempDate)) + tempCount

					rs.moveNext
				loop

				allCount = 0
				for i = 0 to dataLength
					response.write "<tr><td><a href='/devin/catalog/##cart" & data(i, 13) & "'>" & data(i, 0) & "</a></td>"
					tempCount = 0
					for j = 1 to 12
						if data(i, j) > 0 then
							response.write "<td>" & data(i, j) & "</td>"
							tempCount = tempCount + data(i, j)
						else
							response.write "<td></td>"
						end if
					next
					response.write "<td>" & tempCount & "</td></tr>"
					allCount = allCount + tempCount
				next

				response.write "<tr class='summary'><td colspan='13'>Итого</td><td>" & allCount & "</td></tr>"
			%>
			</tbody>
		</table>
		<br />

	<%
		end if
		rs.close

		sql = "SELECT d.Description AS [Name], d.Attribute AS [Location], d.number_device AS [Id], r.Date AS [Date], r.Units AS [Count] FROM Remont r LEFT OUTER JOIN Device d ON d.number_device = r.ID_D WHERE d.class_device = 'PRN' AND Year(r.Date) = " & y & " ORDER BY d.Description"

		rs.open sql, conn
		if not rs.eof then
	%>

		<table class='stats'>
			<thead>
				<tr>
					<th data-type='string'>Устройство<br /><small>Расположение</small></th>
					<th data-type='number' width='50px'>Январь</th>
					<th data-type='number' width='50px'>Февраль</th>
					<th data-type='number' width='50px'>Март</th>
					<th data-type='number' width='50px'>Апрель</th>
					<th data-type='number' width='50px'>Май</th>
					<th data-type='number' width='50px'>Июнь</th>
					<th data-type='number' width='50px'>Июль</th>
					<th data-type='number' width='50px'>Август</th>
					<th data-type='number' width='50px'>Сентябрь</th>
					<th data-type='number' width='50px'>Октябрь</th>
					<th data-type='number' width='50px'>Ноябрь</th>
					<th data-type='number' width='50px'>Декабрь</th>
					<th data-type='number' width='50px'>Всего</th>
				</tr>
			</thead>
			<tbody>
			<%
				dataLength = -1
				do while not rs.eof

					tempName     = rs("Name")
					tempLocation = rs("Location")
					tempId       = rs("Id")
					tempDate     = rs("Date")
					tempCount    = rs("Count")

					if tempName <> currentName then

						dataLength  = dataLength + 1
						currentName = tempName

						data(dataLength, 0)  = tempName & "<br /><small>" & tempLocation & "</small>"
						data(dataLength, 13) = tempId

						for i = 1 to 12
							data(dataLength, i) = 0
						next

					end if

					data(dataLength, month(tempDate)) = data(dataLength, month(tempDate)) + tempCount

					rs.moveNext
				loop

				for i = 0 to dataLength
					response.write "<tr><td><a href='/devin/devices/##" & data(i, 13) & "'>" & data(i, 0) & "</a></td>"
					tempCount = 0
					for j = 1 to 12
						if data(i, j) > 0 then
							response.write "<td>" & data(i, j) & "</td>"
							tempCount = tempCount + data(i, j)
						else
							response.write "<td></td>"
						end if
					next
					response.write "<td>" & tempCount & "</td></tr>"
				next
			%>
			</tbody>
		</table>
		<br />

	<%
		end if
		rs.close

		conn.close
		set rs   = nothing
		set conn = nothing
	%>
	</div>

	<script src='/devin/content/js/jquery-1.12.4.min.js'></script>
	<script src="/devin/content/js/core.js"></script>
</BODY>

</HTML>