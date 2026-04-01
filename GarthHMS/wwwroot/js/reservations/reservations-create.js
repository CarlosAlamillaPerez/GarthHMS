// =============================================================================
// reservations-create.js
// Módulo JavaScript para el formulario de creación de reservas (Nightly)
// GarthHMS — Componente 3
// =============================================================================

// ─── Estado del módulo ──────────────────────────────────────────────────────
const ResCreate = {
    // Huésped seleccionado
    guest: null,                // { guestId, firstName, lastName, phone, email, isVip }

    // Habitaciones agregadas
    rooms: [],                  // [{ roomId, roomTypeId, roomNumber, floor, roomTypeName, roomTypeCode, pricePerNight, numAdults, ... }]

    // Selector temporal (modal)
    sel: {
        roomTypes: [],          // tipos de habitación disponibles
        availableRooms: [],     // habitaciones del tipo seleccionado
        selectedTypeId: null,
        selectedRoom: null,
    },

    // Financiero calculado
    finance: {
        basePrice: 0,
        discountAmount: 0,
        discountPercent: 0,
        taxIva: 0,
        taxIsh: 0,
        total: 0,
        depositAmount: 0,
        balance: 0,
    },

    // Timer para quick search
    searchTimer: null,
    depositState: 'none',
    requiresInvoice: false,
    paymentSplits: []
};

// ─── Inicialización ─────────────────────────────────────────────────────────
$(document).ready(function () {
    initSourceButtons();
    initDates();

    // Si estamos en modo edición, precargar datos
    if (window.IsEditMode && window.EditReservation) {
        preloadEditData(window.EditReservation);
    }
});

// ─── Botones de canal (radio) ────────────────────────────────────────────────
function initSourceButtons() {
    document.querySelectorAll('.source-btn').forEach(btn => {

        btn.addEventListener('click', () => {
            // Reset estilos
            document.querySelectorAll('.source-btn').forEach(b => {
                b.style.background = '';
                b.style.color = '';
                b.style.borderColor = '';
            });

            // Aplicar estilo al seleccionado
            btn.style.background = 'var(--primary)';
            btn.style.color = '#fff';
            btn.style.borderColor = 'var(--primary)';

            // Marcar radio
            const radio = btn.querySelector('input[type=radio]');
            if (radio) radio.checked = true;

            if (btn.dataset.value) {
                onSourceChange(btn.dataset.value);
            }
        });

        // Estado inicial
        const radio = btn.querySelector('input[type=radio]');
        if (radio?.checked) {
            btn.style.background = 'var(--primary)';
            btn.style.color = '#fff';
            btn.style.borderColor = 'var(--primary)';
        }
    });
}

// ─── Fechas ──────────────────────────────────────────────────────────────────
const LATE_NIGHT_CUTOFF_HOUR = 5; // antes de las 5am se permite día anterior

function getMinCheckInDate() {
    const now = new Date();
    const hour = now.getHours();

    // Usar fecha local explícita, no ISO (que convierte a UTC)
    const pad = n => String(n).padStart(2, '0');
    const localToday = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;

    if (hour < LATE_NIGHT_CUTOFF_HOUR) {
        const yesterday = new Date(now);
        yesterday.setDate(yesterday.getDate() - 1);
        return `${yesterday.getFullYear()}-${pad(yesterday.getMonth() + 1)}-${pad(yesterday.getDate())}`;
    }
    return localToday;
}

function initDates() {
    const now = new Date();
    const pad = n => String(n).padStart(2, '0');
    const localToday = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;

    document.getElementById('checkInDate').min = getMinCheckInDate();
    document.getElementById('checkOutDate').min = localToday;
}

function onDatesChange() {
    const ci = document.getElementById('checkInDate').value;
    const co = document.getElementById('checkOutDate').value;
    const now = new Date();
    const pad = n => String(n).padStart(2, '0');

    // Fecha local — NO usar toISOString() porque convierte a UTC
    const today = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;

    // Limpiar aviso anterior
    const existingAlert = document.getElementById('lateNightAlert');
    if (existingAlert) existingAlert.remove();

    if (!ci) return;

    // Validar que no sea fecha pasada fuera de ventana madrugada
    if (ci < today) {
        if (now.getHours() >= LATE_NIGHT_CUTOFF_HOUR) {
            showWarningToast('No se puede seleccionar una fecha pasada después de las 5:00am');
            document.getElementById('checkInDate').value = '';
            return;
        }

        // Es día anterior y estamos en ventana de madrugada — mostrar aviso
        const horasRestantes = LATE_NIGHT_CUTOFF_HOUR - now.getHours();
        const minutosRestantes = 60 - now.getMinutes();
        const tiempoTexto = horasRestantes > 1
            ? `${horasRestantes} horas`
            : `${minutosRestantes} minutos`;

        const alert = document.createElement('div');
        alert.id = 'lateNightAlert';
        alert.className = 'alert alert-warning d-flex align-items-start gap-2 mt-2 mb-0 py-2';
        alert.innerHTML = `
            <i class="fas fa-moon mt-1 text-warning"></i>
            <div>
                <strong>Reserva de madrugada</strong> — Esta reserva contará a partir de ayer
                (${formatDate(ci)}). El check-out será a la hora normal aunque el huésped
                haya llegado esta madrugada.
                <br><small class="text-muted">Esta opción estará disponible por aproximadamente ${tiempoTexto} más (hasta las 5:00am).</small>
            </div>`;

        document.getElementById('checkInDate').closest('.col-md-4')
            .insertAdjacentElement('afterend', alert);

        // Mover el alert fuera del col para que ocupe todo el ancho
        document.getElementById('checkInDate').closest('.row')
            .insertAdjacentElement('afterend', alert);
    }

    if (!co) return;

    if (co <= ci) {
        showWarningToast('La fecha de check-out debe ser posterior al check-in');
        document.getElementById('checkOutDate').value = '';
        document.getElementById('nightsDisplay').textContent = '—';
        return;
    }

    const nights = Math.round((new Date(co) - new Date(ci)) / 86400000);
    document.getElementById('nightsDisplay').textContent =
        `${nights} noche${nights !== 1 ? 's' : ''}`;

    recalculate();
    renderRoomsTable();
}

function getNights() {
    const ci = document.getElementById('checkInDate').value;
    const co = document.getElementById('checkOutDate').value;
    if (!ci || !co || co <= ci) return 0;
    return Math.round((new Date(co) - new Date(ci)) / 86400000);
}

// ─── Huéspedes (contadores) ──────────────────────────────────────────────────
function changeGuests(field, delta) {
    const el = document.getElementById(`total${field.charAt(0).toUpperCase() + field.slice(1)}`);
    const min = field === 'adults' ? 1 : 0;
    const max = field === 'adults' ? 20 : (field === 'children' ? 10 : 5);
    const newVal = Math.max(min, Math.min(max, parseInt(el.value || 0) + delta));
    el.value = newVal;
    recalculate();
}

// ─── Mascotas ────────────────────────────────────────────────────────────────
function togglePets(checked) {
    document.getElementById('petsCountArea').style.display = checked ? 'flex' : 'none';
    if (!checked) document.getElementById('petsCount').value = 1;
}

function changePets(delta) {
    const el = document.getElementById('petsCount');
    const max = parseInt(el.max) || 3;
    el.value = Math.max(1, Math.min(max, parseInt(el.value) + delta));
}

// ─── BÚSQUEDA DE HUÉSPED ─────────────────────────────────────────────────────

function onGuestQuickSearch(query) {
    clearTimeout(ResCreate.searchTimer);
    const box = document.getElementById('guestQuickResults');

    if (query.length < 2) { box.style.display = 'none'; return; }

    ResCreate.searchTimer = setTimeout(async () => {
        try {
            const res = await fetch(`/Guests/Search?query=${encodeURIComponent(query)}&maxResults=6`);
            const data = await res.json();

            if (!data.success || !data.data?.length) {
                box.innerHTML = `<div class="px-3 py-2 text-muted small">Sin resultados</div>`;
                box.style.display = 'block';
                return;
            }

            box.innerHTML = data.data.map(g => `
                <div class="d-flex align-items-center gap-2 px-3 py-2 border-bottom quick-result-item"
                     style="cursor:pointer; transition:.1s;"
                     onclick='selectGuestFromSearch(${JSON.stringify(g)})'
                     onmouseenter="this.style.background='var(--bg-secondary)'"
                     onmouseleave="this.style.background=''">
                    <div style="width:32px;height:32px;border-radius:50%;background:var(--primary);color:#fff;
                                display:flex;align-items:center;justify-content:center;font-weight:700;font-size:.85rem;flex-shrink:0;">
                        ${(g.firstName || g.first_name || '?')[0].toUpperCase()}
                    </div>
                    <div>
                        <div style="font-weight:600;font-size:.88rem;">${g.fullName || g.firstName + ' ' + g.lastName || ''}</div>
                        <div style="font-size:.78rem;color:var(--text-secondary);">${g.phone || ''} · ${g.email || ''}</div>
                    </div>
                    ${(g.isVip || g.is_vip) ? '<span class="badge bg-warning text-dark ms-auto"><i class="fas fa-star"></i></span>' : ''}
                </div>
            `).join('');
            box.style.display = 'block';
        } catch (err) {
            console.error('Error en quick search de huéspedes:', err);
        }
    }, 350);
}

