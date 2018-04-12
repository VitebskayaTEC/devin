// Глобальные переменные и навешивание функций
$(".view")
	.on("mousedown", ".unit:not(#solo) .caption th,.unit:not(#solo) .caption td:not(:first-child)", function() { toggle(this) })
	.on("mousedown", ".items tbody td:not(:first-child), .title", function() { cartOpen(this); })
	.on("mousedown", ".items thead th", function() { _sort(this) });

function toggle(node) {
    var $unit = $(node).closest('.unit');
	var id = $unit.attr("id") ? $unit.attr("id") : $unit.find(".title-wrapper:first-child").attr("id") ;
    if ($unit.hasClass("open")) {
        $unit.children(".items_block").slideToggle(150, function() { $unit.removeClass("open"); });
        setCookie(id, "close", { expires: 9999999999 });
    } else {
        $unit.addClass("open").children(".items_block").slideToggle(150);
        setCookie(id, "open", { expires: 9999999999 });
    }
}

function cartOpen(node) {
	id = node.parentNode.id;
	cartOpenBack();
	setHash(id);
}

function cartOpenBack() {
	$("#cart")
		.load("/devin/views/" + (id.indexOf("off") < 0 ? "repair_cart.asp" : "writeoff_cart.asp") + "?id=" + id.replace("off", "") + "&r=" + Math.random())
		.fadeIn(150);
	$(".view .selected").removeClass("selected");
	$("#" + id).addClass("selected");
}

function cartSave() {
	$.post("/devin/exes/repair/" + (id.indexOf("off") < 0 ? "repair" : "writeoff") + "_save_cart.asp?id=" + id + "&r=" + Math.random(), $("#form").serialize(), function(data) {
		if (data.indexOf("error") < 0) {
			$("#cart").load("/devin/views/" + (id.indexOf("off") < 0 ? "repair_cart.asp" : "writeoff_cart.asp") + "?id=" + id.replace("off", "") + "&r=" + Math.random(), function() {
				document.getElementById("console").innerHTML = data;
			})
		} else {
			document.getElementById("console").innerHTML = data;
		}
	});
}

function cartDelete() {
	if (confirm("Данный объект будет удален. Продолжить?"))
	$.get("/devin/exes/repair/" + (id.indexOf("off") < 0 ? "repair" : "writeoff") + "_delete_cart.asp?id=" + id + "&r=" + Math.random(), function() {
		$("#cart").fadeOut(150);
		if (document.getElementById(id)) {
			if (id.indexOf("off") < 0) {
				document.getElementById(id).parentNode.removeChild(document.getElementById(id));
			} else {
				$("#" + id).closest(".unit").remove();
			}	
		}
	});
}

function writeoffSetup() {
	$("#cart").load("/devin/views/writeoff_setup_cart.asp?r=" + Math.random()).fadeIn(150);
	$(".view .selected").removeClass("selected");
	id = "setup";
	setHash("setup");
}

function writeoffCreate() {
	$.get("/devin/exes/repair/writeoff_create_cart.asp?r=" + Math.random(), function(data) {
		if (data.indexOf("error") < 0) {
			id = data;
			cartOpenBack();
		}
	})
}

function writeoffExport() {
	$.ajax({
		url: "http://web.vst.vitebsk.energo.net/export/writeoff/print/" + id.replace("off", ""),
		crossDomain: true,
		xhrFields: {
			origin: "http://www.vst.vitebsk.energo.net",
			withCredentials: true
		}
	}).done(function (data) {
		$("#console").html(data);
	})

	//$("#console").load("http://web.vst.vitebsk.energo.net/devin/writeoff/print/" + id.replace("off", "") + "&r=" + Math.random());
}

function onSelectedRepairs() {
	$.post("/devin/exes/repair/repair_on_all_selected.asp?r=" + Math.random(), selectionToForm("repairs", ";;"), restore);
}

function offSelectedRepairs() {
	$.post("/devin/exes/repair/repair_off_all_selected.asp?r=" + Math.random(), selectionToForm("repairs", ";;"), restore);
}

function moveSelectedRepairs() {
	$.post("/devin/exes/repair/repair_move_all_selected.asp?r=" + Math.random(), selectionToForm("repairs", ";;") + "&key=" + document.getElementById("moveKey").value, function (data) {
		removeAllSelection();
		restore();
	});
}

function deleteSelectedRepairs() {
	$.post("/devin/exes/repair/repair_delete_all_selected.asp?r=" + Math.random(), selectionToForm("repairs", ";;"), restore);
}

function search(input) {
    if (input.value == "" && document.location.search.indexOf("text=") > -1) document.location = '/devin/repair/';
    var e = event || window.event;
    if (e.keyCode == 13) {
        document.location.search = "text=" + encodeURIComponent(input.value);
    }
}

// Составление списка доступных для перемещения в них данного списания групп. Аналогична такой же функции для групп, но другой коллбэк
function writeoffMove() {
	$.post("/devin/exes/repair/writeoff_move_group.asp?r=" + Math.random(), "id=" + menuId + "&in=" + $("#modal select:first-child").val(), restore);
	$("#modal").fadeOut(100);
}

// Открытие карточки списания из контекстного меню
function writeoffOpen() {
	cartOpen(document.getElementById(menuId).querySelector(".title"));
}

// Экспорт списания в Excel без открытия карточки
function writeoffPrint() {
	$.ajax({
		url: "http://web.vst.vitebsk.energo.net/export/writeoff/print/" + menuId.replace("off", ""),
		crossDomain: true,
		xhrFields: {
			origin: "http://www.vst.vitebsk.energo.net",
			withCredentials: true
		}
	}).done(function (data) {
		document.getElementById("excelExportsLink").innerHTML = data;
		$(".panel:not(#excelExports").fadeOut(100);
		$("#excelExports").fadeIn(100);
	})
	/* $.get("http://web.vst.vitebsk.energo.net/devin/writeoff/print/" + menuId.replace("off", ""), function(data) {
		document.getElementById("excelExportsLink").innerHTML = data;
		$(".panel:not(#excelExports").fadeOut(100);
		$("#excelExports").fadeIn(100);
	}); */
}

// Списывание всех ремонтов в списании
function offMenuRepairs() {
	$.post("/devin/exes/repair/repair_off_all.asp?r=" + Math.random(), "id=" + menuId.replace("off", ""), restore);
}

// Отмена списывания всех ремонтов в списании
function onMenuRepairs() {
	$.post("/devin/exes/repair/repair_on_all.asp?r=" + Math.random(), "id=" + menuId.replace("off", ""), restore);
}

// Удаление всех ремонтов в списании
function deleteMenuRepairs() {
	if (confirm("Все ремонты в выбранном списании будут отменены, использованные позиции будут возвращены на склад. Продолжить?"))
	$.post("/devin/exes/repair/repair_delete_all.asp?r=" + Math.random(), "id=" + menuId.replace("off", ""), restore);
}

// Удаление списания с сохранением ремонтов
function writeoffDelete() {
	if (confirm("Данное списание будет удалено (без удаления вложенных ремонтов). Продолжить?"))
	$.get("/devin/exes/repair/writeoff_delete_cart.asp?id=" + menuId + "&r=" + Math.random(), restore);
}