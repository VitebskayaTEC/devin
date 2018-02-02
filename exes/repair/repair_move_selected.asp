<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim id : id = request.querystring("id")
	dim conn : set conn = server.createobject("ADODB.Connection")
	dim sql : sql = ""

	dim key : key = request.form("key")
	dim inums : inums = request.form("inums")
	inums = split(inums, ".")

	dim inum
	for each inum in inums
		if sql <> "" then sql = sql & " OR "
		sql = sql & "(INum = " & inum & ")"
		response.write log("repair" & inum, "Ремонт [" & inum & "] перемещен в списание [" & key & "]")
	next
	
	sql = "UPDATE REMONT SET G_ID = " & key & " WHERE " & sql
	conn.open everest	
	conn.execute sql	
	conn.close
	set conn = nothing
%>
<script>location.reload();</script>