function selectGuestFromSearch(guest) {
    document.getElementById('guestQuickResults').style.display = 'none';
    document.getElementById('guestQuickSearch').value = '';
    setSelectedGuest({
        guestId: guest.guestId,
        firstName: guest.fullName,
        lastName: '',
        phone: guest.phone,
        email: guest.email,
        isVip: guest.isVip || false,
        billingRfc: guest.billingRfc || null,          
        billingBusinessName: guest.billingBusinessName || null, 
        billingEmail: guest.billingEmail || null         
    });
}

// Abre el modal con Bootstrap Table para buscar huéspedes
async function openGuestSearchModal() {
    try {
        // Cargar datos iniciales
        const res = await fetch('/Guests/GetAll?pageSize=20&sortBy=created_at&sortOrder=desc');
        const data = await res.json();
        const guests = data?.data || [];

        const tableRows = guests.map(g => `
            <tr style="cursor:pointer;"
                onclick='chooseGuestFromModal(${JSON.stringify({
                    guestId: g.guestId,
                    firstName: g.fullName,
                    lastName: '',
                    phone: g.phone,
                    email: g.email,
                    isVip: g.isVip,
                    billingRfc: g.billingRfc || null,              
                    billingBusinessName: g.billingBusinessName || null,
                    billingEmail: g.billingEmail || null           
                })})'
                onmouseenter="this.style.background='var(--bg-secondary)'"
                onmouseleave="this.style.background=''">
                <td>
                    <span style="font-weight:600;">${g.fullName || g.firstName + ' ' + g.lastName}</span>
                    ${g.isVip ? ' <span class="badge bg-warning text-dark"><i class="fas fa-star"></i></span>' : ''}
                </td>
                <td>${g.phone || '—'}</td>
                <td>${g.email || '—'}</td>
                <td>
                    <span class="badge" style="background:var(--primary);color:#fff;font-size:.75rem;">
                        ${sourceLabel(g.source)}
                    </span>
                </td>
            </tr>
        `).join('');

        await Swal.fire({
            title: '<i class="fas fa-search"></i> Seleccionar Huésped',
            html: `
                <div style="text-align:left;">
                    <div class="input-group mb-3">
                        <span class="input-group-text"><i class="fas fa-search"></i></span>
                        <input type="text" id="modalGuestSearch" class="form-control"
                               placeholder="Nombre, teléfono o email..."
                               oninput="filterGuestModalTable(this.value)">
                    </div>
                    <div style="max-height:340px; overflow-y:auto;">
                        <table class="table table-sm table-hover" id="guestModalTable">
                            <thead style="position:sticky;top:0;z-index:1;background:var(--secondary);">
                                <tr>
                                    <th>Nombre</th>
                                    <th>Teléfono</th>
                                    <th>Email</th>
                                    <th>Canal</th>
                                </tr>
                            </thead>
                            <tbody id="guestModalBody">
                                ${tableRows || '<tr><td colspan="4" class="text-center text-muted py-3">Sin resultados</td></tr>'}
                            </tbody>
                        </table>
                    </div>
                    <div class="mt-3 pt-2 border-top">
                        <button class="btn btn-outline-primary btn-sm" onclick="openCreateGuestModalFromSwal()">
                            <i class="fas fa-user-plus"></i> Crear nuevo huésped
                        </button>
                    </div>
                </div>
            `,
            width: '700px',
            showConfirmButton: false,
            showCloseButton: true,
        });
    } catch (err) {
        console.error('Error al abrir modal de huéspedes:', err);
        showErrorToast('Error al cargar huéspedes');
    }
}

function filterGuestModalTable(query) {
    const q = query.toLowerCase();
    const rows = document.querySelectorAll('#guestModalBody tr');
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(q) ? '' : 'none';
    });
}

function chooseGuestFromModal(guest) {
    setSelectedGuest({
        guestId: guest.guestId,
        firstName: guest.firstName,
        lastName: "",
        phone: guest.phone,
        email: guest.email,
        isVip: guest.isVip || false,
        billingRfc: guest.billingRfc || null,
        billingBusinessName: guest.billingBusinessName || null,
        billingEmail: guest.billingEmail || null
    });
    Swal.close();
}

function setSelectedGuest(guest) {
    ResCreate.guest = guest;
    document.getElementById('selectedGuestId').value = guest.guestId;

    const fullName = `${guest.firstName} ${guest.lastName}`.trim();
    const initial = (guest.firstName || '?')[0].toUpperCase();

    document.getElementById('guestInitials').textContent = initial;
    document.getElementById('guestSelectedName').textContent = fullName;
    document.getElementById('guestSelectedPhone').textContent = guest.phone || '';
    document.getElementById('guestSelectedEmail').textContent = guest.email || '';
    document.getElementById('guestVipBadge').style.display = guest.isVip ? 'inline-block' : 'none';

    document.getElementById('guestSelectedCard').style.display = 'flex';
    document.getElementById('guestSearchArea').style.display = 'none';
}

function clearGuest() {
    ResCreate.guest = null;
    document.getElementById('selectedGuestId').value = '';
    document.getElementById('guestSelectedCard').style.display = 'none';
    document.getElementById('guestSearchArea').style.display = 'block';
    document.getElementById('guestQuickSearch').value = '';
}

// Abre el modal de creación de huésped (mismo partial del GuestsController)
async function openCreateGuestModal() {
    try {
        const html = await fetchHTML('/Guests/GetCreateForm');
        if (!html) return;

        await Swal.fire({
            title: '<i class="fas fa-user-plus"></i> Nuevo Huésped',
            html: html,
            width: '820px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false,
            didOpen: () => {
                document.querySelectorAll('#createGuestTabs button')
                    .forEach(el => new bootstrap.Tab(el));

                // Interceptar el submit aquí, sin depender de guests.js
                $(document).off('submit.guestFromReservation').on('submit.guestFromReservation', '#createForm', async function (e) {
                    e.preventDefault();
                    e.stopImmediatePropagation();

                    const formData = getFormData('createForm');

                    if (!formData.FirstName || !formData.LastName || !formData.Phone) {
                        showErrorToast('Nombre, Apellido y Teléfono son requeridos');
                        return;
                    }
                    if (!formData.IsBlacklisted) formData.BlacklistReason = null;

                    showLoading('Guardando huésped...');
                    const result = await postJSON('/Guests/Create', formData);
                    hideLoading();

                    if (!result.success) {
                        showErrorToast(result.message || 'Error al crear el huésped');
                        return;
                    }

                    // Seleccionar automáticamente el huésped recién creado
                    setSelectedGuest({
                        guestId: result.data.guestId,
                        firstName: result.data.firstName || formData.FirstName,
                        lastName: result.data.lastName || formData.LastName,
                        phone: formData.Phone,
                        email: formData.Email || null,
                        isVip: false
                    });

                    $(document).off('submit.guestFromReservation');
                    Swal.close();
                    showSuccessToast('Huésped creado y seleccionado');
                });
            },
            willClose: () => {
                $(document).off('submit.guestFromReservation');
            }
        });

    } catch (err) {
        console.error('Error al abrir modal de creación de huésped:', err);
        showErrorToast('Error al abrir el formulario');
    }
}

async function openCreateGuestModalFromSwal() {
    Swal.close();
    await openCreateGuestModal();
}

// Escucha evento personalizado si el guest.js emite al crear huésped
document.addEventListener('guestCreated', function (e) {
    if (e.detail) {
        setSelectedGuest({
            guestId: e.detail.guestId,
            firstName: e.detail.firstName || '',
            lastName: e.detail.lastName || '',
            phone: e.detail.phone || '',
            email: e.detail.email || '',
            isVip: false
        });
    }
});

// ─── SELECTOR DE HABITACIONES ─────────────────────────────────────────────────

async function openRoomSelector() {
    const ci = document.getElementById('checkInDate').value;
    const co = document.getElementById('checkOutDate').value;

    if (!ci || !co) {
        showWarningToast('Primero selecciona las fechas de check-in y check-out');
        return;
    }

    ResCreate.sel.selectedTypeId = null;
    ResCreate.sel.selectedRoom = null;

    const tpl = document.getElementById('roomSelectorTpl').innerHTML;

    await Swal.fire({
        title: '<i class="fas fa-bed"></i> Agregar Habitación',
        html: tpl,
        width: '750px',
        showConfirmButton: true,
        showCancelButton: true,
        confirmButtonText: 'Agregar Habitación',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: 'var(--primary)',
        showCloseButton: false,
        allowOutsideClick: false,
        didOpen: () => {
            Swal.getConfirmButton().disabled = true;
            document.getElementById('selectorDates').textContent =
                `${formatDate(ci)} → ${formatDate(co)}`;
            loadRoomTypes(ci, co);
        },
        preConfirm: () => {
            if (!ResCreate.sel.selectedRoom) {
                Swal.showValidationMessage('Selecciona una habitación específica');
                return false;
            }
            return true;
        }
    }).then(result => {
        if (result.isConfirmed && ResCreate.sel.selectedRoom) {
            addRoomToList(ResCreate.sel.selectedRoom);
        }
    });
}

