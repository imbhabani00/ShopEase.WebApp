$(document).ready(function () {
    debugger
    $.validator.unobtrusive.parse('#addUserForm');
    loadDropdown('/Lookup/GetRoles', 'RoleId' ,'' ,  'Select Role');
});

$('#addUserBtn').click(function (e) {
    debugger
    e.preventDefault();
    var form = $('#addUserForm');
    if (!form.valid()) {
        return;
    }
    $.ajax({
        url: '/User/Save',
        type: 'POST',
        data: form.serialize(),
        cache: false,
        beforeSend: function () {
            setButtonLoading($('#addUserBtn'), true);
        },
        success: function (response) {    
            if (response.statusCode == 200) {
                showSuccess(response.message || 'User saved successfully');
                setTimeout(function () {
                    closePopup();
                    LoadRoles();
                }, 500);
            }
            else {
                setButtonLoading($('#addUserBtn'), false);
                showError(response.message || 'User save failed');
            }
        },
        error: function () {

            setButtonLoading($('#addUserBtn'), false);
            showError('User save failed. Please try again.');
        }
    });
});