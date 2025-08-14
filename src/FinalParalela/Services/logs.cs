using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;

namespace FinalParalela.Services
{
    // Servicio para procesar, clasificar y analizar logs aplicando paralelismo
    public class LogsService
    {
        // Representa una entrada individual del log con fecha, tipo y mensaje
        public class LogEntry
        {
            public DateTime Fecha { get; set; }   // Fecha y hora del log
            public string Tipo { get; set; }      // Tipo de log (ERROR, WARNING, INFO)
            public string Mensaje { get; set; }   // Mensaje descriptivo del log
        }

        // Contenedor para agrupar las entradas de log por su tipo
        public class LogsResult
        {
            public List<LogEntry> Errors { get; set; }    // Lista de logs de tipo error
            public List<LogEntry> Warnings { get; set; }  // Lista de logs de tipo advertencia
            public List<LogEntry> Infos { get; set; }     // Lista de logs de tipo información
        }

        // Metodo estatico que lee un archivo CSV de logs y los clasifica
        public static LogsResult SanitizeLogs(string path)
        {
            string filePath = path;

            // Verifica si el archivo existe; si no, retorna un resultado vacio
            if (!System.IO.File.Exists(filePath))
                return new LogsResult { Errors = new(), Warnings = new(), Infos = new() };

            var logs = new List<LogEntry>();

            // Abre el archivo para lectura
            using (var reader = new StreamReader(filePath))
            {
                // Salta la primera linea
                reader.ReadLine();

                // Procesa linea por linea
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

            // Clasifica y ordena por fecha descendente
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

        // Clase para almacenar el resultado del analisis paralelo
        public class ParallelAnalysisResult
        {
            public int TotalErrors { get; set; }
            public int TotalWarnings { get; set; }
            public int TotalInfos { get; set; }
            public double Speedup { get; set; }
            public double Eficiencia { get; set; }
            public long TiempoSecuencialMs { get; set; }
            public long TiempoParaleloMs { get; set; }
        }

        // Metodo para realizar el analisis paralelo y secuencial
        public static ParallelAnalysisResult AnalizarLogsParalelo(LogsResult logs)
        {
            // Ejecución secuencial
            var swSec = Stopwatch.StartNew();
            int erroresSec = logs.Errors.Count;
            int warningsSec = logs.Warnings.Count;
            int infosSec = logs.Infos.Count;
            swSec.Stop();

            // Ejecucion paralela
            var swPar = Stopwatch.StartNew();
            int erroresPar = 0, warningsPar = 0, infosPar = 0;
            object locker = new object();

            var todasLasEntradas = new List<LogEntry>();
            todasLasEntradas.AddRange(logs.Errors);
            todasLasEntradas.AddRange(logs.Warnings);
            todasLasEntradas.AddRange(logs.Infos);

            Parallel.ForEach(
                todasLasEntradas,
                () => (errores: 0, warnings: 0, infos: 0),
                (log, state, local) =>
                {
                    if (log.Tipo.Contains("ERROR")) local.errores++;
                    else if (log.Tipo.Contains("WARNING")) local.warnings++;
                    else if (log.Tipo.Contains("INFO")) local.infos++;
                    return local;
                },
                localFinal =>
                {
                    lock (locker)
                    {
                        erroresPar += localFinal.errores;
                        warningsPar += localFinal.warnings;
                        infosPar += localFinal.infos;
                    }
                });

            swPar.Stop();

            // Metricas
            double speedup = (double)swSec.ElapsedMilliseconds / swPar.ElapsedMilliseconds;
            double eficiencia = speedup / Environment.ProcessorCount;

            return new ParallelAnalysisResult
            {
                TotalErrors = erroresPar,
                TotalWarnings = warningsPar,
                TotalInfos = infosPar,
                Speedup = speedup,
                Eficiencia = eficiencia,
                TiempoSecuencialMs = swSec.ElapsedMilliseconds,
                TiempoParaleloMs = swPar.ElapsedMilliseconds
            };
        }
    }
}