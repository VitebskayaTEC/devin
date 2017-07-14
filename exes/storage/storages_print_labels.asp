<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim storages : storages = split(request.form("select"), ";")
	dim sql: sql = ""
	dim storage
	
	for each storage in storages
		if storage <> "" then 
			if sql <> "" then sql = sql & " OR "
			sql = sql & "Ncard = '" & storage & "'"
		end if
	next

	sql = "SELECT SKLAD.Nis, SKLAD.NCard, CARTRIDGE.Caption, SKLAD.Date, SKLAD.Name FROM SKLAD LEFT OUTER JOIN CARTRIDGE ON SKLAD.ID_cart = CARTRIDGE.N WHERE " & sql & " ORDER BY Date DESC"

	dim conn: set conn = server.createobject("ADODB.Connection")
	dim rs:   set rs   = server.createobject("ADODB.Recordset")

	conn.open everest
	rs.open sql, conn

	if not rs.eof then

		dim data(1000, 3), Ndata, i

		do while not rs.eof
			for i = 1 to rs("Nis")
				data(Ndata, 0) = rs("Ncard")
				data(Ndata, 1) = rs("Caption")
				data(Ndata, 2) = rs("Date")
				if isnull(data(Ndata, 1)) then data(Ndata, 1) = rs("Name")
				Ndata = Ndata + 1
			next
			rs.movenext
		loop

		dim excelApp   : set excelApp = server.createobject("Excel.Application")
		excelApp.application.enableEvents = false
		excelApp.application.displayAlerts = false
		dim excelBook  : set excelBook = excelApp.workbooks.open("D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\exl\labels.xls")
		dim excelCells : set excelCells = excelBook.activesheet.cells

		dim isLeft: isLeft = true
		dim rowCount:  rowCount  = 1

		for i = 0 to Ndata - 1

			if isLeft then

				excelCells(rowCount * 3 - 2, 1) = "№"
				excelCells(rowCount * 3 - 1, 1) = "Тип: "
				excelCells(rowCount * 3, 1) = "Приход: "

				excelCells(rowCount * 3 - 2, 2) = cstr(data(i, 0))
				excelCells(rowCount * 3 - 1, 2) = cstr(data(i, 1))
				excelCells(rowCount * 3, 2) = cstr(data(i, 2))

				'excelSheet.range("A1:D" & (rowCount * 3)).Borders.Weight = -4138 ' центрирование

				isLeft = false
			else
				excelCells(rowCount * 3 - 2, 3) = "№"
				excelCells(rowCount * 3 - 1, 3) = "Тип: "
				excelCells(rowCount * 3, 3) = "Приход: "

				excelCells(rowCount * 3 - 2, 4) = cstr(data(i, 0))
				excelCells(rowCount * 3 - 1, 4) = cstr(data(i, 1))
				excelCells(rowCount * 3, 4) = cstr(data(i, 2))

				rowCount = rowCount + 1
				isLeft = true				
			end if

		next

		excelBook.saveAs "D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\labels.xls"
		excelBook.close false
		set excelCells = nothing
		set excelBook  = nothing
		excelApp.quit
		set excelApp   = nothing

		response.write "<a onclick='closeExportsPanel()' href='/devin/excels/labels.xls'>Бирки</a>"

	else

		response.write "Нечего печатать"

	end if

	rs.close
	conn.close
	set rs   = nothing
	set conn = nothing

	
%>
