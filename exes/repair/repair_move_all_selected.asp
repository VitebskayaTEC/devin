<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim R : R = request.form("repairs")
	if R = "" or isnull(R) then
		response.write "<div class='error'>Не получен список выбранных элементов</div>"
		response.end
	else
		dim key	: key = request.form("key")
		R = split(R, ";;")
		dim inum
		dim conn : set conn = server.createobject("ADODB.Connection")
		conn.open everest
		for each inum in R
			if inum <> "" then
				if key = "0" then
					conn.execute "UPDATE REMONT SET W_ID = 0, G_ID = 0 WHERE Inum = " & inum
					response.write log("repair" & inum, "Ремонт [repair" & inum & "] размещен отдельно")
				elseif instr(key, "w") > 0 then
					conn.execute "UPDATE REMONT SET W_ID = " & replace(key, "w", "") & ", G_ID = 0 WHERE Inum = " & inum
					response.write log("repair" & inum, "Ремонт [repair" & inum & "] перемещен в списание [off" & key & "]")
				else
					conn.execute "UPDATE REMONT SET W_ID = 0, G_ID = " & replace(key, "g", "") & " WHERE Inum = " & inum
					response.write log("repair" & inum, "Ремонт [repair" & inum & "] перемещен в группу [group" & key & "]")
				end if
			end if
		next
		conn.close
		set conn = nothing
	end if
%>