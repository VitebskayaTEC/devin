let pageName = '';
if (document.location.pathname.includes('devices')) pageName = 'devices';
else if (document.location.pathname.includes('storages')) pageName = 'storages';
else if (document.location.pathname.includes('repairs')) pageName = 'repairs';
else if (document.location.pathname.includes('catalog')) pageName = 'catalog';
else if (document.location.pathname.includes('aida')) pageName = 'aida';

document.addEventListener('click', e => {
    let unit = e.target.closest('.caption');
    if (unit) if (unit.id !== 'solo') toggle(unit);
    if (e.target.closest('.items th')) _sort(e.target);
});

function toggle(node) {
    let unit = node.closest('.unit');
    let name = unit.id || unit.querySelector('.title-wrapper:first-child').id;
    let block = unit.querySelector('.itemsBlock');

    console.log(name);

    if (unit.classList.contains('open')) {
        unit.classList.remove('open');
        block.classList.remove('itemsBlock_expanded');
        setCookie(name, '', { expires: 9999999999 });
    } else {
        unit.classList.add('open');
        block.classList.add('itemsBlock_expanded');
        setCookie(name, 'open', { expires: 9999999999 });
    }
}

function getCookie(name) {
	let matches = document.cookie.match(new RegExp("(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"));
	return matches ? decodeURIComponent(matches[1]) : undefined;
}

function setCookie(name, value, options) {
	options = options || {};
	let expires = options.expires;
	if (typeof expires == "number" && expires) {
        let d = new Date();
		d.setTime(d.getTime() + expires * 1000);
		expires = options.expires = d;
	}
	if (expires && expires.toUTCString) options.expires = expires.toUTCString();
	value = encodeURIComponent(value);
    let updatedCookie = name + "=" + value;
    for (let propName in options) {
		updatedCookie += "; " + propName;
        let propValue = options[propName];
		if (propValue !== true) updatedCookie += "=" + propValue;
	}
	document.cookie = updatedCookie;
}


let when, menuId;

function _menu(obj) {
    document.querySelectorAll('.contextMenu_visible').forEach(el => el.classList.remove('contextMenu_visible'));
    clearTimeout(when);

    let menu = document.getElementById(obj.getAttribute('menu'));
    let rect = obj.getBoundingClientRect();
    let container = obj.closest('.unit');

    menu.onmouseleave = () => when = setTimeout(() => menu.classList.remove('contextMenu_visible'), 1000);
    menu.onmouseenter = () => clearTimeout(when);
    menu.onclick = () => menu.classList.remove('contextMenu_visible');
    menu.style.top = (rect.top + pageYOffset + 6) + 'px';
    menu.style.left = (rect.left + pageXOffset + 2) + 'px';
    menu.classList.add('contextMenu_visible');

    menuId = container.hasAttribute('id') ? container.id : container.querySelector('.title-wrapper:first-child').id;
}

function _modal(target, source, handler) {
    document.querySelectorAll('.contextMenu_visible').forEach(el => el.classList.remove('contextMenu_visible'));
	clearTimeout(when);

    let menu = document.getElementById("modal");
    let rect = target.getBoundingClientRect();

    menu.style.top = (rect.top + pageYOffset + 6) + "px";
    menu.style.left = (rect.left + pageXOffset + 2) + "px";
	menu.getElementsByTagName("li")[0].innerHTML = source;
    menu.getElementsByTagName("li")[1].onclick = handler;
    menu.classList.add('contextMenu_visible');
}

function restore() {
    fetch(host + pageName + '/list')
        .then(res => res.text())
        .then(text => {
            document.getElementById('view').innerHTML = text;
            let el = document.getElementById(id);
            if (el) el.classList.add('selected');
        });
};

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
            compare = (rowA, rowB) => (way == "up")
                ? rowA.cells[colNum].innerHTML - rowB.cells[colNum].innerHTML
                : rowB.cells[colNum].innerHTML - rowA.cells[colNum].innerHTML;
			break;
		case 'string':
            compare = (rowA, rowB) => (way == "up")
                ? rowA.cells[colNum].innerHTML > rowB.cells[colNum].innerHTML ? 1 : -1
                : rowB.cells[colNum].innerHTML > rowA.cells[colNum].innerHTML ? 1 : -1;
			break;
		case 'date':
			compare = (rowA, rowB) => {
                let a = +to_date(rowA.cells[colNum].innerHTML);
				let b = +to_date(rowB.cells[colNum].innerHTML);
                return (way == "up")
                    ? b > a ? 1 : -1
                    : a > b ? 1 : -1;
			};
			break;
		case "type":
            compare = (rowA, rowB) => (way == "up")
                ? rowA.cells[colNum].querySelector("div").className > rowB.cells[colNum].querySelector("div").className ? 1 : -1
                : rowB.cells[colNum].querySelector("div").className > rowA.cells[colNum].querySelector("div").className ? 1 : -1;
			break;
		case "unique":
            compare = (rowA, rowB) => unique(rowA, rowB, way, colNum);
            break;
        default: return;
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

