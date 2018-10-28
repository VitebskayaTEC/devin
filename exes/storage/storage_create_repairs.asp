<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim allrepairs : allrepairs = request.form("allrepairs")

	' Проверка, пришли ли данные из формы
	if isnull(allrepairs) or allrepairs = "" then
		response.write "<div class='error'>Данные по выбранным ремонтам не были получены</div>"
	else
		dim repairs : repairs = split(allrepairs, "<separate>")
		dim conn : set conn = server.createObject("ADODB.Connection")
		conn.open everest
		dim repair, repairArray, repairsAllCount
		repairsAllCount = 0

		' Обработка записей о ремонтах
		for each repair in repairs
			' отсекаем пустые сегменты
			if repair <> "" then
				repairsAllCount = repairsAllCount + 1
				repairArray = split(repair, ";")
				' отсекаем поврежденные записи с неполным либо избыточным кол-вом элементов
				if ubound(repairArray) = 3 then
					conn.execute "BEGIN TRANSACTION;" _
					& "DECLARE @ID_D varchar(20), @ID_U varchar(10), @Units int, @Virtual int, @Date datetime, @User varchar(15);" _
					& "SET @ID_U = '" & repairArray(0) & "';" _
					& "SET @ID_D = '" & repairArray(1) & "';" _
					& "SET @Units = " & repairArray(2) & ";" _
					& "SET @Virtual = " & repairArray(3) & ";" _
					& "SET @Date = GETDATE();" _
					& "SET @User = '" & replace(request.servervariables("REMOTE_USER"), "VST\", "") & "';" _
					& "IF @Units > 0 BEGIN" _
					& " IF @Virtual = 0 BEGIN" _
					& "  DECLARE @AllUnits int;" _
					& "  SELECT @AllUnits = Nis FROM SKLAD WHERE NCard = @ID_U;" _
					& "  IF @AllUnits >= @Units BEGIN" _
					& "   INSERT INTO REMONT (ID_D, ID_U, Units, Date, IfSpis, Virtual, Author, G_ID, W_ID)" _
					& "    VALUES (@ID_D, @ID_U, @Units, @Date, 0, 0, @User, 0, -2);" _
					& "   UPDATE Storages SET Nstorage = Nstorage - @Units, Nrepairs = Nrepairs + @Units WHERE Ncard = @ID_U;" _
					& "   INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS)" _
					& "    VALUES (@Date, @ID_U, @User, 'Администратор DEVIN', 'При создании ремонта ' + CAST(@Units AS varchar(6)) + ' деталей перемещены со склада в используемые');" _
					& "   INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS)" _
					& "    VALUES (@Date, @ID_D, @User, 'Администратор DEVIN', 'Ремонт: исп. ' + @ID_U + ' ' + CAST(@Units AS varchar(6)) + ' шт.');" _
					& "  END;" _
					& " END ELSE BEGIN" _
					& "  INSERT INTO REMONT (ID_D, ID_U, Units, Date, IfSpis, Virtual, Author, G_ID, W_ID)" _
					& "   VALUES (@ID_D, @ID_U, @Units, @Date, 0, 1, @User, 0, -2);" _
					& "  UPDATE Storages SET Nrepairs = Nrepairs + @Units WHERE Ncard = @ID_U;" _
					& "  INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS)" _
					& "   VALUES (@Date, @ID_U, @User, 'Администратор DEVIN', 'При создании виртуального ремонта ' + CAST(@Units AS varchar(6)) + ' деталей добавлены в используемые')" _
					& "  INSERT INTO ELMEVENTS (EDATE, CNAME, CUSER, EVGR, EVENTS)" _
					& "   VALUES (@Date, @ID_D, @User, 'Администратор DEVIN', 'Ремонт: исп. ' + @ID_U + ' ' + CAST(@Units AS varchar(6)) + ' шт. (виртуально)')" _
					& " END;" _
					& "END;" _
					& "COMMIT;"
				end if
			end if
		next

		' Получение информации о созданных ремонтах и обратная связь в форму
		dim rs : set rs = server.createObject("ADODB.Recordset")
		dim ifOne : ifOne = null
		dim repairsCount : repairsCount = 0
		rs.open "SELECT Inum, ROW_NUMBER() OVER(ORDER BY W_ID) AS N FROM REMONT WHERE (W_ID = -2) ORDER BY Inum DESC", conn
			if not rs.eof then
				ifOne = rs(0)
				repairsCount = rs(1)
			end if
		rs.close

		' Группировка ремонтов в списание
		if not isnull(ifOne) then
			dim offGroup : offGroup = request.form("createOff")
			if isnull(offGroup) or offGroup = "" then
				conn.execute "UPDATE REMONT SET W_ID = 0 WHERE (W_ID = -2)"
				response.write "<div class='done'>Создано ремонтов: " & repairsCount & " из " & repairsAllCount & "</div>"
				response.write "<a href='/devin/repair/##" & ifOne & "'>Перейти к последнему ремонту из созданных</a>"
			else
				conn.execute "INSERT INTO Writeoffs (Name, Type, Date, FolderId) VALUES ('" & DecodeUTF8(offGroup) & "', 'expl', GetDate(), 0)"
				dim offID
				rs.open "SELECT Max(Id) FROM Writeoffs", conn
					offID = rs(0)
				rs.close
				conn.execute "UPDATE REMONT SET W_ID = " & offID & " WHERE (W_ID = -2)"
				response.write log("off" & offID, "Автоматическое создание списания " & DecodeUTF8(offGroup) & " [off" & offID & "] при оформлении группы ремонтов со склада")

				response.write "<div class='done'>Создано ремонтов: " & repairsCount & " из " & repairsAllCount & "</div>"
				response.write "<a href='/devin/repair/##off" & offID & "'>Перейти к созданному списанию</a>"
			end if
		else
			if repairsAllCount > 0 then
				response.write "<div class='error'>Не было создано ни одного ремонта из " & repairsAllCount & "</div>"
			else
				response.write "<div class='error'>Не было получено ни одного ремонта</div>"
			end if
		end if

		conn.close
		set rs = nothing
		set conn = nothing
	end if
%>