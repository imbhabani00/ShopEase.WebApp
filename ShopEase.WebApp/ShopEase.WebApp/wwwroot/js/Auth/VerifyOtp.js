$(function () {
    var otpForm = $('#otpForm');
    var verifyBtn = $('#verifyBtn');
    var resendBtn = $('#resendBtn');
    var otpInput = otpForm.find('[name="Otp"]');

    otpInput.on('keypress', function (e) {
        var char = String.fromCharCode(e.which);
        if (!/[0-9]/.test(char)) {
            e.preventDefault();
            return false;
        }
    });
    otpInput.on('input', function () {
        var value = $(this).val().replace(/\D/g, ''); 
        $(this).val(value.slice(0, 6));
    });

    verifyBtn.on('click', function (e) {
        e.preventDefault();
        var otp = otpInput.val().trim();
        if (!otp || otp.length !== 6 || isNaN(otp)) {
            showError('Please enter a valid 6-digit OTP');
            return false;
        }
        setButtonLoading(verifyBtn, true);
        var formData = new FormData(otpForm[0]);
        $.ajax({
            url: '/Account/VerifyOtp',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.status === true) {
                    showSuccess(response.message || 'OTP verified successfully');
                    setTimeout(function () {
                        window.location.href = response.returnUrl || '/Dashboard';
                    }, 1000);
                } else {
                    showError(response.message || 'OTP verification failed');
                    otpInput.val('');
                    otpInput.focus();
                    setButtonLoading(verifyBtn, false);
                }
            },
            error: function (xhr) {
                setButtonLoading(verifyBtn, false);
                showError('An error occurred. Please try again.');
                otpInput.val('');
                otpInput.focus();
            }
        });
    });

    resendBtn.on('click', function (e) {
        e.preventDefault();

        setButtonLoading(resendBtn, true);

        $.ajax({
            url: '/Account/ResendOtp',
            type: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': $('[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.status === true) {
                    showSuccess(response.message || 'OTP resent to your email');
                    startResendCountdown(60);
                } else {
                    showError(response.message || 'Failed to resend OTP');
                    setButtonLoading(resendBtn, false);
                }
            },
            error: function () {
                setButtonLoading(resendBtn, false);
                showError('Failed to resend OTP. Please try again.');
            }
        });
    });

    function startResendCountdown(seconds) {
        var remaining = seconds;
        resendBtn.prop('disabled', true);
        resendBtn.css('opacity', '0.5');
        resendBtn.css('cursor', 'not-allowed');

        var countdownInterval = setInterval(function () {
            remaining--;
            if (remaining <= 0) {
                clearInterval(countdownInterval);
                resendBtn.prop('disabled', false);
                resendBtn.text('Resend OTP');
                resendBtn.css('opacity', '1');
                resendBtn.css('cursor', 'pointer');
                setButtonLoading(resendBtn, false);
            } else {
                resendBtn.text('Resend OTP (' + remaining + 's)');
            }
        }, 1000);
    }
    otpForm.validate({
        rules: {
            Otp: {
                required: true,
                minlength: 6,
                maxlength: 6,
                pattern: /^\d{6}$/
            }
        },
        messages: {
            Otp: {
                required: 'OTP is required',
                minlength: 'OTP must be 6 digits',
                maxlength: 'OTP must be 6 digits',
                pattern: 'OTP must contain only digits'
            }
        },
        errorClass: 'field-validation-error',
        validClass: 'field-validation-valid',
        errorPlacement: function (error, element) {
            var errorSpan = element.closest('.form-group').find('.field-error');
            if (errorSpan.length) {
                errorSpan.text(error.text());
            } else {
                error.insertAfter(element);
            }
        },
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        }
    });
    otpInput.focus();
});