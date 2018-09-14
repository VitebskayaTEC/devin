<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn:          set conn      = server.createObject("ADODB.Connection")
	dim rs:            set rs        = server.createObject("ADODB.Recordset")
	dim key:           key           = array("name", "inventory", "attribute", "mol", "description", "name, class_device")
	dim direction:     direction     = array("", " DESC")
	dim sortKey:       sortKey       = request.queryString("key")
	dim sortDirection: sortDirection = request.queryString("direction")
	dim id:            id            = request.queryString("id")
	dim search:        search        = request.queryString("search")
	dim head:          head          = "<table class='items'><thead>" _
	& "<td width='20px'><input type='checkbox' class='selecter-all' />" _
	& "<th width='20px' data-type='type'>" _
	& "<th width='100px' data-type='string'>название" _
	& "<th width='80px' data-type='number'>инв. номер" _
	& "<th width='140px' data-type='string'>м.о.л." _
	& "<th data-type='string'>описание" _
	& "</tr></thead><tbody class='drop-source'>"

	if sortKey = "" or isnull(sortKey) then sortKey  = "5"
	if sortDirection  = "" or isnull(sortDirection) then sortDirection = "0"

	dim sql: sql = "SELECT " _
	& "DEVICE.inventory, " _
	& "DEVICE.class_device, " _
	& "DEVICE.number_device, " _
	& "DEVICE.name, " _
	& "DEVICE.number_comp, " _
	& "DEVICE.description, " _
	& "DEVICE.mol, " _
	& "DEVICE.attribute, " _
	& "DEVICE.used, " _
	& "[GROUP].G_ID " _
	& "FROM DEVICE " _
	& "LEFT OUTER JOIN [GROUP] ON [GROUP].G_ID = DEVICE.G_ID " _
	& "WHERE (DEVICE.deleted = 0) ORDER BY " & key(sortKey) & direction(sortDirection)

	if search <> "" then
		sql = replace(sql, "WHERE", replace("WHERE (DEVICE.inventory {0} OR DEVICE.class_device {0} OR DEVICE.number_device {0} OR DEVICE.name {0} OR DEVICE.number_comp {0} OR DEVICE.description1C {0} OR DEVICE.description {0} OR DEVICE.install_date {0} OR DEVICE.MOL {0} OR DEVICE.number_serial {0} OR DEVICE.number_passport {0} OR DEVICE.attribute {0}) AND ", "{0}", " LIKE '%" & search & "%'"))
	end if

	dim i, j, q
	dim led
	dim devices(1500, 9), Ndevices
	conn.open everest
	rs.open   sql, conn
	Ndevices = 0
		do while not rs.eof
			for i = 0 to 9
				devices(Ndevices,i) = trim(rs(i))
			next
			rs.MoveNext
			Ndevices = Ndevices + 1
		loop
	rs.close

	if search = "" then

		' Составление массива указателей на позиции-компьютеры
		dim computers(500, 1), Ncomputers
		Ncomputers = 0
		for i = 0 to Ndevices
			if devices(i, 1) = "CMP" then
				computers(Ncomputers, 0) = devices(i, 2)
				computers(Ncomputers, 1) = i
				Ncomputers = Ncomputers + 1
			end if
		next

		' Распределение элементов по компьютерам
		dim cookie, className
		for j = 0 to Ncomputers - 1
			cookie = request.cookies(computers(j, 0))
			if devices(computers(j, 1), 9) = "" or devices(computers(j, 1), 9) = "0" or devices(computers(j, 1), 9) = "-1" or isnull(devices(computers(j, 1), 9)) then
				className = ""
			else
				className = " hide_first"
			end if
            if devices(computers(j, 1), 8) = 1 then led = "on" else led = "off"
			response.write "<div class='unit computer " & cookie & className & "' in='dg" & devices(computers(j, 1), 9) & "'>" _
			& "<table class='caption'><tr>" _
			& "<td width='30px'><div class='icon ic-computer' menu='computer' onmousedown='_menu(this)'></div>" _
			& "<th width='200px'>" & devices(computers(j, 1), 3) _
            & "<td width='40px'><div class='led " & led & "'></div></td>" _
			& "<td width='60px'>" & devices(computers(j, 1), 0) _
			& "<td width='200px'>" & devices(computers(j, 1), 7) _
			& "<td width='140px'>" & devices(computers(j, 1), 6) _
			& "<td>" & devices(computers(j, 1), 5) _
			& "</tr></table>"

			if cookie = "open" then response.write "<div class='items_block'>" else response.write "<div class='items_block hide'>"

			response.write "<div class='title-wrapper' id='" & computers(j, 0) & "'>" _
			& "<div class='title'>Компьютер " & devices(computers(j, 1), 3) & " - открыть карточку</div></div>" & head
			for i = 0 to Ndevices
				if devices(i, 4) = computers(j, 0) then
					if devices(i, 9) = "" or devices(i, 9) = "0" or devices(i, 9) = "-1" or isnull(devices(i, 9)) then
						className = ""
					else
						className = "hide_first"
					end if
					if devices(i, 8) = 1 then led = "on" else led = "off"
					response.write "<tr id='" & devices(i, 2) & "' in='dg" & devices(i, 9) & "' class='item drop-el " & className & "'>" _
					& "<td><input type='checkbox' class='selecter' />" _
					& "<td><div class='led " & led & "'></div>" _
					& "<td>" & devices(i, 3) _
					& "<td>" & devices(i, 0) _
					& "<td>" & devices(i, 6) _
					& "<td>" & devices(i, 5) _
					& "</tr>"
				end if
			next
			response.write "</tbody></table></div></div>"
		next

		' Не распределенные элементы
		sql = "SELECT " _
		& "DEVICE.used, " _
		& "DEVICE.number_device, " _
		& "[GROUP].G_ID, " _
		& "DEVICE.name, " _
		& "DEVICE.inventory, " _
		& "DEVICE.MOL, " _
		& "DEVICE.description " _
		& "FROM DEVICE " _
		& "LEFT OUTER JOIN DEVICE AS DEVICE_1 ON DEVICE.number_comp = DEVICE_1.number_device " _
		& "LEFT OUTER JOIN [GROUP] ON [GROUP].G_ID = DEVICE.G_ID " _
		& "WHERE (DEVICE.deleted = 0) AND (DEVICE.class_device <> 'cmp') AND (DEVICE_1.name IS NULL)"

		rs.open sql, conn
		if not rs.eof then
			response.write "<div class='unit' id='solo'><table class='caption'><tr>" _
			& "<td width='24px'><div class='icon ic-cached'></div>" _
			& "<th>Не распределенные устройства" _
			& "</tr></table>" & head
			do while not rs.eof
				if rs(0) = 1 then led = "on" else led = "off"
				response.write "<tr id='" & trim(rs(1)) & "' in='dg" & rs(2) & "' class='item drop-el'>" _
				& "<td><input type='checkbox' class='selecter' />" _
				& "<td><div class='led " & led & "'></div>" _
				& "<td>" & trim(rs(3)) _
				& "<td>" & trim(rs(4)) _
				& "<td>" & trim(rs(5)) _
				& "<td>" & trim(rs(6)) _
				& "</tr>"
				rs.movenext
			loop
			response.write "</tbody></table></div>"
		end if
		rs.close

		' Получение списка групп
		dim groups(200, 2), Ngroups
		sql = "SELECT " _
		& "G_ID, " _
		& "G_Inside, " _
		& "G_Title " _
		& "FROM [GROUP] " _
		& "WHERE (G_Type = 'device') ORDER BY G_Title"
		'response.write "<div class='debug'>" & sql & "</div>"
		rs.open sql, conn
		if not rs.eof then
			do while not rs.eof
				for i = 0 to 2
					groups(Ngroups, i) = rs(i)
				next
				cookie = request.cookies("dg" & groups(Ngroups, 0))

				if groups(Ngroups, 1) = "" or groups(Ngroups, 1) = "0" or groups(Ngroups, 1) = "-1" or isnull(groups(Ngroups, 1)) then
					response.write "<div class='unit group " & cookie & "' id='dg" & groups(Ngroups, 0) & "' in='dg" & groups(Ngroups, 1) & "'>"
				else
					response.write "<div class='unit hide_first group " & cookie & "' id='dg" & groups(Ngroups, 0) & "' in='dg" & groups(Ngroups, 1) & "'>"
				end if
				response.write "<table class='caption'><tr>" _
				& "<td width='24px' menu='group' onmousedown='_menu(this)'><div class='icon ic-folder'></div>" _
				& "<th>" & groups(Ngroups, 2) _
				& "</tr></table>"
				if cookie = "open" then
					response.write "<div class='items_block'>" & head
				else
					response.write "<div class='items_block hide'>" & head
				end if
				response.write "</tbody></table></div></div>"

				Ngroups = Ngroups + 1
				rs.movenext
			loop
		end if
		rs.close

		' Скрипт для группировки позиций и составления списка доступных для перемещения контейнеров
		%>
		<script id='insert_move_select'>
			document.getElementById('move_select').innerHTML = "<select id='moveKey'><option value='0'>Разместить отдельно<%
				for i = 0 to Ncomputers - 1
					response.write "<option value='cmp" & computers(i, 0) & "'>[Компьютер] " & devices(computers(i, 1), 3)
				next
				for i = 0 to Ngroups - 1
					response.write "<option value='g" & groups(i, 0) & "'>[Группа] " & groups(i, 2)
				next
			%></select>";
			{
				var _in;
				$("tr.item,div.group,div.computer").each(function() {
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

		response.write"<div class='unit'><table class='caption'><tr><th>Поиск совпадений по запросу: " & search & "</tr></table>" & head
		for i = 0 to Ndevices - 1
            if (devices(i, 8) = 1) then led = "on" else led = "off"
			response.write "<tr id='" & devices(i, 2) & "'>" _
			& "<td><input type='checkbox' class='selecter' />" _
			& "<td><div class='led " & led & "'></div>" _
			& "<td>" & devices(i, 3) _
			& "<td>" & devices(i, 0) _
			& "<td>" & devices(i, 6) _
			& "<td>" & devices(i, 5) _
			& "</tr>"
		next
		response.write"</tbody></table></div>"

	end if

	conn.close
	set rs   = nothing
	set conn = nothing
%>