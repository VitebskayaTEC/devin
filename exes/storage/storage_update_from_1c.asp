<!-- #include virtual ="/devin/core/core.inc" -->
<%
	dim conn : set conn = server.createobject("ADODB.Connection")
	conn.open everest

	dim queries : queries = split(DecodeUTF8(request.form("query")), chr(13))
	dim query
	for each query in queries
		conn.execute query
		'response.write query & "<br />"
	next

	conn.close
	set conn = nothing
%>