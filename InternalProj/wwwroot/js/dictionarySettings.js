// Toggle visibility of children nodes
window.toggleChildren = function (element) {
    const children = element.nextElementSibling;
    if (children) {
        const isVisible = children.style.display === "block";
        children.style.display = isVisible ? "none" : "block";
        if (!isVisible) bindChildLinks(children);
    }
};

// Format camelCase or PascalCase 
function formatHeaderName(str) {
    return str
        .replace(/([A-Z])/g, ' $1')       // Add space before capital letters
        .replace(/^./, char => char.toUpperCase()) // Capitalize first letter
        .trim();                          // Remove leading/trailing spaces
}

// Attach click events to .child-link elements inside a container
function bindChildLinks() {
    document.querySelectorAll('.child-link').forEach(link => {
        link.addEventListener('click', async (e) => {
            e.preventDefault();
            const type = link.getAttribute('data-type');
            const contentArea = document.getElementById('content-area');
            contentArea.innerHTML = `<p class="text-gray-500">Loading ${type}...</p>`;

            // ✅ Fix pluralization
            let endpoint = '';
            if (type === "Dictionary") {
                endpoint = "/Settings/GetDictionaries";  // correct plural
            } else if (type === "GeneralSetting") {
                endpoint = "/Settings/GetGeneralSettings";
            } else {
                endpoint = `/Settings/Get${type}s`; // fallback
            }

            try {
                const response = await fetch(endpoint);
                const data = await response.json();

                if (data && data.length > 0) {
                    let tableHtml = `
                        <table class="table-auto border-collapse border border-gray-300 w-full mt-4">
                            <thead>
                                <tr>
                                    ${Object.keys(data[0]).map(key => `<th class="border border-gray-300 px-4 py-2">${formatHeaderName(key)}</th>`).join('')}
                                </tr>
                            </thead>
                            <tbody>
                                ${data.map(row => `
                                    <tr>
                                        ${Object.values(row).map(val => `<td class="border border-gray-300 px-4 py-2">${val}</td>`).join('')}
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    `;
                    contentArea.innerHTML = tableHtml;
                } else {
                    contentArea.innerHTML = `<p class="text-gray-500">No data found for ${type}.</p>`;
                }
            } catch (error) {
                console.error("Error fetching data:", error);
                contentArea.innerHTML = `<p class="text-red-500">Failed to load ${type}.</p>`;
            }
        });
    });
}

window.bindChildLinks = bindChildLinks;
