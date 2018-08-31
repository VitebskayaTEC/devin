<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs   : set rs = Server.CreateObject("ADODB.Recordset")
	dim name : name = Request.QueryString("name")
	dim id   : id = 0
	dim sql  : sql = ""

	conn.open everest

	response.write "<div class='cart-header'>ОС и ПО компьютера " & name & "</div>"

	' Получение ID репорта на выбранный комп. Комп выбирается по имени (хз почему, т.к. привязка по uuid)
	sql = "SELECT Max(ID) FROM Report WHERE LOWER(RHost) = LOWER('" & name & "')"
	rs.open sql, conn
		if not rs.eof then id = rs(0)
	rs.close

	' Запрос на получение данных о операционной системе
	sql = "SELECT IField, IValue FROM Item WHERE ReportId = " & id & " AND IPage = 'Операционная система' AND (" _
	& "(IGroup = 'Свойства операционной системы' AND IField = 'Название ОС') " _
	& "OR (IGroup = 'Свойства операционной системы' AND IField = 'Версия ОС') " _
	& "OR (IGroup = 'Лицензионная информация' AND IField = 'Ключ продукта'))"
	rs.open sql, conn
	if not rs.eof then
		response.write "<b>Операционная система</b><table><tbody>"
		do while not rs.eof
			response.write "<tr><td>" & rs(0) & "</td><td>" & rs(1) & "</td></tr>"
			rs.movenext
		loop
		response.write "</tbody></table><hr />"
	end if
	rs.close

	' Запрос на получение данных из базы и парсинг данных
	sql = "SELECT IDevice FROM Item WHERE ReportId = " & id _
	& " AND IPage = 'Установленные программы'" _
	& " AND IDevice NOT LIKE '%Microsoft Visual C++%'" _
	& " AND IDevice NOT LIKE '%.NET F%'" _
	& " AND IDevice NOT LIKE '%vs_%'" _
	& " AND IDevice NOT LIKE '%Update%'" _
	& " AND IDevice NOT LIKE '%icecap_%'" _
	& " GROUP BY IDevice"
	rs.open sql, conn
	if not rs.eof then
		response.write "<div class='cart-overflow'><b>Установленное ПО</b>"
		do while not rs.eof
			response.write "<div>" & rs(0) & "</div>"
			rs.movenext
		loop
		response.write "</div>"
	end if
	rs.close

	conn.close
	set rs = nothing
	set conn = nothing

	response.write "<table class='cart-menu'><tr><td onclick='cartBack()'>Вернуться к карточке<td onclick='cartClose()'>Закрыть</tr></table>"
%>