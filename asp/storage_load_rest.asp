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
	<title>DEVIN | 1�</title>
</HEAD>

<BODY>

<%
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
%>

<%
	dim excelApp : set excelApp = server.createobject("Excel.Application")
	excelApp.application.enableEvents = false
	excelApp.application.displayAlerts = false
	dim excelBook : set excelBook = excelApp.workbooks.open("D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\" & excelName)
	dim excelCells : set excelCells = excelBook.activesheet.cells


	' ������ �������� �� �������
	dim balances(1000, 10), storage(1000, 10), escape, cursor, position, index, excel_cells, score, find

	' �������� ������ �� Excel
	excel_cells = array(11, 12, 13, 16, 29, 33) '�����������, ���. �����, ���� 1��, ���� �������, ���-�� �������, ���� �����
	escape = 0
	position = 9
	index = 0
	do while not escape > 5
		cursor = excelCells(position, 33)
		if cursor = "" or isnull(cursor) then
			escape = escape + 1 '���� ������ ������, �������� �� �������, ���� 5 ������ - �����
		else
			escape = 0 '�������� ������� ������ ������, �.�. ��� �� ����� ���������
			'��������, ���� �� �������
			if excelCells(position, excel_cells(4)) = "" or excelCells(position, excel_cells(4)) = " " or isnull(excelCells(position, excel_cells(4))) then
				index = index
			else
				score = excelCells(position, 33)
				for i = 0 to 5
					balances(index, i) = trim(excelCells(position, excel_cells(i))) '���������� ������� �������� �� Excel
				next
				index = index + 1
			end if
		end if
		position = position + 1
	loop
	index = index - 1

	' �������� ������ �� ��������� ����
	set conn = server.createobject("ADODB.Connection")
	set rs = server.createobject("ADODB.Recordset")
	conn.open "Driver={SQL Server}; Server=log1\SQL2005; Database=EVEREST; Uid=user_everesr; Pwd=EveresT10;"

	sql = "SELECT INum, Name, NCard, Price, Nadd, Nis, Nuse, Nbreak, Date FROM SKLAD WHERE (uchet = '" & score & "')"
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

	'������ ������ � �������

	response.write "���� �����: " & score & "<br />"
	response.write "<table>"
	response.write "<tr><th>���. �<th>������������<th>�� ������ ������<th>� �������<th>������� �� 1�</tr>"
	for i = 0 to position
		find = false
		for j = 0 to index
			if storage(i, 2) = balances(j, 1) and not find then '������� ������������
				if cint(storage(i, 5)) + cint(storage(i, 6)) = cint(balances(j, 4)) then response.write "<tr class='compare_true'>" else response.write "<tr>"
				response.write "<td>" & storage(i, 2) & "<td>" & storage(i, 1) & "<td>" & storage(i, 5) & "<td>" & storage(i, 6) & "<td>" & balances(j, 4) & "</tr>"
				find = true
			end if
		next
		if not find then '������� ����� only
			if cint(storage(i, 5)) + cint(storage(i, 6)) = 0 then response.write "<tr class='compare_true'>" else response.write "<tr>"
			response.write "<td>" & storage(i, 2) & "<td>" & storage(i, 1) & "<td>" & storage(i, 5) & "<td>" & storage(i, 6) & "<td>�� ������</tr>"
		end if
	next
	for j = 0 to index
		find = false
		for i = 0 to position
			if storage(i, 2) = balances(j, 1) and not find then find = true
		next
		'������� 1� only
		if not find then response.write "<tr><td>" & balances(j, 1) & "<td>" & balances(j, 0) & "<td>�� ������<td>�� ������<td>" & balances(j, 4) & "</tr>"
	next
	response.write "</table>"

	excelBook.close
	set excelCells = nothing
	set excelBook = nothing
	set excelApp = nothing
%>

</BODY>

</HTML>