




function LoadTable() {
    DataTable = $('#ViewTable').DataTable({
        "ajax": { url: '/Admin/Products/GetAll' },
        "columns": [
            { data: 'title' },
            { data: 'isbn' },
            { data: 'author' },
            { data: 'category.name' },
            { data: 'listPrice' },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group role="group">
                        <a href="/Admin/Products/Upsert?id=${data}" class="btn btn-primary mx-2"> Edit </a>
                        <a onClick=Del('/Admin/Products/Delete?id=${data}') class="btn btn-danger mx-2"> Delete </a>
                    </div>`
                }
            },


        ]
    });
}


$(document).ready(function () {

    LoadTable();

});


function Del(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        Swal.fire("Deleted!", data.message, "success");
                        DataTable.ajax.reload();
                        //tostart.success(data.message);
                    }
                },
            });
        }
    });
}