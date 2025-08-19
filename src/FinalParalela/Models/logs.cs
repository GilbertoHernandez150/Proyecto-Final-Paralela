using System;
using System.Collections.Generic;

namespace FinalParalela.Models
{
    // Entrada individual del log
    public class LogEntry
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string Mensaje { get; set; }
    }

    // Contenedor para agrupar las entradas de log
    public class LogsResult
    {
        public List<LogEntry> Errors { get; set; }
        public List<LogEntry> Warnings { get; set; }
        public List<LogEntry> Infos { get; set; }
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
}
