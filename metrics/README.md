# Métricas de Análisis Paralelo de Logs

Esta carpeta contiene los resultados del **análisis comparativo de rendimiento** entre ejecución **secuencial** y **paralela** aplicado a distintos tamaños de logs procesados remotamente.

## Contenido
- **metrics_logs.xlsx** → Documento en Excel con:
  - Tabla comparativa de resultados (errores, warnings, infos, tiempos).
  - Gráficos de:
    - Tiempo Secuencial vs Paralelo.
    - Speedup obtenido.
    - Eficiencia por tamaño de log.

## Variables medidas
- **Peso del log** → Tamaño de cada archivo procesado.
- **Errores / Warnings / Infos** → Cantidad de mensajes procesados en el log.
- **Tiempo Secuencial (ms)** → Tiempo en milisegundos usando un solo hilo/proceso.
- **Tiempo Paralelo (ms)** → Tiempo en milisegundos usando paralelismo.
- **Speedup** → Relación entre tiempo secuencial y paralelo.
  - `Speedup = TiempoSecuencial / TiempoParalelo`
- **Eficiencia** → Aprovechamiento del paralelismo.
  - `Eficiencia = Speedup / N` (N = número de hilos/núcleos utilizados).

---

