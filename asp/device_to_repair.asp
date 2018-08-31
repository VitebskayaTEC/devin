<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim id : id = request.querystring("id")
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs : set rs = Server.CreateObject("ADODB.Recordset")
	dim sql : sql = "SELECT TOP (1) class_device, ID_prn, name, description, attribute, mol FROM DEVICE WHERE (number_device = '" & id & "')"
	dim class_device, idprn
	conn.open everest
	rs.open sql, conn
	if rs.eof then
		response.write "Произошла ошибка. Устройство с заданным ID больше не существует или его ID был изменен."
		response.end
	else
		class_device = trim(rs("class_device"))
		idprn = rs("ID_prn")
		response.write "<div class='cart-header'>Создание записи о ремонте => " & trim(rs("name")) & "</div>"
		response.write "<table class='cart-table'>"
		response.write "<tr><td>Описание:<td>" & trim(rs("description")) & "</tr>"
		response.write "<tr><td>Расположение:<td>" & trim(rs("attribute")) & "</tr>"
		response.write "<tr><td>МОЛ:<td>" & trim(rs("mol")) & "</tr>"
		response.write "</table>"
	end if
	rs.close
%>
	<div>Запчасть:
		<select id='group' onchange='filterClassName(this)'>
		<%
		rs.open "SELECT G_Id, G_Title FROM [GROUP] WHERE G_Type = 'storage'", conn
		if not rs.eof then
			do while not rs.eof
				response.write "<option value='" & rs(0) & "'>" & rs(1) & "</option>"
				rs.moveNext
			loop
		end if
		rs.close
		%>
		</select>

		<input type='checkbox' id='only' checked onchange='filterOnly(this)' /><label for='only'>Только доступные</label>

		<input type='checkbox' id='writeoff' /><label for='writeoff'>Сгруппировать</label>
		<input type='text' class='def' def='название группы' value='Ремонты за <%=date%>' id='writeoff-name' />
	</div>

	<form id='repair-form'>
		<div class='cart-overflow' id='repair-data'></div>
	</form>
	<div id='console'></div>
	<table class='cart-menu'><tr>
		<td onclick='cartBack()'>Назад
		<td onclick='createRepairs()'>Подтвердить
		<td onclick='cartClose()'>Закрыть
	</tr></table>

	<script>
		$("#repair-data").load("/devin/exes/device/device_repair_data.asp?id=<%=id%>&r=" + Math.random());
		document.getElementById("repair-form").onsubmit = function () {
			var event = event || window.event;
			event.preventDefault();
		};
		var class_device = "<%=class_device%>";
		document.getElementById("classname").value = class_device;
	</script>