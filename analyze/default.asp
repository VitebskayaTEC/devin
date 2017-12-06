<!-- #include virtual ="/devin/core/core.inc" -->
<!DOCTYPE html>
<html>

<head>
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<link rel="shortcut icon" href="/devin/img/favicon.ico" type="image/x-icon" />
	<link href="/devin/css/core.css" rel="stylesheet" />
	<link href="/devin/css/analyze.css" rel="stylesheet" />
	<title>DEVIN | ������ ����������</title>
</head>

<body>
	<% 
		menu("<li><a onclick='exportToExcel()'>������ ������ �� ������� ����������</a>")

		dim conn: set conn = Server.CreateObject("ADODB.Connection")
		dim rs:   set rs = Server.CreateObject("ADODB.Recordset")
		conn.open everest


		' ����������� ������� ��������, ������������ � �������� ���������
		const minBaseValue = 23

		' ����� SQL ��������
		dim sql
		' ������ ��� ����� ���������� �� ���������� � ��� �������
		dim cartridges(3000, 8)
		dim Ncartridges: Ncartridges = -1
		' ����������-����� ��� �������� ���������
		dim cost, account, fixcost
		' ����������-����� ��� �������� �� ���������
		dim cartID: cartID = ""
		dim tempCartID
		dim defPrice: defPrice = 0

		' ��������� ������ �� ���� ������� ���������� � �� ������� �� ������
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
			' ��������� �������������� �������� �� ��������� ��� ���������
			tempCartID = rs("Cartridge")

			' ��������� �������� �������� � �����������
			if tempCartID = cartID then

				' ���� �������� �����������, �� ���������� �������� ����� ����� �, ���� ����, �������� ���������, � ����� ���������� ���������� �� ������
				' ���������� ���������� ������ ���� �� ������
				cartridges(Ncartridges, 2) = cartridges(Ncartridges, 2) + rs("Count")
				' ���������� ���������
				account = rs("Account")
				cost    = rs("Price")
				' ���� ���� ����� - ���������, �� ��������� ������� � ���������� �������������, �.�. ����� ��������� ���������
				if account = "10.5.1" then fixcost = 1.2 else fixcost = 2.4
				' ���� ��������� �������, �� ������� ������� ��������
				if cost = "0" then cost = minBaseValue * 1.2 else cost = cost * fixcost
				' ���� ���������� �������� ��������� ������, ��� ����������, ����� ������� �����
				defPrice = rs("DefPrice")
				if isNull(defPrice) or defPrice = "" or defPrice = "0" or not isNumeric(defPrice) then	
					if cost > cartridges(Ncartridges, 5) then cartridges(Ncartridges, 5) = round(cost, 2)
				end if

			else

				' ���� �������� �����, �� ���������� �������� ����� ������ � ������� ����������, � ��� �� ������� ���������� �������� ��� ���������
				Ncartridges = Ncartridges + 1
				' ���������� ������� ������ ��� ���������� � ������
				cartID = tempCartID
				' ��� ���������
				cartridges(Ncartridges, 0) = rs("Type")
				select case cartridges(Ncartridges, 0)
					case "flow"   
						cartridges(Ncartridges, 0) = "�������� ��������"
					case "laser"  
						cartridges(Ncartridges, 0) = "�����-��������"
					case "matrix" 
						cartridges(Ncartridges, 0) = "�������� ���������"
					case else     
						cartridges(Ncartridges, 0) = "��������"
				end select
				' �������� ���������
				cartridges(Ncartridges, 1) = rs("Caption")
				' ���������� ���������� ������ ���� �� ������
				cartridges(Ncartridges, 2) = rs("Count")
				' ���� "���� �� �������"
				cartridges(Ncartridges, 3) = false
				' ����������, ������������ � �������
				cartridges(Ncartridges, 4) = 0
				' ���������� ���������
				account = rs("Account")
				cost    = rs("Price")
				' ���� ���� ����� - ���������, �� ��������� ������� � ���������� �������������, �.�. ����� ��������� ���������
				if account = "10.5.1" then fixcost = 1.2 else fixcost = 2.4
				' ���� ��������� �������, �� ������� ������� ��������
				if cost = "0" then cost = minBaseValue * fixcost else cost = cost * fixcost
				' ���� � �������� �� ���� ������ ���������, �� ������������� ���������, ������� �������� �� ������ �������
				defPrice = rs("DefPrice")
				if isNull(defPrice) or defPrice = "" or defPrice = "0" or not isNumeric(defPrice) then	
					cartridges(Ncartridges, 5) = round(cost, 2) 
				else
					cartridges(Ncartridges, 5) = ccur(defPrice)
				end if
				' �� ���������
				cartridges(Ncartridges, 6) = rs("ID")
				' ��� ��������� �� ���� ��� �������� � �������� �����
				cartridges(Ncartridges, 7) = rs("AltType")
				' ���� ��������� �� ���� ��� �������� � �������� �����
				cartridges(Ncartridges, 8) = rs("Color")

			end if
		
			rs.movenext
		loop
		rs.close


		' ������ ��� �������������� ���������� ������ �� �������� ���� �������-��������
		dim data(3000, 8)
		' ������� ����� ������� ������������� ������
		dim N: N = 0
		dim i
		' ���������� - ��������� ���������� ���������� �������������� �����
		dim forecast: forecast = 0
		' ���������� - ����� ���������� ������������� ���������� ��� ����
		dim amount: amount = 0
		' ���������� - ����� ����� �������� ����������
		dim rate: rate = 0
		' ���������� ��� �������� ��������������� ��������������� ���� � ������� ���� ��� ��������
		dim activePrinter,   cursorPrinter
		dim activeCartridge, cursorCartridge
		' ��������� �������
		dim firstDate, lastDate, differenceDate
		' ����������, �������� ��������� ����������, �� ������� ����������� ������ �������
		dim limit: limit = 90
		' ���������� ������� �� ���������� �������
		dim leftDate
		' Cookie
		dim cookie

		response.write "<div class='view' id='view'>"

		'��������� ������ ���� ����� ���������� �� ��� �����
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

		' �������� �������� � ��������� ������ �� ��������
		rs.open sql, conn

			' ��������� ������ �� �������� � ��������� �����
			cookie = request.cookies("analyzeRepairs")
			response.write "<div class='unit " & cookie & "' id='analyzeRepairs'><table class='caption'><tr><td><th>������ �� ��������</tr></table>"
			if cookie = "open" then response.write "<div class='items_block'>" else response.write "<div class='items_block hide'>"
			response.write "<table class='items'>" _
			& "<thead><tr>" _
			& "<th>���������" _
			& "<th>����������" _
			& "<th>��� ����������" _
			& "<th>����" _
			& "<th>���-��" _
			& "<th>��� ���������" _
			& "<th>���������, ��" _
			& "<th>���� �����" _
			& "</tr></thead>" _
			& "<tbody>"

			' ���� �� ������� �����, �������� �������� ��������. ������� �� �� ����� ������������
			activePrinter   = ""
			activeCartridge = ""

			' ����� ���� ������ �� ��������
			do while not rs.eof

				' �������� �������������� �������� � ��������� ��������������� ������ � �������
				cursorPrinter   = rs("ID_P")
				cursorCartridge = rs("ID_C")

				' ���� ����� ���� �������-��������
				if cursorPrinter <> activePrinter or cursorCartridge <> activeCartridge then				

					' �������� �� ������ ��� (��� ��� ��� ������ ������������ ��� ��� ������)
					if activePrinter <> "" then analyzer()

					' ����������, ��� �� ������ ������ � ���� ����� �������-��������
					activePrinter   = cursorPrinter
					activeCartridge = cursorCartridge

					' ����� ������� � ������� �� �������� ����
					N = 0
					
				else 
					' ���� ��� �� ����� ������� � ��������, �� ���������� � ������ ������
				end if

				' ��������� ������ �� ���������� ������� � ���������� � ������ ������������� ��������	
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

		
		' ��������� ������ �� ���� ������� ���������� � ��������� �����
		cookie = request.cookies("analyzeCartridges")
		response.write "<div class='unit group " & cookie & "' id='analyzeCartridges'>" _
		& "<table class='caption'><tr><td><th>������� ������� ����������</tr></table>"
		if cookie = "open" then response.write "<div class='items_block'>" else response.write "<div class='items_block hide'>"
		response.write "<table class='items'>" _
		& "<thead><tr>" _
		& "<th data-type='string'>��� ���������" _
		& "<th data-type='unique'>������������ ���������" _
		& "<th data-type='string'>���������" _
		& "<th data-type='number'>�� ������" _
		& "<th data-type='string'>�������" _
		& "<th data-type='none'>����" _
		& "</tr></thead>" _
		& "<tbody>"

		for i = 0 to Ncartridges
			response.write "<tr class='item'>" _
			& "<td>" & cartridges(i, 0) _
			& "<td><a href='/devin/catalog/##cart" & cartridges(i, 6) & "'>" & cartridges(i, 1) & "</a>" _
			& "<td>" & cartridges(i, 5) _
			& "<td>" & cartridges(i, 2) _
			& "<td>"
			
			' ����������� ����, ����� �� ���������� ��������� (���������� ����� ��������� ������� � �������)
			if cartridges(i, 3) then 
				forecast = cartridges(i, 4) - cartridges(i, 2)
				if forecast > 0 then
					response.write "������������ �������� " & forecast & " ��."
				elseif cartridges(i, 2) = 0 then
					forecast = 1
					response.write "������������ �������� 1 �������� ��� ������� ������� (������ �� �����������)"	
				else
					forecast = 0
					response.write "����� ���������� �� ���������"	
				end if
			elseif cartridges(i, 2) = 0 then
				forecast = 1
				response.write "������������ �������� 1 �������� ��� ������� ������� (�� ��������� �� ������ �������)"	
			else
				forecast = 0
				response.write "������ ���������� �� �����������"
			end if
			response.write "<td><input type='number' color='" & cartridges(i, 8) & "' name='" & cartridges(i, 7) & "' value='" & forecast & "' /></tr>"
		next

		response.write "</tbody></table></div></div></div>"

		sub analyzer()
			' ���� ��� �� ������ ���� � ������, �� ����� ���������� ���������� ����
			' ����� ��������� ������� ����� ����� �������� ��� ���� ���
			response.write "<tr><th colspan='8'><br/>"

			' ��������� ���������� �������� ��� ���� ����
			if N = 1 then

				' ���� ������ ����� ����, ����� �������� � �������������� ������ ���� ������
				lastDate = data(0, 3)
				differenceDate = datediff("d", lastDate, date)

				if differenceDate > limit then
					forecast = 1
					response.write "������������ ������. �������������� <b>1</b> ������<br/>"
				else
					forecast = 0
					response.write "������������ ������. ������ �� ��������������<br/>"
				end if
			else 
				' ������ ��������� �����
				firstDate      = data(0, 3)
				lastDate       = data(N - 1, 3)
				differenceDate = datediff("d", firstDate, date)

				amount = 0
				' ������ ����� �������������� ����������
				for i = 0 to N - 1
					amount = amount + data(i, 4)
				next

				' ������ ���������� ���� ����� �������� ����������
				rate = fix( differenceDate / amount ) + 1

				' ���������� ���������� ���� � ��������� ������
				leftDate = datediff("d", lastDate, date)

				' ���������� ��������������� ���������� �����
				forecast = fix( limit / rate) + 1

				response.write "���������� ���� ����� ������ � ��������� ���������: <b>" & differenceDate & "</b><br/>"
				response.write "���������� ����� ��������, ����: <b>" & rate & "</b><br/>"
				response.write "���� � ���������� �������: <b>" & leftDate & "</b><br/>"
				response.write "�������������� ������: <b>" & forecast & "</b><br/>"
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
		<div><a onclick='$(this).closest(".panel").slideUp(100)'>�������</a></div>
	</div>

	<script src='/devin/js/jquery-1.12.4.min.js'></script>
	<script src="/devin/js/jquery-ui.min.js"></script>
	<script src='/devin/js/core.js'></script>
	<script src='/devin/js/js-analyze.js'></script>	
</body>	

</html>