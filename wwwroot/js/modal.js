// Focus management for <Modal>. Keeps a stack so nested dialogs restore correctly.
window.__modal = (function () {
    const stack = [];

    const FOCUSABLE = [
        'a[href]',
        'button:not([disabled])',
        'input:not([disabled]):not([type="hidden"])',
        'select:not([disabled])',
        'textarea:not([disabled])',
        '[tabindex]:not([tabindex="-1"])'
    ].join(',');

    function visibleFocusable(root) {
        return Array.prototype.slice
            .call(root.querySelectorAll(FOCUSABLE))
            .filter(function (el) { return el.offsetParent !== null; });
    }

    return {
        activate: function (el) {
            if (!el) return;

            const entry = { el: el, prev: document.activeElement };

            entry.handler = function (e) {
                if (e.key !== 'Tab') return;

                const items = visibleFocusable(el);
                if (items.length === 0) { e.preventDefault(); return; }

                const first = items[0];
                const last = items[items.length - 1];

                if (e.shiftKey && document.activeElement === first) {
                    e.preventDefault();
                    last.focus();
                } else if (!e.shiftKey && document.activeElement === last) {
                    e.preventDefault();
                    first.focus();
                }
            };

            el.addEventListener('keydown', entry.handler);
            stack.push(entry);

            const items = visibleFocusable(el);
            (items.length > 0 ? items[0] : el).focus();
        },

        deactivate: function () {
            const entry = stack.pop();
            if (!entry) return;

            entry.el.removeEventListener('keydown', entry.handler);

            if (entry.prev && document.body.contains(entry.prev)) {
                entry.prev.focus();
            }
        }
    };
})();
