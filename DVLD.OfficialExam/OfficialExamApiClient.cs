
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DVLD.OfficialExam
{
    internal sealed class OfficialExamApiClient
    {
        private readonly Uri _baseUrl;

        private static readonly HttpClient _httpClient =
            new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

        public OfficialExamApiClient()
        {
            string url = ConfigurationManager.AppSettings[
                "OfficialExamApiBaseUrl"];

            Uri baseUrl;

            if (string.IsNullOrWhiteSpace(url) ||
                !Uri.TryCreate(url, UriKind.Absolute, out baseUrl) ||
                baseUrl.Scheme != Uri.UriSchemeHttps)
            {
                throw new ConfigurationErrorsException(
                    "OfficialExamApiBaseUrl is missing or invalid.");
            }

            _baseUrl = baseUrl;
        }

        // Test connectivity without starting an exam.
        public async Task<bool> CheckConnectionAsync()
        {
            Uri endpoint = new Uri(
                _baseUrl,
                "openapi/v1.json");

            using (HttpResponseMessage response =
                await _httpClient.GetAsync(endpoint))
            {
                return response.IsSuccessStatusCode;
            }
        }

        // Start an official exam and retrieve candidate-safe questions.
        public async Task<StartExamResponse> StartExamAsync(
            string nationalNo)
        {
            if (string.IsNullOrWhiteSpace(nationalNo))
            {
                throw new ArgumentException(
                    "الرجاء إدخال الرقم الوطني.");
            }

            Uri endpoint = new Uri(
                _baseUrl,
                "api/official-exams/start");

            JavaScriptSerializer serializer =
                new JavaScriptSerializer();

            string json = serializer.Serialize(new
            {
                nationalNo = nationalNo.Trim()
            });

            using (StringContent content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"))
            using (HttpResponseMessage response =
                await _httpClient.PostAsync(endpoint, content))
            {
                string responseBody =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string message =
                        "تعذر بدء الامتحان. HTTP " +
                        (int)response.StatusCode;

                    try
                    {
                        ApiErrorResponse error =
                            serializer.Deserialize<ApiErrorResponse>(
                                responseBody);

                        if (error != null &&
                            !string.IsNullOrWhiteSpace(error.message))
                        {
                            message = error.message;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Keep the HTTP error if the body is not JSON.
                    }

                    throw new InvalidOperationException(message);
                }

                StartExamResponse result =
                    serializer.Deserialize<StartExamResponse>(
                        responseBody);

                if (result == null ||
                    result.examAttemptID <= 0 ||
                    result.questions == null ||
                    result.questions.Count == 0)
                {
                    throw new InvalidOperationException(
                        "استجابة بدء الامتحان غير مكتملة.");
                }

                return result;
            }
        }


        public async Task SaveAnswerAsync(
            long examAttemptID,
            long examQuestionID,
            string selectedOption)
        {
            if (examAttemptID <= 0 || examQuestionID <= 0)
            {
                throw new ArgumentException(
                    "بيانات محاولة الامتحان غير صحيحة.");
            }

            Uri endpoint = new Uri(
                _baseUrl,
                "api/official-exams/" + examAttemptID +
                "/answers/" + examQuestionID);

            JavaScriptSerializer serializer =
                new JavaScriptSerializer();

            string json = serializer.Serialize(new
            {
                selectedOption = selectedOption
            });

            using (StringContent content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"))
            using (HttpResponseMessage response =
                await _httpClient.PutAsync(endpoint, content))
            {
                string responseBody =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string message =
                        "تعذر حفظ الإجابة. HTTP " +
                        (int)response.StatusCode;

                    try
                    {
                        ApiErrorResponse error =
                            serializer.Deserialize<ApiErrorResponse>(
                                responseBody);

                        if (error != null &&
                            !string.IsNullOrWhiteSpace(error.message))
                        {
                            message = error.message;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Keep the HTTP error if the response is invalid.
                    }

                    throw new InvalidOperationException(message);
                }
            }
        }


        public async Task<SubmitExamResponse> SubmitExamAsync(
            long examAttemptID)
        {
            if (examAttemptID <= 0)
            {
                throw new ArgumentException(
                    "رقم محاولة الامتحان غير صحيح.");
            }

            Uri endpoint = new Uri(
                _baseUrl,
                "api/official-exams/" + examAttemptID + "/submit");

            JavaScriptSerializer serializer =
                new JavaScriptSerializer();

            using (HttpResponseMessage response =
                await _httpClient.PostAsync(endpoint, null))
            {
                string responseBody =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string message =
                        "تعذر تسليم الامتحان. HTTP " +
                        (int)response.StatusCode;

                    try
                    {
                        ApiErrorResponse error =
                            serializer.Deserialize<ApiErrorResponse>(
                                responseBody);

                        if (error != null &&
                            !string.IsNullOrWhiteSpace(error.message))
                        {
                            message = error.message;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Keep the original HTTP error.
                    }

                    throw new InvalidOperationException(message);
                }

                SubmitExamResponse result =
                    serializer.Deserialize<SubmitExamResponse>(
                        responseBody);

                if (result == null ||
                    result.examAttemptID != examAttemptID ||
                    result.testID <= 0)
                {
                    throw new InvalidOperationException(
                        "استجابة تسليم الامتحان غير مكتملة.");
                }

                return result;
            }
        }
    }



    // Property names match the API JSON response.
    internal sealed class StartExamResponse
    {
        public long examAttemptID { get; set; }

        public int testAppointmentID { get; set; }

        public string startedAtUtc { get; set; }

        public string deadlineAtUtc { get; set; }

        public List<ExamQuestion> questions { get; set; }
    }


    internal sealed class SubmitExamResponse
    {
        public long examAttemptID { get; set; }

        public int testAppointmentID { get; set; }

        public int testID { get; set; }

        public int questionCount { get; set; }

        public int requiredCorrectAnswers { get; set; }

        public int correctAnswers { get; set; }

        public bool passed { get; set; }

        public string status { get; set; }

        public string finishedAtUtc { get; set; }
    }

    internal sealed class ExamQuestion
    {
        public long examQuestionID { get; set; }

        public short questionNumber { get; set; }

        public string questionType { get; set; }

        public string questionText { get; set; }

        public string optionA { get; set; }

        public string optionB { get; set; }

        public string optionC { get; set; }

        public string optionD { get; set; }
    }

    internal sealed class ApiErrorResponse
    {
        public string message { get; set; }
    }
}