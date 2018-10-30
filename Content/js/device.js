function toggle(node) {
	var $unit = $(node).closest('.unit');
	var id = $unit.attr('id')
		? $unit.attr('id')
		: $unit.find('.title-wrapper:first-child').attr('id');
	if ($unit.hasClass('open')) {
		$unit
			.children('.items_block')
			.slideToggle(150, function() {
				$unit.removeClass('open');
			});
		setCookie(id, '', { expires: 9999999999 });
	} else {
		$unit
			.addClass('open')
			.children('.items_block')
			.slideToggle(150);
		setCookie(id, 'open', { expires: 9999999999 });
	}
}

function cartOpen(node) {
	id = node.parentNode.id;
	cartOpenBack();
	setHash(id);
}



function cartBack() {
	cartOpenBack();
}


function cartHistoryRepair() {
	$('#cart')
		.html("<b class='load'>загрузка...</b>")
		.fadeIn(100)
		.css({ maxWidth: 600 })
		.load(
			'/devin/asp/device_history_repair.asp?id=' +
				id +
				'&r=' +
				Math.random()
		);
}

function history() {
	$('#cart')
		.html("<b class='load'>загрузка...</b>")
		.fadeIn(100).css({ maxWidth: 600 })
		.load('/devin/asp/history.ashx?r=' + Math.random());
	$('.view .selected').removeClass('selected');
}

$('input.def').on('keyup', function(e) {
	if (e.keyCode == 13) reload({ text: this.value });
});

function loadCompare() {
	$('#excl').slideToggle(100);
}

/* Создание ремонтов */

function togglePosition(node) {
	var tr = node.parentNode,
		input = tr.getElementsByTagName('input');
	if (tr.className == 'selected') {
		tr.className = '';
		input[0].value = '0';
		input[1].checked = false;
	} else {
		tr.className = 'selected';
		input[0].value = '1';
		input[1].checked = true;
	}
}

function setPosition(node) {
	var tr = node.parentNode.parentNode;
	if (
		isNaN(node.value) ||
		node.value.indexOf('.') > -1 ||
		node.value.indexOf(',') > -1
	)
		node.value = '0';
	if (+node.value <= 0) {
		node.value = '0';
		if (tr.className != '') {
			tr.className = '';
			tr.getElementsByTagName('input')[1].checked = false;
		}
	} else {
		if (tr.className != 'selected') {
			tr.className = 'selected';
			tr.getElementsByTagName('input')[1].checked = true;
		}
		var max = +tr.getElementsByTagName('td')[1].innerHTML;
		if (+node.value > max) node.value = max;
	}
}

function createRepairs() {
	$.post(
		'/devin/exes/device/device_create_repair.asp?id=' +
			id +
			(document.getElementById('writeoff').checked
				? '&won=on&writeoff=' +
					document.getElementById('writeoff-name').value
				: '') +
			'&r=' +
			Math.random(),
		$('#repair-form').serialize(),
		function(data) {
			$('#repair-data')
				.find('tr.selected')
				.each(function() {
					var input = this.getElementsByTagName('input');
					this.className = '';
					input[1].checked = false;
					if (input[2].checked) {
						input[2].checked = false;
					} else {
						this.getElementsByTagName('td')[1].innerHTML =
							+this.getElementsByTagName('td')[1].innerHTML -
							+input[0].value;
					}
					input[0].value = '0';
				});
			document.getElementById('console').innerHTML = data;
		}
	);
}

function filterClassName(node) {
	document.getElementById('only').checked = false;
	$('#repair-data').load(
		'/devin/exes/device/device_repair_data.asp?only=no&id=' +
			id +
			'&gid=' +
			node.value +
			'&r=' +
			Math.random()
	);
}

function filterOnly(node) {
	if (node.checked) {
		document.getElementById('group').value = class_device;
		$('#repair-data').load(
			'/devin/exes/device/device_repair_data.asp?only=1&id=' +
				id +
				'&r=' +
				Math.random()
		);
	} else {
		$('#repair-data').load(
			'/devin/exes/device/device_repair_data.asp?only=no&id=' +
				id +
				'&gid=' +
				document.getElementById('group').value +
				'&r=' +
				Math.random()
		);
	}
}

function _find() {
	var tr = document.getElementById('tbl').getElementsByTagName('tr');
	var ss = document.getElementById('s' + q).value.toLowerCase();
	var fs;
	for (var i = 0; i < tr.length; i++) {
		var td = tr[i].getElementsByTagName('td')[q];
		if (ss == '') {
			fs = true;
		} else {
			fs = false;
			var sp = td.innerHTML.toLowerCase();
			if (sp.indexOf(ss) != -1) {
				fs = true;
			}
		}
		if (fs) {
			tr[i].style.display = 'block';
		} else {
			tr[i].style.display = 'none';
		}
	}
}

function computerOpen() {
	cartOpen(document.getElementById(menuId).querySelector('.title'));
}

function computerDelete() {
	if (confirm('Данный компьютер будет удален. Продолжить?')) {
		id = menuId;
		cartDelete();
	}
}

function computerMove() {
    let form = new FormData();
        form.append('Key', $('#modal select:first-child').val().replace('dg', ''));

    fetch(host + 'devices/move/' + menuId, { method: 'POST', body: form })
        .then(() => {
            restore();
            $('#modal').fadeOut(100);
        });
}

function computerAida() {
	id = menuId;
	cartAida();
}