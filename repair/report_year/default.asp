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

	<% menu("") %>

	<div id='view' class='view'>
		<div id='summary'></div>
		<br />
	<%
		dim conn: set conn = server.createObject("ADODB.Connection")
		dim rs:   set rs   = server.createObject("ADODB.Recordset")
		dim sql

		conn.open everest

		sql = "SELECT " _
			& "w.W_Id              AS Id,  " _
			& "w.W_Date            AS [Date], " _
			& "w.W_Name            AS [Name], " _
			& "r.Inum              AS Repair_Id, " _
			& "d.number_device     AS Repair_Device_Id, " _
			& "d.name              AS Repair_Device_Name, " _
			& "d.description       AS Repair_Device_Description, " _
			& "s.Ncard             AS Repair_Storage_Id, " _
			& "s.Name              AS Repair_Storage_Name, " _
			& "r.Units             AS Repair_Count, " _
			& "(r.Units * s.Price) AS Repair_Price, " _
			& "r.Date              AS Repair_Date, " _
			& "r.Author            AS Repair_User " _
			& "FROM Writeoff w " _
			& "LEFT OUTER JOIN Remont r ON w.W_ID = r.W_ID " _
			& "LEFT OUTER JOIN Device d ON r.Id_D = d.number_device " _
			& "LEFT OUTER JOIN Sklad s ON r.Id_U = s.Ncard " _
			& "WHERE Year(w.W_Date) = Year(GetDate()) AND r.IfSpis = 1 " _
			& "ORDER BY [Date], Id, Repair_Date, Repair_Device_Id;"

		rs.open sql, conn
		if rs.eof then
			response.write "Данных нет"
		else
			dim head: head = "<table class='items'>" _
			& "<thead><tr>" _
			& "<th data-type='string'>объект ремонта</th>" _
			& "<th data-type='string' width='460px'>деталь</th>" _
			& "<th data-type='number' width='50px'>кол-во</th>" _
			& "<th data-type='number' width='80px'>стоимость</th>" _
			& "<th data-type='date' width='70px'>дата</th>" _
			& "<th data-type='string' width='100px'>автор</th>" _
			& "</tr></thead><tbody>"

			dim writeoff: writeoff = ""
			dim repair:   repair   = ""
			dim device:   device   = ""
			dim storage:  storage  = ""
			dim wId
			dim start: start = true
			do while not rs.eof
				wId = rs("Id")
				if wId <> writeoff then
					if not start then response.write "</tbody></table></div></div>"
					start = false

					response.write "<div class='unit' id='writeoff" & wId & "'><table class='caption'><tr>" _
						& "<td width='100px'>" & datevalue(rs("Date")) & "</td>" _
						& "<th width='800px'>" & rs("Name") & "</th>" _
						& "<td width='140px' count></td>" _
						& "<td cost></td>" _
						& "</tr></table><div class='items_block hide'>" & head

					writeoff = wId
				end if

				response.write "<tr id='repair" & rs("Repair_Id") & "'>" _
				& "<td id='device" & rs("Repair_Device_Id") & "'>" _
					& rs("Repair_Device_Name") _
					& ": " _
					& rs("Repair_Device_Description") _
				& "</td>" _
				& "<td id='storage" & rs("Repair_Storage_Id") & "'>" & rs("Repair_Storage_Name") & "</td>" _
				& "<td count>" & rs("Repair_Count") & "</td>" _
				& "<td cost>" & rs("Repair_Price") & "</td>" _
				& "<td>" & rs("Repair_Date") & "</td>" _
				& "<td>" & rs("Repair_User") & "</td>" _
				& "</tr>"

				rs.moveNext
			loop
			response.write "</tbody></table></div></div>"
		end if
		rs.close

		conn.close
		set rs   = nothing
		set conn = nothing
	%>
	</div>

	<script src='/devin/content/js/jquery-1.12.4.min.js'></script>
	<script src="/devin/content/js/jquery-ui.min.js"></script>
	<script src="/devin/content/js/core.js"></script>
	<script src="/devin/content/js/repair.js"></script>
	<script>
		var $units = $('.unit');
		var allcost = 0;
		$units.each(function () {
			var $unit = $(this);
			var $count = $unit.find('.caption td[count]');
			var $cost = $unit.find('.caption td[cost]');
			var cost = 0;
			var count = 0;
			$unit.find('.items tr').each(function () {
				var $row = $(this);
				count += +$row.find('td[count]').text();
				cost += +$row.find('td[cost]').text().replace(',', '.');
			});
			$count.text('Кол-во: ' + count + ' шт.');
			$cost.text('Стоимость: ' + cost.toFixed(2) + ' р.');
			allcost += cost;
		});
		$('#summary').text('Стоимость: ' + allcost.toFixed(2) + ' р.');
	</script>

</BODY>

</HTML>