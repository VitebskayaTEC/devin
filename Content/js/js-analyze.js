$(".view")
	.on("mousedown", ".group .items thead th", function() { _sort(this) })
	.on("mousedown", ".unit .caption th", function() { toggle(this) });

function toggle(node) {
	var $unit = $(node).closest('.unit');
	if ($unit.hasClass("open")) {
		$unit.children(".items_block").slideToggle(150, function() {
			$unit.removeClass("open")
		});
		setCookie($unit.attr("id"), "", {
			expires: 999999999
		});
	} else {
		$unit.addClass("open").children(".items_block").slideToggle(150);
		setCookie($unit.attr("id"), "open", {
			expires: 999999999
		});
	}
}

function unique(rowA, rowB, way, colNum) {
	if (way == "up") {
		return rowA.cells[colNum].querySelector("a").innerHTML > rowB.cells[colNum].querySelector("a").innerHTML ? 1 : -1;
	} else {
		return rowB.cells[colNum].querySelector("a").innerHTML > rowA.cells[colNum].querySelector("a").innerHTML ? 1 : -1;
	}
};

function exportToExcel() {
	var formData = "";
	var trs = document.getElementById("analyzeCartridges").getElementsByTagName("tbody")[1].getElementsByTagName("tr");
	for (var i = 0; i < trs.length; i++) {
		var input = trs[i].querySelector("input");
		var val = +input.value;
		if (val > 0 && val != NaN) {
			// Получаем данные, если предполагается или введено некоторое положительное количество
			formData += (formData == "" ? "" : "----") +
				encodeURIComponent(trs[i].querySelector("a").innerHTML.toString()) + "__" +
				trs[i].getElementsByTagName("td")[2].innerHTML.toString() + "__" +
				input.name + "__" +
				val.toFixed(0) + "__" +
				input.getAttribute("color");
		}
	}
	document.getElementById("export").style.display = "none";
	$.post("/devin/exes/analyze/analyze_export.asp", "data=" + formData, function(data) {
		document.getElementById("export").querySelector("div").innerHTML = data;
		$("#export").slideDown(100);
	});
}