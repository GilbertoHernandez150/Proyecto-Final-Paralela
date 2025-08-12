using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FinalParalela.Services
{
    // Servicio para procesar y clasificar logs (registros) de un archivo CSV
    public class LogsService
    {
        // Representa una entrada individual del log con fecha, tipo y mensaje
        public class LogEntry
        {
            public DateTime Fecha { get; set; }    // Fecha y hora del log
            public string Tipo { get; set; }       // Tipo de log (ERROR, WARNING, INFO)
            public string Mensaje { get; set; }    // Mensaje descriptivo del log
        }

        // Contenedor para agrupar las entradas de log por su tipo
        public class LogsResult
        {
            public List<LogEntry> Errors { get; set; }    // Lista de logs de tipo error
            public List<LogEntry> Warnings { get; set; }  // Lista de logs de tipo advertencia
            public List<LogEntry> Infos { get; set; }     // Lista de logs de tipo información
        }

        // Método estático que lee un archivo CSV de logs y los clasifica en Errors, Warnings e Infos
        public static LogsResult SanitizeLogs(string path)
        {
            string filePath = path;

            // Verifica si el archivo existe; si no, retorna un resultado vacío con listas vacías
            if (!System.IO.File.Exists(filePath))
                return new LogsResult { Errors = new(), Warnings = new(), Infos = new() };

            var logs = new List<LogEntry>();

            // Abre el archivo para lectura
            using (var reader = new StreamReader(filePath))
            {
                // Salta la primera línea que se asume es el encabezado del CSV
                reader.ReadLine();

                // Lee línea por línea hasta el final del archivo
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(',');  // Divide la línea por comas

                    // Solo procesa si la línea tiene al menos 3 partes (Fecha, Tipo, Mensaje)
                    if (parts.Length >= 3)
                    {
                        // Intenta convertir la primera parte a DateTime con formato específico
                        if (!DateTime.TryParseExact(
                                parts[0].Trim('"'),
                                "M/d/yyyy h:mm:ss tt",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var fecha))
                        {
                            // Si la conversión falla, asigna fecha mínima (por defecto)
                            fecha = DateTime.MinValue;
                        }

                        // Crea una nueva entrada de log y la añade a la lista
                        logs.Add(new LogEntry
                        {
                            Fecha = fecha,
                            Tipo = parts[1].Trim('"').ToUpper(),  // Convierte el tipo a mayúsculas para estandarizar
                            Mensaje = parts[2].Trim('"')
                        });
                    }
                }
            }

            // Clasifica las entradas según su tipo y las ordena por fecha descendente (más recientes primero)
            return new LogsResult
            {
                Errors = logs.Where(l => l.Tipo.Contains("ERROR"))
                             .OrderByDescending(l => l.Fecha)
                             .ToList(),

                Warnings = logs.Where(l => l.Tipo.Contains("WARNING"))
                               .OrderByDescending(l => l.Fecha)
                               .ToList(),

                Infos = logs.Where(l => l.Tipo.Contains("INFO"))
                            .OrderByDescending(l => l.Fecha)
                            .ToList()
            };
        }
    }
}
