$(document).ready(function () {
    $.validator.unobtrusive.parse('#addRoleForm');
});

$('#addRoleBtn').click(function (e) {
    e.preventDefault();
    var form = $('#addRoleForm');
    if (!form.valid()) {
        return;
    }
    $.ajax({
        url: '/Role/Save',
        type: 'PUT',
        data: form.serialize(),
        cache: false,
        beforeSend: function () {
            setButtonLoading($('#addRoleBtn'), true);
        },
        success: function (response) {
            if (response.statusCode == 200) {
                showSuccess(response.message || 'Role saved successfully');
                setTimeout(function () {
                    closePopup();
                    LoadRoles();
                }, 500);
            }
            else {
                setButtonLoading($('#addRoleBtn'), false);
                showError(response.message || 'Role save failed');
            }
        },
        error: function () {

            setButtonLoading($('#addRoleBtn'), false);
            showError('Role save failed. Please try again.');
        }
    });
});