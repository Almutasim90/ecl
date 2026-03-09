// drawer.js - modular drawer toggle
// Features:
// - Toggles `body.drawer-open` to show/hide the left drawer
// - Updates ARIA attributes on the toggle button
// - Persists state in localStorage (optional)
// - Ensures responsive default (closed on small screens)

const STORAGE_KEY = 'ecl.drawer.open';

function isMobile() {
  return window.matchMedia('(max-width: 767.98px)').matches;
}

function setDrawerOpen(open, { save = true } = {}) {
  const body = document.body;
  const toggle = document.getElementById('drawer-toggle');
  if (!toggle) return;
  if (open) {
    body.classList.add('drawer-open');
    toggle.setAttribute('aria-expanded', 'true');
    toggle.querySelector('i')?.classList.replace('bi-list', 'bi-x');
  } else {
    body.classList.remove('drawer-open');
    toggle.setAttribute('aria-expanded', 'false');
    toggle.querySelector('i')?.classList.replace('bi-x', 'bi-list');
  }
  if (save) {
    try { localStorage.setItem(STORAGE_KEY, open ? '1' : '0'); } catch (e) { }
  }
}

function toggleDrawer() {
  const isOpen = document.body.classList.contains('drawer-open');
  setDrawerOpen(!isOpen);
}

function initDrawer() {
  const toggle = document.getElementById('drawer-toggle');
  const sidebar = document.getElementById('sidebar');
  if (!toggle || !sidebar) return;

  // Click handler
  toggle.addEventListener('click', (e) => {
    e.stopPropagation();
    toggleDrawer();
  });

  // Clicking outside closes the drawer on mobile
  document.addEventListener('click', (e) => {
    if (!document.body.classList.contains('drawer-open')) return;
    if (!isMobile()) return; // only for mobile overlay
    if (!sidebar.contains(e.target) && !toggle.contains(e.target)) {
      setDrawerOpen(false);
    }
  });

  // Escape key closes
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && document.body.classList.contains('drawer-open')) {
      setDrawerOpen(false);
    }
  });

  // Initialize state: prefer stored state on desktop; close on mobile by default
  let open = null;
  try { open = localStorage.getItem(STORAGE_KEY); } catch (e) { /* ignore */ }
  if (isMobile()) {
    setDrawerOpen(false, { save: false });
  } else if (open !== null) {
    setDrawerOpen(open === '1', { save: false });
  } else {
    // default open on desktop
    setDrawerOpen(true, { save: false });
  }

  // Keep drawer state reasonable when resizing
  let lastIsMobile = isMobile();
  window.addEventListener('resize', () => {
    const nowIsMobile = isMobile();
    if (nowIsMobile !== lastIsMobile) {
      if (nowIsMobile) {
        setDrawerOpen(false, { save: false });
      } else {
        // restore saved state or open by default on larger screens
        let saved = null;
        try { saved = localStorage.getItem(STORAGE_KEY); } catch (e) { }
        if (saved !== null) setDrawerOpen(saved === '1', { save: false });
        else setDrawerOpen(true, { save: false });
      }
      lastIsMobile = nowIsMobile;
    }
  });
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initDrawer);
} else {
  initDrawer();
}
