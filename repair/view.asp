<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs:   set rs   = server.createObject("ADODB.Recordset")

	' Шапка таблицы в группе, определяет отображение позиций (ремонтов)
	dim head: head = "<table class='items'>" _
	& "<thead><tr>" _
	& "<td width='20px'><input type='checkbox' class='selecter-all' />" _
	& "<th data-type='string'>объект ремонта" _
	& "<th data-type='string' width='460px'>деталь" _
	& "<th data-type='number' width='50px'>кол-во" _
	& "<th data-type='number' width='70px'>стоимость" _
	& "<th data-type='date' width='70px'>дата" _
	& "<th data-type='string' width='40px'>спис." _
	& "<th data-type='string' width='40px'>вирт." _
	& "</tr></thead><tbody>"

	sub render(k)
		dim check(2)
		dim storageLed
		if cint(repairs(k, 10)) <> cint(repairs(k, 11)) + cint(repairs(k, 12)) + cint(repairs(k, 13)) then
			storageLed = " warning"
		elseif cint(repairs(k, 10)) = cint(repairs(k, 13)) then
			storageLed = " off"
		elseif cint(repairs(k, 11)) = 0 then
			storageLed = " onwork"
		elseif cint(repairs(k, 12)) > 0 then
			storageLed = " onwork"
		else
			storageLed = ""
		end if

		if repairs(k, 8) = "1" then check(0) = "checked" else check(0) = ""
		if repairs(k, 9) = "1" then check(1) = "checked" else check(1) = ""
		if repairs(k, 0) = "" or repairs(k, 0) = "0" or repairs(k, 0) = "-1" or isnull(repairs(k, 0)) then check(2) = "" else check(2) = " hide_first"
		response.write "<tr id='" & repairs(k, 2) & "' in='rg" & repairs(k, 0) & "' class='item " & check(2) & "'>" _
			& "<td><input type='checkbox' class='selecter' />" _
			& "<td><b>" & repairs(k, 3) & "</b> " & repairs(k, 4) _
			& "<td><div class='led" & storageLed & "'></div> " & repairs(k, 5) _
			& "<td>" & repairs(k, 6) _
			& "<td>" & (repairs(k, 14) * repairs(k, 6)) & " р." _
			& "<td>" & datevalue(repairs(k, 7)) _
			& "<td><input type='checkbox' disabled " & check(0) & " />" _
			& "<td><input type='checkbox' disabled " & check(1) & " />" _
			& "</tr>"
	end sub

	dim writeoff(200, 4), repairs(1000, 14), Nwriteoff, Nrepairs, i, j, sql

	conn.open everest

	dim search : search = DecodeUTF8(request.querystring("text"))

	if search = "" then

		' Получение списка групп
		sql = "SELECT G_ID, G_Inside, G_Title FROM [GROUP] WHERE (G_Type = 'repair')"
		rs.open sql, conn

		dim groups(300, 3), Ngroups
		Ngroups = 0
		do while not rs.eof
			groups(Ngroups, 0) = rs(0)
			groups(Ngroups, 1) = rs(1)
			groups(Ngroups, 2) = rs(2)

			Ngroups = Ngroups + 1
			rs.movenext
		loop
		rs.close

		' Получение списка всех списаний
		sql = "" _
		& " SELECT " _
		& " [GROUP].G_ID, writeoff.W_ID, writeoff.W_Name, catalog_writeoffs.O_Name, writeoff.W_Date" _
		& " FROM writeoff" _
		& " LEFT OUTER JOIN catalog_writeoffs ON writeoff.W_Type = catalog_writeoffs.O_Alias" _
		& " LEFT OUTER JOIN [GROUP] ON writeoff.G_ID = [GROUP].G_ID" _
		& " ORDER BY writeoff.W_Date DESC"
		rs.open sql, conn
		if not rs.eof then
			Nwriteoff = 0
			do while not rs.eof
				for i = 0 to 4
					writeoff(Nwriteoff, i) = trim(rs(i))
				next
				Nwriteoff = Nwriteoff + 1
				rs.movenext
			loop
			Nwriteoff = Nwriteoff - 1
		end if
		rs.close

		' Получение списка всех ремонтов
		sql = "SELECT " _
		& "[GROUP].G_ID, " _
		& "writeoff.W_ID, " _
		& "REMONT.INum, " _
		& "DEVICE.name AS Device, " _
		& "DEVICE.description, " _
		& "SKLAD.Name, " _
		& "REMONT.Units, " _
		& "REMONT.Date, " _
		& "REMONT.IfSpis, " _
		& "REMONT.Virtual, " _
		& "SKLAD.Nadd, " _
		& "SKLAD.Nis, " _
		& "SKLAD.Nuse, " _
		& "SKLAD.Nbreak, " _
		& "SKLAD.Price " _
		& "FROM REMONT " _
		& "LEFT OUTER JOIN DEVICE ON REMONT.ID_D = DEVICE.number_device " _
		& "LEFT OUTER JOIN SKLAD ON REMONT.ID_U = SKLAD.NCard " _
		& "LEFT OUTER JOIN [GROUP] ON REMONT.G_ID = [GROUP].G_ID " _
		& "LEFT OUTER JOIN writeoff ON REMONT.W_ID = writeoff.W_ID " _
		& "ORDER BY [GROUP].G_ID, writeoff.W_ID, REMONT.Date DESC"
		rs.open sql, conn
		if not rs.eof then
			rs.movefirst
			Nrepairs = 0
			do while not rs.eof
				for i = 0 to 14
					repairs(Nrepairs, i) = trim(rs(i))
				next
				rs.movenext
				Nrepairs = Nrepairs + 1
			loop
			Nrepairs = Nrepairs - 1
		end if
		rs.close


		' Отображение нераспределенных ремонтов
		response.write "<div class='unit' id='solo'>" _
			& "<table class='caption'><tr>" _
			& "<td width='24px'><div class='icon ic-cached'></div>" _
			& "<th>Не распределенные ремонты" _
			& "</tr></table><div class='items_block'>" _
			& head
		for i = 0 to Nrepairs
			if isnull(repairs(i, 1)) then render(i)
		next
		response.write "</tbody></table></div></div>"


		' Отображение списаний с вложенными ремонтами
		dim cookie, inGroup, allCost
		for i = 0 to Nwriteoff
			allCost = 0
			cookie = request.cookies("off" & writeoff(i, 1))
			if writeoff(i, 0) = "" or writeoff(i, 0) = "0" or writeoff(i, 0) = "-1" or isnull(writeoff(i, 0)) then inGroup = "" else inGroup = " hide_first"

			for j = 0 to Nrepairs
				if repairs(j, 1) = writeoff(i, 1) then allCost = allCost + (repairs(j, 6) * repairs(j, 14))
			next
			response.write "<div class='unit writeoff " & cookie & inGroup & "' in='rg" & writeoff(i, 0) & "'>" _
				& "<table class='caption'><tr>" _
				& "<td width='24px' menu='writeoff' onmousedown='_menu(this)'><div class='icon ic-info-outline'></div>" _
				& "<th>" & writeoff(i, 2) _
				& "<td width='250px'>" & writeoff(i, 3) _
				& "<td width='150px'>" & allCost & " р." _
				& "<td width='70px'>" & datevalue(writeoff(i, 4)) _
				& "</tr></table>"
			if cookie = "open" then response.write "<div class='items_block'>" else response.write "<div class='items_block hide'>"
			response.write "<div class='title-wrapper' id='off" & writeoff(i, 1) & "'><div class='title'>Открыть карточку списания</div></div>" _
				& head
			for j = 0 to Nrepairs
				if repairs(j, 1) = writeoff(i, 1) then render(j)
			next
			response.write "</tbody></table></div></div>"
		next

		' Отображение групп
		for i = 0 to Ngroups - 1
			cookie = request.cookies("rg" & groups(i, 0))
			if groups(i, 1) = "" or groups(i, 1) = "0" or groups(i, 1) = "-1" or isnull(groups(i, 1)) then
				response.write "<div class='unit group " & cookie & "' id='rg" & groups(i, 0) & "' in='rg" & groups(i, 1) & "'>"
			else
				response.write "<div class='unit hide_first group " & cookie & "' id='rg" & groups(i, 0) & "' in='rg" & groups(i, 1) & "'>"
			end if
			response.write "<table class='caption'><tr>" _
							& "<td width='24px' menu='group' onmousedown='_menu(this)'><div class='icon ic-folder'></div>" _
							& "<th>" & groups(i, 2) _
							& "</tr></table>"
			if cookie = "open" then
				response.write "<div class='items_block'>" & head
			else
				response.write "<div class='items_block hide'>" & head
			end if
			response.write "</tbody></table></div></div>"
		next

		' Скрипт для группировки позиций и составления списка доступных для перемещения контейнеров
		%>
		<script id='insert_move_select'>
			document.getElementById('move_select').innerHTML = "<select id='moveKey'><option value='0'>Разместить отдельно<%
				for i = 0 to Nwriteoff - 1
					response.write "<option value='w" & writeoff(i, 1) & "'>[Списание] " & replace(writeoff(i, 2), """", "\""")
				next
				for i = 0 to Ngroups - 1
					response.write "<option value='g" & groups(i, 0) & "'>[Группа] " & replace(groups(i, 2), """", "\""")
				next
			%></select>";
			{
				var _in;
				$("tr.item,div.group,div.writeoff").each(function() {
					_in = this.getAttribute('in');
					if (_in.slice(2) != '0') {
						if ($(this).hasClass('item')) {
							$("#" + _in).children(".items_block").children("table").find("tbody").append(this);
						} else {
							$(this).insertBefore($("#" + _in).children(".items_block").children("table").first());
						}
					}
				});
				for(var i = 0, items = document.querySelectorAll("table.items"); i < items.length; i++) {
					if (items[i].getElementsByTagName("tr").length < 2) items[i].className += " hide";
				}
				$(".hide_first").removeClass("hide_first");
			};
			document.getElementById('insert_move_select').parentNode.removeChild(document.getElementById('insert_move_select'));
		</script>
		<%

	else

		response.write "<div class='unit'><table class='caption'><tr><th>Поиск совпадений по запросу: " & search & "</tr></table>" & head

		' Получение списка всех ремонтов
		sql = "SELECT " _
		& "[GROUP].G_ID, " _
		& "writeoff.W_ID, " _
		& "REMONT.INum, " _
		& "DEVICE.name AS Device, " _
		& "DEVICE.description, " _
		& "SKLAD.Name, " _
		& "REMONT.Units, " _
		& "REMONT.Date, " _
		& "REMONT.IfSpis, " _
		& "REMONT.Virtual, " _
		& "SKLAD.Nadd, " _
		& "SKLAD.Nis, " _
		& "SKLAD.Nuse, " _
		& "SKLAD.Nbreak " _
		& "FROM REMONT " _
		& "LEFT OUTER JOIN DEVICE ON REMONT.ID_D = DEVICE.number_device " _
		& "LEFT OUTER JOIN SKLAD ON REMONT.ID_U = SKLAD.NCard " _
		& "LEFT OUTER JOIN [GROUP] ON REMONT.G_ID = [GROUP].G_ID " _
		& "LEFT OUTER JOIN writeoff ON REMONT.W_ID = writeoff.W_ID " _
		& "WHERE DEVICE.name {0} OR DEVICE.description {0} OR DEVICE.number_device {0} OR SKLAD.Name {0} OR SKLAD.Ncard {0} " _
		& "ORDER BY REMONT.Date DESC"
		rs.open replace(sql, "{0}", " LIKE '%" & search & "%'"), conn
		if not rs.eof then
			rs.movefirst
			Nrepairs = 0
			do while not rs.eof
				for i = 0 to 13
					repairs(Nrepairs, i) = trim(rs(i))
				next
				rs.movenext
				render(Nrepairs)
				Nrepairs = Nrepairs + 1
			loop
			Nrepairs = Nrepairs - 1
			response.write "</tbody></table></div>"
		else
			response.write "<div class='error'>Совпадений нет</div>"
		end if
		rs.close

		response.write "</tbody></table>"

	end if

	conn.close
	set rs   = nothing
	set conn = nothing
%>