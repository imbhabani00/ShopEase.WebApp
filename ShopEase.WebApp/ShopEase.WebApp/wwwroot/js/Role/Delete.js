$('#deleteRoleBtn').click(function () {
    $.ajax({
        url: '/Role/Delete',
        type: 'DELETE',
        data: {
            roleId: $('#RoleId').val()
        },
        cache: false,
        beforeSend: function () {
            setButtonLoading($('#deleteRoleBtn'), true);
        },
        success: function (response) {
            if (response.statusCode == 200) {
                showSuccess(response.message || 'Role deleted successfully');
                closePopup();
                LoadRoles();
            }
            else {
                setButtonLoading($('#deleteRoleBtn'), false);
                showError(response.message || 'Role deletion failed');
            }
        },
        error: function () {
            setButtonLoading($('#deleteRoleBtn'), false);
            showError('Role deletion failed. Please try again.');
        }
    });
});