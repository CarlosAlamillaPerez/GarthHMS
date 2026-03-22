using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.DTOs.AvailabilityEngine
{
    /// <summary>
    /// Datos de ocupación por día para el calendario mensual
    /// SP: sp_availability_get_monthly_calendar
    /// </summary>
    public class CalendarDayDto
    {
        public DateTime DayDate { get; set; }
        public long TotalRooms { get; set; }
        public long OccupiedRooms { get; set; }
        public long ReservationCount { get; set; }
        public long AvailableRooms { get; set; }
        public decimal OccupancyPercent { get; set; }

        /// <summary>
        /// Nivel visual de ocupación para color-coding en calendario
        /// Valores: "none" | "low" | "medium" | "high" | "full"
        /// </summary>
        public string OccupancyLevel => OccupancyPercent switch
        {
            0 => "none",
            <= 25 => "low",
            <= 50 => "medium",
            <= 75 => "high",
            _ => "full"
        };
    }
}