let id = '', hashSet = true;

window.onload = () => cartOpenByHash();
window.onhashchange = () => hashSet && cartOpenByHash();

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
	let hash = document.location.hash;
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

document.addEventListener('change', e => {
    if (e.target.classList.contains('selecter-all')) setAllSelection(e.target);
    if (e.target.classList.contains('selecter')) setSelection(e.target);
});

var selected = [];

function setSelection(node) {
	node.checked ? addSelection(node) : removeSelection(node);
	selectionPanel();
}

function addSelection(node) {
    selected.push(node.parentNode.parentNode.id);
    node.classList.add('selection');
}

function removeSelection(node) {
    let selectedId = node.parentNode.parentNode.id;
	for (let i = 0; i < selected.length; i++) {
		if (selected[i] == selectedId) {
			selected.splice(i, 1);
			break;
		}
    }
    node.classList.remove('selection');
}

function setAllSelection(node) {
	let checks = node.closest('.items').querySelectorAll('.selecter');
    if (node.checked) {
        checks.forEach(el => {
            if (!el.checked) {
                el.checked = true;
                addSelection(el);
			}
		});
		node.classList.add('selection');
	} else {
        checks.forEach(el => {
            if (el.checked) {
                el.checked = false;
                removeSelection(el);
			}
		});
        node.classList.remove('selection');
	}
	selectionPanel();
}

function removeAllSelection() {
    document.querySelectorAll('.selection').forEach(el => {
        el.checked = false;
        el.classList.remove('selection');
    });
	selected = [];
	selectionPanel();
}

function selectionPanel() {
    let sel = document.getElementById('selected');
    if (selected.length > 0) {
        sel.classList.add('selection_visible');
        sel.querySelector('b').innerHTML = selected.length;
    }
    else {
        sel.classList.remove('selection_visible');
	}
}


/* Переход на другую модель организации JS кода */

var Folders = {

    fetch(url, data, callback) {
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

        Folders.fetch(host + 'folders/create', { Type: pageName, Name: name });
    },

    createInner() {
        let name = prompt("Название группы: ", "Новая группа (" + (new Date).toLocaleString() + ")");
        if (!name) return;
        
        Folders.fetch(host + 'folders/createInner', {
            Type: pageName,
            Name: name,
            FolderId: menuId.slice(2)
        });
    },

    beforeMove() {
        var obj = $("#" + menuId).closest(".unit");
        var exceptions = [];

        exceptions.push(menuId);
        exceptions.push($(obj).parent().closest(".group").attr("id"));
        $(obj).find(".group").each(function () { exceptions.push(this.id); });

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
		        Folders.move();
        });
    },

    move() {
        Folders.fetch(host + 'folders/move', {
            FolderId: document.getElementById('modal').querySelector('select').value.replace(/\D+/g, ''),
            Id: menuId.slice(2)
        });
        $("#modal").fadeOut(100);
    },

    update() {
	    let name = prompt("Название группы: ", $("#" + menuId).find("th").first().text());
        if (!name) return;
        
        Folders.fetch(host + 'folders/update', { Name: name, Id: menuId.slice(2) }, () => $("#" + menuId).find("th").first().html(title));
    },

    clear() {
        Folders.fetch(host + 'folders/clear', { Type: pageName, Id: menuId.slice(2) });
    },

    del() {
        Folders._fetch(host + 'folders/delete', { Type: pageName, Id: menuId.slice(2) });
    }
};

