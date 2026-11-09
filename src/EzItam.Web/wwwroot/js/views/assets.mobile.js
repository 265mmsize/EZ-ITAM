let html5QrCode;

document.addEventListener("DOMContentLoaded", function () {
    const startBtn = document.getElementById("btnStartScan");
    const stopBtn = document.getElementById("btnStopScan");
    const deviceInfoCard = document.getElementById("deviceInfoCard");
    const newEmployeeSelect = document.getElementById("NewEmployeeId");

    $('#NewEmployeeId').select2({
        theme: "bootstrap-5",
        width: '100%',
        placeholder: newEmployeeSelect.dataset.placeholder,
        allowClear: true
    });


    //Khởi động camera quét QR
    startBtn.addEventListener("click", function () {
        startBtn.classList.add("d-none");
        stopBtn.classList.remove("d-none");

        html5QrCode = new Html5Qrcode("reader");
        html5QrCode.start(
            { facingMode: "environment" }, // Ưu tiên camera sau
            { fps: 10, qrbox: { width: 250, height: 250 } },
            (decodedText, decodedResult) => {
                document.getElementById("assetTagInput").value = decodedText;

                stopCamera();

                fetchAssetInfo(decodedText);
            },
            (errorMessage) => {
                alert(errorMessage);
            }
        ).catch(err => {
            alert(err);
            stopCamera();
        });
    });

    stopBtn.addEventListener("click", stopCamera);

    function stopCamera() {
        if (html5QrCode && html5QrCode.isScanning) {
            html5QrCode.stop().then(() => {
                startBtn.classList.remove("d-none");
                stopBtn.classList.add("d-none");
            }).catch(err => console.error(err));
        } else {
            startBtn.classList.remove("d-none");
            stopBtn.classList.add("d-none");
        }
    }

    document.getElementById("btnSearchTag").addEventListener("click", function () {
        let tag = document.getElementById("assetTagInput").value.trim();
        if (tag) fetchAssetInfo(tag);
    });

    function fetchAssetInfo(tag) {
        fetch(`/Assets/GetAssetByTag?tag=${encodeURIComponent(tag)}`)
            .then(async response => {
                const result = await response.json();
                if (!response.ok) throw new Error(result.message || "Không thể tải thông tin thiết bị.");
                return result;
            })
            .then(data => {
                if (data.success) {
                    document.getElementById("AssetId").value = data.item.id;
                    document.getElementById("lblAssetTag").textContent = data.item.assetTag;
                    document.getElementById("lblAssetName").textContent = data.item.name;
                    document.getElementById("lblAssetCategory").textContent = data.item.category.name;
                    document.getElementById("lblOldStatus").textContent = getAssetStatusString(data.item.status);
                    document.getElementById("lblOldEmployee").textContent = data.item.currentEmployeeId != null ? data.item.currentEmployee.displayName : "-";
                    document.getElementById("lblOldLocation").textContent = data.item.locationId != null ? data.item.location.name : "-";

                    document.getElementById("NewStatus").value = data.item.status;
                    $('#NewEmployeeId').val(data.item.currentEmployeeId).trigger('change');
                    document.getElementById("NewLocationId").value = data.item.locationId;

                    deviceInfoCard.classList.remove("d-none");
                } else {
                    alert(data.message || "Không tìm thấy thiết bị!");
                    deviceInfoCard.classList.add("d-none");
                }
            })
            .catch(err => alert("Lỗi kết nối máy chủ!"));
    }

    document.getElementById("btnConfirmTransfer").addEventListener("click", function () {
        let payload = {
            AssetId: parseInt(document.getElementById("AssetId").value),
            NewEmployeeId: document.getElementById("NewEmployeeId").value ? parseInt(document.getElementById("NewEmployeeId").value) : null,
            NewLocationId: document.getElementById("NewLocationId").value ? parseInt(document.getElementById("NewLocationId").value) : null,
            NewStatus: parseInt(document.getElementById("NewStatus").value),
            Notes: document.getElementById("TransferNotes").value
        };
        fetch('/Assets/TransferAsset', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(async response => {
                const result = await response.json();
                if (!response.ok) throw new Error(result.message || "Có lỗi xảy ra khi lưu.");
                return result;
            })
            .then(res => {
                if (res.success) {
                    alert(newEmployeeSelect.dataset.transferSuccess);
                    location.reload();
                } else {
                    alert(res.message || "Có lỗi xảy ra khi lưu.");
                }
            })
            .catch(err => alert(err.message || "Có lỗi xảy ra khi lưu."));
    });


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