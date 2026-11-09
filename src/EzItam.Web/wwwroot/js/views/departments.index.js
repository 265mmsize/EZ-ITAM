$(document).ready(function () {
    $('#departmentsTable').DataTable({
        "initComplete": function () {
            $('#departmentsTable').css('visibility', 'visible');
        }
    });
});