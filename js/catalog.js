function cartOpen(node) {
	id = node.id;
	cartOpenBack();
	setHash(id);
}

function search(input) {
	var val = input.value.toLowerCase();
	if (val == "") {
		$(".items tbody tr").css("display", "table-row");
	} else {
		$(".items tbody tr").each(function() {
			var text = this.querySelector("td").innerHTML.toLowerCase();
			this.style.display = text.indexOf(val) > -1 ? 'table-row' : 'none';
		})
	}
}

function cartOpenBack() {
	$("#cart").fadeIn(100).load("/devin/views/catalog_" + getCart() + "_cart.asp?id=" + id + "&r=" + Math.random());
	$(".view .selected").removeClass("selected");
	try { $("#" + id).parent().addClass("selected"); } catch (e) {}
}

function getCart() {
	return (id.indexOf("prn") > -1) ? "printer" : "cartridge";
}


function createPrinter() {
	$.post("/devin/exes/catalog/catalog_create_printer.asp?r=" + Math.random(), function(data) {
		document.location.hash = "##prn" + data;
	});
}

function createCartridge() {
	$.post("/devin/exes/catalog/catalog_create_cartridge.asp?r=" + Math.random(), function(data) {
		document.location.hash = "##cart" + data;
	});
}

/*
Работа с карточками каталога (для принтеров и картриджей)
*/

function cartSave() {
	$.post("/devin/exes/catalog/catalog_save_" + getCart() + "_cart.asp?id=" + id + "&r=" + Math.random(), $("#form").serialize(), function(data) {
		document.getElementById("console").innerHTML = data;
	});
}

function cartDelete() {
	$.post("/devin/exes/catalog/catalog_delete_" + getCart() + "_cart.asp?id=" + id + "&r=" + Math.random(), "", function(data) {
		$(".view .selected").remove();
		cartClose();
	});
}

function addCompare() {
	$.post("/devin/exes/catalog/catalog_add_compare.asp?r=" + Math.random(), "compare=" + document.getElementById("new-compare").value + "&active=" + getCart() + "&id=" + id, function(data) {
		var text = data;
		$("#cart").load("/devin/views/catalog_" + getCart() + "_cart.asp?id=" + id + "&r=" + Math.random(), function() {
			document.getElementById("console").innerHTML = text;
		});
	});
}

function delCompare(div) {
	$.post("/devin/exes/catalog/catalog_delete_compare.asp?r=" + Math.random(), "compare=" + div.parentNode.getAttribute("data-id") + "&active=" + getCart() + "&id=" + id, function(data) {
		var text = data;
		$("#cart").load("/devin/views/catalog_" + getCart() + "_cart.asp?id=" + id + "&r=" + Math.random(), function() {
			document.getElementById("console").innerHTML = text;
		});
	});
}