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
};

// ─── Inicialización ─────────────────────────────────────────────────────────
$(document).ready(function () {
    initSourceButtons();
    initDates();
});

// ─── Botones de canal (radio) ────────────────────────────────────────────────
function initSourceButtons() {
    document.querySelectorAll('.source-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.source-btn').forEach(b => {
                b.style.background = '';
                b.style.color = '';
                b.style.borderColor = '';
            });
            btn.style.background = 'var(--primary)';
            btn.style.color = '#fff';
            btn.style.borderColor = 'var(--primary)';
            btn.querySelector('input[type=radio]').checked = true;
        });

        if (btn.querySelector('input[type=radio]').checked) {
            btn.style.background = 'var(--primary)';
            btn.style.color = '#fff';
            btn.style.borderColor = 'var(--primary)';
        }
    });
}

// ─── Fechas ──────────────────────────────────────────────────────────────────
function initDates() {
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('checkInDate').min = today;
    document.getElementById('checkOutDate').min = today;
}

function onDatesChange() {
    const ci = document.getElementById('checkInDate').value;
    const co = document.getElementById('checkOutDate').value;

    if (!ci || !co) return;

    if (co <= ci) {
        showWarningToast('La fecha de check-out debe ser posterior al check-in');
        document.getElementById('checkOutDate').value = '';
        document.getElementById('nightsDisplay').textContent = '—';
        return;
    }

    const nights = Math.round((new Date(co) - new Date(ci)) / 86400000);
    document.getElementById('nightsDisplay').textContent = `${nights} noche${nights !== 1 ? 's' : ''}`;

    // Recalcular habitaciones ya agregadas (subtotales por noche cambian)
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
        guestId: guest.guestId || guest.guest_id,
        firstName: guest.firstName || guest.first_name || '',
        lastName: guest.lastName || guest.last_name || '',
        phone: guest.phone || '',
        email: guest.email || '',
        isVip: guest.isVip || guest.is_vip || false
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
            guestId: g.guestId, firstName: g.firstName, lastName: g.lastName,
            phone: g.phone, email: g.email, isVip: g.isVip
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
    setSelectedGuest(guest);
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
                // Inicializar tabs de Bootstrap si los hay
                document.querySelectorAll('#createGuestTabs button').forEach(el => new bootstrap.Tab(el));
            }
        });

        // Nota: el handleCreate del guests.js manejará el submit.
        // Si se crea exitosamente, el evento 'guestCreated' nos notificará.
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
                    pricePerNight: room.pricePerNight,
                    available: 0,
                    rooms: []
                });
            }
            const t = typeMap.get(room.roomTypeId);
            if (room.isAvailable) t.available++;
            t.rooms.push(room);
        });

        ResCreate.sel.roomTypes = Array.from(typeMap.values());

        grid.innerHTML = ResCreate.sel.roomTypes.map(t => `
            <div class="room-type-card-sel" data-type-id="${t.roomTypeId}" onclick="selectRoomType('${t.roomTypeId}')">
                <div class="rt-name">${t.roomTypeName} <small style="font-weight:400;opacity:.7;">${t.roomTypeCode}</small></div>
                <div class="rt-price">$${formatMoney(t.pricePerNight)}/noche</div>
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
        const occupied = !r.isAvailable || alreadyAdded;
        return `
            <div class="room-card-sel ${occupied ? 'occupied' : ''} ${alreadyAdded ? 'already-added' : ''}"
                 data-room-id="${r.roomId}"
                 title="${alreadyAdded ? 'Ya agregada a esta reserva' : (occupied ? 'No disponible' : 'Disponible')}"
                 onclick="${occupied ? '' : `chooseRoom('${typeId}', '${r.roomId}')`}">
                <div class="rc-num">${r.roomNumber}</div>
                <div class="rc-floor">Piso ${r.floor || '?'}</div>
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

function chooseRoom(typeId, roomId) {
    const type = ResCreate.sel.roomTypes.find(t => t.roomTypeId === typeId);
    const room = type?.rooms.find(r => r.roomId === roomId);
    if (!room || !room.isAvailable) return;

    ResCreate.sel.selectedRoom = {
        roomId: room.roomId,
        roomTypeId: typeId,
        roomNumber: room.roomNumber,
        floor: room.floor,
        roomTypeName: type.roomTypeName,
        roomTypeCode: type.roomTypeCode,
        pricePerNight: type.pricePerNight,
        numAdults: parseInt(document.getElementById('totalAdults').value) || 1,
        numChildren: parseInt(document.getElementById('totalChildren').value) || 0,
        numBabies: parseInt(document.getElementById('totalBabies').value) || 0,
        hasPets: document.getElementById('hasPets').checked,
        numPets: parseInt(document.getElementById('petsCount').value) || 0,
        petChargeApplied: 0,
        vehiclePlate: null,
        extraPersonCharge: 0,
        subtotal: 0  // se recalcula al agregar
    };

    document.querySelectorAll('.room-card-sel').forEach(c => c.classList.remove('selected'));
    document.querySelector(`.room-card-sel[data-room-id="${roomId}"]`)?.classList.add('selected');

    Swal.getConfirmButton().disabled = false;
}

function addRoomToList(room) {
    if (ResCreate.rooms.find(r => r.roomId === room.roomId)) {
        showWarningToast('Esa habitación ya está en la reserva');
        return;
    }

    const nights = getNights();
    room.subtotal = room.pricePerNight * nights;

    ResCreate.rooms.push(room);
    renderRoomsTable();
    recalculate();
    showSuccessToast(`Habitación ${room.roomNumber} agregada`);
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

    // Anticipo
    const requiresDeposit = document.getElementById('depositYes')?.checked;
    if (requiresDeposit) {
        const pctMode = document.getElementById('depositPct')?.checked;
        if (pctMode) {
            finance.depositAmount = finance.total * (config.minDepositPct / 100);
        } else {
            finance.depositAmount = parseFloat(document.getElementById('depositCustomAmount')?.value || 0);
        }
        finance.depositAmount = Math.min(finance.depositAmount, finance.total);
    } else {
        finance.depositAmount = 0;
    }
    finance.balance = finance.total - finance.depositAmount;

    updatePriceDisplay();
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
    setText('depositPctAmount', `$${formatMoney(f.depositAmount)}`);
    setText('depositBalance', `$${formatMoney(f.balance)}`);
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
    const isFixed = document.getElementById('depositFixed')?.checked;
    document.getElementById('depositCustomAmount').style.display = isFixed ? 'block' : 'none';
    recalculate();
}

function toggleProofUpload() {
    const method = document.getElementById('depositMethod').value;
    const refArea = document.getElementById('depositReferenceArea');
    refArea.style.display = (method === 'transfer' || method === 'card') ? 'block' : 'none';
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
        const requiresDeposit = document.getElementById('depositYes').checked;
        const depositMethod = document.getElementById('depositMethod').value || null;

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

            subtotal: f.basePrice,
            discountAmount: f.discountAmount,
            discountPercent: f.discountPercent,
            discountReason: null,
            taxesAmount: f.taxIva + f.taxIsh,
            total: f.total,

            requiresDeposit: requiresDeposit,
            depositAmount: requiresDeposit ? f.depositAmount : 0,
            depositPaymentMethod: requiresDeposit && depositMethod ? depositMethod : null,
            depositReference: document.getElementById('depositReference')?.value?.trim() || null,
            depositProofUrl: null,
            depositDueDate: document.getElementById('depositDueDate')?.value || null,

            guestNotes: document.getElementById('guestNotes').value.trim() || null,
            internalNotes: document.getElementById('internalNotes').value.trim() || null,

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
                subtotal: r.subtotal
            }))
        };

        // ── Confirmar antes de guardar ─────────────────────────────────────
        const confirmMsg = status === 'draft'
            ? 'Se guardará como borrador. Podrás completarla después.'
            : `Se creará la reserva con folio asignado automáticamente.<br>
               Total: <strong>$${formatMoney(f.total)}</strong>
               ${requiresDeposit ? `· Anticipo: <strong>$${formatMoney(f.depositAmount)}</strong>` : '· Sin anticipo registrado'}`;

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
        showLoading(status === 'draft' ? 'Guardando borrador...' : 'Creando reserva...');

        const result = await postJSON('/Reservations/Create', dto);

        hideLoading();

        if (!result.success) {
            showErrorToast(result.message || 'Error al guardar la reserva');
            console.error('Error del servidor:', result);
            return;
        }

        // ── Éxito ──────────────────────────────────────────────────────────
        const swalResult = await Swal.fire({
            icon: 'success',
            title: status === 'draft' ? '💾 Borrador guardado' : '🎉 Reserva confirmada',
            html: `
                <div style="font-size:1.1rem;margin:1rem 0;">
                    Folio: <span style="font-weight:700;color:var(--primary);font-size:1.3rem;">${result.data.folio}</span>
                </div>
                <p style="color:var(--text-secondary);font-size:.9rem;">
                    ${status === 'draft' ? 'Guardada como borrador' : 'La reserva ha sido creada correctamente'}
                </p>
            `,
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonText: 'Ver en calendario',
            cancelButtonText: 'Nueva reserva',
            denyButtonText: 'Ver detalles',
            confirmButtonColor: 'var(--primary)',
            denyButtonColor: 'var(--tertiary)',
        });

        if (swalResult.isConfirmed) {
            window.location.href = '/Availability';
        } else if (swalResult.isDenied) {
            window.location.href = `/Availability?reservationId=${result.data.reservationId}`;
        } else {
            // Nueva reserva — reset del formulario
            resetForm();
        }

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