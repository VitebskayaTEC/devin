<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest
	
	conn.execute "" _
	& "BEGIN TRANSACTION;" _
	& "" _
	& "DECLARE @Inum int; SET @Inum = " & request.querystring("id") & ";" _
	& "DECLARE @X varchar(10);" _
	& "" _
	& "DECLARE @Units int, @IfSpis int, @Virtual int, @Ncard varchar(50);" _
	& "DECLARE @User varchar(20); SET @User = '" & replace(request.servervariables("REMOTE_USER"), "VST\", "") & "';" _
	& "" _
	& "SELECT " _
	& "	@Units = REMONT.Units, " _
	& "	@IfSpis = REMONT.IfSpis, " _
	& "	@Virtual = REMONT.Virtual, " _
	& "	@Ncard = SKLAD.NCard " _
	& "FROM REMONT " _
	& "	LEFT OUTER JOIN SKLAD ON SKLAD.NCard = REMONT.ID_U " _
	& "WHERE (REMONT.INum = @Inum)" _
	& "" _
	& "DELETE FROM REMONT WHERE (INum = @Inum) " _
	& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), 'repair' + CAST(@Inum AS varchar(10)), @User, '������������� DEVIN', '������ ������ [repair' + CAST(@Inum AS varchar(10)) + ']');" _
	& "" _
	& "	IF @IfSpis = 1" _
	& "		IF @Virtual = 1	BEGIN" _
	& "			UPDATE SKLAD SET Nbreak = Nbreak - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, '������������� DEVIN', '��������� ������� [' + @Ncard + '] ��� �������� ������������ ������� [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' ��. ������� ������ �� ���������');" _
	& "		END	ELSE BEGIN" _
	& "			UPDATE SKLAD SET Nis = Nis + @Units, Nbreak = Nbreak - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, '������������� DEVIN', '��������� ������� [' + @Ncard + '] ��� �������� ������� [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' ��. ������� ���������� �� ����� �� ���������');" _
	& "		END" _
	& "	ELSE" _
	& "		IF @Virtual = 1 BEGIN" _
	& "			UPDATE SKLAD SET Nuse = Nuse - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, '������������� DEVIN', '��������� ������� [' + @Ncard + '] ��� �������� ������������ ������� [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' ��. ������� ������ �� ������������');" _
	& "		END ELSE BEGIN" _
	& "			UPDATE SKLAD SET Nis = Nis + @Units, Nuse = Nuse - @Units WHERE (NCard = @Ncard);" _
	& "			INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (GETDATE(), @Ncard, @User, '������������� DEVIN', '��������� ������� [' + @Ncard + '] ��� �������� ������� [repair' + CAST(@Inum AS varchar(10)) + ']: ' + CAST(@Units AS varchar(10)) + ' ��. ������� ���������� �� ����� �� ������������');" _
	& "		END  " _
	& "" _
	& "COMMIT;"

	conn.close
	set conn = nothing
%>