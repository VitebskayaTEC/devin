<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn 	: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	conn.execute "INSERT INTO [GROUP] (G_Type, G_Title, G_Inside) VALUES ('" & request.form("app") & "','" & DecodeUTF8(request.form("title")) & "', 0)"

	conn.close
	set conn = nothing
%>