using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using WellFitPlus.WebAPI.Models;
using WellFitPlus.Common;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;

namespace WellFitPlus.WebAPI.Controllers
{
    public class AddressController : ApiController
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(AddressController));

        private AddressRepository _addressRepo = new AddressRepository();
        
        [HttpPost]
        public bool Add(AddressView actView) {
            try {
                Address address = new Address();

                address.City = actView.City;
                address.State = actView.State;
                address.Street = actView.Street;
                address.Zip = actView.Zip;
                
                _addressRepo.Add(address);

            } catch (Exception ex) {
                log.Error(ex);
                return false;
            }
            return true;
        }

        [HttpPost]
        public void Edit(Guid addressID) {
            try {
                Address address = _addressRepo.GetAddress(addressID);
                if (address != null) {

                    _addressRepo.Edit(address);
                }
            } catch (Exception ex) {
                log.Error(ex);
            }
        }

    }
}
