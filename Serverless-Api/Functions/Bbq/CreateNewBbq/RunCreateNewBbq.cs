using Eveneum;
using System.Net;
using CrossCutting;
using Domain.Events;
using Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Repositories;

namespace Serverless_Api
{
    public partial class RunCreateNewBbq
    {
        private readonly Person _user;
        private readonly SnapshotStore _snapshots;
        private readonly IPersonRepository _persons;
        private readonly IBbqRepository _repository;
        private readonly IEventStore<Person> _peopleStore;
        private readonly IEventStore<Bbq> _bbqsStore;

        public RunCreateNewBbq(SnapshotStore snapshots
                             , Person user
                             , IPersonRepository persons
                             , IBbqRepository repository
                             , IEventStore<Person> peopleStore
                             , IEventStore<Bbq> bbqsStore)
        {
            _user = user;
            _snapshots = snapshots;
            _persons = persons;
            _repository = repository;
            _peopleStore = peopleStore;
            _bbqsStore = bbqsStore;
        }

        [Function(nameof(RunCreateNewBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "churras")] HttpRequestData req)
        {
            var input = await req.Body<NewBbqRequest>();

            if (input == null)
            {
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");
            }

            var churras = new Bbq();
            churras.Apply(new ThereIsSomeoneElseInTheMood(Guid.NewGuid(), input.Date, input.Reason, input.IsTrincasPaying));

            await _repository.SaveAsync(churras);

            await _bbqsStore.WriteToStream(churras.Id, churras.Changes.Select(evento => new EventData(churras.Id, evento, new { CreatedBy = _user.Id }, churras.Version, DateTime.Now.ToString())).ToArray(), expectedVersion: churras.Version == 0 ? null : churras.Version);

            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            var p = await _persons.GetAsync(_user.Id);

            foreach (var personId in lookups.ModeratorIds)
            {
                var person = await _persons.GetAsync(personId);
                var @event = new PersonHasBeenInvitedToBbq(churras.Id, churras.Date, churras.Reason);
                person.Apply(@event);
                await _persons.SaveAsync(person);
            }

            return await req.CreateResponse(HttpStatusCode.Created, churras.TakeSnapshot());


            /*var churras = new Bbq();
            churras.Apply(new ThereIsSomeoneElseInTheMood(Guid.NewGuid(), input.Date, input.Reason, input.IsTrincasPaying));

            await _bbqsStore.WriteToStream(churras.Id, churras.Changes.Select(evento => new EventData(churras.Id, evento, new { CreatedBy = _user.Id }, churras.Version, DateTime.Now.ToString())).ToArray(), expectedVersion: churras.Version == 0 ? null : churras.Version);

            var churrasSnapshot = churras.TakeSnapshot();

            var Lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            foreach (var personId in Lookups.ModeratorIds)
            {
                var header = await _peopleStore.ReadHeader(personId);
                var @event = new PersonHasBeenInvitedToBbq(churras.Id, churras.Date, churras.Reason);
                await _peopleStore.WriteToStream(personId, new[] { new EventData(personId, @event, new { CreatedBy = _user.Id }, header.StreamHeader.Version, DateTime.Now.ToString()) }, expectedVersion: header.StreamHeader.Version == 0 ? null : header.StreamHeader.Version);
            }

            return await req.CreateResponse(HttpStatusCode.Created, churrasSnapshot);*/
        }
    }
}
