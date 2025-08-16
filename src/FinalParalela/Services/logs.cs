using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
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
        public static ParallelAnalysisResult ProcesarLogsSecuencialVsParalelo(string path)
        { 
            //Ver si el archivo existe en la ruta recibida
            if (!File.Exists(path))
                //retornamos el objeto vacio
                return new ParallelAnalysisResult();

            // Iniciamos la ejecucion secuencial
            var swSec = Stopwatch.StartNew();

            // Creamos listas para cada tipo de log, haciendo instancia de LogEntry para la representacion
            var secErrors = new List<LogEntry>();
            var secWarnings = new List<LogEntry>();
            var secInfos = new List<LogEntry>();

            //iteramos de manera secuencial sobre cada linea del archivo csv
            foreach (var linea in File.ReadLines(path).Skip(1)) 
            {
                //separamos cada linea por comas
                var parts = linea.Split(',');

                // validamos si la linea tiene 3 partes
                if (parts.Length >= 3)
                {
                    // intentamos convertir la fecha con el formato esperado
                    DateTime fecha;
                    if (!DateTime.TryParseExact(parts[0].Trim('"'),
                            "M/d/yyyy h:mm:ss tt",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out fecha))
                        fecha = DateTime.MinValue;

                    //almacenamos el tipo correspondiente en cada variable, le quitamos las comillas para evitar que salga '"MENSAJE"'
                    var tipo = parts[1].Trim('"').ToUpper();
                    var mensaje = parts[2].Trim('"');
                    var entry = new LogEntry { Fecha = fecha, Tipo = tipo, Mensaje = mensaje };

                    //segun el tipo del log, lo agregamo a la lista de errores correspondientes de manera secuencial
                    if (tipo.Contains("ERROR")) secErrors.Add(entry);
                    else if (tipo.Contains("WARNING")) secWarnings.Add(entry);
                    else if (tipo.Contains("INFO")) secInfos.Add(entry);
                }
            }

            //asignamos la cantidad de errores a las variables para el secuencial
            int erroresSec = secErrors.Count;
            int warningsSec = secWarnings.Count;
            int infosSec = secInfos.Count;

            //detenemos el tiempo para calcular la ejecucion secuencial

            swSec.Stop();

            // iniciamos la ejecucion paralela
            var swPar = Stopwatch.StartNew();
            //creamos la listas para guardar los resultados despues de la ejecucion paralela
            var parErrors = new List<LogEntry>();
            var parWarnings = new List<LogEntry>();
            var parInfos = new List<LogEntry>();

            //creamos los objetos de bloque para que se procese luego de manera paralela

            object lockErrors = new object();
            object lockWarnings = new object();
            object lockInfos = new object();

            //leenmos las lineas del archivo
            var lineas = File.ReadAllLines(path).Skip(1).ToArray();
            //iteramos de manera paralela sobre cada linea del archivo 
            Parallel.ForEach(
                Partitioner.Create(0, lineas.Length), //dividimos el trabajo en particiones oara procesarlo en paralelo
                range =>
                {
                    //creamos listas locales para los tipos de logs
                    var localErrors = new List<LogEntry>();
                    var localWarnings = new List<LogEntry>();
                    var localInfos = new List<LogEntry>();

                    //iteramos sobre el rango obtenido
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        //dividmos cada linea para obtener cada uno de los valores del row del csv
                        var parts = lineas[i].Split(',');

                        //verificamos si la tiene al menos las 3 partes del log requerido
                        if (parts.Length >= 3)
                        {
                            //parseamos la fecha con el formato esperado 
                            DateTime fecha;
                            if (!DateTime.TryParseExact(parts[0].Trim('"'),
                                    "M/d/yyyy h:mm:ss tt",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out fecha))
                                fecha = DateTime.MinValue;

                            //almacenamos el tipo de valor correspondiente en cada variable y le quitamos las commillas
                            var tipo = parts[1].Trim('"').ToUpper();
                            var mensaje = parts[2].Trim('"');
                            var entry = new LogEntry { Fecha = fecha, Tipo = tipo, Mensaje = mensaje };

                            //segun el tipo del log, lo agregamos a la lista de errores correspondientes de manera paralela
                            if (tipo.Contains("ERROR")) localErrors.Add(entry);
                            else if (tipo.Contains("WARNING")) localWarnings.Add(entry);
                            else if (tipo.Contains("INFO")) localInfos.Add(entry);
                        }
                    }

                    // hacemos un lock para agregar los resultados de manera segura dentro de la ejecucion para evitar race condition en los datos
                    lock (lockErrors) parErrors.AddRange(localErrors);
                    lock (lockWarnings) parWarnings.AddRange(localWarnings);
                    lock (lockInfos) parInfos.AddRange(localInfos);
                });
            
            // asginamos la cantidad de errores a las variables para el paralelo
            int erroresPar = parErrors.Count;
            int warningsPar = parWarnings.Count;
            int infosPar = parInfos.Count;

            //finalizamos el tiempo para la ejecucion paralela
            swPar.Stop();

            // medimos el speedp y eficiencia, usamos el ternario para evitar divisiones entre cero
            double speedup = swPar.ElapsedMilliseconds > 0 ? (double)swSec.ElapsedMilliseconds / swPar.ElapsedMilliseconds: 0;
            double eficiencia = Environment.ProcessorCount > 0 ? speedup / Environment.ProcessorCount: 0;

            //retornamos el objeto del analisis paralelo realizado
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
        };

        
    
}
