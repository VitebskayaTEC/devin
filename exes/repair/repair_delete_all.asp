<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createobject("ADODB.Connection")
	dim rs:   set rs   = server.createobject("ADODB.Recordset")
	dim id:   id   = request.form("id")
	dim user: user = replace(request.servervariables("REMOTE_USER"), "VST\", "")
	dim sql:  sql  = "SELECT " _
		& "REMONT.INum " _
		& "FROM REMONT " _
		& "INNER JOIN [writeoff] ON REMONT.W_ID = writeoff.W_ID " _
		& "WHERE (writeoff.W_ID = '" & id & "')"

	conn.open everest
	rs.open   sql, conn
	if not rs.eof then
		do while not rs.eof
			conn.execute "" _
	& "BEGIN TRANSACTION;" _
	& "DECLARE @Inum int; SET @Inum = " & rs(0) & ";" _
	& "DECLARE @X varchar(10);" _
	& "DECLARE @Units int, @IfSpis int, @Virtual int, @Ncard varchar(50);" _
	& "DECLARE @User varchar(20); SET @User = '" & user & "';" _
	& "SELECT " _
	& "	@Units = REMONT.Units, " _
	& "	@IfSpis = REMONT.IfSpis, " _
	& "	@Virtual = REMONT.Virtual, " _
	& "	@Ncard = SKLAD.NCard " _
	& "FROM REMONT " _
	& "	LEFT OUTER JOIN SKLAD ON SKLAD.NCard = REMONT.ID_U " _
	& "WHERE (REMONT.INum = @Inum)" _
	& "DELETE FROM REMONT WHERE (INum = @Inum) " _
	& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), 'repair' + CAST(@Inum AS varchar(10)), @User, 'Администратор DEVIN', 'Удален ремонт [repair' + CAST(@Inum AS varchar(10)) + ']');" _
	& "	IF @IfSpis = 1" _
	& "		IF @Virtual = 1	BEGIN" _
	& "			UPDATE SKLAD SET Nbreak = Nbreak - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении виртуального ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей изъяты из списанных');" _
	& "		END	ELSE BEGIN" _
	& "			UPDATE SKLAD SET Nis = Nis + @Units, Nbreak = Nbreak - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей возвращены на склад из списанных');" _
	& "		END" _
	& "	ELSE" _
	& "		IF @Virtual = 1 BEGIN" _
	& "			UPDATE SKLAD SET Nuse = Nuse - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении виртуального ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей изъяты из используемых');" _
	& "		END ELSE BEGIN" _
	& "			UPDATE SKLAD SET Nis = Nis + @Units, Nuse = Nuse - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей возвращены на склад из используемых');" _
	& "		END  " _
	& "COMMIT;"

			rs.movenext
		loop
	end if
	rs.close
	conn.close
	set rs  = nothing
	set conn = nothing
%>