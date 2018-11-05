let pageName = '';
if (document.location.pathname.includes('devices')) pageName = 'devices';
else if (document.location.pathname.includes('storages')) pageName = 'storages';
else if (document.location.pathname.includes('repairs')) pageName = 'repairs';
else if (document.location.pathname.includes('catalog')) pageName = 'catalog';
else if (document.location.pathname.includes('aida')) pageName = 'aida';

document.addEventListener('click', e => {
    let unit = e.target.closest('.caption');
    if (unit && (e.target.tagName !== 'TD' || e.target.tagName !== 'B') && unit.parentNode.id !== 'solo') toggle(unit);
    if (e.target.tagName === 'TH' && e.target.parentNode.parentNode.parentNode.classList.contains('items')) sortTable(e.target);
});

function toggle(node) {
    let unit = node.closest('.unit');
    let name = unit.id || unit.getAttribute('data-id');
    if (!name) return;
    let block = unit.querySelector('.itemsBlock');

    if (unit.classList.contains('open')) {
        unit.classList.remove('open');
        block.classList.remove('itemsBlock_expanded');
        setCookie(name, '', { expires: 9999999999 });
    }
    else {
        if (!block.querySelector('div')) {
            fetch(host + pageName + '/load?Item=' + name)
                .then(res => res.text())
                .then(text => {
                    block.innerHTML = text;
                    unit.classList.add('open');
                    block.classList.add('itemsBlock_expanded');
                    setCookie(name, 'open', { expires: 9999999999 });
                })
        }
        else {
            unit.classList.add('open');
            block.classList.add('itemsBlock_expanded');
            setCookie(name, 'open', { expires: 9999999999 });
        }
    }
}

function sortTable(th) {
    function to_date(s) {
        if (s.indexOf(' ') > -1) {
            let t = s.split(' ');
            let tdate = t[0].split('.');
            let ttime = t[1].split(':');
            return new Date(tdate[2], tdate[1], tdate[0], ttime[0], ttime[1], ttime[2]);
        } else {
            if (s.indexOf('.') > -1) {
                let t = s.split('.');
                return new Date(t[2], t[1], t[0]);
            } else {
                let t = s.split(':');
                return new Date(2000, 1, 1, t[0], t[1], t[2]);
            }
        }
    }

    var table = th.parentNode.parentNode.parentNode,
        tbody = table.getElementsByTagName('tbody')[0],
        rowsArray = [],
        type = th.getAttribute('data-type') || 'string',
        way = th.getAttribute('data-way'),
        colNum = th.cellIndex,
        compare;
    for (let i = 0; i < tbody.rows.length; i++) rowsArray.push(tbody.rows[i]);
    switch (type) {
        case 'number':
            compare = (rowA, rowB) => (way == 'up')
                ? rowA.cells[colNum].innerHTML - rowB.cells[colNum].innerHTML
                : rowB.cells[colNum].innerHTML - rowA.cells[colNum].innerHTML;
            break;
        case 'string':
            compare = (rowA, rowB) => (way == 'up')
                ? rowA.cells[colNum].innerHTML > rowB.cells[colNum].innerHTML ? 1 : -1
                : rowB.cells[colNum].innerHTML > rowA.cells[colNum].innerHTML ? 1 : -1;
            break;
        case 'date':
            compare = (rowA, rowB) => {
                let a = +to_date(rowA.cells[colNum].innerHTML);
                let b = +to_date(rowB.cells[colNum].innerHTML);
                return (way == 'up')
                    ? b > a ? 1 : -1
                    : a > b ? 1 : -1;
            };
            break;
        case 'type':
            compare = (rowA, rowB) => (way == 'up')
                ? rowA.cells[colNum].querySelector('div').className > rowB.cells[colNum].querySelector('div').className ? 1 : -1
                : rowB.cells[colNum].querySelector('div').className > rowA.cells[colNum].querySelector('div').className ? 1 : -1;
            break;
        case 'unique':
            compare = (rowA, rowB) => unique(rowA, rowB, way, colNum);
            break;
        default: return;
    }

    for (let i = 0, ths = th.parentNode.getElementsByTagName('th'), len = ths.length; i < len; i++) ths[i].className = '';
    if (way == 'up') {
        th.setAttribute('data-way', 'down');
        th.className = 'sort_down';
    } else {
        th.setAttribute('data-way', 'up');
        th.className = 'sort_up';
    }
    rowsArray.sort(compare);
    table.removeChild(tbody);
    for (let i = 0; i < rowsArray.length; i++) tbody.appendChild(rowsArray[i]);
    table.appendChild(tbody);
}

