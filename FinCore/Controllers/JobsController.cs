using BusinessObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace FinCore.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route(xtradeConstants.API_ROUTE_CONTROLLER)]
    public class JobsController : BaseController
    {
        [AcceptVerbs("GET")]
        [HttpGet]
        public IEnumerable<ScheduledJobView> Get()
        {
            try
            {
                List<ScheduledJobView> jobs = new List<ScheduledJobView>();
                var list = MainService.GetAllJobsList();
                int i = 1;
                foreach (var job in list)
                    jobs.Add(CreateJobView(i++, job));
                return jobs;
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
            }

            return null;
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        [Route("[action]")]
        public IEnumerable<ScheduledJobView> GetRunning()
        {
            try
            {
                List<ScheduledJobView> jobs = new List<ScheduledJobView>();
                var list = MainService.GetRunningJobs();
                int i = 1;
                foreach (var job in list)
                    jobs.Add(CreateJobView(i++, job.Value));
                return jobs;
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
            }

            return null;
        }

        private ScheduledJobView CreateJobView(int i, ScheduledJobInfo job)
        {
            return new ScheduledJobView
            {
                Id = i,
                IsRunning = job.IsRunning,
                Name = job.Name,
                Group = job.Group,
                PrevDate = new DateTime(job.PrevTime, DateTimeKind.Utc),
                NextDate = new DateTime(job.NextTime, DateTimeKind.Utc),
                Schedule = job.Schedule,
                Log = job.Log
            };
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [AcceptVerbs("POST")]
        [Route("[action]")]
        public ActionResult Post(JobParam query)
        {
            try
            {
                if (query == null)
                    return Problem("Empty Params passed to RunJob method!", "Error", StatusCodes.Status500InternalServerError);

                MainService.RunJobNow(query.Group, query.Name);
                return Ok($"Job {query.Name} Launched!");
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult Stop(JobParam query)
        {
            try
            {
                if (query == null)
                    return Problem("Empty Params passed to RunJob method!", "Error", StatusCodes.Status500InternalServerError);

                MainService.StopJobNow(query.Group, query.Name);
                return Ok($"Job {query.Name} Stop Request Sent!");
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Problem(e.ToString(), "Error", StatusCodes.Status500InternalServerError);
            }
        }

        public class JobParam
        {
            public string Group { get; set; }
            public string Name { get; set; }
        }
    }
}