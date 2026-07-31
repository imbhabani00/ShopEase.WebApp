$('#deleteUserBtn').click(function () {

    $.ajax({
        url: '/User/Delete',
        type: 'DELETE',
        data: {
            userId: $('#UserId').val()
        },
        cache: false,

        beforeSend: function () {
            setButtonLoading($('#deleteUserBtn'), true);
        },

        success: function (response) {

            if (response.statusCode == 200) {

                showSuccess(response.message || 'User deleted successfully');

                closePopup();

                LoadUsers();
            }
            else {

                setButtonLoading($('#deleteUserBtn'), false);

                showError(response.message || 'User deletion failed');
            }
        },

        error: function () {

            setButtonLoading($('#deleteUserBtn'), false);

            showError('User deletion failed. Please try again.');
        }
    });

});