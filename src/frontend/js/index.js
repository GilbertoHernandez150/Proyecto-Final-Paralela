const ASP_BACKEND_PORT = 7082;

document.addEventListener("DOMContentLoaded", async () => {
      try {
        //  hacemos la soli
        const response = await fetch(`https://localhost:${ASP_BACKEND_PORT}/api/cores`);
        const data = await response.json();

        // seleccionamos el input del core
        const input = document.getElementById("cores");

        // Ponemos la cantidad de procesadores disponibles
        input.placeholder = `Tienes disponible ${data.systemCores}`; 
        input.value = data.systemCores; 

      } catch (error) {
        console.error("No se pudo setear los cores:", error);
      }
    });

document.getElementById('analysisForm').addEventListener('submit', async function (e) {
    //evitamos que se reinicien los campos del form
    e.preventDefault();

    const btn = document.getElementById('analyzeBtn');
    const statusMessage = document.getElementById('statusMessage');

    // Show loading state
    btn.innerHTML = '<span class="loading"></span>Analizando...';
    //desactivamos el boton para evitar errores al dar click
    btn.disabled = true;
    statusMessage.style.display = 'none';

    // obtenemos la informacion del form
    const formData = {
        host: document.getElementById('host').value,
        port: document.getElementById('port').value,
        user: document.getElementById('user').value,
        password: document.getElementById('password').value,
        cores: document.getElementById('cores').value
    };
    
    //Hacemos la solicitud con los datos para realizar el analisis
    try {
        //hacemos un await para no detener cualquier otra accion hasta obtener la info del analisis
        const response = await fetch(`https://localhost:${ASP_BACKEND_PORT}/api/ssh/run`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData) //parseamos la info a json para que se pueda reconocer en el body
        });

        //verificamos el estatus de la respuesta y mostramos el error
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        
        console.log("Esta es la respuesta de la data", data)
        // mostramos un mensaje de completacion
        statusMessage.textContent = data.mensaje || 'Análisis completado exitosamente';
        statusMessage.className = 'status-message status-success';
        statusMessage.style.display = 'block';
        
        //guardamos la info del analisis 
        sessionStorage.setItem("analisis",JSON.stringify(data.resultado))

        //aqui seteamos un interval para hacer la redireccion al dashboard
        setTimeout(()=>{
            // console.log("Te redireccionaremos al dashboard!!!");
            //hacemos la redireccion
            const WEB_SERVER_URL = "/src/frontend/";
            window.location.href =  `${WEB_SERVER_URL}dashboard.html`;
            alert("Haremos la redireccion")
        },1)

        
    } catch (error) {
        console.error('Error:', error);
        statusMessage.textContent = 'Error al realizar el análisis: ' + error.message;
        statusMessage.className = 'status-message status-error';
        statusMessage.style.display = 'block';
    } finally {
        // Reset button
        btn.innerHTML = 'Iniciar Análisis';
        btn.disabled = false;
    }
});

// Saludo por horario
(function greet() {
    const h = new Date().getHours();
    const t = document.getElementById('greetingTitle');
    if (!t) return;
    let msg = '¡Bienvenido!';
    if (h >= 5 && h < 12) msg = '¡Buenos días!';
    else if (h >= 12 && h < 19) msg = '¡Buenas tardes!';
    else msg = '¡Buenas noches!';
    t.textContent = msg;
})();

// Mostrar / Ocultar contraseña
(function togglePassword() {
    const btn = document.getElementById('togglePass');
    const input = document.getElementById('password');
    if (!btn || !input) return;
    btn.addEventListener('click', () => {
        const isPass = input.type === 'password';
        input.type = isPass ? 'text' : 'password';
    });
})();

// Recordar host/port/user en localStorage (no guardamos password)
(function rememberFields() {
    const host = document.getElementById('host');
    const port = document.getElementById('port');
    const user = document.getElementById('user');
    const key = 'ssh-sentinel:last-conn';

    // Cargar
    try {
        const last = JSON.parse(localStorage.getItem(key) || '{}');
        if (last.host) host.value = last.host;
        if (last.port) port.value = last.port;
        if (last.user) user.value = last.user;
    } catch { }

    // Guardar al enviar
    document.getElementById('analysisForm').addEventListener('submit', () => {
        const data = { host: host.value.trim(), port: port.value.trim(), user: user.value.trim() };
        localStorage.setItem(key, JSON.stringify(data));
    });
})();