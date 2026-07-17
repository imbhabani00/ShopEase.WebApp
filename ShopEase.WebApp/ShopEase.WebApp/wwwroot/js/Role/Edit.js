$('#editRoleBtn').click(function () {
debugger
    $.ajax({
        url: '/Role/Save',
        type: 'POST',
        data: $('#editRoleForm').serialize(),
        cache: false,

        success: function (response) {

            if (response.statusCode == 200) {
                showSuccess(response.message || 'Role updated successfully');

                setTimeout(function () {
                    closePopup();
                    LoadRoles();
                }, 500);
            }
            else {
                showError(response.message || 'Role update failed');
            }

        },

        error: function () {
            showError('Role update failed. Please try again.');
        }
    });

});