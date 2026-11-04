document.addEventListener("DOMContentLoaded", function () {
	const selectAllCheckbox = document.getElementById("select-all");
	const assetCheckboxes = document.querySelectorAll(".asset-checkbox");
	const btnPrintLabel = document.getElementById("btn-print-label");

	if (selectAllCheckbox) {
		selectAllCheckbox.addEventListener("change", function () {
			assetCheckboxes.forEach(cb => cb.checked = this.checked);
		});
	}

	assetCheckboxes.forEach(cb => {
		cb.addEventListener("change", function () {
			const allChecked = Array.from(assetCheckboxes).every(item => item.checked);
			const anyChecked = Array.from(assetCheckboxes).some(item => item.checked);
			if (selectAllCheckbox) {
				selectAllCheckbox.checked = allChecked;
				selectAllCheckbox.indeterminate = anyChecked && !allChecked;
			}
		});
	});

	if (btnPrintLabel) {
		btnPrintLabel.addEventListener("click", function () {
			const selectedIds = Array.from(assetCheckboxes)
				.filter(cb => cb.checked)
				.map(cb => cb.value);

			if (selectedIds.length === 0) {
				alert(btnPrintLabel.dataset.alertMessage);
				return;
			}

			const idsString = selectedIds.join(",");
			const base64Ids = btoa(idsString);
			const url = btnPrintLabel.dataset.printUrl + "?ids=" + encodeURIComponent(base64Ids);
			window.open(url, "_blank", "popup");
		});
	}

});