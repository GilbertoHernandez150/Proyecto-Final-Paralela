function renderCharts(){

    const data =  JSON.parse(sessionStorage.getItem("analisis"));
    //grafico de comparacion de speedup y eficiencia
    const ctx1 = document.getElementById('performanceChart').getContext('2d');
    new Chart(ctx1, {
        type: 'bar',
        data: {
            labels: ['Speedup', 'Eficiencia'],
            datasets: [{
                label: 'Métricas',
                data: [data.speedup, data.eficiencia],
                backgroundColor: ['#22c55e', '#f59e0b']
            }]
        },
        options: { responsive: true }
    });

    //grafico de tiempos
    const ctx2 = document.getElementById('timeChart').getContext('2d');
    new Chart(ctx2, {
        type: 'bar',
        data: {
            labels: ['Secuencial', 'Paralelo'],
            datasets: [{
                label: 'Tiempo (ms)',
                data: [data.tiempoSecuencialMs, data.tiempoParaleloMs],
                backgroundColor: ['#ef4444', '#22c55e']
            }]
        },
        options: { responsive: true }
    })
}


function setValueHtmlFields(){
    const data =  JSON.parse(sessionStorage.getItem("analisis"));

    const totalErrors =  document.getElementById('total-errors');
    const warnings = document.getElementById('error-warning');
    const info = document.getElementById('error-info');
    const speedup = document.getElementById('speedup');
    const eficiencia = document.getElementById('eficiencia');
    const msSeq =  document.getElementById('msSecuencial');
    const msPar =  document.getElementById('msParalelo');

    //seteamos los valores a los compos correspondientes
    totalErrors.innerHTML = data.totalErrors;
    warnings.innerHTML = data.totalWarnings;
    info.innerHTML =  data.totalInfos;
    speedup.innerHTML = data.speedup;
    eficiencia.innerHTML = data.eficiencia
    msSeq.innerHTML = ${data.tiempoSecuencialMs} MS;
    msPar.innerHTML =  ${data.tiempoParaleloMs} MS;
}

function initializeDashboard(){
    renderCharts();
    setValueHtmlFields();
}

initializeDashboard();