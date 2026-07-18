var pageNumber = 1;
var pageSize = 10;

$(document).ready(function () {
    LoadRoles();
    $(document).on('keyup', '#searchInput', function () {
        pageNumber = 1;
        LoadRoles();
    });
});

function LoadRoles() {
    debugger
    var search = $('#searchInput').val();

    $('#roleListContainer').html('<div class="spinner"></div>');

    $.ajax({
        url: '/Role/GetList',
        type: 'GET',
        data: {
            pageNumber: pageNumber,
            pageSize: pageSize,
            search: search
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

// Change page (accessible globally)
function ChangePage(newPageNumber) {
    pageNumber = newPageNumber;
    LoadRoles();
}