function sort(header, type) {

    let head = header.parentNode.parentNode
    let table = head.parentNode
    let body = table.querySelector('tbody')

    type = type || 'string'
    let way = header.getAttribute('sort-way')

    let colNum = header.cellIndex
    let arr = []
    let compare

    for (let i = 0; i < body.rows.length; i++) arr.push(body.rows[i])

    switch (type) {
        case 'sort':
            compare = (rowA, rowB) => {
                let a = +rowA.cells[colNum].getAttribute('sort');
                let b = +rowB.cells[colNum].getAttribute('sort');
                return (way == 'up')
                    ? b > a ? 1 : -1
                    : a > b ? 1 : -1;
            };
            break;
        case 'number':
            compare = (rowA, rowB) => (way == 'up')
                ? +rowA.cells[colNum].innerHTML > +rowB.cells[colNum].innerHTML ? 1 : -1
                : +rowB.cells[colNum].innerHTML > +rowA.cells[colNum].innerHTML ? 1 : -1;
            break;
        case 'string':
            compare = (rowA, rowB) => (way == 'up')
                ? rowA.cells[colNum].innerHTML > rowB.cells[colNum].innerHTML ? 1 : -1
                : rowB.cells[colNum].innerHTML > rowA.cells[colNum].innerHTML ? 1 : -1;
            break;
    }

    for (let i = 0, headers = head.getElementsByTagName('th'), len = headers.length; i < len; i++) headers[i].className = '';
    if (way == 'up') {
        header.setAttribute('sort-way', 'down');
        header.className = 'sort-down';
    } else {
        header.setAttribute('sort-way', 'up');
        header.className = 'sort-up';
    }

    arr.sort(compare)
    table.removeChild(body)
    for (let i = 0; i < arr.length; i++) body.appendChild(arr[i])
    table.appendChild(body)
}

window.addEventListener('load', function () {

    var items = document.querySelectorAll('#fact tbody tr')

    let max = [0, 0, 0],
        min = [Infinity, Infinity, Infinity]

    items.forEach(row => {
        let cells = row.cells
        for (let i = 0; i < 3; i++) {
            let value = +cells[i + 2].getAttribute('sort')
            if (value > max[i]) max[i] = value
            if (value < min[i]) min[i] = value
        }
    })

    const color = 220

    items.forEach(row => {
        let cells = row.cells
        let totalScore = 0
        for (let i = 0; i < 3; i++) {
            let value = +cells[i + 2].getAttribute('sort')
            let score = (value - min[i]) / (max[i] - min[i])
            let rgb = Math.round(score * color * 2)

            totalScore += rgb

            cells[i + 2].style.color = '#000'
            cells[i + 2].style.background = rgb < color
                ? 'rgb(' + color + ',' + ((255 - color) + rgb) + ',0)'
                : 'rgb(' + (color + 255 - rgb) + ',' + color + ',0)'
        }

        cells[5].innerHTML = totalScore
    })
})