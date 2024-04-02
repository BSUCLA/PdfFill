using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iText.Kernel.Pdf;
using iText.Forms;

namespace PdfFill
{
    public class PdfFill
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public PdfFill(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _logger = loggerFactory.CreateLogger<PdfFill>();
            _httpClient = httpClientFactory.CreateClient();
        }

        [Function("PdfFill")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("PdfFill function processed a request.");

            var response = req.CreateResponse();

            // Receive request information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if(string.IsNullOrEmpty(requestBody)){
                _logger.LogError("Request body empty.");
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.WriteString("Request body empty.");    
                return response;
            }

            dynamic requestJson = JsonConvert.DeserializeObject(requestBody)!;

            if (!requestJson.ContainsKey("templateUrl"))
            {
                _logger.LogError("Request did not contain URL for blank pdf.");
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.WriteString("Request did not contain url for blank pdf.");
                return response;
            }

            if (!requestJson.ContainsKey("pdfFormData"))
            {
                _logger.LogError("Request did not contain PDF form data.");
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.WriteString("Request did not contain PDF form data.");
                return response;
            }

            string templateUrl = requestJson.templateUrl;
            JObject pdfFormData = requestJson.pdfFormData;

            // Download blank PDF and store in memory
            byte[] templateBytes;

            try
            {
                templateBytes = await _httpClient.GetByteArrayAsync(templateUrl);
            }
            catch (Exception e)
            {
                _logger.LogError("Could not download blank pdf: {Message}", e.Message);
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.WriteString($"Could not download blank PDF: {e.Message}");
                return response;
            }

            // Fill PDF with information from formData
            using MemoryStream completedStream = new MemoryStream();

            using (PdfWriter completedWriter = new PdfWriter(completedStream))
            using (PdfReader templateReader = new PdfReader(new MemoryStream(templateBytes)))
            using (PdfDocument completedPdf = new PdfDocument(templateReader, completedWriter))
            {
                completedWriter.SetCloseStream(false);
                PdfAcroForm form = PdfAcroForm.GetAcroForm(completedPdf, true);

                foreach (var field in pdfFormData)
                {
                    form.GetField(field.Key)?.SetValue(field.Value?.ToString());
                }
            }

            completedStream.Position = 0;

            // Return the filled PDF as a response
            response.StatusCode = System.Net.HttpStatusCode.OK;
            response.Headers.Add("Content-Type", "application/pdf");
            response.Headers.Add("Content-Disposition", "attachment; filename=filledPdf.pdf");
            await completedStream.CopyToAsync(response.Body);

            return response;
        }
    }
}