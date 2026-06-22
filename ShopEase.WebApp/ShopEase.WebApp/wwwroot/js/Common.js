/* =============================================
   common.js — ShopEase Web App
   Shared utilities for all pages
   ============================================= */

'use strict';

/* ──────────────────────────────────────────────
   1. TOASTR CONFIGURATION
────────────────────────────────────────────── */
toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: 'toast-top-right',
    timeOut: 3500,
    extendedTimeOut: 1000,
    showEasing: 'swing',
    hideEasing: 'linear',
    showMethod: 'fadeIn',
    hideMethod: 'fadeOut'
};

function showSuccess(message) { toastr.success(message || 'Operation successful'); }
function showError(message) { toastr.error(message || 'Something went wrong'); }
function showWarning(message) { toastr.warning(message || 'Warning'); }
function showInfo(message) { toastr.info(message || 'Info'); }


/* ──────────────────────────────────────────────
   2. SIDEBAR TOGGLE
────────────────────────────────────────────── */
$(function () {
    var $sidebar = $('#sidebar');
    var $overlay = $('#sidebarOverlay');
    var $toggle = $('#sidebarToggle');
    var $close = $('#sidebarClose');

    function openSidebar() {
        $sidebar.addClass('open');
        $overlay.addClass('open');
        $('body').css('overflow', 'hidden');
    }

    function closeSidebar() {
        $sidebar.removeClass('open');
        $overlay.removeClass('open');
        $('body').css('overflow', '');
    }

    $toggle.on('click', openSidebar);
    $close.on('click', closeSidebar);
    $overlay.on('click', closeSidebar);
});


/* ──────────────────────────────────────────────
   3. USER DROPDOWN
────────────────────────────────────────────── */
$(function () {
    var $btn = $('#userDropBtn');
    var $drop = $('#userDrop');

    $btn.on('click', function (e) {
        e.stopPropagation();
        $drop.toggleClass('open');
    });

    $(document).on('click', function () {
        $drop.removeClass('open');
    });
});


/* ──────────────────────────────────────────────
   4. MODAL  — loadPopup(url, title, size)
   size: 'sm' | 'md' (default) | 'lg' | 'xl'
────────────────────────────────────────────── */
var _modalStack = [];

function loadPopup(url, title, size) {
    var $backdrop = $('#modalBackdrop');
    var $container = $('#modalContainer');
    var $box = $('#modalBox');
    var $title = $('#modalTitle');
    var $body = $('#modalBody');

    // Remove previous size classes
    $box.removeClass('modal-sm modal-lg modal-xl');
    if (size === 'sm') $box.addClass('modal-sm');
    else if (size === 'lg') $box.addClass('modal-lg');
    else if (size === 'xl') $box.addClass('modal-xl');

    $title.text(title || '');
    $body.html('<div class="spinner"></div>');

    $backdrop.addClass('open');
    $container.addClass('open');

    $.ajax({
        url: url,
        type: 'GET',
        success: function (html) {
            $body.html(html);
            // Re-init any selects or datepickers inside modal
            initFormControls($body);
        },
        error: function () {
            $body.html('<p class="text-danger" style="padding:20px">Failed to load content.</p>');
        }
    });
}

function closePopup() {
    $('#modalBackdrop').removeClass('open');
    $('#modalContainer').removeClass('open');
    $('#modalBody').html('');
}

// Close on backdrop click
$('#modalBackdrop').on('click', closePopup);

// Close on X button
$('#modalClose').on('click', closePopup);

// Close on Escape key
$(document).on('keydown', function (e) {
    if (e.key === 'Escape') closePopup();
});


