$(function () {
    $(document).on('click', '#activeInactiveRoleBtn', function () {
        changeRoleStatus();
    });
});

function changeRoleStatus() {
    var roleId = $("#hdn_RoleId").val();
    var currentStatus = $("#hdn_IsActive").val().toLowerCase() === "true";
    var newStatus = !currentStatus;

    $.ajax({
        url: '/Role/ChangeStatus',
        type: 'PUT',
        data: {
            roleId: roleId,
            isActive: newStatus
        },
        cache: false,

        beforeSend: function () {
            setButtonLoading($('#activeInactiveRoleBtn'), true);
        },

        success: function (response) {
            if (response.statusCode == 200) {
                showSuccess(response.message || 'Role status updated successfully');
                closePopup();
                LoadRoles();
            }
            else {
                setButtonLoading($('#activeInactiveRoleBtn'), false);
                showError(response.message || 'Role status update failed');
            }
        },

        error: function () {
            setButtonLoading($('#activeInactiveRoleBtn'), false);
            showError('Role status update failed. Please try again.');
        }
    });
}