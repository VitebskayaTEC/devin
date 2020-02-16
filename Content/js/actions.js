



class aida {

    static toggle() {

    }

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
}