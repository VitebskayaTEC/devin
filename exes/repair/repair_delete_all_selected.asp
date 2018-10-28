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
		dim user : user = replace(request.servervariables("REMOTE_USER"), "VST\", "")
		conn.open everest
		for each inum in R
			if inum <> "" then
				conn.execute "" _
	& "BEGIN TRANSACTION;" _
	& "" _
	& "DECLARE @Inum int; SET @Inum = " & inum & ";" _
	& "DECLARE @X varchar(10);" _
	& "" _
	& "DECLARE @Units int, @IfSpis int, @Virtual int, @Ncard varchar(50);" _
	& "DECLARE @User varchar(20); SET @User = '" & user & "';" _
	& "" _
	& "SELECT " _
	& "	@Units = REMONT.Units, " _
	& "	@IfSpis = REMONT.IfSpis, " _
	& "	@Virtual = REMONT.Virtual, " _
	& "	@Ncard = Storages.Ncard " _
	& "FROM REMONT " _
	& "	LEFT OUTER JOIN Storages ON Storages.Ncard = REMONT.ID_U " _
	& "WHERE (REMONT.INum = @Inum)" _
	& "" _
	& "DELETE FROM REMONT WHERE (INum = @Inum) " _
	& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), 'repair' + CAST(@Inum AS varchar(10)), @User, 'Администратор DEVIN', 'Удален ремонт [repair' + CAST(@Inum AS varchar(10)) + ']');" _
	& "" _
	& "	IF @IfSpis = 1" _
	& "		IF @Virtual = 1	BEGIN" _
	& "			UPDATE Storages SET Noff = Noff - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении виртуального ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей изъяты из списанных');" _
	& "		END	ELSE BEGIN" _
	& "			UPDATE Storages SET Nstorage = Nstorage + @Units, Noff = Noff - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей возвращены на склад из списанных');" _
	& "		END" _
	& "	ELSE" _
	& "		IF @Virtual = 1 BEGIN" _
	& "			UPDATE Storages SET Nrepairs = Nrepairs - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении виртуального ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей изъяты из используемых');" _
	& "		END ELSE BEGIN" _
	& "			UPDATE Storages SET Nstorage = Nstorage + @Units, Nrepairs = Nrepairs - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, 'Администратор DEVIN', 'Обновлена позиция [' + @Ncard + '] при удалении ремонта [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' шт. деталей возвращены на склад из используемых');" _
	& "		END  " _
	& "" _
	& "COMMIT;"
			end if
		next
		conn.close
		set conn = nothing
	end if
%>