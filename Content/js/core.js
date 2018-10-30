// Обслуживание текстовых полей ввода с классом def, предполагающих наличие в пустом поле поясняющего текста
// Поясняющий текст указывается через атрибут def для каждого из текстовых полей
$("input.def").each(function () {
    this.setAttribute("placeholder", this.getAttribute("def"));
    _blur(this);
});

$(document)
    .on("focus", "input.def", function () { _focus(this); })
    .on("blur", "input.def", function () { _blur(this); });

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
function _search(input, allTable) {

    if (allTable) {
        var val = input.value.toLowerCase();
        var tbody = input.parentNode.parentNode.parentNode.parentNode.getElementsByTagName("tbody")[0];
        var rows = tbody.getElementsByTagName("tr");
        if (val == "") {
            for (var i = 0; i < rows.length; i++) rows[i].style.display = 'table-row';
        } else {
            for (var i = 0; i < rows.length; i++) {
                var cells = rows[i].getElementsByTagName('td');
                var found = false;
                for (var j = 0; j < cells.length; j++) {
                    if (!found && cells[j].innerHTML.toLowerCase().indexOf(val) > -1) found = true;
                }
                rows[i].style.display = found ? 'table-row' : 'none';
            }
        }

    } else {
        var index = input.parentNode.cellIndex;
        var tbody = input.parentNode.parentNode.parentNode.parentNode.getElementsByTagName("tbody")[0];
        var rows = tbody.getElementsByTagName("tr");
        var val = input.value.toLowerCase();
        if (val == "") {
            for (var i = 0; i < rows.length; i++) rows[i].style.display = 'table-row';
        } else {
            for (var i = 0; i < rows.length; i++) {
                var text = trs[i].getElementsByTagName("td")[index].innerHTML.toLowerCase();
                rows[i].style.display = text.indexOf(val) > -1 ? 'table-row' : 'none';
            }
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
let pageName = '';
if (document.location.pathname.indexOf('devices') > -1) pageName = 'devices';
else if (document.location.pathname.indexOf('storages') > -1) pageName = 'storages';
else if (document.location.pathname.indexOf('repairs') > -1) pageName = 'repairs';
else if (document.location.pathname.indexOf('catalog') > -1) pageName = 'catalog';
else if (document.location.pathname.indexOf('aida') > -1) pageName = 'aida';



function restore() {
    fetch(host + pageName + '/list')
        .then(res => res.text())
        .then(text => {
            document.getElementById('view').innerHTML = text;
            let el = document.getElementById(id);
            if (el) el.classList.add('selected');
        });
};


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
			compare = (rowA, rowB) => (way == "up") ? rowA.cells[colNum].innerHTML - rowB.cells[colNum].innerHTML : rowB.cells[colNum].innerHTML - rowA.cells[colNum].innerHTML;
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
            compare = function (rowA, rowB) { return unique(rowA, rowB, way, colNum); };
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
    .on("load", function () { cartOpenByHash(); })
    .on("hashchange", function () { if (hashSet) cartOpenByHash(); });

function setHash(hash) {
	hashSet = false;
	try { document.location.hash = "##" + hash; } catch (e) {}
	setTimeout(() => hashSet = true, 100);
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
    document.getElementById('cart').classList.remove('cart_visible');
    document.getElementById('view').querySelectorAll('.selected').forEach(el => el.classList.remove('selected'));
	id = '';
	setHash(null);
}


/**
 * Мульти-выбор табличных элементов (универсальный)
 * Все действия с выбранными элементами реализуются в div#selected и собственных обработчиках
 * Для обработки списка выбранных элементов используется глобальный массив selected и сериализующая его функция selectionToForm(имя, разделитель)
 * */
$(".view")
    .on("change", ".items input.selecter-all", function () { setAllSelection(this); })
    .on("change", ".items input.selecter", function () { setSelection(this); });

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
    $("input.selection").each(function () {
        this.checked = false;
        $(this).removeClass("selection");
    });
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




/* Переход на другую модель организации JS кода */


// Табы
document.addEventListener('click', ev => {
    document.querySelectorAll('.tabsSelector').forEach(el => {
        if (el.contains(ev.target)) {
            let currentSelector = ev.target.closest('.tabsSelector__item');
            let tabName = currentSelector.getAttribute('data-tab');
            let currentContainer = document.querySelector('.tabsContainer__item[data-tab="' + tabName + '"]');

            let changeContainer = () => {
                document.querySelector('.tabsSelector__item_selected').classList.remove('tabsSelector__item_selected');
                document.querySelector('.tabsContainer__item_selected').classList.remove('tabsContainer__item_selected');
                currentSelector.classList.add('tabsSelector__item_selected');
                currentContainer.classList.add('tabsContainer__item_selected');
            };

            if (currentContainer) {
                if (currentContainer.hasAttribute('data-tab-lazy')) {
                    fetch(currentContainer.getAttribute('data-tab-lazy'))
                        .then(res => res.text())
                        .then(text => {
                            currentContainer.innerHTML = text;
                            changeContainer();
                        });
                }
                else {
                    changeContainer();
                }
            }
        }
    });
});



var Folder = {

    _fetch(url, data, callback) {
        data = data || {};
        let form = new FormData();
        Object.keys(data).forEach(key => form.append(key, data[key]));
        fetch(url, { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                    if (callback) callback(json);
                }
            });
    },

    create() {
        let name = prompt("Название группы: ", "Новая группа (" + (new Date).toLocaleString() + ")");
        if (!name) return;

        Folder._fetch(host + 'folders/create', { Type: pageName, Name: name });
    },

    createInner() {
        let name = prompt("Название группы: ", "Новая группа (" + (new Date).toLocaleString() + ")");
        if (!name) return;
        
        Folder._fetch(host + 'folders/createInner', {
            Type: pageName,
            Name: name,
            FolderId: menuId.slice(2)
        });
    },

    beforeMove() {
        var obj = $("#" + menuId).closest(".unit"); // Получаем ссылку на группу, относительн которой будет спозиционировано меню
        var exceptions = []; // Массив возможных вариантов для перемещени данной группы

        // Составляем список исключений
        exceptions.push(menuId); // Сама перемещаемая группа
        exceptions.push($(obj).parent().closest(".group").attr("id")); // Группа, в которой аходится перемещаемая (на 1 уровне вложенности)
        $(obj).find(".group").each(function () { exceptions.push(this.id); }); // Все группы, вложенные в перемещаемую

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
		        Folder.move();
        });
    },

    move() {
        Folder._fetch(host + 'folders/move', {
            FolderId: document.getElementById('modal').querySelector('select').value.replace(/\D+/g, ''),
            Id: menuId.slice(2)
        });
        $("#modal").fadeOut(100);
    },

    update() {
	    let name = prompt("Название группы: ", $("#" + menuId).find("th").first().text());
        if (!name) return;
        
        Folder._fetch(host + 'folders/update', { Name: name, Id: menuId.slice(2) }, () => $("#" + menuId).find("th").first().html(title));
    },

    clear() {
        Folder._fetch(host + 'folders/clear', { Type: pageName, Id: menuId.slice(2) });
    },

    del() {
        Folder._fetch(host + 'folders/delete', { Type: pageName, Id: menuId.slice(2) });
    }
};

