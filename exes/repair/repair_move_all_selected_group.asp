<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim R : R = request.form("repairs")
	if R = "" or isnull(R) then 
		response.write "<div class='error'>�� ������� ������ ��������� ���������</div>"
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
					conn.execute "UPDATE REMONT SET W_ID = " & key & ", G_ID = 0 WHERE Inum = " & inum
					response.write log("repair" & inum, "������ [repair" & inum & "] �������� ��������")
				else
					conn.execute "UPDATE REMONT SET G_ID = " & key & " WHERE Inum = " & inum
					response.write log("repair" & inum, "������ [repair" & inum & "] ��������� � ������ [off" & key & "]")
				end if
			end if
		next
		conn.close
		set conn = nothing
	end if
%>