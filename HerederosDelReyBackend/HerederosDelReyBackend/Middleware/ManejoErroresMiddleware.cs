using HerederosDelReyBackend.Exceptions;

namespace HerederosDelReyBackend.Middleware
{
    /// <summary>
    /// Convierte los datos duplicados en una respuesta 409 Conflict con
    /// el formato { "mensaje": "..." } que ya muestran los formularios.
    /// </summary>
    public class ManejoErroresMiddleware
    {
        private readonly RequestDelegate _next;

        public ManejoErroresMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DatoDuplicadoException ex) when (!context.Response.HasStarted)
            {
                // No se usa Response.Clear() para conservar los encabezados CORS.
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new { mensaje = ex.Message });
            }
        }
    }
}
