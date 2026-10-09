using System;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    // Intended only for a supervised, isolated examination-centre deployment.
    // National-number-only access is NOT suitable for a public Internet API.
    [ApiController]
    [Route("api/official-exams")]
    public sealed class OfficialExamsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OfficialExamsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPut("{examAttemptID:long}/answers/{examQuestionID:long}")]
        public IActionResult SaveAnswer(
            long examAttemptID,
            long examQuestionID,
            [FromBody] SaveOfficialExamAnswerRequest request)
        {
            if (request == null || examAttemptID <= 0 || examQuestionID <= 0)
                return BadRequest(new { message = "Valid attempt, question and request required." });
            try
            {
                clsOfficialExamSession.SaveAnswer(examAttemptID, examQuestionID,
                                                   request.SelectedOption);
                return Ok(new { ExamAttemptID = examAttemptID,
                                ExamQuestionID = examQuestionID,
                                Saved = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("{examAttemptID:long}/submit")]
        public IActionResult Submit(long examAttemptID)
        {
            if (examAttemptID <= 0)
                return BadRequest(new { message = "Invalid exam attempt." });

            try
            {
                return Ok(clsOfficialExamSession.SubmitExam(examAttemptID));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("start")]
        public IActionResult Start([FromBody] StartOfficialExamRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NationalNo))
            {
                return BadRequest(new
                {
                    message = "NationalNo is required."
                });
            }

            // Exam policy is SERVER-OWNED. Never accept question count,
            // duration or pass threshold from the candidate/request body.
            int? count = _configuration.GetValue<int?>("OfficialExam:QuestionCount");
            int? pass = _configuration.GetValue<int?>("OfficialExam:RequiredCorrectAnswers");
            int? duration = _configuration.GetValue<int?>("OfficialExam:DurationSeconds");
            int? early = _configuration.GetValue<int?>("OfficialExam:StartEarlyMinutes");
            int? late = _configuration.GetValue<int?>("OfficialExam:StartLateMinutes");

            if (!count.HasValue || !pass.HasValue || !duration.HasValue ||
                !early.HasValue || !late.HasValue ||
                count.Value < 1 || count.Value > 200 ||
                pass.Value < 1 || pass.Value > count.Value ||
                duration.Value < 1 || duration.Value > 86400 ||
                early.Value < 0 || early.Value > 1440 ||
                late.Value < 0 || late.Value > 1440)
            {
                return StatusCode(503, new
                {
                    message = "Official exam policy is not configured or is invalid."
                });
            }

            try
            {
                clsOfficialExamStart.StartResult result =
                    clsOfficialExamStart.StartByNationalNo(
                        request.NationalNo,
                        count.Value,
                        pass.Value,
                        duration.Value,
                        early.Value,
                        late.Value);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public sealed class SaveOfficialExamAnswerRequest
    {
        // A/B for TrueFalse; A/B/C/D for MultipleChoice; null clears the answer.
        public string SelectedOption { get; set; }
    }

    public sealed class StartOfficialExamRequest
    {
        public string NationalNo { get; set; } = string.Empty;
    }
}
