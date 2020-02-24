// Контекстные меню

let menuTimeout, menuId;

function menu(obj, name) {

    let e = event || window.event
    e.preventDefault()

    document.querySelectorAll('.menu').forEach(el => el.style.display = 'none')
    clearTimeout(menuTimeout);

    let menu = document.getElementById(name || obj.getAttribute('menu'));
    if (!menu) return
    menu.onmouseleave = () => menuTimeout = setTimeout(() => menu.style.display = 'none', 1000);
    menu.onmouseenter = () => clearTimeout(menuTimeout);
    menu.onclick = () => menu.style.display = 'none';

    let rect = obj.getBoundingClientRect();
    menu.style.top = (rect.top + pageYOffset + 6) + 'px';
    menu.style.left = (rect.left + pageXOffset + 2) + 'px';
    menu.style.display = 'inherit';

    let container = obj.closest('.unit');
    if (container) menuId = container.id || container.getAttribute('data-id');
}


// Запросы и действия

var post = function (url, data) {
    return new Promise(function (resolve, reject) {
        var xhr = new XMLHttpRequest()
        xhr.open('POST', host + url + (url.indexOf('?') > -1 ? '&' : '?') + 'r=' + Math.random(), true)
        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4) return
            if (xhr.status >= 400) reject(xhr.status + ' [' + xhr.statusText + ']')
            try {
                var json = JSON.parse(xhr.responseText, function (_key, value) {
                    if (typeof value === 'string') {
                        var d = /\/Date\((\d*)\)\//.exec(value)
                        return (d) ? new Date(+d[1]) : value
                    }
                    return value
                })
                if (json.Err) log(json.Error, 'error')
                else if (json.Warn) log(json.Warning, 'warn')
                else {
                    if (json.Done) log(json.Done)
                    resolve(json)
                }
            }
            catch (e) {
                reject(e.message)
            }
        }
        var body = JSON.stringify(data)
        xhr.setRequestHeader('Content-Type', 'application/json')
        xhr.send(body)
    }).catch(reason => log(reason, 'error'))
}

var get = function (url, data) {
    return new Promise(function (resolve, reject) {
        var xhr = new XMLHttpRequest()
        xhr.open('POST', host + url + (url.indexOf('?') > -1 ? '&' : '?') + 'r=' + Math.random(), true)
        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4) return
            if (xhr.status >= 400) reject(log(xhr.status + ' [' + xhr.statusText + ']', 'err'))
            try {
                resolve(xhr.responseText)
            }
            catch (e) {
                reject(log(e.message, 'err'))
            }
        }
        var body = JSON.stringify(data)
        xhr.setRequestHeader('Content-Type', 'application/json')
        xhr.send(body)
    })
}


// строка поиска


// Служебные сообщения

var logs = document.getElementById('logs')

logs.onclick = function (e) {
    if (e.target.parentNode == logs) logs.removeChild(e.target)
}

function log(text, type) {
    let div = document.createElement('div')
    div.innerHTML = text
    div.className = type || ''
    logs.appendChild(div)
    setTimeout(() => logs.removeChild(div), 5000)
}



/*
Маршрутизация

Хеш-строка страницы делится на отрезки слэшами
Каждый из отрезком отвечает за какой-то функционал, отображаемый в соответствии с заданными настройками
В случае, если какой-то из отрезков не указывется, используется поведение по умолчанию
Секции проверяются при каждом изменении хэша страницы
Действие секций применяется, если найдено различие между текущим и предыдущим вариантами

Основные секции:

1.  Карточки элементов
    card:<type>|<id>|<tab-name>, 
        где type и id отправляются на сервер, 
        а необязательный параметр tab открывает нужную вкладку карточки после её открытия, при условии, что такая вкладка есть
        по умолчанию закрывает элемент карточки

2.  Поиск по разделу 
    search:<query>|<type>, 
        где query это сам запрос, 
        а необязательный параметр type устанавливает, как именно выполняется поиск
        по умолчанию вызывает отображение исходного контента 

3.  Тип отображения контента (не реализовано)
    view:<type>
        по умолчанию контент отображается как список интерактивных объектов, 
        но некоторые страницы допускают отображение в табличном виде или группами с группировкой по какому-либо принципу 
        возможные варианты определяет сама страница, 
        для этого в шапке должен быть определен select с именем view
*/

let pageName = '', pathname = document.location.pathname.toLowerCase();
['devices', 'storages', 'repairs', 'catalog', 'objects1c', 'aida'].forEach(s => pathname.includes(s) && (pageName = s))

document.querySelector('.nav').addEventListener('change', function () {
    let href = ''
    let parent = document.querySelector('.nav')
    let query = parent.querySelector('[name="query"]')
    if (!query) return

    href = query.value
    if (href != '') {
        let type = parent.querySelector('[name="type"]')
        if (type) href += '|' + type.value
    }
    NAV.toHash({ search: href })
})

document.addEventListener('click', function (e) {
    let el = e.target
    let parentEl = el.closest('[card]')
    if (!parentEl) return
    if (!parentEl.hasAttribute('card')) return
    NAV.toHash({ card: parentEl.getAttribute('card') })
})

document.addEventListener('click', function (e) {
    let el = e.target
    let parentEl = el.closest('[sort]')
    if (!parentEl) return
    if (!parentEl.hasAttribute('sort')) return
    NAV.toHash({ sort: parentEl.getAttribute('sort') })
})

