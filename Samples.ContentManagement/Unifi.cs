﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ContentManagement.Entities;
using Parameter = ContentManagement.Entities.Parameter;

namespace ContentManagement {
    class Unifi {
        /// <summary>
        /// Get a list of libraries within the UNIFI Content Management System
        /// </summary>
        /// <param name="apiKey">UNIFI API Key</param>
        /// <returns></returns>
        public static List<Library> GetLibraries() {
            // Instantiate a list of Library objects to deserialize the JSON response
            var _libraries = new List<Library>();

            var client = new RestClient("https://api.unifilabs.com/libraries");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization",  Secrets.ApiKey);
            var response = client.Execute(request);

            // Deserialize JSON response to a List of Library objects
            if (response.Content.Contains("fail") == false)
            {
                _libraries = JsonConvert.DeserializeObject<List<Library>>(response.Content);
            }
            else
            {
                MessageBox.Show("Error parsing Unifi libraries:\n\n" + "Message from server Unifi server: " + response.Content);
                System.Windows.Application.Current.Shutdown();

            }


            return _libraries;
        }

        /// <summary>
        /// Retrieves a list of content from a specific library.
        /// </summary>
        /// <param name="apiKey">UNIFI API Key.</param>
        /// <param name="libraryId">The library ID to search.</param>
        /// <returns></returns>
        public static List<Content> GetContentFromLibrary(Guid libraryId) {
            var totalContent = new List<Content>();
            int pageSize = 20;
            int pageNumber = 0;
            int pageOffset = 0;
            bool nextPage = true;

            do
            {
                var pageContent = new List<Content>();

                try
                {
                    var client = new RestClient("https://api.unifilabs.com/search");
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("Authorization", Secrets.ApiKey);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("undefined", JsonConvert.SerializeObject(new
                    {
                        terms = "*",
                        libraries = new[] { libraryId },
                        @return = "with-parameters",
                        fileTypes = new int[] { 1 },     // Only return Revit families
                        size = pageSize,
                        offset = pageOffset,
                    }), ParameterType.RequestBody);
                    var response = client.Execute(request);

                    // Deserialize JSON response to a List of Content objects
                    try { pageContent = JsonConvert.DeserializeObject<List<Content>>(response.Content); }
                    catch (Exception ex) { MessageBox.Show(response.Content, "Exception: " + ex.ToString()); }

                    // Increment page number
                    pageNumber += 1;
                    pageOffset = pageNumber * pageSize;

                    Debug.WriteLine(String.Format(
                        "Found page {0} containing {1} items.",
                        pageNumber,
                        pageContent.Count().ToString()
                        ));

                    // Add the content from this page to the main list
                    totalContent.AddRange(pageContent);

                    foreach (var c in pageContent)
                    {
                        Debug.WriteLine("+ " + c.Title);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

                // Control pagination
                if (pageContent.Count() != pageSize)
                {
                    nextPage = false;
                }

                Debug.WriteLine("Total items: " + totalContent.Count().ToString());
            }
            while (nextPage == true);

            return totalContent;
        }

        /// <summary>
        /// Get a specific Content by its name.
        /// </summary>
        /// <param name="apiKey">UNIFI API Key.</param>
        /// <param name="name">The name of the Content to search for.</param>
        /// <returns></returns>
        public static Content GetContentByName(string name) {
            var content = new List<Content>();

            var client = new RestClient("https://api.unifilabs.com/search");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", Secrets.ApiKey);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", JsonConvert.SerializeObject(new { terms = name, @return = "with-parameters" }), ParameterType.RequestBody);
            var response = client.Execute(request);

            // Deserialize JSON response to a Content object
            try { content = JsonConvert.DeserializeObject<List<Content>>(response.Content); }
            catch (Exception ex) {
                // Display debug info if exception is caught
                MessageBox.Show(ex.ToString(), "Exception");
                MessageBox.Show(response.Content, "Exception");
            }

            // TODO: Ensure that only one content is returned rather than hardcoding the first item in list
            if (content.Count > 1) { MessageBox.Show("Warning, more than one piece of content matches this query.", "Warning"); }

            return content[0];
        }

        /// <summary>
        /// Get a specific Content by its FileRevisionId
        /// </summary>
        /// <param name="apiKey">UNIFI API Key.</param>
        /// <param name="revisionId">The FileRevisionId property of the content.</param>
        /// <param name="libraryId">The ID of a library the content belongs to.</param>
        /// <returns></returns>
        public static Content GetContentByRevisionId(Guid revisionId, Guid libraryId)
        {
            var content = new List<Content>();

            var client = new RestClient("https://api.unifilabs.com/search");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", Secrets.ApiKey);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", JsonConvert.SerializeObject(
                new {
                        terms = "*",
                        parameters = new [] { new { FileRevisionId = revisionId }
                    },
                    libraries = new [] { libraryId },
                    @return = "with-parameters"
                }
                ), 
            ParameterType.RequestBody
            );
            var response = client.Execute(request);

            // Deserialize JSON response to a Content object
            try
            {
                content = JsonConvert.DeserializeObject<List<Content>>(response.Content);
            }
            catch (Exception ex)
            {
                // Display debug info if exception is caught
                MessageBox.Show(ex.ToString(), "Exception");
                MessageBox.Show(response.Content, "Exception");
            }

            // TODO: Ensure that only one content is returned rather than hardcoding the first item in list
            if (content.Count > 1) { MessageBox.Show("Warning, more than one piece of content matches this query.", "Warning"); }

            return content[0];
        }


