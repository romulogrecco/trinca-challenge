using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    public class Bbq : AggregateRoot
    {
        public string Reason { get; set; }
        public BbqStatus Status { get; set; }
        public DateTime Date { get; set; }
        public bool IsTrincasPaying { get; set; }
        public List<string> ConfirmedGuests { get; set; }
        public ShopList ShopList { get; set; }


        public void When(ThereIsSomeoneElseInTheMood @event)
        {
            Id = @event.Id.ToString();
            Date = @event.Date;
            Reason = @event.Reason;
            Status = BbqStatus.New;
            ConfirmedGuests = new List<string>();  
        }

        public void When(BbqStatusUpdated @event)
        {
            if (@event.GonnaHappen)
                Status = BbqStatus.PendingConfirmations;
            else 
                Status = BbqStatus.ItsNotGonnaHappen;

            if (@event.TrincaWillPay)
                IsTrincasPaying = true;
        }

        public void When(InviteWasDeclined @event)
        {
            if (ShopList is null) ShopList = new ShopList(@event.InviteId);

            if(ConfirmedGuests.Any(x => x == @event.PersonId))
            {
                ConfirmedGuests.Remove(@event.PersonId);
                ShopList.Decrement(@event.isVeg);
            }

            if (ConfirmedGuests.Count() < 2) Status = BbqStatus.PendingConfirmations;
        }

        public void When(InviteWasAccepted @event)
        {
            if (ShopList is null) ShopList = new ShopList(@event.InviteId);

            ShopList.Incremet(@event.IsVeg);

            ConfirmedGuests.Add(@event.PersonId);

            if (ConfirmedGuests.Count() == 2) Status = BbqStatus.Confirmed;     
        }

        public object TakeSnapshot()
        {
            return new
            {
                Id,
                Date,
                IsTrincasPaying,
                Status = Status.ToString()
            };
        }
    }
}
