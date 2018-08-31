<!-- #include virtual ="/devin/core/core.inc" -->
<!-- #include virtual ="/devin/core/upload.asp"-->

<!doctype html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/devin/content/css/core.css" rel="stylesheet" />
	<link href="/devin/content/css/device.css" rel="stylesheet" />
	<link href="/devin/content/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | 1С</title>
	<style>.compare_good{background: #95d89b}.compare_bad{background: #e29090;}.compare_notfound{background: #fdb87b;}</style>
</HEAD>

<BODY>

<%
	menu("")

	' Загружаем файл отчета
	dim Uploader : set Uploader = new FileUploader
	dim File, excelName

	Uploader.Upload()
	If Uploader.Files.Count <> 0 Then
		for each File In Uploader.Files.Items
			File.SaveToDisk "D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\"
			excelName = File.FileName
		next
		objStream1.close
		set objStream1 = nothing
	end if

	' Сверка выгрузки со складом
	dim oexcel, oworkbook, ocells
	set oexcel = createobject("Excel.Application")
		oexcel.application.enableevents = false
		oexcel.application.displayalerts = false
	set oworkbook = oexcel.workbooks.open("D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\" & excelName)
	set ocells = oworkbook.activesheet.cells

	dim balances(1000, 10), storage(1000, 10), escape, cursor, position, index, excel_cells, score, find, i, j, scores(10)

	' Забираем данные из Excel
	excel_cells = array(11, 12, 13, 16, 29, 33) 'Обозначение, инв. номер, цена 1шт, дата прихода, кол-во остатка, счет учета
	escape = 0
	position = 9
	index = 0
	score = 0
	dim find_score, temp_score
	do while not escape > 5
		cursor = ocells(position, 33)
		if cursor = "" or isnull(cursor) then
			escape = escape + 1 'если пустая клетка, начинаем их считать, если 5 подряд - выход
		else
			escape = 0 'обнуляем счетчик пустых клеток, т.к. еще не конец документа
			'проверка, есть ли остаток
			if ocells(position, excel_cells(4)) = "" or ocells(position, excel_cells(4)) = " " or isnull(ocells(position, excel_cells(4))) then
				index = index
			else
				find_score = false
				temp_score = ocells(position, 33)
				if temp_score <> "" then
					for i = 0 to score
						if scores(i) = temp_score then find_score = true
					next
					if not find_score then
						scores(score) = temp_score
						score = score + 1
					end if
				end if

				for i = 0 to 5
					balances(index, i) = trim(ocells(position, excel_cells(i))) 'заполнение массива остатков из Excel
				next
				index = index + 1
			end if
		end if
		position = position + 1
	loop
	index = index - 1
	oworkbook.close
	set ocells = nothing
	set oworkbook = nothing
	set oexcel = nothing

	' Забираем данные из складской базы

	dim conn, rs, sql
	set conn = server.createobject("ADODB.Connection")
	set rs = server.createobject("ADODB.Recordset")
	conn.open "Driver={SQL Server}; Server=log1\SQL2005; Database=EVEREST; Uid=user_everesr; Pwd=EveresT10;"

	for i = 0 to score - 1
		if i > 0 then sql = sql & " OR "
		sql = sql & "(uchet = '" & scores(i) & "')"
	next
	if sql <> "" then sql = " AND (" & sql & ")" else sql = " AND (1 = 2)"
	sql = "SELECT INum, Name, NCard, Price, Nadd, Nis, Nuse, Nbreak, Date FROM SKLAD WHERE (delit <> 0)" & sql
	'response.write sql
	rs.open sql, conn
	if not rs.eof then
		position = 0
		do while not rs.eof
			for i = 0 to 8
				storage(position, i) = trim(rs(i))
			next
			rs.movenext
			position = position + 1
		loop
		position = position - 1
	end if
	rs.close
	set rs = nothing
	conn.close
	set conn = nothing

	'Сводим данные в таблицу


	response.write "<div class='view'><div class='unit'>"
	response.write "<table class='caption'><td>Счеты учета: "
	for i = 0 to score - 1
		if i > 0 then response.write ", "
		response.write scores(i)
	next
	response.write "</tr></table><table class='items'>"
	response.write "<thead><tr><th>Инв. №<th>Наименование<th>На складе сейчас<th>В ремонте<th>Остаток по 1С</tr></thead><tbody>"
	for i = 0 to position
		find = false
		for j = 0 to index
			if storage(i, 2) = balances(j, 1) and not find then 'выводим соответствие
				if cint(storage(i, 5)) + cint(storage(i, 6)) = cint(balances(j, 4)) then response.write "<tr class='compare_good'>" else response.write "<tr class='compare_bad' title='Несоответствие по количеству'>"
				response.write "<td>" & storage(i, 2) & "<td>" & storage(i, 1) & "<td>" & storage(i, 5) & "<td>" & storage(i, 6) & "<td>" & balances(j, 4) & "</tr>"
				find = true
			end if
		next
		if not find then 'выводим склад only
			if cint(storage(i, 5)) + cint(storage(i, 6)) = 0 then response.write "<tr class='compare_good'>" else response.write "<tr class='compare_bad' title='Позиция существует только на складе'>"
			response.write "<td>" & storage(i, 2) & "<td>" & storage(i, 1) & "<td>" & storage(i, 5) & "<td>" & storage(i, 6) & "<td>Не найден</tr>"
		end if
	next
	for j = 0 to index
		find = false
		for i = 0 to position
			if storage(i, 2) = balances(j, 1) and not find then find = true
		next
		'выводим 1с only
		if not find then response.write "<tr class='compare_notfound' title='Позиция существует только в 1С'><td>" & balances(j, 1) & "<td>" & balances(j, 0) & "<td>Не найден<td>Не найден<td>" & balances(j, 4) & "</tr>"
	next
	response.write "</tbody></table></div></div>"
%>

</BODY>

</HTML>