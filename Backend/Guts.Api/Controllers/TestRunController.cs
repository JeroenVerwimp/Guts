﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Guts.Api.Models;
using Guts.Api.Models.Converters;
using Guts.Business.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guts.Api.Controllers
{
    /// <summary>
    /// Manage test runs.
    /// </summary>
    [Produces("application/json")]
    [Route("api/testruns")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TestRunController : ControllerBase
    {
        private readonly ITestRunConverter _testRunConverter;
        private readonly ITestRunService _testRunService;
        private readonly IAssignmentService _assignmentService;

        public TestRunController(ITestRunConverter testRunConverter, 
            ITestRunService testRunService, 
            IAssignmentService assignmentService)
        {
            _testRunConverter = testRunConverter;
            _testRunService = testRunService;
            _assignmentService = assignmentService;
        }

        /// <summary>
        /// Retrieves a testrun.
        /// </summary>
        /// <param name="id">Identifier of the testrun in the database</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SavedTestRunModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<ActionResult> GetTestRun(int id)
        {
            var storedTestRun = await _testRunService.GetTestRunAsync(id);
            var model = _testRunConverter.ToTestRunModel(storedTestRun);
            return Ok(model);
        }

        /// <summary>
        /// Saves a testrun for an exercise. The testrun may contain results for one, multiple or all tests.
        /// If the exercise (or its chapter) does not exists yet (for the current period) a new exercise / chapter is created for the current period. 
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SavedTestRunModel), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> PostExerciseTestRun([FromBody] ExerciseCreateTestRunModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exercise = await _assignmentService.GetOrCreateExerciseAsync(model.Exercise);
            var testNames = model.Results.Select(testResult => testResult.TestName);
            await _assignmentService.LoadOrCreateTestsForAssignmentAsync(exercise, testNames);

            var testRun = _testRunConverter.From(model.Results, model.SourceCode, GetUserId(), exercise);
            var savedTestRun = await _testRunService.RegisterRunAsync(testRun);

            var savedModel = _testRunConverter.ToTestRunModel(savedTestRun);

            return CreatedAtAction(nameof(GetTestRun), new {id = savedModel.Id}, savedModel);
        }

        public async Task<IActionResult> PostProjectTestRun([FromBody] CreateProjectTestRunModel model)
        {
            //TODO: var component = await _assignmentService.GetOrCreateComponentAsync(model.ProjectComponent);

            var testNames = model.Results.Select(testResult => testResult.TestName);
            //TODO: await _assignmentService.LoadOrCreateTestsForAssignmentAsync(component, testNames);

            //TODO: var testRun = _testRunConverter.From(model.Results, model.SourceCode, GetUserId(), component);
            //TODO: var savedTestRun = await _testRunService.RegisterRunAsync(testRun);

            //TODO: var savedModel = _testRunConverter.ToTestRunModel(savedTestRun);

            //TODO: return CreatedAtAction(nameof(GetTestRun), new { id = savedModel.Id }, savedModel);
            throw new NotImplementedException();
        }
    }
}
