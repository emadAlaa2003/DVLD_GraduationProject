using DVLD.AI;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/knowledge-documents")]
    public class KnowledgeDocumentsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public KnowledgeDocumentsController(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }


        [HttpPost]
        public async Task<IActionResult> UploadDocument(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "No file was uploaded."
                });
            }

            string extension =
                Path.GetExtension(file.FileName);

            if (!string.Equals(
                    extension,
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only PDF files are allowed."
                });
            }


            string documentsFolder = Path.Combine(
                _environment.ContentRootPath,
                "KnowledgeDocuments");

            Directory.CreateDirectory(
                documentsFolder);


            string storedFileName =
                Guid.NewGuid().ToString() + ".pdf";

            string fullPath = Path.Combine(
                documentsFolder,
                storedFileName);


            try
            {
                using (FileStream stream =
                       new FileStream(
                           fullPath,
                           FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                throw;
            }


            int documentID;

            try
            {
                documentID =
                    clsKnowledgeDocument.AddNewDocument(
                        Path.GetFileName(file.FileName),
                        storedFileName,
                        fullPath,
                        file.Length);

                if (documentID == -1)
                {
                    throw new Exception(
                        "Could not save document information.");
                }
            }
            catch
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                throw;
            }


            try
            {
                PdfExtractionResult extractionResult =
                    PdfTextExtractor.Extract(
                        fullPath);


                clsKnowledgeDocument.UpdateAfterTextExtraction(
                    documentID,
                    extractionResult.TotalPages);


                List<TextChunk> chunks =
                    TextChunker.CreateChunks(
                        extractionResult);


                clsKnowledgeDocument.UpdateChunkCount(
                    documentID,
                    chunks.Count);


                await KnowledgeDocumentVectorProcessor.ProcessAsync(
                    documentID,
                    chunks);


                clsKnowledgeDocument.MarkProcessingCompleted(
                    documentID,
                    chunks.Count);


                return Created(
                    $"/api/knowledge-documents/{documentID}",
                    new
                    {
                        DocumentID =
                            documentID,

                        OriginalFileName =
                            Path.GetFileName(
                                file.FileName),

                        FileSizeBytes =
                            file.Length,

                        TotalPages =
                            extractionResult.TotalPages,

                        ChunkCount =
                            chunks.Count,

                        FirstChunkPage =
                            chunks.Count > 0
                                ? chunks[0].PageNumber
                                : (int?)null,

                        FirstChunkPreview =
                            chunks.Count > 0
                                ? chunks[0].Text
                                : null,

                        ProcessingStatus =
                            "Ready"
                    });
            }
            catch (Exception ex)
            {
                clsKnowledgeDocument.MarkProcessingFailed(
                    documentID,
                    ex.Message);

                throw;
            }
        }


        [HttpGet("qdrant-health")]
        public async Task<IActionResult> QdrantHealth()
        {
            bool isConnected =
                await QdrantConnection.TestConnectionAsync();

            if (!isConnected)
            {
                return StatusCode(
                    503,
                    new
                    {
                        status =
                            "Unavailable",

                        qdrant =
                            "Disconnected"
                    });
            }

            return Ok(
                new
                {
                    status =
                        "Running",

                    qdrant =
                        "Connected"
                });
        }


        [HttpGet("embedding-test")]
        public async Task<IActionResult> EmbeddingTest()
        {
            float[] embedding =
                await OllamaEmbeddingService.GenerateEmbeddingAsync(
                    "يجب على السائق التوقف عند الإشارة الحمراء");

            return Ok(
                new
                {
                    status =
                        "Success",

                    dimensions =
                        embedding.Length
                });
        }


        [HttpPost("qdrant-collection")]
        public async Task<IActionResult> CreateQdrantCollection()
        {
            await QdrantConnection
                .CreateKnowledgeCollectionAsync();

            return Ok(
                new
                {
                    status =
                        "Created",

                    collection =
                        QdrantConnection
                            .KnowledgeCollectionName,

                    vectorSize =
                        QdrantConnection
                            .EmbeddingSize
                });
        }


        [HttpPost("qdrant-upsert-test")]
        public async Task<IActionResult> QdrantUpsertTest()
        {
            TextChunk testChunk =
                new TextChunk
                {
                    PageNumber = 1,
                    ChunkIndex = 1,

                    Text =
                        "يجب على السائق التوقف عند الإشارة الحمراء."
                };


            float[] embedding =
                await OllamaEmbeddingService.GenerateEmbeddingAsync(
                    testChunk.Text);


            await QdrantKnowledgeStore.UpsertChunkAsync(
                documentID: 0,
                chunk: testChunk,
                embedding: embedding);


            return Ok(
                new
                {
                    status =
                        "Stored",

                    documentID =
                        0,

                    pageNumber =
                        testChunk.PageNumber,

                    chunkIndex =
                        testChunk.ChunkIndex,

                    dimensions =
                        embedding.Length
                });
        }


        [HttpPost("search")]
        public async Task<IActionResult> SearchKnowledge(
            [FromBody] KnowledgeSearchRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new
                {
                    message = "Question is required."
                });
            }


            // 1. نحول سؤال المستخدم إلى Query Embedding
            float[] queryEmbedding =
                await OllamaEmbeddingService
                    .GenerateQueryEmbeddingAsync(
                        request.Question);


            // 2. Qdrant يرجع أفضل 8 مرشحين
            var searchResults =
                await QdrantKnowledgeStore.SearchAsync(
                    queryEmbedding,
                    8);


            // 3. نستبعد نقطة الاختبار
            // وأي نتيجة لا تحتوي على نص
            var candidates =
                searchResults
                    .Where(result =>
                        result.Payload.ContainsKey(
                            "document_id") &&

                        result.Payload["document_id"]
                            .IntegerValue > 0 &&

                        result.Payload.ContainsKey(
                            "text") &&

                        !string.IsNullOrWhiteSpace(
                            result.Payload["text"]
                                .StringValue))
                    .ToList();


            if (candidates.Count == 0)
            {
                return Ok(new
                {
                    Question =
                        request.Question,

                    Results =
                        new List<object>()
                });
            }


            // 4. نرسل نصوص الـChunks
            // إلى الـCross-Encoder Reranker
            List<string> texts =
                candidates
                    .Select(result =>
                        result.Payload["text"]
                            .StringValue)
                    .ToList();


            // 5. الـReranker يفحص:
            // السؤال + كل Chunk
            // ويرتبهم حسب مدى ارتباطهم الحقيقي
            List<RerankerResult> rerankedResults =
                await LocalRerankerService.RerankAsync(
                    request.Question,
                    texts);


            // 6. نرجع أفضل 5 نتائج فقط
            var response =
                rerankedResults
                    .Where(result =>
                        result.Index >= 0 &&
                        result.Index <
                            candidates.Count)

                    .Take(5)

                    .Select(result =>
                    {
                        var candidate =
                            candidates[
                                result.Index];

                        return new
                        {
                            RerankerScore =
                                result.Score,

                            SemanticScore =
                                candidate.Score,

                            DocumentID =
                                candidate.Payload[
                                    "document_id"]
                                    .IntegerValue,

                            PageNumber =
                                candidate.Payload[
                                    "page_number"]
                                    .IntegerValue,

                            ChunkIndex =
                                candidate.Payload[
                                    "chunk_index"]
                                    .IntegerValue,

                            Text =
                                candidate.Payload[
                                    "text"]
                                    .StringValue
                        };
                    })
                    .ToList();


            return Ok(new
            {
                Question =
                    request.Question,

                Results =
                    response
            });
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
            [FromBody] KnowledgeSearchRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new
                {
                    message = "Question is required."
                });
            }


            float[] queryEmbedding =
                await OllamaEmbeddingService
                    .GenerateQueryEmbeddingAsync(
                        request.Question);


            var searchResults =
                await QdrantKnowledgeStore.SearchAsync(
                    queryEmbedding,
                    8);


            var candidates =
                searchResults
                    .Where(result =>
                        result.Payload.ContainsKey(
                            "document_id") &&

                        result.Payload["document_id"]
                            .IntegerValue > 0 &&

                        result.Payload.ContainsKey(
                            "text") &&

                        !string.IsNullOrWhiteSpace(
                            result.Payload["text"]
                                .StringValue))
                    .ToList();


            if (candidates.Count == 0)
            {
                return Ok(new
                {
                    Question =
                        request.Question,

                    Answer =
                        "لم أجد معلومات كافية في المصادر للإجابة عن هذا السؤال.",

                    Sources =
                        Array.Empty<object>()
                });
            }


            List<string> texts =
                candidates
                    .Select(result =>
                        result.Payload["text"]
                            .StringValue)
                    .ToList();


            List<RerankerResult> rerankedResults =
                await LocalRerankerService.RerankAsync(
                    request.Question,
                    texts);


            var bestResults =
                rerankedResults
                    .Where(result =>
                        result.Index >= 0 &&
                        result.Index < candidates.Count)
                    .Take(5)
                    .Select(result => new
                    {
                        Candidate =
                            candidates[result.Index],

                        RerankerScore =
                            result.Score
                    })
                    .ToList();


            if (bestResults.Count == 0)
            {
                return Ok(new
                {
                    Question =
                        request.Question,

                    Answer =
                        "لم أجد معلومات كافية في المصادر للإجابة عن هذا السؤال.",

                    Sources =
                        Array.Empty<object>()
                });
            }


            List<string> contextChunks =
                bestResults
                    .Select(item =>
                    {
                        long pageNumber =
                            item.Candidate.Payload[
                                "page_number"]
                                .IntegerValue;

                        string chunkText =
                            item.Candidate.Payload[
                                "text"]
                                .StringValue;

                        return
                            $"[الصفحة {pageNumber}]\n{chunkText}";
                    })
                    .ToList();


            string answer =
                await OllamaChatService
                    .GenerateAnswerAsync(
                        request.Question,
                        contextChunks);


            var sources =
                bestResults
                    .Select(item => new
                    {
                        DocumentID =
                            item.Candidate.Payload[
                                "document_id"]
                                .IntegerValue,

                        PageNumber =
                            item.Candidate.Payload[
                                "page_number"]
                                .IntegerValue,

                        ChunkIndex =
                            item.Candidate.Payload[
                                "chunk_index"]
                                .IntegerValue,

                        SemanticScore =
                            item.Candidate.Score,

                        RerankerScore =
                            item.RerankerScore
                    })
                    .ToList();


            return Ok(new
            {
                Question =
                    request.Question,

                Answer =
                    answer,

                Sources =
                    sources
            });
        }


        [HttpPost("{documentID:int}/reprocess")]
        public async Task<IActionResult> ReprocessDocument(
    int documentID)
        {
            if (documentID <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid document ID."
                });
            }


            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        documentID);


            if (string.IsNullOrWhiteSpace(filePath))
            {
                return NotFound(new
                {
                    message = "Document was not found."
                });
            }


            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message = "The PDF file does not exist on disk."
                });
            }


            try
            {
                // استخراج النص من ملف الـ PDF الموجود
                PdfExtractionResult extractionResult =
                    PdfTextExtractor.Extract(
                        filePath);


                // إنشاء Chunks جديدة بالحجم الحالي
                // حالياً 1200 / 200
                List<TextChunk> chunks =
                    TextChunker.CreateChunks(
                        extractionResult);


                if (chunks.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No text chunks were created.");
                }


                // تحديث حالة الوثيقة
                clsKnowledgeDocument
                    .UpdateAfterTextExtraction(
                        documentID,
                        extractionResult.TotalPages);


                // حذف الـ Vectors القديمة لهذه الوثيقة فقط
                await QdrantKnowledgeStore
                    .DeleteDocumentChunksAsync(
                        documentID);


                // إنشاء Embeddings جديدة
                // وحفظ الـ Chunks الجديدة في Qdrant
                await KnowledgeDocumentVectorProcessor
                    .ProcessAsync(
                        documentID,
                        chunks);


                // تحديث SQL بعد النجاح
                clsKnowledgeDocument
                    .MarkProcessingCompleted(
                        documentID,
                        chunks.Count);


                return Ok(new
                {
                    DocumentID =
                        documentID,

                    TotalPages =
                        extractionResult.TotalPages,

                    ChunkCount =
                        chunks.Count,

                    ProcessingStatus =
                        "Ready"
                });
            }
            catch (Exception ex)
            {
                clsKnowledgeDocument
                    .MarkProcessingFailed(
                        documentID,
                        ex.Message);

                throw;
            }
        }
        // =====================================================
        // Diagnostic: عرض النص المستخرج من صفحة محددة
        // لا يحفظ ولا يعدل أي بيانات
        // =====================================================

        [HttpGet("{documentID:int}/page-text/{pageNumber:int}")]
        public IActionResult GetExtractedPageText(
            int documentID,
            int pageNumber)
        {
            if (documentID <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid document ID."
                });
            }


            if (pageNumber <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid page number."
                });
            }


            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        documentID);


            if (string.IsNullOrWhiteSpace(
                    filePath))
            {
                return NotFound(new
                {
                    message = "Document was not found."
                });
            }


            if (!System.IO.File.Exists(
                    filePath))
            {
                return NotFound(new
                {
                    message =
                        "The PDF file does not exist on disk."
                });
            }


            PdfExtractionResult extractionResult =
                PdfTextExtractor.Extract(
                    filePath);


            if (pageNumber >
                extractionResult.TotalPages)
            {
                return BadRequest(new
                {
                    message =
                        $"Page number must be between 1 and " +
                        $"{extractionResult.TotalPages}."
                });
            }


            PdfPageText page =
                extractionResult.Pages
                    .FirstOrDefault(
                        item =>
                            item.PageNumber ==
                            pageNumber);


            if (page == null)
            {
                return NotFound(new
                {
                    message =
                        "Page text was not found."
                });
            }


            return Ok(new
            {
                DocumentID =
                    documentID,

                PageNumber =
                    page.PageNumber,

                TotalPages =
                    extractionResult.TotalPages,

                TextLength =
                    page.Text?.Length ?? 0,

                Text =
                    page.Text
            });
        }
        // =====================================================
        // Diagnostic: فحص ترتيب كلمات PDF ومواقعها
        // لا يحفظ ولا يعدل أي بيانات
        // =====================================================

        [HttpGet("{documentID:int}/layout-diagnostic/{pageNumber:int}")]
        public IActionResult GetLayoutDiagnostic(
            int documentID,
            int pageNumber)
        {
            if (documentID <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid document ID."
                });
            }


            if (pageNumber <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid page number."
                });
            }


            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        documentID);


            if (string.IsNullOrWhiteSpace(
                    filePath))
            {
                return NotFound(new
                {
                    message = "Document was not found."
                });
            }


            if (!System.IO.File.Exists(
                    filePath))
            {
                return NotFound(new
                {
                    message =
                        "The PDF file does not exist on disk."
                });
            }


            PdfLayoutDiagnosticResult result =
                PdfLayoutDiagnostic.AnalyzePage(
                    filePath,
                    pageNumber,
                    maximumWords: 250);


            return Ok(result);
        }
        // =====================================================
        // Diagnostic: اختبار إعادة بناء RTL بدون تعديل النظام
        // =====================================================

        [HttpGet("{documentID:int}/rtl-reconstructed/{pageNumber:int}")]
        public IActionResult GetRtlReconstructedText(
            int documentID,
            int pageNumber)
        {
            if (documentID <= 0 ||
                pageNumber <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid document or page number."
                });
            }


            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        documentID);


            if (string.IsNullOrWhiteSpace(filePath) ||
                !System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message = "Document was not found."
                });
            }


            using var document =
                UglyToad.PdfPig.PdfDocument.Open(
                    filePath);


            if (pageNumber >
                document.NumberOfPages)
            {
                return BadRequest(new
                {
                    message =
                        $"Page number must be between 1 and {document.NumberOfPages}."
                });
            }


            var page =
                document.GetPage(
                    pageNumber);


            string reconstructedText =
                PdfRtlTextReconstructor
                    .Reconstruct(
                        page);


            return Ok(new
            {
                DocumentID =
                    documentID,

                PageNumber =
                    pageNumber,

                TextLength =
                    reconstructedText.Length,

                Text =
                    reconstructedText
            });
        }
        // =====================================================
        // Diagnostic: مقارنة جودة الاستخراج العادي مع RTL
        // لا يحفظ ولا يعدل أي بيانات
        // =====================================================

        [HttpGet("{documentID:int}/compare-extraction/{pageNumber:int}")]
        public IActionResult CompareExtraction(
            int documentID,
            int pageNumber)
        {
            if (documentID <= 0 ||
                pageNumber <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid document or page number."
                });
            }


            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        documentID);


            if (string.IsNullOrWhiteSpace(filePath) ||
                !System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message = "Document was not found."
                });
            }


            // =============================================
            // الاستخراج الحالي
            // =============================================

            PdfExtractionResult normalExtraction =
                PdfTextExtractor.Extract(
                    filePath);


            if (pageNumber >
                normalExtraction.TotalPages)
            {
                return BadRequest(new
                {
                    message =
                        $"Page number must be between 1 and " +
                        $"{normalExtraction.TotalPages}."
                });
            }


            PdfPageText normalPage =
                normalExtraction.Pages
                    .FirstOrDefault(
                        page =>
                            page.PageNumber ==
                            pageNumber);


            string normalText =
                normalPage?.Text ??
                string.Empty;


            // =============================================
            // إعادة بناء RTL
            // =============================================

            using var document =
                UglyToad.PdfPig.PdfDocument.Open(
                    filePath);


            var pdfPage =
                document.GetPage(
                    pageNumber);


            string rtlText =
                PdfRtlTextReconstructor
                    .Reconstruct(
                        pdfPage);


            // =============================================
            // تقييم الطريقتين
            // =============================================

            PdfTextQualityResult normalQuality =
                PdfTextQualityEvaluator
                    .Evaluate(
                        normalText);


            PdfTextQualityResult rtlQuality =
                PdfTextQualityEvaluator
                    .Evaluate(
                        rtlText);


            string recommendedMethod =
                rtlQuality.Score >
                normalQuality.Score
                    ? "RTL"
                    : "Normal";


            return Ok(new
            {
                DocumentID =
                    documentID,

                PageNumber =
                    pageNumber,

                RecommendedMethod =
                    recommendedMethod,

                Normal = new
                {
                    Quality =
                        normalQuality,

                    TextLength =
                        normalText.Length,

                    Text =
                        normalText
                },

                RTL = new
                {
                    Quality =
                        rtlQuality,

                    TextLength =
                        rtlText.Length,

                    Text =
                        rtlText
                }
            });
        }
    }

}