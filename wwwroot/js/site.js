// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Dark mode toggle: persists the chosen theme in localStorage and flips Bootstrap's data-bs-theme attribute.
(function () {
    var toggle = document.getElementById('darkModeToggle');
    if (!toggle) {
        return;
    }

    function updateIcon() {
        var isDark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
        toggle.innerHTML = isDark ? '<i class="fa-solid fa-sun"></i>' : '<i class="fa-solid fa-moon"></i>';
    }

    updateIcon();

    toggle.addEventListener('click', function () {
        var current = document.documentElement.getAttribute('data-bs-theme');
        var next = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-bs-theme', next);
        localStorage.setItem('theme', next);
        updateIcon();
    });
})();
