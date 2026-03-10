namespace desktop_app.Services
{
    /// <summary>
    /// Almacén centralizado para el token de acceso de la sesión actual.
    /// </summary>
    /// <remarks>
    /// Esta clase mantiene en memoria el token JWT obtenido tras un inicio de sesión exitoso.
    /// </remarks>
    public static class TokenStore
    {
        public static string? AccessToken { get; set; }
    }
}