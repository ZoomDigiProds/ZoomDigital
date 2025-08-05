let originalContent = '';
let isEditing = false;

// Wait for DOM and initialize
function waitForElementsAndInitialize() {
    const textarea = document.getElementById('smsContent-input');
    const editBtn = document.getElementById('edit-btn');

    if (textarea && editBtn) {
        initializeSmsSettings();
    } else {
        setTimeout(waitForElementsAndInitialize, 500);
    }
}

document.addEventListener('DOMContentLoaded', function () {
    waitForElementsAndInitialize();
});

if (document.readyState !== 'loading') {
    waitForElementsAndInitialize();
}

window.initializeSmsSettings = function () {
    initializeSmsSettings();
};

function initializeSmsSettings() {
    const textarea = document.getElementById('smsContent-input');
    const editBtn = document.getElementById('edit-btn');
    const saveBtn = document.getElementById('save-btn');
    const cancelBtn = document.getElementById('cancel-btn');

    if (!textarea || !editBtn) return false;

    if (editBtn.hasAttribute('data-sms-initialized')) return true;

    originalContent = textarea.value;

    editBtn.addEventListener('click', function (e) {
        e.preventDefault();
        toggleEdit();
    });
    editBtn.setAttribute('data-sms-initialized', 'true');

    if (saveBtn) {
        saveBtn.addEventListener('click', function (e) {
            e.preventDefault();
            saveSmsContent();
        });
    }

    if (cancelBtn) {
        cancelBtn.addEventListener('click', function (e) {
            e.preventDefault();
            cancelEdit();
        });
    }

    return true;
}

function toggleEdit() {
    const textarea = document.getElementById('smsContent-input');
    const editBtn = document.getElementById('edit-btn');
    const saveBtn = document.getElementById('save-btn');
    const cancelBtn = document.getElementById('cancel-btn');

    if (!textarea || isEditing) return;

    originalContent = textarea.value;

    textarea.disabled = false;
    textarea.readOnly = false;

    if (editBtn) editBtn.style.display = 'none';
    if (saveBtn) saveBtn.style.display = 'inline-block';
    if (cancelBtn) cancelBtn.style.display = 'inline-block';

    isEditing = true;

    setTimeout(() => textarea.focus(), 100);
}

function cancelEdit() {
    const textarea = document.getElementById('smsContent-input');
    const editBtn = document.getElementById('edit-btn');
    const saveBtn = document.getElementById('save-btn');
    const cancelBtn = document.getElementById('cancel-btn');

    if (!textarea) return;

    textarea.value = originalContent;
    textarea.disabled = true;
    textarea.readOnly = true;

    if (editBtn) editBtn.style.display = 'inline-block';
    if (saveBtn) saveBtn.style.display = 'none';
    if (cancelBtn) cancelBtn.style.display = 'none';

    isEditing = false;

    clearAlerts();
}

function saveSmsContent() {
    const textarea = document.getElementById('smsContent-input');
    const loadingIndicator = document.getElementById('loading-indicator');

    if (!textarea) return;

    const content = textarea.value;

    if (!content.trim()) {
        alert("SMS content cannot be empty.")
        return;
    }

    if (loadingIndicator) loadingIndicator.style.display = 'block';

    const buttons = document.querySelectorAll('#sms-content-container button');
    buttons.forEach(btn => btn.disabled = true);

    const formData = new FormData();
    formData.append('content', content);

    fetch('/Settings/UpdateSmsContent', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (loadingIndicator) loadingIndicator.style.display = 'none';

            if (data.success) {
                originalContent = content;
                textarea.disabled = true;
                textarea.readOnly = true;

                const editBtn = document.getElementById('edit-btn');
                const saveBtn = document.getElementById('save-btn');
                const cancelBtn = document.getElementById('cancel-btn');

                if (editBtn) editBtn.style.display = 'inline-block';
                if (saveBtn) saveBtn.style.display = 'none';
                if (cancelBtn) cancelBtn.style.display = 'none';

                isEditing = false;

                alert("Message saved successfully.")
            } else {
                alert("Error while saving. Please save again.")
            }
        })
        .catch(() => {
            if (loadingIndicator) loadingIndicator.style.display = 'none';
            alert('An error occurred while saving. Please try again.');
        })
        .finally(() => {
            buttons.forEach(btn => btn.disabled = false);
        });
}

function clearAlerts() {
    const alertContainer = document.getElementById('alert-container');
    if (alertContainer) {
        alertContainer.innerHTML = '';
    }
}

// Ctrl+S to Save
document.addEventListener('keydown', function (e) {
    if (e.ctrlKey && e.key === 's' && isEditing) {
        e.preventDefault();
        saveSmsContent();
    }
});
