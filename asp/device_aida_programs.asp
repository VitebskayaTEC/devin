<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = Server.CreateObject("ADODB.Connection")
	dim rs   : set rs = Server.CreateObject("ADODB.Recordset")
	dim name : name = Request.QueryString("name")
	dim id   : id = 0
	dim sql  : sql = ""

	conn.open everest

	response.write "<div class='cart-header'>�� � �� ���������� " & name & "</div>"

	' ��������� ID ������� �� ��������� ����. ���� ���������� �� ����� (�� ������, �.�. �������� �� uuid)
	sql = "SELECT Max(ID) FROM Report WHERE LOWER(RHost) = LOWER('" & name & "')"
	rs.open sql, conn
		if not rs.eof then id = rs(0)
	rs.close

	' ������ �� ��������� ������ � ������������ �������
	sql = "SELECT IField, IValue FROM Item WHERE ReportId = " & id & " AND IPage = '������������ �������' AND (" _
	& "(IGroup = '�������� ������������ �������' AND IField = '�������� ��') " _
	& "OR (IGroup = '�������� ������������ �������' AND IField = '������ ��') " _
	& "OR (IGroup = '������������ ����������' AND IField = '���� ��������'))"
	rs.open sql, conn
	if not rs.eof then
		response.write "<b>������������ �������</b><table><tbody>"
		do while not rs.eof
			response.write "<tr><td>" & rs(0) & "</td><td>" & rs(1) & "</td></tr>"
			rs.movenext
		loop
		response.write "</tbody></table><hr />"
	end if
	rs.close

	' ������ �� ��������� ������ �� ���� � ������� ������
	sql = "SELECT IDevice FROM Item WHERE ReportId = " & id _
	& " AND IPage = '������������� ���������'" _
	& " AND IDevice NOT LIKE '%Microsoft Visual C++%'" _
	& " AND IDevice NOT LIKE '%.NET F%'" _
	& " AND IDevice NOT LIKE '%vs_%'" _
	& " AND IDevice NOT LIKE '%Update%'" _
	& " AND IDevice NOT LIKE '%icecap_%'" _
	& " GROUP BY IDevice"
	rs.open sql, conn
	if not rs.eof then
		response.write "<div class='cart-overflow'><b>������������� ��</b>"
		do while not rs.eof
			response.write "<div>" & rs(0) & "</div>"
			rs.movenext
		loop
		response.write "</div>"
	end if
	rs.close

	conn.close
	set rs = nothing
	set conn = nothing

	response.write "<table class='cart-menu'><tr><td onclick='cartBack()'>��������� � ��������<td onclick='cartClose()'>�������</tr></table>"
%>