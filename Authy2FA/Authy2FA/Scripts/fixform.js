setTimeout(function () {
    // Authy form helpers clobber bootstrap styles, need to add them back
    $('.countries-input').addClass('form-control');
},200);