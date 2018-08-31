<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim cart
	dim cartridge
	dim dataArray(500, 4)
	dim i, N
	dim data
	dim sum
	dim quarter
	dim merged, tempType

	if month(date) > 8 then
		quarter = "в IV квартале"
	elseif month(date) > 5 then
		quarter = "в III квартале"
	elseif month(date) > 2 then
		quarter = "во II квартале"
	else
		quarter = "в I квартале"
	end if

	on error resume next

	' Забираем данные из формы
	data = split(DecodeUTF8(request.form("data")), "----")
	N = -1

	for each cart in data

		cartridge = split(cart, "__")
		N = N + 1

		select case cartridge(2)
			case "flow"   cartridge(2) = "Картридж струйный"
			case "laser"  cartridge(2) = "Тонер-картридж"
			case "matrix" cartridge(2) = "Матричная лента"
		end select

		select case cartridge(4)
			case "black"  cartridge(4) = "черный"
			case "blue"   cartridge(4) = "голубой"
			case "red"    cartridge(4) = "красный"
			case "yellow" cartridge(4) = "желтый"
			case "3color" cartridge(4) = "трехцветный"
			case "5color" cartridge(4) = "многоцветный"
		end select

		for i = 0 to 4
			dataArray(N, i) = cartridge(i)
		next

		sum = sum + ( ccur(cartridge(1)) * cint(cartridge(3)) )

	next

	' Открываем шаблон excel
	dim excel:    set excel    = createObject("Excel.Application")
		excel.application.enableEvents  = false
	    excel.application.displayAlerts = false
	dim workbook: set workbook = excel.workbooks.open("C:\Inetpub\wwwroot\Devin\Content\exl\analyze.xls")
	dim sheet:    set sheet    = workbook.worksheets(1)


	for i = 0 to N - 1
		sheet.range("A18:J18").insert -4121, 0
		sheet.range("B18:C18").merge
		sheet.range("F18:G18").merge
		sheet.range("H18:I18").merge
	next

	merged = 0
	tempType = ""
	for i = 0 to N

		sheet.cells(18 + i, 4).value = "шт."
		sheet.cells(18 + i, 5).value = dataArray(i, 3)
		sheet.cells(18 + i, 6).value = dataArray(i, 0) & ", " & dataArray(i, 4)
		sheet.cells(18 + i, 8).value = dataArray(i, 1) & " BYN за 1 шт."

		if i = 0 then tempType = dataArray(i, 2)

		if dataArray(i, 2) <> tempType then
			sheet.range("B" & (18 + merged) & ":C" & (17 + i)).merge
			sheet.range("B" & (18 + merged) & ":C" & (17 + i)).value = tempType
			tempType = dataArray(i, 2)
			merged = i
		end if

	next

	sheet.range("B" & (18 + merged) & ":C" & (18 + N)).merge
	sheet.range("B" & (18 + merged) & ":C" & (18 + N)).value = tempType

	' Итоговое форматирование
	sheet.cells(8, 2).value = date
	sheet.cells(13, 2).value = replace(sheet.cells(13, 2).value, "@quarter", quarter)
	sheet.cells(13, 2).value = replace(sheet.cells(13, 2).value, "@year", year(date))
	sheet.cells(19 + N, 8).value = replace(sheet.cells(19 + N, 8).value, "@sum", sum)
	sheet.range("A18:J" & (18 + N)).HorizontalAlignment = -4108 'центр
	sheet.range("A18:J" & (18 + N)).VerticalAlignment   = -4108 'центр
	sheet.range("A18:J" & (18 + N)).WrapText            = true
	sheet.range("A18:J" & (18 + N)).Font.Bold           = false
	sheet.range("A18:J" & (18 + N)).AddIndent           = false
    sheet.range("F18:I" & (18 + N)).HorizontalAlignment = -4131
	sheet.range("A18:J" & (18 + N)).Rows.AutoFit


		workbook.saveAs "C:\Inetpub\wwwroot\Devin\Content\Excels\analyze.xls"
	    workbook.close false
		excel.quit
	set sheet    = nothing
	set workbook = nothing
	set excel    = nothing

	response.write "<a href='/devin/content/excels/analyze.xls' download>Сохранить файл</a>"
%>