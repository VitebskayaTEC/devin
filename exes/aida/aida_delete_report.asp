<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn: set conn = server.createObject("ADODB.Connection")
	conn.open everest

	conn.execute "UPDATE report SET RHostComment = 'deleted' WHERE (RHost = '" & request.form("rhost") & "')"
	
	conn.close
	set conn = nothing
%>