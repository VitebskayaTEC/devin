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

Dim i, text, CONN, temp
text = ""

'Забираем данные из формы'
for i = 1 to 13
	if i > 1 then text = text & ";;"
	if i = 4 then 
		text = text & "0" 
	else 
		if i = 7 then 
			temp = split(DecodeUTF8(Request.Form("T" & i))," в ")
			on error resume next
			text = text & temp(0)
		else
			text = text & DecodeUTF8(Request.Form("T" & i))
		end if
	end if
next

'Чтение запроса и выгрузка в массив'
Set CONN = Server.CreateObject("ADODB.Connection")
CONN.Open "Driver={SQL Server}; Server=log1\SQL2005; Database=EVEREST; Uid=user_everesr; Pwd=EveresT10;" 
CONN.Execute("UPDATE DATABANK SET Data = '" & text & "' WHERE Provider = 'CartridgeAnalyze'")
CONN.Close
Set CONN = Nothing

Response.Write("<span style='color: #2FDA37'>Save!</span>")
%>