var Devices = {

    Cart: {

        create() {
            fetch(host + 'devices/create', { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        id = json.Id;
                        cartOpenBack();
                        restore();
                    }
                });
        },

        update(Id) {
            let form = new FormData();
            document.getElementById('form').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value));

            fetch(host + 'devices/update/' + Id, { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Error) message(json.Error);
                    if (json.Warning) message(json.Warning, 'warning');
                    if (json.Good) {
                        message(json.Good, 'good');
                        restore();
                        cartOpenBack();
                    }
                });
        },

        copy() {
            fetch(host + 'devices/copy/' + id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        id = json.Id;
                        cartOpenBack();
                        restore();
                    }
                });
        },

        del() {
            fetch(host + 'devices/delete/' + id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        cartClose();
                        restore();
                    }
                });
        },

        moveSelected() {
            let form = new FormData();
                form.append('Key', document.getElementById('moveKey').value);
                form.append('Devices', selected.join(';;'));
            fetch(host + 'devices/moveSelected', { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        restore();
                    }
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

            let form = new FormData();
            document.getElementById('defect-cart').querySelectorAll('input,textarea').forEach(el => form.append(el.name, el.value));

            let positions = '';
            document.getElementById('defect_container').querySelectorAll('tr').forEach(el => {
                let cells = el.getElementsByTagName('td');
                let type = cells[0].getAttribute('val');

                positions += (type !== 'unique' ? cells[0].getAttribute('val') : cells[0].innerHTML) + '::' + cells[1].innerHTML + ';;';
            });
            form.append('Positions', positions);

            fetch(host + 'devices/printDefectAct', { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Error) message(json.Error);
                    if (json.Good) {
                        message(json.Good, 'good');
                        let a = document.createElement('a');
                        document.body.appendChild(a);
                        a.href = json.Link;
                        a.click();
                    }
                });
        }
    },

    printReport() {
        fetch(host + 'devices/printRecordCart/' + menuId, { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good);
                    let a = document.createElement('a');
                    document.body.appendChild(a);
                    a.href = json.Link;
                    a.click();
                }
            });
    }
};

var Storages = {

    create() {
        fetch(host + 'storages/create', { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                    document.location.hash = '##' + json.Id;
                }
            });
    },

    update(Id) {
        let form = new FormData();
        document.getElementById('form').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value));
        fetch(host + 'storages/update/' + Id, { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Warning) message(json.Warning, 'warning');
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                    cartOpenBack();
                }
            });
    },

    del(Id) {
        if (!confirm("Данный объект будет удален. Продолжить?")) return;
        fetch(host + 'storages/delete/' + Id, { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Warning) message(json.Warning, 'warning');
                if (json.Good) {
                    message(json.Good, 'good');
                    cartClose()
                    restore();
                }
            });
    },

    compare() {
        document.querySelectorAll('.panel:not(#excl,#selected)').forEach(el = el.style.display = 'none');
        document.getElementById('excl').style.display = 'block';
    },

    move() {
        let form = new FormData();
        form.append('Select', selected.join(';;'));
        form.append('FolderId', document.getElementById('move_select').value);
        fetch(host + 'storages/move', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    removeAllSelection();
                    restore();
                }
            });
    },

    labels() {
        let form = new FormData();
        form.append('Select', selected.join(','));
        fetch(host + 'storages/labels', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    removeAllSelection();
                    let a = document.createElement('a');
                    document.body.appendChild(a);
                    a.href = json.Link;
                    a.click();
                }
            });
    },

    repair(Id) {
        let form = new FormData();
        form.append('Select', Id);
        Repairs.Storage.create(form);
    },

    repairsSelected() {
        let form = new FormData();
        form.append('Select', selected.join(','));
        removeAllSelection();
        Repairs.Storage.create(form);
    }
};