/* ──────────────────────────────────────────────
   5. LOAD DROPDOWN
   loadDropdown(url, controlId, prefillValue, defaultSelect)
   
   API response expected: [{ id, name, code }]
────────────────────────────────────────────── */
function loadDropdown(url, controlId, prefillValue, defaultSelect) {
    var $select = $('#' + controlId);

    if (!$select.length) {
        console.warn('loadDropdown: #' + controlId + ' not found');
        return;
    }

    $select.prop('disabled', true).html('<option value="">Loading...</option>');

    var accessToken = getAccessToken();

    $.ajax({
        url: url,
        type: 'GET',
        headers: accessToken ? { 'Authorization': 'Bearer ' + accessToken } : {},
        success: function (response) {
            $select.empty();

            // Default placeholder option
            var defaultText = defaultSelect || '— Select —';
            $select.append($('<option>', { value: '', text: defaultText }));

            var data = [];

            // Handle both raw array and wrapped ApiResponse
            if (Array.isArray(response)) {
                data = response;
            } else if (response && response.status && Array.isArray(response.response)) {
                data = response.response;
            }

            $.each(data, function (i, item) {
                var id = item.id || item.Id || item.value || item.Value || '';
                var name = item.name || item.Name || item.text || item.Text || '';

                var $opt = $('<option>', { value: id, text: name });
                $opt.data('code', item.code || item.Code || '');
                $opt.data('item', item);

                if (prefillValue !== null && prefillValue !== undefined && String(id) === String(prefillValue)) {
                    $opt.prop('selected', true);
                }

                $select.append($opt);
            });

            $select.prop('disabled', false);
            $select.trigger('dropdown:loaded');
        },
        error: function (xhr) {
            $select.html('<option value="">Failed to load</option>').prop('disabled', false);
            console.error('loadDropdown error:', xhr.status, url);
        }
    });
}

// Get selected item's code from dropdown
function getDropdownCode(controlId) {
    var $selected = $('#' + controlId + ' option:selected');
    return $selected.data('code') || '';
}

// Get full item object from selected option
function getDropdownItem(controlId) {
    var $selected = $('#' + controlId + ' option:selected');
    return $selected.data('item') || null;
}


/* ──────────────────────────────────────────────
   6. ACCESS TOKEN HELPER
   Reads from session via hidden meta tag in layout
   Add this in _Layout.cshtml inside <head>:
   <meta name="access-token" content="@(HttpContext.Session.GetString("AccessToken") ?? "")" />
────────────────────────────────────────────── */
function getAccessToken() {
    return $('meta[name="access-token"]').attr('content') || '';
}


/* ──────────────────────────────────────────────
   7. AJAX POST HELPER
   ajaxPost(url, data, successFn, errorFn)
────────────────────────────────────────────── */
function ajaxPost(url, data, successFn, errorFn) {
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        headers: { 'RequestVerificationToken': token },
        success: function (response) {
            if (typeof successFn === 'function') successFn(response);
        },
        error: function (xhr) {
            if (typeof errorFn === 'function') {
                errorFn(xhr);
            } else {
                showError('Request failed. Please try again.');
            }
        }
    });
}


/* ──────────────────────────────────────────────
   8. AJAX POST JSON HELPER
   ajaxPostJson(url, jsonData, successFn, errorFn)
────────────────────────────────────────────── */
function ajaxPostJson(url, jsonData, successFn, errorFn) {
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(jsonData),
        headers: { 'RequestVerificationToken': token },
        success: function (response) {
            if (typeof successFn === 'function') successFn(response);
        },
        error: function (xhr) {
            if (typeof errorFn === 'function') {
                errorFn(xhr);
            } else {
                showError('Request failed. Please try again.');
            }
        }
    });
}


/* ──────────────────────────────────────────────
   9. CONFIRM DELETE — showConfirm(message, onConfirm)
────────────────────────────────────────────── */
function showConfirm(message, onConfirm) {
    var $backdrop = $('#modalBackdrop');
    var $container = $('#modalContainer');
    var $box = $('#modalBox');
    var $title = $('#modalTitle');
    var $body = $('#modalBody');

    $box.removeClass('modal-sm modal-lg modal-xl').addClass('modal-sm');
    $title.text('Confirm');
    $body.html(
        '<p style="margin-bottom:20px;color:#444">' + (message || 'Are you sure you want to delete this record?') + '</p>' +
        '<div style="display:flex;justify-content:flex-end;gap:8px">' +
        '  <button class="btn btn-secondary btn-sm" onclick="closePopup()">Cancel</button>' +
        '  <button class="btn btn-danger btn-sm" id="confirmOkBtn">Delete</button>' +
        '</div>'
    );

    $backdrop.addClass('open');
    $container.addClass('open');

    $('#confirmOkBtn').off('click').on('click', function () {
        closePopup();
        if (typeof onConfirm === 'function') onConfirm();
    });
}


