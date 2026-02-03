// ============================================================================
// USERS.JS - Gestión de Usuarios
// Autor: GarthHMS Team
// Metodología: Partial Views + AJAX + SweetAlert2
// ============================================================================

let $table;

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    $table = $('#usersTable');
    loadUsers();
    setupEventHandlers();
});

// ============================================================================
// EVENT HANDLERS
// ============================================================================

function setupEventHandlers() {
    // Submit formulario crear
    $(document).on('submit', '#createForm', async function (e) {
        e.preventDefault();
        await handleCreate();
    });

    // Submit formulario editar
    $(document).on('submit', '#editForm', async function (e) {
        e.preventDefault();
        await handleUpdate();
    });

    // Submit formulario cambiar contraseña
    $(document).on('submit', '#changePasswordForm', async function (e) {
        e.preventDefault();
        await handleChangePassword();
    });
}

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadUsers() {
    try {
        const response = await fetch('/Users/GetAll');
        const result = await response.json();

        if (result.success) {
            $table.bootstrapTable('load', result.data);
        } else {
            showErrorToast(result.message || 'Error al cargar los usuarios');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        console.error('Error loading users:', error);
        showErrorToast('Error al cargar los usuarios');
    }
}

// ============================================================================
// FORMATTERS PARA BOOTSTRAP TABLE
// ============================================================================

function statusFormatter(value, row) {
    if (row.isActive) {
        return `<span class="badge bg-success">
                    <i class="fas fa-check-circle"></i> Activo
                </span>`;
    } else {
        return `<span class="badge bg-danger">
                    <i class="fas fa-times-circle"></i> Inactivo
                </span>`;
    }
}

function actionsFormatter(value, row) {
    return `
        <div class="btn-group" role="group">
            <button class="btn btn-sm btn-info" 
                    onclick="viewDetails('${row.userId}')" 
                    title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-sm btn-warning" 
                    onclick="openEditModal('${row.userId}')" 
                    title="Editar">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-${row.isActive ? 'secondary' : 'success'}" 
                    onclick="toggleUserStatus('${row.userId}')" 
                    title="${row.isActive ? 'Desactivar' : 'Activar'}">
                <i class="fas fa-${row.isActive ? 'ban' : 'check'}"></i>
            </button>
            <button class="btn btn-sm btn-primary" 
                    onclick="openChangePasswordModal('${row.userId}')" 
                    title="Cambiar contraseña">
                <i class="fas fa-key"></i>
            </button>
            <button class="btn btn-sm btn-danger" 
                    onclick="openDeleteModal('${row.userId}')" 
                    title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    `;
}

// ============================================================================
// MODAL CREAR USUARIO
// ============================================================================

async function openCreateModal() {
    try {
        const html = await fetchHTML('/Users/GetCreateForm');
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-user-plus"></i> Crear Nuevo Usuario',
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
        if (!formData.FirstName || !formData.LastName) {
            showErrorToast('El nombre y apellido son requeridos');
            return;
        }

        if (!formData.Email) {
            showErrorToast('El email es requerido');
            return;
        }

        if (!formData.Username) {
            showErrorToast('El nombre de usuario es requerido');
            return;
        }

        if (!formData.Password) {
            showErrorToast('La contraseña es requerida');
            return;
        }

        if (formData.Password.length < 6) {
            showErrorToast('La contraseña debe tener al menos 6 caracteres');
            return;
        }

        if (!formData.RoleId) {
            showErrorToast('Debe seleccionar un rol');
            return;
        }

        // Validar formato de email
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(formData.Email)) {
            showErrorToast('El formato del email no es válido');
            return;
        }

        showLoading('Creando usuario...');

        const result = await postJSON('/Users/Create', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadUsers();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating user:', error);
        showErrorToast(`Error: ${error.message || 'Error al crear el usuario'}`);
    }
}

// ============================================================================
// MODAL EDITAR USUARIO
// ============================================================================

async function openEditModal(userId) {
    try {
        const html = await fetchHTML(`/Users/GetEditForm/${userId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-edit"></i> Editar Usuario',
            html: html,
            width: '800px',
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

        // Validaciones básicas
        if (!formData.FirstName || !formData.LastName) {
            showErrorToast('El nombre y apellido son requeridos');
            return;
        }

        if (!formData.Email) {
            showErrorToast('El email es requerido');
            return;
        }

        if (!formData.Username) {
            showErrorToast('El nombre de usuario es requerido');
            return;
        }

        if (!formData.RoleId) {
            showErrorToast('Debe seleccionar un rol');
            return;
        }

        // Validar formato de email
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(formData.Email)) {
            showErrorToast('El formato del email no es válido');
            return;
        }

        showLoading('Actualizando usuario...');

        const result = await postJSON('/Users/Update', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadUsers();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating user:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar el usuario'}`);
    }
}

// ============================================================================
// MODAL VER DETALLES
// ============================================================================

async function viewDetails(userId) {
    try {
        const html = await fetchHTML(`/Users/GetDetails/${userId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-user-circle"></i> Detalles del Usuario',
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
// MODAL ELIMINAR USUARIO
// ============================================================================

async function openDeleteModal(userId) {
    try {
        const html = await fetchHTML(`/Users/GetDeleteConfirmation/${userId}`);
        if (!html) return;

        Swal.fire({
            html: html,
            width: '500px',
            showConfirmButton: false,
            showCloseButton: true
        });
    } catch (error) {
        console.error('Error opening delete modal:', error);
        showErrorToast('Error al abrir la confirmación');
    }
}

async function confirmDeleteAction(userId) {
    try {
        showLoading('Eliminando usuario...');

        const response = await fetch(`/Users/Delete/${userId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadUsers();
        } else {
            showErrorToast(result.message);
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting user:', error);
        showErrorToast('Error al eliminar el usuario');
    }
}

// ============================================================================
// MODAL CAMBIAR CONTRASEÑA
// ============================================================================

async function openChangePasswordModal(userId) {
    try {
        const html = await fetchHTML(`/Users/GetChangePasswordForm/${userId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-key"></i> Cambiar Contraseña',
            html: html,
            width: '600px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false
        });
    } catch (error) {
        console.error('Error opening change password modal:', error);
        showErrorToast('Error al abrir el formulario');
    }
}

async function handleChangePassword() {
    try {
        const formData = getFormData('changePasswordForm');

        // Validaciones
        if (!formData.CurrentPassword) {
            showErrorToast('La contraseña actual es requerida');
            return;
        }

        if (!formData.NewPassword) {
            showErrorToast('La nueva contraseña es requerida');
            return;
        }

        if (formData.NewPassword.length < 6) {
            showErrorToast('La nueva contraseña debe tener al menos 6 caracteres');
            return;
        }

        if (!formData.ConfirmPassword) {
            showErrorToast('Debe confirmar la nueva contraseña');
            return;
        }

        if (formData.NewPassword !== formData.ConfirmPassword) {
            showErrorToast('Las contraseñas no coinciden');
            return;
        }

        if (formData.CurrentPassword === formData.NewPassword) {
            showErrorToast('La nueva contraseña debe ser diferente a la actual');
            return;
        }

        showLoading('Cambiando contraseña...');

        const result = await postJSON('/Users/ChangePassword', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error changing password:', error);
        showErrorToast(`Error: ${error.message || 'Error al cambiar la contraseña'}`);
    }
}

// ============================================================================
// ACTIVAR/DESACTIVAR USUARIO
// ============================================================================

async function toggleUserStatus(userId) {
    try {
        // Obtener el usuario actual para saber su estado
        const response = await fetch('/Users/GetAll');
        const result = await response.json();

        if (!result.success) {
            showErrorToast('Error al obtener información del usuario');
            return;
        }

        const user = result.data.find(u => u.userId === userId);
        if (!user) {
            showErrorToast('Usuario no encontrado');
            return;
        }

        const action = user.isActive ? 'desactivar' : 'activar';
        const actionTitle = user.isActive ? 'Desactivar' : 'Activar';

        const confirmed = await Swal.fire({
            title: `¿${actionTitle} Usuario?`,
            html: `
                <div class="text-start">
                    <p>¿Estás seguro de ${action} a este usuario?</p>
                    <div class="alert alert-info">
                        <strong>${user.fullName}</strong><br>
                        <small>${user.email}</small>
                    </div>
                </div>
            `,
            icon: 'question',
            showCancelButton: true,
            reverseButtons: true,
            confirmButtonColor: user.isActive ? '#dc3545' : '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: `Sí, ${action}`,
            cancelButtonText: 'Cancelar'
        });

        if (!confirmed.isConfirmed) return;

        showLoading(`${actionTitle}ndo usuario...`);

        const toggleResponse = await fetch(`/Users/ToggleStatus/${userId}`, {
            method: 'POST'
        });

        const toggleResult = await toggleResponse.json();

        hideLoading();

        if (toggleResult.success) {
            showSuccessToast(toggleResult.message);
            await loadUsers();
        } else {
            showErrorToast(toggleResult.message);
            console.error('Error del servidor:', toggleResult);
        }
    } catch (error) {
        hideLoading();
        console.error('Error toggling user status:', error);
        showErrorToast('Error al cambiar el estado del usuario');
    }
}

// ============================================================================
// RESTABLECER CONTRASEÑA (ADMIN)
// ============================================================================

async function resetPassword(userId) {
    try {
        const confirmed = await Swal.fire({
            title: '¿Restablecer Contraseña?',
            html: `
                <div class="text-start">
                    <p>Se generará una contraseña temporal para este usuario.</p>
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle"></i>
                        <strong>Importante:</strong> Guarda la contraseña temporal y envíala al usuario de forma segura.
                    </div>
                </div>
            `,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ffc107',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, generar contraseña',
            cancelButtonText: 'Cancelar'
        });

        if (!confirmed.isConfirmed) return;

        showLoading('Generando contraseña temporal...');

        const response = await fetch(`/Users/ResetPassword/${userId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            await Swal.fire({
                title: 'Contraseña Restablecida',
                html: `
                    <div class="text-start">
                        <p>La contraseña temporal ha sido generada exitosamente.</p>
                        <div class="alert alert-success">
                            <strong>Contraseña Temporal:</strong><br>
                            <code style="font-size: 1.2rem; user-select: all;">${result.data.temporaryPassword}</code>
                        </div>
                        <div class="alert alert-warning">
                            <i class="fas fa-exclamation-triangle"></i>
                            <strong>Importante:</strong> Copia esta contraseña y envíala al usuario. No podrás verla de nuevo.
                        </div>
                    </div>
                `,
                icon: 'success',
                confirmButtonText: 'Entendido'
            });
        } else {
            showErrorToast(result.message);
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error resetting password:', error);
        showErrorToast('Error al restablecer la contraseña');
    }
}