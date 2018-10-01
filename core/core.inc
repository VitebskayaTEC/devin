<%
	option explicit
	
	response.contenttype = "text/html; charset=windows-1251"

	dim everest : everest = "Driver={SQL Server};Server=log1\SQL2005; Database=EVEREST; Uid=user_everesr; Pwd=EveresT10;"
	
	Function strGetCurrTime(dtTimer)
		Dim intHours : intHours = dtTimer \ (3600)
		Dim intMinutes : intMinutes = (dtTimer - (intHours * (3600))) \ 60
		Dim intSeconds : intSeconds = Fix(dtTimer - (intHours * (3600)) - intMinutes * 60)
		Dim intMilliSeconds : intMilliSeconds = Fix((dtTimer - Fix(dtTimer)) * 1000)
		strGetCurrTime = CStr(TimeSerial(intHours, intMinutes, intSeconds)) & "." & Right("000" & CStr(intMilliSeconds), 3)
	End Function

	Function log(id, Message)
		On Error Resume Next
		Dim CN
		Set CN = Server.CreateObject("ADODB.Connection")
			CN.Open everest
		CN.Execute "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), '" & id & "', '" & lcase(mid(Request.ServerVariables("REMOTE_USER"), 5)) & "', 'Администратор DEVIN', '" & Message & "')"
		CN.Close
		Set CN = Nothing
	End Function
	
	function S_D(ss)
		dim b, i, s, s1, j, k, l
		dim classes : classes = array("CMP", "PRN", "DIS", "MOD", "SWT", "SCN", "UPS", "TO")
		for l = 0 to 7
			b = true
			s = ss
			ss = ""
			do while b
				i = InStr(s, "-" & classes(l) & "-")
				if i <> 0 then
					s1 = left(s, i + 6)
					j = i - 1
					do while (ascw(mid(s, j, 1)) <= 57) and (ascw(mid(s, j, 1)) >= 48) and (j > 1)
						j = j - 1
					loop
					s1 = mid(s1, j + 1, i + 6)
					ss = ss & left(s, j) & "<a href='/devin/devices/##" & s1 & "'>" & s1 & "</a>"
					s = mid(s, i + 7, len(s))
				else
					b = false
				end if
			loop
			if ss="" then ss = s else ss = ss & s
		next
		S_D = s
	end function

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

	Function menu(points)
		dim temp : temp = "<ul class='main-menu'><li><a href='/' style='width: 36px'><div class='icon ic-exit-to-app'></div></a><li><a href='/devin/devices/'>DEVIN</a><li><a href='/devin/storage/'>Склад</a><li><a href='/devin/repair/'>Ремонты</a><li><a href='/devin/aida/'>AIDA</a><li><a href='/devin/catalog/'>Каталог</a>"
		if instr(lcase(Request.ServerVariables("URL")), "analyze") > 0 then 
			response.write temp & points & "</ul>"
		else
			dim current : current = replace(lcase(Request.ServerVariables("URL")), "default.asp", "") & "'"
			response.write Replace(temp, current, current & "' class='point-now'") & points & "</ul>"
		end if
	End Function

	Function DateToInput(source)
		dim temp 
		temp = year(source) & "-"
		if month(source) < 10 then temp = temp & "0"
		temp = temp & month(source) & "-"
		if day(source) < 10 then temp = temp & "0"
		temp = temp &day(source)
		DateToInput = temp
	End Function

	Function DateToSql(source)
		dim temp 
		
		if month(source) < 10 then temp = temp & "0"
		temp = temp & month(source) & "."
		if day(source) < 10 then temp = temp & "0"
		temp = temp & day(source) & "."  & year(source) 
		DateToSql = temp
	End Function
%>