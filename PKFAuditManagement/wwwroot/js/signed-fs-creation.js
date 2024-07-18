
$(function () {
    $("#autocomplete").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: '/Tags/GetTags',
                type: 'GET',
                success: function (data) {
                    var matcher = new RegExp("^" + $.ui.autocomplete.escapeRegex(request.term), "i");
                    response($.grep(data, function (item) {
                        return matcher.test(item);
                    }));
                }
            });
        }
    });
});