// ============================================================================
// GARTH HMS - HOUR PACKAGES MODULE
// Metodología: Partial Views + AJAX + SweetAlert2
// ============================================================================

// ============================================================================
// VARIABLES GLOBALES
// ============================================================================

let $table;

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    console.log('Hour Packages module initialized');
    $table = $('#hourPackagesTable');
    loadHourPackages();
});

// Event handlers para formularios (delegados)
$(document).on('submit', '#createForm', function (e) {
    e.preventDefault();
    handleCreate();
});

$(document).on('submit', '#editForm', function (e) {
    e.preventDefault();
    handleUpdate();
});

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadHourPackages() {
    try {
        showLoading('Cargando paquetes de horas...');

        const response = await fetch('/HourPackages/GetAll');
        const result = await response.json();

        hideLoading();

        if (result.success) {
            $table.bootstrapTable('load', result.data);
            console.log('Paquetes cargados:', result.data.length);
        } else {
            showErrorToast(result.message || 'Error al cargar paquetes');
            console.error('Error al cargar paquetes:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error loading hour packages:', error);
        showErrorToast('Error al cargar los datos');
    }
}

// ============================================================================
// FORMATTERS (Bootstrap Table)
// ============================================================================

function hoursFormatter(value, row) {
    return `<span class="badge bg-info">
                <i class="fas fa-hourglass-half"></i> ${value} horas
            </span>`;
}

function priceFormatter(value, row) {
    return `<span class="text-success fw-bold">$${parseFloat(value).toFixed(2)}</span>`;
}

function extensionsFormatter(value, row) {
    if (value) {
        return `<span class="badge bg-success">
                    <i class="fas fa-check-circle"></i> Sí
                </span>`;
    } else {
        return `<span class="badge bg-secondary">
                    <i class="fas fa-times-circle"></i> No
                </span>`;
    }
}

function statusFormatter(value, row) {
    if (value) {
        return `<span class="badge bg-success">
                    <i class="fas fa-check-circle"></i> Activo
                </span>`;
    } else {
        return `<span class="badge bg-secondary">
                    <i class="fas fa-times-circle"></i> Inactivo
                </span>`;
    }
}

function actionsFormatter(value, row) {
    return `
        <div class="btn-group" role="group">
            <button class="btn btn-sm btn-info" 
                    onclick="viewDetails('${row.hourPackageId}')" 
                    title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-sm btn-warning" 
                    onclick="openEditModal('${row.hourPackageId}')" 
                    title="Editar">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-danger" 
                    onclick="openDeleteModal('${row.hourPackageId}')" 
                    title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    `;
}

// ============================================================================
// MODAL CREAR
// ============================================================================

async function openCreateModal() {
    try {
        const html = await fetchHTML('/HourPackages/GetCreateForm');
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-plus-circle text-primary"></i> Crear Paquete de Horas',
            html: html,
            width: '800px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false
        });
    } catch (error) {
        console.error('Error opening create modal:', error);
        showErrorToast('Error al abrir el formulario');
    }
}

async function handleCreate() {
    try {
        const formData = getFormData('createForm');

        // Validaciones básicas
        if (!formData.RoomTypeId) {
            showErrorToast('Debe seleccionar un tipo de habitación');
            return;
        }

        if (!formData.Name || !formData.Hours || !formData.Price) {
            showErrorToast('Por favor complete todos los campos requeridos');
            return;
        }

        // Validación de horas
        const hours = parseInt(formData.Hours);
        if (hours < 1 || hours > 48) {
            showErrorToast('Las horas deben estar entre 1 y 48');
            return;
        }

        // Validación de precio
        const price = parseFloat(formData.Price);
        if (price <= 0) {
            showErrorToast('El precio debe ser mayor a 0');
            return;
        }

        showLoading('Creando paquete de horas...');

        const result = await postJSON('/HourPackages/Create', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message || 'Paquete creado exitosamente');
            await loadHourPackages();
        } else {
            showErrorToast(result.message || 'Error al crear el paquete');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating hour package:', error);
        showErrorToast(`Error: ${error.message || 'Error al crear el paquete'}`);
    }
}

// ============================================================================
// MODAL EDITAR
// ============================================================================

async function openEditModal(hourPackageId) {
    try {
        const html = await fetchHTML(`/HourPackages/GetEditForm/${hourPackageId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-edit text-warning"></i> Editar Paquete de Horas',
            html: html,
            width: '800px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false
        });
    } catch (error) {
        console.error('Error opening edit modal:', error);
        showErrorToast('Error al abrir el formulario de edición');
    }
}

async function handleUpdate() {
    try {
        const formData = getFormData('editForm');

        // Validaciones básicas
        if (!formData.Name || !formData.Hours || !formData.Price) {
            showErrorToast('Por favor complete todos los campos requeridos');
            return;
        }

        // Validación de horas
        const hours = parseInt(formData.Hours);
        if (hours < 1 || hours > 48) {
            showErrorToast('Las horas deben estar entre 1 y 48');
            return;
        }

        // Validación de precio
        const price = parseFloat(formData.Price);
        if (price <= 0) {
            showErrorToast('El precio debe ser mayor a 0');
            return;
        }

        showLoading('Actualizando paquete...');

        const result = await postJSON('/HourPackages/Update', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message || 'Paquete actualizado exitosamente');
            await loadHourPackages();
        } else {
            showErrorToast(result.message || 'Error al actualizar el paquete');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating hour package:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar el paquete'}`);
    }
}

// ============================================================================
// MODAL VER DETALLES
// ============================================================================

async function viewDetails(hourPackageId) {
    try {
        const html = await fetchHTML(`/HourPackages/GetDetails/${hourPackageId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-info-circle text-info"></i> Detalles del Paquete',
            html: html,
            width: '900px',
            showConfirmButton: false,
            showCloseButton: true
        });
    } catch (error) {
        console.error('Error viewing details:', error);
        showErrorToast('Error al cargar los detalles');
    }
}

// ============================================================================
// MODAL ELIMINAR
// ============================================================================

async function openDeleteModal(hourPackageId) {
    try {
        const html = await fetchHTML(`/HourPackages/GetDeleteConfirmation/${hourPackageId}`);
        if (!html) return;

        Swal.fire({
            html: html,
            width: '600px',
            showConfirmButton: false,
            showCloseButton: true
        });
    } catch (error) {
        console.error('Error opening delete modal:', error);
        showErrorToast('Error al abrir la confirmación');
    }
}

async function confirmDeleteAction(hourPackageId) {
    try {
        showLoading('Eliminando paquete...');

        const response = await fetch(`/HourPackages/Delete/${hourPackageId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message || 'Paquete eliminado exitosamente');
            await loadHourPackages();
        } else {
            showErrorToast(result.message || 'Error al eliminar el paquete');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting hour package:', error);
        showErrorToast('Error al eliminar el paquete');
    }
}