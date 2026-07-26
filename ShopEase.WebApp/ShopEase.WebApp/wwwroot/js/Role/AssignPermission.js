$(document).ready(function () {
    $(document).on('change', '.select-all-checkbox', function () {
        var $block = $(this).closest('.permission-module-block');
        $block.find('.action-checkbox').prop('checked', $(this).is(':checked'));
    });

    $(document).on('change', '.action-checkbox', function () {
        var $block = $(this).closest('.permission-module-block');
        var total = $block.find('.action-checkbox').length;
        var checked = $block.find('.action-checkbox:checked').length;
        $block.find('.select-all-checkbox').prop('checked', total === checked);
    });

    $('#savePermissionBtn').on('click', function () {
        var roleId = $('#RoleId').val();
        var permissions = [];

        $('.permission-module-block').each(function () {
            permissions.push({
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
            data: JSON.stringify({ RoleId: roleId, Permissions: permissions }),
            success: function (res) {
                if (res.status) {
                    closePopup();
                } else {
                    alert(res.message || 'Failed to save permissions.');
                }
            },
            error: function () {
                alert('An error occurred.');
            }
        });
    });
});