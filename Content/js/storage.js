function toggle(node) {
    var $unit = $(node).closest('.unit');
    if ($unit.hasClass("open")) {
        $unit.children(".items_block").slideToggle(150, function() {
            $unit.removeClass("open");
        });
        setCookie($unit.attr("id"), "", {
            expires: 999999999
        });
    } else {
        $unit.addClass("open").children(".items_block").slideToggle(150);
        setCookie($unit.attr("id"), "open", {
            expires: 999999999
        });
    }
}

function search(input) {
    if (input.value == "" && document.location.search.indexOf("text=") > -1) document.location = '/devin/storages/';
    var e = event || window.event;
    if (e.keyCode == 13) {
        document.location.search = "text=" + encodeURIComponent(input.value);
    }
}

function cartOpenBack() {
    $("#cart").attr("class", "cart-new").load("/devin/asp/storage_cart.asp?id=" + id + "&r=" + Math.random()).fadeIn(150);
    $(".selected").removeClass("selected");
    $("#" + id).addClass("selected");
}

function cartHistory() {
    $("#cart").load("/devin/asp/storage_history.asp?id=" + id + "&r=" + Math.random()).fadeIn(150);
}

function cartBack() {
    cartOpenBack();
}

function cartCreate() {
    $.get("/devin/exes/storage/storage_create_cart.asp?r=" + Math.random(), function(data) {
        if (data.indexOf("error") < 0) {
			restore();
            document.location = "/devin/storages/##" + data;
        }
    });
}

function filter() {
    $('.panel:not(#filter,#selected)').slideUp(100);
    $('#filter').slideToggle(100);
}

function cartDelete() {
    $("#cart").load("/devin/exes/storage/storage_delete_cart.asp?id=" + id + "&r=" + Math.random(), function() {
        $("#" + id).remove();
        cartClose();
    });
}

function cartRepair() {
    $("#cart").load("/devin/asp/storage_repair.asp?id=" + id + "&r=" + Math.random()).fadeIn(150);
}

function cartSave() {
    $.post("/devin/exes/storage/storage_save_cart.asp?id=" + id + "&r=" + Math.random(), $("form").serialize(), function (data) {
        // ���������� view �� ����� ������
        if (document.location.search.indexOf("text=") < 0) {
            $.get("/devin/storages/list?r=" + Math.random(), function (data) {
                $("#view").html(data);
                setTimeout(function () {
                    $("#" + id).addClass("selected");
                }, 50);
            });

            $("#cart").load("/devin/asp/storage_cart.asp?id=" + id + "&r=" + Math.random(), function () {
                try {
                    document.getElementById("console").innerHTML = data;
                } catch (e) { }
            });
        } else {
            $.get("/devin/storages/list" + document.location.search + "&r=" + Math.random(), function (data) {
                $("#view").html(data);
                setTimeout(function () {
                    $("#" + id).addClass("selected");
                }, 50);
            });

            $("#cart").load("/devin/asp/storage_cart.asp?id=" + id + "&r=" + Math.random(), function () {
                try {
                    document.getElementById("console").innerHTML = data;
                } catch (e) { }
            });
        }
    });
}

function compare() {
    $('.panel:not(#excl,#selected)').slideUp(100);
    $('#excl').slideToggle(100);
}

function compareSetSource(node) {
    switch (node.value) {
        case "in":
            document.getElementById("compare-form").action = "/devin/asp/storage_load_positions.asp";
            break;
        case "out":
            document.getElementById("compare-form").action = "/devin/asp/storage_load_rest.asp";
            break;
        case "compare":
            document.getElementById("compare-form").action = "/devin/asp/storage_1c_compare.asp";
            break;
    }
}

function allStoragesToRepair() {
    $.post("/devin/asp/storage_all_repairs.asp?r=" + Math.random(), selectionToForm("select", ";"), function(data) {
        $("#cart").addClass("repairs").html(data).fadeIn(150);
        removeAllSelection();
    });
}

function storageToRepair() {
    $.post("/devin/asp/storage_all_repairs.asp?r=" + Math.random(), "select=" + id + ";", function(data) {
        $("#cart").addClass("repairs").html(data).fadeIn(150);
    });
}

function storagesToGroup() {
    $.post("/devin/exes/storage/storages_to_group.asp?r=" + Math.random(), selectionToForm("select", ";") + "&gid=" + document.getElementById('move_select').getElementsByTagName('select')[0].value, restore);
    removeAllSelection();
}


/*
���� �������� ������ �������� ��� ��������� �������
*/

// �������� ��������� �������� ���������� �� ������������ ���������� ��������
function verifyCounts(input) {
    // ���������� �������, � ������� ����� ��������
    var tr = input.parentNode.parentNode;

    // ��������, �������� �� ��������� �������� ����� ������
    if (isNaN(input.value) || (input.value.indexOf(".") > -1) || (input.value.indexOf(",") > -1)) {
        input.value = "0";
        return;
    }

    // ������ ������ ���������� ���������� ������� 
    var allVals = 0,
        query = "." + tr.className;
    for (var i = 0, allInputs = tr.parentNode.querySelectorAll(query); i < allInputs.length; i++)
        if (allInputs[i].querySelector(".number") != input && !allInputs[i].getElementsByTagName("input")[1].checked)
            allVals += +allInputs[i].querySelector(".number").value;

        // ������ ����������� ���-�� ������� ��� ������ ������ � �������� �� ��������� � ���������� ��������
    var val = +input.value,
        max = +tr.querySelector("span").innerHTML - allVals;
    if (val < 0) input.value = 0;
    if (val > max && !tr.getElementsByTagName("input")[1].checked) input.value = max;
}

