class aida {

    /**
     * Удаление отчёта по компьютеру из базы данных Everest
     * @param {Number} id
     */
    static delReport(id) {
        post('/aida/delete', { id })
            .then(() => {
                let el = document.querySelector('[card="aida|' + id + '"]')
                if (el) el = el.closest('.entry')
                if (el) el.remove()
                NAV.toHash({ card: '' })
            })
    }

    static description(cell, name) {
        let text = cell.querySelector('textarea')
        if (text) return 

        let i = cell.querySelector('i')
        if (i) cell.removeChild(i)

        cell.innerHTML = '<textarea>' + cell.innerHTML.trim() + '</textarea><button>Сохранить</button>'

        text = cell.querySelector('textarea')
        text.focus()

        let button = cell.querySelector('button')
        button.onclick = function () {
            post('/aida/description', { description: text.value, name })
                .then(json => {
                    if (json.Done) {
                        cell.innerHTML = text.value == '' ? '<i>добавить описание...</i>' : text.value
					}
				})
        }
	}
}