        /// <summary>
        /// Get Revit Family Types from a Content object.
        /// </summary>
        /// <param name="content">The UNIFI Content object to get parameters from.</param>
        /// <returns></returns>
        public static List<string> GetFamilyTypes(Content content) {
            var familyTypes = new List<string>();

            // Loop through all parameters
            foreach (var p in content.Parameters) {
                // Pass parameter values to Content object
                if (p.TypeName != "") { familyTypes.Add(p.TypeName); }
            }

            // Pass the list of unique list items as Family Type names array
            var uniqueItems = new HashSet<string>(familyTypes);
            familyTypes = uniqueItems.ToList();

            return familyTypes;
        }

        /// <summary>
        /// Set the Revit Family Type Parameter value.
        /// </summary>
        /// <param name="apiKey">UNIFI API Key</param>
        /// <param name="content">The content to modify</param>
        /// <param name="typeName">The Revit Family Type name</param>
        /// <param name="parameterName">The name of the Type Parameter to modify</param>
        /// <param name="parameterValue">The new value of the Type Parameter</param>
        /// <param name="dataType">The data type for the Type Parameter</param>
        /// <param name="revitYear">The Revit year of the Family to modify</param>
        /// <returns></returns>
        public static Batch SetTypeParameterValue(
            Content content,
            string typeName,
            string parameterName,
            string parameterValue,
            string dataType,
            int revitYear
        ) {
            var client = new RestClient("https://api.unifilabs.com/batch");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", Secrets.ApiKey);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", JsonConvert.SerializeObject(new {
                Requests = new[] {
                    new {
                        ObjectId = content.RepositoryFileId,
                        Operation = "SetTypeValues",
                        ExistingName = parameterName,
                        Type = dataType,
                        RevitYear = revitYear,
                        Values = new[] { new { TypeName = ToLiteral(typeName), Value = parameterValue } }
                    }
                }
            }), ParameterType.RequestBody);

            var response = client.Execute(request);

            // Deserialize reponse as Batch object
            var batch = JsonConvert.DeserializeObject<Batch>(response.Content);

            return batch;
        }

        /// <summary>
        /// Get the status of a Batch from its ID
        /// </summary>
        /// <param name="apiKey">UNIFI API Key</param>
        /// <param name="batchId">The ID of the Batch</param>
        /// <returns></returns>
        public static async Task<BatchStatus> GetBatchStatus(string batchId) {
            var client = new RestClient("https://api.unifilabs.com/batch/" + batchId);
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", Secrets.ApiKey);
            request.AddHeader("Content-Type", "application/json");
            var response = await client.ExecuteAsync<BatchStatus>(request);

            return response.Data;
        }

        /// <summary>
        /// Convert a string to a string literal. Useful for passing Type Names and other values that have quotation marks.
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <returns></returns>
        private static string ToLiteral(string input) {
            using (var writer = new StringWriter()) {
                using (var provider = CodeDomProvider.CreateProvider("CSharp")) {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
}