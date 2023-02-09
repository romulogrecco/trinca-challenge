using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;

namespace Serverless_Api.Functions.Bbq.GetShopList
{
    public partial class RunGetShopList
    {
        private readonly IBbqRepository _repository;
        private readonly IPersonRepository _persons;
        private readonly Person _user;

        public RunGetShopList(IBbqRepository repository, Person user, IPersonRepository persons)
        {
            _repository = repository;
            _user = user;
            _persons = persons;
        }

        [Function(nameof(RunGetShopList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras/{id}/shoplist")] HttpRequestData req, string id)
        {
            var person = await _persons.GetAsync(_user.Id);

            if (person.IsCoOwner)
            {
                var bbq = await _repository.GetAsync(id);

                if (bbq is null)
                    return req.CreateResponse(HttpStatusCode.NoContent);

                return await req.CreateResponse(HttpStatusCode.OK, bbq.ShopList.TakeSnapshot());
            }

            return req.CreateResponse(HttpStatusCode.Forbidden); 
        }     
    }
}