var NAV = {

    card: null,
    search: null,
    sort: null,

    toHash(data) {

        let hash = ''
        let data2 = extend({ card: NAV.card, search: NAV.search, sort: NAV.sort }, data, true)

        if (data2.card && data2.card != '') hash += '/card:' + data2.card
        if (data2.search && data2.search != '') hash += '/search:' + data2.search
        if (data2.sort && data2.sort != '') hash += '/sort:' + data2.sort

        document.location.hash = hash == '' ? '/' : hash
    },

    /**
     * 
     * @param {string} hash
     */
    fromHash(hash) {

        let body = document.getElementById('body')
        if (!body) return

        let obj = { card: '', search: '', sort: '' }

        hash.split('/')
            .filter(x => x != '' && x.indexOf(':') > -1)
            .map(x => x.split(':'))
            .forEach(x => obj[x[0]] = decodeURIComponent(x[1]))

        // карточка
        if (obj.card != NAV.card) {

            NAV.card = obj.card

            let el = document.getElementById('card')
            if (el) {

                if (obj.card) {
                    el.classList.remove('hide')

                    let tokens = obj.card.split('|')
                    get('/home/card', { type: tokens[0], id: Number(tokens[1]) })
                        .then(html => {
                            el.innerHTML = html
                            let tab = (tokens.length > 2 ? el.querySelector('[tab="' + tokens[2] + '"]') : null)
                                || el.querySelector('[tab].active')
                                || el.querySelector('[tab]')
                            if (tab) tab.click()
                        })
                        .then(resume)
                }
                else {
                    el.classList.add('hide')
                    resume()
                }
            }
        }

        if (obj.search == NAV.search && obj.sort == NAV.sort) return

        if (obj.search != NAV.search) NAV.search = obj.search
        if (obj.sort != NAV.sort) NAV.sort = obj.sort

        let query = '', type = '', sort = '', direction = ''

        let searches = NAV.search.split('|')
        let sortables = NAV.sort.split('|')

        query = searches[0]
        type = (searches.length > 1 ? searches[1] : null)
        sort = sortables[0]
        direction = (sortables.length > 1 ? sortables[1] : null)

        let el = document.getElementById('search')
        if (el) if (el.value != query) el.value = query

        get('/' + pageName + '/load', { query, type, sort, direction })
            .then(html => body.innerHTML = html)
            .then(resume)
    }
}

if (location.hash.replace(/#|#\//, '') == '') location.hash = '/'
let hash = ''

setInterval(function () {
    let h = location.hash.replace(/#|#\//, '')
    if (h != hash) {
        hash = h
        NAV.fromHash(h)
    }
}, 100)

function resume() {
    // подсветка открытого в карточке элемента
    let active = document.querySelector('[card].active')
    if (active) active.classList.remove('active')
    active = document.querySelector('[card="' + NAV.card + '"]')
    if (active) active.classList.add('active')
}


// вкладки

document.addEventListener('click', function (e) {

    let tabName = e.target.getAttribute('tab')
    if (!tabName) return

    let tabsPanel = e.target.parentNode
    if (!tabsPanel || !tabsPanel.classList.contains('tabs-panel')) return

    let tabsContainer = tabsPanel.parentNode.querySelector('.tabs')
    if (!tabsContainer) return

    let tab = tabsContainer.querySelector('[tab="' + tabName + '"]')
    if (!tab) return

    tabsPanel.querySelector('.active').classList.remove('active')
    tabsContainer.querySelector('.active').classList.remove('active')

    e.target.classList.add('active')
    tab.classList.add('active')

    if (tab.hasAttribute('view')) {
        get(tab.getAttribute('view')).then(html => tab.innerHTML = html)
    }
})

document.onerror = function (e) {
    log('JS: ' + e.message, 'err')
}


// открытие-закрытие папок

document.addEventListener('click', function (e) {
    if (!e.target.classList.contains('folder_caption')) return
    let folder = e.target.parentNode
    let i = e.target.querySelector('i')
    i.classList.toggle('ic-folder-open')
    i.classList.toggle('ic-folder')
    folder.classList.toggle('open')
    cookie(folder.id, folder.classList.contains('open'), { expires: 99999999 })
})

function cookie(name, value, options) {
    if (arguments.length == 1) {
        let matches = document.cookie.match(new RegExp('(?:^|; )' + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)'));
        return matches ? decodeURIComponent(matches[1]) : undefined;
    }
    else {
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
}


// Вспомогательные функции

function __aida(a, r) {
    document.getElementById('aidablock' + r).classList.toggle('open');
    a.classList.toggle('open')
}


// базовый функционал

function extend(to, from, overwrite) {
    function isDate(obj) {
        return (/Date/).test(Object.prototype.toString.call(obj)) && !isNaN(obj.getTime())
    }
    function isArray(obj) {
        return (/Array/).test(Object.prototype.toString.call(obj))
    }
    var prop, hasProp;
    for (prop in from) {
        hasProp = to[prop] !== undefined;
        if (hasProp && typeof from[prop] === 'object' && from[prop] !== null && from[prop].nodeName === undefined) {
            if (isDate(from[prop])) {
                if (overwrite) {
                    to[prop] = new Date(from[prop].getTime());
                }
            } else if (isArray(from[prop])) {
                if (overwrite) {
                    to[prop] = from[prop].slice(0);
                }
            } else {
                to[prop] = extend({}, from[prop], overwrite);
            }
        } else if (overwrite || !hasProp) {
            to[prop] = from[prop];
        }
    }
    return to;
}