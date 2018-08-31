<!-- #include virtual ="/devin/core/core.inc" -->
<%
	sub drop(str)
		on error resume next
		rs.close
		conn.close
		set rs = nothing
		set conn = nothing
		response.write "<div class='error'>" & str & "</div><table class='cart-menu'><tr><td onclick='cartClose()'>�������</td></tr></table>"
		response.end
	end sub

	' ���������, ������ �� ������ ��������� �������
	dim formData : formData = request.form("select")
	if isnull(formData) or formData = "" then drop("�� ������� ������ ��������� ���������") else response.write "<div class='cart-header'>���������� �������� �� ��������� ��������</div>"

	' ��������� ������ �� ���� ��������
	dim conn : set conn = server.createObject("ADODB.Connection")
	dim rs : set rs = server.createObject("ADODB.Recordset")
	conn.open everest
	dim ncard, sql
	for each ncard in split(formData, ";")
		if ncard <> "" then
			if sql <> "" then sql = sql & " OR "
			sql = sql & "NCard = '" & ncard & "'"
		end if
	next
	dim storages(1000, 2), i, Nstorages
	rs.open "SELECT NCard, Name, Nis FROM SKLAD WHERE (" & sql & ") AND (Nis > 0) ORDER BY NCard", conn
	if rs.eof then
		drop("��� ��������� ��� ���������� ��������")
	else
		Nstorages = -1
		do while not rs.eof
			Nstorages = Nstorages + 1
			for i = 0 to 2
				storages(Nstorages, i) = trim(rs(i))
			next
			rs.moveNext
		loop
	end if
	rs.close

	response.write "<div id='off-group'><input type='text' value='������� �� " & date & "' id='name-off-group' /> <input type='checkbox' checked id='create-off-group' /><label for='create-off-group'>������������� ��������� �������</label></div>"

	' ��������� ������ ������ �� �����������
	response.write "<div class='hide' id='computers-first'><select class='computers' onchange='changeDevice(this)'><optgroup label='�� �������'><option value='0'>?"
	rs.open "SELECT DP.name, DP.number_device, D.number_device, D.name, CASE WHEN LEN(D.description) < 50 THEN D.description ELSE(CAST(D.description AS nvarchar(47)) + '...') END FROM DEVICE AS D LEFT OUTER JOIN DEVICE AS DP ON (D.number_comp = DP.number_device AND DP.deleted <> 1) WHERE (D.deleted <> 1 AND D.used = 1 AND (D.class_device <> 'CMP' OR (D.class_device = 'CMP' AND D.number_device NOT IN (SELECT number_comp FROM DEVICE WHERE number_comp IS NOT NULL AND number_comp <> '' AND number_comp <> '?' GROUP BY number_comp)))) ORDER BY DP.name, D.name"
	dim activeID, prevID
	prevID = ""
	do while not rs.eof
		activeID = trim(rs(0))
		if activeID <> prevID then
			prevID = activeID
			response.write "</optgroup><optgroup label='" & activeID & "'><option value='" & trim(rs(1)) & "'>[���������] " & activeID
		end if
		response.write "<option value='" & trim(rs(2)) & "'>" & trim(rs(3)) & ": " & trim(rs(4))
		rs.moveNext
	loop
	rs.close
	response.write "</optgroup></select></div>"

	' ���������� ������� �����������
	response.write "<div class='cart-overflow small-text'><table>" _
		& "<thead><th>�������<th>�� ����� ����������<th>���-��<th>����.</thead><tbody id='repairs-positions'>"
	for i = 0 to Nstorages
		response.write "<tr class='pos" & storages(i, 0) & "'>" _
		& "<td>" & storages(i, 0) & " [<span>" & storages(i, 2) & "</span> ��. ��������]" _
		& " <a onclick='removePosition(this)'>�������</a>" _
		& " <a onclick='separatePosition(this)' class='pos-separate'>���������</a>" _
		& "<br />" & storages(i, 1) & "<td class='computers-container'>" _
		& "<td><input class='number' type='number' value='0' onkeyup='verifyCounts(this)' onchange='verifyCounts(this)' />" _
		& "<td><input type='checkbox' onchange='virtualVerifyCounts(this)'/></tr>"
	next
	response.write "</tbody></table></div>"

	' ������� ������� ����������� ��� ������ ����� � ��������� �� �� ���������
	response.write "<div id='console'></div>" _
		& "<table class='cart-menu'><tr>" _
		& "<td onclick='createRepairs()'>���������</td>" _
		& "<td onclick='cartClose()'>�������</td>" _
		& "</tr></table>"

	' ������� ���������� �������
	conn.close
	set rs = nothing
	set conn = nothing
%>
<script>
	// ����������� ������� � ������������ �� ��� ������. �������� �� �������, ����� �� ���������� ���������� ����� �� ����
	(function() {
		var computers = document.getElementById("computers-first").innerHTML;
		$("#cart").find(".computers-container").html(computers);

		//$('.computers').bselect();
	})()
</script>