var Repairs = {

    Device: {

        interval: 0,
        searchQuery: '',

        search(s) {
            document.getElementById('repairsData').querySelectorAll('.repairs__row').forEach(el => {
                if (s === '') {
                    el.classList.remove('repairs__row_hided');
                }
                else {
                    let text = '';
                    el.querySelectorAll('td').forEach(cell => text += cell.innerHTML.toLowerCase());
                    if (text.indexOf(s) > -1) el.classList.remove('repairs__row_hided');
                    else el.classList.add('repairs__row_hided');
                }
            });
        },

        create() {
            fetch(host + 'repairs/createFromDevice/' + id)
                .then(res => res.text())
                .then(text => {
                    let cart = document.getElementById('cart');
                    cart.innerHTML = text;
                    cart.classList.add('cart_visible');
                    this.load();

                    this.interval = setInterval(() => {
                        let s = document.getElementById('repairsSearch').value.toLowerCase();
                        if (s !== this.searchQuery) {
                            this.searchQuery = s;
                            this.search(s);
                        }
                    }, 50);

                    document.getElementById('repairsForm').addEventListener('change', this.load);
                    document.getElementById('repairsData').addEventListener('change', e => {
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
                                input.parentNode.parentNode.classList.add('repairs__row_checked');
                            }
                            else {
                                input.parentNode.parentNode.classList.remove('repairs__row_checked');
                            }
                        }
                    });
                });
        },

        load() {
            let form = new FormData();
            form.append('Id', id);
            document.getElementById('repairsForm').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value));
            fetch(host + 'repairs/createFromDeviceData', { method: 'POST', body: form })
                .then(res => res.text())
                .then(text => {
                    document.getElementById('repairsData').innerHTML = text;
                    setTimeout(() => this.search(this.searchQuery), 100);
                });
        },

        end(withWriteoff) {

            let form = new FormData();
            form.append('Id', id);
            document.getElementById('repairsData').querySelectorAll('.repairs__row_checked').forEach(el => {
                let inventory = el.getAttribute('data-id');
                let number = el.querySelector('input[type="number"]').value;
                let virtual = el.querySelector('input[type="checkbox"]').checked;
                form.append('Repairs[]', inventory + ':' + number + ':' + virtual);
            });

            if (withWriteoff) {
                form.append('Writeoff', prompt('Введите наименование нового списания', 'Списание: ' + (new Date).toLocaleString()));
            }

            fetch(host + 'repairs/endCreateFromDevice', { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Error) message(json.Error);
                    if (json.Warning) message(json.Warning, 'warning');
                    if (json.Good) {
                        message(json.Good, 'good');
                        if (json.WriteoffId !== 0) {
                            message('<a href="' + host + 'repairs/##off' + json.WriteoffId + '">Перейти к созданному списанию</a>', 'good');
                        }
                        this.load();
                    }
                });
        },

        back() {
            clearInterval(this.interval);
            cartOpenBack();
        }
    },

    Storage: {
        create(form) {
            fetch(host + 'repairs/createFromStorages', { method: 'POST', body: form })
                .then(res => res.text())
                .then(text => {
                    let cart = document.getElementById('cart');
                    cart.innerHTML = text;
                    cart.classList.add('cart_visible');

                    document.getElementById('repairsData').addEventListener('change', e => {
                        let input = e.target;
                        if (input.type === 'number') {
                            if (input.value > 0) {
                                input.parentNode.parentNode.classList.add('repairs__row_checked');
                            }
                            else {
                                input.parentNode.parentNode.classList.remove('repairs__row_checked');
                            }
                        }
                    });
                });
        },

        add(button) {
            var row = button.parentNode.parentNode;
            var copyRow = row.cloneNode(true);
            row.parentNode.insertBefore(copyRow, row);
        },

        remove(button) {
            var row = button.parentNode.parentNode;
            row.parentNode.removeChild(row);
            if (document.getElementById("repairsData").querySelectorAll("tr").length == 0) cartClose();
        },

        end(withWriteoff) {
            let form = new FormData();
            form.append('Id', id);
            let rows = document.getElementById('repairsData').querySelectorAll('.repairs__row_checked');

            rows.forEach(el => {
                let inventory = el.getAttribute('data-id');
                let device = el.querySelector('select').value;
                let number = el.querySelector('input[type="number"]').value;
                let virtual = el.querySelector('input[type="checkbox"]').checked;
                form.append('Repairs[]', inventory + ':' + device + ':' + number + ':' + virtual);
            });

            if (withWriteoff) {
                form.append('Writeoff', prompt('Введите наименование нового списания', 'Списание: ' + (new Date).toLocaleString()));
            }

            fetch(host + 'repairs/endCreateFromStorages', { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Error) message(json.Error);
                    if (json.Warning) message(json.Warning, 'warning');
                    if (json.Good) {
                        message(json.Good, 'good');
                        if (json.WriteoffId !== 0) {
                            message('<a href="' + host + 'repairs/##off' + json.WriteoffId + '">Перейти к созданному списанию</a>', 'good');
                        }
                        rows.forEach(el => el.parentNode.removeChild(el));
                    }
                });
        }

    },

    Cart: {

        update(Id) {
            let form = new FormData();
            document.getElementById('form').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value))

            fetch(host + 'repairs/update/' + Id, { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        restore();
                        cartOpenBack();
                    }
                    if (json.Warning) message(json.Warning, 'warning');
                    if (json.Error) message(json.Error);
                });
        },

        del(Id) {
            if (!confirm("Данный объект будет удален. Продолжить?")) return;

            fetch(host + 'repairs/delete/' + Id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        cartClose()
                        restore();
                    }
                });
        },
    },

    off() {
        fetch(host + 'repairs/off/' + menuId.replace("off", ""), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
    },

    on() {
        fetch(host + 'repairs/on/' + menuId.replace("off", ""), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
    },

    clear() {
        if (!confirm("Все ремонты в выбранном списании будут отменены, использованные позиции будут возвращены на склад. Продолжить?")) return;
        fetch(host + 'repairs/deleteAll/' + menuId.replace('off', ''), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
    },

    move() {
        let form = new FormData();
        form.append('Key', document.getElementById('moveKey').value);
        form.append('Repairs', selected.join(';;'));
        fetch(host + 'repairs/move', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Good) {
                    message(json.Good, 'good');
                    removeAllSelection();
                    restore();
                }
            });
    },

    offSelected() {
        let form = new FormData();
        form.append('Repairs', selected.join(';;'));
        fetch(host + 'repairs/offSelected', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Good) {
                    message(json.Good, 'good');
                    removeAllSelection();
                    restore();
                }
            });
    },

    onSelected() {
        let form = new FormData();
        form.append('Repairs', selected.join(';;'));
        fetch(host + 'repairs/onSelected', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Good) {
                    message(json.Good, 'good');
                    removeAllSelection();
                    restore();
                }
            });
    },

    delSelected() {
        let form = new FormData();
        form.append('Repairs', selected.join(';;'));
        fetch(host + 'repairs/deleteSelected', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
    }
};

