$(document).ready(function () {
    $.validator.unobtrusive.parse('#editRoleForm');
});
$('#editRoleBtn').click(function (e) {
    e.preventDefault();
    var form = $('#editRoleForm');
    if (!form.valid()) {
        return;
    }
    $.ajax({
        url: '/Role/Save',
        type: 'POST',
        data: form.serialize(),
        cache: false,
        beforeSend: function () {
            setButtonLoading($('#editRoleBtn'), true);
        },
        success: function (response) {
            if (response.statusCode == 200) {
                showSuccess(response.message || 'Role updated successfully');
                setTimeout(function () {
                    closePopup();
                    LoadRoles();
                }, 500);
            }
            else {
                setButtonLoading($('#editRoleBtn'), false);
                showError(response.message || 'Role update failed');
            }
        },
        error: function () {
            setButtonLoading($('#editRoleBtn'), false);
            showError('Role update failed. Please try again.');
        }
    });
});