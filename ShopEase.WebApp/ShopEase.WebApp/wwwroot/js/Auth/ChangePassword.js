$(document).ready(function () {
    var token = $('input[name="__RequestVerificationToken"]').val();

    // STEP 1: Send OTP
    $('#sendOtpBtn').click(function () {
        $.ajax({
            url: '/Account/RequestPasswordChangeOtp',
            type: 'POST',
            data: { __RequestVerificationToken: token },
            cache: false,
            beforeSend: function () {
                setButtonLoading($('#sendOtpBtn'), true);
            },
            success: function (response) {
                setButtonLoading($('#sendOtpBtn'), false);
                if (response.status) {
                    showSuccess(response.message || 'OTP sent to your email');
                    $('#otpRequestSection').addClass('hidden');
                    $('#otpVerifySection').removeClass('hidden');
                } else {
                    showError(response.message || 'Failed to send OTP');
                }
            },
            error: function () {
                setButtonLoading($('#sendOtpBtn'), false);
                showError('Failed to send OTP. Please try again.');
            }
        });
    });

    // Resend OTP (reuse same endpoint)
    $('#resendOtpLink').click(function () {
        $('#sendOtpBtn').trigger('click');
    });

    // STEP 2: Verify OTP
    $('#verifyOtpBtn').click(function () {
        var otp = $('#otpInput').val().trim();
        $('#otpError').text('');

        if (!otp || otp.length !== 6) {
            $('#otpError').text('Please enter a valid 6-digit OTP');
            return;
        }

        $.ajax({
            url: '/Account/VerifyPasswordChangeOtp',
            type: 'POST',
            data: { Otp: otp, __RequestVerificationToken: token },
            cache: false,
            beforeSend: function () {
                setButtonLoading($('#verifyOtpBtn'), true);
            },
            success: function (response) {
                setButtonLoading($('#verifyOtpBtn'), false);
                if (response.status) {
                    showSuccess(response.message || 'OTP verified');
                    $('#otpVerifySection').addClass('hidden');
                    $('#changePasswordForm').removeClass('hidden');
                } else {
                    $('#otpError').text(response.message || 'Invalid or expired OTP');
                }
            },
            error: function () {
                setButtonLoading($('#verifyOtpBtn'), false);
                showError('Something went wrong. Please try again.');
            }
        });
    });

    // STEP 3: Submit new password
    $('#changePasswordForm').submit(function (e) {
        e.preventDefault();
        var form = $(this);

        if (!form.valid()) {
            return;
        }

        if ($('#newPassword').val() !== $('#confirmPassword').val()) {
            showError('Passwords do not match');
            return;
        }

        $.ajax({
            url: '/Account/ChangePassword',
            type: 'POST',
            data: form.serialize() + '&__RequestVerificationToken=' + encodeURIComponent(token),
            cache: false,
            beforeSend: function () {
                setButtonLoading($('#changePasswordBtn'), true);
            },
            success: function (response) {
                if (response.status) {
                    showSuccess(response.message || 'Password changed successfully');
                    setTimeout(function () {
                        window.location.href = response.returnUrl || '/Dashboard';
                    }, 800);
                } else {
                    setButtonLoading($('#changePasswordBtn'), false);
                    showError(response.message || 'Failed to change password');
                }
            },
            error: function () {
                setButtonLoading($('#changePasswordBtn'), false);
                showError('Failed to change password. Please try again.');
            }
        });
    });
});