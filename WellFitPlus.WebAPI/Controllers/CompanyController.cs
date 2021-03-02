using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;

using Newtonsoft.Json;

using log4net;
using WellFitPlus.WebAPI.Models;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;

namespace WellFitPlus.WebAPI.Controllers {

    [Authorize]
    [RoutePrefix("api/company")]
    public class CompanyController : ApiController {
        public static readonly ILog log = LogManager.GetLogger(typeof(CompanyController));

        private CompanyRepository _companyRepo = new CompanyRepository();
        private AddressRepository _addressRepo = new AddressRepository();

        [HttpPost]
        [Route("Add")]
        public bool Add(CompanyView companyView) {
            try {
                Company company = new Company();
                Address addr = new Address();

                addr.City = companyView.City;
                addr.State = companyView.State;
                addr.Street = companyView.Street;
                addr.Zip = companyView.Zip;
                _addressRepo.Add(addr);

                company.AddressID = addr.Id;

                UpdateModel(ref company, companyView);

                _companyRepo.Add(company);

            } catch (Exception ex) {
                log.Error(ex);
                return false;
            }
            return true;
        }

        [HttpPost]
        [Route("Edit")]
        public void Edit(CompanyView companyView) {
            try {
                Company company = _companyRepo.Get(companyView.CompanyID);
                if (company != null) {

                    UpdateModel(ref company, companyView);
                    _companyRepo.Edit(company);
                }
            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        [HttpPost]
        public List<CompanyView> GetCompanyList() {
            List<CompanyView> companyViews = new List<CompanyView>();

            try {
                var companies = _companyRepo.GetAll().ToList();

                foreach (Company comp in companies) {
                    CompanyView companyView = new CompanyView();

                    //companyView.AddressID = comp.AddressID;
                    companyView.AnnualRenewal = comp.AnnualRenewal;
                    //companyView.BillingContact.Email = comp.BillingPerson.Email;
                    //companyView.BillingContact.FirstName = comp.BillingPerson.FirstName;
                    //companyView.BillingContact.LastName = comp.BillingPerson.LastName;
                    companyView.Name = comp.Name;
                    //companyView.SalesContact.Email = comp.SalesPerson.Email;
                    //companyView.SalesContact.FirstName = comp.SalesPerson.FirstName;
                    //companyView.SalesContact.LastName = comp.SalesPerson.LastName;

                    companyViews.Add(companyView);
                }

            } catch (Exception ex) {
                log.Error(ex);
            }
            return companyViews;
        }

        [HttpPost]
        [Route("RegisterExistingUser")]
        public HttpResponseMessage RegisterExistingUser(RegisterCompanyUserRequest request) {

            // Validate input
            if (string.IsNullOrEmpty(request.GroupCode) || request.UserId == Guid.Empty) {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.Content = new StringContent("Missing request data");

                throw new HttpResponseException(message);
            }

            var companyRepo = new CompanyRepository();
            var company = companyRepo.GetByGroupCode(request.GroupCode);

            // Validate database parameters.
            if (company == null) {
                
                // NOTE: The message is not referring to the user because the app will not be able to log in
                //       if there was not a user in the database for their credentials. That means that this
                //       endpoint would not be hit unless a user actually exists. We still check the user
                //       as a sanity check.

                var message = new HttpResponseMessage(HttpStatusCode.NotFound);
                message.Content = new StringContent("Company Code invalid");

                throw new HttpResponseException(message);
            }

            try
            {
                companyRepo.UpdateUser(request.UserId, company.Id);
            }
            catch (ApplicationException e) {
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.Content = new StringContent(e.ToString());

                throw new HttpResponseException(message);
            }

            var responseObj = new RegisterCompanyUserResponse();
            responseObj.CompanyName = company.Name;

            // Format the result manually to ignore reference loops
            JsonSerializerSettings jsSettings = new JsonSerializerSettings();
            jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            var json = JsonConvert.SerializeObject(responseObj, Formatting.None, jsSettings);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }

        [HttpPost]
        [Route("DeregisterUser")]
        public HttpResponseMessage DeregisterUser(RegisterCompanyUserRequest request)
        {

            // Validate input
            if (request.UserId == Guid.Empty)
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.Content = new StringContent("Missing request data");

                throw new HttpResponseException(message);
            }

            try
            {
                new CompanyRepository().UpdateUser(request.UserId, null);
            }
            catch (ApplicationException e) {
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.Content = new StringContent(e.ToString());

                throw new HttpResponseException(message);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent("Success", Encoding.UTF8, "application/json");
            return response;
        }

        private void UpdateModel(ref Company companyModel, CompanyView companyView) {
            companyModel.AnnualRenewal = companyView.AnnualRenewal;
            //companyModel.BillingPerson.Email = companyView.BillingContact.Email;
            //companyModel.BillingPerson.FirstName = companyView.BillingContact.FirstName;
            //companyModel.SalesPerson.Email = companyView.SalesContact.Email;
            //companyModel.SalesPerson.FirstName = companyView.SalesContact.FirstName;
            //companyModel.SalesPerson.LastName = companyView.SalesContact.LastName;
            //companyModel.BillingPerson.LastName = companyView.BillingContact.LastName;
            companyModel.Name = companyView.Name;

        }
    }
}
