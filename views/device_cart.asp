<!-- #include virtual ="/devin/core/core.inc" -->
<%
	' ���������� ���������� � ����������� � ���� ������
	dim conn: set conn = server.createObject("ADODB.Connection")
	dim rs:   set rs   = server.createObject("ADODB.Recordset")
	dim keys: set keys = server.createObject("Scripting.Dictionary")
	dim id:   id = request.queryString("id")
	dim sql:  sql = ""

	conn.open everest

	dim size: size = 22
	dim device(22, 2), temp, cls

	'�������� ������ �� ����������
	sql = "Exec GetDeviceById '" & id & "';"
	rs.open sql, conn
	if not rs.eof then
		dim i
		for i = 0 to rs.fields.count - 1
			keys.add rs(i).name, trim(rs(i))
		next
	else
		response.write "���������� �� �������!"
		response.end
	end if
	rs.close

	cls = keys("class_device")

	'������ header � ����������� �� ���� � ������� � ���� ����������
%>

<div class='cart-header'>
	<% if cls = "CMP" then %>
		��������� <%=keys("name")%>
	<% else %>
		<% if id = "000000-000-00" then %>
			����� ����������
		<% else %>
		<%
			rs.open "SELECT name FROM DEVICE WHERE (number_device = '"& keys("number_comp") &"')", conn
			if not rs.eof then
				response.write keys("name") & " �� " & trim(rs(0))
			else
				response.write keys("name")
			end if
			rs.close
		%>
		<% end if %>
	<% end if %>
</div>

