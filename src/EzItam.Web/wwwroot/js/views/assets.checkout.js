document.addEventListener("DOMContentLoaded", function () {

	const employeeSelect = document.getElementById('EmployeeId');

	$('#EmployeeId').select2({
		theme: "bootstrap-5",
		width: '100%',
		placeholder: employeeSelect.dataset.placeholder,
		allowClear: true
	});
});