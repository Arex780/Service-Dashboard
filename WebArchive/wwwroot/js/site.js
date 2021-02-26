// Enable multi-line text in textarea input type
var textAreas = document.getElementsByTagName('textarea');
Array.prototype.forEach.call(textAreas, function (elem) {
    elem.placeholder = elem.placeholder.replace(/\\n/g, '\n');
});

/* When the user clicks on the button,
toggle between hiding and showing the dropdown content */
function dropdownToggle(popupid) {
    document.getElementById(popupid).classList.toggle("show");
}

// Close the dropdown menu if the user clicks outside of it
window.onclick = function (event) {
    if (event.target.parentElement == null || !event.target.parentElement.matches('.dropdown-popup')) {
        var dropdowns = document.getElementsByClassName("dropdown-popup-content");
        var i;
        for (i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
}

// Handling button focus
$("div.dropdown-popup-handler").mouseleave(function () {
    if (!($('.dropdown-popup-content').hasClass("show")))
        $('.dropdown-popup').blur();
});

// Match width of thumbnail+user with dropdown menu content
window.onload = function() {
    var parentWidth = document.getElementsByClassName('dropdown-popup-handler')[0].offsetWidth;
    document.getElementsByClassName("dropdown-popup-content")[0].style.minWidth = parentWidth + 'px';
}