<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn:          set conn      = server.createObject("ADODB.Connection")
	dim rs:            set rs        = server.createObject("ADODB.Recordset")
	dim key:           key           = array("name", "inventory", "attribute", "mol", "description", "name, class_device")
	dim direction:     direction     = array("", " DESC")
	dim sortKey:       sortKey       = request.queryString("key")
	dim sortDirection: sortDirection = request.queryString("direction")
	dim id:            id            = request.queryString("id")
	dim search:        search        = request.queryString("text")
	dim head:          head          = "<table class='items'><thead>" _
	& "<td width='20px'><input type='checkbox' class='selecter-all' />" _
	& "<th width='20px' data-type='type'>" _
	& "<th width='100px' data-type='string'>название" _
	& "<th width='80px' data-type='number'>инв. номер" _
	& "<th width='140px' data-type='string'>м.о.л." _
	& "<th data-type='string'>описание" _
	& "</tr></thead><tbody>"

	if sortKey = "" or isnull(sortKey) then sortKey  = "5"
	if sortDirection  = "" or isnull(sortDirection) then sortDirection = "0"

	dim sql: sql = "SELECT " _
	& "DEVICE.inventory, " _
	& "DEVICE.class_device, " _
	& "DEVICE.number_device, " _
	& "DEVICE.name, " _
	& "DEVICE.number_comp, " _
	& "DEVICE.description, " _
	& "DEVICE.mol, " _
	& "DEVICE.attribute, " _
	& "DEVICE.used, " _
	& "[GROUP].G_ID " _
	& "FROM DEVICE " _
	& "LEFT OUTER JOIN [GROUP] ON [GROUP].G_ID = DEVICE.G_ID " _
	& "WHERE (DEVICE.deleted = 0) ORDER BY " & key(sortKey) & direction(sortDirection)

	if search <> "" then 
		sql = replace(sql, "WHERE", replace("WHERE (DEVICE.inventory {0} OR DEVICE.class_device {0} OR DEVICE.number_device {0} OR DEVICE.name {0} OR DEVICE.number_comp {0} OR DEVICE.description1C {0} OR DEVICE.description {0} OR DEVICE.install_date {0} OR DEVICE.MOL {0} OR DEVICE.number_serial {0} OR DEVICE.number_passport {0} OR DEVICE.attribute {0}) AND ", "{0}", " LIKE '%" & search & "%'"))
	end if
	'response.write "<div class='debug'>" & sql & "</div>"

	dim i, j, q
	dim devices(1500, 9), Ndevices
	conn.open everest
	rs.open   sql, conn
	Ndevices = 0
		do while not rs.eof
			for i = 0 to 9
				devices(Ndevices,i) = trim(rs(i))
			next
			rs.MoveNext
			Ndevices = Ndevices + 1
		loop
	rs.close

	if search = "" then 

		dim RHost, RHostTemp, IDTemp

		'Подготовка списка актуальных записей
		sql = "SELECT Report.RHost, Report.ID, Report.RUser, Report.RDateTime, Item.IField, Item.IValue " _
		& "FROM Report INNER JOIN Item ON Report.ID = Item.ReportID " _
		& "WHERE (Item.IPage = 'Суммарная информация') AND (Report.RHostComment <> 'deleted') " _
		& "ORDER BY Report.RHost, Report.ID, Item.IField DESC"

		rs.open sql, conn
		do while not rs.eof
			RHostTemp = rs("RHost")
			IDTemp = rs("ID")
			if RHostTemp <> RHost then 
				if RHost <> "" then response.write "</table></div></div>"
				if request.cookies("comp" & RHostTemp) = "open" then
					response.write "<div class='unit open' id='comp" & RHostTemp & "'>" _
					& "<table class='caption'><tr>" _
					& "<th width='150px'>" & RHostTemp _
					& "<td width='150px'>" & rs("RUser") _
					& "<td>" & rs("RDateTime") & " (" & datediff("d", datevalue(rs("RDateTime")), date) & " дней)" _
					& "</tr></table>" _
					& "<div class='items_block'>" _
					& "<div class='title' actions='openComputerCart' id='" & RHostTemp & "'>Открыть карточку компьютера</div>" _
					& "<table class='items'>"
				else
					response.write "<div class='unit' id='comp" & RHostTemp & "'>" _
					& "<table class='caption'><tr>" _
					& "<th width='150px'>" & RHostTemp _
					& "<td width='150px'>" & rs("RUser") _
					& "<td>" & rs("RDateTime") & " (" & datediff("d", datevalue(rs("RDateTime")), date) & " дней)" _
					& "</tr></table>" _
					& "<div class='items_block hide'>" _
					& "<div class='title' actions='openComputerCart' id='" & RHostTemp & "'>Открыть карточку компьютера</div>" _
					& "<table class='items'>"
				end if
				RHost = RHostTemp
				ID = IDTemp	
				response.write "<tr><td width='250px'>" & rs("IField") & "<td>" & rs("IValue") & "</tr>"
			else 
				if ID = IDTemp then response.write "<tr><td width='250px'>" & rs("IField") & "<td>" & rs("IValue") & "</tr>"
			end if
			rs.movenext
		loop
		response.write "</table></div></div>"
		rs.close

	else

		response.write"<div class='unit'><table class='caption'><tr><th>Поиск совпадений по запросу: " & search & "</tr></table>" & head
		for i = 0 to Ndevices - 1 
			response.write "<tr id='" & devices(i, 2) & "'>" _
			& "<td><input type='checkbox' class='selecter' />" _
			& "<td><div class='led on'></div>" _
			& "<td>" & devices(i, 3) _
			& "<td>" & devices(i, 0) _
			& "<td>" & devices(i, 6) _
			& "<td>" & devices(i, 5) _
			& "</tr>"
		next
		response.write"</tbody></table></div>"

	end if

	conn.close
	set rs   = nothing
	set conn = nothing
%>