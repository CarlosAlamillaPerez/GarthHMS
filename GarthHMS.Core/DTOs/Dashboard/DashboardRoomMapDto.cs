namespace GarthHMS.Core.DTOs.Dashboard
{
    /// <summary>
    /// DTO para cada habitación del mapa visual del dashboard
    /// Mapea directamente con sp_dashboard_get_rooms_map
    /// </summary>
    public class DashboardRoomMapDto
    {
        public Guid RoomId { get; set; }

        /// <summary>
        /// Número de habitación (101, 102, 201, etc.)
        /// </summary>
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Piso donde está la habitación
        /// </summary>
        public int Floor { get; set; }

        /// <summary>
        /// Estado actual: available, occupied, dirty, cleaning, maintenance, reserved
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Texto legible del estado (ej: "Disponible", "Ocupada")
        /// </summary>
        public string StatusText { get; set; } = string.Empty;

        public Guid RoomTypeId { get; set; }

        /// <summary>
        /// Nombre del tipo de habitación (ej: "Doble", "Suite")
        /// </summary>
        public string RoomTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Código del tipo (ej: "DBL", "SUI")
        /// </summary>
        public string RoomTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// ID de la estancia actual (null si no está ocupada)
        /// </summary>
        public Guid? CurrentStayId { get; set; }

        // ====================================================================
        // PROPIEDADES CALCULADAS (para el frontend)
        // ====================================================================

        /// <summary>
        /// Clase CSS basada en el estado
        /// </summary>
        public string StatusCssClass => Status switch
        {
            "available" => "available",
            "occupied" => "occupied",
            "dirty" => "dirty",
            "cleaning" => "cleaning",
            "maintenance" => "maintenance",
            "reserved" => "reserved",
            _ => "unknown"
        };

        /// <summary>
        /// Indica si la habitación está ocupada
        /// </summary>
        public bool IsOccupied => Status == "occupied";
    }
}