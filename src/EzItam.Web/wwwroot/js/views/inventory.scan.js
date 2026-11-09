document.addEventListener("DOMContentLoaded", function () {

    $('#IdEmployeeIV').select2({
        theme: "bootstrap-5",
        width: '100%',
        placeholder: "-- Select --",
        allowClear: true
    });

    const assetTagInput = document.getElementById("assetTagInput");
    const scanForm = document.getElementById("scanForm");
    const scanAlert = document.getElementById("scanAlert");
    const scannedItemPanel = document.getElementById("scannedItemPanel");
    const inventoryId = document.getElementById("InventoryId").value;
    const localizationCard = document.querySelector("[data-inv-alreadyscan]");

    scanForm.addEventListener("submit", function (e) {
        e.preventDefault();
        let tag = assetTagInput.value.trim();
        if (!tag) return;

        fetchAssetInfo(tag);
    });

    function fetchAssetInfo(tag) {
        fetch(`/Inventory/GetDetailByTag?inventoryId=${inventoryId}&tag=${encodeURIComponent(tag)}`)
            .then(response => {
                if (!response.ok) throw new Error("Không thể tải thông tin thiết bị.");
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    document.getElementById("DetailId").value = data.item.id;
                    document.getElementById("lblAssetName").textContent = data.item.assetName;
                    document.getElementById("lblCategory").textContent = data.item.categoryName || "-";
                    document.getElementById("lblLocationName").textContent = data.item.locationName;
                    document.getElementById("lblStatusName").textContent = getAssetStatusString(data.item.status);
                    document.getElementById("lblEmployeeName").textContent = data.item.employeeName ?? "-";

                    document.getElementById("IdLocationIV").value = data.item.idLocationIV;
                    document.getElementById("StatusIV").value = data.item.statusIV;

                    $('#IdEmployeeIV').val(data.item.idEmployeeIV).trigger('change');

                    document.getElementById("Notes").value = data.item.notes || "";

                    //HIỂN THỊ MÃ ASSET TAG LÊN BADGE TRÊN CÙNG CỦA PANEL
                    const statusBadge = document.getElementById("scanStatusBadge");
                    statusBadge.textContent = data.item.assetTag;
                    statusBadge.className = "badge bg-info text-dark";

                    scannedItemPanel.style.opacity = "1";
                    scannedItemPanel.style.pointerEvents = "auto";

                    if (data.item.scanCount > 0) {
                        showAlert(localizationCard.dataset.invAlreadyscan, "success");
                    } else {
                        showAlert(localizationCard.dataset.invScansuccessfully, "success");
                    }
                    assetTagInput.value = "";
                } else {
                    showAlert(data.message || "Không tìm thấy thiết bị trong đợt kiểm kê này.", "danger");
                    resetPanel();
                    assetTagInput.value = "";
                }
            })
            .catch(err => {
                showAlert(err.message || "Không thể tải thông tin thiết bị.", "danger");
            });
    }

    document.getElementById("btnSaveScan").addEventListener("click", function () {
        let formData = {
            id: parseInt(document.getElementById("DetailId").value),
            idLocationIV: document.getElementById("IdLocationIV").value ? parseInt(document.getElementById("IdLocationIV").value) : null,
            statusIV: parseInt(document.getElementById("StatusIV").value) || 0,
            idEmployeeIV: document.getElementById("IdEmployeeIV").value ? parseInt(document.getElementById("IdEmployeeIV").value) : null,
            notes: document.getElementById("Notes").value
        };


        fetch('/Inventory/SaveScanResult', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        })
            .then(async response => {
                const result = await response.json();
                if (!response.ok) throw new Error(result.message || "Lỗi khi lưu dữ liệu.");
                return result;
            })
            .then(res => {
                if (res.success) {
                    showAlert(localizationCard.dataset.invSavesuccessfully, "success");

                    assetTagInput.value = "";
                    assetTagInput.focus();
                    resetPanel();
                } else {
                    showAlert(res.message || "Lỗi khi lưu dữ liệu.", "danger");
                }
            })
            .catch(err => showAlert(err.message || "Lỗi khi lưu dữ liệu.", "danger"));
    });

    function showAlert(message, type) {
        scanAlert.className = `alert alert-${type} mt-3`;
        scanAlert.textContent = message;
        scanAlert.classList.remove("d-none");
    }

    function resetPanel() {
        scannedItemPanel.style.opacity = "0.6";
        scannedItemPanel.style.pointerEvents = "none";
        document.getElementById("updateDetailForm").reset();

        const statusBadge = document.getElementById("scanStatusBadge");
        statusBadge.textContent = localizationCard.dataset.invNull;
        statusBadge.className = "badge bg-light text-dark";
    }

    function getAssetStatusString(statusValue) {
        const statuses = {
            0: "InStock",
            1: "Assigned",
            2: "InRepair",
            3: "Idle",
            4: "Retired",
            5: "Disposed",
            6: "Lost"
        };
        return statuses[statusValue] || "Status: " + statusValue;
    }
});