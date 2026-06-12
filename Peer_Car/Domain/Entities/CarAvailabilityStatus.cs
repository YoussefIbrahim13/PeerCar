namespace Peer_Car.Domain.Entities
{
    public enum CarAvailabilityStatus
    {
        Available,      // العربية جاهزة للايجار وتظهر في البحث
        Rented,         // العربية حالياً مع مستخدم (في رحلة)
        Reserved,       // العربية تم دفع عربونها ومحجوزة لرحلة قادمة (مختفية من البحث مؤقتاً)
        Maintenance,    // العربية في الصيانة (صاحبها قفلها لفترة)
        InActive,       // صاحب العربية عطلها يدوياً (مش عايز يأجرها دلوقتي)
        OutofService    // خرجت من الخدمة نهائياً (حادثة أو تم بيعها مثلاً)
    }
}
