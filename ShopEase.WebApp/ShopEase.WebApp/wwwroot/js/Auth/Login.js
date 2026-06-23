$(function () {
    var loginForm = $('#loginForm');
    var loginBtn = $('#loginBtn');

    loginBtn.on('click', function (e) {
        e.preventDefault();
        if (!loginForm.valid()) {
            return false; 
        }

        setButtonLoading(loginBtn, true);
        var formData = new FormData(loginForm[0]);

        $.ajax({
            url: '/Account/Login',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                debugger
                if (response.status) {
                    debugger
                    showSuccess(response.message || 'Login successful');
                    setTimeout(function () {
                        window.location.href = response.redirectUrl || '/Dashboard';
                    }, 500);
                } else {
                    if (response.errors && Array.isArray(response.errors)) {
                        response.errors.forEach(function (err) {
                            if (err.propertyName) {
                                var input = loginForm.find('[name="' + err.propertyName + '"]');
                                var errorSpan = input.closest('.form-group').find('.field-error');

                                input.addClass('is-invalid');
                                errorSpan.removeClass('field-validation-valid').addClass('field-validation-error');
                                errorSpan.text(err.errorMessage);
                            }
                        });
                    } else {
                        showError(response.message || 'Login failed');
                    }
                    setButtonLoading(loginBtn, false);
                }
            },
            error: function () {
                setButtonLoading(loginBtn, false);
                showError('Login failed. Please try again.');
            }
        });
    });
});