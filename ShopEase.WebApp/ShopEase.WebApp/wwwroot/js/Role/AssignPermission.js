$(function () {
    $(document).off('change', '.select-all-checkbox').on('change', '.select-all-checkbox', function () {
        var $block = $(this).closest('.permission-module-block');
        var isChecked = $(this).is(':checked');
        $block.find('.action-checkbox').prop('checked', isChecked);
    });

    $(document).off('change', '.action-checkbox').on('change', '.action-checkbox', function () {
        var $block = $(this).closest('.permission-module-block');
        var total = $block.find('.action-checkbox').length;
        var checked = $block.find('.action-checkbox:checked').length;
        $block.find('.select-all-checkbox').prop('checked', total === checked);
    });

    $(document).off('click', '#savePermissionBtn').on('click', '#savePermissionBtn', function () {
        var roleId = $('#RoleId').val();
        var requests = [];

        $('.permission-module-block').each(function () {
            requests.push({
                RoleId: parseInt(roleId),
                ModuleId: $(this).data('module-id'),
                CanView: $(this).find('[data-action="CanView"]').is(':checked'),
                CanAdd: $(this).find('[data-action="CanAdd"]').is(':checked'),
                CanEdit: $(this).find('[data-action="CanEdit"]').is(':checked'),
                CanDelete: $(this).find('[data-action="CanDelete"]').is(':checked'),
                CanInactive: $(this).find('[data-action="CanInactive"]').is(':checked')
            });
        });

        $.ajax({
            url: '/Role/SavePermissions',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            data: JSON.stringify(requests),
            success: function (res) {
                if (res.status) {
                    showSuccess(res.message || 'Permissions saved successfully.');
                    closePopup();
                } else {
                    showError(res.message || 'Failed to save permissions.');
                }
            },
            error: function () {
                showError('An error occurred while saving permissions.');
            }
        });
    });
});