$(document).ready(function () {
    debugger
    LoadData();
});

function LoadData() {
    debugger
    LoadRoles();
}

function LoadRoles() {
    var search = $('#roleSearchInput').val();
    var pageNumber = $("#hdn_PageNumber").val();
    var pageSize = $("#hdn_PageSize").val();

    $('#roleListContainer').html('<div class="spinner"></div>');

    $.ajax({
        url: '/Role/GetList',
        type: 'GET',
        data: {
            PageNumber: pageNumber,
            PageSize: pageSize,
            SearchString: search
        },
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        cache: false,
        success: function (data) {
            if (data && data.trim().length > 0) {
                $('#roleListContainer').html(data);
            } else {
                $('#roleListContainer').html(
                    '<div style="padding:40px;text-align:center;color:#aaa">' +
                    '<p>No roles found.</p>' +
                    '</div>'
                );
            }
        },
        error: function () {
            $('#roleListContainer').html(
                '<div style="padding:20px;text-align:center;color:#d32f2f">' +
                '<p>Failed to load roles. Please try again.</p>' +
                '<button class="btn btn-sm btn-primary" onclick="location.reload()">Retry</button>' +
                '</div>'
            );
        }
    });
}