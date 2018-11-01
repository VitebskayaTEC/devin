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