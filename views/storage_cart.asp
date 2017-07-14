<!-- #include virtual ="/devin/core/core.inc" -->
<%

	dim conn, rs, sub_rs, sql, temp
	set conn = Server.CreateObject("ADODB.Connection")
		conn.open everest
	set rs = Server.CreateObject("ADODB.Recordset")

	dim id : id = Request.QueryString("id")
	dim watch : watch = timer()

	function options(query)
		rs.open query, conn
		if not rs.eof then
			response.write "<option value='0'>?"
			do while not rs.eof
				if rs(0) = temp then 
					response.write "<option value='" & rs(1) & "' selected>" & rs(2)
				else
					response.write "<option value='" & rs(1) & "'>" & rs(2) 
				end if
				rs.movenext
			loop
		end if
		rs.close
		options = ""
	end function
	
	sql = "SELECT Name, NCard, class_name, Price, Nadd, Nis, Nuse, Nbreak, Date, delit, uchet, ID_cart, G_ID FROM SKLAD WHERE (NCard = '" & id & "')"
	dim info(12), i
	rs.open sql, conn
	for i = 0 to rs.fields.count - 1
		info(i) = rs(i)
	next
	rs.close
%>

<div class='cart-header'><%=info(0)%></div>
<form id='form' method='post'>
	<table class='cart-table'>
		<tbody>
			<tr><td>Инвентарный №<td><input name="ncard" type="text" value="<%=info(1)%>" style="width: 100px" /></tr>
			<tr><td>Наименование<td><textarea name="name"><%=info(0)%></textarea></tr>
			
			<tr><td>Тип устройства<td><select name="class_name">
				<% 
					temp = info(2)
					options("SELECT T_alias, T_alias, T_storages FROM catalog_device_types") 
				%>
			</tr>

			<tr><td>Счет учета<td><select name="uchet">
				<% 
					temp = info(10)
					options("SELECT uchet AS uch1, uchet AS uch2, uchet AS uch3 FROM SKLAD GROUP BY uchet ORDER BY uch2") 
				%>
			</tr>

			<tr><td>Стоимость (р. за шт.)<td><input name="Price" type="text" value="<%=info(3)%>" style="width: 100px" /></tr>
			<tr><td>Дата прихода<td><input name="Date" type="text" value="<%=info(8)%>" style="width: 100px" /></tr>

			<tr><td>Приход<td><input name="Nadd" type="number" value="<%=info(4)%>" /></tr>
			<tr><td>На складе<td><input name="Nis" type="number" value="<%=info(5)%>" /></tr>
			<tr><td>Используется<td><input name="Nuse" type="number" value="<%=info(6)%>" /></tr>
			<tr><td>Списано<td><input name="Nbreak" type="number" value="<%=info(7)%>" /></tr>
			
			<%
				if info(2) = "PRN" then
					response.write "<tr><td>Типовой картридж<td><select name='ID_cart'>"
					temp = info(11)
					options("SELECT N, N, Caption FROM CARTRIDGE ORDER BY Caption")
					response.write "</tr>"
				end if
			%>
			
			<tr><td>Группа<td><select name="G_ID">
				<%
					temp = info(12)
					options("SELECT G_ID, G_ID, G_Title FROM [GROUP] WHERE (G_Type = 'storage') ORDER BY G_Title") 
				%>
			</tr>
		</tbody>
	</table>
</form>
<div id='console'></div>
<table class='cart-table cart-menu'>
	<tbody>
		<tr>
			<td onmousedown='cartSave()'>Сохранить
			<td onmousedown='storageToRepair()'>Ремонт
			<td onmousedown='cartHistory()'>История
			<td onmousedown='cartDelete()'>Удалить
			<td onmousedown='cartClose()'>Закрыть
		</tr>
	</tbody>
</table>

<%
	set rs = nothing
	conn.close
	set conn = nothing
	dim sqltime : sqltime = strGetCurrTime(Timer() - watch)
%>

<script>try { console.log("cart: <%=sqltime%>")	} catch (e) {}</script>