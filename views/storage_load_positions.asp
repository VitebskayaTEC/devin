<!-- #include virtual ="/devin/core/core.inc" -->
<!-- #include virtual ="/devin/core/upload.asp"-->

<!doctype html>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="content-type" Content="text/html; charset=windows-1251" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/device.css" rel="stylesheet" />
	<link href="/devin/img/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<title>DEVIN | 1С</title>
</HEAD>

<BODY>

<%
	menu("<li><a onclick='queryAll()'>Применить все</a>")
	
	response.write "<div class='view'>"

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
	
	' Открытие Excel отчета и считывание данных
	dim i, j
	dim excel 		: set excel = createobject("Excel.Application")
	excel.application.enableevents = false
	excel.application.displayalerts = false
	dim workbook 	: set workbook = excel.workbooks.open("D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\" & excelName)
	dim cells 		: set cells = workbook.activesheet.cells
	
	dim exCells 	: exCells = array(11, 12, 13, 16, 21, 33) 'Обозначение, инв. номер, цена 1шт, дата прихода, кол-во прихода, кол-во остатка, счет учета
	dim escape, position, excelData(1000, 5), Nexcel, cursor
	escape = 0
	position = 9
	Nexcel = 0
	do while not escape > 5
		cursor = cells(position, 33)
		if cursor = "" or isnull(cursor) then 
			escape = escape + 1 'если пустая клетка, начинаем их считать, если 5 подряд - выход
		else 
			escape = 0 'обнуляем счетчик пустых клеток, т.к. еще не конец документа
			'проверка, есть ли остаток
			if cells(position, 21) <> " " then
				for i = 0 to 5
					excelData(Nexcel, i) = trim(cells(position, exCells(i))) 'заполнение массива остатков из Excel
				next
				Nexcel = Nexcel + 1
			end if
		end if
		position = position + 1
	loop
	Nexcel = Nexcel - 1

	workbook.close
	set excel = nothing
	set workbook = nothing
	set cells = nothing

	
	' Получение данных из базы
	dim conn, rs, sql
	set conn = server.createobject("ADODB.Connection")
	set rs = server.createobject("ADODB.Recordset")
	
	for i = 0 to Nexcel
		if i > 0 then sql = sql & " OR "
		sql = sql & "(NCard = '" & excelData(i, 1) & "')"
	next
	
	dim storage(1000, 3), Nstorage
	sql = "SELECT NCard, Date, Nadd, Inum FROM SKLAD WHERE " & sql & " ORDER BY NCard"
	
	conn.open everest
	rs.open sql, conn
		Nstorage = 0
		do while not rs.eof
			for i = 0 to 3
				storage(Nstorage, i) = rs(i)
			next
			rs.movenext
			Nstorage = Nstorage + 1
		loop
	rs.close
	conn.close
	set rs = nothing
	set conn = nothing
	
	' Выполнение обновления или добавления
	dim compared, queryAll, className, queryOne
	dim user : user = replace(request.servervariables("REMOTE_USER"), "VST\", "")
	response.write "<div class='unit'>" _
	& "<table class='caption'><tr><td>" & excelName & "</tr></table>" _
	& "<table class='items'>" _
	& "<thead>" _
	& "<th width='100px'>Инвентарный №<th>Наименование<th width='100px'>Дата прихода 1С<th width='100px'>Приход по 1C<th width='180px'>Приход по Devin" _
	& "</thead><tbody>"
	for i = 0 to Nexcel
		
		compared = false
		for j = 0 to Nstorage
			if excelData(i, 1) = storage(j, 0) then
				compared = true
				if cstr(excelData(i, 4)) <> cstr(storage(j, 2)) then
					queryOne = "UPDATE SKLAD SET Nadd = '" & excelData(i, 4) & "', Date = '" & DateToSql(excelData(i, 3)) & "' WHERE (NCard = '" & storage(j, 0) & "') "  & chr(13) _
					& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), '" & excelData(i, 1) & "', '" & user & "', 'Администратор DEVIN', 'Позиция " & excelData(i, 0) & " [" & excelData(i, 1) & "] обновлена на основании данных из выгрузки 1С [" & excelName & "], изменены: приход с [" & storage(j, 2) & "] на [" & excelData(i, 4) & "], дата прихода с [" & datevalue(storage(j, 1)) & "] на [" & datevalue(excelData(i, 3)) & "]')"
				
					response.write "<tr>" _
					& "<td>" & excelData(i, 1) _
					& "<td>" & excelData(i, 0)  _
					& "<td>" & datevalue(excelData(i, 3))  _
					& "<td>" & excelData(i, 4) _
					& "<td>" & storage(j, 2) & " (" & datevalue(storage(j, 1)) & ") " _
					& "<input type='button' value='Обновить' onclick='queryOne(this)' /><div class='hide'>" & queryOne & "</div>" _
					& "</tr>"
					
					queryAll = queryAll & queryOne & chr(13)
				end if
			end if
		next
		if not compared then 
			' Выбор типа для позиции
			className = ""
		
			queryOne = "INSERT INTO SKLAD (Ncard, Name, Date, Price, Nadd, Nis, Nuse, Nbreak, uchet, G_ID, delit, class_name) VALUES (" _
			& "'" & excelData(i, 1) & "', " _
			& "'" & excelData(i, 0) & "', " _
			& "'" & DateToSql(excelData(i, 3)) & "', " _
			& "'" & replace(excelData(i, 2), ",", ".") & "', " _
			& "'" & excelData(i, 4) & "', " _
			& "'" & excelData(i, 4) & "', " _
			& "'0', " _
			& "'0', " _
			& "'" & excelData(i, 5) & "', " _
			& "'0', " _
			& "'1', " _
			& "'" & className & "') "  & chr(13) _
			& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), '" & excelData(i, 1) & "', '" & user & "', 'Администратор DEVIN', 'Позиция " & excelData(i, 0) & " [" & excelData(i, 1) & "] добавлена из выгрузки 1С [" & excelName & "]')"
		
			response.write "<tr>" _
			& "<td>" & excelData(i, 1) _
			& "<td>" & excelData(i, 0) _
			& "<td>" & datevalue(excelData(i, 3)) _
			& "<td>" & excelData(i, 4) _
			& "<td>не найден " _
			& "<input type='button' value='Добавить' onclick='queryOne(this)'/><div class='hide'>" & queryOne & "</div>" _
			& "</tr>"
			
			queryAll = queryAll & queryOne & chr(13)
		end if
	next
	if queryAll <> "" then
		response.write "<div class='hide' id='queryAll'>" & queryAll & "</div>"
	else 
		response.write "<tr><td colspan='5'>Новых данных нет</tr>"
	end if
	response.write "</tbody></table></div></div>"
%>

<script src='/devin/js/jquery-1.12.4.min.js'></script>
<script>
	function queryAll() {
		if (document.getElementById("queryAll")) {
			$.post("/devin/exes/storage/storage_update_from_1c.asp?r=" + Math.random(), "query=" + encodeURIComponent(document.getElementById("queryAll").innerHTML), function(data) {
				if (data.indexOf("error") < 0) {
					alert("Данные успешно обновлены");
					document.getElementById("queryAll").parentNode.removeChild(document.getElementById("queryAll"));
				} else {
					alert(data);
				}
			});
		} else {
			alert("Нет новых данных для внесения изменений");
		}
	}
	
	function queryOne(button) {
		var div = button.parentNode.getElementsByTagName("div");
		if (div.length > 0) {
			$.post("/devin/exes/storage/storage_update_from_1c.asp?r=" + Math.random(), "query=" + encodeURIComponent(div[0].innerHTML), function(data) {
				if (data.indexOf("error") < 0) {
					alert("Позиция успешно обновлена");
					var tr = button.parentNode.parentNode;
					tr.parentNode.removeChild(tr);
				} else {
					alert(data);
				}
			});
		}
	}
</script>

</BODY>

</HTML>