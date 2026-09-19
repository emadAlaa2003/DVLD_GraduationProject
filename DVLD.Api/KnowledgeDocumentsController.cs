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


            float[] queryEmbedding =
                await OllamaEmbeddingService
                    .GenerateEmbeddingAsync(
                        request.Question);


            var results =
                await QdrantKnowledgeStore.SearchAsync(
                    queryEmbedding,
                    5);


            var response =
                results.Select(result => new
                {
                    Score =
                        result.Score,

                    DocumentID =
                        result.Payload["document_id"]
                            .IntegerValue,

                    PageNumber =
                        result.Payload["page_number"]
                            .IntegerValue,

                    ChunkIndex =
                        result.Payload["chunk_index"]
                            .IntegerValue,

                    Text =
                        result.Payload["text"]
                            .StringValue
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
    }
}