/* ──────────────────────────────────────────────
   10. PARTIAL VIEW RELOAD — reloadPartial(url, containerId)
────────────────────────────────────────────── */
function reloadPartial(url, containerId) {
    var $container = $('#' + containerId);
    $container.html('<div class="spinner"></div>');

    $.ajax({
        url: url,
        type: 'GET',
        success: function (html) {
            $container.html(html);
        },
        error: function () {
            $container.html('<p class="text-danger">Failed to reload.</p>');
        }
    });
}


/* ──────────────────────────────────────────────
   11. FORM HELPERS
────────────────────────────────────────────── */

// Serialize form to plain JS object
function serializeForm(formId) {
    var data = {};
    $('#' + formId).serializeArray().forEach(function (item) {
        data[item.name] = item.value;
    });
    return data;
}

// Reset form fields and clear validation errors
function resetForm(formId) {
    var $form = $('#' + formId);
    $form[0].reset();
    $form.find('.is-invalid').removeClass('is-invalid');
    $form.find('.field-error').text('');
}

// Show server-side validation errors on fields
function showValidationErrors(errors) {
    if (!errors || !Array.isArray(errors)) return;
    errors.forEach(function (err) {
        var $input = $('[name="' + err.propertyName + '"]');
        $input.addClass('is-invalid');
        $input.siblings('.field-error').text(err.errorMessage);
    });
}

// Init form controls (called after modal loads)
function initFormControls($context) {
    // Placeholder for datepicker, select2 etc. inits
    // e.g. $context.find('.datepicker').datepicker();
}


/* ──────────────────────────────────────────────
   12. BUTTON LOADING STATE
────────────────────────────────────────────── */
function setButtonLoading($btn, loading) {
    if (loading) {
        $btn.prop('disabled', true)
            .find('.btn-text').addClass('hidden').end()
            .find('.btn-spinner').removeClass('hidden');
    } else {
        $btn.prop('disabled', false)
            .find('.btn-text').removeClass('hidden').end()
            .find('.btn-spinner').addClass('hidden');
    }
}


/* ──────────────────────────────────────────────
   13. HANDLE API RESPONSE (standard pattern)
   Usage:
     var result = handleApiResponse(response);
     if (!result) return; // failed, toast already shown
────────────────────────────────────────────── */
function handleApiResponse(response, successMessage) {
    if (!response) {
        showError('No response from server.');
        return null;
    }
    if (!response.status) {
        showError(response.message || 'Operation failed.');
        return null;
    }
    if (successMessage) showSuccess(successMessage);
    return response.response;
}


/* ──────────────────────────────────────────────
   14. FORMAT HELPERS
────────────────────────────────────────────── */
function formatDate(dateStr) {
    if (!dateStr) return '—';
    var d = new Date(dateStr);
    if (isNaN(d)) return dateStr;
    return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}

function formatCurrency(amount, currency) {
    if (amount === null || amount === undefined) return '—';
    return new Intl.NumberFormat('en-IN', {
        style: 'currency',
        currency: currency || 'INR',
        minimumFractionDigits: 2
    }).format(amount);
}

function formatNumber(num) {
    if (num === null || num === undefined) return '—';
    return new Intl.NumberFormat('en-IN').format(num);
}


/* ──────────────────────────────────────────────
   15. AUTH.JS HELPERS (password toggle + strength)
────────────────────────────────────────────── */
function togglePassword(inputId, btn) {
    var $input = $('#' + inputId);
    var isText = $input.attr('type') === 'text';
    $input.attr('type', isText ? 'password' : 'text');
    // Swap icon
    $(btn).find('svg').toggle();
}

function updateStrength(val) {
    var score = 0;
    if (val.length >= 8) score++;
    if (/[A-Z]/.test(val)) score++;
    if (/[0-9]/.test(val)) score++;
    if (/[^A-Za-z0-9]/.test(val)) score++;

    var colors = ['', '#e74c3c', '#e67e22', '#1a7a3c', '#534AB7'];
    var labels = ['', 'Weak', 'Fair', 'Good', 'Strong'];

    for (var i = 1; i <= 4; i++) {
        var $seg = $('#s' + i);
        if (i <= score) {
            $seg.css('background', colors[score]);
        } else {
            $seg.css('background', '#eee');
        }
    }

    var $label = $('#strengthLabel');
    if ($label.length) {
        $label.text(val.length ? labels[score] : '').css('color', colors[score]);
    }
}

function googleSignIn() {
    // Wire up your Google OAuth flow here
    showInfo('Google Sign-In coming soon.');
}