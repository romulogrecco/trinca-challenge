﻿namespace Domain.Events
{
    public class InviteWasDeclined : IEvent
    {
        public string InviteId { get; set; }
        public string PersonId { get; set; }
        public bool isVeg { get; set; }
    }
}
