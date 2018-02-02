<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createobject("ADODB.Connection")
	dim rs:   set rs   = server.createobject("ADODB.Recordset")
	dim id:   id   = request.form("id")
	dim user: user = replace(request.servervariables("REMOTE_USER"), "VST\", "")
	dim sql:  sql  = "SELECT " _
		& "REMONT.INum, REMONT.Units, REMONT.ID_U " _
		& "FROM REMONT " _
		& "INNER JOIN [writeoff] ON REMONT.W_ID = writeoff.W_ID " _
		& "WHERE (writeoff.W_ID = '" & id & "') AND (REMONT.IfSpis = 0)"
	
	conn.open everest
	rs.open   sql, conn
	if not rs.eof then
		do while not rs.eof
			conn.execute "" _
			& "BEGIN TRANSACTION;" _
			& "DECLARE @Units int, @Inum varchar(20), @Ncard varchar(50), @User varchar(20), @Date datetime;" _
			& "SET @INum = '" & rs(0) & "';" _
			& "SET @Units = " & rs(1) & ";" _
			& "SET @Ncard = '" & rs(2) & "';" _
			& "SET @User = '" & user & "';" _
			& "SET @Date = GETDATE();" _
			& "UPDATE SKLAD SET Nbreak = Nbreak + @Units, Nuse = Nuse - @Units WHERE (NCard = @Ncard);" _
			& "UPDATE REMONT SET IfSpis = 1 WHERE (INum = CAST(@Inum AS int));" _
			& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (@Date, @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при переводе ремонта [repair' + @Inum + '] в списанные: ' + CAST(@Units AS varchar(10)) + ' шт. деталей перемещены из используемых в списанные');" _
			& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (@Date, 'repair' + @INum, @User, 'Администратор DEVIN', 'Ремонт [repair' + @Inum + '] помечен как списанный');" _
			& "COMMIT;"
				
			rs.movenext
		loop
	end if
	rs.close
	
	conn.close
	set rs   = nothing
	set conn = nothing
%>