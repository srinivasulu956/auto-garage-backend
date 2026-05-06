namespace Auto_Garage.Models.DomainModels.ServiceBookingModel
{
    public enum BookingStatus
    {
        Pending = 0,              // Customer just booked
        Confirmed = 1,            // Admin confirmed the booking
        AssignedToMechanic = 2,   // Admin assigned a mechanic
        InProgress = 3,           // Mechanic started work
        WaitingForParts = 4,      // On hold - waiting for spare parts
        QualityCheck = 5,         // Final inspection before handover
        Completed = 6,            // Work done - ready for handover
        InvoiceGenerated = 7,     // Admin raised the invoice
        Paid = 8,                 // Customer paid
        Cancelled = 9             // Booking cancelled
    }
}
