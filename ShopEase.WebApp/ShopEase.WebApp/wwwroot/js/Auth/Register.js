$(document).ready(function () {
    $("#registerForm").on("submit", function (e) {
        debugger
        e.preventDefault();
        var password = $("#regPassword").val().trim();
        var confirmPassword = $("#regConfirmPassword").val().trim();
        if (password === "") {
            showError("Password is required.");
            return;
        }
        if (confirmPassword === "") {
            showError("Confirm Password is required.");
            return;
        }
        const passwordRegex =
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,12}$/;
        if (!passwordRegex.test(password)) {
            showError("Password must be 8-12 characters and contain an uppercase letter, lowercase letter, number, and special character.");
            return;
        }

        if (password !== confirmPassword) {
            showError("Password and Confirm Password do not match.");
            return;
        }
        $("#PasswordHash").val(password);
        this.submit();
    });
});

$(function () {
    debugger
    var registerForm = $('#registerForm');
    var registerBtn = $('#registerBtn');

    registerBtn.on('click', function (e) {
        e.preventDefault();
        if (!registerForm.valid()) {
            return false;
        }

        setButtonLoading(registerBtn, true);
        var formData = new FormData(registerForm[0]);

        $.ajax({
            url: '/Account/Register',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.statusCode == 200) {
                    showSuccess(response.message || 'Registration successful');
                    setTimeout(function () {
                        window.location.href = response.redirectUrl || '/Account/Login';
                    }, 500);
                } else {
                    if (response.errors && Array.isArray(response.errors)) {
                        response.errors.forEach(function (err) {
                            if (err.propertyName) {
                                var input = registerForm.find('[name="' + err.propertyName + '"]');
                                var errorSpan = input.closest('.form-group').find('.field-error');
                                input.addClass('is-invalid');
                                errorSpan.removeClass('field-validation-valid').addClass('field-validation-error');
                                errorSpan.text(err.errorMessage);
                            }
                        });
                    } else {
                        showError(response.message || 'Registration failed');
                    }
                    setButtonLoading(registerBtn, false);
                }
            },
            error: function () {
                setButtonLoading(registerBtn, false);
                showError('Registration failed. Please try again.');
            }
        });
    });
});

function togglePassword(inputId, btn) {
    var input = document.getElementById(inputId);
    var isPassword = input.type === 'password';
    input.type = isPassword ? 'text' : 'password';
    btn.classList.toggle('active', isPassword);
}

function updateStrength(value) {
    var segs = [
        document.getElementById('s1'),
        document.getElementById('s2'),
        document.getElementById('s3'),
        document.getElementById('s4')
    ];
    var label = document.getElementById('strengthLabel');

    var score = 0;
    if (value.length >= 8) score++;
    if (/[A-Z]/.test(value)) score++;
    if (/[0-9]/.test(value)) score++;
    if (/[^A-Za-z0-9]/.test(value)) score++;

    var colors = ['#e74c3c', '#e67e22', '#f1c40f', '#2ecc71'];
    var labels = ['Weak', 'Fair', 'Good', 'Strong'];

    segs.forEach(function (seg, i) {
        seg.style.backgroundColor = i < score ? colors[score - 1] : '';
    });

    label.textContent = value.length === 0 ? '' : labels[score - 1] || '';
    label.style.color = score > 0 ? colors[score - 1] : '';
}