var Devices = {

    Cart: {

        create() {
            fetch(host + 'devices/create', { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    id = json.Id;
                    cartOpenBack();
                    restore();
                    if (json.Good) message(json.Good, 'good');
                });
        },

        update(Id) {
            let form = new FormData();

            document.getElementById('form').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value));

            fetch(host + 'devices/update/' + Id, { method: 'POST', body: form })
                .then(res => res.text())
                .then(text => {
                    text.indexOf('error:') > -1 ? message(text.replace('error:', '')) : message(text, 'good');
                    restore();
                })
                .catch(e => message(`'[${e.status}] ${e.statusText}<br />${e.responseText}`));
        },

        copy() {
            fetch(host + 'devices/copy/' + id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    id = json.Id;
                    cartOpenBack();
                    restore();
                    if (json.Good) message(json.Good, 'good');
                });
        },

        del() {
            fetch(host + 'devices/delete/' + id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    restore();
                    if (json.Good) {
                        message(json.Good, 'good');
                        cartClose();
                    }
                });
        },

        moveSelected() {
            let form = new FormData();
                form.append('Key', document.getElementById('moveKey').value);
                form.append('Devices', selectionToForm('devices', ';;'));
            fetch(host + 'devices/moveSelected', { method: 'POST', body: form })
                .then(() => {
                    removeAllSelection();
                    restore();
                });
        },

        selectAddon(url) {
            fetch(url)
                .then(res => {
                    if (res.ok) {
                        res.text().then(text => {
                            document.querySelector('.addons').classList.add('hide');
                            let body = document.querySelector('.addons__body');
                            body.innerHTML = text;
                            body.classList.remove('hide');
                        });
                    } else {
                        message(res.status + ' (' + res.statusText + ')')
                    }
                });
        },

        backToAddons() {
            document.querySelector('.addons').classList.remove('hide');
            document.querySelector('.addons__body').classList.add('hide');
        }
    },

    DefectAct: {

        add() {
            let count = document.getElementById('defect_count').value;
            if (isNaN(+count) || count === '') return alert('Количество деталей должно быть числом');

            let select = document.getElementById('defect_position');
            let type = select.value;
            let text = type === 'unique' ? document.getElementById('unique_name').value : select.options[select.selectedIndex].innerHTML;
            if (type === 'unique' && String(document.getElementById('unique_name').value) === '') return alert("Не введено название неисправности");

            document.getElementById('defect_container').insertAdjacentHTML('beforeend', `
            <tr>
                <td val="${type}">${text}</td>
                <td>${Math.round(count)}</td>
                <td><button onclick="Devices.DefectAct.del(this)">Удалить</button></td>
            </tr>`);
        },

        del(button) {
            let row = button.parentNode.parentNode;
            row.parentNode.removeChild(row);
        },

        check(select) {
            document.getElementById('defect_unique').innerHTML = select.value === 'unique'
                ? `Свое: <input type="text" id="unique_name" />`
                : '';
        },

        print() {

            let data = new FormData();

            document.getElementById('defect-cart').querySelectorAll('input,textarea').forEach(el => data.append(el.name, el.value));

            let positions = '';
            document.getElementById('defect_container').querySelectorAll('tr').forEach(el => {
                let cells = el.getElementsByTagName('td');
                let type = cells[0].getAttribute('val');

                positions += (type !== 'unique' ? cells[0].getAttribute('val') : cells[0].innerHTML) + '::' + cells[1].innerHTML + ';;';
            });
            data.append('positions', positions);

            let link = document.getElementById('defect_link');
            link.innerHTML = '<i>... идет печать ...</i>';

            fetch(host + 'devices/printDefectAct', {
                method: 'POST',
                body: data
            })
                .then(res => res.text())
                .then(text => link.innerHTML = text)
                .catch(() => message('Произошла ошибка'));
        }
    }
};