var Writeoffs = {

    cart() {
        document.location.hash = '##' + menuId;
    },

    create() {
        fetch(host + 'writeoffs/create', { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                    document.location.hash = '##' + json.Id;
                }
            });
    },

    update(Id) {
        let form = new FormData();
        document.getElementById('form').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value))

        fetch(host + 'writeoffs/update/' + Id, { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                    cartOpenBack();
                }
                if (json.Warning) message(json.Warning, 'warning');
                if (json.Error) message(json.Error);
            });
    },

    export(Id) {
        fetch(host + 'writeoffs/print/' + (Id || menuId.replace("off", "")), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Good) {
                    message(json.Good, 'good');
                    let a = document.createElement('a');
                    document.body.appendChild(a);
                    a.href = json.Link;
                    a.click();
                }
            });
    },

    del(Id) {
        if (!confirm("Данный объект будет удален. Продолжить?")) return;

        fetch(host + 'writeoffs/delete/' + (Id || menuId.replace('off', '')), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    cartClose()
                    restore();
                }
            });
    },

    move() {
        fetch(host + 'writeoff/move/' + menuId + '?FolderId=' + $("#modal select:first-child").val(), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
        $("#modal").fadeOut(100);
    }
};

var Catalog = {

    create(type) {
        fetch(host + 'catalog/create' + type, { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    document.location.hash = '##' + json.Id;
                }
            });
    },

    update() {
        let form = new FormData();
        document.getElementById('form').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value));
        form.append('Id', id.replace(/cart|prn/, ''));

        fetch(host + 'catalog/update' + (id.includes('prn') ? 'printer' : 'cartridge'), { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) message(json.Good, 'good');
                if (json.Warning) message(json.Warning, 'warning');
                if (json.Error) message(json.Error);
            });
    },

    del() {
        if (!confirm("Данный объект будет удален. Продолжить?")) return;
        fetch(host + 'catalog/delete' + (id.includes('prn') ? 'printer' : 'cartridge') + '/' + id.replace(/cart|prn/, ''), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    cartClose()
                }
            });
    },

    add(obj) {
        let form = new FormData();
        form.append('PrinterId', obj.PrinterId);
        form.append('CartridgeId', obj.CartridgeId);
        fetch(host + 'catalog/createCompare', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    cartOpenBack();
                }
            });
    },

    remove(obj) {
        let form = new FormData();
        form.append('PrinterId', obj.PrinterId);
        form.append('CartridgeId', obj.CartridgeId);
        fetch(host + 'catalog/deleteCompare', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    cartOpenBack();
                }
            });
    },
};

var Aida = {

    del() {
        if (!confirm("Отчет по данному компьютеру будет безвозвратно удален. Продолжить?")) return;
        fetch(host + 'aida/delete/' + id, { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    cartClose();
                    restore();
                }
            });
    }
};

document.addEventListener('click', e => {
    document.querySelectorAll('.tabsSelector').forEach(el => {
        if (el.contains(e.target)) {
            let currentSelector = e.target.closest('.tabsSelector__item');
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

(function (ELEMENT) {
    ELEMENT.matches = ELEMENT.matches || ELEMENT.mozMatchesSelector || ELEMENT.msMatchesSelector || ELEMENT.oMatchesSelector || ELEMENT.webkitMatchesSelector;
    ELEMENT.closest = ELEMENT.closest || function closest(selector) {
        if (!this) return null;
        if (this.matches(selector)) return this;
        if (!this.parentElement) { return null }
        else return this.parentElement.closest(selector)
    };
}(Element.prototype));