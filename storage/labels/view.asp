<!-- #include virtual ="/devin/core/core.inc" -->
<%
	' Забираем данные из складской базы	
	dim conn: set conn = server.createobject("ADODB.Connection")
	dim rs:   set rs   = server.createobject("ADODB.Recordset")
	dim sql:  sql = "SELECT SKLAD.NCard, SKLAD.Name, CARTRIDGE.Caption, SKLAD.Date, SKLAD.Nadd, SKLAD.Nis, SKLAD.uchet FROM SKLAD LEFT OUTER JOIN CARTRIDGE ON SKLAD.ID_cart = CARTRIDGE.N WHERE class_name = 'PRN' AND delit = 1 AND Nis > 0 ORDER BY Date DESC"
	dim i
	dim data(6)
	dim led

	conn.open everest
	rs.open sql, conn

	if not rs.eof then
		
		response.write "<div class='unit'><table class='caption'><tr><th>Картриджи на складе</tr></table>" _
			& "<table class='items'><thead><tr>" _
			& "<th width='30px'><input type='checkbox' class='selecter-all' />" _
			& "<th width='20px' data-type='type'>" _
			& "<th width='56px' data-type='number'>Инв. №" _
			& "<th data-type='string'>Наименование" _
			& "<th width='200px' data-type='number'>Типовой картридж" _
			& "<th width='80px' data-type='number'>Дата покупки" _
			& "<th width='60px' data-type='number'>Приход" _
			& "<th width='60px' data-type='number'>На складе" _
			& "<th width='70px' data-type='string'>Счет учета" _
			& "</tr></thead><tbody>"

		do while not rs.eof

			for i = 0 to 6
				data(i) = rs(i)
			next

			if data(5) = 0 then led = "off" else led = ""

			response.write "<tr id='" & data(0) & "'>" _
				& "<th><input type='checkbox' class='selecter' />" _
				& "<td><div class='led " & led & "'></div>" _
				& "<td>" & data(0) _
				& "<td>" & data(1) _
				& "<td>" & data(2) _
				& "<td>" & data(3) _
				& "<td>" & data(4) _
				& "<td>" & data(5) _
				& "<td>" & data(6) _
				& "</tr>"

			rs.movenext
			
		loop

		response.write "</tbody></table></div>"
		
	end if

	rs.close
	conn.close
	set rs   = nothing
	set conn = nothing
%>