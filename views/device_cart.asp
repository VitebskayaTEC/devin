<!-- #include virtual ="/devin/core/core.inc" -->
<%

function getValue(name)
	dim q
	for q = 0 to ubound(device)
		if lcase(device(q, 0)) = lcase(name) then getValue = device(q, 1)
	next
end function

' объявление переменных и подключение к базе данных
dim conn, rs, sql
Set conn = Server.CreateObject("ADODB.Connection")
	conn.open everest
Set rs = Server.CreateObject("ADODB.Recordset")

dim id : id = Request.QueryString("id")

dim size : size = 21
dim device(21, 2), temp, cls

'Получаем данные по устройству
rs.open "SELECT * FROM DEVICE WHERE (number_device = '" & id & "')", conn
if not rs.eof then
	dim i
	for i = 0 to rs.fields.count - 1
		device(i, 0) = rs(i).name
		device(i, 1) = trim(rs(i))
	next
else 
	response.write "Устройство не найдено!"
	response.end
end if
rs.close

cls = getValue("class_device")
'Рисуем header в зависимости от типа и наличия в базе устройства
response.write "<div class='cart-header'>"
if cls = "CMP" then 
	response.write "Компьютер " & getValue("name")
else
	if id = "000000-000-00" then
	   response.write "Новое устройство"
	else 
		rs.open "SELECT name FROM DEVICE WHERE (number_device = '"& getValue("number_comp") &"')", conn
		if not rs.eof then 
			response.write getValue("name") & " на " & trim(rs(0))
		else 
			response.write getValue("name")
		end if
		rs.close
	end if
end if
response.write "</div><form id='cart-form'><table class='cart-table'>" 

response.write "<tr><td>Наименование<td><input name='name' value='" & getValue("name") & "' /></tr>"
response.write "<tr><td>Инвентарный номер<td><input name='inventory' value='" & getValue("inventory") & "' /></tr>"

response.write "<tr><td>Тип устройства<td><select name='class_device'><option value='0'>?"
rs.open "SELECT T_alias, T_alias, T_name FROM catalog_device_types ORDER BY T_name", conn
do while not rs.eof
	if cls = rs(0) then 
		response.write "<option value='" & rs(1) & "' selected>" & rs(2)
	else
		response.write "<option value='" & rs(1) & "'>" & rs(2)
	end if
	rs.movenext
loop
rs.close
response.write "</select></tr>"

response.write "<tr><td>Номер устройства<td><i><small>" & getValue("number_device") & "</small></i></tr>"

if cls <> "CMP" then
	response.write "<tr><td>Компьютер<td><select name='number_comp'><option value='0'>?"
	rs.Open "SELECT RTRIM(number_device), RTRIM(number_device), RTRIM(name) FROM DEVICE WHERE (class_device = 'CMP') ORDER BY name", conn
	temp = getValue("number_comp")
	do while not rs.eof
		if temp = rs(0) then
			Response.Write "<option selected value='" & rs(1) & "'>" & rs(2)
		else
			Response.Write "<option value='" & rs(1) & "'>" & rs(2)
		end if
		rs.MoveNext
	loop
	rs.Close
	response.write "</select> <a onclick='cartOpen(document.getElementById(" & chr(34) & temp & chr(34) & "))'>Карточка</a></tr>"
end if

response.write "<tr><td>Подпись в 1С<td><input name='description1C' value='" & getValue("description1C") & "' /></tr>"
response.write "<tr><td>Описание<td><textarea name='description'>" & getValue("description") & "</textarea></tr>"
response.write "<tr><td>М.О.Л.<td><input name='mol' value='" & getValue("mol") & "' /></tr>"
response.write "<tr><td>Расположение<td><input name='attribute' value='" & getValue("attribute") & "' /></tr>"
response.write "<tr><td>Серийный номер<td><input name='number_serial' value='" & getValue("number_serial") & "' /></tr>"
response.write "<tr><td>Паспортный номер<td><input name='number_passport' value='" & getValue("number_passport") & "' /></tr>"

response.write "<tr><td>Дата установки<td><input name='install_date' style='width: 100px' value='" & datevalue(getValue("install_date")) & "' /> <a><b>" & datediff("h", datevalue(getValue("install_date")), datevalue(date)) & " часов</b></a></tr>"	

