function toggle(node) {
	var $unit = $(node).closest('.unit');
	var id = $unit.attr('id')
		? $unit.attr('id')
		: $unit.find('.title-wrapper:first-child').attr('id');
	if ($unit.hasClass('open')) {
		$unit
			//.animate({ marginTop: '1px', marginBottom: '1px' }, 150)
			.children('.items_block')
			.slideToggle(150, function() {
				$unit.removeClass('open');
			});
		setCookie(id, '', { expires: 9999999999 });
	} else {
		$unit
			//.animate({ marginTop: '10px', marginBottom: '10px' }, 150)
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

function cartOpenBack() {
	$('#cart')
		.fadeIn(100)
		.load('/devin/views/device_cart.asp?id=' + id + '&r=' + Math.random());
	$('.view .selected').removeClass('selected');
	try {
		$('#' + id).addClass('selected');
	} catch (e) {}
}

function cartBack() {
	cartOpenBack();
}

function deviceToRepair() {
	$('#cart')
		.fadeIn(100)
		.load(
			'/devin/views/device_to_repair.asp?id=' + id + '&r=' + Math.random()
		);
}

function cartHistoryRepair() {
	$('#cart')
		.html("<b class='load'>загрузка...</b>")
		.fadeIn(100)
		.load(
			'/devin/views/device_history_repair.asp?id=' +
				id +
				'&r=' +
				Math.random()
		);
}

function cartHistory(query) {
	$('#cart')
		.html("<b class='load'>загрузка...</b>")
		.fadeIn(100)
		.load(
			'/devin/views/device_history.asp?id=' +
				id +
				'&' +
				query +
				'&r=' +
				Math.random()
		);
}

function cartAida() {
	$('#cart')
		.html("<b class='load'>загрузка...</b>")
		.load('/devin/views/device_aida.asp?id=' + id + '&r=' + Math.random())
		.fadeIn(100);
}

function cartAidaDevices() {
	$('#cart')
		.load(
			'/devin/views/device_aida_devices.asp?id=' +
				id +
				'&r=' +
				Math.random()
		)
		.fadeIn(100);
}

function cartAidaAutorun() {
	$('#cart')
		.load(
			'/devin/views/device_aida_autorun.asp?id=' +
				id +
				'&r=' +
				Math.random()
		)
		.fadeIn(100);
}

function cartAidaPrograms(name) {
	$.get('/devin/views/device_aida_programs.asp', { 
			name: name,
			r: Math.random()
		}, function (data) {
			$('#cart').html(data).fadeIn(100);
		});
}

function cartDefect() {
	$('#cart')
		.load(
			'/devin/views/device_defect_act.asp?id=' +
				id +
				'&r=' +
				Math.random()
		)
		.fadeIn(100);
}

function cartCreate() {
	$.get('/devin/exes/device/device_create.asp?r=' + Math.random(), function(data) {
		if (data.indexOf('error') < 0) {
			id = data;
			reload({});
			$('#cart')
				.load(
					'/devin/views/device_cart.asp?id=' +
						id +
						'&r=' +
						Math.random()
				)
				.fadeIn(100);
		}
	});
}

function cartSave() {
	$.post(
		'/devin/exes/device/device_save_cart.asp?id=' + id,
		//'/devin.net/devices/api/devicesave.ashx?id=' + id,
		$('#cart-form').serialize(),
		function(data) {
			if (data.indexOf('error: ') == -1) {
				reload({});
				document.getElementById('console').innerHTML =
					"<div class='done'>" + data + '</div>';
			} else {
				document.getElementById('console').innerHTML =
					"<div class='error'>" + data + '</div>';
			}
		}
	);
}

function cartCopy() {
	$.get(
		'/devin/exes/device/device_copy.asp?id=' + id + '&r=' + Math.random(),
		function(data) {
			if (data.indexOf('error') < 0) {
				id = data;
				reload({});
				$('#cart')
					.load(
						'/devin/views/device_cart.asp?id=' +
							id +
							'&r=' +
							Math.random()
					)
					.fadeIn(100);
			}
		}
	);
}

function cartDelete() {
	$.get('/devin/exes/device/device_delete.asp?id=' + id, function() {
		reload({});
		$('#cart').fadeOut(100, function() {
			document.getElementById('cart').innerHTML = '';
		});
	});
}

function history() {
	$('#cart')
		.html("<b class='load'>загрузка...</b>")
		.fadeIn(100)
		.load('/devin/views/history.ashx?r=' + Math.random());
	$('.view .selected').removeClass('selected');
}

var state = {
	key: 0,
	direction: 0,
	text: '',
	queryString: function() {
		return (
			'text=' +
			encodeURIComponent(this.text) +
			'&key=' +
			this.key +
			'&direction=' +
			this.direction +
			'&r=' +
			Math.random()
		);
	}
};

function reload(obj) {
	$.extend(state, obj);
	$('#view').load('view.asp?' + state.queryString(), function() {
		$('#' + id).addClass('selected');
		$('.main th:eq(' + state.key + ')').addClass(
			state.direction == 0 ? 'sort_up' : 'sort_down'
		);
		selectionPanel();
	});
}

function sort(node) {
	if (state.key == node.cellIndex)
		reload(state.direction == 0 ? { direction: 1 } : { direction: 0 });
	else reload({ key: node.cellIndex, direction: 0 });
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

function moveSelectedDevices() {
	$.post(
		'/devin/exes/device/device_move_all_selected.asp?r=' + Math.random(),
		selectionToForm('devices', ';;') +
			'&key=' +
			document.getElementById('moveKey').value,
		function(data) {
			removeAllSelection();
			reload({});
		}
	);
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
	$.post(
		'/devin/exes/device/device_move_computer.asp?r=' + Math.random(),
		'id=' +
			menuId +
			'&key=' +
			$('#modal select:first-child')
				.val()
				.replace('dg', ''),
		function() {
			reload({});
		}
	);
	$('#modal').fadeOut(100);
}

function computerAida() {
	id = menuId;
	cartAida();
}