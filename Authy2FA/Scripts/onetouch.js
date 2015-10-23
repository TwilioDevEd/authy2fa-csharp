$(document).ready(function() {
    var checkOneTouchStatus = function() {
        $.post("/Authy/OneTouchStatus", function (data) {
            if (data === "approved") {
                window.location.href = "/";
            } else if (data === "denied") {
                window.location.href = "/Account/Login";
            } else {
                // Probably a better way to implement this is using web sockets.
                setTimeout(checkOneTouchStatus, 2000);
            }
        });
    }

    checkOneTouchStatus();
})