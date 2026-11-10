namespace HerederosDelReyBackend.Exceptions
{
    /// <summary>
    /// Se lanza cuando se intenta crear o editar un registro con un dato
    /// que debe ser único y ya existe (nombre, código, NIT, correo...).
    /// El middleware de errores la convierte en una respuesta 409 Conflict.
    /// </summary>
    public class DatoDuplicadoException : Exception
    {
        public DatoDuplicadoException(string mensaje) : base(mensaje)
        {
        }
    }
}
