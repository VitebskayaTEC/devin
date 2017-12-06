<!-- #include virtual ="/devin/core/core.inc" -->
<!DOCTYPE html>
<html>

<head>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<link rel="shortcut icon" href="/devin/img/favicon.ico" type="image/x-icon" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/analyze.css" rel="stylesheet" />
	<title>DEVIN | Расход картриджей</title>
</head>

<body>
	<% 
		menu("<li><a onclick='exportToExcel()'>Печать заказа на закупку картриджей</a>")

		dim conn: set conn = Server.CreateObject("ADODB.Connection")
		dim rs:   set rs = Server.CreateObject("ADODB.Recordset")
		conn.open everest


		' Минимальная базовая величина, используется в расчетах стоимости
		const minBaseValue = 23

		' Текст SQL запросов
		dim sql
		' Массив для сбора информации по картриджам и его счетчик
		dim cartridges(3000, 8)
		dim Ncartridges: Ncartridges = -1
		' Переменная-буфер для значения стоимости
		dim cost, account, fixcost
		' Переменная-буфер для значения ИД картриджа
		dim cartID: cartID = ""
		dim tempCartID
		dim defPrice: defPrice = 0

		' Получение данных по всем типовым картриджам и их наличию на складе
		sql = "SELECT " _
			& "CARTRIDGE.N AS Cartridge, " _
			& "CARTRIDGE.Type AS Type, " _
			& "CARTRIDGE.Caption AS Caption, " _
			& "SKLAD.Nis AS Count, " _
			& "SKLAD.Uchet AS Account, " _
			& "SKLAD.Price AS Price, " _
			& "CARTRIDGE.Price AS DefPrice, " _
			& "SKLAD.ID_cart AS ID, " _
			& "CARTRIDGE.Type AS AltType, " _
			& "CARTRIDGE.Color AS Color " _
			& "FROM CARTRIDGE " _
			& "INNER JOIN Sklad ON CARTRIDGE.N = SKLAD.ID_cart " _		
			& "ORDER BY Type, Caption"
			
		rs.open sql, conn
		do while not rs.eof
			' Получение промежуточного значения ИД картриджа для сравнения
			tempCartID = rs("Cartridge")

			' Сравнение текущего значения с сохраненным
			if tempCartID = cartID then

				' Если значение сохраняется, то происходит проверка счета учета и, если надо, пересчет стоимости, а также увеличение количества на складе
				' Количество картриджей такого типа на складе
				cartridges(Ncartridges, 2) = cartridges(Ncartridges, 2) + rs("Count")
				' Вычисление стоимости
				account = rs("Account")
				cost    = rs("Price")
				' Если счет учета - материалы, то стоимость берется с половинным коэффициентом, т.к. часть стоимости возмещена
				if account = "10.5.1" then fixcost = 1.2 else fixcost = 2.4
				' Если стоимость нулевая, то берется базовая величина
				if cost = "0" then cost = minBaseValue * 1.2 else cost = cost * fixcost
				' Если полученное значение стоимости больше, чем предыдущее, будет выбрано новое
				defPrice = rs("DefPrice")
				if isNull(defPrice) or defPrice = "" or defPrice = "0" or not isNumeric(defPrice) then	
					if cost > cartridges(Ncartridges, 5) then cartridges(Ncartridges, 5) = round(cost, 2)
				end if

			else

				' Если значение новое, то происходит создание новой записи в массиве картриджей, а так же задание первичного значения для стоимости
				Ncartridges = Ncartridges + 1
				' Сохранение текущей записи как включенной в массив
				cartID = tempCartID
				' Тип картриджа
				cartridges(Ncartridges, 0) = rs("Type")
				select case cartridges(Ncartridges, 0)
					case "flow"   
						cartridges(Ncartridges, 0) = "Картридж струйный"
					case "laser"  
						cartridges(Ncartridges, 0) = "Тонер-картридж"
					case "matrix" 
						cartridges(Ncartridges, 0) = "Картридж матричный"
					case else     
						cartridges(Ncartridges, 0) = "Картридж"
				end select
				' Название картриджа
				cartridges(Ncartridges, 1) = rs("Caption")
				' Количество картриджей такого типа на складе
				cartridges(Ncartridges, 2) = rs("Count")
				' Флаг "если ли ремонты"
				cartridges(Ncartridges, 3) = false
				' Количество, предлагаемое к закупке
				cartridges(Ncartridges, 4) = 0
				' Вычисление стоимости
				account = rs("Account")
				cost    = rs("Price")
				' Если счет учета - материалы, то стоимость берется с половинным коэффициентом, т.к. часть стоимости возмещена
				if account = "10.5.1" then fixcost = 1.2 else fixcost = 2.4
				' Если стоимость нулевая, то берется базовая величина
				if cost = "0" then cost = minBaseValue * fixcost else cost = cost * fixcost
				' Если в каталоге не явно задана стоимость, то присваивается стоимость, которая получена на основе расчета
				defPrice = rs("DefPrice")
				if isNull(defPrice) or defPrice = "" or defPrice = "0" or not isNumeric(defPrice) then	
					cartridges(Ncartridges, 5) = round(cost, 2) 
				else
					cartridges(Ncartridges, 5) = ccur(defPrice)
				end if
				' ИД картриджа
				cartridges(Ncartridges, 6) = rs("ID")
				' Тип картриджа из базы для передачи в печатную форму
				cartridges(Ncartridges, 7) = rs("AltType")
				' Цвет картриджа из базы для передачи в печатную форму
				cartridges(Ncartridges, 8) = rs("Color")

			end if
		
			rs.movenext
		loop
		rs.close


		' Массив для промежуточного сохранения данных по ремонтам пары принтер-картридж
		dim data(3000, 8)
		' Счетчик длины массива промежуточных данных
		dim N: N = 0
		dim i
		' Переменная - результат вычисления количества предполагаемых замен
		dim forecast: forecast = 0
		' Переменная - сумма количества установленных картриджей для пары
		dim amount: amount = 0
		' Переменная - время между заменами картриджей
		dim rate: rate = 0
		' Переменные для хранения идентификаторов просматриваемой пары и текущей пары под курсором
		dim activePrinter,   cursorPrinter
		dim activeCartridge, cursorCartridge
		' Временные границы
		dim firstDate, lastDate, differenceDate
		' Переменная, задающая временной промежуток, на который выполняется расчет расхода
		dim limit: limit = 90
		' Промежуток времени от последнего ремонта
		dim leftDate
		' Cookie
		dim cookie

		response.write "<div class='view' id='view'>"

		'Получение списка всех замен картриджей за все время
		sql = "SELECT " _
			& "DEVICE.number_device AS ID_P, " _
			& "SKLAD.ID_cart AS ID_C, " _
			& "COMPUTERS.name AS Computer, " _
			& "DEVICE.description AS Device, " _
			& "PRINTER.Caption AS Printer, " _
			& "REMONT.Date AS Date, " _
			& "REMONT.Units AS N, " _
			& "CARTRIDGE.Caption AS Cartridge, " _
			& "SKLAD.Price, " _
			& "SKLAD.Uchet AS Account " _

			& "FROM REMONT " _
			& "INNER JOIN DEVICE ON REMONT.ID_D = DEVICE.number_device " _
			& "INNER JOIN DEVICE AS COMPUTERS ON DEVICE.number_comp = COMPUTERS.number_device " _
			& "INNER JOIN SKLAD ON REMONT.ID_U = SKLAD.NCard " _
			& "INNER JOIN PRINTER ON DEVICE.ID_prn = PRINTER.N " _
			& "INNER JOIN CARTRIDGE ON SKLAD.ID_cart = CARTRIDGE.N " _

			& "WHERE " _
			& "(REMONT.ID_D LIKE '%PRN%') " _
			& "AND (REMONT.Virtual = 0) " _

			& "ORDER BY ID_P, ID_C, Date"

		' Начинаем просмотр и обработку данных по ремонтам
		rs.open sql, conn

			' Отрисовка данных по ремонтам в отдельном блоке
			cookie = request.cookies("analyzeRepairs")
			response.write "<div class='unit " & cookie & "' id='analyzeRepairs'><table class='caption'><tr><td><th>Данные по ремонтам</tr></table>"
			if cookie = "open" then response.write "<div class='items_block'>" else response.write "<div class='items_block hide'>"
			response.write "<table class='items'>" _
			& "<thead><tr>" _
			& "<th>Компьютер" _
			& "<th>Устройство" _
			& "<th>Тип устройства" _
			& "<th>Дата" _
			& "<th>Кол-во" _
			& "<th>Тип картриджа" _
			& "<th>Стоимость, шт" _
			& "<th>Счет учета" _
			& "</tr></thead>" _
			& "<tbody>"

			' Пока не начался обход, активные элементы занулены. Поэтому их не нужно рассчитывать
			activePrinter   = ""
			activeCartridge = ""

			' Обход всех данных по ремонтам
			do while not rs.eof

				' Получаем идентификаторы принтера и картриджа просматриваемой записи о ремонте
				cursorPrinter   = rs("ID_P")
				cursorCartridge = rs("ID_C")

				' Если новая пара принтер-картридж
				if cursorPrinter <> activePrinter or cursorCartridge <> activeCartridge then				

					' Проверка на первый раз (так как при первом срабатывании еще нет данных)
					if activePrinter <> "" then analyzer()

					' Показываем, что мы начали работу с этой парой принтер-картридж
					activePrinter   = cursorPrinter
					activeCartridge = cursorCartridge

					' Сброс массива с данными по ремонтам пары
					N = 0
					
				else 
					' Если тот же самый принтер и картридж, то добавление в массив данных
				end if

				' Получение данных по очередному ремонту и сохранение в массив промежуточных значений	
				response.write "<tr>"
				for i = 0 to 7
					data(N, i) = rs(i + 2)
					response.write "<td>" & data(N, i)
				next
				response.write "</tr>"

				N = N + 1

				rs.movenext

			loop

			analyzer()

			response.write "</tbody></table></div></div>"

		rs.close
		
		conn.close
		set rs   = nothing
		set conn = nothing

		
		' Отрисовка данных по всем типовым картриджам в отдельном блоке
		cookie = request.cookies("analyzeCartridges")
		response.write "<div class='unit group " & cookie & "' id='analyzeCartridges'>" _
		& "<table class='caption'><tr><td><th>Прогноз закупки картриджей</tr></table>"
		if cookie = "open" then response.write "<div class='items_block'>" else response.write "<div class='items_block hide'>"
		response.write "<table class='items'>" _
		& "<thead><tr>" _
		& "<th data-type='string'>Тип картриджа" _
		& "<th data-type='unique'>Наименование картриджа" _
		& "<th data-type='string'>Стоимость" _
		& "<th data-type='number'>На складе" _
		& "<th data-type='string'>Прогноз" _
		& "<th data-type='none'>Итог" _
		& "</tr></thead>" _
		& "<tbody>"

		for i = 0 to Ncartridges
			response.write "<tr class='item'>" _
			& "<td>" & cartridges(i, 0) _
			& "<td><a href='/devin/catalog/##cart" & cartridges(i, 6) & "'>" & cartridges(i, 1) & "</a>" _
			& "<td>" & cartridges(i, 5) _
			& "<td>" & cartridges(i, 2) _
			& "<td>"
			
			' Определение того, нужно ли заказывать картриджи (оповещение через отдельный столбец в таблице)
			if cartridges(i, 3) then 
				forecast = cartridges(i, 4) - cartridges(i, 2)
				if forecast > 0 then
					response.write "Предлагается заказать " & forecast & " шт."
				elseif cartridges(i, 2) = 0 then
					forecast = 1
					response.write "Предлагается заказать 1 картридж для наличия резерва (замены не планируются)"	
				else
					forecast = 0
					response.write "Заказ картриджей не требуется"	
				end if
			elseif cartridges(i, 2) = 0 then
				forecast = 1
				response.write "Предлагается заказать 1 картридж для наличия резерва (не проведено ни одного ремонта)"	
			else
				forecast = 0
				response.write "Замены картриджей не проводились"
			end if
			response.write "<td><input type='number' color='" & cartridges(i, 8) & "' name='" & cartridges(i, 7) & "' value='" & forecast & "' /></tr>"
		next

		response.write "</tbody></table></div></div></div>"

		sub analyzer()
			' Если это не первая пара в списке, то нужно рассчитать предыдущую пару
			' Нужно вычислить среднее время между заменами для этой пар
			response.write "<tr><th colspan='8'><br/>"

			' Проверяем количество ремонтов для этой пары
			if N = 1 then

				' Если ремонт всего один, нужно записать в предполагаемый расход одну замену
				lastDate = data(0, 3)
				differenceDate = datediff("d", lastDate, date)

				if differenceDate > limit then
					forecast = 1
					response.write "Недостаточно данных. Предполагается <b>1</b> замена<br/>"
				else
					forecast = 0
					response.write "Недостаточно данных. Замены не предполагаются<br/>"
				end if
			else 
				' Расчет временных рамок
				firstDate      = data(0, 3)
				lastDate       = data(N - 1, 3)
				differenceDate = datediff("d", firstDate, date)

				amount = 0
				' Расчет суммы использованных картриджей
				for i = 0 to N - 1
					amount = amount + data(i, 4)
				next

				' Расчет количества дней между заменами картриджей
				rate = fix( differenceDate / amount ) + 1

				' Вычисление количества дней с последней замены
				leftDate = datediff("d", lastDate, date)

				' Вычисление предполагаемого количества замен
				forecast = fix( limit / rate) + 1

				response.write "Количество дней между первым и последним ремонтами: <b>" & differenceDate & "</b><br/>"
				response.write "Промежуток между заменами, дней: <b>" & rate & "</b><br/>"
				response.write "Дней с последнего ремонта: <b>" & leftDate & "</b><br/>"
				response.write "Предполагаемые замены: <b>" & forecast & "</b><br/>"
			end if

			for i = 0 to Ncartridges
				if cursorCartridge = cartridges(i, 6) then
					cartridges(i, 4) = cartridges(i, 4) + forecast
					cartridges(i, 3) = true
					exit for
				end if
			next
			
			response.write "<br/></th></tr>"
		end sub
	%>

	<div id='export' class='panel'>
		<div></div>
		<div><a onclick='$(this).closest(".panel").slideUp(100)'>Закрыть</a></div>
	</div>

	<script src='/devin/js/jquery-1.12.4.min.js'></script>
	<script src="/devin/js/jquery-ui.min.js"></script>
	<script src='/devin/js/core.js'></script>
	<script src='/devin/js/js-analyze.js'></script>	
</body>	

</html>