$(document).ready(function () {
    $('#employeesTable').DataTable({
        "initComplete": function () {
            $('#employeesTable').css('visibility', 'visible');
        }
    });
});