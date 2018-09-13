// Обслуживание текстовых полей ввода с классом def, предполагающих наличие в пустом поле поясняющего текста
// Поясняющий текст указывается через атрибут def для каждого из текстовых полей
$("input.def").each(function () {
    this.setAttribute("placeholder", this.getAttribute("def"));
    _blur(this);
});

$(document)
	.on("focus", "input.def", function() { _focus(this) })
	.on("blur", "input.def", function() { _blur(this) });

function _blur(input) {
	if (input.value == '') {
		input.value = input.getAttribute("def");
		$(input).addClass('unfocused');
	}
}

function _focus(input) {
	if (input.value == input.getAttribute("def")) {
		input.value = '';
		$(input).removeClass('unfocused');
	}
}


// Работа с cookie значениями
// Можно устанавливать и читать значения
// Для удаления значения необходимо выполнить установку значения, но указать при этом expires: -1
function getCookie(name) {
	var matches = document.cookie.match(new RegExp("(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"));
	return matches ? decodeURIComponent(matches[1]) : undefined;
}

function setCookie(name, value, options) {
	options = options || {};
	var expires = options.expires;
	if (typeof expires == "number" && expires) {
		var d = new Date();
		d.setTime(d.getTime() + expires * 1000);
		expires = options.expires = d;
	}
	if (expires && expires.toUTCString) options.expires = expires.toUTCString();
	value = encodeURIComponent(value);
	var updatedCookie = name + "=" + value;
	for (var propName in options) {
		updatedCookie += "; " + propName;
		var propValue = options[propName];
		if (propValue !== true) updatedCookie += "=" + propValue;
	}
	document.cookie = updatedCookie;
}


/** Поиск по строкам таблицы */
function _search(input) {
	var index = input.parentNode.cellIndex;
	var tbody = input.parentNode.parentNode.parentNode.parentNode.getElementsByTagName("tbody")[0];
	var trs = tbody.getElementsByTagName("tr");
	var val = input.value.toLowerCase();
	if (val == "") {
		for (var i = 0; i < trs.length; i++) trs[i].style.display = 'table-row';
	} else {
		for (var i = 0; i < trs.length; i++) {
			var text = trs[i].getElementsByTagName("td")[index].innerHTML.toLowerCase();
			trs[i].style.display = text.indexOf(val) > -1 ? 'table-row' : 'none';
		}
	}
}


/** Контекстные меню */
var when, menuId;

/** Контекстное меню по клику */
function _menu(obj) {
	$(".context-menu:visible").css("display", "none");
	clearTimeout(when);

	var menu = document.getElementById(obj.getAttribute("menu"));
    menu.onmouseleave = function () { when = setTimeout(function () { $(menu).fadeOut(100); }, 1000); };
    menu.onmouseenter = function () { clearTimeout(when); };
    menu.onclick = function () { $(menu).fadeOut(100); };
	menu.style.top = ($(obj).offset().top + 6) + "px";
	menu.style.left = ($(obj).offset().left + 2) + "px";
	$(menu).fadeIn(100);

	menuId = $(obj).closest(".unit").attr("id") ? $(obj).closest(".unit").attr("id") : $(obj).closest(".unit").find(".title-wrapper:first-child").attr("id");
}


/**
 * Диалоговые окна для взаимодействия с пользователем и выполнения несложных операций
 * Должно предусматривать:
 * выбор варианта/отмена
 * подтверждение/отмена
 * ввод данных/отмена
 * уведомление
 * */
function _modal(target, source, handler) {

	// Скрытие всех активных меню
	$(".context-menu:visible").css("display", "none");
	clearTimeout(when);

	var menu = document.getElementById("modal");
	menu.style.top = ($(target).offset().top + 6) + "px";
	menu.style.left = ($(target).offset().left + 2) + "px";
	menu.getElementsByTagName("li")[0].innerHTML = source;
	menu.getElementsByTagName("li")[1].onclick = handler;
	$(menu).css("display", "block");
}


/** Функционал групп */
function _group() {
	var _in;
	$("tr.item,div.group").each(function() {
		_in = this.getAttribute('in');
		if (_in.slice(2) != '0') {
			if ($(this).hasClass('item')) {
				$("#" + _in).children(".items_block").children("table").find("tbody").append(this);
			} else {
				$(this).insertBefore($("#" + _in).children(".items_block").children("table").first());
			}
		}
	});
	$(".hide_first").removeClass("hide_first");
}

var path = document.location.pathname.split("/"),
	url = path[path.length - 2];

function groupCreate() {
	var title = prompt("Название группы: ", "Новая группа (" + (new Date).toLocaleString() + ")");
	if (title != null) {
		$.post("/devin/exes/core/core_create_group.asp?r=" + Math.random(), "app=" + url + "&title=" + encodeURIComponent(title), restore);
	}
}

