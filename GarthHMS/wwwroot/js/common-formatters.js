// ============================================================================
// COMMON-FORMATTERS.JS - Formatters para Bootstrap Table
// Formatters reutilizables para mostrar datos en tablas
// ============================================================================

// ============================================================================
// FORMATTERS DE PRECIO/MONEDA
// ============================================================================

function priceFormatter(value, row) {
    if (value === null || value === undefined || value === 0) {
        return '<span class="text-muted">$0.00</span>';
    }
    return '<strong>$' + parseFloat(value).toFixed(2) + '</strong>';
}

function currencyFormatter(value, row, currency = 'MXN') {
    if (value === null || value === undefined) {
        return '<span class="text-muted">-</span>';
    }
    return '<strong>' + formatCurrency(value) + '</strong>';
}

// ============================================================================
// FORMATTERS DE ESTADO/BADGES
// ============================================================================

function statusFormatter(value, row) {
    if (value) {
        return '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Activo</span>';
    } else {
        return '<span class="badge bg-secondary"><i class="fas fa-times-circle"></i> Inactivo</span>';
    }
}

function booleanFormatter(value, row, trueText = 'Sí', falseText = 'No') {
    if (value) {
        return `<span class="badge bg-success">${trueText}</span>`;
    } else {
        return `<span class="badge bg-secondary">${falseText}</span>`;
    }
}

// ============================================================================
// FORMATTERS DE FECHA
// ============================================================================

function dateFormatter(value, row) {
    if (!value) return '<span class="text-muted">-</span>';
    return formatDate(value);
}

function dateTimeFormatter(value, row) {
    if (!value) return '<span class="text-muted">-</span>';
    return formatDateTime(value);
}

function relativeTimeFormatter(value, row) {
    if (!value) return '<span class="text-muted">-</span>';

    const date = new Date(value);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Ahora';
    if (diffMins < 60) return `Hace ${diffMins} min`;
    if (diffHours < 24) return `Hace ${diffHours} hrs`;
    if (diffDays < 7) return `Hace ${diffDays} días`;

    return formatDate(value);
}

// ============================================================================
// FORMATTERS DE HABITACIÓN
// ============================================================================

function roomStatusFormatter(value, row) {
    const statusMap = {
        'Available': { badge: 'badge-available', icon: 'fa-check-circle', text: 'Disponible' },
        'Occupied': { badge: 'badge-occupied', icon: 'fa-user', text: 'Ocupada' },
        'Dirty': { badge: 'badge-dirty', icon: 'fa-broom', text: 'Sucia' },
        'Cleaning': { badge: 'badge-cleaning', icon: 'fa-spray-can', text: 'Limpiando' },
        'Blocked': { badge: 'badge-blocked', icon: 'fa-ban', text: 'Bloqueada' },
        'Maintenance': { badge: 'badge-maintenance', icon: 'fa-wrench', text: 'Mantenimiento' }
    };

    const status = statusMap[value] || statusMap['Available'];
    return `<span class="badge ${status.badge}"><i class="fas ${status.icon}"></i> ${status.text}</span>`;
}

function capacityFormatter(value, row) {
    if (!value) return '<span class="text-muted">-</span>';
    return `<i class="fas fa-users"></i> ${value} ${value === 1 ? 'persona' : 'personas'}`;
}

// ============================================================================
// FORMATTERS DE NÚMERO
// ============================================================================

function numberFormatter(value, row) {
    if (value === null || value === undefined) {
        return '<span class="text-muted">-</span>';
    }
    return new Intl.NumberFormat('es-MX').format(value);
}

function percentageFormatter(value, row) {
    if (value === null || value === undefined) {
        return '<span class="text-muted">-</span>';
    }
    return `${parseFloat(value).toFixed(2)}%`;
}

// ============================================================================
// FORMATTERS DE TEXTO
// ============================================================================

function truncateFormatter(value, row, maxLength = 50) {
    if (!value) return '<span class="text-muted">-</span>';
    if (value.length <= maxLength) return value;
    return `<span title="${value}">${value.substring(0, maxLength)}...</span>`;
}

function emailFormatter(value, row) {
    if (!value) return '<span class="text-muted">-</span>';
    return `<a href="mailto:${value}"><i class="fas fa-envelope"></i> ${value}</a>`;
}

function phoneFormatter(value, row) {
    if (!value) return '<span class="text-muted">-</span>';
    return `<a href="tel:${value}"><i class="fas fa-phone"></i> ${value}</a>`;
}

function urlFormatter(value, row, text = 'Ver') {
    if (!value) return '<span class="text-muted">-</span>';
    return `<a href="${value}" target="_blank"><i class="fas fa-external-link-alt"></i> ${text}</a>`;
}

