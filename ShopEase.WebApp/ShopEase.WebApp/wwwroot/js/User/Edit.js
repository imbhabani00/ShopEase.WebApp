$(document).ready(function () {
    debugger
    $.validator.unobtrusive.parse('#editUserForm');
    var roleId = $('#hdnRoleId').val();
    loadDropdown('/Lookup/GetRoles', 'RoleId', roleId , 'Select Role');
});

$('#editUserBtn').click(function (e) {
    e.preventDefault();
    var form = $('#editUserForm');
    if (!form.valid()) {
        return;
    }
    $.ajax({
        url: '/User/Save',
        type: 'POST',
        data: form.serialize(),
        cache: false,
        beforeSend: function () {
            setButtonLoading($('#editUserBtn'), true);
        },
        success: function (response) {
            debugger
            if (response.statusCode == 200) {
                showSuccess(response.message || 'User updated successfully');
                setTimeout(function () {
                    closePopup();
                    LoadUsers();
                }, 500);
            }
            else {
                setButtonLoading($('#editUserBtn'), false);
                showError(response.message || 'User updated failed');
            }
        },
        error: function () {

            setButtonLoading($('#editUserBtn'), false);
            showError('User updated failed. Please try again.');
        }
    });
});