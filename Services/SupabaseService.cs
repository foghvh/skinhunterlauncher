﻿using Supabase;
using System.Threading.Tasks;
using SkinHunterLauncher.Models;
using System.Collections.Generic;
using System.Linq;
using Client = Supabase.Client;
using SupabaseUser = Supabase.Gotrue.User;
using Supabase.Gotrue;
using BCryptNet = BCrypt.Net;
using Postgrest.Responses;
using System.Text.Json;
using System.Net.Http; // AÑADIDO
using System; // AÑADIDO

namespace SkinHunterLauncher.Services
{
    public class SupabaseService
    {
        private readonly Client _supabase;
        private readonly HttpClient _httpClient; // AÑADIDO

        private const string SupabaseUrl = "https://odlqwkgewzxxmbsqutja.supabase.co";
        private const string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im9kbHF3a2dld3p4eG1ic3F1dGphIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzQyMTM2NzcsImV4cCI6MjA0OTc4OTY3N30.qka6a71bavDeUQgy_BKoVavaClRQa_gT36Au7oO9AF0";

        public SupabaseService()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            _supabase = new Client(SupabaseUrl, SupabaseAnonKey, options);
            _httpClient = new HttpClient(); // AÑADIDO
        }

        public async Task InitializeAsync()
        {
            await _supabase.InitializeAsync();
        }

        public Client GetClient()
        {
            return _supabase;
        }

        public async Task<Models.User?> GetUserByLogin(string login)
        {
            var response = await _supabase.From<Models.User>().Filter("login", Postgrest.Constants.Operator.Equals, login).Get();

            if (response?.ResponseMessage != null && !response.ResponseMessage.IsSuccessStatusCode)
            {
                string errorDetail = response.Content ?? response.ResponseMessage.ReasonPhrase ?? "Unknown error fetching user by login.";
                System.Diagnostics.Debug.WriteLine($"Error fetching user by login {login}: {errorDetail}");
                return null;
            }
            return response?.Models.FirstOrDefault();
        }
        public async Task<Models.User?> GetUserByEmail(string email)
        {
            var response = await _supabase.From<Models.User>().Filter("email", Postgrest.Constants.Operator.Equals, email).Get();

            if (response?.ResponseMessage != null && !response.ResponseMessage.IsSuccessStatusCode)
            {
                string errorDetail = response.Content ?? response.ResponseMessage.ReasonPhrase ?? "Unknown error fetching user by email.";
                System.Diagnostics.Debug.WriteLine($"Error fetching user by email {email}: {errorDetail}");
                return null;
            }
            return response?.Models.FirstOrDefault();
        }

        public async Task<Session?> SignUpUserWithPassword(string email, string password, string username)
        {
            var existingUserByEmail = await GetUserByEmail(email);
            if (existingUserByEmail != null)
            {
                throw new System.Exception("User with this email already exists.");
            }

            var existingUserByLogin = await GetUserByLogin(username);
            if (existingUserByLogin != null)
            {
                throw new System.Exception("User with this login already exists.");
            }

            string hashedPassword = BCryptNet.BCrypt.HashPassword(password);
            var newUser = new Models.User
            {
                Email = email,
                PasswordHash = hashedPassword,
                Login = username,
                Credits = 0,
                IsBuyer = false
            };

            var insertResponse = await _supabase.From<Models.User>().Insert(newUser);

            if (insertResponse?.ResponseMessage != null && insertResponse.ResponseMessage.IsSuccessStatusCode && insertResponse.Models.Any())
            {
                return null;
            }
            else
            {
                string errorMessage = "Failed to register user in custom table.";

                if (insertResponse?.ResponseMessage != null && !insertResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrEmpty(insertResponse.Content))
                    {
                        try
                        {
                            var errorDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(insertResponse.Content);
                            if (errorDetails != null && errorDetails.TryGetValue("message", out var msg))
                            {
                                errorMessage = msg;
                            }
                            else
                            {
                                errorMessage = insertResponse.Content;
                            }
                        }
                        catch
                        {
                            errorMessage = insertResponse.Content;
                        }
                    }
                    else if (!string.IsNullOrEmpty(insertResponse.ResponseMessage.ReasonPhrase))
                    {
                        errorMessage = insertResponse.ResponseMessage.ReasonPhrase;
                    }
                }
                else if (insertResponse == null || insertResponse.ResponseMessage == null)
                {
                    errorMessage = "Failed to register user: No valid response from server.";
                }

                System.Diagnostics.Debug.WriteLine($"Supabase Insert Error: {errorMessage}");
                throw new System.Exception(errorMessage);
            }
        }

        public async Task<byte[]?> DownloadFileBytesAsync(string bucketName, string filePathInBucket)
        {
            try
            {
                string storageUrlPart = "/storage/v1";
                // Evitar duplicar /storage/v1 si ya está en SupabaseUrl
                string baseUrl = SupabaseUrl.EndsWith(storageUrlPart) ? SupabaseUrl : SupabaseUrl + storageUrlPart;

                string publicUrl = $"{baseUrl}/object/public/{bucketName}/{filePathInBucket}";

                publicUrl = publicUrl.Replace("//object", "/object"); // Limpieza extra por si acaso

                System.Diagnostics.Debug.WriteLine($"Attempting to download from Storage: {publicUrl}");

                HttpResponseMessage response = await _httpClient.GetAsync(publicUrl);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error downloading file from Supabase Storage. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Content: {errorContent} (URL: {publicUrl})");
                    return null;
                }
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HttpRequestException downloading file from Supabase Storage: {ex.Message} (URL: {bucketName}/{filePathInBucket})");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error downloading file {bucketName}/{filePathInBucket}: {ex.Message}");
                return null;
            }
        }
    }
}