<!-- #include virtual ="/devin/core/core.inc" -->
<%
	sub renderItem(arr)
		if arr(9) = "" or arr(9) = "0" or arr(9) = "-1" or isnull(arr(9)) then
			response.write "<tr class='item' in='sg" & arr(9) & "' id='" & arr(0) & "'><th><input type='checkbox' class='selecter' /><td width='20px'>"
		else
			response.write "<tr class='item hide_first' in='sg" & arr(9) & "' id='" & arr(0) & "'><th><input type='checkbox' class='selecter' /><td width='20px'>"
		end if
		if arr(3) <> arr(5) + arr(6) + arr(7) then 	
			response.write "<div class='led warning'></div>" ' Несоответствия
		elseif arr(3) = arr(7) then 
			response.write "<div class='led off'></div>" ' Списаны
		elseif arr(5) = 0 then
			response.write "<div class='led onwork'></div>" ' Целиком в работе
		elseif arr(6) > 0 then				
			response.write "<div class='led onwork'></div>" ' В работе		
		else
			response.write "<div class='led'></div>" ' Все оставшиеся
		end if

		dim q
		for q = 0 to 8
			response.write "<td>" & arr(q)
		next
		response.write "</tr>"
	end sub

	dim head : head = "" _
	& "<table class='items'><thead><tr>" _
	& "<th width='20px'><input type='checkbox' class='selecter-all' />" _
	& "<th width='20px' data-type='type'>" _
	& "<th width='56px' data-type='number'>Инв. №" _
	& "<th data-type='string'>Наименование" _
	& "<th width='56px' data-type='number'>Стоимость" _
	& "<th width='56px' data-type='number'>Приход" _
	& "<th width='90px' data-type='date'>Дата покупки" _
	& "<th width='60px' data-type='number'>На складе" _
	& "<th width='60px' data-type='number'>Установлено" _
	& "<th width='60px' data-type='number'>Списано" _
	& "<th width='70px' data-type='string'>Счет учета" _
	& "</tr></thead><tbody>"

	dim conn, rs, sql
	set conn = Server.CreateObject("ADODB.Connection")
		conn.open everest
	set rs = Server.CreateObject("ADODB.Recordset")

	' Получение списка групп
	sql = "SELECT G_ID, G_Inside, G_Title FROM [GROUP] WHERE (G_Type = 'storage')"
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

	dim data(9), i, j
	dim search : search = DecodeUTF8(request.querystring("text"))
	
	if search = "" then	

		dim offs(1000, 9)
		dim Noffs : Noffs = 0

		' Отображение записей
		sql = "SELECT Ncard, Name, Price, NAdd, Date, Nis, Nuse, Nbreak, uchet, G_ID FROM SKLAD WHERE (delit > 0) ORDER BY Date DESC"
		rs.open sql, conn 
			response.write "<div class='unit' id='solo'>" _
			& "<table class='caption'><tr>" _
			& "<td width='24px'><div class='icon ic-cached'></div>" _ 
			& "<th>Не распределенные позиции" _
			& "</tr></table>" _
			& "<div class='items_block'>" & head
			do while not rs.eof
				' Получение данных из записи в базе
				for i = 0 to 9 
					data(i) = rs(i)
				next

				if data(3) = data(7) and data(5) + data(6) = 0 then
					for i = 0 to 9
						offs(Noffs, i) = data(i)
					next
					Noffs = Noffs + 1
				else
					renderItem(data)
				end if	

				rs.movenext
			loop
			response.write "</tbody></table></div></div>"
		rs.close

		' Агрегатор для списанных позиций
		cookie = request.cookies("aggregateOff")
		response.write "<div class='unit " & cookie & "' id='aggregateOff'>"
		response.write "<table class='caption'><tr>" _
						& "<td width='24px'><div class='icon ic-cached'></div>" _ 
						& "<th>Списанные позиции" _
						& "</tr></table>"
		if cookie = "open" then 
			response.write "<div class='items_block'>" & head 
		else 
			response.write "<div class='items_block hide'>" & head
		end if

		for i = 0 to Noffs - 1
			for j = 0 to 9 
				data(j) = offs(i, j)
			next
			renderItem(data)
		next
		response.write "</tbody></table></div></div>"

		' Отображение групп
		dim cookie
		for i = 0 to Ngroups - 1
			cookie = request.cookies("sg" & groups(i, 0))
			if groups(i, 1) = "" or groups(i, 1) = "0" or groups(i, 1) = "-1" or isnull(groups(i, 1)) then
				response.write "<div class='unit group " & cookie & "' id='sg" & groups(i, 0) & "' in='sg" & groups(i, 1) & "'>"
			else 
				response.write "<div class='unit hide_first group " & cookie & "' id='sg" & groups(i, 0) & "' in='sg" & groups(i, 1) & "'>"
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

	else

		sql = replace("SELECT Ncard, Name, Price, NAdd, Date, Nis, Nuse, Nbreak, uchet, G_ID FROM SKLAD WHERE (delit > 0) AND ((Ncard LIKE '%{0}%') OR (Name LIKE '%{0}%') OR (Name LIKE '%{0}%') OR (Date LIKE '%{0}%') OR (uchet LIKE '%{0}%')) ORDER BY CAST(Ncard AS int)", "{0}", search)
		rs.open sql, conn 

		response.write("<div class='unit'><table class='caption'><tr><th>Поиск совпадений по запросу: " & search & "</tr></table>" & head)
		if not rs.eof then
			do while not rs.eof
				for i = 0 to 9 
					data(i) = rs(i)
				next

				renderItem(data)

				rs.movenext
			loop
			response.write("</tbody></table></div>")
		else 
			response.write "<div class='error'>Совпадений нет</div>"
		end if
		response.write("</tbody></table>")
		rs.close
	end if	
%>
	<script id='insert_move_select'>
		document.getElementById('move_select').innerHTML = "<select><option value='0'>Расположить отдельно<%for i = 0 to Ngroups - 1
			response.write "<option value='" & groups(i, 0) & "'>" & groups(i, 2)
		next%></select>";
		{
			var _in;
			$("tr.item,div.group").each(function() {
				_in = this.getAttribute('in');
				if (_in.slice(2) != '0') {
					if ($(this).hasClass('item')) {
						$("#" + _in).children(".items_block").children("table").find("tbody").append(this);
					} else {
						$(this).insertBefore($("#" + _in).children(".items_block").children("table").first());
					}
				}
			});
			$(".hide_first").removeClass("hide_first");
			if ($("#solo").find("tr").length < 3) $("#solo").remove();
		};
		document.getElementById('insert_move_select').parentNode.removeChild(document.getElementById('insert_move_select'));
	</script>
<%
	conn.close
	set rs = nothing
	set conn = nothing
%>