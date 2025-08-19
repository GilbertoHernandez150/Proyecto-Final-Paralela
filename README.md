# Análisis Paralelo de Logs del Sistema a través de Conexión Remota  

## Descripción General  
Este proyecto consiste en el desarrollo de un sistema que permite conectarse a dispositivos remotos (Windows), ejecutar comandos o scripts para generar archivos de logs del sistema, obtener dichos archivos y analizarlos mediante técnicas de procesamiento paralelo.  

El objetivo principal es identificar eventos relevantes (errores, advertencias e información general), clasificarlos y generar métricas de rendimiento que permitan interpretar el comportamiento de los sistemas remotos.  

El sistema se compone de:  
- Backend (API REST): desarrollado en ASP.NET Core, encargado de gestionar la conexión remota, procesamiento de logs y exposición de resultados.  
- Frontend (Index y Dashboard Web): construido con HTML, CSS y JavaScript, para la visualización de métricas, gráficas y clasificación de eventos.  

---

## Objetivos  
- Conectarse a dispositivos remotos para obtener logs del sistema.  
- Analizar los logs de manera paralela para mejorar rendimiento.  
- Clasificar los eventos en Errores, Advertencias e Información.  
- Generar métricas y estadísticas para la interpretación de resultados.  
- Comparar desempeño entre ejecución secuencial y paralela.  
- Proyectar los resultados en un dashboard web interactivo.  

---

## Funcionalidades Clave  
- Ejecución simultánea de múltiples tareas:  
  Los logs se dividen en bloques que son procesados en paralelo con técnicas como Parallel.For y tareas asincrónicas.  

- Datos compartidos entre tareas:  
  Los resultados parciales (conteos de errores, advertencias y eventos) son integrados mediante estructuras de datos compartidas con mecanismos de sincronización.  

- Exploración de estrategias de paralelización:  
  Comparación entre diferentes métodos de división: por líneas, por secciones o por tipo de evento.  

- Escalabilidad:  
  Diseñado para manejar archivos grandes y múltiples dispositivos, aprovechando más recursos disponibles (núcleos/hilos adicionales).  

- Métricas de evaluación:  
  - Tiempo de ejecución secuencial vs paralelo.  
  - Uso de CPU.  
  - Speedup y eficiencia.  

- Proyección de resultados en la web:  
  El frontend muestra gráficas dinámicas y tablas interactivas con los eventos analizados, incluyendo:  
  - Conteo de Errores, Advertencias e Información.  
  - Distribución de eventos en gráficas de barras y pastel.  
  - Comparación visual entre ejecución secuencial y paralela.  

---

## Tecnologías Utilizadas  
### Backend  
- ASP.NET Core (C#) → API para análisis y gestión de logs.  

### Frontend  
- HTML5 → estructura de la interfaz.  
- CSS3 → estilos y diseño responsivo.  
- JavaScript (Vanilla JS) → consumo de la API y visualización dinámica de datos.  

### Otros  
- Tareas paralelas y asincrónicas (Parallel.For, Task, async/await).  
- Estructuras concurrentes para sincronización y combinación de resultados.  
- Librerías JS para visualización de datos.  

---

## Ejecución del Proyecto  

### 1. Clonar el repositorio  
```bash
git clone https://github.com/GilbertoHernandez150/Proyecto-Final-Paralela
cd Proyecto-Final-Paralela
