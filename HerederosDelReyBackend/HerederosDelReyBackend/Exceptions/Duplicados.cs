using Microsoft.AspNetCore.Mvc;

namespace HerederosDelReyBackend.Exceptions
{
    // Ejecuta una operación del servicio y, si hay un dato duplicado,
    // devuelve 409 Conflict { mensaje } desde el propio controlador.
    // Atraparlo aquí (y no solo en el middleware) evita que Visual Studio
    // se detenga en la excepción mientras depuras.
    public static class Duplicados
    {
        public static async Task<(T Resultado, IActionResult? Conflicto)> IntentarAsync<T>(Func<Task<T>> operacion)
        {
            try
            {
                return (await operacion(), null);
            }
            catch (DatoDuplicadoException ex)
            {
                return (default!, new ConflictObjectResult(new { mensaje = ex.Message }));
            }
        }
    }
}
