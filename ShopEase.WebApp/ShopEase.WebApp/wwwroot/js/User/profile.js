$(document).on('change', '#profilePicInput', function () {
    var file = this.files[0];
    if (!file) return;

    var formData = new FormData();
    formData.append('file', file);
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Account/UploadProfilePicture',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: { 'RequestVerificationToken': token },
        success: function (res) {
            if (res && res.status) {
                setAvatarEverywhere(res.response, null, null);
                showSuccess('Profile picture updated.');
            } else {
                showError(res.message || 'Upload failed.');
            }
        },
        error: function () { showError('Upload failed.'); }
    });
});

$(document).on('click', '#btnRemovePic', function () {
    var $wrap = $('#profileAvatarWrap');
    var initials = $wrap.data('initials');
    var color = $wrap.data('color');

    showConfirm('Remove your profile picture?', function () {
        ajaxPost('/Account/RemoveProfilePicture', {}, function (res) {
            if (res && res.status) {
                setAvatarEverywhere(null, initials, color);
                showSuccess('Profile picture removed.');
            } else {
                showError(res.message || 'Failed to remove.');
            }
        });
    });
});

function setAvatarEverywhere(imgUrl, initials, color) {
    var $targets = $('.topbar__avatar, .sidebar__avatar, #layoutAvatar, #profileAvatarWrap .profile-avatar-img, #profileAvatarWrap .profile-avatar-fallback');

    $targets.each(function () {
        var $el = $(this);
        if (imgUrl) {
            if ($el.is('img')) {
                $el.attr('src', imgUrl);
            } else {
                $el.css('background', '').text('');
                var $img = $('<img>').addClass($el.attr('class')).attr('src', imgUrl);
                $el.replaceWith($img);
            }
        } else {
            if ($el.is('img')) {
                var $div = $('<div>').addClass($el.attr('class'))
                    .css('background', color).text(initials);
                $el.replaceWith($div);
            } else {
                $el.css('background', color).text(initials);
            }
        }
    });
}