// ============================================================================
// FORMATTERS DE USUARIO/AUDITORÍA
// ============================================================================

function userFormatter(value, row) {
    if (!value) return '<span class="text-muted">Sistema</span>';
    return `<i class="fas fa-user"></i> ${value}`;
}

function createdByFormatter(value, row) {
    if (!row.createdBy) return '<span class="text-muted">-</span>';

    const date = row.createdAt ? formatDateTime(row.createdAt) : '';
    return `
        <small class="text-muted">
            <i class="fas fa-user"></i> ${row.createdBy}<br>
            <i class="fas fa-calendar"></i> ${date}
        </small>
    `;
}

// ============================================================================
// FORMATTERS DE PRIORIDAD
// ============================================================================

function priorityFormatter(value, row) {
    const priorityMap = {
        'Urgent': { class: 'danger', icon: 'fa-exclamation-circle', text: 'Urgente' },
        'Normal': { class: 'warning', icon: 'fa-circle', text: 'Normal' },
        'Low': { class: 'success', icon: 'fa-check-circle', text: 'Baja' }
    };

    const priority = priorityMap[value] || priorityMap['Normal'];
    return `<span class="badge bg-${priority.class}"><i class="fas ${priority.icon}"></i> ${priority.text}</span>`;
}

// ============================================================================
// FORMATTERS DE ACCIONES (Botones)
// ============================================================================

function actionsFormatter(value, row, options = {}) {
    const {
        view = true,
        edit = true,
        delete: del = true,
        viewFn = 'viewDetails',
        editFn = 'openEditModal',
        deleteFn = 'openDeleteModal',
        idField = 'id'
    } = options;

    const id = row[idField];
    let buttons = '<div class="btn-group" role="group">';

    if (view) {
        buttons += `
            <button class="btn btn-sm btn-info" onclick="${viewFn}('${id}')" title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
        `;
    }

    if (edit) {
        buttons += `
            <button class="btn btn-sm btn-warning" onclick="${editFn}('${id}')" title="Editar">
                <i class="fas fa-edit"></i>
            </button>
        `;
    }

    if (del) {
        buttons += `
            <button class="btn btn-sm btn-danger" onclick="${deleteFn}('${id}')" title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
        `;
    }

    buttons += '</div>';
    return buttons;
}

// Formatter de acciones simplificado (solo usa el row.id por defecto)
function simpleActionsFormatter(value, row) {
    return actionsFormatter(value, row, { idField: 'id' });
}

// ============================================================================
// FORMATTERS PERSONALIZADOS COMUNES
// ============================================================================

function imageFormatter(value, row, defaultImage = '/img/default.png') {
    const src = value || defaultImage;
    return `<img src="${src}" alt="Imagen" style="width: 50px; height: 50px; object-fit: cover; border-radius: 4px;">`;
}

function colorFormatter(value, row) {
    if (!value) return '<span class="text-muted">-</span>';
    return `
        <div style="display: inline-flex; align-items: center; gap: 8px;">
            <span style="display: inline-block; width: 20px; height: 20px; background-color: ${value}; border: 1px solid #dee2e6; border-radius: 4px;"></span>
            <span>${value}</span>
        </div>
    `;
}

function ratingFormatter(value, row, maxStars = 5) {
    if (value === null || value === undefined) {
        return '<span class="text-muted">-</span>';
    }

    let stars = '';
    const rating = parseFloat(value);

    for (let i = 1; i <= maxStars; i++) {
        if (i <= rating) {
            stars += '<i class="fas fa-star text-warning"></i>';
        } else if (i - 0.5 <= rating) {
            stars += '<i class="fas fa-star-half-alt text-warning"></i>';
        } else {
            stars += '<i class="far fa-star text-warning"></i>';
        }
    }

    return `<span>${stars} <small class="text-muted">(${rating.toFixed(1)})</small></span>`;
}

// ============================================================================
// EJEMPLO DE USO EN INDEX.CSHTML:
// ============================================================================
/*
<table id="myTable"
       data-toggle="table"
       ...>
    <thead>
        <tr>
            <th data-field="name" data-sortable="true">Nombre</th>
            <th data-field="price" data-formatter="priceFormatter">Precio</th>
            <th data-field="isActive" data-formatter="statusFormatter">Estado</th>
            <th data-field="createdAt" data-formatter="dateTimeFormatter">Fecha</th>
            <th data-field="actions" data-formatter="actionsFormatter">Acciones</th>
        </tr>
    </thead>
</table>
*/