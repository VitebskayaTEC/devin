<%option explicit%>
<%
Function DecodeUTF8(s)
	Dim i, c, n, b1, b2, b3
	i = 1
	Do While i <= len(s)
		c = asc(mid(s,i,1))
		If (c and &hC0) = &hC0 Then
			n = 1
			Do While i + n <= len(s)
				If (asc(mid(s,i+n,1)) and &hC0) <> &h80 Then
					Exit Do
				End If
				n = n + 1
			Loop
			If n = 2 and ((c and &hE0) = &hC0) Then
				b1 = asc(mid(s,i+1,1)) and &h3F
				b2 = c and &h1F
				c = b1 + b2 * &h40
			Elseif n = 3 and ((c and &hF0) = &hE0) Then
				b1 = asc(mid(s,i+2,1)) and &h3F
				b2 = asc(mid(s,i+1,1)) and &h3F
				b3 = c and &h0F
				c = b3 * &H1000 + b2 * &H40 + b1
			Else
				' Символ больше U+FFFF или неправильная последовательность
				c = &hFFFD
			End if
			s = left(s,i-1) + chrw( c) + mid(s,i+n)
		Elseif (c and &hC0) = &h80 then
			' Неожидаемый продолжающий байт
			s = left(s,i-1) + chrw(&hFFFD) + mid(s,i+1)
		End If
		i = i + 1
	Loop
	DecodeUTF8 = s
End Function

Dim oExcel, oBook, oSheet, oBookName, i, j, text(15), cartridge(1000,4), cartN

'Забираем данные из формы'
for i = 1 to 13
	text(i) = Request.Form("T" & i) 
next
i = 0
do while not Request.Form("exl-" & i & "-0") = ""
	for j = 0 to 4
		cartridge(i,j) = Request.Form("exl-" & i & "-" & j)
	next
	i = 1 + i
loop
cartN = i - 1

'Открываем и заполняем excel'
Set oExcel = CreateObject("Excel.Application")
oExcel.Application.EnableEvents= false
oExcel.Application.DisplayAlerts = false
Set oBook = oExcel.Workbooks.Open("D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\ЗакупкаКартриджей.xls")
Set oSheet = oBook.Worksheets(1)

for i = 1 to 8
	if i > 2 then oSheet.Cells(i + 1,2).Value = DecodeUTF8(text(i)) else oSheet.Cells(i,2).Value = DecodeUTF8(text(i))
next
for i = 9 to 11
	oSheet.Cells(i - 8,5).Value = DecodeUTF8(text(i))
next
for i = 0 to cartN
	'ОБРАБОТАТЬ МАССИВ text И ВСТАВИТЬ В ТАБЛИЦУ'
	for j = 0 to 4
		oSheet.Cells(i + 13,j + 2).Value = DecodeUTF8(cartridge(i,j))
		oSheet.Cells(i + 13,j + 2).BorderAround 1, -4138
	next
next
oSheet.Cells(cartN + 15,2).Value = DecodeUTF8(text(12))
oSheet.Cells(cartN + 15,6).Value = DecodeUTF8(text(13))

'Сохраняем excel'
dim a : a = "Cartridges " & Day(date) & "-" & Month(date) & "-" & Year(date) &".xls"
oBook.SaveAs "D:\data\DFS\Files\Inetpub\wwwroot\DEVIN\Excels\" & a
Response.Write "<a href='\DEVIN\Excels\" & a & "' download>" & a & "</a>"

oBook.Close False
Set oSheet = Nothing
Set oBook= Nothing
oExcel.Quit
Set oExcel = Nothing
%>