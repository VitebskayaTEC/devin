<!-- #include virtual ="/devin/core/core.inc" -->
<!DOCTYPE HTML>
<HTML>

<HEAD>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	<link rel="shortcut icon" href="/devin/img/favicon.ico" type="image/x-icon">
	<link href="style.css" rel="stylesheet" type="text/css">
	<link href="/devin/css/core.css" rel="stylesheet" />
	<title>DEVIN | Расход картриджей</title>
</HEAD>

<BODY>
	<% 
		menu("")

		response.write "<div class='view'>"
		
		'Составление массивов данных по принтерам и картриджам с уточнениями из других баз'
		Dim CONN, RS, SQL, base(1000,8), computers(1000,1), sklad(1000,4), printers(1000,8), tempPrinter(4), noPrinters(1000,5), parsed(10), cartridges(1000,8), excel(1000,5), template, baseN, computersN, printersN, noPrintersN, cartridgesN, excelN, skladN, i, j, prevPrinter, prevPrinterCN, newCartridge, searchCartridge, noPrinter
		Dim first, last, count, tap, tapN, diff, x, y, z, xCorr, diffCorr, zCorr, replace, prevP, tempS
		Dim limit : limit = 90
		Dim limitSingle : limitSingle = 90
  
		'Чтение запроса и выгрузка в массив'
		Set CONN = Server.CreateObject("ADODB.Connection")
		CONN.Open everest
		Set RS = Server.CreateObject("ADODB.Recordset")

		'Запрос на историю замен картриджей'
		SQL = "SELECT DEVICE.number_device, DEVICE.number_comp, PRINTER.Caption, CARTRIDGE.Caption AS Expr1, REMONT.Date, REMONT.Units, CARTRIDGE.N, SKLAD.Price, SKLAD.Uchet FROM DEVICE INNER JOIN REMONT ON REMONT.ID_D = DEVICE.number_device INNER JOIN SKLAD ON SKLAD.NCard = REMONT.ID_U INNER JOIN CARTRIDGE ON CARTRIDGE.N = SKLAD.ID_cart INNER JOIN PRINTER ON PRINTER.N = DEVICE.ID_prn WHERE (DEVICE.class_device = 'prn') AND (DEVICE.ID_prn IS NOT NULL) AND (DEVICE.number_comp NOT LIKE '%xxx%') AND (DEVICE.number_comp NOT LIKE '%списано%') AND (REMONT.Virtual = 0) ORDER BY DEVICE.number_device, Expr1, REMONT.Date"
		RS.Open SQL,CONN
		if not RS.EOF then
			baseN = -1	
			printersN = 0
			cartridgesN = 0
			prevPrinter = ""
			prevPrinterCN = ""
			do while not RS.EOF
				baseN = baseN + 1
				for i = 0 to 8
					base(baseN,i) = trim(RS(i))
				next
				'Построение базы по принтерам'
				if base(baseN,0) <> prevPrinter or base(baseN,6) <> prevPrinterCN then 
					printers(printersN,0) = base(baseN,0) 'ид принтера'
					printers(printersN,1) = base(baseN,1) 'ид компа'
					printers(printersN,2) = base(baseN,2) 'имя принтера'
					printers(printersN,3) = base(baseN,4) 'дата для сравнения'
					printers(printersN,5) = base(baseN,6) 'ид картриджа'
					printersN = 1 + printersN
					prevPrinter = base(baseN,0)
					prevPrinterCN = base(baseN,6)
				end if
				'Составление перечня используемых картриджей'
				i = 0
				newCartridge = true
				searchCartridge = false
				do while i < cartridgesN and not searchCartridge
					if base(baseN,6) = cartridges(i,0) then
						cartridges(i,2) = CInt(base(baseN,5)) + cartridges(i,2) 'кол-во'
						searchCartridge = true
						newCartridge = false
					end if
					i = 1 + i
				loop
				if newCartridge then 
					cartridges(cartridgesN,0) = base(baseN,6) 'ид картриджа'
					cartridges(cartridgesN,1) = base(baseN,3) 'имя картриджа'
					cartridges(cartridgesN,2) = CInt(base(baseN,5)) 'кол-во'
					cartridges(cartridgesN,2) = 0 'кол-во принтеров'
					'Стоимость единицы'
					if base(baseN,8) = "10.5.1" then 
						cartridges(cartridgesN,6) = base(baseN,7) 
					else 
						if base(baseN,7) = "0" then 
							cartridges(cartridgesN,6) = "20"
						else 
							cartridges(cartridgesN,6) = cdbl(base(baseN,7)) / 2
						end if
					end if
					cartridgesN = 1 + cartridgesN
				end if
				RS.MoveNext
			loop
		end if
		RS.Close

		'Запрос на имена компов'
		SQL = "SELECT number_device, name FROM DEVICE WHERE (class_device = 'cmp')"
		RS.Open SQL,CONN
		if not RS.EOF then
			i = 0
			do while not RS.EOF
				computers(i,0) = trim(RS(0)) 'ид компа'
				computers(i,1) = trim(RS(1)) 'имя компа'
				for j = 0 to printersN
					if printers(j,1) = computers(i,0) then printers(j,4) = computers(i,1) 'имя компа'
				next
				i = 1 + i
				RS.MoveNext
			loop
		end if
		RS.Close

		'Запрос на неучаствовавшие в ремонтах принтеры'
		SQL = "SELECT DEVICE.number_device, DEVICE_1.name, PRINTER.Caption, CARTRIDGE.Caption AS Expr1, CARTRIDGE.N FROM DEVICE INNER JOIN DEVICE AS DEVICE_1 ON DEVICE.number_comp = DEVICE_1.number_device INNER JOIN PRINTER ON PRINTER.N = DEVICE.ID_prn INNER JOIN OFFICE ON PRINTER.N = OFFICE.Printer INNER JOIN CARTRIDGE ON OFFICE.Cartridge = CARTRIDGE.N WHERE (DEVICE.class_device = 'prn') AND (DEVICE.ID_prn IS NOT NULL) AND (DEVICE.number_comp NOT LIKE '%xxx%') AND (DEVICE.number_comp NOT LIKE '%списано%') ORDER BY DEVICE_1.name, PRINTER.Caption, Expr1"
		RS.Open SQL,CONN
		if not RS.EOF then
			i = 0
			do while not RS.EOF
				for j = 0 to 4
					tempPrinter(j) = trim(RS(j))
				next
				noPrinter = false
				for j = 0 to printersN - 1
					if noPrinter = false then if printers(j,0) = tempPrinter(0) then noPrinter = true
				next
				if not noPrinter then 
					for j = 0 to 4
						noPrinters(i,j) = tempPrinter(j)
					next
					i = 1 + i
				end if
				RS.MoveNext
			loop
			noPrintersN = i - 1
		end if
		RS.Close

		'Запрос на наличие картриджей'
		SQL = "SELECT CARTRIDGE.N, SUM(SKLAD.Nis) AS Expr1, CARTRIDGE.Price, CARTRIDGE.Type, CARTRIDGE.Color FROM SKLAD INNER JOIN CARTRIDGE ON CARTRIDGE.N = SKLAD.ID_cart GROUP BY CARTRIDGE.N, CARTRIDGE.Price, CARTRIDGE.Type, CARTRIDGE.Color, CARTRIDGE.Caption ORDER BY CARTRIDGE.N"
		RS.Open SQL,CONN
		if not RS.EOF then
			i = 0
			do while not RS.EOF
				for j = 0 to 4
					sklad(i,j) = trim(RS(j))
				next
				for j = 0 to cartridgesN
					if cartridges(j,0) = sklad(i,0) then 
						cartridges(j,3) = sklad(i,1) 'кол-во на складе'
						if sklad(i,2) <> "" then cartridges(j,6) = sklad(i,2) 'типовая цена картриджа'
						cartridges(j,7) = sklad(i,3) 'тип картриджа'
						cartridges(j,8) = sklad(i,4) 'цвет картриджа'
					end if
				next
				for j = 0 to noPrintersN
					if noPrinters(j,4) = sklad(i,0) then noPrinters(j,5) = sklad(i,1) 'кол-во на складе'
				next
				i = 1 + i
				RS.MoveNext
			loop
		end if
		RS.Close
		
		'Запрос шаблона таблицы для заявки'
		SQL = "SELECT Data FROM DATABANK WHERE Provider = 'CartridgeAnalyze'"
		RS.Open SQL,CONN
		if not RS.EOF then
			template = split(RS(0),";;")
			Select case Month(date)
				case 0, 1, 2 
					template(6) = template(6) & " в I квартале " & Year(date) & " г."
				case 3, 4, 5 
					template(6) = template(6) & " в II квартале " & Year(date) & " г."
				case 6, 8 
					template(6) = template(6) & " в III квартале " & Year(date) & " г."
				case 9, 11 
					template(6) = template(6) & " в IV квартале " & Year(date) & " г."
			end select
		end if
		RS.Close

		Set RS = Nothing
		CONN.Close
		Set CONN = Nothing
	%>
	
	<div>
		<a href="#" onclick="view(this)">История замен по принтерам</a>
		<table class='notTR' style="display: none">
			<tr>
				<td class='Tx'>Компьютер
				<td class='Tx'>Принтер
				<td class='Tx'>Картридж
				<td class='Tx'>Дата/последний
				<td class='Tx'>Кол-во/срок работы
				<td class='Tx'>Отработал
				<td class='Tx'>Осталось
			</tr>
	
			<% 'Математика и вывод информации по принтерам и ремонтам'
				for i = 0 to printersN - 1
					Response.Write("<tr><td>" & printers(i,4) & "<td>" & printers(i,2) & "")
					first = printers(i,3)
					last = printers(i,3)
					count = 0
					erase parsed
					for j = 0 to baseN
						if printers(i,0) = base(j,0) and printers(i,5) = base(j,6) then
							parsed(0) = base(j,3) + "<BR>" + parsed(0) 'имя картриджа'
							parsed(1) = base(j,4) + "<BR>" + parsed(1) 'дата установки'
							parsed(2) = base(j,5) + "<BR>" + parsed(2) 'кол-во установлено'
							parsed(3) = "--<BR>" + parsed(3) 'выровнять'
							if DateDiff("d",base(j,4),first) > 0 then first = base(j,4)
							if DateDiff("d",base(j,4),last) < 0 then last = base(j,4)
							count = base(j,5) + count
							tap = base(j,3)
						end if
					next
					diff = DateDiff("d",first,last) 'временной промежуток для расчетов'
					if diff = 0 then 
						x = DateDiff("d",last,date)
						if x > limitSingle then 'проверка на слишком долгий единственный ремонт'
							Response.Write("<td>" & parsed(0) & "<td>" & parsed(1) & "<td>" & parsed(2) & "<td>" & x & "<td>? [1]</tr>")
							for j = 0 to cartridgesN - 1
								if cartridges(j,0) = printers(i,5) then 
									cartridges(j,4) = cartridges(j,4) + 1 'расчетное кол-по замен'
									if cartridges(j,5) = "" or instr(cartridges(j,5), printers(i,2)) = 0 then
										cartridges(j,5) = printers(i,2) 'кол-во принтеров'
									end if
								end if
							next
						else
							Response.Write("<td>" & parsed(0) & "<td>" & parsed(1) & "<td>" & parsed(2) & "<td>" & x & "<td>? [0]</tr>")
							for j = 0 to cartridgesN - 1
								if cartridges(j,0) = printers(i,5) then 
									if cartridges(j,5) = "" or instr(cartridges(j,5), printers(i,2)) = 0 then
										cartridges(j,5) = printers(i,2) 'кол-во принтеров'
									end if
								end if
							next
						end if	
					else 
						x = round(diff/count) 'срок работы картриджа'
						y = DateDiff("d",last,date) 'прошло дней с последней установки'
						z = x - y 'оставшееся время работы текущего картриджа'
						'корректировка в случае превышения срока работы'
						xCorr = 0
						if y > x then
							diffCorr = DateDiff("d",first,date)
							xCorr = round (diffCorr/(count+1))
							zCorr = xCorr - y
						end if
						'Расчет необходимости замены'
						if z > limit then 
							replace = 0 
						else 
							if xCorr = 0 then 
								if z < 0 then replace = abs(fix((limit)/x)) + 1 else replace = abs(fix((limit-z)/x)) + 1 
							else 
								if zCorr < 0 then replace = abs(fix((limit)/xCorr)) + 1 else replace = abs(fix((limit-zCorr)/xCorr)) + 1 'необходимость замен за лимит времени'
							end if
						end if
						if replace > 0 then 
							for j = 0 to cartridgesN - 1
								if cartridges(j,0) = printers(i,5) then 
									cartridges(j,4) = cartridges(j,4) + replace 'расчетное кол-по замен'
									if cartridges(j,5) = "" or instr(cartridges(j,5), printers(i,2)) = 0 then
										cartridges(j,5) = printers(i,2) 'кол-во принтеров'
									end if
								end if
							next
						end if
						'Вывод'
						if xCorr = 0 then
							Response.Write("<td>" & parsed(0) & "------------------<br>--<td>" & parsed(1) & "------------------<br>" & x & "<td>" & parsed(2) & "------------------<br>" & last & "<td>" & parsed(3) & "------------------<br>" & y & "<td>" & parsed(3) & "------------------<br>" & z & " [" & replace & "]</tr>")
						else 
							Response.Write("<td>" & parsed(0) & "------------------<br>--<td>" & parsed(1) & "------------------<br>" & xCorr  & " (к) <td>" & parsed(2) & "------------------<br>" & last & "<td>" & parsed(3) & "------------------<br>" & y & "<td>" & parsed(3) & "------------------<br>" & zCorr & " [" & replace & "]</tr>")
						end if
					end if
				next
			%>
	
		</table>
	</div>
	
	<div>
		<a href="#" onclick="view(this)">Прогноз закупки картриджей</a>
		<table class='notTR' style="display: none">
			<tr>
				<td class='Tx'>Задействованные в ремонтах картриджи
				<td class='Tx'>Кол-во принтеров
				<td class='Tx'>На складе
				<td class='Tx'>Предполагаемые замены
				<td class='Tx'>Надо закупить
			</tr>
	
			<% 'Математика и вывод информации по картриджам и анализу закупки'
				excelN = 0
				for i = 0 to cartridgesN - 1
					for j = 0 to noPrintersN
						if cartridges(i,0) = noPrinters(j,4) then
							cartridges(i,4) = CInt(cartridges(i,4)) + 1
							noPrinters(j,4) = "escape"
						end if
					next
					x = CInt(cartridges(i,3)) 'на складе'
					y = CInt(cartridges(i,4)) 'плановые замены на лимитное время'
					z = 0 'предлагается закупить на лимитное время (обнуляем)'
					'Расчет необходимости закупки'
					if x = 0 then 'если на складе нет'
						if y = 0 then 'если замены не планируются'
							z = 1 'докупить хотя бы один'
						else 'если замены планируются'
							z = y 'докупить нужное кол-во плюс один про запас'
						end if
					else 'если на складе есть'
						if y = 0 then 'если замены не планируются'
							z = 0 'хватает'
						else 'если замены планируются'
							if x > y then 'если есть запас'
								z = 0 'хватает'
							else 'если не хватит'
								z = y - x 'докупить нужное кол-во плюс один про запас'
							end if
						end if
					end if
					
					'Вывод'
					Response.Write("<tr><td>" & cartridges(i,1) & "<td>" & cartridges(i,5) & "<td>" & x & "<td>" & y & "<td>")
					if z = 0 then 
						Response.Write("не требуется") 
					else 
						Response.Write(z)
						'Составление таблицы для закупки'
						if cartridges(i,7) = "laser" then excel(excelN,0) = "Тонер-картридж " else excel(excelN,0) = "Принт-картридж "
						tempS = split(cartridges(i,5))
						on error resume next
						if tempS(0) = "Work" then excel(excelN,0) = excel(excelN,0) & "Work Centre" else excel(excelN,0) = excel(excelN,0) & tempS(0)
						if cartridges(i,8) = "black" then excel(excelN,1) = "черный"
						if cartridges(i,8) = "blue" then excel(excelN,1) = "голубой"
						if cartridges(i,8) = "red" then excel(excelN,1) = "пурпурный"
						if cartridges(i,8) = "yellow" then excel(excelN,1) = "желтый"
						excel(excelN,2) = cartridges(i,1)
						excel(excelN,3) = z
						excel(excelN,4) = cartridges(i,5)
						excel(excelN,5) = cartridges(i,6)
						excelN = 1 + excelN
					end if
					Response.Write("</tr>")
				next
			%>
	
		</table>
	</div>

	<div>
		<a href="#" onclick="view(this)">Незадействованные принтеры</a>
		<table class='notTR' style="display: none">
			<tr>
				<td class='Tx'>Компьютер
				<td class='Tx'>Принтер
				<td class='Tx'>Тип картриджа
				<td class='Tx'>Наличие на складе
			</tr>
	
			<% 'Вывод таблицы незадействованных принтеров'
				for i = 0 to noPrintersN
					if noPrinters(i,4) <> "escape" then response.write "<tr><td>" & noPrinters(i,1) & "<td>" & noPrinters(i,2) & "<td>" & noPrinters(i,3) & "<td>" & CInt(noPrinters(i,5)) & "</tr>"
				next
			%>	
	
		</table>
	</div>
	
	<p id="post">
		<b>Составление заявки на закупку картриджей</b>&emsp;<input type="button" value="Сохранить шаблон" onclick="saveData()"></input>&emsp;<input type="button" value="Печать" onclick="print()"></input>
	</p>
	<span id="save"></span>
	
	<table class="excel">
		<tr>
			<td>
			<td>
				<input name="T1" class="template" value="<%=template(0)%>"></input>
			<td colspan=2>
			<td colspan=3>
				<input name="T9" class="right template" value="<%=template(8)%>"></input>
			<td>
		</tr>
		<tr>
			<td>
			<td>
				<input name="T2" class="template" value="<%=template(1)%>"></input>
			<td colspan=2>
			<td colspan=3>
				<input name="T10" class="right template" value="<%=template(9)%>"></input>
			<td>
		</tr>
		<tr>
			<td>
			<td colspan=3>
			<td colspan=3>
				<input name="T11" class="right template" value="<%=template(10)%>"></input>
			<td>
		</tr>
		<tr>
			<td>
			<td colspan=6>
				<input name="T3" class="title template" value="<%=template(2)%>"></input>
			<td>
		</tr>
		<tr>
			<td>
			<td>
				<input name="T4" class="template" value="<%=DateValue(Date)%>"></input>
			<td colspan=5>
			<td>
		</tr>
		<tr>
			<td>
			<td>
				<input name="T5" class="template" value="<%=template(4)%>"></input>
			<td colspan=5>
			<td>
		</tr>
		<tr>
			<td>
			<td colspan=6>
				<input name="T6" class="title template" value="<%=template(5)%>"></input>
			<td>
		</tr>
		<tr>
			<td>
			<td class="center" colspan=6>
				<input name="T7" class="template" value="<%=template(6)%>"></input>
			<td>
		</tr>
		<tr>
			<td>
			<td class="center" colspan=6>
				<input name="T8" class="template" value="<%=template(7)%>"></input>
			<td>
		</tr>
		<tr>
			<td class='tab'>
			<td class="tableSeparate">Материал
			<td class="tableSeparate">Цвет
			<td class="tableSeparate">Тип / марка
			<td class="tableSeparate">Количество
			<td class="tableSeparate">Стоимость
			<td class="tableSeparate">Примечание
			<td class='tab'>
		</tr>
		<tr>
			<td class='tab'>
			<td class="tableHead" colspan=6>Лазерные картриджи
			<td class='tab'>
		</tr>

		<% 'Вывод таблицы закупок'
			dim summ : summ = 0 'Сумма стоимости всего заказа'
			for i = 0 to excelN - 1
				Response.Write("<tr class='tableRow'><td class='tab'><td><input name='exl-" & i & "-0' value='" & excel(i,0) & "'></input><td><input name='exl-" & i & "-1' value='" & excel(i,1) & "'></input><td><input name='exl-" & i & "-2' value='" & excel(i,2) & "'></input><td><input type='number' name='exl-" & i & "-3' value='" & excel(i,3) & "'></input><td><input name='exl-" & i & "-5' value='" & (excel(i,3) * excel(i,5)) & "'></input><td><textarea name='exl-" & i & "-4'>" & excel(i,4) & "</textarea><td class='tab'></tr>")
				summ = summ + (excel(i,3) * excel(i,5))
			next
		%>
		
		<tr class='tableRow'>
			<td class='tab'>
			<td>
			<td colspan=3>
			<td>
				<input name="T13" class="right template" value="<%=summ%>"></input>
			<td>
			<td class='tab'>
		</tr>
		<tr>
			<td>
			<td>
				<input name="T12" class="template" value="<%=template(11)%>"></input>
			<td colspan=4>
			<td>
				<input name="T13" class="right template" value="<%=template(12)%>"></input>
			<td>
		</tr>
	</table>
	
	
	<%
		Erase base
		Erase printers
		Erase noPrinters
		Erase tempPrinter
		Erase computers
		Erase cartridges
		Erase sklad
		Erase parsed
	%>
	
	</div>

	<script src='/devin/js/jquery-1.12.4.min.js'></script>	
	<script>
		function view(obj) {
			var table = obj.parentNode.getElementsByTagName("table")[0];
			if (table.style.display == "block") {table.style.display = "none"} else {table.style.display = "block"};
		}

		function print() {
			var table = document.querySelector(".excel");
			var body = "";
			for (var i = 0, n = table.getElementsByTagName("input"), l = n.length; i < l; i++) {
				if (i > 0) {
					body += "&";
				}
				body += n[i].name + "=" + encodeURIComponent(n[i].value);
			}
			for (var i = 0, n = table.getElementsByTagName("textarea"), l = n.length; i < l; i++) {
				body += "&" + n[i].name + "=" + encodeURIComponent(n[i].innerHTML);
			}
			$.post("exe.asp", body, function(data) {document.getElementById("save").innerHTML = data});
		}
		
		function saveData() {
			var table = document.querySelector(".excel");
			var body = "";
			for (var i = 0, n = table.querySelectorAll(".template"), l = n.length; i < l; i++) {
				if (i > 0) {
					body += "&";
				}
				body += n[i].name + "=" + encodeURIComponent(n[i].value);
			}
			//document.getElementById("save").innerHTML = body;
			$.post("save.asp", body, function(data) {document.getElementById("save").innerHTML = data});
		}	
	</script>

</BODY>	

</HTML>