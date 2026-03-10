using desktop_app.Models;
using desktop_app.ViewModels;
using desktop_app.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace desktop_app.Services
{
    /// <summary>
    /// Servicio para gestionar operaciones CRUD de usuarios contra la API.
    /// </summary>
    /// <remarks>
    /// Encapsula llamadas HTTP para listar, crear, actualizar y eliminar usuarios, además de helpers para refrescar colecciones en la UI (WPF).
    /// </remarks>
    public class UsersService
    {
        /// <summary>
        /// Contexto de navegación para utilidades de usuario (crear/editar), compartiendo modo y usuario seleccionado entre vistas.
        /// </summary>
        public static class UserNavigationContext
        {
            public static UserUtilitiesMode Mode { get; set; }
            public static UserModel? User { get; set; }
        }

        /// <summary>
        /// Opciones JSON usadas para serialización/deserialización.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Crea un contenido JSON para enviar en el cuerpo de una request.
        /// </summary>
        private static StringContent JsonBody(object payload) => new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        /// <summary>
        /// Lee la respuesta HTTP y la deserializa a un tipo JSON.
        /// </summary>
        private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage resp, CancellationToken ct)
        {
            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        /// <summary>
        /// Obtiene todos los usuarios desde la API.
        /// </summary>
        public async Task<IReadOnlyList<UserModel>> GetAllAsync(CancellationToken ct = default)
        {
            using var resp = await ApiService._httpClient.GetAsync("user", ct);
            resp.EnsureSuccessStatusCode();

            var data = await ReadJsonAsync<List<UserModel>>(resp, ct);
            return data ?? new List<UserModel>();
        }

        /// <summary>
        /// Elimina un usuario por su identificador.
        /// </summary>
        public async Task DeleteByIdAsync(string id, CancellationToken ct = default)
        {
            using var resp = await ApiService._httpClient.DeleteAsync($"user/delete/{id}", ct);
            resp.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Hace uso de la funcion anteriror y tambien elimina al usuario de la Lsita visual
        /// </summary>
        public async Task DeleteAndRemoveAsync(
            UserModel user,
            ObservableCollection<UserModel> target,
            ICollectionView? viewToRefresh = null,
            CancellationToken ct = default)
        {
            await DeleteByIdAsync(user.Id, ct);

            target.Remove(user);
            viewToRefresh?.Refresh();
        }

        /// <summary>
        /// Crea un usuario en la API.
        /// </summary>
        public async Task<UserModel?> CreateAsync(UserModel user, CancellationToken ct = default)
        {
            var payload = new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                dni = user.Dni,
                phoneNumber = user.PhoneNumber,
                cityName = user.CityName,
                imageRoute = user.ImageRoute,
                rol = user.Rol,
                vipStatus = user.VipStatus,
                birthDate = user.BirthDate,
                gender = user.Gender
            };

            using var resp = await ApiService._httpClient.PostAsync("user/registerEsc", JsonBody(payload), ct);
            if (!resp.IsSuccessStatusCode)
            {
                var apiError = await TryReadApiErrorAsync(resp, ct);
                throw new Exception(apiError ?? $"Error.");
            }
            return user;
        }

        /// <summary>
        /// Actualiza un usuario existente en la API.
        /// </summary>
        public async Task<UserModel?> UpdateAsync(UserModel user, CancellationToken ct = default)
        {
            var payload = new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                dni = user.Dni,
                phoneNumber = user.PhoneNumber,
                cityName = user.CityName,
                imageRoute = user.ImageRoute,
                rol = user.Rol,
                vipStatus = user.VipStatus,
                birthDate = user.BirthDate,
                gender = user.Gender
            };

            using var resp = await ApiService._httpClient.PutAsync("user/update", JsonBody(payload), ct);

            if (!resp.IsSuccessStatusCode)
            {
                var apiError = await TryReadApiErrorAsync(resp, ct);
                throw new Exception(apiError ?? "Error.");
            }

            return user;
        }

        /// <summary>
        /// Recarga los usuarios desde la API en una colección observable y refresca la vista si se indica.
        /// </summary>
        public async Task ReloadIntoAsync(
            ObservableCollection<UserModel> target,
            ICollectionView? viewToRefresh = null,
            CancellationToken ct = default)
        {
            var list = await GetAllAsync(ct);

            target.Clear();
            foreach (var u in list)
                target.Add(u);

            viewToRefresh?.Refresh();
        }

        private static async Task<string?> TryReadApiErrorAsync(HttpResponseMessage resp, CancellationToken ct)
        {
            try
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(body)) return null;

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String)
                        return e.GetString();
                }

                return body;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Guarda un usuario según el modo (crear o editar).
        /// </summary>
        public async Task<UserModel?> SaveAsync(UserUtilitiesMode mode, UserModel user, CancellationToken ct = default)
        {
            return mode switch
            {
                UserUtilitiesMode.Create => await CreateAsync(user, ct),
                UserUtilitiesMode.Edit => await UpdateAsync(user, ct),
                _ => null
            };
        }
    }
}
