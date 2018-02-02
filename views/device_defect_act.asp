<!-- #include virtual ="/devin/core/core.inc" -->

<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs:   set rs   = server.createObject("ADODB.Recordset")
	dim id:   id = request.queryString("id")
%>

<div class='cart-header'>
	Дефектный акт для <%=id%>
</div>

<%
	conn.open everest

	dim sql: sql = "SELECT D.[class_device], D.[description1C], D.[description], D.[inventory], D.[name], C.[name] AS [comp_name], D.[install_date], D.[mol] " _
	& "FROM [DEVICE] D LEFT OUTER JOIN [DEVICE] C ON D.[number_comp] = C.[number_device] " _
	& "WHERE D.[number_device] = '" & id & "'"

	rs.open sql, conn
	if not rs.eof then
		dim cls:         cls = trim(rs(0))
		dim description: description = trim(rs(1)): if description = "" or isnull(description) then description = trim(rs(2))
		dim inventory:   inventory = trim(rs(3))
		dim name:        if cls = "CMP" then name = trim(rs(4)) else name = trim(rs(5))
		dim work_time:   work_time = datediff("h", datevalue(rs(6)), datevalue(date))
		dim mol:         mol = trim(rs(7))
%>

<div class='cart-overflow'>
	<table class='cart-table'>
		<tbody>
			<tr>
				<td>На дату</td>
				<td><input style="width: 100px" id="date" value="<%=date%>" /></td>
			</tr>
			<tr>
				<td>Наименование по 1С</td>
				<td><textarea id="description"><%=description%></textarea></td>
			</tr>
			<tr>
				<td>Инвентарный номер</td>
				<td><input id="inventory" value="<%=inventory%>" /></td>
			</tr>
			<tr>
				<% if cls = "CMP" then %> <td>Сетевое имя</td> <% else %> <td>Имя компьютера</td> <% end if %>
				<td><input id="name" value="<%=name%>" /></td>
			</tr>
			<tr>
				<td>М.О.Л. (должность)</td>
				<td><input id="mol_post" value="" /></td>
			</tr>
			<tr>
				<td>М.О.Л. (фамилия и.о.)</td>
				<td><input id="mol_name" value="<%=mol%>" /></td>
			</tr>
			<tr>
				<td>Время наработки</td>
				<td><input id="work_time" style="width: 100px;" value="<%=work_time%>" /> часов</td>
			</tr>
		</tbody>
	</table>
	<br/>
	<b>Неисправности</b>
	<table>
		<thead>
			<tr>
				<td>Тип: <select id="defect_position" onchange="checkIfUnique(this)">
					<% if cls = "CMP" then %>
					<option value="motherboard">Материнская плата</option>
					<option value="cpu">Процессор</option>
					<option value="power">Блок питания</option>
					<option value="ram">Оперативная память</option>
					<option value="hdd">Жесткий диск</option>
					<option value="videocard">Видеокарта</option>
					<% end if %>
					<option value="unique">Свое</option>
				</select></td>
				<td id="defect_unique"></td>
				<td>Кол-во: <input id="defect_count" style="width: 60px;" value="1" /></td>
				<td><button onclick="addDefectPosition()">Добавить</button></td>
			</tr>
		</thead>
	</table>
	<table>
		<thead>
			<tr>
				<th></th>
				<th width="100px"></th>
				<th width="100px"></th>
			</tr>
		</thead>
		<tbody id="defect_container"></tbody>
	</table>
	<div id="defect_link"></div>
</div>
<table class='cart-menu'>
	<tbody>
		<tr>
			<td onclick='cartBack()'>Вернуться к карточке</td>
			<td onclick='defectPrint()'>Печать</td>
			<td onclick='cartClose()'>Закрыть</td>
		</tr>
	</tbody>
</table>

<script>
	function addDefectPosition() {
		var count = document.getElementById('defect_count').value;
		if (isNaN(+count) || count == '') return alert('Количество деталей должно быть числом');

		var select = document.getElementById('defect_position');
		var type = select.value;
		var text = (type === 'unique')  ? document.getElementById('unique_name').value : select.options[select.selectedIndex].innerHTML;
		if (type === 'unique' && String(document.getElementById('unique_name').value) == '') return alert("Не введено название неисправности")

		$('#defect_container')
			.append('<tr><td val="' + type + '">' + text + '</td><td>' + Math.round(count) + '</td><td><button onclick="deletePosition(this)">Удалить</button></td></tr>');}
	function deletePosition(button) {
		var row = button.parentNode.parentNode;
		row.parentNode.removeChild(row);}
	function checkIfUnique(select) {
		document.getElementById('defect_unique').innerHTML = (select.value === 'unique')
			? 'Свое: <input type="text" id="unique_name" />'
			: '';}
	function defectPrint() {
		var form = 'id=<%=id%>';
		form += '&type=<%=cls%>';
		form += '&date=' + encodeURIComponent(document.getElementById('date').value);
		form += '&description=' + encodeURIComponent(document.getElementById('description').innerHTML);
		form += '&inventory=' + encodeURIComponent(document.getElementById('inventory').value);
		form += '&name=' + encodeURIComponent(document.getElementById('name').value);
		form += '&mol_post=' + encodeURIComponent(document.getElementById('mol_post').value);
		form += '&mol_name=' + encodeURIComponent(document.getElementById('mol_name').value);
		form += '&work_time=' + encodeURIComponent(document.getElementById('work_time').value);
		form += '&positions=';
		$('#defect_container tr').each(function(i) {
			var td = this.getElementsByTagName('td');
			form +=
				(i > 0
					? ';;'
					: '') +
				(td[0].getAttribute('val') !== 'unique'
					? td[0].getAttribute('val')
					: encodeURIComponent(td[0].innerHTML)) +
				'::' +
				encodeURIComponent(td[1].innerHTML);
		})

		var link = document.getElementById('defect_link');
		link.innerHTML = '<i>... идет печать ...</i>';
		$.post('/devin/exes/device/device_print_defect.asp?r=' + Math.random(), form).done(function(data) {
			link.innerHTML = data;
		}).fail(function() {
			link.innerHTML = '<div class="error">Произошла ошибка</div>';
		})}
</script>

<%
	else
%>

<div class='cart-overflow'>Устройство не найдено в базе данных</div>
<table class='cart-menu'>
	<tbody>
		<tr>
			<td onclick='cartBack()'>Вернуться к карточке</td>
			<td onclick='cartClose()'>Закрыть</td>
		</tr>
	</tbody>
</table>

<%
	end if
	rs.close:   set rs   = nothing
	conn.close: set conn = nothing
%>