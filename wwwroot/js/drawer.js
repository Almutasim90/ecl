// drawer.js — sidebar toggle for mobile (<768px overlay mode)
// On tablet and up (≥768px) the sidebar is in the normal flex flow with a
// collapse toggle in the topbar; this script only activates the overlay
// drawer on phone-sized viewports.

const STORAGE_KEY = 'ecl.drawer.open';
const DESKTOP_BP  = 768; // px — must match the CSS media query breakpoint

function isDesktop() {
  return window.matchMedia(`(min-width: ${DESKTOP_BP}px)`).matches;
}

function setDrawerOpen(open, { save = true } = {}) {
  const body     = document.body;
  const toggle   = document.getElementById('drawer-toggle');

  if (open) {
    body.classList.add('drawer-open');
    if (toggle) {
      toggle.setAttribute('aria-expanded', 'true');
      toggle.querySelector('i')?.classList.replace('bi-list', 'bi-x');
    }
  } else {
    body.classList.remove('drawer-open');
    if (toggle) {
      toggle.setAttribute('aria-expanded', 'false');
      toggle.querySelector('i')?.classList.replace('bi-x', 'bi-list');
    }
  }

  if (save && !isDesktop()) {
    try { localStorage.setItem(STORAGE_KEY, open ? '1' : '0'); } catch (e) { /* ignore */ }
  }
}

function toggleDrawer() {
  setDrawerOpen(!document.body.classList.contains('drawer-open'));
}

function initDrawer() {
  const toggle   = document.getElementById('drawer-toggle');
  const sidebar  = document.getElementById('sidebar');
  const backdrop = document.getElementById('sidebar-backdrop');

  if (!toggle || !sidebar) return;

  // Hamburger click
  toggle.addEventListener('click', (e) => {
    e.stopPropagation();
    toggleDrawer();
  });

  // Backdrop click closes drawer
  if (backdrop) {
    backdrop.addEventListener('click', () => setDrawerOpen(false));
  }

  // Close the drawer after navigation (mobile/tablet only)
  sidebar.addEventListener('click', (e) => {
    if (isDesktop()) return;
    const target = e.target instanceof Element ? e.target.closest('a') : null;
    if (target) setDrawerOpen(false);
  });

  // Escape key closes drawer
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && document.body.classList.contains('drawer-open')) {
      setDrawerOpen(false);
    }
  });

  // Initial state
  if (isDesktop()) {
    // Desktop: ensure drawer-open class is removed (sidebar is always visible via CSS)
    body_classList_remove_drawer_open();
  } else {
    // Mobile/tablet: restore saved state, default closed
    let saved = null;
    try { saved = localStorage.getItem(STORAGE_KEY); } catch (e) { /* ignore */ }
    setDrawerOpen(saved === '1', { save: false });
  }

  // Handle browser resize across the breakpoint
  let wasDesktop = isDesktop();
  window.addEventListener('resize', () => {
    const nowDesktop = isDesktop();
    if (nowDesktop === wasDesktop) return;
    wasDesktop = nowDesktop;

    if (nowDesktop) {
      // Switched to desktop — remove overlay state; CSS makes sidebar always visible
      setDrawerOpen(false, { save: false });
    } else {
      // Switched to mobile/tablet — restore saved state, default closed
      let saved = null;
      try { saved = localStorage.getItem(STORAGE_KEY); } catch (e) { /* ignore */ }
      setDrawerOpen(saved === '1', { save: false });
    }
  });
}

function body_classList_remove_drawer_open() {
  document.body.classList.remove('drawer-open');
  const toggle = document.getElementById('drawer-toggle');
  if (toggle) {
    toggle.setAttribute('aria-expanded', 'false');
    const icon = toggle.querySelector('i');
    if (icon) { icon.classList.remove('bi-x'); icon.classList.add('bi-list'); }
  }
}

// ── Desktop sidebar collapse ───────────────────────────────────────────────
const COLLAPSE_KEY = 'ecl.sidebar.collapsed'; 

function initDesktopCollapse() {
  const btn = document.getElementById('sidebar-desktop-toggle');
  if (!btn) return;

  // Restore saved state
  let collapsed = false;
  try { collapsed = localStorage.getItem(COLLAPSE_KEY) === '1'; } catch (e) { /* ignore */ }
  if (collapsed) document.body.classList.add('sidebar-collapsed');

  btn.addEventListener('click', () => {
    const isCollapsed = document.body.classList.toggle('sidebar-collapsed');
    try { localStorage.setItem(COLLAPSE_KEY, isCollapsed ? '1' : '0'); } catch (e) { /* ignore */ }
  });

  // Clear collapsed state when resizing to mobile (overlay mode takes over)
  window.addEventListener('resize', () => {
    if (!isDesktop()) document.body.classList.remove('sidebar-collapsed');
  });
}

// ── Active link highlighting ───────────────────────────────────────────────
// Adds `.active` to the .slink whose href best matches the current URL,
// preferring an exact path match, then a controller-segment match.
function initActiveLink() {
  const links = document.querySelectorAll('.sidebar a.slink');
  if (!links.length) return;

  const currentPath = (location.pathname || '/').replace(/\/+$/, '') || '/';
  const currentSeg  = currentPath.split('/').filter(Boolean)[0] || '';

  let exact = null;
  let segment = null;

  links.forEach((a) => {
    const href = (a.getAttribute('href') || '').replace(/\/+$/, '') || '/';
    if (href === currentPath) exact = a;
    const seg = href.split('/').filter(Boolean)[0] || '';
    if (!segment && seg && seg.toLowerCase() === currentSeg.toLowerCase()) {
      segment = a;
    }
  });

  const winner = exact || segment;
  if (winner) winner.classList.add('active');
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => {
    initDrawer();
    initDesktopCollapse();
    initActiveLink();
  });
} else {
  initDrawer();
  initDesktopCollapse();
  initActiveLink();
}
