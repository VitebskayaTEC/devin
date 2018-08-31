<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs : set rs = Server.CreateObject("ADODB.Recordset")
	dim id : id = Request.QueryString("id")

	' Получение имени устройства, на которое запрашивается отчет, и его ключа uuid
	dim sql : sql = "SELECT name, DMI_UUID FROM DEVICE WHERE (number_device = '" & id & "')"
	conn.open everest
	rs.open sql, conn
		dim name : name = trim(rs(0))
		dim aida : aida = RS(1)
	rs.close

	response.write "<div class='cart-header'>Список устройств для " & name & "</div>"

	' Получение ID репорта на выбранный комп. Комп выбирается по имени (хз почему, т.к. привязка по uuid)
	sql = "SELECT ID FROM Report WHERE (RHost = '" & name & "') ORDER BY RDateTime DESC"
	rs.open sql, conn
		dim report
		if not rs.eof then report = rs(0)
	rs.close

	' Запрос на получение данных из базы и парсинг данных
	sql = "SELECT " _
	& "IPage, IDevice, IGroup, IField, IValue " _
	& "FROM Item " _
	& "WHERE (ReportID = '" & report & "') AND (IPage = 'Монитор' OR IPage = 'Принтеры') " _
	& "ORDER BY IPage, IDevice, IGroup, IField, IValue"

	'response.write sql

	rs.open sql, conn
	if not rs.eof then
		response.write "" _
		& "<div class='cart-overflow cart-history-box'>" _
		& "<table class='cart-history'>" _
		& "<thead>" _
		& "<tr>" _
		& "<th width='300px' data-type='string' onclick='_sort(this)'>Устройство" _
		& "<th data-type='string' onclick='_sort(this)'>Информация" _

		& "</tr>" _
		& "</thead>"

		'& "<th data-type='string' onclick='_sort(this)'>Значение" _

		dim idevice : idevice = ""
		dim ideviceOld : ideviceOld = ""
		do while not rs.eof
			idevice = trim(rs("IDevice"))
			if idevice = ideviceOld then
				response.write "<div><b>" & trim(rs("IField")) & "</b>: " & trim(rs("IValue")) & "</div>"
			else
				if ideviceOld <> "" then response.write "</div></tr>"
				ideviceOld = idevice
				response.write "" _
				& "<tr><td>" & idevice _
				& "<td><div><a onclick='toggleInfoBlock(this)'>Просмотреть</a></div><div class='cart-info-block hide'><div><b>" & trim(rs("IField")) _
				& "</b>: " & trim(rs("IValue")) _
				& "</div>"
			end if
			rs.movenext
		loop
		response.write "</tbody></table></div>"
	else
		response.write "<div class='error'>Данных нет</div>"
	end if
	rs.close

	conn.close
	set rs = nothing
	set conn = nothing

	response.write "<table class='cart-menu'><tr><td onclick='cartBack()'>Вернуться к карточке<td onclick='cartClose()'>Закрыть</tr></table>"
%>
<script>
	function toggleInfoBlock(a) {
		$(a).closest("td").find(".cart-info-block").slideToggle(50);
		a.innerHTML = a.innerHTML == "Просмотреть" ? "Свернуть" : "Просмотреть" ;
	}
</script>