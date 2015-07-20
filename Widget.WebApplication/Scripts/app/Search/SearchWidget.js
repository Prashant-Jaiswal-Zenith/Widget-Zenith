/*!
 * Search Widget JS Handler
 *
 */

var SearchWidgetModule = (function () {
    /* JQuery UI DatePicker begin */
    var DatePickerHandler = (function () {
        var resetCheckOutDate = function () {
            var dt1 = new Date();
            var dt2 = jq214('#CheckIn').datepicker('getDate');

            var datediff = Math.floor((dt2 - dt1) / 86400000) + 2;

            jq214('#CheckOut').datepicker("destroy");

            jq214('#CheckOut').datepicker({
                numberOfMonths: 2,
                autoSize: true,
                changeMonth: true,
                changeYear: true,
                duration: "slow",
                gotoCurrent: true,
                showAnim: "fold",
                dateFormat: jq214('#CheckOut').data('format'),
                minDate: +datediff,
                onSelect: checkCheckInDate
            });

            if (jq214('#CheckIn').val() == "") {
                jq214('#CheckOut').datepicker('setDate', +datediff);
            } else if (jq214('#CheckOut').datepicker('getDate') <= jq214('#CheckIn').datepicker('getDate')) {
                jq214('#CheckOut').datepicker('setDate', +datediff);
            }
        }

        var checkCheckInDate = function () {
            if (jq214('#CheckIn').val() == "") {
                var dt1 = new Date();
                var dt2 = jq214('#CheckOut').datepicker('getDate');
                var datediff = Math.floor((dt2 - dt1) / 86400000);

                jq214('#CheckIn').datepicker('setDate', +datediff);

                resetCheckOutDate();
            }
        }

        var _initDateControls = function () {
            jq214.validator.methods["date"] = function (value, element) { return true; }

            jq214(".date-picker").bind("keydown", function (e) {
                e.preventDefault();
                e.stopImmediatePropagation();
                return false;
            });

            jq214('#CheckIn').datepicker({
                numberOfMonths: 2,
                autoSize: true,
                changeMonth: true,
                changeYear: true,
                duration: "slow",
                gotoCurrent: true,
                showAnim: "fold",
                dateFormat: jq214('#CheckIn').data('format'),
                minDate: +1,
                onSelect: resetCheckOutDate
            });

            jq214('#CheckOut').datepicker({
                numberOfMonths: 2,
                autoSize: true,
                changeMonth: true,
                changeYear: true,
                duration: "slow",
                gotoCurrent: true,
                showAnim: "fold",
                dateFormat: jq214('#CheckOut').data('format'),
                minDate: +2,
                onSelect: checkCheckInDate
            });
        }

        return { //exposed to public
            initializeDateControls: _initDateControls,
        }
    }());
    /* JQuery UI DatePicker end */

    var RoomsSelectionHandler = (function () {
        var _roomSelectionChange = function () {
            var start = jq214("#ddlSearchWidgetRooms option:selected").val();

            jq214('.roomSelection li:lt(' + start + ')').show();
            jq214('.roomSelection li:gt(' + (start - 1) + ')').hide();
        }

        var _roomAdultSelectionChange = function (roomNo) {
            var adults = jq214("#ddlSearchWidgetAdult" + roomNo + " option:selected").val();
            var length = jq214("#ddlSearchWidgetAdult" + roomNo + " option").length;
            var childCount = length - adults;

            jq214('#ddlSearchWidgetChild' + roomNo + ' >option').remove();
            for (i = 0; i <= childCount; i++) {
                jq214('#ddlSearchWidgetChild' + roomNo).append("<option value= " + i + ">" + i + "</option>");
            }
        }

        return { //exposed to public
            SelectionChange: _roomSelectionChange,

            AdultSelectionChange: _roomAdultSelectionChange
        }
    }());

    var PageControlsHandler = (function () {
        var _dropdownChange = function () {
            jq214('#ddlSearchWidgetRooms').change(RoomsSelectionHandler.SelectionChange);
            RoomsSelectionHandler.SelectionChange();
        }

        var _initialize = function () {
            _dropdownChange();
        }

        return { //exposed to public
            initialize: _initialize,
        }
    }());

    var _initialize = function () {
        setTimeout(function () {
            jq214(document).on('invalid-form.validate', 'form', function () {
                var button = jq214(this).find('input[type="submit"]');
                setTimeout(function () {
                    button.removeAttr('disabled');
                }, 1);
            });

            jq214(document).on('submit', 'form', function () {
                var button = jq214(this).find('input[type="submit"]');
                setTimeout(function () {
                    button.attr('disabled', 'disabled');
                }, 0);
            });

            try {
                DatePickerHandler.initializeDateControls();
            } catch (err) {

            } finally {

            }

            try {
                PageControlsHandler.initialize();
            } catch (err) {

            } finally {

            }
        }, 50);
    }

    var _adultSelectionChange = function (roomNo) {
        RoomsSelectionHandler.AdultSelectionChange(roomNo);
    }

    return { //exposed to public
        init: _initialize,

        roomAdultSelectionChange: _adultSelectionChange
    }
}());

jq214(window).load(function () { SearchWidgetModule.init(); });