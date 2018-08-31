<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")
	dim rs 		: set rs = server.createObject("ADODB.Recordset")
	dim id 		: id = request.querystring("id")

	dim wParams, wType, wName, wDate, defExcel

	dim drag(3)

	conn.open "Driver={SQL Server}; Server=WEB1\SQLCORE; Initial Catalog=site; Uid=core_user; Pwd=core_123;"
	rs.open "SELECT D_Gold,D_Silver,D_MPG FROM DragMetal", conn

	if not rs.eof then
		drag(0) = rs(0)
		drag(1) = rs(1)
		drag(2) = rs(2)
	end if

	rs.close
	conn.close

	conn.open everest

	sub drop(str)
		on error resume next
		response.write "<div class='error'>" & str & "</div>"
		book.close false
		set sheet = nothing
		set book = nothing
		excel.quit
		set excel = nothing
		set rs = nothing
		set conn = nothing
		response.end
	end sub

	' ��������� ������ �� �������� � ���� � ����� �������
	rs.open "SELECT writeoff.W_Params, writeoff.W_Type, writeoff.W_Name, writeoff.W_Date, catalog_writeoffs.O_Excel, catalog_writeoffs.O_Template FROM writeoff LEFT OUTER JOIN catalog_writeoffs ON writeoff.W_Type = catalog_writeoffs.O_Alias WHERE (W_ID = " & id & ")", conn
	if rs.eof then
		drop("��� ������ �� ������� ID")
	else
		wParams = rs(0)
		wType = rs(1)
		wName = rs(2)
		wDate = rs(3)
		defExcel = rs(4)
		' ���� ��������� � �������� �� ������, �������� ������ ������
		if wParams = "" or isnull(wParams) then wParams = rs(5)
	end if
	rs.close

	' �������� ������� � ����������� ����� �������
	if defExcel = "" or isnull(defExcel) then
		drop("�� ����� ������ ��� ��������</div>")
	else
		dim fso : set fso = createObject("Scripting.FileSystemObject")
		if not fso.fileExists(defExcel) then drop("������ ��� �������� �� ������ " & defExcel & "�� ������")
	end if

	dim month1 	: month1 = array ("������", "�������", "�����", "������", "���", "����", "����", "�������", "��������", "�������", "������", "�������")
	dim month2 	: month2 = array ("������", "�������", "�����", "������", "���", "����", "����", "�������", "��������", "�������", "������", "�������")

	dim excel 	: set excel = createObject("Excel.Application")
	excel.application.enableEvents = false
	excel.application.displayAlerts = false
	dim book 	: set book = excel.workbooks.open(defExcel)
	dim sheet, i, j, sum, f(200, 10)

	select case wType
		'���������������� �������'
		case "expl"
			set sheet = book.worksheets(1)
			if wParams = "" or isnull(wParams) then drop("�� ������ ��������� �� ��������� ��� ������� ���� ��������")
			wParams = split(wParams, ";;")
			if ubound(wParams) <> 1 then drop("������������� ���������� ���������� ��� ��������. �������� " & ubound(wParams) & ", ��������� 2 ���������")

			sheet.cells(14, 1).value = "      ��������,  ����������� �������� �108 �� 28.08.2012�. ��������� ������� � �������� �������-������������ ���������, ��������������� � " & month1(Month(wDate) - 1) & " " & Year(wDate) & " �. ���� ������������  ��������� ���������:"

			rs.open "SELECT SKLAD.NCard, SKLAD.Name, REMONT.Units, SKLAD.Price, DEVICE.inventory, SKLAD.uchet FROM REMONT LEFT OUTER JOIN SKLAD ON REMONT.ID_U = SKLAD.NCard LEFT OUTER JOIN DEVICE ON REMONT.ID_D = DEVICE.number_device WHERE (REMONT.W_ID = " & id & ")", CONN
			if rs.eof then

			else
				i = 0
				do while not rs.eof
					for j = 0 to 5
						f(i, j) = trim(rs(j))
					next
					rs.movenext
					if instr(f(i, 4), "***") > 0 or instr(f(i, 4), "xxx") > 0 then f(i, 4) = "���������������� �����" else f(i, 4) = "���������� �: ���. � " & f(i, 4)
					sheet.range("A21:H21").insert -4121, 0
					i = 1 + i
				loop
				'������������ �� ���� ����
				sheet.range("A21:H" & (21 + i)).HorizontalAlignment = -4108 '�����
				sheet.range("A21:H" & (21 + i)).VerticalAlignment = -4108 '�����
				sheet.range("A21:H" & (21 + i)).WrapText = True
				sheet.range("A21:H" & (21 + i)).Font.Size = 8
				sheet.range("A21:H" & (21 + i)).Rows.AutoFit
				sheet.range("A" & (21 + i) & ":H" & (21 + i)).WrapText = False
				'�� ������������
				sheet.range("C21:C" & (21 + i)).HorizontalAlignment = -4131 '���� ��� C
				sheet.range("H21:H" & (21 + i)).HorizontalAlignment = -4131 '���� ��� H
				sheet.range("F21:G" & (21 + i)).HorizontalAlignment = -4152 '����� ��� ���
				for j = 0 to i - 1
					sum = sum + CCur(f(j, 2)) * CCur(f(j, 3))
				next
				sheet.cells(21 + i, 7).Value = FormatNumber(sum, , -1)
				sheet.range("G21:G" & (22 + i)).HorizontalAlignment = -4152 '����� ��� ���
			end if
			rs.close
			for j = 0 to i - 1
				sheet.cells(21 + j, 1).Value = j + 1
				sheet.cells(21 + j, 2).Value = f(j, 0)
				sheet.cells(21 + j, 3).Value = f(j, 1) & "; ���� " & f(j, 5)
				sheet.cells(21 + j, 4).Value = "��."
				sheet.cells(21 + j, 5).Value = f(j, 2)
				sheet.cells(21 + j, 6).Value = FormatNumber(CCur(f(j, 3)), , -1)
				sheet.cells(21 + j, 7).Value = FormatNumber(CCur(f(j, 2)) * CCur(f(j, 3)), , -1)
				sheet.cells(21 + j, 8).Value = f(j, 4)
			next

			sheet.cells(40 + i, 1).Value = "��� ��������� " & day(wDate) & " " & month2(Month(wDate) - 1) & " " & year(wDate) & " �."
			sheet.cells(42 + i, 4).Value = wParams(0)
			sheet.cells(42 + i, 7).Value = wParams(1)

		' ������ ��������� ��������
		case "mat"
			set sheet = book.worksheets(8)

			' ������ ������������� �����
			sheet.cells(26, 17).value = id
			if day(wDate) > 9 then
				sheet.cells(27, 17).value = chr(34) & day(wDate) & chr(34) & " " & month2(month(wDate) - 1) & " " & year(wDate) & " �."
				if month(wDate) > 9 then
					sheet.cells(28, 17).value = day(wDate) & "." & month(wDate) & "." & year(wDate) & " �."
				else
					sheet.cells(28, 17).value = day(wDate) & ".0" & month(wDate) & "." & year(wDate) & " �."
				end if
			else
				sheet.cells(27, 17).value = chr(34) & "0" & day(wDate) & chr(34) & " " & month2(month(wDate) - 1) & " " & year(wDate) & " �."
				if month(wDate) > 9 then
					sheet.cells(28, 17).value = "0" & day(wDate) & "." & month(wDate) & "." & year(wDate) & " �."
				else
					sheet.cells(28, 17).value = "0" & day(wDate) & ".0" & month(wDate) & "." & year(wDate) & " �."
				end if
			end if
			sheet.cells(29, 17).value = month1(month(wDate) - 1) & " " & year(wDate) & " �."
			sheet.cells(28, 40).value = year(wDate) & " �."
			rs.open "SELECT TOP (1) ID_D, COUNT(INum) AS N FROM REMONT WHERE (W_ID = " & id & ") GROUP BY ID_D", CONN
			if rs.eof then
				drop("�� ������� �������� ��������, �� ������� ��������� �������")
			else
				dim number_device : number_device = trim(rs(0)) 'id ��
				dim ns : ns = rs(1) '���-�� ��������
			end if
			rs.close
			rs.open "SELECT TOP (1) inventory, description, description1C, number_serial, MOL FROM DEVICE WHERE (number_device = '" & number_device & "')", CONN
			if rs.eof then
				drop("�� ������� �������� ��������� ��������")
			else
				dim inventory:     inventory     = trim(rs(0))
				dim description:   description   = trim(rs(1))
				dim description1c: description1c = trim(rs(2))
				dim valueText:     valueText     = ""
				select case inventory
					case "075755" valueText = "������������ ��� ���: "
					case "075750" valueText = "������������ ������������� ����: "
					case "075155" valueText = "������������ ����� ����: "
					case else     valueText = ""
				end select
				if description1c = "" or isNull(description1c) then
					sheet.cells(30, 17).value = valueText & description
				else
					sheet.cells(30, 17).value = valueText & description1c
				end if
				sheet.cells(32, 17).value = inventory
				sheet.cells(33, 17).value = trim(rs(3))
				sheet.cells(34, 17).value = ns
				sheet.cells(53, 40).value = trim(rs(4))
			end if
			rs.close

			' ������� ������������ ������������ �������� - ������ �� ������� - sql
			rs.open "SELECT REMONT.Units, SKLAD.Name, SKLAD.Price, SKLAD.NCard FROM REMONT LEFT OUTER JOIN SKLAD ON REMONT.ID_U = SKLAD.NCard WHERE (REMONT.W_ID = " & id & ")", conn
			if rs.eof then
				drop("�� ������� �� ����� ������� �� ������ �� ��������� � �������� ��������� ��������")
			else
				i = 0
				do while not rs.eof
					sheet.cells(123 + i, 26).value = rs(0) 'units
					sheet.cells(123 + i, 29).value = "��"
					sheet.cells(123 + i, 33).value = rs(1) 'name
					sheet.cells(123 + i, 52).value = "������� ������"
					sheet.cells(123 + i, 58).value = rs(2) 'price
					sheet.cells(123 + i, 64).value = rs(3) 'ncard
					rs.movenext
					i = 1 + i
				loop
			end if
			rs.close

			sheet.cells(11, 17).value = drag(0)
			sheet.cells(12, 17).value = drag(1)
			sheet.cells(13, 17).value = drag(2)

			wParams = split(wParams, ";;")

			if ubound(wParams) > 4 then
				sheet.cells(51, 28).value = wParams(0) & " " & wParams(1)
				sheet.cells(53, 28).value = wParams(1) & " " & wParams(0)
				sheet.cells(53, 17).value = wParams(2)
				sheet.cells(35, 17).value = wParams(3)
				sheet.cells(34, 17).value = wParams(4)
				sheet.cells(28, 49).value = wParams(5)
			end if

			rs.open "SELECT TOP (1) PassportGold, PassportSilver, PassportPlatinum, PassportMPG FROM DEVICE WHERE (number_device = '" & number_device & "')", conn
			for i = 0 to 3
				ns = rs(i)
				if isNumeric(ns) then
					sheet.cells(64 + i, 60).value = replace(ns, ",", ".")
				else
					sheet.cells(64 + i, 60).value = "0.000000"
				end if
			next
			rs.close

	end select

	dim excelName : excelName = wName & " " & date
	book.saveas "C:\Inetpub\wwwroot\Devin\Content\Excels\" & excelName & ".xls"
	response.write "<a href='/devin/content/excels/" & excelName & ".xls'>" & excelName & "</a>"

	book.close false
	set sheet = nothing
	set book = nothing
	excel.quit
	set excel = nothing

	conn.execute "UPDATE writeoff SET W_Last_Excel = '" & excelName & ".xls', W_Last_Date = GETDATE() WHERE (W_ID = '" & id & "')"
	response.write log("off" & id, "��������� ������ �������� " & wName & " [off" & id & "]")

	set rs = nothing
	set conn = nothing
%>