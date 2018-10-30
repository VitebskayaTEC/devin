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

function cartDelete() {
    if (!confirm("Данный объект будет удален. Продолжить?")) return;

    fetch(host + (id.indexOf('off') < 0 ? 'repairs' : 'writeoffs') + '/delete/' + id, { method: 'POST' })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                document.getElementById('cart').classList.remove('show');
                restore();
            }
        });
}

function writeoffCreate() {
    fetch(host + 'writeoffs/create', { method: 'POST' })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                id = json.Id;
                restore();
                cartOpenBack();
            }
        });
}


function deleteSelectedRepairs() {
    fetch(host + 'repairs/deleteSelected', { method: 'POST', body: selectionToForm("repairs", ";;") })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
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
    fetch(host + 'writeoff/move/' + menuId + '?FolderId=' + $("#modal select:first-child").val())
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
	$("#modal").fadeOut(100);
}

// Открытие карточки списания из контекстного меню
function writeoffOpen() {
	cartOpen(document.getElementById(menuId).querySelector(".title"));
}

// Удаление всех ремонтов в списании
function deleteMenuRepairs() {
    if (!confirm("Все ремонты в выбранном списании будут отменены, использованные позиции будут возвращены на склад. Продолжить?")) return;
    fetch(host + 'repairs/deleteAll/' + menuId.replace('off', ''), { method: 'POST' })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
}


function writeoffDelete() {
    if (!confirm("Данное списание будет удалено (без удаления вложенных ремонтов). Продолжить?")) return;
    fetch(host + 'writeoffs/delete/' + id, { method: 'POST' })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
}

function cartOpenBack() {
    fetch(host + (id.indexOf("off") < 0 ? "repairs" : "writeoffs") + '/cart/' + id.replace('off', ''))
        .then(res => res.text())
        .then(text => {
            let cart = document.getElementById('cart');
            cart.classList.add('show');
            cart.innerHTML = text;
            document.getElementById('view').querySelectorAll('.selected').forEach(el => el.classList.remove('selected'));
            document.getElementById(id).classList.add('selected');
        });
}


function cartSave() {

    let form = new FormData();
    document.getElementById('form').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value))

    fetch(host + (id.indexOf("off") < 0 ? "repairs" : "writeoffs") + "/update/" + id, { method: 'POST', body: form })
    then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
                cartOpenBack();
            }
            if (json.Warning) message(json.Warning, 'warning');
            if (json.Error) message(json.Error);
        });
}

function writeoffPrint() {
    $.get(host + 'writeoffs/print/' + menuId.replace("off", "")).done(data => {
        document.getElementById("excelExportsLink").innerHTML = data;
        $(".panel:not(#excelExports").fadeOut(100);
        $("#excelExports").fadeIn(100);
    });
}

function writeoffExport() {
    $.get(host + 'writeoffs/print/' + id.replace("off", "")).done(data => $("#console").html(data));
}

function moveSelectedRepairs() {
    $.post(host + 'repairs/move', selectionToForm('repairs', ';;') + '&key=' + document.getElementById('moveKey').value, function (data) {
        removeAllSelection();
        restore();
    });
}

function offMenuRepairs() {
    fetch(host + 'repairs/off/' + menuId.replace("off", ""), { method: 'POST' })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
}

function onMenuRepairs() {
    fetch(host + 'repairs/on/' + menuId.replace("off", ""), { method: 'POST' })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
}

function offSelectedRepairs() {
    fetch(host + 'repairs/offSelected', { method: 'POST', body: selectionToForm("repairs", ";;") })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
}

function onSelectedRepairs() {
    fetch(host + 'repairs/onSelected', { method: 'POST', body: selectionToForm("repairs", ";;") })
        .then(res => res.json())
        .then(json => {
            if (json.Good) {
                message(json.Good, 'good');
                restore();
            }
        });
}