// �������� ���������� � ������ ��� �������� ����� "����������� ������"
function virtualVerifyCounts(checkbox) {
    if (!checkbox.checked) {
        // ���������� �������, � ������� ����� ��������
        var tr = checkbox.parentNode.parentNode;
        var input = tr.querySelector(".number");

        // ������ ������ ���������� ���������� ������� 
        var allVals = 0,
            query = "." + tr.className;
        for (var i = 0, allInputs = tr.parentNode.querySelectorAll(query); i < allInputs.length; i++)
            if (allInputs[i] != tr && !allInputs[i].getElementsByTagName("input")[1].checked)
                allVals += +allInputs[i].querySelector(".number").value;

            // ������ ����������� ���-�� ������� ��� ������ ������ � �������� �� ��������� � ���������� ��������
        var val = +input.value,
            max = +tr.querySelector("span").innerHTML - allVals;
        if (val > max) input.value = max;
    }
}

// ���������� �������
function separatePosition(a) {
    // ���������� �������, � ������� ����� ��������
    var tr = a.parentNode.parentNode;

    // ��������� ������� �������� ���������� � ���������� ������, ����� ����� ����� ��������� ��������� �� ��������� ���-�� �� ������
    //var oldVal = tr.querySelector(".number").value;
    //tr.querySelector(".number").value = (+oldVal > 0 ? (+oldVal - 1) : 0);

    // �������� �������� ���������� � ����� ������-�����
    var newTr = tr.cloneNode(true);
    newTr.querySelector(".number").value = 0;

    // ������� ������������� ������
    tr.parentNode.insertBefore(newTr, tr);

    // �������� ���������� �����, ��� �� ������ ��������� ���-�� �� ������. ���� ���-�� ����� �������� ���-�� �� ������, �������� ������-�����������
    //var $allTrs = $(tr).parent().find("." + tr.className), val = +tr.querySelector("span").innerHTML;
    //if (val == $allTrs.length) $allTrs.find(".pos-separate").css("display", "none");
}

// �������� �������
function removePosition(a) {
    // ���������� �������, � ������� ����� ��������, � �� �����, ����� ����� � ��������� ������� ���� �������
    var tr = a.parentNode.parentNode;

    // ��������� ���-�� �����, ���� ��� ������, ��� ���-�� �� ������, ���������� ������-�����������
    //var $allTrs = $(tr).parent().find("." + pos), val = +tr.querySelector("span").innerHTML;
    //if (val > $allTrs.length - 1) $allTrs.find(".pos-separate").css("display", "inline");

    // ������� ������
    tr.parentNode.removeChild(tr);

    // ���������, �������� �� ��� ������� ��� �������
    if (document.getElementById("repairs-positions").getElementsByTagName("tr").length == 0) cartClose();
}

// ���������� ���������� ������������ ������� � ������� ��� ��������� ������� �������
function changeDevice(select) {
    var input = select.parentNode.parentNode.querySelector(".number");
    if (select.value == "0") {
        input.value = "0";
    } else if (input.value == "0") {
        input.value = "1";
    }
}

// ���������� ��������� �������� 
function createRepairs() {
    var trs = document.getElementById("repairs-positions").getElementsByTagName("tr");
    var form = "allrepairs=";
    for (var i = 0; i < trs.length; i++) {
        var device = trs[i].getElementsByTagName("select")[0],
            units = trs[i].getElementsByTagName("input")[0];
        if (device.value != "0" && units.value != "0") {
            form += trs[i].className.replace("pos", "") + ";" + device.value + ";" + units.value + ";" + (trs[i].getElementsByTagName("input")[1].checked ? "1" : "0") + "<separate>";
        }
    }
    if (document.getElementById("create-off-group").checked) form += "&createOff=" + encodeURIComponent(document.getElementById("name-off-group").value);

    $.post("/devin/exes/storage/storage_create_repairs.asp?r=" + Math.random(), form, function(data) {

            // ������� ����������� ��������� �������� ������� ��� �������� ��������� �������
            $(".cart-overflow > table").remove();
            $("#off-group").remove();
            $("#cart").find(".cart-menu td:first-child").remove();

            // ������� ������ �� ������ ��������, ���� � ���������� "�������"
            document.getElementById("console").innerHTML = data;
        })
        .fail(function() {
            document.getElementById("console").innerHTML = "��������� ������ ��� ��������� �����";
        });
}



function excelExports() {
    $.post("/devin/storages/labels", selectionToForm("select", ";"), function(data) {
        $("#view").load("/devin/storages/list?r=" + Math.random());
        document.getElementById("excelExportsLink").innerHTML = data;
        $('#excelExports').slideDown(100);
    });
    removeAllSelection();
}

function closeExportsPanel() {
    $('#excelExports').slideUp(100);
    document.getElementById("excelExportsLink").innerHTML = "";
}