<form id='cart-form'>
	<table class='cart-table'>

		<tr>
			<td>������������</td>
			<td><input name='name' value='<%=keys("name")%>' /></td>
		</tr>

		<tr>
			<td>����������� �����</td>
			<td><input name='inventory' value='<%=keys("inventory")%>' /></td>
		</tr>

		<tr>
			<td>��� ����������</td>
			<td>
				<select name='class_device'>
					<option value='0'>?</option>
					<%
						rs.open "SELECT T_alias, T_alias, T_name FROM catalog_device_types ORDER BY T_name", conn
						do while not rs.eof
							if cls = rs(0) then
								response.write "<option value='" & rs(1) & "' selected>" & rs(2)
							else
								response.write "<option value='" & rs(1) & "'>" & rs(2)
							end if
							rs.movenext
						loop
						rs.close
					%>
				</select>
			</td>
		</tr>

		<tr>
			<td>����� ����������</td>
			<td>
				<i><small><%=keys("number_device")%></small></i>
			</td>
		</tr>

		<% if cls <> "CMP" then %>
		<tr>
			<td>���������</td>
			<td>
				<select name='number_comp'>
					<option value='0'>?"</option>
					<%
						rs.open "SELECT RTRIM(number_device), RTRIM(number_device), RTRIM(name) FROM DEVICE WHERE (class_device = 'CMP' AND deleted = 0) ORDER BY name", conn
						temp = keys("number_comp")
						do while not rs.eof
							if temp = rs(0) then
								Response.Write "<option selected value='" & rs(1) & "'>" & rs(2)
							else
								Response.Write "<option value='" & rs(1) & "'>" & rs(2)
							end if
							rs.MoveNext
						loop
						rs.Close
					%>
				</select>
				<a onclick='cartOpen(document.getElementById("<%=temp%>"))'>��������</a>
			</td>
		</tr>
		<% end if %>

		<tr>
			<td>������� � 1�</td>
			<td><input name='description1C' value='<%=keys("description1C")%>' /></td>
		</tr>

		<tr>
			<td>��������</td>
			<td><textarea name='description'><%=keys("description")%></textarea></td>
		</tr>

		<tr>
			<td>�.�.�.</td>
			<td><input name='mol' value='<%=keys("MOL")%>' /></td>
		</tr>

		<tr>
			<td>������������</td>
			<td><input name='attribute' value='<%=keys("attribute")%>' /></td>
		</tr>

		<tr>
			<td>�������� �����</td>
			<td><input name='number_serial' value='<%=keys("number_serial")%>' /></td>
		</tr>

		<tr>
			<td>���������� �����</td>
			<td><input name='number_passport' value='<%=keys("number_passport")%>' /></td>
		</tr>

		<tr>
			<td>������-���</td>
			<td><input name='service_tag' value='<%=keys("service_tag")%>' /></td>
		</tr>

		<tr>
			<td>���� ���������</td>
			<td>
				<input name='install_date' style='width: 100px' value='<%=datevalue(keys("install_date"))%>' />
				<a>
					<b>
						<%=datediff("h", datevalue(keys("install_date")), datevalue(date))%> �����
					</b>
				</a>
			</td>
		</tr>

		<% if cls = "CMP" then %>
		<tr>
			<td>��</td>
			<td>
				<select name='OS'>
					<option value='0'>?</option>
					<%
						rs.Open "Select T_alias, T_alias,T_name From catalog_os Order by T_name", CONN
						do while not rs.eof
							if keys("OS") = trim(rs(0)) then
								response.write "<option selected value='" & trim(rs(1)) & "'>" & trim(rs(2))
							else
								response.write "<option value='" & trim(rs(1)) & "'>" & trim(rs(2))
							end if
							rs.MoveNext
						loop
						rs.Close
					%>
				</select>
			</td>
		</tr>
		<% end if %>

		<% if cls = "CMP" then %>
		<tr>
			<td>���� ��</td>
			<td><input name='OSKEY' value='<%=keys("OSKEY")%>' /></td>
		</tr>

		<tr>
			<td>���� ��� �����</td>
			<td><input name='PRKEY' value='<%=keys("PRKEY")%>' /></td>
		</tr>
		<% end if %>

		<tr>
			<td>������</td>
			<td>
				<input class='numbers' name='passportgold' value='<%=keys("PassportGold")%>' />
				<a>
					<b>
						<% if not IsNull(keys("Devices1C_Gold")) then response.write (Fix(keys("Devices1C_Gold") * 100000) / 100000) %>
					</b>
				</a>
			</td>
		</tr>

		<tr>
			<td>�������</td>
			<td>
				<input class='numbers' name='passportsilver' value='<%=keys("PassportSilver")%>' />
				<a>
					<b>
						<% if not IsNull(keys("Devices1C_Silver")) then response.write (Fix(keys("Devices1C_Silver") * 100000) / 100000) %>
					</b>
				</a>
			</td>
		</tr>

		<tr>
			<td>�������</td>
			<td>
				<input class='numbers' name='passportplatinum' value='<%=keys("PassportPlatinum")%>' />
				<a>
					<b>
						<% if not IsNull(keys("Devices1C_Platinum")) then response.write (Fix(keys("Devices1C_Platinum") * 100000) / 100000) %>
					</b>
				</a>
			</td>
		</tr>

		<tr>
			<td>��������</td>
			<td>
				<a>
					<b>
						<% if not IsNull(keys("Devices1C_Palladium")) then response.write (Fix(keys("Devices1C_Palladium") * 100000) / 100000) %>
					</b>
				</a>
			</td>
		</tr>

		<tr>
			<td>���</td>
			<td>
				<input class='numbers' name='passportmpg' value='<%=keys("PassportMPG")%>' />
				<a>
					<b>
						<% if not IsNull(keys("Devices1C_Mpg")) then response.write (Fix(keys("Devices1C_Mpg") * 100000) / 100000) %>
					</b>
				</a>
			</td>
		</tr>

		<% if cls = "PRN" then %>
		<tr>
			<td>������� �������</td>
			<td>
				<select name='ID_prn'>
					<option value='0'>?</option>
					<%
						rs.Open "Select N, Caption From PRINTER Order by Caption", CONN
						dim temp_n
						do while not rs.eof
							temp_n = rs(0)
							if cstr(keys("ID_prn")) = cstr(temp_n) then
								response.write "<option selected value='" & temp_n & "'>" & rs(1)
							else
								response.write "<option value='" & temp_n & "'>" & rs(1)
							end if
							rs.MoveNext
						loop
						rs.Close
					%>
				</select>
			</td>
		</tr>
		<% end if %>

		<tr>
			<td>������������</td>
			<td>
				<select name='used'>
					<option value='0'>���</option>
					<option value='1' <% if keys("used") = "1" then response.write "selected" %>>��</option>
				</select>
			</td>
		</tr>

		<tr>
			<td>������ � 1�</td>
			<td>
				<select name='check1C'>
					<option value='0'>���</option>
					<option value='1' <% if keys("check1C") = "1" then response.write "selected" %>>��</option>
				</select>
			</td>
		</tr>

		<% if cls = "CMP" then %>
		<tr>
			<td>������ � AIDA</td>
			<td>
				<select name='checkEverest'>
					<option value='0'>���</option>
					<option value='1' <% if keys("checkEverest") = "1" then response.write "selected" %>>��</option>
				</select>
			</td>
		</tr>
		<% end if %>

		<tr>
			<td>������</td>
			<td>
				<select name='G_ID'>
					<option value='0'>?</option>
					<%
						rs.Open "Select G_ID, G_ID, G_Title From [GROUP] Where (G_Type = 'device') Order by G_Title", CONN
						do while not rs.eof
							if keys("G_ID") = cstr(rs(0)) then
								Response.Write "<option value='" & trim(rs(1)) & "' selected>" & trim(rs(2))
							else
								Response.Write "<option value='" & trim(rs(1)) & "'>" & trim(rs(2))
							end if
							rs.movenext
						loop
					%>
				</select>
			</td>
		</tr>
	</table>
