<%@ WebHandler Language="C#" Class="history" %>

using System;
using System.Web;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

public class history : IHttpHandler {

	public void ProcessRequest (HttpContext context) {
		context.Response.Write("<div class='cart-header'>История событий DEVIN</div>" +
		"<div class='cart-overflow'><table class='cart-history small-text'>" +
		"<col width='140'><col width='120'><col width='70'><col width='700'>" +
		"<thead>" +
		"<tr>" +
		"<th data-type='date' onclick='_sort(this)'>Дата" +
		"<th data-type='string' onclick='_sort(this)'>Объект" +
		"<th data-type='string' onclick='_sort(this)'>Юзер" +
		"<th data-type='string' onclick='_sort(this)'>Событие" +
		"</tr>" +
		"<tr>" +
		"<td><input onkeyup='_search(this)' />" +
		"<td><input onkeyup='_search(this)' />" +
		"<td><input onkeyup='_search(this)' />" +
		"<td><input onkeyup='_search(this)' />" +
		"</tr>" +
		"</thead>" +
		"<tbody>");


		SqlConnection connection = new SqlConnection(@"Server=log1\SQL2005; Database=EVEREST; Uid=user_everesr; Pwd=EveresT10;");
		try
		{
			connection.Open();
			SqlCommand command = new SqlCommand("SELECT EDATE, CNAME, CUSER, EVENTS FROM ELMEVENTS WHERE (EVGR = 'Администратор DEVIN') ORDER BY EDATE DESC", connection);
			using (SqlDataReader reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					context.Response.Write("<tr><td>" + reader[0].ToString() + "<td>" + reader[1].ToString().Trim() + "<td>" + reader[2].ToString().Trim() + "<td>" + parse(reader[3].ToString().Trim()) + "</tr>");
				}
			}
		}
		catch (Exception error) {context.Response.Write("<div class='error'>" + error.Message + "</div>");}
		finally {connection.Close();}

		context.Response.Write("</tbody>" +
		"</table></div>" +
		"<table class='cart-menu'><tr>" +
		"<td onclick='cartClose()'>Закрыть</td>" +
		"</tr></table>");
	}

	string parse(string text)
	{
		const string
			//device_pattern = @"[xX]{5,7}-[A-Z]{0,3}-\d{1,3}|[0-9a-zA-Zа-яА-Я\*]{5,7}-[A-Z]{3}-\d{1,3}|\*\*\*-[A-Z]{0,3}-\d{1,3}",
			device_pattern = @"([xX]{5,7}|[0-9a-zA-Zа-яА-Я\*]{5,7}|\*\*\*)-[A-Z]{0,3}-\d{1,3}",
			storage_pattern = @"исп.\ \d{5,7}\ |\d{5,7} ",
			catalog_pattern = @"cart[0-9]{0,}|prn[0-9]{0,}",
			writeoff_pattern = @"off[0-9]{0,4}",
			repair_pattern = @"repair[0-9]{0,4}";

		// Устройства
		foreach (Match match in Regex.Matches(text, device_pattern, RegexOptions.IgnoreCase))
			text = text.Replace(match.Value, "<a href='/devin/devices/##" + match.Value + "'>" + match.Value + "</a>");

		// Детали
		foreach (Match match in Regex.Matches(text, storage_pattern, RegexOptions.IgnoreCase))
			text = text.Replace(match.Value, "<a href='/devin/storage/##" + match.Value.Replace("исп. ", "") + "'>" + match.Value + "</a>");

		// Элементы каталога
		foreach (Match match in Regex.Matches(text, catalog_pattern, RegexOptions.IgnoreCase))
			text = text.Replace(match.Value, "<a href='/devin/catalog/##" + match.Value+ "'>" + match.Value + "</a>");

		// Ремонты
		foreach (Match match in Regex.Matches(text, repair_pattern, RegexOptions.IgnoreCase))
			text = text.Replace(match.Value, "<a href='/devin/repair/##" + match.Value.Replace("repair", "") + "'>" + match.Value + "</a>");

		// Списания
		foreach (Match match in Regex.Matches(text, writeoff_pattern, RegexOptions.IgnoreCase))
			text = text.Replace(match.Value, "<a href='/devin/repair/##" + match.Value + "'>" + match.Value + "</a>");

		return text;
	}

	public bool IsReusable {get {return false;}}
}