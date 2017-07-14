<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim D: D = request.form("devices")
	if D = "" or isnull(D) then 
		response.write "<div class='error'>Не получен список выбранных элементов</div>"
		response.end
	else
		dim key: key = request.form("key")
		D = split(D, ";;")
		dim id
		dim conn: set conn = server.createobject("ADODB.Connection")
		conn.open everest
		for each id in D
			if id <> "" then
				if key = "0" then
					conn.execute "UPDATE DEVICE SET number_comp = NULL, G_ID = 0 WHERE (number_device = '" & id & "')"
					response.write log(id, "Устройство [" & id & "] размещено отдельно")
				elseif instr(key, "cmp") > 0 then
					conn.execute "UPDATE DEVICE SET number_comp = '" & replace(key, "cmp", "") & "', G_ID = 0 WHERE (number_device = '" & id & "')"
					response.write log(id, "Устройство [" & id & "] перемещено в компьютер [" & replace(key, "cmp", "") & "]")
				else 
					conn.execute "UPDATE DEVICE SET number_comp = NULL, G_ID = " & replace(key, "g", "") & "WHERE (number_device = '" & id & "')"
					response.write log(id, "Устройство [" & id & "] перемещено в группу [group" & replace(key, "g", "") & "]")
				end if
			end if
		next
		conn.close
		set conn = nothing
	end if
%>