function toggle(node) {
	var $unit = $(node).closest('.unit');
	if ($unit.hasClass("open")) {
		$unit.children(".items_block").slideToggle(150, function() { $unit.removeClass("open"); });
		setCookie($unit.attr("id"), "", { expires: 9999999999 });
	} else {
		$unit.addClass("open").children(".items_block").slideToggle(150);
		setCookie($unit.attr("id"), "open", { expires: 9999999999 });
	}
}

function cartOpen(node) {
	id = node.id;
	cartOpenBack();
	setHash(id);
}

function cartOpenBack() {
	$("#cart").html('loading...').fadeIn(100).load("/devin/asp/aida_cart.asp?id=" + id + "&r=" + Math.random());
	$(".view .selected").removeClass("selected");
	try { $("#" + id).addClass("selected"); } catch (e) {}
}

function cartBack() { cartOpenBack(); }

function reportDelete() {
	if (confirm("Отчет по данному компьютеру будет безвозвратно удален. Продолжить?")) {
		$.post("/devin/exes/aida/aida_delete_report.asp?r=" + Math.random, "rhost=" + id, restore);
		cartClose();
	}
}