using Microsoft.AspNetCore.Mvc;
using System.Globalization;
 
namespace FinalParalela.Services
{
    public class LogsService
    {
        public class LogEntry
        {
            public DateTime Fecha { get; set; }
            public string Tipo { get; set; }
            public string Mensaje { get; set; }
        }
 
        public class LogsResult
        {
            public List<LogEntry> Errors { get; set; }
            public List<LogEntry> Warnings { get; set; }
            public List<LogEntry> Infos { get; set; }
        }
 
        public static LogsResult SanitizeLogs(string path)
        {
            string filePath = path;
 
            if (!System.IO.File.Exists(filePath))
                return new LogsResult { Errors = new(), Warnings = new(), Infos = new() };
 
            var logs = new List<LogEntry>();
 
            using (var reader = new StreamReader(filePath))
            {
                // Saltar encabezado
                reader.ReadLine();
 
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(',');
 
                    if (parts.Length >= 3)
                    {
                        if (!DateTime.TryParseExact(
                                parts[0].Trim('"'),
                                "M/d/yyyy h:mm:ss tt",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var fecha))
                        {
                            fecha = DateTime.MinValue;
                        }
 
                        logs.Add(new LogEntry
                        {
                            Fecha = fecha,
                            Tipo = parts[1].Trim('"').ToUpper(),
                            Mensaje = parts[2].Trim('"')
                        });
                    }
                }
            }
 
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