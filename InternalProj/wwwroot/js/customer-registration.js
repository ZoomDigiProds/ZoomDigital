console.log("Script loaded!");

document.addEventListener("DOMContentLoaded", function () {
    const stateDropdown = document.getElementById("NewAddress_StateId");
    const regionDropdown = document.getElementById("NewAddress_RegionId");

    if (stateDropdown && regionDropdown) {
        console.log("Dropdowns found");

        stateDropdown.addEventListener("change", function () {
            const stateId = stateDropdown.value;
            console.log("Selected stateId: ", stateId);

            regionDropdown.innerHTML = '<option>Loading...</option>';

            fetch(`/CustomerReg/GetRegionsByState?stateId=${stateId}`)
                .then(response => response.json())
                .then(regions => {
                    let options = '<option value="">-- Select Region --</option>';
                    regions.forEach(region => {
                        options += `<option value="${region.id}">${region.name}</option>`;
                    });
                    regionDropdown.innerHTML = options;
                })
                .catch(error => {
                    console.error("Error loading regions: ", error);
                    regionDropdown.innerHTML = '<option value="">-- Error loading --</option>';
                });
        });
    } else {
        console.error("Dropdown elements not found!");
    }
});
