
const sshinfo = document.getElementById("ssh-info");
const sshData = JSON.parse(localStorage.getItem("ssh-sentinel:last-conn"));
//le colocamos la info de la conexion que se realizo
sshinfo.innerText =  `Host: ${sshData.host} Puerto: ${sshData.port} Usuario: ${sshData.user} Cores Utilizados:${sshData.cores}`
//funcion para retornar el analisis parseado
function getAnalysisData(){
    try{

        return JSON.parse(sessionStorage.getItem("analisis"));
    }
    catch{
        return null; //retornamos null ya que no hay ningun valor, podriamos retornar un objeto vacio pero luego serian mas validaciones
    }
}

//validaremos si tenemos la data del analisis
function validateAnalysisData(){
  const data =  getAnalysisData();
    if(!data){
        const WEB_SERVER_URL = "/src/frontend/";
        alert("No pudimos obtener el analisis, te estaremos redireccionando al index nuevamente")
        window.location.href = `${WEB_SERVER_URL}index.html`;
    };

    //simulacion de que estamos cargando la informacion
    setTimeout(()=>{
        console.log("Te estaremos redireccionado en 5 segundos")
        
        //seteamos ahora el dashboard ya que si recibimos info
        const dashboard =  document.getElementById("dashboard")
        dashboard.style.display = "block";

        // ocultamos el loading DESPUÉS de mostrar el dashboard
        const loading =  document.getElementById("loadingOverlay");
        loading.style.display = "none";

    }, 3000)
   
   
}
function renderCharts(){
     const data =  getAnalysisData();
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
    type: 'doughnut',
    data: {
        labels: ['Secuencial', 'Paralelo'],
        datasets: [{
            label: 'Tiempo (ms)',
            data: [data.tiempoSecuencialMs, data.tiempoParaleloMs],
            backgroundColor: ['#3b82f6', '#8b5cf6'], // azul y púrpura
            borderWidth: 1
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'bottom'
            }
        }
    }
});
}


function setValueHtmlFields(){
     const data =  getAnalysisData();

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
    msSeq.innerHTML = `${data.tiempoSecuencialMs} MS`;
    msPar.innerHTML =  `${data.tiempoParaleloMs} MS`;
}
//inicializa la informacion que estara dentro del dashboard
function initializeDashboard(){
    renderCharts();//renderizara los graficos
    setValueHtmlFields(); //va a setear los valores tipo speedup, eficiencia y etc
}
validateAnalysisData();
initializeDashboard();