$(function () {
    debugger
    $(document).on('click', '#activeInactiveUserBtn', function () {
        changeUserStatus();
    });
});

function changeUserStatus() {
    debugger
    var userId = $("#hdn_UserId").val();
    var currentStatus = $("#hdn_IsActive").val().toLowerCase() === "true";
    var newStatus = !currentStatus;

    $.ajax({
        url: '/User/ChangeStatus',
        type: 'PUT',
        data: {
            userId: userId,
            isActive: newStatus
        },
        cache: false,

        beforeSend: function () {
            setButtonLoading($('#activeInactiveUserBtn'), true);
        },

        success: function (response) {
            if (response.statusCode == 200) {
                showSuccess(response.message || 'User status updated successfully');
                closePopup();
                LoadUsers();
            }
            else {
                setButtonLoading($('#activeInactiveUserBtn'), false);
                showError(response.message || 'User status update failed');
            }
        },

        error: function () {
            setButtonLoading($('#activeInactiveUserBtn'), false);
            showError('User status update failed. Please try again.');
        }
    });
}