</form>

<div id="device1C" class="hide">
	<hr />
	<a onclick="$('#links, #device1C').toggle()">�������</a>
	<div class="cart-overflow" style="min-height: 10px; max-height: 140px;">
		<table>
			<tr>
				<td style="width: 250px;">������������</td>
				<td><%=keys("Devices1C_Description")%></td>
			</tr>
			<tr>
				<td>���</td>
				<td><%=keys("Devices1C_Guild")%></td>
			</tr>
			<tr>
				<td>�������������</td>
				<td><%=keys("Devices1C_SubDivision")%></td>
			</tr>
			<tr>
				<td>�.�.�.</td>
				<td><%=keys("Devices1C_Mol")%></td>
			</tr>
			<tr>
				<td>��������� ���������</td>
				<td><%=keys("Devices1C_BalanceCost")%></td>
			</tr>
			<tr>
				<td>��������������� ���������</td>
				<td><%=keys("Devices1C_DepreciationCost")%></td>
			</tr>
			<tr>
				<td>������</td>
				<td><%=keys("Devices1C_Gold")%></td>
			</tr>
			<tr>
				<td>�������</td>
				<td><%=keys("Devices1C_Silver")%></td>
			</tr>
			<tr>
				<td>�������</td>
				<td><%=keys("Devices1C_Platinum")%></td>
			</tr>
			<tr>
				<td>��������</td>
				<td><%=keys("Devices1C_Palladium")%></td>
			</tr>
			<tr>
				<td>���</td>
				<td><%=keys("Devices1C_Mpg")%></td>
			</tr>
		</table>
	</div>
</div>

<div id="links" class='cart-links'>
	<a onclick="$('#links, #device1C').toggle()">�������� �� ���. � �� 1�</a>
	<br />
	<a onclick='cartHistoryRepair()'>������� ��������</a>
	<br />

	<% if cls = "CMP" then %>
	<a onclick='cartAidaAutorun()'>������������</a>
	<br/>
	<a onclick='cartAidaPrograms("<%=keys("name")%>")'>������������ ������� � ������������� ��</a>
	<br/>
	<a onclick='cartAidaDevices()'>������������</a><br/>
	<% end if %>

	<% if cls = "CMP" or cls = "DIS" or cls = "PRN" then %>
	<a onclick='cartDefect()'>��������� ���</a>
	<% end if %>
</div>

<div id='console'></div>

<table class='cart-menu'>
	<tr>
		<td onclick='cartSave()'>���������</td>
		<td onclick='deviceToRepair()'>������</td>

		<% if cls = "CMP" then %>
		<td onclick='cartAida()'>Everest</td>
		<td onclick='cartHistory("<%=keys("name")%>")'>������� �� �����</td>

		<% end if %>

		<td onclick='cartHistory("<%=keys("number_device")%>")'>������� �� ���.�.</td>
		<td onclick='cartDelete()'>�������</td>
		<td onclick='cartCopy()'>�����.</td>
		<td onclick='cartClose()'>�������</td>
	</tr>
</table>

<%
	rs.close:   set rs   = nothing
	conn.close: set conn = nothing
	set keys = nothing
%>