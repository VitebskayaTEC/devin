<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim user : user = replace(Request.ServerVariables("REMOTE_USER"), "VST\", "")
	dim id : id = request.querystring("id")

	dim data(2)
	dim n : n = 0
	dim key, insert, update

	dim conn : set CONN = Server.CreateObject("ADODB.Connection")
	conn.open everest

	dim writeoff : writeoff = "0"
	if request.querystring("won") = "on" then
		dim text : text = request.querystring("writeoff")
		if text = "" or isnull(text) then text = "������� �� " & date
		conn.execute "INSERT INTO Writeoffs (Name, Type, Date, FolderId) VALUES ('" & text & "', 'mat', GetDate(), 0)"
		dim rs : set rs = server.createobject("ADODB.Recordset")
		rs.open "SELECT Max(Id) FROM Writeoffs", conn
		writeoff = rs(0)
		rs.close
		set rs = nothing
		response.write log("off" & writeoff, "�������������� �������� �������� ��� ���������� ������� �� ���������� [" & id & "]")
	end if

	for each key in request.form
		if instr(key, "count") = 0 and instr(key, "virtual") = 0 and request.form(key) = "on" then
			' ����������� ����� ������
			data(0) = key
			' ��������� ���-�� ������� ��� �������
			data(1) = request.form(key & "count")
			' ����������� ������ ������ ��� ���
			data(2) = request.form(key & "virtual")
			if data(2) = "on" then
				data(2) = "1"
				update = update & "UPDATE Storages SET Nrepairs = Nrepairs + " & data(1) & " WHERE (Ncard = '" & data(0) & "') " & chr(13)
				response.write log(id, "������: ������������ ������� � ����������� � [" & data(0) & "] � ���������� [" & data(1) & "] ��. (�����������)")
			else
				data(2) = "0"
				update = update & "UPDATE Storages SET Nrepairs = Nrepairs + " & data(1) & ", Nstorage = Nstorage - " & data(1) & " WHERE (Ncard = '" & data(0) & "') " & chr(13)
				response.write log(id, "������: ������������ ������� � ����������� � [" & data(0) & "] � ���������� [" & data(1) & "] ��.")
			end if
			insert = insert & "INSERT INTO REMONT (ID_D, ID_U, Units, Date, Author, IfSpis, Virtual, W_ID, G_ID) VALUES ('" & id & "', '" & data(0) & "', '" & data(1) & "', GETDATE(), '" & user & "', '0', '" & data(2) & "', '" & writeoff & "', 0) " & chr(13)

			n = n + 1
		end if
	next

	conn.execute insert
	conn.execute update

	response.write "������� ��������: " & n
	if writeoff <> "0" then
		response.write " <a href='/devin/repair/##off" & writeoff & "'>������� � ��������� ������</a>"
	end if

	conn.close
	set conn = nothing
%>