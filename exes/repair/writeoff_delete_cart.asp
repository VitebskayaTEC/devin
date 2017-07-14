<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createobject("ADODB.Connection")
	dim rs:   set rs = server.createobject("ADODB.Recordset")
	dim id:   id = replace(request.querystring("id"), "off", "")
	
	conn.open everest
	rs.open "SELECT W_Name FROM writeoff WHERE (W_ID = '" & id & "')", conn
	dim w_name: w_name = rs(0)
	rs.close
			
	conn.execute "DELETE FROM writeoff WHERE (W_ID = '" & id & "') "
	conn.execute "UPDATE REMONT SET W_ID = 0 WHERE (W_ID = '" & id & "')"

	response.write log("off" & id, "Удалено списание " & w_name & " [off" & id & "]")

	conn.close
	set rs = nothing
	set conn = nothing
%>