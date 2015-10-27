$(document).ready(function() {
    var checkOneTouchStatus = function() {
        $.post("/Authy/OneTouchStatus")
            .done(function (data) {
                if (data === "approved" || data === "denied") {
                    $("form").get(0).submit();
                } else {
                    setTimeout(checkOneTouchStatus, 2000);
                }
            }
        );
    }

    checkOneTouchStatus();
})