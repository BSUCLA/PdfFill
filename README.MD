# PDF Fill Azure Function

A simple program written in C# that dynamically fills a blank PDF form based on provided data and returns the completed PDF form as a downloadable file. Developed specifically for Azure Functions using the isolated process model.

## Features
* Receives HTTP POST requests containing JSON data with a URL to a publicly accessible download link of a blank PDF form and a mapping of form field names and data.
* Downloads the blank PDF to memory.
* Uses the form field mapping to fills the corresponding fields with data from the request JSON
* Returns the filled PDF as an HTTP response with the appropriate content type and the filename "filledPdf.pdf"

## Technologies Used
* [.NET Core](https://dotnet.microsoft.com/en-us/download) (8.0.3): C# runtime environment
* [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp) (4.0.5611): Tools for creating, testing, and publishing Azure Functions projects
* [Json.NET](https://www.newtonsoft.com/json): Library for parsing JSON data from requests.
* [iText Community](https://itextpdf.com/products/itext-community): Library for working with PDF files including reading, filling, and creating PDF documents.

## Usage

1. Send an HTTP POST request to the function endpoint including a JSON body with:
    * `{'templateUrl'}`: The direct download link to the blank PDF form template.
    * `{'pdfFormData'}`: A nested object of the form fields and the corresponding data.
    
    Example JSON Payload:
    ```JSON
    {
        "templateUrl": "https://example.com/blank-template.pdf",
        "pdfFormData": {
            "field 1": "data for field 1",
            "field 2": "other data for field 2"
        }
    }
    ```

2. Receive the filled PDF form as a an HTTP response.