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
function bindChildLinks(container = document) {
    container.querySelectorAll('.child-link').forEach(link => {
        link.onclick = function () {
            const type = this.dataset.type;
            const contentArea = document.getElementById('content-area');

            contentArea.innerHTML = `<h2>${type}</h2><p>Loading...</p>`;

            fetch(`/Settings/Get${type}s`)
                .then(response => {
                    if (!response.ok) throw new Error(`HTTP ${response.status}`);
                    return response.json();
                })
                .then(data => {
                    if (!Array.isArray(data) || data.length === 0) {
                        contentArea.innerHTML = `<h2>${type}</h2><p>No data found.</p>`;
                        return;
                    }

                    let table = `<h2>${type}</h2><table border="1" cellpadding="5" cellspacing="0" style="border-collapse: collapse; width: 100%;">`;

                    const columns = Object.keys(data[0]);
                    table += "<thead><tr>";
                    columns.forEach(col => {
                        //console.log("Original:", col, "→ Formatted:", formatHeaderName(col));
                        const formattedCol = formatHeaderName(col);
                        table += `<th style="background:#f2f2f2;">${formattedCol}</th>`;
                    });
                    table += "</tr></thead>";

                    table += "<tbody>";
                    data.forEach(row => {
                        table += "<tr>";
                        columns.forEach(col => {
                            table += `<td>${row[col]}</td>`;
                        });
                        table += "</tr>";
                    });
                    table += "</tbody></table>";

                    contentArea.innerHTML = table;
                })
                .catch(err => {
                    contentArea.innerHTML = `<h2>${type}</h2><div style="color:red;">Error: ${err.message}</div>`;
                });
        };
    });
}

window.bindChildLinks = bindChildLinks;