function groupCreateInner() {
	var title = prompt("Название группы: ", "Новая группа (" + (new Date).toLocaleString() + ")");
	if (title != null) {
		$.post("/devin/exes/core/core_create_group_inner.asp?r=" + Math.random(), "app=" + url + "&in=" + menuId.slice(2) + "&title=" + encodeURIComponent(title), restore);
	}
}

function groupBeforeMove() {
	var obj = $("#" + menuId).closest(".unit"); // Получаем ссылку на группу, относительн которой будет спозиционировано меню
	var exceptions = []; // Массив возможных вариантов для перемещени данной группы

	// Составляем список исключений
	exceptions.push(menuId); // Сама перемещаемая группа
	exceptions.push($(obj).parent().closest(".group").attr("id")); // Группа, в которой аходится перемещаемая (на 1 уровне вложенности)
	$(obj).find(".group").each(function() { exceptions.push(this.id) }); // Все группы, вложенные в перемещаемую

	// Составляем список доступных вариантов для перемещения
	var select = "<option value='0'>Расположить отдельно";
	$("div.group").each(function() {
		var exception = false;
		for (var i = 0; i < exceptions.length; i++)
			if (this.id == exceptions[i]) exception = true;
		if (!exception) {
			select += "<option value='" + this.id + "'>" + $(this).children(".caption").find("th").html();
		}
	});

	_modal(obj, "<select>" + select + "</select>", function() {
		if (menuId.indexOf("off") > -1)
			writeoffMove();
		else if (menuId.indexOf("-") > -1)
			computerMove();
		else
			groupMove();
	});
}

function groupMove() {
	$.post("/devin/exes/core/core_move_group.asp?r=" + Math.random(), "gid=" + menuId.slice(2) + "&in=" + $("#modal select:first-child").val().slice(2), restore);
	$("#modal").fadeOut(100);
}

function groupEdit() {
	var title = prompt("Название группы: ", $("#" + menuId).find("th").first().text());
	if (title != null) {
		$.post("/devin/exes/core/core_edit_group.asp?r=" + Math.random(), "gid=" + menuId.slice(2) + "&title=" + encodeURIComponent(title), function() {
			//restore();
			$("#" + menuId).find("th").first().html(title);
		});
	}
}

function groupErase() {
	$.post("/devin/exes/core/core_erase_group.asp?r=" + Math.random(), "app=" + url + "&gid=" + menuId.slice(2), restore);
}

function groupDelete() {
	$.post("/devin/exes/core/core_delete_group.asp?r=" + Math.random(), "app=" + url + "&gid=" + menuId.slice(2), restore);
}

var restore = function() {
	$.get("view.asp?r=" + Math.random(), function(data) {
		$("#view").html(data);
	});
}


/* Drag-n-drop функционал */
if (document.getElementById('cart')) {
    try {
        $("#cart")
            .draggable().draggable("disable");
        $("#cart")
            .on("mousedown", ".cart-header", function () { $("#cart").draggable("enable"); })
            .on("mouseup", ".cart-header", function () { $("#cart").draggable("disable"); });
    } catch (e) {}
}




/**
 * Сортировка в таблице по столбцу
 * Для его корректной работы нужно четко указать элементы thead и tbody, а так же прописать тип данных в столбце для каждого th в thead
 * Типы для сортировки:
 * number - приведение к числовому значению
 * date - приведение к дате, работает с форматами дд.мм.гггг
 * string - алфавитное сравнение
 * type - алфавитное сравнение по имени класса элемента div в ячейке
 * */
