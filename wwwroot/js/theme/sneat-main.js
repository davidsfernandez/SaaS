/**
 * Sneat Main JS
 * 100% English comments
 */

'use strict';

let menu, animate;

(function () {
  // Initialize menu
  // --------------------------------------------------------------------

  let layoutMenuEl = document.querySelectorAll('#layout-menu');
  layoutMenuEl.forEach(function (element) {
    // Menu initialization logic would go here if using a menu library
    // For now, we'll handle basic toggles
  });

  // Layout Toggle
  // --------------------------------------------------------------------

  let menuToggler = document.querySelectorAll('.layout-menu-toggle');
  menuToggler.forEach(item => {
    item.addEventListener('click', event => {
      event.preventDefault();
      window.Helpers.toggleCollapsed();
    });
  });

  // Display menu helper
  // --------------------------------------------------------------------

  // Helper object to manage layout states
  window.Helpers = {
    // Check if menu is collapsed
    isCollapsed: function () {
      return document.getElementById('html-tag').classList.contains('layout-menu-collapsed');
    },

    // Toggle menu collapse
    toggleCollapsed: function () {
      const htmlTag = document.documentElement;
      if (htmlTag.classList.contains('layout-menu-collapsed')) {
        htmlTag.classList.remove('layout-menu-collapsed');
        localStorage.setItem('templateCustomizer-vertical-menu-collapsed', 'false');
      } else {
        htmlTag.classList.add('layout-menu-collapsed');
        localStorage.setItem('templateCustomizer-vertical-menu-collapsed', 'true');
      }
    },

    // Initialize layout state from local storage
    init: function () {
      const isCollapsed = localStorage.getItem('templateCustomizer-vertical-menu-collapsed') === 'true';
      if (isCollapsed) {
        document.documentElement.classList.add('layout-menu-collapsed');
      }
    }
  };

  // Initialize on load
  window.Helpers.init();

  // Tooltip & Popover
  // --------------------------------------------------------------------
  const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
  tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });

  const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
  popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl);
  });
})();
