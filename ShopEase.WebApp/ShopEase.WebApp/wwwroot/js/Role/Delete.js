$('#deleteRoleBtn').click(function () {
    debugger
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
            debugger
            if (response.statusCode == 200) {
                debugger
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