function _sort(th) {
	function to_date(s) {
		if (s.indexOf(" ") > -1) {
			var t = s.split(" ");
			var tdate = t[0].split(".");
			var ttime = t[1].split(":");
			return new Date(tdate[2], tdate[1], tdate[0], ttime[0], ttime[1], ttime[2]);
		} else {
			if (s.indexOf(".") > -1) {
				var t = s.split(".");
				return new Date(t[2], t[1], t[0]);
			} else {
				var t = s.split(":");
				return new Date(2000, 1, 1, t[0], t[1], t[2]);
			}
		}
	}

	var table = th.parentNode.parentNode.parentNode,
		tbody = table.getElementsByTagName("tbody")[0],
		rowsArray = [],
		type = th.getAttribute("data-type") || "string",
		way = th.getAttribute("data-way"),
		colNum = th.cellIndex,
		compare;
	for (var i = 0; i < tbody.rows.length; i++) rowsArray.push(tbody.rows[i]);
	switch (type) {
		case 'number':
			compare = function(rowA, rowB) {
				if (way == "up") { return rowA.cells[colNum].innerHTML - rowB.cells[colNum].innerHTML; } else { return rowB.cells[colNum].innerHTML - rowA.cells[colNum].innerHTML; };
			};
			break;
		case 'string':
			compare = function(rowA, rowB) {
				if (way == "up") { return rowA.cells[colNum].innerHTML > rowB.cells[colNum].innerHTML ? 1 : -1; } else { return rowB.cells[colNum].innerHTML > rowA.cells[colNum].innerHTML ? 1 : -1; }
			};
			break;
		case 'date':
			compare = function(rowA, rowB) {
				var a = +to_date(rowA.cells[colNum].innerHTML),
					b = +to_date(rowB.cells[colNum].innerHTML);
				if (way == "up") { return b > a ? 1 : -1; } else { return a > b ? 1 : -1; }
			};
			break;
		case "type":
			compare = function(rowA, rowB) {
				if (way == "up") { return rowA.cells[colNum].querySelector("div").className > rowB.cells[colNum].querySelector("div").className ? 1 : -1; } else { return rowB.cells[colNum].querySelector("div").className > rowA.cells[colNum].querySelector("div").className ? 1 : -1; }
			};
			break;
		case "none":
			return;
		case "unique":
			compare = function(rowA, rowB) { return unique(rowA, rowB, way, colNum) };
	}

	for (var i = 0, ths = th.parentNode.getElementsByTagName("th"), len = ths.length; i < len; i++) ths[i].className = "";
	if (way == "up") {
		th.setAttribute("data-way", "down");
		th.className = "sort_down";
	} else {
		th.setAttribute("data-way", "up");
		th.className = "sort_up";
	}
	rowsArray.sort(compare);
	table.removeChild(tbody);
	for (var i = 0; i < rowsArray.length; i++) tbody.appendChild(rowsArray[i]);
	table.appendChild(tbody);
}

/*
Реализация истории браузера с учетом ajax вьювов через location.hash
Функция cartOpenBack отдается на откуп конкретному приложению, для корректной обработки типа открываемого элемента
*/
var id = "",
	hashSet = true;

$(window)
	.on("load", function() { cartOpenByHash() })
	.on("hashchange", function() { if (hashSet) cartOpenByHash() });

function setHash(hash) {
	hashSet = false;
	try { document.location.hash = "##" + hash; } catch (e) {}
	setTimeout(function() { hashSet = true }, 100);
}

function cartOpen(node) {
	id = node.parentNode.id;
	cartOpenBack();
	setHash(id);
}

function cartOpenByHash() {
	var hash = this.location.hash;
	if (hash.indexOf("##") > -1 && hash != "##null") {
		id = hash.replace("##", "");
		try { cartOpenBack(); } catch (e) {}
	} else try { cartClose(); } catch (e) {}
}

function cartClose() {
	$("#cart").fadeOut(150, function() { document.getElementById("cart").innerHTML = ""; });
	$(".view .selected").removeClass("selected");
	id = "";
	setHash("null");
}


/**
 * Мульти-выбор табличных элементов (универсальный)
 * Все действия с выбранными элементами реализуются в div#selected и собственных обработчиках
 * Для обработки списка выбранных элементов используется глобальный массив selected и сериализующая его функция selectionToForm(имя, разделитель)
 * */
$(".view")
	.on("change", ".items input.selecter-all", function() { setAllSelection(this) })
	.on("change", ".items input.selecter", function() { setSelection(this) });

var selected = [];

function setSelection(node) {
	node.checked ? addSelection(node) : removeSelection(node);
	selectionPanel();
}

function addSelection(node) {
	selected.push(node.parentNode.parentNode.id);
	$(node).addClass("selection");
}

function removeSelection(node) {
	var selectedId = node.parentNode.parentNode.id;
	for (var i = 0; i < selected.length; i++) {
		if (selected[i] == selectedId) {
			selected.splice(i, 1);
			break;
		}
	}
	$(node).removeClass("selection");
}

function setAllSelection(node) {
	var $checks = $(node).closest(".items").find("input.selecter");
	if (node.checked) {
		$checks.each(function() {
			if (!this.checked) {
				this.checked = true;
				addSelection(this);
			}
		});
		$(node).addClass("selection");
	} else {
		$checks.each(function() {
			if (this.checked) {
				this.checked = false;
				removeSelection(this);
			}
		});
		$(node).removeClass("selection");
	}
	selectionPanel();
}

function removeAllSelection() {
	$("input.selection").each(function() {
		this.checked = false;
		$(this).removeClass("selection");
	})
	selected = [];
	selectionPanel();
}

function selectionPanel() {
	var $sel = $("#selected");
	if (selected.length > 0) {
		"none" == $sel.css("display") && $sel.slideDown(100);
		$sel.find("b").html(String(selected.length));
	} else {
		"none" != $sel.css("display") && $sel.slideUp(100);
	}
}

function selectionToForm(name, separate) {
	var text = name + "=";
	for (var i = 0; i < selected.length; i++) {
		text += selected[i] + separate;
	}
	return text;
}

function closeExportsPanel() {
	$('#excelExports').slideUp(100);
	document.getElementById("excelExportsLink").innerHTML = "";
}