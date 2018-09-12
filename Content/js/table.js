function _sort(th) {
    function to_date(s) {
        if (s == "") return new Date(1999, 0, 0);
		else if (s.indexOf(" ") > -1) {
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
            compare = (rowA, rowB) => way == "up"
                ? rowA.cells[colNum].innerHTML - rowB.cells[colNum].innerHTML
                : rowB.cells[colNum].innerHTML - rowA.cells[colNum].innerHTML;
			break;
		case 'string':
            compare = (rowA, rowB) => way == "up"
                ? rowA.cells[colNum].innerHTML > rowB.cells[colNum].innerHTML ? 1 : -1
                : rowB.cells[colNum].innerHTML > rowA.cells[colNum].innerHTML ? 1 : -1;
			break;
		case 'date':
            compare = (rowA, rowB) => way == "up"
				? +to_date(rowB.cells[colNum].innerHTML) > +to_date(rowA.cells[colNum].innerHTML) ? 1 : -1 
                : +to_date(rowA.cells[colNum].innerHTML) > +to_date(rowB.cells[colNum].innerHTML) ? 1 : -1;
			break;
		case "type":
            compare = (rowA, rowB) => way == "up"
                ? rowA.cells[colNum].querySelector("div").className > rowB.cells[colNum].querySelector("div").className ? 1 : -1
                : rowB.cells[colNum].querySelector("div").className > rowA.cells[colNum].querySelector("div").className ? 1 : -1;
			break;
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

let search = '';
setInterval(() => {
    let s = document.getElementById('search').value.toLowerCase();
    if (s !== search) {
        search = s;
        let rows = document.querySelectorAll('#data tbody tr');
        rows.forEach(row => {
            if (search === '') {
                row.style.display = '';
            } else {
                let cells = row.getElementsByTagName('td');
                let has = false;
                Array.from(cells).forEach(cell => {
                    if (cell.innerHTML.toLowerCase().indexOf(search) > -1 && !has) has = true;
                });
                row.style.display = has ? '' : 'none';
            }
        });
    }
}, 100);