function getCookie(name) {
	let matches = document.cookie.match(new RegExp('(?:^|; )' + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)'));
	return matches ? decodeURIComponent(matches[1]) : undefined;
}

function setCookie(name, value, options) {
	options = options || {};
	let expires = options.expires;
	if (typeof expires == 'number' && expires) {
        let d = new Date();
		d.setTime(d.getTime() + expires * 1000);
		expires = options.expires = d;
	}
	if (expires && expires.toUTCString) options.expires = expires.toUTCString();
	value = encodeURIComponent(value);
    let updatedCookie = name + '=' + value;
    for (let propName in options) {
		updatedCookie += '; ' + propName;
        let propValue = options[propName];
		if (propValue !== true) updatedCookie += '=' + propValue;
	}
	document.cookie = updatedCookie;
}


let when, menuId;

function _menu(obj) {
    document.querySelectorAll('.contextMenu_visible').forEach(el => el.classList.remove('contextMenu_visible'));
    clearTimeout(when);

    let menu = document.getElementById(obj.getAttribute('menu'));
    menu.onmouseleave = () => when = setTimeout(() => menu.classList.remove('contextMenu_visible'), 1000);
    menu.onmouseenter = () => clearTimeout(when);
    menu.onclick = () => menu.classList.remove('contextMenu_visible');

    let rect = obj.getBoundingClientRect();
    menu.style.top = (rect.top + pageYOffset + 6) + 'px';
    menu.style.left = (rect.left + pageXOffset + 2) + 'px';
    menu.classList.add('contextMenu_visible');

    let container = obj.closest('.unit');
    if (container) menuId = container.id || container.getAttribute('data-id');
}

function restore() {
    fetch(host + pageName + '/load?search=' + search)
        .then(res => res.text())
        .then(text => {
            document.getElementById('view').innerHTML = text;
            let el = document.getElementById(Cart.id);
            if (el) el.classList.add('selected');
        });
};

window.onload = () => Cart.byHash();
window.onhashchange = () => Cart.hashSet && Cart.byHash();

var Cart = {

    id: '',
    hashSet: true,

    open(node) {
        this.id = node.parentNode.id;
        this.reopen();
        this.setHash(this.id);
    },

    setHash(hash) {
        this.hashSet = false;
        document.location.hash = '##' + hash;
        setTimeout(() => this.hashSet = true, 100);
    },

    byHash() {
        let hash = document.location.hash;
        if (hash.indexOf('##') > -1 && hash != '##null') {
            this.id = hash.replace('##', '');
            try { this.reopen(); } catch (e) { }
        } else try { this.close(); } catch (e) { }
    },

    reopen() {
        fetch(host + 'home/cart/' + this.id)
            .then(res => res.text())
            .then(text => {
                let cart = document.getElementById('cart');
                cart.innerHTML = text;
                cart.classList.add('cart_visible');
                let el = document.getElementById(this.id);
                if (el) {
                    document.querySelectorAll('#view .selected').forEach(el => el.classList.remove('selected'));
                    el.classList.add('selected');
                }
            });
    },

    close() {
        document.getElementById('cart').classList.remove('cart_visible');
        document.getElementById('view').querySelectorAll('.selected').forEach(el => el.classList.remove('selected'));
        this.id = '';
        this.setHash(null);
    }
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

var Folders = {

    fetch(url, data, callback) {
        data = data || {};
        let form = new FormData();
        Object.keys(data).forEach(key => form.append(key, data[key]));
        fetch(url, { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Warning) message(json.Warning);
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                    if (callback) callback(json);
                }
            });
    },

    create() {
        let name = prompt('Название группы: ', 'Новая группа (' + (new Date).toLocaleString() + ')');
        if (!name) return;

        Folders.fetch(host + 'folders/create', { Type: pageName, Name: name });
    },

    createInner() {
        let name = prompt('Название группы: ', 'Новая группа (' + (new Date).toLocaleString() + ')');
        if (!name) return;
        
        Folders.fetch(host + 'folders/createInner', {
            Type: pageName,
            Name: name,
            FolderId: menuId.replace(/\D+/g, '')
        });
    },

    beforeMove() {
        let obj = document.getElementById(menuId) || document.querySelector('[data-id="' + menuId + '"]');
        obj = obj.closest('.unit');
        let exceptions = [];

        exceptions.push(menuId);
        let parent = obj.parentNode.closest('.group');
        if (parent) exceptions.push(parent.id);
        obj.querySelectorAll('.group').forEach(el => exceptions.push(el.id));

        let options = '<option value="0">Расположить отдельно';
        document.querySelectorAll('div.group').forEach(el => {
            let exception = false;
            for (let i = 0; i < exceptions.length; i++) if (el.id === exceptions[i]) exception = true;
            if (!exception) options += '<option value="' + el.id + '">' + el.querySelector('.caption b').innerHTML;
        });

        document.querySelectorAll('.contextMenu_visible').forEach(el => el.classList.remove('contextMenu_visible'));
        clearTimeout(when);

        let menu = document.getElementById('modal');
        let rect = obj.getBoundingClientRect();

        menu.style.top = (rect.top + pageYOffset + 6) + 'px';
        menu.style.left = (rect.left + pageXOffset + 2) + 'px';
        menu.getElementsByTagName('li')[0].innerHTML = '<select>' + options + '</select>';
        menu.getElementsByTagName('li')[1].onclick = () => {
            if (menuId.includes('folder')) Folders.move();
            else if (menuId.includes('off')) Writeoffs.move();
            else Devices.computerMove();
            menu.classList.remove('contextMenu_visible');
        };
        menu.classList.add('contextMenu_visible');
    },

    move() {
        Folders.fetch(host + 'folders/move', {
            FolderId: document.getElementById('modal').querySelector('select').value.replace(/\D+/g, ''),
            Id: menuId.replace(/\D+/g, '')
        });
    },

    update() {
        let name = prompt('Название группы: ', document.querySelector('#' + menuId + ' .caption b').innerHTML);
        if (!name) return;
        
        Folders.fetch(host + 'folders/update', { Name: name, Id: menuId.replace(/\D+/g, '') }, () => document.querySelector('#' + menuId + ' .caption b').innerHTML = name);
    },

    clear() {
        Folders.fetch(host + 'folders/clear', { Type: pageName, Id: menuId.replace(/\D+/g, '') });
    },

    del() {
        Folders.fetch(host + 'folders/delete', { Type: pageName, Id: menuId.replace(/\D+/g, '') });
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
                        Cart.id = json.Id;
                        Cart.reopen();
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
                        Cart.reopen();
                    }
                });
        },

        copy() {
            fetch(host + 'devices/copy/' + Cart.id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        Cart.id = json.Id;
                        Cart.reopen();
                        restore();
                    }
                });
        },

        del() {
            fetch(host + 'devices/delete/' + Cart.id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        Cart.close();
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
            if (isNaN(+count) || count === '') return message('Количество деталей должно быть числом');

            let select = document.getElementById('defect_position');
            let type = select.value;
            let text = type === 'unique' ? document.getElementById('defect_unique').value : select.options[select.selectedIndex].innerHTML;
            if (type === 'unique' && text === '') return message('Не введено название неисправности');

            document.getElementById('defect_container').insertAdjacentHTML('beforeend', `
            <tr>
                <td val="${type}" colspan="2">${text}</td>
                <td>${Math.round(count)}</td>
                <td><button class="cart__button" onclick="Devices.DefectAct.del(this)">Удалить</button></td>
            </tr>`);
        },

        del(button) {
            let row = button.parentNode.parentNode;
            row.parentNode.removeChild(row);
        },

        check(select) {
            document.getElementById('defect_unique').disabled = select.value !== 'unique';
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
                    message(json.Good, 'good');
                    let a = document.createElement('a');
                    document.body.appendChild(a);
                    a.href = json.Link;
                    a.click();
                }
            });
    },

    printReportByFolder() {
        fetch(host + 'devices/printRecordCartByFolder/' + menuId.replace(/\D+/g, ''), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    let a = document.createElement('a');
                    document.body.appendChild(a);
                    a.href = json.Link;
                    a.click();
                }
            });
    },

    computerOpen() {
        document.location.hash = '##' + menuId;
    },

    computerDelete() {
        Devices.del(menuId);
    },

    computerMove() {
        let form = new FormData();
        form.append('Id', menuId.replace(/\D+/g, ''));
        form.append('FolderId', document.querySelector('#modal select').value.replace(/\D+/g, ''));

        fetch(host + 'devices/move/', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
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
                    Cart.reopen();
                }
            });
    },

    del(Id) {
        if (!confirm('Данный объект будет удален. Продолжить?')) return;
        fetch(host + 'storages/delete/' + Id, { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Error) message(json.Error);
                if (json.Warning) message(json.Warning, 'warning');
                if (json.Good) {
                    message(json.Good, 'good');
                    Cart.close()
                    restore();
                }
            });
    },

    compare() {
        document.getElementById('selected').classList.remove('selection_visible');
        document.getElementById('excl').classList.add('selection_visible');
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
    },

    Import: {

        add(obj, excel) {
            
            let form = new FormData();
            for (let key in excel) form.append(key, excel[key]);

            fetch(host + 'storages/addExcelToStorage', { method: 'POST', body: form })
                .then(res => res.json())
                .then(json => {
                    if (json.Error) message(json.Error);
                    if (json.Good) {
                        message(json.Good, 'good');

                        let row = obj.closest('tr');
                        row.classList.remove('compare_not_found');
                        row.querySelector('td[insert="Nis"]').innerHTML = excel.Nall;
                        row.querySelector('td[insert="Nuse"]').innerHTML = 0;
                        row.setAttribute('id', json.Id);

                        obj.onclick = () => Storages.Import.open(json.Id);
                        obj.innerHTML = 'Открыть карточку';
                    }
                });
        },

        addAll() {
            let rows = document.querySelectorAll('#view .compare_not_found button');
            if (rows.length == 0) return message('Нет позиций, которые необходимо добавить', 'warning');
            rows.forEach(el => el.click());
        },

        open(Id) {
            fetch(host + 'storages/cart/' + Id)
                .then(res => res.text())
                .then(text => {
                    let cart = document.getElementById('cart');
                    cart.innerHTML = text;
                    cart.classList.add('cart_visible');
                    document.querySelectorAll('#view .selected').forEach(el => el.classList.remove('selected'));
                    document.getElementById(Id).classList.add('selected');
                });
        }
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
            fetch(host + 'repairs/createFromDevice/' + Cart.id)
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
            form.append('Id', Cart.id);
            document.getElementById('repairsForm').querySelectorAll('input,select,textarea').forEach(el => form.append(el.name, el.value));
            fetch(host + 'repairs/createFromDeviceData', { method: 'POST', body: form })
                .then(res => res.text())
                .then(text => {
                    document.getElementById('repairsData').innerHTML = text;
                });
        },

        end(withWriteoff) {

            let form = new FormData();
            form.append('Id', Cart.id);
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
            Cart.setHash(null);
            Cart.close();
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
            if (document.getElementById('repairsData').querySelectorAll('tr').length == 0) Cart.close();
        },

        end(withWriteoff) {
            let form = new FormData();
            form.append('Id', Cart.id);
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
        },

        back() {
            Cart.reopen();
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
                        Cart.reopen();
                    }
                    if (json.Warning) message(json.Warning, 'warning');
                    if (json.Error) message(json.Error);
                });
        },

        del(Id) {
            if (!confirm('Данный объект будет удален. Продолжить?')) return;

            fetch(host + 'repairs/delete/' + Id, { method: 'POST' })
                .then(res => res.json())
                .then(json => {
                    if (json.Good) {
                        message(json.Good, 'good');
                        Cart.close()
                        restore();
                    }
                });
        },
    },

    off() {
        fetch(host + 'repairs/off/' + menuId.replace('off', ''), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
    },

    on() {
        fetch(host + 'repairs/on/' + menuId.replace('off', ''), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
    },

    clear() {
        if (!confirm('Все ремонты в выбранном списании будут отменены, использованные позиции будут возвращены на склад. Продолжить?')) return;
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
                    Cart.reopen();
                }
                if (json.Warning) message(json.Warning, 'warning');
                if (json.Error) message(json.Error);
            });
    },

    export(Id) {
        fetch(host + 'writeoffs/print/' + (Id || menuId.replace(/\D+/g, '')), { method: 'POST' })
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
        if (!confirm('Данный объект будет удален. Продолжить?')) return;

        fetch(host + 'writeoffs/delete/' + (Id || menuId.replace(/\D+/g, '')), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    Cart.close()
                    restore();
                }
            });
    },

    move() {
        let form = new FormData();
        form.append('Id', menuId.replace(/\D+/g, ''))
        form.append('FolderId', document.querySelector('#modal select').value);
        fetch(host + 'writeoff/move', { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    restore();
                }
            });
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
        form.append('Id', Cart.id.replace(/cart|prn/, ''));

        fetch(host + 'catalog/update' + (Cart.id.includes('prn') ? 'printer' : 'cartridge'), { method: 'POST', body: form })
            .then(res => res.json())
            .then(json => {
                if (json.Good) message(json.Good, 'good');
                if (json.Warning) message(json.Warning, 'warning');
                if (json.Error) message(json.Error);
            });
    },

    del() {
        if (!confirm('Данный объект будет удален. Продолжить?')) return;
        fetch(host + 'catalog/delete' + (Cart.id.includes('prn') ? 'printer' : 'cartridge') + '/' + Cart.id.replace(/cart|prn/, ''), { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    Cart.close()
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
                    Cart.reopen();
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
                    Cart.reopen();
                }
            });
    },

    search(input) {
        let s = input.value.toLowerCase();
        if (s === '') {
            document.querySelectorAll('.items tr').forEach(el => el.style.display = '');
        } else {
            document.querySelectorAll('.items tr').forEach(el => {
                let text = el.querySelector('td').innerHTML.toLowerCase();
                el.style.display = text.includes(s) ? '' : 'none';
            });
        }
    }
};

var Aida = {

    toggle(el) {
        el.classList.toggle('open');
        document.getElementById(el.getAttribute("for")).classList.toggle('open');
    },

    del() {
        if (!confirm('Отчет по данному компьютеру будет безвозвратно удален. Продолжить?')) return;
        fetch(host + 'aida/delete/' + Cart.id, { method: 'POST' })
            .then(res => res.json())
            .then(json => {
                if (json.Good) {
                    message(json.Good, 'good');
                    Cart.close();
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