if cls = "CMP" then
	response.write "<tr><td>ОС<td><select name='OS'><option value='0'>?"
	rs.Open "Select T_alias, T_alias,T_name From catalog_os Order by T_name", CONN
	do while not rs.eof
		if getValue("os") = trim(rs(0)) then
			response.write "<option selected value='" & trim(rs(1)) & "'>" & trim(rs(2))
		else
			response.write "<option value='" & trim(rs(1)) & "'>" & trim(rs(2))
		end if
		rs.MoveNext
	loop
	rs.Close
	response.write "</select></tr>"
end if

if cls = "CMP" then 
	response.write "<tr><td>Ключ ОС<td><input name='OSKEY' value='" & getValue("oskey") & "' /></tr>"
	response.write "<tr><td>Ключ для софта<td><input name='PRKEY' value='" & getValue("prkey") & "' /></tr>"
end if

if cls = "PRN" then 
	response.write "<tr><td>Типовой принтер<td><select name='ID_prn'><option value='0'>?"
	rs.Open "Select N, Caption From PRINTER Order by Caption", CONN
	dim temp_n
	do while not rs.eof
		temp_n = rs(0)
		'response.write "<option>1: [" & getValue("ID_prn") & "] 2: [" & RS(0) & "]"
		if cstr(getValue("ID_prn")) = cstr(temp_n) then
			response.write "<option selected value='" & temp_n & "'>" & rs(1)
		else
			response.write "<option value='" & temp_n & "'>" & rs(1)
		end if
		rs.MoveNext
	loop
	rs.Close
	response.write "</select></tr>"
end if

Response.Write "<tr><td>Используется<td><select name='used'>"
if getValue("used") = "1" then
	Response.Write "<option value='0'>Нет<option selected value='1'>Да"
else
	Response.Write "<option selected value='0'>Нет<option value='1'>Да"
end if
Response.Write "</select></tr>"

Response.Write "<tr><td>Сверка с 1С<td><select name='check1C'>"
if getValue("check1C") = "1" then
	Response.Write "<option value='0'>Нет<option selected value='1'>Да"
else
	Response.Write "<option selected value='0'>Нет<option value='1'>Да"
end if
Response.Write "</select></tr>"

if cls = "CMP" then 
	Response.Write "<tr><td>Сверка с AIDA<td><select name='checkEverest'><option value='0'>Нет<option"
	if getValue("checkEverest") = "1" then response.write " selected"
	Response.Write " value='1'>Да</select></tr>"
end if

Response.Write "<tr><td>Группа<td><select name='G_ID'><option value='0'>?"
rs.Open "Select G_ID, G_ID, G_Title From [GROUP] Where (G_Type = 'device') Order by G_Title", CONN      
do while not rs.eof
	if getValue("G_ID") = cstr(rs(0)) then 
		Response.Write "<option value='" & trim(rs(1)) & "' selected>" & trim(rs(2))
	else
		Response.Write "<option value='" & trim(rs(1)) & "'>" & trim(rs(2))
	end if
	rs.movenext
loop
rs.close
response.write "</select></tr></table></form>"

Set rs = Nothing
conn.close
Set conn = Nothing

response.write "<div class='cart-links'><a onclick='cartHistoryRepair()'>История ремонтов</a>"
if cls = "CMP" then 
	response.write "<a onclick='cartAidaAutorun()'>Автозагрузка</a>"
	response.write "<a onclick='cartAidaDevices()'>Оборудование</a>"
end if

response.write "</div><div id='console'></div><table class='cart-menu'><tr>"
	response.write "<td onclick='cartSave()'>Сохранить"
	response.write "<td onclick='deviceToRepair()'>Ремонт"
	if cls = "CMP" then 
		response.write "<td onclick='cartAida()'>Everest"
		response.write "<td onclick='cartHistory(" & chr(34) & "t=name" & chr(34) & ")'>История по имени"
	end if
	response.write "<td onclick='cartHistory(" & chr(34) & "t=id" & chr(34) & ")'>История по инв.н."
	response.write "<td onclick='cartDelete()'>Удалить"
	response.write "<td onclick='cartCopy()'>Копир."
	response.write "<td onclick='cartClose()'>Закрыть"
response.write "</tr></table>"
%>