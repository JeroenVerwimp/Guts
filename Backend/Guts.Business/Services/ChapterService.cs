﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Guts.Business.Converters;
using Guts.Data;
using Guts.Data.Repositories;
using Guts.Domain;

namespace Guts.Business.Services
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IPeriodRepository _periodRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly IAssignmentWitResultsConverter _assignmentWitResultsConverter;

        public ChapterService(IChapterRepository chapterRepository, 
            ICourseRepository courseRepository, 
            IPeriodRepository periodRepository,
            ITestResultRepository testResultRepository,
            IAssignmentWitResultsConverter assignmentWitResultsConverter)
        {
            _chapterRepository = chapterRepository;
            _courseRepository = courseRepository;
            _periodRepository = periodRepository;
            _testResultRepository = testResultRepository;
            _assignmentWitResultsConverter = assignmentWitResultsConverter;
        }

        public async Task<Chapter> GetOrCreateChapterAsync(string courseCode, int chapterNumber)
        {
            var currentPeriod = await _periodRepository.GetCurrentPeriodAsync();

            Chapter chapter;
            try
            {
                chapter = await _chapterRepository.GetSingleAsync(courseCode, chapterNumber, currentPeriod.Id);
            }
            catch (DataNotFoundException)
            {
                var course = await _courseRepository.GetSingleAsync(courseCode);
                chapter = new Chapter
                {
                    Number = chapterNumber,
                    CourseId = course.Id,
                    PeriodId = currentPeriod.Id
                };
                chapter = await _chapterRepository.AddAsync(chapter);
            }

            return chapter;
        }

        public async Task<Chapter> LoadChapterAsync(int courseId, int chapterNumber)
        {
            var currentPeriod = await _periodRepository.GetCurrentPeriodAsync();

            var chapter = await _chapterRepository.LoadWithExercisesAsync(courseId, chapterNumber, currentPeriod.Id);

            return chapter;
        }

        public async Task<Chapter> LoadChapterWithTestsAsync(int courseId, int chapterNumber)
        {
            var currentPeriod = await _periodRepository.GetCurrentPeriodAsync();

            var chapter = await _chapterRepository.LoadWithExercisesAndTestsAsync(courseId, chapterNumber, currentPeriod.Id);

            return chapter;
        }

        public async Task<IList<AssignmentResultDto>> GetResultsForUserAsync(Chapter chapter, int userId, DateTime? dateUtc)
        {
            if (chapter.Exercises == null)
            {
                throw new ArgumentException("The chapter should have its exercises loaded");
            }

            var results = new List<AssignmentResultDto>();
            foreach (var exercise in chapter.Exercises)
            {
                var dto = new AssignmentResultDto
                {
                    AssignmentId = exercise.Id,
                    TestResults = await _testResultRepository.GetLastTestResultsOfExerciseAsync(exercise.Id, userId, dateUtc)
                };
                results.Add(dto);
            }

            return results;
        }

        public async Task<IList<AssignmentStatisticsDto>> GetChapterStatisticsAsync(Chapter chapter, DateTime? dateUtc)
        {
            var results = new List<AssignmentStatisticsDto>();
            foreach (var exercise in chapter.Exercises)
            {
                var testResults =
                    await _testResultRepository.GetLastTestResultsOfExerciseAsync(exercise.Id, null, dateUtc);
                results.Add(_assignmentWitResultsConverter.ToAssignmentStatisticsDto(exercise.Id, testResults));
               
            }
            return results;
        }

        public async Task<IList<Chapter>> GetChaptersOfCourseAsync(int courseId)
        {
            Period currentPeriod;
            try
            {
                currentPeriod = await _periodRepository.GetCurrentPeriodAsync();
            }
            catch (DataNotFoundException)
            {
                return new List<Chapter>();
            }

            var chapters = await _chapterRepository.GetByCourseIdAsync(courseId, currentPeriod.Id);
            return chapters.OrderBy(chapter => chapter.Number).ToList();
        }
    }
}