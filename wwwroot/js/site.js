const THEME_KEY = 'ecl.theme';

function getSystemTheme() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function getStoredTheme() {
    try {
        const value = localStorage.getItem(THEME_KEY);
        return value === 'light' || value === 'dark' ? value : null;
    } catch (_) {
        return null;
    }
}

function setTheme(theme, { persist = true } = {}) {
    document.documentElement.setAttribute('data-theme', theme);

    if (persist) {
        try { localStorage.setItem(THEME_KEY, theme); } catch (_) { }
    }

    const toggle = document.getElementById('theme-toggle');
    const icon = document.getElementById('theme-toggle-icon');
    const label = document.getElementById('theme-toggle-label');
    if (!toggle || !icon || !label) return;

    const isDark = theme === 'dark';
    icon.className = isDark ? 'bi bi-sun' : 'bi bi-moon-stars';
    label.textContent = isDark ? 'Light' : 'Dark';
    toggle.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
    toggle.setAttribute('title', isDark ? 'Switch to light mode' : 'Switch to dark mode');
}

document.addEventListener('DOMContentLoaded', function () {
    const toggle = document.getElementById('theme-toggle');
    const hasStored = !!getStoredTheme();
    const currentTheme = document.documentElement.getAttribute('data-theme') || getSystemTheme();

    setTheme(currentTheme, { persist: hasStored });

    if (toggle) {
        toggle.addEventListener('click', function () {
            const active = document.documentElement.getAttribute('data-theme') || 'light';
            const next = active === 'dark' ? 'light' : 'dark';
            setTheme(next, { persist: true });
        });
    }

    const media = window.matchMedia('(prefers-color-scheme: dark)');
    media.addEventListener('change', function () {
        if (getStoredTheme()) return;
        setTheme(getSystemTheme(), { persist: false });
    });
});
