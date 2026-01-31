// ============================================================================
// COMMON.JS - Funciones Globales de GARTH HMS
// ============================================================================

// ============================================================================
// TOAST CONFIGURATION (SweetAlert2)
// ============================================================================

const Toast = Swal.mixin({
    toast: true,
    position: "top-end",
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    didOpen: (toast) => {
        toast.onmouseenter = Swal.stopTimer;
        toast.onmouseleave = Swal.resumeTimer;
    }
});

// ============================================================================
// TOAST HELPERS
// ============================================================================

function showSuccessToast(message) {
    Toast.fire({
        icon: "success",
        title: message
    });
}

function showErrorToast(message) {
    Toast.fire({
        icon: "error",
        title: message
    });
}

function showInfoToast(message) {
    Toast.fire({
        icon: "info",
        title: message
    });
}

function showWarningToast(message) {
    Toast.fire({
        icon: "warning",
        title: message
    });
}

// ============================================================================
// LOADING HELPERS
// ============================================================================

function showLoading(message = 'Cargando...') {
    Swal.fire({
        title: message,
        allowOutsideClick: false,
        allowEscapeKey: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

function hideLoading() {
    Swal.close();
}

// ============================================================================
// CONFIRMATION DIALOG
// ============================================================================

async function confirmAction(title, text, confirmButtonText = 'Sí, continuar') {
    const result = await Swal.fire({
        title: title,
        text: text,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#2ba49a',
        cancelButtonColor: '#6c757d',
        confirmButtonText: confirmButtonText,
        cancelButtonText: 'Cancelar'
    });

    return result.isConfirmed;
}

// ============================================================================
// FORMAT HELPERS
// ============================================================================

function formatCurrency(amount) {
    return new Intl.NumberFormat('es-MX', {
        style: 'currency',
        currency: 'MXN'
    }).format(amount);
}

function formatDate(date) {
    return new Intl.DateTimeFormat('es-MX').format(new Date(date));
}

function formatDateTime(date) {
    return new Intl.DateTimeFormat('es-MX', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(date));
}

// ============================================================================
// AJAX HELPERS
// ============================================================================

async function fetchHTML(url) {
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.text();
    } catch (error) {
        console.error('Error fetching HTML:', error);
        showErrorToast('Error al cargar el contenido');
        return null;
    }
}

async function postJSON(url, data) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });
        return await response.json();
    } catch (error) {
        console.error('Error posting JSON:', error);
        showErrorToast('Error al procesar la solicitud');
        return { success: false, message: 'Error de red' };
    }
}

// ============================================================================
// FORM HELPERS
// ============================================================================

function getFormData(formId) {
    const form = document.getElementById(formId);
    const formData = new FormData(form);
    const data = {};

    for (let [key, value] of formData.entries()) {
        const element = form.elements[key];

        if (!element) {
            data[key] = value;
            continue;
        }

        // Si es checkbox
        if (element.type === 'checkbox') {
            data[key] = element.checked;
        }
        // Si es un campo numérico
        else if (element.type === 'number') {
            // Si está vacío, enviar null en lugar de string vacío
            if (value === '' || value === null) {
                data[key] = null;
            } else {
                // Convertir a número (int o decimal)
                data[key] = element.step && element.step.includes('.')
                    ? parseFloat(value)
                    : parseInt(value);
            }
        }
        // Si es un campo de texto
        else {
            // Si está vacío, enviar null en lugar de string vacío para campos opcionales
            data[key] = value === '' ? null : value;
        }
    }

    return data;
}

function resetForm(formId) {
    document.getElementById(formId).reset();
}

// ============================================================================
// TABLE HELPERS
// ============================================================================

function showTableLoading(tableId, colspan) {
    $(`#${tableId} tbody`).html(`
        <tr>
            <td colspan="${colspan}" class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                <p class="mt-2 text-muted">Cargando datos...</p>
            </td>
        </tr>
    `);
}

function showTableEmpty(tableId, colspan, message = 'No hay datos para mostrar') {
    $(`#${tableId} tbody`).html(`
        <tr>
            <td colspan="${colspan}" class="text-center text-muted py-4">
                <i class="fas fa-inbox fa-3x mb-3"></i>
                <p>${message}</p>
            </td>
        </tr>
    `);
}