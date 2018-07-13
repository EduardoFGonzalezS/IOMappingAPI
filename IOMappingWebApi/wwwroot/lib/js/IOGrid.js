$(function () {
    //called when save changes button is clicked.
    function saveChanges() {
        var grid = $grid.pqGrid('getInstance').grid;

        //debugger;
        //attempt to save editing cell.
        if (grid.saveEditCell() === false) {
            return false;
        }

        var isDirty = grid.isDirty();
        if (isDirty) {
            //validate the new added rows.                
            var addList = grid.getChanges().addList;
            //debugger;
            for (var i = 0; i < addList.length; i++) {
                var rowData = addList[i];
                var isValid = grid.isValid({ "rowData": rowData }).valid;
                if (!isValid) {
                    return;
                }
            }
            var changes = grid.getChanges({ format: "byVal" });

            //post changes to server 
            $.ajax({
                dataType: "json",
                type: "POST",
                async: true,
                beforeSend: function (jqXHR, settings) {
                    grid.showLoading();
                },
                url: "/pro/products/batch", //for ASP.NET, java                                                
                data: { list: JSON.stringify(changes) },
                success: function (changes) {
                    //debugger;
                    grid.commit({ type: 'add', rows: changes.addList });
                    grid.commit({ type: 'update', rows: changes.updateList });
                    grid.commit({ type: 'delete', rows: changes.deleteList });

                    grid.history({ method: 'reset' });
                },
                complete: function () {
                    grid.hideLoading();
                }
            });
        }
    }
    var obj = {
        hwrap: false,
        resizable: true,
        rowBorders: false,
        virtualX: true,
        filterModel: { header: true, type: 'local' },
        trackModel: { on: true }, //to turn on the track changes.            
        toolbar: {
            items: [
                {
                    type: 'button', icon: 'ui-icon-plus', label: 'Add Line', listener:
                        {
                            "click": function (evt, ui) {
                                //append empty row at the end.                            
                                var rowData = { ProductID: 34, UnitPrice: 0.2 }; //empty row
                                var rowIndx = $grid.pqGrid("addRow", { rowData: rowData, checkEditable: true });
                                $grid.pqGrid("goToPage", { rowIndx: rowIndx });
                                $grid.pqGrid("editFirstCellInRow", { rowIndx: rowIndx });
                            }
                        }
                },
                { type: 'separator' },
                {
                    type: 'button', icon: 'ui-icon-disk', label: 'Save Changes', cls: 'changes', listener:
                        {
                            "click": function (evt, ui) {
                                saveChanges();
                            }
                        },
                    options: { disabled: true }
                },
                {
                    type: 'button', icon: 'ui-icon-cancel', label: 'Reject Changes', cls: 'changes', listener:
                        {
                            "click": function (evt, ui) {
                                $grid.pqGrid("rollback");
                                $grid.pqGrid("history", { method: 'resetUndo' });
                            }
                        },
                    options: { disabled: true }
                },
                { type: 'separator' },
                {
                    type: 'button', icon: 'ui-icon-arrowreturn-1-s', label: 'Undo', cls: 'changes', listener:
                        {
                            "click": function (evt, ui) {
                                $grid.pqGrid("history", { method: 'undo' });
                            }
                        },
                    options: { disabled: true }
                },
                {
                    type: 'button', icon: 'ui-icon-arrowrefresh-1-s', label: 'Redo', listener:
                        {
                            "click": function (evt, ui) {
                                $grid.pqGrid("history", { method: 'redo' });
                            }
                        },
                    options: { disabled: true }
                }
            ]
        },
        scrollModel: {
            autoFit: true
        },
        swipeModel: { on: false },
        editModel: {
            saveKey: $.ui.keyCode.ENTER
        },
        editor: {
            select: true
        },
        title: "<b>IO List</b>",
        history: function (evt, ui) {
            var $grid = $(this);
            if (ui.canUndo != null) {
                $("button.changes", $grid).button("option", { disabled: !ui.canUndo });
            }
            if (ui.canRedo != null) {
                $("button:contains('Redo')", $grid).button("option", "disabled", !ui.canRedo);
            }
            $("button:contains('Undo')", $grid).button("option", { label: 'Undo (' + ui.num_undo + ')' });
            $("button:contains('Redo')", $grid).button("option", { label: 'Redo (' + ui.num_redo + ')' });
        },
        colModel: [
            {
                title: "Instance", dataType: "string", dataIndx: "InstanceName", editable: false, width: 120,
                filter: { type: 'textbox', condition: 'begin', listeners: ['keyup'] }
            },
            {
                title: "Attribute", dataType: "string", dataIndx: "AttributeName", editable: false, width: 120
            },
            {
                title: "IO Tag", dataType: "string", dataIndx: "IOTagName", editable: false, width: 80
            },
            {
                title: "PLC Tag", width: 165, dataType: "string", dataIndx: "PLCTagName",
                //filter: { type: 'textbox', condition: 'begin', listeners: ['keyup'] },
                validations: [
                    { type: 'minLen', value: 1, msg: "Required" },
                    { type: 'maxLen', value: 40, msg: "length should be <= 40" }
                ]
            },
            {
                title: "Rack", width: 50, dataType: "integer", align: "center", dataIndx: "Rack",
                validations: [
                    { type: 'gte', value: 1, msg: "should be >= 0" },
                    { type: 'lte', value: 1000, msg: "should be <= 32" }
                ]
            },
            {
                title: "Slot", width: 50, dataType: "integer", align: "center", dataIndx: "Slot",
                validations: [
                    { type: 'gte', value: 1, msg: "should be >= 0" },
                    { type: 'lte', value: 1000, msg: "should be <= 32" }
                ]
            },
            {
                title: "Point", width: 50, dataType: "integer", align: "center", dataIndx: "Point",
                validations: [
                    { type: 'gte', value: 1, msg: "should be >= 0" },
                    { type: 'lte', value: 1000, msg: "should be <= 32" }
                ]
            }
        ],
        pageModel: { type: "local", rPP: 20 },
        dataModel: {
            dataType: "JSON",
            location: "remote",
            recIndx: "ProductID",
            url: "/pro/products/get", //for ASP.NET
            //url: "/pro/products.php", //for PHP
            getData: function (response) {
                return { data: response.data };
            }
        },
        change: function (evt, ui) {
            //refresh the filter.
            if (ui.source != "add") {
                $grid.pqGrid("filter", { oper: 'add', data: [] });
            }
        },
        refresh: function () {
            $("#grid_editing").find("button.delete_btn").button({ icons: { primary: 'ui-icon-scissors' } })
                .unbind("click")
                .bind("click", function (evt) {
                    var $tr = $(this).closest("tr");
                    var obj = $grid.pqGrid("getRowIndx", { $tr: $tr });
                    var rowIndx = obj.rowIndx;
                    $grid.pqGrid("addClass", { rowIndx: rowIndx, cls: 'pq-row-delete' });

                    var ans = window.confirm("Are you sure to delete row No " + (rowIndx + 1) + "?");
                    $grid.pqGrid("removeClass", { rowIndx: rowIndx, cls: 'pq-row-delete' });
                    if (ans) {
                        $grid.pqGrid("deleteRow", { rowIndx: rowIndx });
                    }
                });
        }
    };
    var $grid = $("#grid_editing").pqGrid(obj);
});