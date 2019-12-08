using DemoEventBusFramewrok;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using QuotationServices.Events;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuotationServices.ViewModels;

namespace QuotationServices.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly IEventBus _eventBus;

        public ValuesController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        //payload:{"ordertype":1,"price":100,"initial":50}
        [HttpPost]
        public async Task<HttpResponseMessage>  Post(QuotationOrderVM model)
        {
            var id = Guid.NewGuid();
            
            //var model = JsonConvert.DeserializeObject<QuotationOrderVM>(value);
            if (model != null)
            {
                await _eventBus.PublishAsync(new QuotationCreateEvent(id,model.Price,model.InitialPrice,model.orderType));

                return new HttpResponseMessage(HttpStatusCode.OK);
            }


            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
