/* MedTrack — global theme toggle */
(function (window) {
    'use strict';

    if (window.__mtThemeInit) return;
    window.__mtThemeInit = true;

    function applyTheme(theme) {
        var html = document.documentElement;
        var next = theme === 'dark' ? 'dark' : 'light';
        html.setAttribute('data-theme', next);
        html.setAttribute('data-bs-theme', next);
        localStorage.setItem('mt-theme', next);
        document.querySelectorAll('[data-mt-theme-toggle]').forEach(function (btn) {
            btn.setAttribute('aria-pressed', next === 'dark' ? 'true' : 'false');
        });
        return next;
    }

    function toggleTheme() {
        var current = document.documentElement.getAttribute('data-theme') || 'light';
        var next = applyTheme(current === 'dark' ? 'light' : 'dark');
        window.dispatchEvent(new CustomEvent('mt-theme-change', { detail: { theme: next } }));
    }

    function bind() {
        applyTheme(localStorage.getItem('mt-theme') || 'light');
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.theme-toggle, [data-mt-theme-toggle]');
            if (!btn) return;
            e.preventDefault();
            e.stopPropagation();
            toggleTheme();
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bind);
    } else {
        bind();
    }

    window.mtApplyTheme = applyTheme;
    window.mtToggleTheme = toggleTheme;
})(window);
