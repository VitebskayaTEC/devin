<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs : set rs = Server.CreateObject("ADODB.Recordset")
	dim id : id = request.queryString("id")
	conn.open everest

	' Получение имени устройства, на которое запрашивается отчет
	dim name, uuid, sql
	sql = "SELECT RTRIM(name) FROM DEVICE WHERE (number_device = '" & id & "')"
	rs.open sql, conn
		if not rs.eof then name = rs(0)
	rs.close

	response.write "<div class='cart-header'>Aida64 : " & name & "</div><div id='aida-data' class='cart-overflow'>"

	' Запрос на получение данных из базы и парсинг данных
	sql = "SELECT INum, IPage, IDevice, IGroup, IField, IValue FROM Item WHERE ReportID  = (" _
		& "	SELECT TOP(1) ID FROM Report WHERE UPPER(RHost) = UPPER('" & name & "') ORDER BY RDateTime DESC" _
		& ") ORDER BY IPage, IDevice, IGroup, IField"
	rs.open sql, conn
	if rs.eof then
		response.write "<div class='error'>Устройство с данным ID не найдено</div>"
	else
		dim page, device, group, Vpage, Vdevice, Vgroup, inum
		page   = ""
		device = ""
		group  = ""
		response.write "<div><div><div>"
		do while not rs.eof
			inum = rs(0)
			Vpage = rs(1)
			Vdevice = rs(2)
			Vgroup = rs(3)

			if Vpage <> page then
				response.write "</div></div></div>" _
					& "<a for='p" & inum & "'>" & Vpage & "</a><div id='p" & inum & "' class='page'>" _
					& "<a for='d" & inum & "'>" & Vdevice & "</a><div id='d" & inum & "' class='set'>" _
					& "<a for='g" & inum & "'>" & Vgroup & "</a><div id='g" & inum & "' class='set'>"
				page = Vpage
				device = Vdevice
				group = Vgroup

			elseif Vdevice <> device then
				response.write "</div></div>" _
					& "<a for='d" & inum & "'>" & Vdevice & "</a><div id='d" & inum & "' class='set'>" _
					& "<a for='g" & inum & "'>" & Vgroup & "</a><div id='g" & inum & "' class='set'>"
				device = Vdevice
				group = Vgroup

			elseif Vgroup <> group then
				response.write "</div>" _
					& "<a for='g" & inum & "'>" & Vgroup & "</a><div id='g" & inum & "' class='set'>"
				group = Vgroup
			end if

			response.write "<div class='fv'><f>" & rs(4) & "</f> : <v>" & rs(5) & "</v></div>"
			rs.moveNext
		loop
		response.write "</div></div></div>"
	end if
	rs.close

	conn.close
	set rs = nothing
	set conn = nothing

	response.write "</div><table class='cart-menu'><tr><td onclick='cartBack()'>Вернуться<td onclick='cartClose()'>Закрыть</tr></table>"
%>
<script>
	$("#aida-data").on("mousedown", "a", function() {
		$(this).add("#" + this.getAttribute("for")).toggleClass("open");
	}).find("a").each(function() {
		if (this.innerHTML == "") this.parentNode.innerHTML = document.getElementById(this.getAttribute("for")).innerHTML;
	});
</script>