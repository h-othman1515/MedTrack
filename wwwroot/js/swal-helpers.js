/* MedTrack — SweetAlert2 helpers (toasts, confirms, TempData) */
(function (window) {
    'use strict';

    var swalReady = typeof Swal !== 'undefined';

    function getTheme() {
        return document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
    }

    function swalThemeOptions(options) {
        options = options || {};
        var isDanger = options.danger === true;
        return {
            customClass: {
                popup: 'mt-swal-popup',
                title: 'mt-swal-title',
                htmlContainer: 'mt-swal-text',
                actions: 'mt-swal-actions',
                confirmButton: isDanger ? 'btn btn-danger px-4 rounded-pill fw-semibold' : 'btn btn-primary px-4 rounded-pill fw-semibold',
                cancelButton: 'btn btn-outline-secondary px-4 rounded-pill fw-semibold ms-2'
            },
            buttonsStyling: false,
            color: getTheme() === 'dark' ? '#e8f1fb' : '#0d1b2a',
            background: getTheme() === 'dark' ? '#111927' : '#ffffff'
        };
    }

    var toastMixin = null;

    function ensureSwal() {
        if (typeof Swal === 'undefined') {
            console.error('MedTrack: SweetAlert2 failed to load. Confirm dialogs may not work.');
            return false;
        }
        if (!toastMixin) {
            toastMixin = Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 4000,
                timerProgressBar: true,
                customClass: { popup: 'mt-swal-toast' },
                didOpen: function (toast) {
                    toast.addEventListener('mouseenter', Swal.stopTimer);
                    toast.addEventListener('mouseleave', Swal.resumeTimer);
                }
            });
        }
        return true;
    }

    function mapType(type) {
        if (type === 'error' || type === 'danger') return 'error';
        if (type === 'success') return 'success';
        if (type === 'warning') return 'warning';
        return 'info';
    }

    window.showToast = function (message, type) {
        if (!ensureSwal()) return;
        toastMixin.fire({
            icon: mapType(type || 'info'),
            title: message
        });
    };

    window.mtConfirm = function (message, options) {
        if (!ensureSwal()) {
            return Promise.resolve(false);
        }

        options = options || {};
        var theme = swalThemeOptions(options);

        return Swal.fire(Object.assign({}, theme, {
            title: options.title || 'Are you sure?',
            text: message,
            icon: options.icon || 'question',
            showCancelButton: true,
            confirmButtonText: options.confirmText || 'Yes',
            cancelButtonText: options.cancelText || 'Cancel',
            reverseButtons: true,
            focusCancel: options.danger === true
        })).then(function (result) {
            return result.isConfirmed;
        });
    };

    window.mtAlert = function (message, options) {
        if (!ensureSwal()) return Promise.resolve();

        options = options || {};
        var theme = swalThemeOptions(options);

        return Swal.fire(Object.assign({}, theme, {
            title: options.title || '',
            text: message,
            icon: options.icon || 'info',
            confirmButtonText: options.confirmText || 'OK'
        }));
    };

    window.mtConfirmDelete = function (itemName) {
        return mtConfirm(
            'Are you sure you want to delete ' + itemName + '? This action cannot be undone.',
            { title: 'Delete ' + itemName + '?', icon: 'warning', danger: true, confirmText: 'Delete' }
        );
    };

    function readBtnOptions(btn) {
        return {
            title: btn.getAttribute('data-mt-swal-title') || 'Are you sure?',
            message: btn.getAttribute('data-mt-swal-message') || 'Continue with this action?',
            icon: btn.getAttribute('data-mt-swal-icon') || 'question',
            danger: btn.getAttribute('data-mt-swal-danger') === 'true',
            confirmText: btn.getAttribute('data-mt-swal-confirm-text') || 'Yes',
            success: btn.getAttribute('data-mt-swal-success'),
            successType: btn.getAttribute('data-mt-swal-success-type') || 'success'
        };
    }

    function initTempDataMessages() {
        var el = document.getElementById('mt-tempdata-json');
        if (!el) return;

        try {
            var data = JSON.parse(el.textContent);
            if (data.success) showToast(data.success, 'success');
            if (data.error) showToast(data.error, 'error');
            if (data.warning) showToast(data.warning, 'warning');
            if (data.info) showToast(data.info, 'info');
        } catch (e) { /* ignore malformed payload */ }
    }

    function initFormConfirms() {
        document.addEventListener('submit', function (e) {
            var form = e.target.closest('form[data-mt-swal-confirm]');
            if (!form || form.dataset.swalConfirmed === '1') return;

            e.preventDefault();
            e.stopPropagation();

            var message = form.getAttribute('data-mt-swal-confirm');
            var icon = form.getAttribute('data-mt-swal-icon') || 'question';
            var danger = form.getAttribute('data-mt-swal-danger') === 'true';
            var confirmText = form.getAttribute('data-mt-swal-confirm-text') || 'Yes';

            mtConfirm(message, {
                title: form.getAttribute('data-mt-swal-title') || 'Are you sure?',
                icon: icon,
                danger: danger,
                confirmText: confirmText
            }).then(function (ok) {
                if (ok) {
                    form.dataset.swalConfirmed = '1';
                    form.requestSubmit ? form.requestSubmit() : form.submit();
                }
            });
        }, true);
    }

    function initButtonConfirms() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-mt-swal-message]');
            if (!btn || btn.disabled) return;

            e.preventDefault();
            e.stopPropagation();

            var opts = readBtnOptions(btn);
            mtConfirm(opts.message, {
                title: opts.title,
                icon: opts.icon,
                danger: opts.danger,
                confirmText: opts.confirmText
            }).then(function (ok) {
                if (!ok) return;

                if (opts.success) {
                    showToast(opts.success, opts.successType);
                }

                var href = btn.getAttribute('data-mt-swal-href');
                if (href) {
                    window.location.href = href;
                    return;
                }

                var form = btn.closest('form');
                if (form) {
                    form.dataset.swalConfirmed = '1';
                    form.requestSubmit ? form.requestSubmit() : form.submit();
                }
            });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        swalReady = ensureSwal();
        initTempDataMessages();
        initFormConfirms();
        initButtonConfirms();
    });

    window.addEventListener('mt-theme-change', function () {
        /* next dialog picks up theme via getTheme() */
    });
})(window);
