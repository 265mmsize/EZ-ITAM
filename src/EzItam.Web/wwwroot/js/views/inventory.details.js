$(document).ready(function () {
    $('#inventoryTable').DataTable({
    });

    $('#changedItemsTable').DataTable({
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const btnPostDiff = document.getElementById("btnPostDiff");

    if (btnPostDiff) {
        btnPostDiff.addEventListener("click", function (e) {
            if (this.classList.contains("disabled") || this.getAttribute("aria-disabled") === "true") {
                e.preventDefault();
                return;
            }

            e.preventDefault();
            const targetUrl = this.href;

            const isConfirmed = confirm(this.dataset.confirmMessage);

            if (isConfirmed) {
                window.location.href = targetUrl;
            }
        });
    }
});