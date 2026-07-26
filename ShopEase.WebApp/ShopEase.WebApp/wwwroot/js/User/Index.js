var searchDebounceTimer = null;

$(document).ready(function () {
    LoadData();
    $('#userSearchBtn').on('click', function () {
        ChangeSearch();
    });

    $('#userSearchInput').on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            ChangeSearch();
            return;
        }

        clearTimeout(searchDebounceTimer);
        searchDebounceTimer = setTimeout(function () {
            ChangeSearch();
        }, 400);
    });
});

function LoadData() {
    LoadUsers();
}

function ChangeSearch() {
    $("#hdn_PageNumber").val(1);
    LoadData();
}

function ChangePage(page) {
    $("#hdn_PageNumber").val(page);
    LoadData();
}

function ChangePageSize(size) {
    $("#hdn_PageSize").val(size);
    $("#hdn_PageNumber").val(1);
    LoadData();
}

function SortData(column) {
    var currentColumn = $("#hdn_SortParameter").val();
    var currentDirection = $("#hdn_SortDirection").val();
    if (currentColumn === column) {
        currentDirection = currentDirection === "ASC" ? "DESC" : "ASC";
    } else {
        currentColumn = column;
        currentDirection = "ASC";
    }
    $("#hdn_SortParameter").val(currentColumn);
    $("#hdn_SortDirection").val(currentDirection);
    $("#hdn_PageNumber").val(1);
    LoadData();
}

function LoadUsers() {
    var search = $('#userSearchInput').val();
    var pageNumber = $("#hdn_PageNumber").val();
    var pageSize = $("#hdn_PageSize").val();
    var sortParameter = $("#hdn_SortParameter").val();
    var sortDirection = $("#hdn_SortDirection").val();

    $('#userListContainer').html('<div class="spinner"></div>');

    $.ajax({
        url: '/User/GetList',
        type: 'GET',
        data: {
            PageNumber: pageNumber,
            PageSize: pageSize,
            SearchString: search,
            SortParameter: sortParameter,
            SortDirection: sortDirection
        },
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        cache: false,
        success: function (data) {
            if (data && data.trim().length > 0) {
                $('#userListContainer').html(data);
            } else {
                $('#userListContainer').html(
                    '<div style="padding:40px;text-align:center;color:#aaa">' +
                    '<p>No users found.</p>' +
                    '</div>'
                );
            }
        },
        error: function () {
            $('#userListContainer').html(
                '<div style="padding:20px;text-align:center;color:#d32f2f">' +
                '<p>Failed to load users. Please try again.</p>' +
                '<button class="btn btn-sm btn-primary" onclick="location.reload()">Retry</button>' +
                '</div>'
            );
        }
    });
}