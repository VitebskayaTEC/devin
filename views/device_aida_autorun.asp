<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs : set rs = Server.CreateObject("ADODB.Recordset")
	dim id : id = Request.QueryString("id")

	' ��������� ����� ����������, �� ������� ������������� �����, � ��� ����� uuid
	dim sql : sql = "SELECT name, DMI_UUID FROM DEVICE WHERE (number_device = '" & id & "')"
	conn.open everest
	rs.open sql, conn
		dim name : name = trim(rs(0))
		dim aida : aida = RS(1)
	rs.close

	response.write "<div class='cart-header'>������������ " & name & "</div>"

	' ��������� ID ������� �� ��������� ����. ���� ���������� �� ����� (�� ������, �.�. �������� �� uuid)
	sql = "SELECT ID FROM Report WHERE (RHost = '" & name & "') ORDER BY RDateTime DESC"
	rs.open sql, conn
		dim report
		if not rs.eof then report = rs(0)
	rs.close

	' ������ �� ��������� ������ �� ���� � ������� ������
	sql = "SELECT IDevice, IField, IValue FROM Item WHERE (ReportID  = '" & report & "' AND IPage = '������������') ORDER BY IDevice"

	rs.open sql, conn
	if not rs.eof then
		response.write "" _
		& "<div class='cart-overflow cart-history-box'>" _
		& "<table class='cart-history'>" _
		& "<thead>" _
		& "<tr>" _
		& "<th width='160px' data-type='string' onclick='_sort(this)'>�������" _
		& "<th width='200px' data-type='string' onclick='_sort(this)'>��������" _
		& "<th data-type='string' onclick='_sort(this)'>��������" _
		& "</tr>" _
		& "</thead>"
		do while not rs.eof
			response.write "" _
			& "<tr>" _
			& "<td>" & trim(rs("IDevice")) _
			& "<td>" & trim(rs("IField")) _
			& "<td>" & trim(rs("IValue")) _
			& "</tr>"
			rs.movenext
		loop
		response.write "</tbody></table></div>"
	else
		response.write "<div class='error'>������ ���</div>"
	end if
	rs.close

	conn.close
	set rs = nothing
	set conn = nothing

	response.write "<table class='cart-menu'><tr><td onclick='cartBack()'>��������� � ��������<td onclick='cartClose()'>�������</tr></table>"
%>