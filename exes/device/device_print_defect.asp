<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim keys: set keys = server.createObject("Scripting.Dictionary")
		keys.add "motherboard", "материнская плата (вздутие конденсаторов, отказ контроллеров)"
		keys.add "power", "блок питания (перегрев входных цепец)"
		keys.add "cpu", "процессор (повреждение внутренних цепей контроллера)"
		keys.add "hdd", "жесткий диск (разрушение поверхности пластин вследствие большого износа накопителя)"
		keys.add "ram", "ОЗУ (перегрев)"
		keys.add "videocard", "видеокарта (перегрев, ошибки контроллера, артефакты)"

	dim works: set works = server.createObject("Scripting.Dictionary")
		works.add "motherboard", "Ремонт материнской платы"
		works.add "power", "Ремонт блока питания"
		works.add "cpu", "Ремонт процессора"
		works.add "hdd", "Ремонт жесткого диска"
		works.add "ram", "Ремонт ОЗУ"
		works.add "videocard", "Ремонт видеокарты"

	dim excel: set excel = createObject("Excel.Application")
		excel.application.enableEvents  = false
		excel.application.displayAlerts = false
	dim book:  set book  = excel.workbooks.open("C:\Inetpub\wwwroot\Devin\Content\exl\defect.xls")
	dim sheet: set sheet = book.worksheets(1)

	dim id:          id          = DecodeUTF8(request.form("id"))
	dim data:        data        = DecodeUTF8(request.form("date"))
	dim description: description = DecodeUTF8(request.form("description"))
	dim inventory:   inventory   = DecodeUTF8(request.form("inventory"))
	dim name:        name        = DecodeUTF8(request.form("name"))
	dim mol_post:    mol_post    = DecodeUTF8(request.form("mol_post"))
	dim mol_name:    mol_name    = DecodeUTF8(request.form("mol_name"))
	dim work_time:   work_time   = DecodeUTF8(request.form("work_time"))
	dim positions:   positions   = DecodeUTF8(request.form("positions"))

	dim months: months = array("января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря")

	dim p, param
	dim i: i = 0
	dim defects: defects = ""

	sheet.cells(28, 23).value = mol_post
	sheet.cells(28, 69).value = mol_name
	sheet.cells(34, 1).value = description & " (" & name & ") инв. № " & inventory & " Cрок работы: " & work_time & " часов"
	sheet.cells(36, 17).value = "произошел выход из строя следующих комплектующих:"

	if isDate(data) then data = datevalue(data) else data = date
	sheet.cells(88, 3).value = day(data)
	sheet.cells(88, 9).value = months(month(data) -1)
	sheet.cells(88, 33).value = year(data)

	if instr(positions, ";;") > 0 then
		positions = split(positions, ";;")
		for each p in positions
			if p <> "" then
				param = split(p, "::")
				if defects <> "" then defects = defects & ", "
				if keys.exists(param(0)) then defects = defects & keys(param(0)) else defects = defects & param(0)
				if works.exists(param(0)) then
					sheet.cells(74 + i, 6).value = works(param(0))
				else
					sheet.cells(74 + i, 6).value = "Ремонт (" & param(0) & ")"
				end if
				sheet.cells(74 + i, 71).value = param(1)
				i = i + 1
			end if
		next
	elseif instr(positions, "::") > 0 then
		param = split(positions, "::")
		if keys.exists(param(0)) then defects = defects & keys(param(0)) else defects = defects & param(0)
		if works.exists(param(0)) then
			sheet.cells(74, 6).value = works(param(0))
		else
			sheet.cells(74, 6).value = "Ремонт (" & param(0) & ")"
		end if
		sheet.cells(74, 71).value = param(1)
	end if

	sheet.cells(39, 1).value = defects

	dim excelName : excelName = "Дефектный акт " & date & " " & id
	book.saveas "C:\Inetpub\wwwroot\Devin\Content\Excels\" & excelName & ".xls"
	response.write "<a href='/devin/content/excels/" & excelName & ".xls'>" & excelName & "</a>"

		book.close false
	set sheet = nothing
	set book  = nothing
		excel.quit
	set excel = nothing
	set works = nothing
	set keys  = nothing
%>