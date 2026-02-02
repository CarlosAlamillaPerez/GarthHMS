// ============================================================================
// GARTH HMS - ROLES MODULE
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
    console.log('Roles module initialized');
    $table = $('#rolesTable');
    loadRoles();
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

$(document).on('submit', '#permissionsForm', function (e) {
    e.preventDefault();
    handleAssignPermissions();
});

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadRoles() {
    try {
        showLoading('Cargando roles...');

        const response = await fetch('/Roles/GetAll');
        const result = await response.json();

        hideLoading();

        if (result.success) {
            $table.bootstrapTable('load', result.data);
            console.log('Roles cargados:', result.data.length);
        } else {
            showErrorToast(result.message || 'Error al cargar roles');
            console.error('Error al cargar roles:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error loading roles:', error);
        showErrorToast('Error al cargar los datos');
    }
}

// ============================================================================
// MODAL CREAR
// ============================================================================

async function openCreateModal() {
    try {
        const html = await fetchHTML('/Roles/GetCreateForm');
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-plus-circle"></i> Nuevo Rol',
            html: html,
            width: '700px',
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

        // Validaciones
        if (!formData.Name || formData.Name.trim().length < 2) {
            showErrorToast('El nombre debe tener al menos 2 caracteres');
            return;
        }

        if (formData.Name.length > 50) {
            showErrorToast('El nombre no puede exceder 50 caracteres');
            return;
        }

        if (formData.MaxDiscountPercent < 0 || formData.MaxDiscountPercent > 100) {
            showErrorToast('El descuento debe estar entre 0 y 100');
            return;
        }

        // Convertir checkbox a boolean
        formData.IsManagerRole = $('#createIsManagerRole').is(':checked');
        formData.IsSystemRole = false; // Los usuarios nunca pueden crear roles de sistema

        showLoading('Creando rol...');

        const result = await postJSON('/Roles/Create', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message || 'Rol creado exitosamente');
            await loadRoles();
        } else {
            showErrorToast(result.message || 'Error al crear el rol');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating role:', error);
        showErrorToast('Error al crear el rol');
    }
}

// ============================================================================
// MODAL EDITAR
// ============================================================================

async function openEditModal(roleId) {
    try {
        const html = await fetchHTML(`/Roles/GetEditForm/${roleId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-edit"></i> Editar Rol',
            html: html,
            width: '700px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false
        });
    } catch (error) {
        console.error('Error opening edit modal:', error);
        showErrorToast('Error al abrir el formulario');
    }
}

async function handleUpdate() {
    try {
        const formData = getFormData('editForm');

        // Validaciones
        if (!formData.Name || formData.Name.trim().length < 2) {
            showErrorToast('El nombre debe tener al menos 2 caracteres');
            return;
        }

        if (formData.Name.length > 50) {
            showErrorToast('El nombre no puede exceder 50 caracteres');
            return;
        }

        if (formData.MaxDiscountPercent < 0 || formData.MaxDiscountPercent > 100) {
            showErrorToast('El descuento debe estar entre 0 y 100');
            return;
        }

        showLoading('Actualizando rol...');

        const result = await postJSON('/Roles/Update', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message || 'Rol actualizado exitosamente');
            await loadRoles();
        } else {
            showErrorToast(result.message || 'Error al actualizar el rol');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating role:', error);
        showErrorToast('Error al actualizar el rol');
    }
}

// ============================================================================
// MODAL VER DETALLES
// ============================================================================

async function viewDetails(roleId) {
    try {
        const html = await fetchHTML(`/Roles/GetDetails/${roleId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-info-circle"></i> Detalles del Rol',
            html: html,
            width: '800px',
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

async function openDeleteModal(roleId) {
    try {
        const html = await fetchHTML(`/Roles/GetDeleteConfirmation/${roleId}`);
        if (!html) return;

        Swal.fire({
            html: html,
            width: '550px',
            showConfirmButton: false,
            showCloseButton: true
        });
    } catch (error) {
        console.error('Error opening delete modal:', error);
        showErrorToast('Error al abrir la confirmación');
    }
}

async function confirmDeleteAction(roleId) {
    try {
        showLoading('Eliminando rol...');

        const response = await fetch(`/Roles/Delete/${roleId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message || 'Rol eliminado exitosamente');
            await loadRoles();
        } else {
            showErrorToast(result.message || 'Error al eliminar el rol');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting role:', error);
        showErrorToast('Error al eliminar el rol');
    }
}

// ============================================================================
// MODAL ASIGNAR PERMISOS
// ============================================================================

async function openPermissionsModal(roleId) {
    try {
        const html = await fetchHTML(`/Roles/GetPermissionsForm/${roleId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-shield-alt"></i> Gestionar Permisos',
            html: html,
            width: '900px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false
        });
    } catch (error) {
        console.error('Error opening permissions modal:', error);
        showErrorToast('Error al abrir el formulario de permisos');
    }
}

async function handleAssignPermissions() {
    try {
        // Obtener el RoleId del campo hidden
        const roleId = $('input[name="RoleId"]').val();

        // Obtener todos los checkboxes marcados
        const selectedPermissions = [];
        $('.permission-checkbox:checked').each(function () {
            selectedPermissions.push($(this).val());
        });

        const data = {
            RoleId: roleId,
            PermissionIds: selectedPermissions
        };

        showLoading('Asignando permisos...');

        const result = await postJSON('/Roles/AssignPermissions', data);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message || 'Permisos asignados exitosamente');
            await loadRoles();
        } else {
            showErrorToast(result.message || 'Error al asignar permisos');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error assigning permissions:', error);
        showErrorToast('Error al asignar permisos');
    }
}

// Función para seleccionar todos los permisos
function selectAllPermissions() {
    $('.permission-checkbox').prop('checked', true);
}

// Función para deseleccionar todos los permisos
function deselectAllPermissions() {
    $('.permission-checkbox').prop('checked', false);
}

// ============================================================================
// FORMATTERS PARA BOOTSTRAP TABLE
// ============================================================================

function discountFormatter(value, row) {
    if (value === 0) {
        return '<span class="badge bg-secondary">Sin descuento</span>';
    }
    return `<span class="badge bg-success">${value}%</span>`;
}

function roleTypeFormatter(value, row) {
    const colorMap = {
        'Sistema': 'primary',
        'Gerencial': 'warning',
        'Personalizado': 'info'
    };

    const color = colorMap[row.roleTypeBadge] || 'secondary';
    return `<span class="badge bg-${color}">${row.roleTypeBadge}</span>`;
}

function statusFormatter(value, row) {
    if (row.isActive) {
        return '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Activo</span>';
    }
    return '<span class="badge bg-secondary"><i class="fas fa-times-circle"></i> Inactivo</span>';
}

function actionsFormatter(value, row) {
    const isSystemRole = row.isSystemRole;

    let buttons = `
        <div class="btn-group btn-group-sm" role="group">
            <button class="btn btn-info" 
                    onclick="viewDetails('${row.roleId}')" 
                    title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-primary" 
                    onclick="openPermissionsModal('${row.roleId}')" 
                    title="Gestionar permisos">
                <i class="fas fa-shield-alt"></i>
            </button>`;

    // Solo mostrar botones de editar/eliminar si NO es rol del sistema
    if (!isSystemRole) {
        buttons += `
            <button class="btn btn-warning" 
                    onclick="openEditModal('${row.roleId}')" 
                    title="Editar">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-danger" 
                    onclick="openDeleteModal('${row.roleId}')" 
                    title="Eliminar">
                <i class="fas fa-trash-alt"></i>
            </button>`;
    } else {
        // Botón deshabilitado para roles del sistema
        buttons += `
            <button class="btn btn-secondary" 
                    disabled 
                    title="No se puede editar (rol del sistema)">
                <i class="fas fa-lock"></i>
            </button>`;
    }

    buttons += `</div>`;
    return buttons;
}

// ============================================================================
// FUNCIONES AUXILIARES
// ============================================================================

// Esta función se puede llamar desde el Index.cshtml si se necesita
window.loadRoles = loadRoles;