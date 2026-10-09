using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DVLD_DataAccess
{
    public static class clsQuestionBankData
    {
        public static int AddNewQuestion(
            string questionText,
            string questionType,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            string correctOption,
            string explanation,
            int? sourceDocumentID,
            int? sourcePageNumber,
            string sourceEvidence = null,
            int? sourceChunkIndex = null,
            string reviewStatus = "Draft")
        {
            int questionID = -1;

            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    INSERT INTO QuestionBank
                    (
                        QuestionText,
                        QuestionType,
                        OptionA,
                        OptionB,
                        OptionC,
                        OptionD,
                        CorrectOption,
                        Explanation,
                        SourceDocumentID,
                        SourcePageNumber,
                        SourceEvidence,
                        SourceChunkIndex,
                        ReviewStatus
                    )
                    VALUES
                    (
                        @QuestionText,
                        @QuestionType,
                        @OptionA,
                        @OptionB,
                        @OptionC,
                        @OptionD,
                        @CorrectOption,
                        @Explanation,
                        @SourceDocumentID,
                        @SourcePageNumber,
                        @SourceEvidence,
                        @SourceChunkIndex,
                        @ReviewStatus
                    );

                    SELECT SCOPE_IDENTITY();";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionText",
                        SqlDbType.NVarChar,
                        1000).Value =
                        questionText;

                    command.Parameters.Add(
                        "@QuestionType",
                        SqlDbType.NVarChar,
                        20).Value =
                        questionType;

                    command.Parameters.Add(
                        "@OptionA",
                        SqlDbType.NVarChar,
                        500).Value =
                        optionA;

                    command.Parameters.Add(
                        "@OptionB",
                        SqlDbType.NVarChar,
                        500).Value =
                        optionB;

                    command.Parameters.Add(
                        "@OptionC",
                        SqlDbType.NVarChar,
                        500).Value =
                        string.IsNullOrWhiteSpace(optionC)
                            ? (object)DBNull.Value
                            : optionC;

                    command.Parameters.Add(
                        "@OptionD",
                        SqlDbType.NVarChar,
                        500).Value =
                        string.IsNullOrWhiteSpace(optionD)
                            ? (object)DBNull.Value
                            : optionD;

                    command.Parameters.Add(
                        "@CorrectOption",
                        SqlDbType.Char,
                        1).Value =
                        correctOption;

                    command.Parameters.Add(
                        "@Explanation",
                        SqlDbType.NVarChar,
                        2000).Value =
                        string.IsNullOrWhiteSpace(explanation)
                            ? (object)DBNull.Value
                            : explanation;

                    command.Parameters.Add(
                        "@SourceDocumentID",
                        SqlDbType.Int).Value =
                        sourceDocumentID.HasValue
                            ? (object)sourceDocumentID.Value
                            : DBNull.Value;

                    command.Parameters.Add(
                        "@SourcePageNumber",
                        SqlDbType.Int).Value =
                        sourcePageNumber.HasValue
                            ? (object)sourcePageNumber.Value
                            : DBNull.Value;

                    command.Parameters.Add(
                        "@SourceEvidence",
                        SqlDbType.NVarChar,
                        4000).Value =
                        string.IsNullOrWhiteSpace(sourceEvidence)
                            ? (object)DBNull.Value
                            : sourceEvidence;

                    command.Parameters.Add(
                        "@SourceChunkIndex",
                        SqlDbType.Int).Value =
                        sourceChunkIndex.HasValue
                            ? (object)sourceChunkIndex.Value
                            : DBNull.Value;

                    command.Parameters.Add(
                        "@ReviewStatus",
                        SqlDbType.NVarChar,
                        20).Value =
                        string.IsNullOrWhiteSpace(reviewStatus)
                            ? "Draft"
                            : reviewStatus;

                    connection.Open();

                    object result =
                        command.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        questionID =
                            Convert.ToInt32(result);
                    }
                }
            }

            return questionID;
        }


        // =====================================================
        // فحص التكرار قبل حفظ السؤال
        // =====================================================
        // يعتبر السؤال مكرراً إذا كان في نفس الوثيقة:
        // 1) نفس نص السؤال تماماً بعد Trim
        // أو
        // 2) نفس SourceEvidence تماماً بعد Trim
        //
        // الأسئلة المرفوضة أو غير الفعالة لا تمنع إنشاء سؤال جديد.
        // =====================================================

        public static bool IsDuplicateQuestion(
            string questionText,
            int? sourceDocumentID,
            string sourceEvidence)
        {
            if (string.IsNullOrWhiteSpace(
                    questionText))
            {
                return false;
            }


            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    SELECT CASE
                        WHEN EXISTS
                        (
                            SELECT 1
                            FROM QuestionBank
                            WHERE
                                ISNULL(IsActive, 1) = 1

                                AND ISNULL(
                                        ReviewStatus,
                                        N'Draft'
                                    ) <> N'Rejected'

                                AND
                                (
                                    (
                                        SourceDocumentID =
                                            @SourceDocumentID
                                    )
                                    OR
                                    (
                                        SourceDocumentID IS NULL
                                        AND
                                        @SourceDocumentID IS NULL
                                    )
                                )

                                AND
                                (
                                    LTRIM(RTRIM(QuestionText)) =
                                        LTRIM(RTRIM(@QuestionText))

                                    OR
                                    (
                                        @SourceEvidence IS NOT NULL
                                        AND
                                        SourceEvidence IS NOT NULL
                                        AND
                                        LTRIM(RTRIM(SourceEvidence)) =
                                            LTRIM(RTRIM(@SourceEvidence))
                                    )
                                )
                        )
                        THEN 1
                        ELSE 0
                    END;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionText",
                        SqlDbType.NVarChar,
                        1000).Value =
                        questionText.Trim();


                    command.Parameters.Add(
                        "@SourceDocumentID",
                        SqlDbType.Int).Value =
                        sourceDocumentID.HasValue
                            ? (object)sourceDocumentID.Value
                            : DBNull.Value;


                    command.Parameters.Add(
                        "@SourceEvidence",
                        SqlDbType.NVarChar,
                        4000).Value =
                        string.IsNullOrWhiteSpace(
                            sourceEvidence)
                            ? (object)DBNull.Value
                            : sourceEvidence.Trim();


                    connection.Open();


                    object result =
                        command.ExecuteScalar();


                    return
                        result != null &&
                        result != DBNull.Value &&
                        Convert.ToInt32(result) == 1;
                }
            }
        }

        // =====================================================
        // تحديث حالة مراجعة السؤال
        // Draft -> Approved / Rejected
        // =====================================================

        public static bool UpdateReviewStatus(
            int questionID,
            string reviewStatus)
        {
            if (questionID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(questionID));
            }


            if (reviewStatus != "Approved" &&
                reviewStatus != "Rejected")
            {
                throw new ArgumentException(
                    "ReviewStatus must be Approved or Rejected.",
                    nameof(reviewStatus));
            }


            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    UPDATE QuestionBank
                    SET ReviewStatus = @ReviewStatus
                    WHERE QuestionID = @QuestionID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionID",
                        SqlDbType.Int).Value =
                        questionID;


                    command.Parameters.Add(
                        "@ReviewStatus",
                        SqlDbType.NVarChar,
                        20).Value =
                        reviewStatus;


                    connection.Open();


                    int affectedRows =
                        command.ExecuteNonQuery();


                    return affectedRows > 0;
                }
            }
        }

        // =====================================================
        // جلب سؤال واحد حسب QuestionID
        // يستخدمه Business Layer في Find(...)
        // =====================================================

        public static bool GetQuestionInfoByID(
            int questionID,
            ref string questionText,
            ref string questionType,
            ref string optionA,
            ref string optionB,
            ref string optionC,
            ref string optionD,
            ref string correctOption,
            ref string explanation,
            ref int? sourceDocumentID,
            ref int? sourcePageNumber,
            ref string sourceEvidence,
            ref int? sourceChunkIndex,
            ref string reviewStatus,
            ref DateTime createdAt,
            ref bool isActive)
        {
            if (questionID <= 0)
            {
                return false;
            }


            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    SELECT TOP 1
                        QuestionText,
                        QuestionType,
                        OptionA,
                        OptionB,
                        OptionC,
                        OptionD,
                        CorrectOption,
                        Explanation,
                        SourceDocumentID,
                        SourcePageNumber,
                        SourceEvidence,
                        SourceChunkIndex,
                        ReviewStatus,
                        CreatedAt,
                        IsActive
                    FROM QuestionBank
                    WHERE QuestionID = @QuestionID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionID",
                        SqlDbType.Int).Value =
                        questionID;


                    connection.Open();


                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }


                        questionText =
                            reader["QuestionText"] == DBNull.Value
                                ? string.Empty
                                : reader["QuestionText"].ToString();

                        questionType =
                            reader["QuestionType"] == DBNull.Value
                                ? "MultipleChoice"
                                : reader["QuestionType"].ToString();

                        optionA =
                            reader["OptionA"] == DBNull.Value
                                ? string.Empty
                                : reader["OptionA"].ToString();

                        optionB =
                            reader["OptionB"] == DBNull.Value
                                ? string.Empty
                                : reader["OptionB"].ToString();

                        optionC =
                            reader["OptionC"] == DBNull.Value
                                ? string.Empty
                                : reader["OptionC"].ToString();

                        optionD =
                            reader["OptionD"] == DBNull.Value
                                ? string.Empty
                                : reader["OptionD"].ToString();

                        correctOption =
                            reader["CorrectOption"] == DBNull.Value
                                ? string.Empty
                                : reader["CorrectOption"].ToString();

                        explanation =
                            reader["Explanation"] == DBNull.Value
                                ? string.Empty
                                : reader["Explanation"].ToString();

                        sourceDocumentID =
                            reader["SourceDocumentID"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(
                                    reader["SourceDocumentID"]);

                        sourcePageNumber =
                            reader["SourcePageNumber"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(
                                    reader["SourcePageNumber"]);

                        sourceEvidence =
                            reader["SourceEvidence"] == DBNull.Value
                                ? string.Empty
                                : reader["SourceEvidence"].ToString();

                        sourceChunkIndex =
                            reader["SourceChunkIndex"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(
                                    reader["SourceChunkIndex"]);

                        reviewStatus =
                            reader["ReviewStatus"] == DBNull.Value
                                ? "Draft"
                                : reader["ReviewStatus"].ToString();

                        createdAt =
                            reader["CreatedAt"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(
                                    reader["CreatedAt"]);

                        isActive =
                            reader["IsActive"] == DBNull.Value
                                ? true
                                : Convert.ToBoolean(
                                    reader["IsActive"]);


                        return true;
                    }
                }
            }
        }


        // =====================================================
        // تعديل محتوى السؤال
        // أي تعديل يعيد السؤال إلى Draft لإعادة مراجعته
        // بيانات المصدر لا يتم تعديلها هنا للحفاظ على provenance
        // =====================================================

        public static bool UpdateQuestion(
            int questionID,
            string questionText,
            string questionType,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            string correctOption,
            string explanation)
        {
            if (questionID <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(questionID));
            }


            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    UPDATE QuestionBank
                    SET
                        QuestionText = @QuestionText,
                        QuestionType = @QuestionType,
                        OptionA = @OptionA,
                        OptionB = @OptionB,
                        OptionC = @OptionC,
                        OptionD = @OptionD,
                        CorrectOption = @CorrectOption,
                        Explanation = @Explanation,
                        ReviewStatus = N'Draft'
                    WHERE QuestionID = @QuestionID;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.Add(
                        "@QuestionID",
                        SqlDbType.Int).Value =
                        questionID;

                    command.Parameters.Add(
                        "@QuestionText",
                        SqlDbType.NVarChar,
                        1000).Value =
                        questionText;

                    command.Parameters.Add(
                        "@QuestionType",
                        SqlDbType.NVarChar,
                        20).Value =
                        questionType;

                    command.Parameters.Add(
                        "@OptionA",
                        SqlDbType.NVarChar,
                        500).Value =
                        optionA;

                    command.Parameters.Add(
                        "@OptionB",
                        SqlDbType.NVarChar,
                        500).Value =
                        optionB;

                    command.Parameters.Add(
                        "@OptionC",
                        SqlDbType.NVarChar,
                        500).Value =
                        string.IsNullOrWhiteSpace(optionC)
                            ? (object)DBNull.Value
                            : optionC;

                    command.Parameters.Add(
                        "@OptionD",
                        SqlDbType.NVarChar,
                        500).Value =
                        string.IsNullOrWhiteSpace(optionD)
                            ? (object)DBNull.Value
                            : optionD;

                    command.Parameters.Add(
                        "@CorrectOption",
                        SqlDbType.Char,
                        1).Value =
                        correctOption;

                    command.Parameters.Add(
                        "@Explanation",
                        SqlDbType.NVarChar,
                        2000).Value =
                        string.IsNullOrWhiteSpace(explanation)
                            ? (object)DBNull.Value
                            : explanation;


                    connection.Open();


                    int affectedRows =
                        command.ExecuteNonQuery();


                    return affectedRows > 0;
                }
            }
        }



        // =====================================================
        // جلب جميع أسئلة بنك الأسئلة لشاشة الإدارة
        // =====================================================

        public static DataTable GetAllQuestions()
        {
            DataTable table =
                new DataTable();


            using (SqlConnection connection =
                   new SqlConnection(
                       clsDataAccessSettings.ConnectionString))
            {
                string query = @"
                    SELECT
                        QuestionID,
                        QuestionText,
                        QuestionType,
                        OptionA,
                        OptionB,
                        OptionC,
                        OptionD,
                        CorrectOption,
                        Explanation,
                        SourceDocumentID,
                        SourcePageNumber,
                        SourceEvidence,
                        SourceChunkIndex,
                        ReviewStatus,
                        CreatedAt,
                        IsActive
                    FROM QuestionBank
                    ORDER BY QuestionID DESC;";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    connection.Open();


                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        table.Load(
                            reader);
                    }
                }
            }


            return table;
        }

        public static bool SetQuestionActive(int questionID, bool isActive)
        {
            if (questionID <= 0)
                return false;

            using (SqlConnection connection = new SqlConnection(
                clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            UPDATE QuestionBank
            SET IsActive = @IsActive
            WHERE QuestionID = @QuestionID";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@QuestionID", SqlDbType.Int).Value =
                        questionID;

                    command.Parameters.Add(
                        "@IsActive", SqlDbType.Bit).Value =
                        isActive;

                    connection.Open();

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool DeleteQuestion(int questionID)
        {
            if (questionID <= 0)
                return false;

            using (SqlConnection connection = new SqlConnection(
                clsDataAccessSettings.ConnectionString))
            {
                string query = @"
            DELETE FROM dbo.QuestionBank
            WHERE QuestionID = @QuestionID
              AND NOT EXISTS
              (
                  SELECT 1
                  FROM dbo.OfficialExamQuestions
                  WHERE QuestionID = @QuestionID
              );";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@QuestionID", SqlDbType.Int).Value =
                        questionID;

                    connection.Open();

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

    }
}
