(function () {
    var key = 'library-theme';
    var root = document.documentElement;
    var saved = null;
    try { saved = localStorage.getItem(key); } catch (e) { }
    var theme = saved || (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    root.setAttribute('data-theme', theme);

    window.libraryTheme = {
        get: function () {
            return root.getAttribute('data-theme') || 'light';
        },
        toggle: function () {
            var next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            root.setAttribute('data-theme', next);
            try { localStorage.setItem(key, next); } catch (e) { }
            return next;
        }
    };
})();
