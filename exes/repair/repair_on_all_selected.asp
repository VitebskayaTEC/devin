<!-- #include virtual ="/devin/core/core.inc" -->
<%
	sub drop(str)
		response.write str
		response.end
	end sub

	dim R 	: R = request.form("repairs")

	if isnull(R) or R = "" then drop("�� ������� ������ ��������� ���������") else R = split(R, ";;")

	dim sql, key
	sql = ""
	for each key in R
		if key <> "" then
			if sql <> "" then sql = sql & " OR "
			sql = sql & "INum = " & key
		end if
	next

	sql = "SELECT INum, Units, ID_U FROM REMONT WHERE (" & sql & ")  AND (IfSpis = 1)"

	dim conn 	: set conn = server.createobject("ADODB.Connection")
	dim rs 		: set rs = server.createobject("ADODB.Recordset")


	conn.open everest
	rs.open sql, conn
	if not rs.eof then
		dim user : user = replace(request.servervariables("REMOTE_USER"), "VST\", "")
		do while not rs.eof
			conn.execute "" _
			& "BEGIN TRANSACTION;" _
			& "" _
			& "DECLARE @Units int, @Inum varchar(20), @Ncard varchar(50), @User varchar(20), @Date datetime;" _
			& "SET @INum = '" & rs(0) & "';" _
			& "SET @Units = " & rs(1) & ";" _
			& "SET @Ncard = '" & rs(2) & "';" _
			& "SET @User = '" & user & "';" _
			& "SET @Date = GETDATE();" _
			& "" _
			& "UPDATE SKLAD SET Nbreak = Nbreak - @Units, Nuse = Nuse + @Units WHERE (NCard = @Ncard);" _
			& "UPDATE REMONT SET IfSpis = 0 WHERE (INum = CAST(@Inum AS int));" _
			& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (@Date, @Ncard, @User, '������������� DEVIN', '��������� ������� [' + @Ncard + '] ��� �������� ������� [repair' + @Inum + '] � �������� ���������: ' + CAST(@Units AS varchar(10)) + ' ��. ������� ���������� �� ��������� � ������������');" _
			& "INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS) VALUES (@Date, 'repair' + @INum, @User, '������������� DEVIN', '������ [repair' + @Inum + '] ������� ��� ��������');" _
			& "" _
			& "COMMIT;"

			rs.movenext
		loop
	end if
	rs.close


	conn.close
	set rs 		= nothing
	set conn 	= nothing
%>