var Repairs = {

    Device: {

        create() {
            fetch(host + 'repairs/createFromDevice/' + id)
                .then(res => res.text())
                .then(text => {
                    let cart = document.getElementById('cart');
                    cart.innerHTML = text;
                    cart.classList.add('cart_visible');
                    Repairs.Device.load();

                    document.getElementById('repairForm').addEventListener('change', Repairs.Device.load);
                    document.getElementById('repairData').addEventListener('change', e => {
                        let input = e.target;
                        if (input.type === 'checkbox') {
                            let number = input.parentNode.parentNode.querySelector('input[type="number"]');
                            if (input.checked) {
                                number.setAttribute('data-max', number.getAttribute('max'));
                                number.removeAttribute('max');
                            } else {
                                number.setAttribute('max', number.getAttribute('data-max'));
                                number.removeAttribute('data-max');
                                if (number.value > +number.getAttribute('max')) number.value = +number.getAttribute('max');
                            }
                        }
                        else if (input.type === 'number') {
                            if (input.value > 0) {
                                input.parentNode.parentNode.classList.add('repair__row_checked');
                            }
                            else {
                                input.parentNode.parentNode.classList.remove('repair__row_checked');
                            }
                        }
                    });
                });
        },

        load() {
            let form = new FormData();
            form.append('Id', id);
            document.getElementById('repairForm').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value));
            fetch(host + 'repairs/createFromDeviceData', { method: 'POST', body: form })
                .then(res => res.text())
                .then(text => document.getElementById('repairData').innerHTML = text);
        },

        end(withWriteoff) {

            let form = new FormData();
            document.getElementById('repairData').querySelectorAll('.repair__row_checked').forEach(el => {
                let inventory = el.getAttribute('data-inventory');
                let number = el.querySelector('input[type="number"]').value;
                let virtual = el.querySelector('input[type="checkbox"]').checked ? "true" : "false";
                form.append(inventory, number + ":" + virtual);
            });

            if (withWriteoff) {
                form.append('writeoff', prompt('Введите наименование нового списания', 'Списание: ' + (new Date).toLocaleString()));
            }

            fetch(host + 'repairs/endCreateFromDevice', { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) message(json.Good, 'good');
                    if (json.Error) message(json.Error);
                    if (json.Warning) message(json.Warning, 'warning');
                });
        },

        back() {
            cartOpenBack();
        }
    },
}

document.querySelector('.messages').addEventListener('click', e => {
    console.log(e.target);
    if (e.target.classList.contains('messages__item')) {
        document.querySelector('.messages').removeChild(e.target);
    }
});

function message(text, type) {
    type = type || 'error';
    let div = document.createElement('div');
    div.innerHTML = text;
    div.classList.add('messages__item');
    div.classList.add('messages__item_' + type);
    document.querySelector('.messages').appendChild(div);
    setTimeout(() => { try { document.querySelector('.messages').removeChild(div); } catch (e) { } }, 5000);
}