$(document).ready(function () {
    if (typeof toastr !== "undefined") {
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
    }
});

function showSuccess(message) { toastr.success(message || 'Operation successful'); }
function showError(message) { toastr.error(message || 'Something went wrong'); }
function showWarning(message) { toastr.warning(message || 'Warning'); }
function showInfo(message) { toastr.info(message || 'Info'); }

$(function () {
    $(document).on('click', '#sidebarToggle', function () {
        $('#sidebar').addClass('open');
        $('#sidebarOverlay').addClass('open');
        $('body').css('overflow', 'hidden');
    });

    $(document).on('click', '#sidebarClose, #sidebarOverlay', function () {
        $('#sidebar').removeClass('open');
        $('#sidebarOverlay').removeClass('open');
        $('body').css('overflow', '');
    });
});

$(function () {
    $(document).on('click', '#userDropBtn', function (e) {
        e.stopPropagation();
        $('#userDrop').toggleClass('open');
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#userDropWrap').length) {
            $('#userDrop').removeClass('open');
        }
    });
});

var _modalStack = [];

function loadPopup(url, title, size) {
    var $backdrop = $('#modalBackdrop');
    var $container = $('#modalContainer');
    var $box = $('#modalBox');
    var $title = $('#modalTitle');
    var $body = $('#modalBody');

    $box.removeClass('modal-sm modal-lg modal-xl');

    if (size === 'sm') {
        $box.addClass('modal-sm');
    } else if (size === 'md') {
    } else if (size === 'lg') {
        $box.addClass('modal-lg');
    } else if (size === 'xl') {
        $box.addClass('modal-xl');
    }

    $title.text(title || '');
    $body.html('<div class="spinner"></div>');

    $backdrop.addClass('open');
    $container.addClass('open');

    $.ajax({
        url: url,
        type: 'GET',
        success: function (html) {
            $body.html(html);
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

$(document).on('click', '#modalBackdrop', closePopup);
$(document).on('click', '#modalClose', closePopup);

$(document).on('keydown', function (e) {
    if (e.key === 'Escape') closePopup();
});

function loadDropdown(url, controlId, prefillValue, defaultSelect) {
    var $select = $('#' + controlId);

    if (!$select.length) {
        return;
    }

    $select.prop('disabled', true).html('<option value="">Loading...</option>');

    var accessToken = getAccessToken();

    $.ajax({
        url: url,
        type: 'GET',
        dataType: 'json',
        headers: accessToken ? { 'Authorization': 'Bearer ' + accessToken } : {},
        success: function (response) {
            if (typeof response === 'string') {
                try {
                    response = JSON.parse(response);
                } catch (e) {
                    response = [];
                }
            }

            $select.empty();
            var defaultText = defaultSelect || '— Select —';
            $select.append($('<option>', { value: '', text: defaultText }));

            var data = [];

            if (Array.isArray(response)) {
                data = response;
            } else if (response && Array.isArray(response.lookupData)) {
                data = response.lookupData;
            } else if (response && Array.isArray(response.response)) {
                data = response.response;
            } else if (response && Array.isArray(response.data)) {
                data = response.data;
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
        }
    });
}

function getDropdownCode(controlId) {
    var $selected = $('#' + controlId + ' option:selected');
    return $selected.data('code') || '';
}
function getDropdownItem(controlId) {
    var $selected = $('#' + controlId + ' option:selected');
    return $selected.data('item') || null;
}

function getAccessToken() {
    return $('meta[name="access-token"]').attr('content') || '';
}

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

function serializeForm(formId) {
    var data = {};
    $('#' + formId).serializeArray().forEach(function (item) {
        data[item.name] = item.value;
    });
    return data;
}

function resetForm(formId) {
    var $form = $('#' + formId);
    $form[0].reset();
    $form.find('.is-invalid').removeClass('is-invalid');
    $form.find('.field-error').text('');
}
function initFormControls($context) {
}
function setButtonLoading($btn, loading) {
    if (loading) {
        $btn.prop('disabled', true);
        $btn.find('.btn-spinner').removeClass('hidden');
    } else {
        $btn.prop('disabled', false);
        $btn.find('.btn-spinner').addClass('hidden');
    }
}

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

function togglePassword(inputId, btn) {
    var $input = $('#' + inputId);
    var isText = $input.attr('type') === 'text';
    $input.attr('type', isText ? 'password' : 'text');
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
    window.location.href = '/Account/GoogleLogin?returnUrl=' + encodeURIComponent(window.location.pathname);
}