async function loadRoomTypes(checkIn, checkOut) {
    try {
        const url = `/Availability/GetAvailableRooms?checkIn=${checkIn}&checkOut=${checkOut}`;
        const res = await fetch(url);
        const data = await res.json();

        const grid = document.getElementById('roomTypeGrid');
        if (!data.success || !data.data?.length) {
            grid.innerHTML = `
                <div style="grid-column:1/-1;text-align:center;padding:2rem;color:var(--text-secondary);">
                    <i class="fas fa-bed fa-2x mb-2 d-block opacity-25"></i>
                    No hay habitaciones disponibles para esas fechas
                </div>`;
            ResCreate.sel.roomTypes = [];
            return;
        }

        // Agrupar por tipo de habitación
        const typeMap = new Map();
        data.data.forEach(room => {
            if (!typeMap.has(room.roomTypeId)) {
                typeMap.set(room.roomTypeId, {
                    roomTypeId: room.roomTypeId,
                    roomTypeName: room.roomTypeName,
                    roomTypeCode: room.roomTypeCode,
                    pricePerNight: room.basePriceNightly,
                    baseCapacity: room.baseCapacity,
                    maxCapacity: room.maxCapacity,
                    extraPersonCharge: room.extraPersonCharge, 
                    available: 0,
                    rooms: []
                });
            }

            const t = typeMap.get(room.roomTypeId);

            // Ajustar capacidades de forma segura
            t.maxCapacity = Math.max(t.maxCapacity ?? 0, room.maxCapacity);
            t.baseCapacity = t.baseCapacity ?? room.baseCapacity;

            t.available++;
            t.rooms.push(room);
        });

        ResCreate.sel.roomTypes = Array.from(typeMap.values());

        grid.innerHTML = ResCreate.sel.roomTypes.map(t => `
            <div class="room-type-card-sel" data-type-id="${t.roomTypeId}" onclick="selectRoomType('${t.roomTypeId}')">
                <div class="rt-name">
                    ${t.roomTypeName} 
                    <small style="font-weight:400;opacity:.7;">${t.roomTypeCode}</small>
                </div>
                <div class="rt-price">$${formatMoney(t.pricePerNight)}/noche</div>
                <div class="rt-capacity">
                    ${t.baseCapacity} - ${t.maxCapacity} personas
                </div>
                <div class="rt-avail">
                    <span style="color:${t.available > 0 ? 'var(--success)' : 'var(--danger)'}">
                        ${t.available} disponible${t.available !== 1 ? 's' : ''}
                    </span>
                </div>
            </div>
        `).join('');

    } catch (err) {
        console.error('Error al cargar tipos de habitación:', err);
        document.getElementById('roomTypeGrid').innerHTML =
            `<div style="grid-column:1/-1;text-align:center;color:var(--danger);">Error al cargar habitaciones</div>`;
    }
}

function selectRoomType(typeId) {
    ResCreate.sel.selectedTypeId = typeId;
    ResCreate.sel.selectedRoom = null;

    document.querySelectorAll('.room-type-card-sel').forEach(c => c.classList.remove('selected'));
    document.querySelector(`.room-type-card-sel[data-type-id="${typeId}"]`)?.classList.add('selected');

    const type = ResCreate.sel.roomTypes.find(t => t.roomTypeId === typeId);
    if (!type) return;

    // Filtrar habitaciones ya agregadas
    const usedIds = new Set(ResCreate.rooms.map(r => r.roomId));

    const roomsHtml = type.rooms.map(r => {
        const alreadyAdded = usedIds.has(r.roomId);
        const occupied = alreadyAdded;
        return `
            <div class="room-card-sel ${occupied ? 'occupied' : ''} ${alreadyAdded ? 'already-added' : ''}"
                 data-room-id="${r.roomId}"
                 title="${alreadyAdded ? 'Ya agregada a esta reserva' : (occupied ? 'No disponible' : 'Disponible')}"
                 onclick="${occupied ? '' : `chooseRoom('${typeId}', '${r.roomId}')`}">
                <div class="rc-num">${r.roomNumber}</div>
                <div class="rc-floor">Piso ${r.floor ?? '?'}</div>
                <div>
                    <span class="rc-status ${occupied ? 'occupied' : 'available'}">
                        ${alreadyAdded ? 'Agregada' : (occupied ? 'Ocupada' : 'Disponible')}
                    </span>
                </div>
            </div>
        `;
    }).join('');

    document.getElementById('selectorStep1').style.display = 'none';
    document.getElementById('selectorStep2').style.display = 'block';
    document.getElementById('roomsGrid').innerHTML = roomsHtml || '<p class="text-muted">Sin habitaciones</p>';

    Swal.getConfirmButton().disabled = true;
}

function backToStep1() {
    ResCreate.sel.selectedTypeId = null;
    ResCreate.sel.selectedRoom = null;
    document.getElementById('selectorStep1').style.display = 'block';
    document.getElementById('selectorStep2').style.display = 'none';
    document.querySelectorAll('.room-type-card-sel').forEach(c => c.classList.remove('selected'));
    Swal.getConfirmButton().disabled = true;
}

window.chooseRoom = function (typeId, roomId) {
    const type = ResCreate.sel.roomTypes.find(t => t.roomTypeId === typeId);
    const room = type?.rooms.find(r => r.roomId === roomId);
    if (!room) return;

    // Marcar visualmente la habitación seleccionada
    document.querySelectorAll('.room-card-sel').forEach(c => c.classList.remove('selected'));
    document.querySelector(`.room-card-sel[data-room-id="${roomId}"]`)?.classList.add('selected');

    // Guardar referencia temporal
    ResCreate.sel.selectedRoom = {
        roomId: room.roomId,
        roomTypeId: typeId,
        roomNumber: room.roomNumber,
        floor: room.floor,
        roomTypeName: type.roomTypeName,
        roomTypeCode: type.roomTypeCode,
        pricePerNight: type.pricePerNight,
        maxCapacity: type.maxCapacity,
        extraPersonCharge: type.extraPersonCharge,
        baseCapacity: type.baseCapacity,
        numAdults: 1, numChildren: 0, numBabies: 0,
        hasPets: false, numPets: 0, petChargeApplied: 0,
        vehiclePlate: null, extraPersonCharge: type.extraPersonCharge, subtotal: 0
    };

    // Mostrar Step 3
    showStep3(type.roomTypeName, room.roomNumber, type.maxCapacity);
};

window.showStep3 = function (typeName, roomNumber, maxCapacity) {
    document.getElementById('selectorStep2').style.display = 'none';
    document.getElementById('selectorStep3').style.display = 'block';

    document.getElementById('step3RoomLabel').textContent =
        `Hab. ${roomNumber} — ${typeName} (máx. ${maxCapacity} personas)`;

    // Resetear controles
    document.getElementById('step3Adults').value = 1;
    document.getElementById('step3Children').value = 0;
    document.getElementById('step3Babies').value = 0;

    updateStep3Total();
    Swal.getConfirmButton().disabled = false;
};

window.backToStep2 = function () {
    document.getElementById('selectorStep3').style.display = 'none';
    document.getElementById('selectorStep2').style.display = 'block';
    ResCreate.sel.selectedRoom = null;
    Swal.getConfirmButton().disabled = true;
};

window.changeStep3 = function (field, delta) {
    const el = document.getElementById(field);
    const max = ResCreate.sel.selectedRoom?.maxCapacity ?? 10;
    let val = parseInt(el.value || 0) + delta;

    const adults = parseInt(document.getElementById('step3Adults').value || 0);
    const children = parseInt(document.getElementById('step3Children').value || 0);
    const thisValue = parseInt(el.value || 0);

    // Bebés no cuentan para el límite de capacidad
    const occupying = field === 'step3Babies'
        ? 0  // si estamos cambiando bebés, no validar contra el máximo
        : (adults + children) - thisValue + val;

    if (val < 0) val = 0;
    if (field === 'step3Adults' && val < 1) val = 1;

    if (field !== 'step3Babies' && occupying > max) {
        Swal.showValidationMessage(`Máximo ${max} personas para esta habitación`);
        return;
    }

    Swal.resetValidationMessage();
    el.value = val;
    updateStep3Total();
};

window.updateStep3Total = function () {
    const adults = parseInt(document.getElementById('step3Adults').value || 0);
    const children = parseInt(document.getElementById('step3Children').value || 0);
    const babies = parseInt(document.getElementById('step3Babies').value || 0);
    const occupying = adults + children;
    const max = ResCreate.sel.selectedRoom?.maxCapacity ?? 10;
    const base = ResCreate.sel.selectedRoom?.baseCapacity ?? max;
    const extra = ResCreate.sel.selectedRoom?.extraPersonCharge ?? 0;
    const basePrice = ResCreate.sel.selectedRoom?.pricePerNight ?? 0;

    const extrasCount = Math.max(0, occupying - base);
    const extraCharge = extrasCount * extra;
    const pricePerNight = basePrice + extraCharge;

    // Actualizar contador
    document.getElementById('step3TotalPersons').textContent =
        `${occupying} / ${max} (+ ${babies} bebé${babies !== 1 ? 's' : ''})`;
    document.getElementById('step3TotalPersons').style.color =
        occupying > max ? 'var(--danger)' : occupying === max ? 'var(--warning)' : 'var(--success)';

    // Mostrar desglose de precio
    const priceEl = document.getElementById('step3PriceBreakdown');
    if (priceEl) {
        if (extrasCount > 0) {
            priceEl.innerHTML =
                `$${formatMoney(basePrice)}/noche + <span class="text-warning">${extrasCount} extra × $${formatMoney(extra)} = <strong>$${formatMoney(pricePerNight)}/noche</strong></span>`;
        } else {
            priceEl.innerHTML = `<strong>$${formatMoney(pricePerNight)}/noche</strong>`;
        }
    }
};

