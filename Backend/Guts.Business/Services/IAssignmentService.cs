﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guts.Domain;

namespace Guts.Business.Services
{
    public interface IAssignmentService
    {
        Task<Exercise> GetOrCreateExerciseAsync(ExerciseDto exerciseDto);
        Task<ProjectComponent> GetOrCreateProjectComponentAsync(ProjectComponentDto componentDto);

        Task LoadOrCreateTestsForAssignmentAsync(Assignment assignment, IEnumerable<string> testNames);

        Task<ExerciseResultDto> GetResultsForUserAsync(int exerciseId, int userId, DateTime? dateUtc);
        Task<ExerciseTestRunInfoDto> GetUserTestRunInfoForExercise(int exerciseId, int userId, DateTime? dateUtc);
    }
}