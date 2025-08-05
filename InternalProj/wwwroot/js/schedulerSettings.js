var jobList = [];
jobList.push(
    { name: "Job A", occurence: "Everyday", time: "08:00" },
    { name: "Job C", occurence: "Weekly", time: "1|09:30" }   
);

var occurenceOptions = ["Everyday", "Weekly","Quarterly", "Monthly", "Yearly"];

function renderTable() {
    const tbody = document.getElementById("scheduler-body");
    if (!tbody) return;

    tbody.innerHTML = "";
    jobList.forEach((job, index) => {

        const timeHTML = generateTimeInput(job.occurence, job.time, index);

        const row = document.createElement("tr");
        row.setAttribute("data-index", index);
        row.innerHTML = `
            <td>${index + 1}</td>
            <td><input type="checkbox" id="chk-${index}" /></td>
            <td>${job.name}</td>
            <td>
                <select id="time-${index}" class="form-select" onchange="handleOccurenceChange(${index})">
                    ${occurenceOptions.map(option =>
                        `<option value="${option}" ${option === job.occurence ? "selected" : ""}>${option}</option>`
        ).join("")}
                </select>
            </td>
            <td id="time1-cell-${index}">
                ${timeHTML}
            </td>
            <td>
                <button onclick="saveRow(${index})" class="btn btn-sm btn-outline-primary">Save</button>
                <button onclick="deleteRow(${index})" class="btn btn-sm btn-outline-danger ms-1">Delete</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

function generateTimeInput(type, value, index) {
    if (type === "Everyday") {
        return `<input type="time" class="form-control" id="time1-${index}" value="${value}" onchange="handleTimeChange(${index})" />`;
    } else if (type === "Monthly" || type === "Quarterly") {
        let [day, time] = value.split("|");
        return `
            <div class="d-flex gap-1">
                <select class="form-select" id="day-${index}" onchange="handleTimeChange(${index})">
                    ${[...Array(31)].map((_, i) =>
            `<option value="${i + 1}" ${parseInt(day) === i + 1 ? "selected" : ""}>${i + 1}</option>`
        ).join("")}
                </select>
                <input type="time" class="form-control" id="time1-time-${index}" value="${time || ""}" onchange="handleTimeChange(${index})" />
            </div>
        `;
    } else if (type === "Weekly") {
        let [day, time] = value.split("|");
        const daysOfWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        return `
            <div class="d-flex gap-1">
                <select class="form-select" id="day-${index}" onchange="handleTimeChange(${index})">
                    ${daysOfWeek.map((dayName, i) =>
            `<option value="${i}" ${parseInt(day) === i ? "selected" : ""}>${dayName}</option>`
        ).join("")}
                </select>
                <input type="time" class="form-control" id="time1-time-${index}" value="${time || ""}" onchange="handleTimeChange(${index})" />
            </div>
        `;
    } else {
        return `<input type="datetime-local" class="form-control" id="time1-${index}" value="${formatForInput(value)}" onchange="handleTimeChange(${index})" />`;
    }
}

function formatForInput(datetimeStr) {
    const date = new Date(datetimeStr);
    if (isNaN(date)) return "";
    const offset = date.getTimezoneOffset();
    const local = new Date(date.getTime() - offset * 60000);
    return local.toISOString().slice(0, 16);
}

function handleOccurenceChange(index) {
    const selectedTime = document.getElementById(`time-${index}`).value;
    jobList[index].occurence = selectedTime;
    const current = jobList[index].time;

    if (selectedTime === "Weekly") {

        const today = new Date();
        jobList[index].time = `0|00:00`; // Default to Sunday and 00:00 time
    }
    else if (selectedTime === "Monthly" || selectedTime === "Quarterly") {
        const today = new Date();
        jobList[index].time = `${today.getDate()}|00:00`;
    } else if (selectedTime === "Everyday") {
        jobList[index].time = current.includes("|") ? current.split("|")[1] : "00:00";
    } else {
        const now = new Date();
        const offset = now.getTimezoneOffset();
        const local = new Date(now.getTime() - offset * 60000);
        jobList[index].time = local.toISOString().slice(0, 16);
    }
    renderTable();
}

function handleTimeChange(index) {
    const timeType = jobList[index].occurence;

    if (timeType === "Monthly" || timeType === "Quarterly") {
        const day = document.getElementById(`day-${index}`).value;
        const timestamp = document.getElementById(`time1-time-${index}`).value;
        jobList[index].time = `${day}|${timestamp}`;
    } else if (timeType === "Weekly") {
        const day = document.getElementById(`day-${index}`).value;
        const timestamp = document.getElementById(`time1-time-${index}`).value;
        jobList[index].time = `${day}|${timestamp}`;
    } else {
        const input = document.getElementById(`time1-${index}`);
        jobList[index].time = input.value;
    }
}

function saveRow(index) {
    const selectedTime = document.getElementById(`time-${index}`).value;
    const action = document.getElementById(`chk-${index}`).checked;
    const time = jobList[index].time;

    alert(`Saved "${jobList[index].name}" with Occurence "${selectedTime}" and Time "${time}" (Action: ${action ? "Enabled" : "Disabled"})`);
}

function deleteRow(index) {
    if (confirm(`Delete "${jobList[index].name}"?`)) {
        jobList.splice(index, 1);
        renderTable();
    }
}

function openAddForm() {
    document.getElementById("add-form").style.display = "block";
    document.getElementById("overlay").style.display = "block";
    document.getElementById("new-job-name").value = "";
    document.getElementById("new-job-occurence").value = "Everyday";

    renderAddFormTimeInput(); // Render default input for 'Everyday'

    // Ensure event listener is attached every time the form opens
    document.getElementById("new-job-occurence").removeEventListener("change", renderAddFormTimeInput);
    document.getElementById("new-job-occurence").addEventListener("change", renderAddFormTimeInput);
}



function closeAddForm() {
    document.getElementById("add-form").style.display = "none";
    document.getElementById("overlay").style.display = "none";
    document.getElementById("new-job-name").value = "";
    document.getElementById("new-job-occurence").value = "Everyday";
    document.getElementById("new-job-time").value = "";
}

function addJob() {
    const name = document.getElementById("new-job-name").value.trim();
    const occurence = document.getElementById("new-job-occurence").value;

    if (!name) {
        alert("Please enter a job name.");
        return;
    }

    let timeValue = "";
    if (occurence === "Everyday") {
        timeValue = document.getElementById("new-job-time").value;
    } else if (occurence === "Monthly" || occurence === "Quarterly" || occurence === "Weekly") {
        const day = document.getElementById("new-job-day").value;
        const time = document.getElementById("new-job-time").value;
        timeValue = `${day}|${time}`;
    } else {
        timeValue = document.getElementById("new-job-time").value;
    }

    jobList.push({ name, occurence, time: timeValue });
    closeAddForm();
    renderTable();
}


document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("new-job-occurence").addEventListener("change", renderAddFormTimeInput);
    renderAddFormTimeInput(); // Render default
});

function renderAddFormTimeInput() {
    const occurence = document.getElementById("new-job-occurence").value;
    const container = document.getElementById("new-job-time-container");
    container.innerHTML = generateAddFormTimeInput(occurence);
}

function generateAddFormTimeInput(type) {
    if (type === "Everyday") {
        return `<input type="time" class="form-control" id="new-job-time" value="08:00" />`;
    } else if (type === "Monthly" || type === "Quarterly") {
        return `
            <div class="d-flex gap-1">
                <select class="form-select" id="new-job-day">
                    ${[...Array(31)].map((_, i) => `<option value="${i + 1}">${i + 1}</option>`).join("")}
                </select>
                <input type="time" class="form-control" id="new-job-time" value="08:00" />
            </div>`;
    } else if (type === "Weekly") {
        const daysOfWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        return `
            <div class="d-flex gap-1">
                <select class="form-select" id="new-job-day">
                    ${daysOfWeek.map((day, i) => `<option value="${i}">${day}</option>`).join("")}
                </select>
                <input type="time" class="form-control" id="new-job-time" value="08:00" />
            </div>`;
    } else {
        const now = new Date().toISOString().slice(0, 16);
        return `<input type="datetime-local" class="form-control" id="new-job-time" value="${now}" />`;
    }
}


window.addEventListener("DOMContentLoaded", renderTable);