function addRoomToList(room) {
    if (ResCreate.rooms.find(r => r.roomId === room.roomId)) {
        showWarningToast('Esa habitación ya está en la reserva');
        return;
    }

    room.numAdults = parseInt(document.getElementById('step3Adults')?.value || 1);
    room.numChildren = parseInt(document.getElementById('step3Children')?.value || 0);
    room.numBabies = parseInt(document.getElementById('step3Babies')?.value || 0);

    // Calcular cargo extra
    const occupying = room.numAdults + room.numChildren;
    const extrasCount = Math.max(0, occupying - (room.baseCapacity ?? occupying));
    room.extraPersonCharge = extrasCount * (room.extraPersonCharge ?? 0);
    room.pricePerNight = room.pricePerNight + room.extraPersonCharge;

    const nights = getNights();
    room.subtotal = room.pricePerNight * nights;

    ResCreate.rooms.push(room);
    renderRoomsTable();
    updateGuestTotals();
    recalculate();
    showSuccessToast(`Habitación ${room.roomNumber} agregada`);
}

function updateGuestTotals() {

    // CALCULAR DESDE ROOMS (source of truth)
    const totals = (ResCreate.rooms || []).reduce((acc, r) => {
        acc.adults += r.numAdults || 0;
        acc.children += r.numChildren || 0;
        acc.babies += r.numBabies || 0;
        return acc;
    }, { adults: 0, children: 0, babies: 0 });

    // Asegurar mínimo 1 adulto
    const totalAdults = totals.adults > 0 ? totals.adults : 1;

    // ACTUALIZAR INPUTS (para submit)
    const elAdults = document.getElementById('totalAdults');
    const elChildren = document.getElementById('totalChildren');
    const elBabies = document.getElementById('totalBabies');

    if (elAdults) elAdults.value = totalAdults;
    if (elChildren) elChildren.value = totals.children;
    if (elBabies) elBabies.value = totals.babies;

    // ACTUALIZAR UI (cards)
    const displayAdults = document.getElementById('displayAdults');
    const displayChildren = document.getElementById('displayChildren');
    const displayBabies = document.getElementById('displayBabies');

    if (displayAdults) displayAdults.textContent = totalAdults;
    if (displayChildren) displayChildren.textContent = totals.children;
    if (displayBabies) displayBabies.textContent = totals.babies;

    // MOSTRAR / OCULTAR CARDS DINÁMICAMENTE
    const cardChildren = document.getElementById('cardChildren');
    const cardBabies = document.getElementById('cardBabies');

    if (cardChildren) cardChildren.style.display = totals.children > 0 ? 'flex' : 'none';
    if (cardBabies) cardBabies.style.display = totals.babies > 0 ? 'flex' : 'none';
}

