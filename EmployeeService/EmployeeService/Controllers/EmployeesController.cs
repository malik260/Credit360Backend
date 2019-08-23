using EmployeeDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace EmployeeService.Controllers
{

    [Authorize]   //enable basic authentication on the entire controller methods
    public class EmployeesController : ApiController
    {

        EmployeeDBEntities entities = new EmployeeDBEntities();
        // list all employees
        ////public IEnumerable<Employee> Get()
        ////{
        ////    using (EmployeeDBEntities entities = new EmployeeDBEntities())
        ////    {
        ////        return entities.Employees.ToList();
        ////    }
        ////}
        //[BasicAuthentication]   enable basic authentication on the get method
        [HttpGet]
        public HttpResponseMessage Get(string gender = "ALL")
        {
            // string username = Thread.CurrentPrincipal.Identity.Name;
            switch (gender.ToLower())
            {
                case "all":
                    return Request.CreateResponse(HttpStatusCode.OK, entities.Employees.ToList());
                case "0":
                    return Request.CreateResponse(HttpStatusCode.OK, entities.Employees.Where(e => e.Gender.ToLower() == "0").ToList());
                case "1":
                    return Request.CreateResponse(HttpStatusCode.OK, entities.Employees.Where(e => e.Gender.ToLower() == "1").ToList());
                default:
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Value for gender my be All, Male or Female " + gender + " is invalid");
            }


        }

        [HttpGet]
        // get single employee
        public HttpResponseMessage Get(int id)
        {
            var entity = entities.Employees.FirstOrDefault(e => e.ID == id);
            if (entity != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Employee with id = " + id.ToString() + " not found");
            }

        }

        [HttpPost]
        // POST api/employees
        public HttpResponseMessage Post([FromBody] Employee employee)
        {
            try
            {

                entities.Employees.Add(employee);
                entities.SaveChanges();

                var message = Request.CreateResponse(HttpStatusCode.Created, employee);
                message.Headers.Location = new Uri(Request.RequestUri + employee.ID.ToString());
                return message;

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }

        }

        [HttpDelete]
        // delete single employee
        public HttpResponseMessage Delete(int id)
        {
            try
            {

                var entity = entities.Employees.FirstOrDefault(e => e.ID == id);
                if (entity != null)
                {
                    entities.Employees.Remove(entity);
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, entity);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Employee with id = " + id.ToString() + " not found to delete");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, ex);

            }

        }

        // put/update api/employees
        ////public HttpResponseMessage Put(int id, [FromBody] Employee employee)
        ////{
        ////    try
        ////    {
        ////        using (EmployeeDBEntities entities = new EmployeeDBEntities())
        ////        {
        ////            var entity = entities.Employees.FirstOrDefault(e => e.ID == id);
        ////            if (entity != null)
        ////            {
        ////                entity.FirstName = employee.FirstName;
        ////                entity.LastName = employee.LastName;
        ////                entity.Gender = employee.Gender;
        ////                entity.Salary = employee.Salary;
        ////                entities.SaveChanges();
        ////                return Request.CreateResponse(HttpStatusCode.OK, entity);
        ////            }
        ////            else
        ////            {
        ////                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Employee with id = " + id.ToString() + " not found to update");

        ////            }
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
        ////    }

        ////}

        [HttpPut]
        public HttpResponseMessage Put([FromUri]int id, [FromBody] Employee employee)
        {
            try
            {
                //int id = employee.ID;

                var entity = entities.Employees.FirstOrDefault(e => e.ID == id);
                if (entity != null)
                {
                    entity.FirstName = employee.FirstName;
                    entity.LastName = employee.LastName;
                    entity.Gender = employee.Gender;
                    entity.Salary = employee.Salary;
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, entity);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Employee with id = " + id.ToString() + " not found to update");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }

        }
    }
}
