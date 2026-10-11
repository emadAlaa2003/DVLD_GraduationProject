using System.Data;
using DVLD.Api.Authentication;
using DVLD_Buisness;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/practice-exams")]
    [Authorize(AuthenticationSchemes = MobileAuthDefaults.Scheme)]
    public sealed class PracticeExamsController : ControllerBase
    {
        [HttpPost("start")]
        public IActionResult Start([FromBody] PracticeExamStartRequest? request)
        {
            if (request == null ||
                request.QuestionCount < 1 || request.QuestionCount > 20)
            {
                return BadRequest(new
                {
                    message = "Question count must be between 1 and 20."
                });
            }

            DataTable questions =
                clsQuestionBank.GetRandomApprovedQuestions(request.QuestionCount);

            if (questions.Rows.Count == 0)
            {
                return Conflict(new
                {
                    message = "No valid approved and active practice questions are available."
                });
            }

            var response = new PracticeExamStartResponse
            {
                RequestedQuestionCount = request.QuestionCount,
                QuestionCount = questions.Rows.Count
            };

            // This DTO deliberately has no answer, explanation or source fields.
            foreach (DataRow row in questions.Rows)
            {
                string questionType = Convert.ToString(row["QuestionType"])!;
                bool trueFalse = questionType == "TrueFalse";

                response.Questions.Add(new PracticeExamQuestionResponse
                {
                    QuestionID = Convert.ToInt32(row["QuestionID"]),
                    QuestionType = questionType,
                    QuestionText = Convert.ToString(row["QuestionText"])!,
                    OptionA = Convert.ToString(row["OptionA"])!,
                    OptionB = Convert.ToString(row["OptionB"])!,
                    OptionC = trueFalse ? null : Convert.ToString(row["OptionC"]),
                    OptionD = trueFalse ? null : Convert.ToString(row["OptionD"])
                });
            }

            return Ok(response);
        }

        [HttpPost("submit")]
        public IActionResult Submit([FromBody] PracticeExamSubmitRequest? request)
        {
            if (request?.Answers == null || request.Answers.Count == 0)
            {
                return BadRequest(new
                {
                    message = "At least one answer is required."
                });
            }

            var questionIDs = new HashSet<int>();
            var answers = new List<PracticeExamAnswer>();

            // Validate the complete request before looking up any questions.
            foreach (PracticeExamAnswer? answer in request.Answers)
            {
                if (answer == null || answer.QuestionID <= 0 ||
                    !questionIDs.Add(answer.QuestionID))
                {
                    return BadRequest(new
                    {
                        message = "Each answer must have a positive, unique question ID."
                    });
                }

                string? selectedOption = answer.SelectedOption?.Trim().ToUpperInvariant();

                if (!IsValidOption(selectedOption))
                {
                    return BadRequest(new
                    {
                        message = "Selected option must be A, B, C or D."
                    });
                }

                answers.Add(new PracticeExamAnswer
                {
                    QuestionID = answer.QuestionID,
                    SelectedOption = selectedOption
                });
            }

            var response = new PracticeExamSubmitResponse
            {
                QuestionCount = answers.Count,
                AnsweredCount = answers.Count
            };

            foreach (PracticeExamAnswer answer in answers)
            {
                clsQuestionBank? question = clsQuestionBank.Find(answer.QuestionID);

                if (question == null || !IsValidQuestion(question))
                {
                    return BadRequest(new
                    {
                        message = $"Question {answer.QuestionID} must be valid, approved and active."
                    });
                }

                if (question.QuestionType == clsQuestionBank.enQuestionType.TrueFalse &&
                    answer.SelectedOption != "A" && answer.SelectedOption != "B")
                {
                    return BadRequest(new
                    {
                        message = $"TrueFalse question {answer.QuestionID} only accepts A or B."
                    });
                }

                string correctOption = question.CorrectOption.Trim().ToUpperInvariant();
                bool isCorrect = answer.SelectedOption == correctOption;

                if (isCorrect) response.CorrectAnswers++;

                response.Results.Add(new PracticeExamAnswerResult
                {
                    QuestionID = answer.QuestionID,
                    SelectedOption = answer.SelectedOption!,
                    CorrectOption = correctOption,
                    IsCorrect = isCorrect,
                    Explanation = string.IsNullOrWhiteSpace(question.Explanation)
                        ? null : question.Explanation
                });
            }

            response.Percentage = response.CorrectAnswers * 100.0 / response.QuestionCount;

            return Ok(response);
        }

        private static bool IsValidOption(string? option)
        {
            return option == "A" || option == "B" || option == "C" || option == "D";
        }

        private static bool IsValidQuestion(clsQuestionBank question)
        {
            if (question.ReviewStatus != clsQuestionBank.enReviewStatus.Approved ||
                !question.IsActive ||
                string.IsNullOrWhiteSpace(question.QuestionText) ||
                string.IsNullOrWhiteSpace(question.OptionA) ||
                string.IsNullOrWhiteSpace(question.OptionB))
            {
                return false;
            }

            string? correctOption = question.CorrectOption?.Trim().ToUpperInvariant();

            if (question.QuestionType == clsQuestionBank.enQuestionType.TrueFalse)
                return correctOption == "A" || correctOption == "B";

            return question.QuestionType == clsQuestionBank.enQuestionType.MultipleChoice &&
                !string.IsNullOrWhiteSpace(question.OptionC) &&
                !string.IsNullOrWhiteSpace(question.OptionD) &&
                IsValidOption(correctOption);
        }
    }

    public sealed class PracticeExamStartRequest
    {
        public int QuestionCount { get; set; }
    }

    public sealed class PracticeExamStartResponse
    {
        public int RequestedQuestionCount { get; set; }
        public int QuestionCount { get; set; }
        public List<PracticeExamQuestionResponse> Questions { get; set; } = new();
    }

    public sealed class PracticeExamQuestionResponse
    {
        public int QuestionID { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
    }

    public sealed class PracticeExamSubmitRequest
    {
        public List<PracticeExamAnswer?>? Answers { get; set; }
    }

    public sealed class PracticeExamAnswer
    {
        public int QuestionID { get; set; }
        public string? SelectedOption { get; set; }
    }

    public sealed class PracticeExamSubmitResponse
    {
        public int QuestionCount { get; set; }
        public int AnsweredCount { get; set; }
        public int CorrectAnswers { get; set; }
        public double Percentage { get; set; }
        public List<PracticeExamAnswerResult> Results { get; set; } = new();
    }

    public sealed class PracticeExamAnswerResult
    {
        public int QuestionID { get; set; }
        public string SelectedOption { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }
}