function removeRoom(index) {
    const room = ResCreate.rooms[index];
    if (!room) return;

    Swal.fire({
        title: '¿Quitar habitación?',
        text: `¿Eliminar la habitación ${room.roomNumber} de la reserva?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, quitar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: 'var(--danger)',
    }).then(r => {
        if (r.isConfirmed) {
            ResCreate.rooms.splice(index, 1);
            renderRoomsTable();
            updateGuestTotals();
            recalculate();
        }
    });
}

function renderRoomsTable() {
    const tbody = document.getElementById('roomsAddedBody');
    const badge = document.getElementById('roomsBadge');
    const nights = getNights();

    if (ResCreate.rooms.length === 0) {
        tbody.innerHTML = `
            <tr id="emptyRoomsRow">
                <td colspan="7" class="text-center py-4 text-muted">
                    <i class="fas fa-bed fa-2x mb-2 d-block opacity-25"></i>
                    No hay habitaciones — haz clic en <strong>Agregar Habitación</strong>
                </td>
            </tr>`;
        badge.textContent = 'Sin habitaciones';
        badge.className = 'badge bg-secondary ms-auto';
        document.getElementById('priceBox').style.display = 'none';
        return;
    }

    // Actualizar subtotales con noches actuales
    ResCreate.rooms.forEach(r => { r.subtotal = r.pricePerNight * nights; });

    tbody.innerHTML = ResCreate.rooms.map((r, i) => `
        <tr>
            <td><span class="room-type-badge-sm">${r.roomTypeCode}</span> ${r.roomTypeName}</td>
            <td class="fw-bold text-primary">${r.roomNumber}</td>
            <td>${r.floor ? `Piso ${r.floor}` : '—'}</td>
            <td>${r.numAdults} adulto${r.numAdults !== 1 ? 's' : ''}</td>
            <td>$${formatMoney(r.pricePerNight)}</td>
            <td class="fw-semibold">$${formatMoney(r.subtotal)}</td>
            <td>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeRoom(${i})" title="Quitar">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');

    badge.textContent = `${ResCreate.rooms.length} hab.`;
    badge.className = 'badge ms-auto';
    badge.style.background = 'var(--primary)';
    badge.style.color = '#fff';
    document.getElementById('priceBox').style.display = 'block';

    renderCompanions();
}

// ─── ACOMPAÑANTES ────────────────────────────────────────────────────────────

function renderCompanions() {
    const container = document.getElementById('companionsContainer');
    if (!container) return; // sección no existe si hotel no lo requiere

    if (ResCreate.rooms.length === 0) {
        container.innerHTML = `
            <div class="text-muted text-center py-3">
                <i class="fas fa-bed me-2"></i>
                Agrega habitaciones para ver los campos de acompañantes
            </div>`;
        return;
    }

    const optional = window.HotelConfig?.companionsOptional ?? true;
    const requiredMark = optional ? '' : '<span class="text-danger">*</span>';

    let html = '';

    ResCreate.rooms.forEach((room, roomIdx) => {
        // Slots: adultos extra (sin contar al huésped principal) + niños
        // Los bebés nunca cuentan
        const adultSlots = Math.max(0, (room.numAdults || 1) - 1);
        const childSlots = room.numChildren || 0;
        const totalSlots = adultSlots + childSlots;

        // Inicializar array de companions si no existe
        if (!room.companions) room.companions = [];
        // Ajustar tamaño del array al número de slots
        while (room.companions.length < totalSlots) room.companions.push({});
        room.companions.length = totalSlots;

        html += `
        <div class="mb-4">
            <div class="d-flex align-items-center gap-2 mb-2">
                <i class="fas fa-door-open text-primary"></i>
                <strong>Hab. ${room.roomNumber} — ${room.roomTypeName}</strong>
                <span class="badge bg-secondary">${room.numAdults} adulto${room.numAdults !== 1 ? 's' : ''} · ${room.numChildren} niño${room.numChildren !== 1 ? 's' : ''}</span>
            </div>`;

        if (totalSlots === 0) {
            html += `
            <div class="text-muted small ps-3">
                <i class="fas fa-info-circle me-1"></i>
                Solo 1 adulto (el huésped principal) — sin acompañantes en esta habitación.
            </div>`;
        } else {
            html += `
            <div class="text-muted small ps-3 mb-2">
                <i class="fas fa-info-circle me-1"></i>
                El huésped principal ya ocupa 1 lugar de adulto.
                ${optional ? 'Los datos son opcionales.' : 'Los nombres son requeridos para confirmar.'}
            </div>
            <div class="table-responsive">
                <table class="table table-sm table-bordered align-middle mb-0">
                    <thead class="table-light">
                        <tr>
                            <th style="width:40px">#</th>
                            <th>Tipo</th>
                            <th>Nombre completo ${requiredMark}</th>
                            <th style="width:80px">Edad</th>
                            <th style="width:130px">Parentesco</th>
                        </tr>
                    </thead>
                    <tbody>`;

            let slotIdx = 0;

            // Filas de adultos extra
            for (let a = 0; a < adultSlots; a++, slotIdx++) {
                const c = room.companions[slotIdx] || {};
                html += companionRow(roomIdx, slotIdx, 'Adulto', c, optional);
            }

            // Filas de niños
            for (let ch = 0; ch < childSlots; ch++, slotIdx++) {
                const c = room.companions[slotIdx] || {};
                html += companionRow(roomIdx, slotIdx, 'Niño', c, optional);
            }

            html += `
                    </tbody>
                </table>
            </div>`;
        }

        html += `</div>`;

        if (roomIdx < ResCreate.rooms.length - 1) {
            html += `<hr class="my-3">`;
        }
    });

    container.innerHTML = html;
}

function companionRow(roomIdx, slotIdx, tipo, companion, optional) {
    const ph = optional ? 'Opcional' : 'Requerido';
    return `
        <tr>
            <td class="text-center text-muted small">${slotIdx + 1}</td>
            <td><span class="badge ${tipo === 'Adulto' ? 'bg-primary' : 'bg-warning text-dark'}">${tipo}</span></td>
            <td>
                <input type="text" class="form-control form-control-sm"
                    placeholder="${ph}"
                    value="${companion.fullName || ''}"
                    oninput="updateCompanion(${roomIdx}, ${slotIdx}, 'fullName', this.value)" />
            </td>
            <td>
                <input type="number" class="form-control form-control-sm" min="0" max="120"
                    placeholder="Edad"
                    value="${companion.age || ''}"
                    oninput="updateCompanion(${roomIdx}, ${slotIdx}, 'age', this.value)" />
            </td>
            <td>
                <input type="text" class="form-control form-control-sm"
                    placeholder="Ej: esposa"
                    value="${companion.relationship || ''}"
                    oninput="updateCompanion(${roomIdx}, ${slotIdx}, 'relationship', this.value)" />
            </td>
        </tr>`;
}

function updateCompanion(roomIdx, slotIdx, field, value) {
    if (!ResCreate.rooms[roomIdx]) return;
    if (!ResCreate.rooms[roomIdx].companions) ResCreate.rooms[roomIdx].companions = [];
    if (!ResCreate.rooms[roomIdx].companions[slotIdx]) ResCreate.rooms[roomIdx].companions[slotIdx] = {};
    ResCreate.rooms[roomIdx].companions[slotIdx][field] = value;
}

function validateCompanions() {
    const optional = window.HotelConfig?.companionsOptional ?? true;
    const required = window.HotelConfig?.requireCompanionDetails ?? false;

    if (!required || optional) return true; // sin validación necesaria
    if (!validateCompanions()) return;

    for (let roomIdx = 0; roomIdx < ResCreate.rooms.length; roomIdx++) {
        const room = ResCreate.rooms[roomIdx];
        const adultSlots = Math.max(0, (room.numAdults || 1) - 1);
        const childSlots = room.numChildren || 0;
        const totalSlots = adultSlots + childSlots;

        for (let i = 0; i < totalSlots; i++) {
            const name = room.companions?.[i]?.fullName?.trim();
            if (!name) {
                showErrorToast(
                    `Falta el nombre del acompañante ${i + 1} en Hab. ${room.roomNumber}`
                );
                document.getElementById('companionsSection')?.scrollIntoView({ behavior: 'smooth' });
                return false;
            }
        }
    }
    return true;
}


// ─── CÁLCULO DE PRECIOS ───────────────────────────────────────────────────────

function recalculate() {
    const nights = getNights();
    const config = window.HotelConfig;
    const finance = ResCreate.finance;

    // Base = suma de habitaciones
    finance.basePrice = ResCreate.rooms.reduce((s, r) => s + r.pricePerNight * nights, 0);

    // Descuento
    const discType = document.getElementById('discountType')?.value;
    if (discType === 'custom') {
        finance.discountPercent = parseFloat(document.getElementById('discountCustom')?.value || 0);
    } else {
        finance.discountPercent = parseFloat(discType || 0);
    }
    finance.discountAmount = finance.basePrice * (finance.discountPercent / 100);

    const afterDiscount = finance.basePrice - finance.discountAmount;

    // Impuestos
    if (config.chargesTaxes) {
        finance.taxIva = afterDiscount * (config.taxIvaPercent / 100);
        finance.taxIsh = afterDiscount * (config.taxIshPercent / 100);
    } else {
        finance.taxIva = 0;
        finance.taxIsh = 0;
    }

    finance.total = afterDiscount + finance.taxIva + finance.taxIsh;

    // Anticipo — nuevo sistema
    const state = ResCreate.depositState;
    if (state === 'received' || state === 'pending') {
        const pctId = state === 'received' ? 'depositPct' : 'depositPct';
        const amountId = state === 'received' ? 'depositCustomAmountR' : 'depositCustomAmount';
        const pctMode = document.getElementById(pctId)?.checked;
        if (pctMode) {
            finance.depositAmount = finance.total * (config.minDepositPct / 100);
        } else {
            finance.depositAmount = parseFloat(document.getElementById(amountId)?.value || 0);
        }
        finance.depositAmount = Math.min(finance.depositAmount, finance.total);
    } else {
        // 'none' o 'platform'
        finance.depositAmount = 0;
    }
    finance.balance = finance.total - finance.depositAmount;

    updatePriceDisplay();

    // Refrescar faltante en splits si está activo el modo recibido
    if (ResCreate.depositState === 'received' && ResCreate.paymentSplits.length > 0) {
        updateSplitsTotals(); // Solo totales, no re-render completo
    }
}

function updatePriceDisplay() {
    const f = ResCreate.finance;
    setText('priceBase', `$${formatMoney(f.basePrice)}`);
    setText('priceDiscount', `-$${formatMoney(f.discountAmount)}`);
    if (window.HotelConfig.chargesTaxes) {
        setText('priceTaxIva', `$${formatMoney(f.taxIva)}`);
        setText('priceTaxIsh', `$${formatMoney(f.taxIsh)}`);
    }
    setText('priceTotal', `$${formatMoney(f.total)}`);
    setText('depositPctAmount', `$${formatMoney(f.depositAmount)}`);  // received
    setText('depositPctAmountP', `$${formatMoney(f.depositAmount)}`);  // pending
    setText('depositBalance', `$${formatMoney(f.balance)}`);        // received
    setText('depositBalanceP', `$${formatMoney(f.balance)}`);        // pending
}

function onDiscountChange() {
    const isCustom = document.getElementById('discountType').value === 'custom';
    document.getElementById('discountCustom').style.display = isCustom ? 'block' : 'none';
    recalculate();
}

// ─── ANTICIPO ─────────────────────────────────────────────────────────────────

function toggleDepositSection(show) {
    document.getElementById('depositSection').style.display = show ? 'block' : 'none';
    recalculate();
}

function onDepositTypeChange() {
    const state = ResCreate.depositState;
    const fixedId = state === 'received' ? 'depositFixed' : 'depositFixed';
    const inputId = state === 'received' ? 'depositCustomAmount' : 'depositCustomAmount';
    const isFixed = document.getElementById(fixedId)?.checked;
    document.getElementById(inputId).style.display = isFixed ? 'block' : 'none';
    recalculate();
}

function toggleProofUpload() {
    const method = document.getElementById('depositMethod')?.value;
    const refArea = document.getElementById('depositReferenceArea');
    if (refArea) refArea.style.display = (method === 'transfer' || method === 'card') ? 'block' : 'none';
}

// ─── ENVÍO DEL FORMULARIO ─────────────────────────────────────────────────────

async function submitReservation(status) {
    try {
        // ── Validaciones ──────────────────────────────────────────────────
        if (!ResCreate.guest) {
            showErrorToast('Selecciona un huésped para la reserva');
            document.getElementById('guestQuickSearch').focus();
            return;
        }

        const ci = document.getElementById('checkInDate').value;
        const co = document.getElementById('checkOutDate').value;

        if (!ci || !co) {
            showErrorToast('Selecciona las fechas de check-in y check-out');
            return;
        }

        if (ResCreate.rooms.length === 0 && status !== 'draft') {
            showErrorToast('Agrega al menos una habitación a la reserva');
            return;
        }

        const source = document.querySelector('input[name="source"]:checked')?.value;
        if (!source) {
            showErrorToast('Selecciona el canal de reserva');
            return;
        }

        // ── Construcción del DTO ───────────────────────────────────────────
        recalculate();
        const f = ResCreate.finance;

        // LÓGICA DE ANTICIPO
        const depositState = ResCreate.depositState;
        const isPlatform = depositState === 'platform';
        const requiresDeposit = (depositState === 'received' || depositState === 'pending');

        const depositMethod = depositState === 'received'
            ? (document.getElementById('depositMethod')?.value || null)
            : null;

        const internalNoteExtra = isPlatform
            ? '\n[PAGO EN PLATAFORMA - Verificar en Booking/Airbnb]'
            : '';

        const dto = {
            guestId: ResCreate.guest.guestId,
            source: source,
            status: status,
            checkInDate: ci,
            checkOutDate: co,
            numNights: getNights(),
            totalAdults: parseInt(document.getElementById('totalAdults').value) || 1,
            totalChildren: parseInt(document.getElementById('totalChildren').value) || 0,
            totalBabies: parseInt(document.getElementById('totalBabies').value) || 0,
            travelReason: document.getElementById('travelReason').value || null,
            requiresInvoice: ResCreate.requiresInvoice,

            subtotal: f.basePrice,
            discountAmount: f.discountAmount,
            discountPercent: f.discountPercent,
            discountReason: null,
            taxesAmount: f.taxIva + f.taxIsh,
            total: f.total,

            requiresDeposit: requiresDeposit,
            depositAmount: requiresDeposit ? f.depositAmount : 0,
            depositPaymentMethod: requiresDeposit && depositMethod ? depositMethod : null,
            depositReference: depositState === 'received'
                ? (document.getElementById('depositReference')?.value?.trim() || null)
                : null,
            depositProofUrl: null,
            depositDueDate: document.getElementById('depositDueDate')?.value || null,

            guestNotes: document.getElementById('guestNotes').value.trim() || null,
            internalNotes: (document.getElementById('internalNotes').value.trim() || '') + internalNoteExtra,

            rooms: ResCreate.rooms.map(r => ({
                roomId: r.roomId,
                roomTypeId: r.roomTypeId,
                roomNumber: r.roomNumber,
                roomTypeName: r.roomTypeName,
                numAdults: r.numAdults,
                numChildren: r.numChildren,
                numBabies: r.numBabies,
                hasPets: r.hasPets,
                numPets: r.numPets,
                petChargeApplied: r.petChargeApplied || 0,
                vehiclePlate: r.vehiclePlate || null,
                vehicleDescription: null,
                pricePerNight: r.pricePerNight,
                extraPersonCharge: r.extraPersonCharge || 0,
                subtotal: r.subtotal,
                companions: (r.companions || []).filter(c => c.fullName?.trim())
            })),

            // En edición nunca re-enviamos splits — los pagos existentes se preservan en BD
            // Solo en creación enviamos splits nuevos
            paymentSplits: window.IsEditMode
                ? []
                : (ResCreate.depositState === 'received'
                    ? ResCreate.paymentSplits
                        .filter(p => parseFloat(p.amount) > 0)
                        .map(p => ({
                            method: p.method,
                            amount: parseFloat(p.amount),
                            reference: p.reference || null
                        }))
                    : [])
        };

        // Validar splits si hay anticipo recibido
        if (ResCreate.depositState === 'received') {
            const splits = ResCreate.paymentSplits.filter(p => parseFloat(p.amount) > 0);

            if (splits.length === 0) {
                showErrorToast('Agrega al menos un método de pago con monto');
                return;
            }

            const cashCount = splits.filter(p => p.method === 'cash').length;
            if (cashCount > 1) {
                showErrorToast('Solo puedes registrar efectivo una vez. Combina los montos en una sola fila.');
                return;
            }

            const missingRef = splits.find(p => p.method !== 'cash' && !p.reference?.trim());
            if (missingRef) {
                showErrorToast('Agrega la referencia para los pagos por transferencia o tarjeta');
                return;
            }
        }

        // ── Confirmar antes de guardar ─────────────────────────────────────
        const confirmMsg = status === 'draft'
            ? 'Se guardará como borrador. Podrás completarla después.'
            : `Se creará la reserva con folio asignado automáticamente.<br>
               Total: <strong>$${formatMoney(f.total)}</strong>
               ${requiresDeposit
                ? `· Anticipo: <strong>$${formatMoney(f.depositAmount)}</strong>`
                : '· Sin anticipo registrado'}`;

        const confirmResult = await Swal.fire({
            title: status === 'draft' ? '💾 Guardar borrador' : '✅ Confirmar reserva',
            html: confirmMsg,
            icon: status === 'draft' ? 'info' : 'question',
            showCancelButton: true,
            confirmButtonText: status === 'draft' ? 'Guardar borrador' : 'Sí, confirmar',
            cancelButtonText: 'Revisar',
            confirmButtonColor: status === 'draft' ? 'var(--primary)' : 'var(--success)',
        });

        if (!confirmResult.isConfirmed) return;

        // ── Enviar al servidor ─────────────────────────────────────────────
        const isEdit = window.IsEditMode === true;

        if (isEdit) dto.reservationId = window.EditReservation?.reservationId;

        showLoading(isEdit ? 'Guardando cambios...' : (status === 'draft' ? 'Guardando borrador...' : 'Creando reserva...'));

        const endpoint = isEdit ? '/Reservations/Update' : '/Reservations/Create';
        const result = await postJSON(endpoint, dto);

        hideLoading();

        if (!result.success) {
            showErrorToast(result.message || 'Error al guardar la reserva');
            console.error('Error del servidor:', result);
            return;
        }

        await Swal.fire({
            icon: 'success',
            title: isEdit ? '✅ Reserva actualizada' : (status === 'draft' ? '💾 Borrador guardado' : '🎉 Reserva confirmada'),
            html: isEdit
                ? `<p>La reserva <strong style="color:var(--primary)">${window.EditReservation?.folio}</strong> fue actualizada correctamente.</p>`
                : `<div style="font-size:1.1rem;margin:1rem 0;">
               Folio: <span style="font-weight:700;color:var(--primary);font-size:1.3rem;">${result.data?.folio ?? ''}</span>
           </div>
           <p style="color:var(--text-secondary);font-size:.9rem;">
               ${status === 'draft' ? 'Borrador guardado. Podrás completarla después.' : 'Reserva registrada exitosamente.'}
           </p>`,
            confirmButtonText: 'Ver reservaciones',
            confirmButtonColor: '#2BA49A',
            showCancelButton: !isEdit,
            cancelButtonText: 'Crear otra',
            cancelButtonColor: '#718096'
        }).then(r => {
            if (isEdit || r.isConfirmed) {
                window.location.href = '/Availability';
            }
        });

    } catch (err) {
        hideLoading();
        console.error('Error al enviar reserva:', err);
        showErrorToast('Error al procesar la reserva');
    }
}

// ─── RESET DEL FORMULARIO ─────────────────────────────────────────────────────

function resetForm() {
    clearGuest();
    ResCreate.rooms = [];
    ResCreate.finance = { basePrice: 0, discountAmount: 0, discountPercent: 0, taxIva: 0, taxIsh: 0, total: 0, depositAmount: 0, balance: 0 };

    document.getElementById('checkInDate').value = '';
    document.getElementById('checkOutDate').value = '';
    document.getElementById('nightsDisplay').textContent = '—';
    document.getElementById('totalAdults').value = 1;
    document.getElementById('totalChildren').value = 0;
    document.getElementById('totalBabies').value = 0;
    document.getElementById('hasPets').checked = false;
    togglePets(false);
    document.getElementById('travelReason').value = '';
    document.getElementById('guestNotes').value = '';
    document.getElementById('internalNotes').value = '';
    document.getElementById('depositDueDate').value = '';
    document.getElementById('depositMethod').value = '';
    document.getElementById('depositReference').value = '';
    document.getElementById('depositReferenceArea').style.display = 'none';
    document.getElementById('depositYes').checked = true;
    toggleDepositSection(true);

    renderRoomsTable();
    recalculate();

    ResCreate.depositState = 'received';
    if (typeof onDepositStateChange === 'function') {
        onDepositStateChange('received');
    }

    if (typeof onSourceChange === 'function') {
        onSourceChange('whatsapp');
    }

    window.scrollTo({ top: 0, behavior: 'smooth' });
    showInfoToast('Formulario limpio — listo para nueva reserva');
}

// ─── UTILIDADES ───────────────────────────────────────────────────────────────

function formatMoney(n) {
    return parseFloat(n || 0).toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatDate(d) {
    if (!d) return '—';
    const [y, m, day] = d.split('-');
    const months = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
    return `${day} ${months[parseInt(m) - 1]} ${y}`;
}

function setText(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
}

function sourceLabel(s) {
    const map = {
        direct: 'Directo', phone: 'Teléfono', whatsapp: 'WhatsApp',
        walk_in: 'Walk-in', ota_booking: 'Booking', ota_airbnb: 'Airbnb'
    };
    return map[s] || s || '—';
}












// ─── ANTICIPO — lógica completa ───────────────────────────────────────────────

// Estado actual del anticipo: 'none' | 'pending' | 'received' | 'platform'
ResCreate.depositState = 'received';

/**
 * Llamado cuando cambia el canal (source).
 * Detecta si es OTA y cambia la UI del anticipo automáticamente.
 */
function onSourceChange(sourceValue) {
    const isPlatform = (sourceValue === 'booking' || sourceValue === 'airbnb'
        || sourceValue === 'ota_booking' || sourceValue === 'ota_airbnb');
    document.getElementById('depositPlatformMsg').style.display = isPlatform ? 'block' : 'none';
    document.getElementById('depositDirectSection').style.display = isPlatform ? 'none' : 'block';

    if (isPlatform) {
        ResCreate.depositState = 'platform';
    } else {
        const checked = document.querySelector('input[name="depositState"]:checked');
        ResCreate.depositState = checked?.value || 'received';
    }
    recalculate();
}

/**
 * Llamado al hacer clic en una de las 3 tarjetas de estado.
 */
function onDepositStateChange(state) {
    ResCreate.depositState = state;

    document.querySelectorAll('.deposit-state-card').forEach(c => c.classList.remove('active'));
    const cardMap = { none: 'cardDepositNone', pending: 'cardDepositPending', received: 'cardDepositReceived' };
    document.getElementById(cardMap[state])?.classList.add('active');

    const radio = document.querySelector(`input[name="depositState"][value="${state}"]`);
    if (radio) radio.checked = true;

    document.getElementById('depositPendingDetails').style.display = state === 'pending' ? 'block' : 'none';
    document.getElementById('depositReceivedDetails').style.display = state === 'received' ? 'block' : 'none';
    if (state === 'received') {
        if (ResCreate.paymentSplits.length === 0) {
            initPaymentSplits();
        } else {
            renderPaymentSplits();
        }
    }

    recalculate();
}

function onDepositTypeChange() {
    const state = ResCreate.depositState;
    const fixedId = state === 'received' ? 'depositFixedR' : 'depositFixed';
    const inputId = state === 'received' ? 'depositCustomAmountR' : 'depositCustomAmount';
    const isFixed = document.getElementById(fixedId)?.checked;
    if (document.getElementById(inputId)) {
        document.getElementById(inputId).style.display = isFixed ? 'block' : 'none';
    }
    recalculate();
}

function toggleProofUpload() {
    const method = document.getElementById('depositMethod')?.value;
    const refArea = document.getElementById('depositReferenceArea');
    if (refArea) refArea.style.display = (method === 'transfer' || method === 'card') ? 'block' : 'none';
}


// ─── FACTURA ──────────────────────────────────────────────────────────────────

ResCreate.requiresInvoice = false;

function toggleInvoice() {
    ResCreate.requiresInvoice = !ResCreate.requiresInvoice;
    const card = document.getElementById('invoiceKpi');
    const icon = document.getElementById('invoiceKpiCheck');
    const sub = document.getElementById('invoiceKpiSub');
    const info = document.getElementById('invoiceBillingInfo');
    const text = document.getElementById('invoiceBillingText');

    if (ResCreate.requiresInvoice) {
        card.classList.add('active');
        icon.className = 'fas fa-toggle-on fa-2x text-primary';
        sub.textContent = 'Factura solicitada';

        // Mostrar datos fiscales del huésped si los tiene
        const g = ResCreate.guest;
        if (g?.billingRfc) {
            text.innerHTML = `<strong>RFC:</strong> ${g.billingRfc} &nbsp;|&nbsp;
                              <strong>Razón social:</strong> ${g.billingBusinessName || '—'} &nbsp;|&nbsp;
                              <strong>Email fiscal:</strong> ${g.billingEmail || '—'}`;
        } else {
            text.innerHTML = `El huésped no tiene datos fiscales registrados. 
                              Se podrán capturar durante el check-in.`;
        }
        if (info) info.style.display = 'block';
    } else {
        card.classList.remove('active');
        icon.className = 'fas fa-toggle-off fa-2x text-muted';
        sub.textContent = 'Clic para activar';
        if (info) info.style.display = 'none';
    }
}

// =============================================================================
// MODO EDICIÓN — Precarga de datos
// =============================================================================

async function preloadEditData(r) {
    try {
        // ── Huésped ──────────────────────────────────────────────────────
        setSelectedGuest({
            guestId: r.guestId,
            firstName: r.guestFirstName,
            lastName: r.guestLastName,
            phone: r.guestPhone ?? '',
            email: r.guestEmail ?? '',
            isVip: r.guestIsVip ?? false
        });

        // ── Fechas ────────────────────────────────────────────────────────
        const ci = r.checkInDate?.split('T')[0];
        const co = r.checkOutDate?.split('T')[0];
        document.getElementById('checkInDate').value = ci ?? '';
        document.getElementById('checkOutDate').value = co ?? '';
        onDatesChange();

        // ── Personas ──────────────────────────────────────────────────────
        const totalAdults = document.getElementById('totalAdults');
        const totalChildren = document.getElementById('totalChildren');
        const totalBabies = document.getElementById('totalBabies');
        if (totalAdults) totalAdults.value = r.totalAdults ?? 1;
        if (totalChildren) totalChildren.value = r.totalChildren ?? 0;
        if (totalBabies) totalBabies.value = r.totalBabies ?? 0;

        // ── Motivo de viaje ───────────────────────────────────────────────
        const travelReason = document.getElementById('travelReason');
        if (travelReason) travelReason.value = r.travelReason ?? '';

        // ── Notas ─────────────────────────────────────────────────────────
        const guestNotes = document.getElementById('guestNotes');
        const internalNotes = document.getElementById('internalNotes');
        if (guestNotes) guestNotes.value = r.guestNotes ?? '';
        if (internalNotes) internalNotes.value = r.internalNotes ?? '';

        // ── Factura ───────────────────────────────────────────────────────
        ResCreate.requiresInvoice = r.requiresInvoice ?? false;

        // ── Anticipo ──────────────────────────────────────────────────────────
        if (window.IsEditMode) {
            // En edición — reemplazar toda la sección con info solo lectura
            const depositSection = document.getElementById('depositSection');
            if (depositSection) {
                const totalPaid = r.payments?.reduce((s, p) => s + (p.amount || 0), 0) ?? 0;
                const hasPayments = r.payments?.length > 0;
                const hasPending = r.depositAmount > 0 && !r.hasDeposit;

                let infoHTML = '';

                if (hasPayments) {
                    infoHTML = `
                <div class="alert alert-success py-2 mb-2 d-flex align-items-center gap-2">
                    <i class="fas fa-check-circle"></i>
                    <div>
                        <strong>Anticipo registrado</strong>
                        <small class="d-block text-muted">Para abonar más usa el módulo de Pagos (Componente 5).</small>
                    </div>
                </div>
                ${r.payments.map(p => `
                    <div class="d-flex justify-content-between align-items-center py-1 px-2 mb-1 rounded"
                         style="background:var(--bg-surface-alt);font-size:.85rem;">
                        <span>
                            <span class="badge bg-secondary me-1">
                                ${p.method === 'cash' ? '💵 Efectivo' : p.method === 'transfer' ? '🏦 Transferencia' : '💳 Tarjeta'}
                            </span>
                            ${p.reference ? `<small class="text-muted">Ref: ${p.reference}</small>` : ''}
                            <small class="text-muted ms-1">· ${new Date(p.paymentDate).toLocaleDateString('es-MX')}</small>
                        </span>
                        <strong>$${(p.amount || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 })}</strong>
                    </div>
                `).join('')}
                <div class="d-flex justify-content-between mt-2 pt-1" style="border-top:1px dashed var(--border-color);">
                    <small class="text-muted">Total pagado</small>
                    <small class="fw-bold text-success">$${totalPaid.toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN</small>
                </div>
                <div class="d-flex justify-content-between">
                    <small class="text-muted">Saldo pendiente</small>
                    <small class="fw-bold ${r.balancePending > 0 ? 'text-danger' : 'text-success'}">
                        $${(r.balancePending || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN
                    </small>
                </div>`;

                } else if (hasPending) {
                    infoHTML = `
                <div class="alert alert-warning py-2 d-flex align-items-center gap-2">
                    <i class="fas fa-clock"></i>
                    <div>
                        <strong>Anticipo solicitado: $${(r.depositAmount || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN</strong>
                        ${r.depositDueDate ? `<small class="d-block text-muted">Vence: ${new Date(r.depositDueDate).toLocaleDateString('es-MX')}</small>` : ''}
                        <small class="d-block text-muted">El huésped aún no ha pagado.</small>
                    </div>
                </div>`;
                } else {
                    infoHTML = `
                <div class="alert alert-secondary py-2 d-flex align-items-center gap-2">
                    <i class="fas fa-minus-circle"></i>
                    <div><strong>Sin anticipo registrado</strong></div>
                </div>`;
                }

                depositSection.innerHTML = `
            <div class="d-flex align-items-center gap-2 mb-3">
                <span class="fw-semibold">Estado del pago</span>
                <span class="badge bg-secondary ms-1" style="font-size:.7rem;">
                    <i class="fas fa-lock me-1"></i>Solo lectura
                </span>
            </div>
            ${infoHTML}`;
            }

        } else {
            // En creación — lógica normal de anticipo
            if (r.hasDeposit && r.payments?.length) {
                ResCreate.depositState = 'received';
                onDepositStateChange('received');
                ResCreate.paymentSplits = r.payments.map(p => ({
                    method: p.method ?? 'cash',
                    amount: p.amount ?? 0,
                    reference: p.reference ?? ''
                }));
                renderPaymentSplits();
            } else if (r.depositAmount > 0 && !r.hasDeposit) {
                ResCreate.depositState = 'pending';
                onDepositStateChange('pending');
            } else {
                ResCreate.depositState = 'none';
                onDepositStateChange('none');
            }
        }

        // ── En modo edición: ocultar formulario de anticipo y mostrar solo info ──
        if (window.IsEditMode) {
            const depositSection = document.getElementById('depositSection');
            if (depositSection) {
                // Calcular info de pago para mostrar
                const totalPaid = r.payments?.reduce((s, p) => s + (p.amount || 0), 0) ?? 0;
                const hasPayments = r.payments?.length > 0;
                const hasPending = r.depositAmount > 0 && !r.hasDeposit;
                const noDeposit = !hasPayments && !hasPending;

                let infoHTML = '';

                if (hasPayments) {
                    infoHTML = `
                <div class="alert alert-success py-2 mb-2 d-flex align-items-center gap-2">
                    <i class="fas fa-check-circle"></i>
                    <div>
                        <strong>Anticipo registrado</strong>
                        <small class="d-block text-muted">Para abonar más, usa el módulo de Pagos (Componente 5).</small>
                    </div>
                </div>
                ${r.payments.map(p => `
                    <div class="d-flex justify-content-between align-items-center py-1 px-2 mb-1 rounded"
                         style="background:var(--bg-surface-alt);font-size:.85rem;">
                        <span>
                            <span class="badge bg-secondary me-1">
                                ${p.method === 'cash' ? '💵 Efectivo' : p.method === 'transfer' ? '🏦 Transferencia' : '💳 Tarjeta'}
                            </span>
                            ${p.reference ? `<small class="text-muted">Ref: ${p.reference}</small>` : ''}
                            <small class="text-muted ms-1">· ${new Date(p.paymentDate).toLocaleDateString('es-MX')}</small>
                        </span>
                        <strong>$${(p.amount || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 })}</strong>
                    </div>
                `).join('')}
                <div class="d-flex justify-content-between mt-2 pt-1" style="border-top:1px dashed var(--border-color);">
                    <small class="text-muted">Total pagado</small>
                    <small class="fw-bold text-success">$${totalPaid.toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN</small>
                </div>
                <div class="d-flex justify-content-between">
                    <small class="text-muted">Saldo pendiente</small>
                    <small class="fw-bold ${r.balancePending > 0 ? 'text-danger' : 'text-success'}">
                        $${(r.balancePending || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN
                    </small>
                </div>`;

                } else if (hasPending) {
                    infoHTML = `
                <div class="alert alert-warning py-2 d-flex align-items-center gap-2">
                    <i class="fas fa-clock"></i>
                    <div>
                        <strong>Anticipo solicitado: $${(r.depositAmount || 0).toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN</strong>
                        ${r.depositDueDate ? `<small class="d-block text-muted">Vence: ${new Date(r.depositDueDate).toLocaleDateString('es-MX')}</small>` : ''}
                        <small class="d-block text-muted">El huésped aún no ha pagado.</small>
                    </div>
                </div>`;

                } else {
                    infoHTML = `
                <div class="alert alert-secondary py-2 d-flex align-items-center gap-2">
                    <i class="fas fa-minus-circle"></i>
                    <div><strong>Sin anticipo registrado</strong></div>
                </div>`;
                }

                // Reemplazar contenido completo de la sección
                depositSection.innerHTML = `
            <div class="d-flex align-items-center gap-2 mb-3">
                <div class="section-icon text-success"><i class="fa-solid fa-dollar-sign"></i></div>
                <span class="fw-semibold">Estado del pago</span>
                <span class="badge bg-secondary ms-1" style="font-size:.7rem;">
                    <i class="fas fa-lock me-1"></i>Solo lectura
                </span>
            </div>
            ${infoHTML}`;
            }
        }

        // ── Habitaciones ──────────────────────────────────────────────────
        if (r.rooms?.length) {
            for (const room of r.rooms) {
                const companions = room.companions ?? [];

                ResCreate.rooms.push({
                    roomId: room.roomId,
                    roomTypeId: room.roomTypeId,
                    roomNumber: room.roomNumber,
                    floor: room.floor,
                    roomTypeName: room.roomTypeName,
                    roomTypeCode: room.roomTypeCode,
                    pricePerNight: room.pricePerNight,
                    extraPersonCharge: room.extraPersonCharge ?? 0,
                    baseCapacity: room.numAdults,
                    maxCapacity: room.numAdults + room.numChildren + room.numBabies,
                    numAdults: room.numAdults,
                    numChildren: room.numChildren ?? 0,
                    numBabies: room.numBabies ?? 0,
                    hasPets: room.hasPets ?? false,
                    numPets: room.numPets ?? 0,
                    petChargeApplied: room.petChargeApplied ?? 0,
                    vehiclePlate: room.vehiclePlate ?? null,
                    subtotal: room.subtotal,
                    companions: companions.map(c => ({
                        fullName: c.fullName ?? '',
                        age: c.age ?? '',
                        phone: c.phone ?? '',
                        relationship: c.relationship ?? ''
                    }))
                });
            }

            renderRoomsTable();
            recalculate();
            updateGuestTotals();
        }

        // ── Canal — al final, después de todos los awaits ─────────────────
        setTimeout(() => {
            const sourceVal = (r.source || '').toLowerCase();

            // Reset todos los botones — mismo código que initSourceButtons
            document.querySelectorAll('.source-btn').forEach(b => {
                b.style.background = '';
                b.style.color = '';
                b.style.borderColor = '';
                b.classList.remove('active');
            });

            const sourceBtn = document.querySelector(`.source-btn[data-value="${sourceVal}"]`);

            if (sourceBtn) {
                // Aplicar exactamente igual que el click en initSourceButtons
                sourceBtn.style.background = 'var(--primary)';
                sourceBtn.style.color = '#fff';
                sourceBtn.style.borderColor = 'var(--primary)';
                sourceBtn.classList.add('active');

                const radio = sourceBtn.querySelector('input[type=radio]');
                if (radio) radio.checked = true;
            } else {
                console.warn('❌ No se encontró botón para:', sourceVal);
            }
        }, 150);

        // ── Toast ─────────────────────────────────────────────────────────
        showInfoToast(`Editando reserva ${r.folio}`);

    } catch (err) {
        console.error('Error al precargar datos de edición:', err);
        showErrorToast('Error al cargar los datos de la reserva');
    }
}

// =============================================================================
// PAYMENT SPLITS — Pagos mixtos
// =============================================================================

function initPaymentSplits() {
    ResCreate.paymentSplits = [];
    addPaymentSplit();
}

function addPaymentSplit() {
    const idx = ResCreate.paymentSplits.length;
    ResCreate.paymentSplits.push({ method: 'cash', amount: 0, reference: '' });
    renderPaymentSplits();
}

function removePaymentSplit(idx) {
    if (ResCreate.paymentSplits.length <= 1) {
        showWarningToast('Debe haber al menos un método de pago');
        return;
    }
    ResCreate.paymentSplits.splice(idx, 1);
    renderPaymentSplits();
}

function updateSplitField(idx, field, value) {
    if (!ResCreate.paymentSplits[idx]) return;
    ResCreate.paymentSplits[idx][field] = value;

    if (field === 'amount') {
        // Solo actualizar totales — NO re-renderizar todo el DOM
        updateSplitsTotals();
        recalculate();
    } else if (field === 'method') {
        // Método sí requiere re-render (aparece/desaparece el campo referencia)
        renderPaymentSplits();
    }
    // reference: solo actualiza el estado, sin re-render
}

function updateSplitsTotals() {
    const splits = ResCreate.paymentSplits.filter(p => parseFloat(p.amount) > 0);
    const totalPaid = splits.reduce((s, p) => s + (parseFloat(p.amount) || 0), 0);
    const totalReserva = ResCreate.finance?.total || 0;
    const diferencia = totalPaid - totalReserva;

    const elTotal = document.getElementById('splitsTotal');
    const elFaltante = document.getElementById('splitsFaltante');
    const elFaltanteRow = document.getElementById('splitsFaltanteRow');
    const elCompletoRow = document.getElementById('splitsCompletoRow');
    const elAlert = document.getElementById('splitsOverAlert');

    if (elTotal) elTotal.textContent = `$${formatMoney(totalPaid)} MXN`;

    if (diferencia < 0) {
        if (elFaltante) elFaltante.textContent = `-$${formatMoney(Math.abs(diferencia))} MXN`;
        if (elFaltanteRow) { elFaltanteRow.style.display = 'flex'; }
        if (elCompletoRow) { elCompletoRow.style.display = 'none'; }
        if (elAlert) elAlert.innerHTML = '';

    } else if (diferencia === 0) {
        if (elFaltanteRow) { elFaltanteRow.style.display = 'none'; }
        if (elCompletoRow) { elCompletoRow.style.display = 'flex'; }
        if (elAlert) elAlert.innerHTML = '';

    } else {
        if (elFaltanteRow) { elFaltanteRow.style.display = 'none'; }
        if (elCompletoRow) { elCompletoRow.style.display = 'none'; }

        const hasCash = splits.some(p => p.method === 'cash');
        const hasNonCash = splits.some(p => p.method !== 'cash');
        const isMixed = hasCash && hasNonCash;
        const isCashOnly = hasCash && !hasNonCash;

        let alertHtml = '';
        if (isCashOnly) {
            alertHtml = `<div class="alert alert-success py-2 mt-2 mb-0">
                <i class="fas fa-coins me-1"></i>
                <strong>Cambio a entregar: $${formatMoney(diferencia)} MXN</strong>
            </div>`;
        } else if (!isMixed) {
            alertHtml = `<div class="alert alert-warning py-2 mt-2 mb-0">
                <i class="fas fa-exclamation-triangle me-1"></i>
                <strong>El monto supera lo acordado por $${formatMoney(diferencia)} MXN</strong>
            </div>`;
        } else {
            alertHtml = `<div class="alert alert-danger py-2 mt-2 mb-0">
                <i class="fas fa-ban me-1"></i>
                <strong>Pago mixto: no se puede entregar cambio en efectivo</strong>
            </div>`;
        }
        if (elAlert) elAlert.innerHTML = alertHtml;
    }
}

function renderPaymentSplits() {
    const container = document.getElementById('paymentSplitsContainer');
    if (!container) return;

    // Métodos ya usados
    const usedCash = ResCreate.paymentSplits.some(p => p.method === 'cash');

    container.innerHTML = ResCreate.paymentSplits.map((split, idx) => `
        <div class="d-flex gap-2 align-items-center mb-2">

            <select class="form-select form-select-sm" style="width:150px;flex-shrink:0;"
                    onchange="updateSplitField(${idx}, 'method', this.value); renderPaymentSplits()">
                <option value="cash"     ${split.method === 'cash' ? 'selected' : ''}>💵 Efectivo</option>
                <option value="transfer" ${split.method === 'transfer' ? 'selected' : ''}>🏦 Transferencia</option>
                <option value="card"     ${split.method === 'card' ? 'selected' : ''}>💳 Tarjeta</option>
            </select>

            <div class="input-group input-group-sm" style="width:150px;flex-shrink:0;">
                <span class="input-group-text">$</span>
                <input type="number" class="form-control"
                       placeholder="0.00" min="0" step="0.01"
                       value="${split.amount || ''}"
                       oninput="updateSplitField(${idx}, 'amount', parseFloat(this.value) || 0)">
            </div>

            <input type="text" class="form-control form-control-sm"
                   style="display:${split.method !== 'cash' ? 'block' : 'none'};height:1rem; flex:1;"
                   placeholder="Referencia / No. operación"
                   value="${split.reference || ''}"
                   oninput="updateSplitField(${idx}, 'reference', this.value)">

            <div style="display:${split.method === 'cash' ? 'block' : 'none'};flex:1;"></div>

            <button type="button" class="btn btn-sm btn-outline-danger flex-shrink-0"
                    onclick="removePaymentSplit(${idx})" title="Quitar">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `).join('');

    // Calcular totales
    // Totales — usar IDs estables para actualizar sin re-render
    const totalPaid = ResCreate.paymentSplits.reduce((s, p) => s + (parseFloat(p.amount) || 0), 0);
    const totalReserva = ResCreate.finance?.total || 0;
    const faltante = Math.max(0, totalReserva - totalPaid);

    container.insertAdjacentHTML('beforeend', `
    <div style="display:flex; justify-content:flex-end; flex-direction:column; align-items:flex-end; margin-top:8px; gap:4px;">
        <div style="display:flex; gap:12px;">
            <small class="text-muted">Total registrado:</small>
            <small class="fw-bold text-success" id="splitsTotal">$${formatMoney(totalPaid)} MXN</small>
        </div>
        <div id="splitsFaltanteRow" style="display:${faltante > 0 ? 'flex' : 'none'}; gap:12px;">
            <small class="text-muted">Faltante para cubrir total:</small>
            <small class="fw-bold text-danger" id="splitsFaltante">-$${formatMoney(faltante)} MXN</small>
        </div>
        <div id="splitsCompletoRow" style="display:${faltante <= 0 ? 'flex' : 'none'}; gap:12px;">
            <small class="fw-bold text-success">
                <i class="fas fa-check-circle me-1"></i>Pago completo
            </small>
        </div>
    </div>
    <div id="splitsOverAlert"></div>
`);

    updateSplitsTotals();
}