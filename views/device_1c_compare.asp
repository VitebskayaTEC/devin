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
	<title>DEVIN | ������ �� 1�</title>
</HEAD>

<BODY>

<%
    menu("<li><a onclick=''>�����������</a>" _
        & "<li><a onclick=''>������ � DEVIN</a>" _
        & "<li><a onclick=''>������ � 1�</a>")

    response.write "<div class='view'>"

    ' ��������� ���� ������
	' dim Uploader : set Uploader = new FileUploader
	dim File, excelName

	' Uploader.Upload()
	' If Uploader.Files.Count <> 0 Then
	' 	for each File In Uploader.Files.Items
	' 		File.SaveToDisk "D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\"
	' 		excelName = File.FileName
	' 	next
	' 	objStream1.close
	' 	set objStream1 = nothing
	' end if

	excelName = "�������� �������� ������� � ������ 2017 ����.xls"


	'dim CONN, RS, SQL, Ex(2000,10), Dev(2000,10), i, j, q, index(2), Spl, MOL, check, ExCols(5), typeEx, max
	dim typeArray: typeArray = array("CMP", "PRN", "DIS", "SWT", "SCN", "UPS", "RR")
	'Ex "���������","�������","�������","�����","������","UPS","������"
	'������ �� EXCEL

	dim excel: set excel = createobject("Excel.Application")
	excel.Application.EnableEvents = false
	excel.Application.DisplayAlerts = false
	dim workbook: set workbook = excel.workbooks.open("D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\" & excelName)
	dim cells: set cells = workbook.activeSheet.cells

	dim regex : set regex = new RegExp
	regex.ignoreCase = true
	regex.global = true

	dim current: current = cint(request.queryString("typeof"))
	select case current
		case 0 regex.pattern = "������|������|�������|�� |���� ����|����|��� ����|������|notebook"
		case 1 regex.pattern = "������� �����|�������|������� �����|���"
		case 2 regex.pattern = "�������|���������"
		case 3 regex.pattern = "����������|������������|������ ����|�������������|�����|������"
		case 4 regex.pattern = "������|������������� ���"
		case 5 regex.pattern = "apc|ups|powerman|������� �������������� �������"
		case 6 regex.pattern = "���������������|�������"
		case else current = -1
	end select

	dim i, j
	dim Edata(1000, 3)
	dim NEdata: NEdata = 0

	if current <> -1 then
		i = 9
		dim tooManyEmpty: tooManyEmpty = 0
		do while not tooManyEmpty = 5
			if cells(i, 12) = "" then
				tooManyEmpty = tooManyEmpty + 1
			elseif regex.test(cstr(cells(i, 1).value)) then
				Edata(NEdata, 0) = cells(i, 12)
				Edata(NEdata, 1) = cells(i, 1)
				Edata(NEdata, 2) = cells(i, 16)
				Edata(NEdata, 3) = cells(i, 33)
				NEdata = NEdata + 1
			end if
			i = i + 1
		loop
	else
		response.write "<div class='error'>�� ����������� ��� �������</div>"
	end if

	workbook.close
	set excel = nothing
	set workbook = nothing
	set cells = nothing



	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs: set rs = server.createObject("ADODB.Recordset")
	dim Sdata(1000, 3)
	dim NSdata: NSdata = 0

	conn.open everest
	rs.open "SELECT inventory, name, description, install_date FROM DEVICE WHERE class_device = '" & typeArray(current) & "'", conn
	if rs.eof then
		response.write "<div class='error'>��� ������� � SQL ����</div>"
	else
		Sdata(NSdata, 0) = trim(rs(0))
		Sdata(NSdata, 1) = trim(rs(1))
		Sdata(NSdata, 2) = trim(rs(2))
		Sdata(NSdata, 3) = trim(rs(3))
		NSdata = NSdata + 1
	end if
	rs.close
	conn.close
	set rs = nothing
	set conn = nothing

	if NSdata > 0 and NEdata > 0 then
		dim found: found = false
		for i = 0 to NEdata
			found = false
			for j = 0 to NSdata
				if trim(cstr(Edata(i, 0))) = trim(cstr(Sdata(j, 0))) then found = true
				'response.write "1(" & Edata(i, 0) & ") 2(" & Sdata(j, 0) & ")<br />"
			next
			if found then
				response.write "<div class='done'>" & Edata(i, 0) & "</div>"
			else
				response.write "<div>" & Edata(i, 0) & "</div>"
			end if
		next
	else
		response.write "<div class='error'>��������� ������</div>"
	end if
%>