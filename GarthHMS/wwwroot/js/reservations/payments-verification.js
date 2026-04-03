// GarthHMS/wwwroot/js/reservations/payments-verification.js
// Componente 6 — Verificación de Anticipos

'use strict';

// ── Estado del módulo ────────────────────────────────────────────────────────
let pvAllPayments = [];

// ── Init ─────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    loadPendingPayments();
});

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadPendingPayments() {
    try {
        const res = await fetch('/Payments/GetPendingVerifications');
        const data = await res.json();

        if (!data.success) {
            console.error('Error al cargar pagos pendientes:', data.message);
            showErrorToast('Error al cargar los pagos pendientes');
            return;
        }

        pvAllPayments = data.data ?? [];
        updateKpis(pvAllPayments);
        $('#pendingPaymentsTable').bootstrapTable('load', pvAllPayments);

    } catch (error) {
        console.error('Error de conexión al cargar pagos pendientes:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
    }
}

// ============================================================================
// KPIs
// ============================================================================

function updateKpis(payments) {
    const count = payments.length;
    const total = payments.reduce((sum, p) => sum + (p.amount ?? 0), 0);
    const oldest = payments.length > 0 ? payments[0].paymentDate : null;

    document.getElementById('kpi-count').textContent = count;
    document.getElementById('kpi-amount').textContent = formatCurrency(total);
    document.getElementById('kpi-oldest').textContent = oldest
        ? new Date(oldest).toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
        : '—';
}

// ============================================================================
// ACCIÓN — VERIFICAR PAGO
// ============================================================================

async function verifyPayment(paymentId, folio, guestName, amount, method) {
    const confirm = await Swal.fire({
        title: '¿Confirmar recepción?',
        html: `
            <div class="text-start">
                <p class="mb-3 text-muted">Estás confirmando que el siguiente pago fue recibido en el banco:</p>
                <div class="d-flex flex-column gap-2">
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Reserva</span>
                        <strong>${folio}</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Huésped</span>
                        <strong>${guestName}</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Método</span>
                        <strong>${method}</strong>
                    </div>
                    <hr class="my-1">
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Monto</span>
                        <strong class="text-success fs-5">${formatCurrency(amount)}</strong>
                    </div>
                </div>
            </div>`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check me-1"></i>Sí, confirmar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: 'var(--success)',
        cancelButtonColor: 'var(--text-muted)',
        reverseButtons: true
    });

    if (!confirm.isConfirmed) return;

    try {
        const res = await fetch('/Payments/Verify', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ paymentId })
        });
        const data = await res.json();

        if (!data.success) {
            console.error('Error al verificar pago:', data.message);
            showErrorToast(data.message || 'Error al verificar el pago');
            return;
        }

        showSuccessToast('Pago verificado correctamente');
        loadPendingPayments();

    } catch (error) {
        console.error('Error de conexión al verificar pago:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
    }
}

// ============================================================================
// FORMATTERS — Bootstrap Table
// ============================================================================

function pv_checkInFormatter(value) {
    if (!value) return '—';
    return new Date(value).toLocaleDateString('es-MX', {
        day: '2-digit', month: 'short', year: 'numeric'
    });
}

function pv_dateFormatter(value) {
    if (!value) return '—';
    const d = new Date(value);
    return d.toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
        + ' ' + d.toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });
}

function pv_amountFormatter(value) {
    return `<strong class="text-success">${formatCurrency(value ?? 0)}</strong>`;
}

function pv_methodFormatter(value) {
    const cfg = {
        'Transferencia': { icon: 'fa-building-columns', color: 'var(--info)' },
        'Tarjeta': { icon: 'fa-credit-card', color: 'var(--tertiary)' },
        'Efectivo': { icon: 'fa-money-bill-wave', color: 'var(--success)' },
    };
    const c = cfg[value] ?? { icon: 'fa-circle-question', color: 'var(--text-muted)' };
    return `<span style="color:${c.color};">
                <i class="fas ${c.icon} me-1"></i>${value ?? '—'}
            </span>`;
}

function pv_referenceFormatter(value) {
    if (!value) return '<span class="text-muted fst-italic">Sin referencia</span>';
    return `<code class="small">${value}</code>`;
}

function pv_actionsFormatter(value, row) {
    return `
        <button class="btn btn-sm btn-success"
                onclick="verifyPayment(
                    '${row.paymentId}',
                    '${row.folio}',
                    '${(row.guestFullName ?? '').replace(/'/g, "\\'")}',
                    ${row.amount ?? 0},
                    '${row.methodLabel ?? ''}'
                )"
                title="Confirmar recepción del pago">
            <i class="fas fa-check me-1"></i>Confirmar
        </button>`;
}