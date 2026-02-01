// ============================================================================
// COMMON-VALIDATIONS.JS - Validaciones Reutilizables
// ============================================================================

// ============================================================================
// VALIDACIONES BÁSICAS
// ============================================================================

function isRequired(value, fieldName = 'Campo') {
    if (!value || value.trim() === '') {
        showErrorToast(`${fieldName} es requerido`);
        return false;
    }
    return true;
}

function isEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!regex.test(email)) {
        showErrorToast('Email inválido');
        return false;
    }
    return true;
}

function isNumeric(value, fieldName = 'Campo') {
    if (isNaN(value)) {
        showErrorToast(`${fieldName} debe ser numérico`);
        return false;
    }
    return true;
}

function isPositive(value, fieldName = 'Campo') {
    if (parseFloat(value) < 0) {
        showErrorToast(`${fieldName} debe ser positivo`);
        return false;
    }
    return true;
}

function isInteger(value, fieldName = 'Campo') {
    if (!Number.isInteger(Number(value))) {
        showErrorToast(`${fieldName} debe ser un número entero`);
        return false;
    }
    return true;
}

// ============================================================================
// VALIDACIONES DE RANGOS
// ============================================================================

function isInRange(value, min, max, fieldName = 'Campo') {
    const num = parseFloat(value);
    if (num < min || num > max) {
        showErrorToast(`${fieldName} debe estar entre ${min} y ${max}`);
        return false;
    }
    return true;
}

function isMinLength(value, minLength, fieldName = 'Campo') {
    if (value.length < minLength) {
        showErrorToast(`${fieldName} debe tener al menos ${minLength} caracteres`);
        return false;
    }
    return true;
}

function isMaxLength(value, maxLength, fieldName = 'Campo') {
    if (value.length > maxLength) {
        showErrorToast(`${fieldName} no puede exceder ${maxLength} caracteres`);
        return false;
    }
    return true;
}

// ============================================================================
// VALIDACIONES DE COMPARACIÓN
// ============================================================================

function isGreaterThan(value1, value2, field1Name, field2Name) {
    if (parseFloat(value1) <= parseFloat(value2)) {
        showErrorToast(`${field1Name} debe ser mayor que ${field2Name}`);
        return false;
    }
    return true;
}

function isGreaterOrEqual(value1, value2, field1Name, field2Name) {
    if (parseFloat(value1) < parseFloat(value2)) {
        showErrorToast(`${field1Name} debe ser mayor o igual a ${field2Name}`);
        return false;
    }
    return true;
}

// ============================================================================
// VALIDACIONES ESPECÍFICAS
// ============================================================================

function isValidPhone(phone) {
    // Acepta formatos: 1234567890, (123) 456-7890, 123-456-7890
    const regex = /^[\d\s\-\(\)]+$/;
    if (!regex.test(phone) || phone.replace(/\D/g, '').length < 10) {
        showErrorToast('Teléfono inválido (mínimo 10 dígitos)');
        return false;
    }
    return true;
}

function isValidURL(url) {
    try {
        new URL(url);
        return true;
    } catch {
        showErrorToast('URL inválida');
        return false;
    }
}

function isValidDate(dateString) {
    const date = new Date(dateString);
    if (isNaN(date.getTime())) {
        showErrorToast('Fecha inválida');
        return false;
    }
    return true;
}

function isFutureDate(dateString, fieldName = 'Fecha') {
    const date = new Date(dateString);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (date < today) {
        showErrorToast(`${fieldName} debe ser futura`);
        return false;
    }
    return true;
}

function isPastDate(dateString, fieldName = 'Fecha') {
    const date = new Date(dateString);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (date > today) {
        showErrorToast(`${fieldName} debe ser pasada`);
        return false;
    }
    return true;
}

// ============================================================================
// VALIDACIÓN DE FORMULARIO COMPLETO
// ============================================================================

function validateForm(formId, rules) {
    const form = document.getElementById(formId);
    if (!form) {
        console.error(`Form ${formId} not found`);
        return false;
    }

    for (const [fieldName, rule] of Object.entries(rules)) {
        const field = form.elements[fieldName];
        if (!field) continue;

        const value = field.value;

        // Required
        if (rule.required && !isRequired(value, rule.label || fieldName)) {
            field.focus();
            return false;
        }

        // Skip further validation if empty and not required
        if (!value && !rule.required) continue;

        // Email
        if (rule.email && !isEmail(value)) {
            field.focus();
            return false;
        }

        // Numeric
        if (rule.numeric && !isNumeric(value, rule.label || fieldName)) {
            field.focus();
            return false;
        }

        // Positive
        if (rule.positive && !isPositive(value, rule.label || fieldName)) {
            field.focus();
            return false;
        }

        // Min/Max
        if (rule.min !== undefined && !isInRange(value, rule.min, Infinity, rule.label || fieldName)) {
            field.focus();
            return false;
        }

        if (rule.max !== undefined && !isInRange(value, -Infinity, rule.max, rule.label || fieldName)) {
            field.focus();
            return false;
        }

        // Length
        if (rule.minLength && !isMinLength(value, rule.minLength, rule.label || fieldName)) {
            field.focus();
            return false;
        }

        if (rule.maxLength && !isMaxLength(value, rule.maxLength, rule.label || fieldName)) {
            field.focus();
            return false;
        }

        // Custom validator
        if (rule.custom && !rule.custom(value, field)) {
            field.focus();
            return false;
        }
    }

    return true;
}

// ============================================================================
// EJEMPLO DE USO:
// ============================================================================
/*
const validationRules = {
    Name: { required: true, minLength: 3, maxLength: 100, label: 'Nombre' },
    Code: { required: true, minLength: 2, maxLength: 10, label: 'Código' },
    BaseCapacity: { required: true, numeric: true, positive: true, label: 'Capacidad Base' },
    MaxCapacity: { required: true, numeric: true, positive: true, label: 'Capacidad Máxima' },
    BasePriceNightly: { required: true, numeric: true, positive: true, label: 'Precio por Noche' },
    Email: { required: true, email: true, label: 'Email' }
};

if (validateForm('createForm', validationRules)) {
    // Formulario válido, continuar con submit
}
*/