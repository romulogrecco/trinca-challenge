using System;

namespace Domain.Events
{
    public class InviteWasAccepted : IEvent
    {
        public string PersonId { get; set; }
        public string InviteId { get; set; }
        public bool IsVeg { get; set; }
        public DateTime Data { get; set; }
    }
}
