using Microsoft.AspNetCore.Mvc;
using FinalParalela.Models;
//using Renci.SshNet;
using System.IO;


//declaramos el namespace al que pertenece el controlador
namespace FinalParalela.Controllers;

// Definimos un controlador para manejar las peticiones SSH
[ApiController]
[Route("api/ssh")]
public class SshController : ControllerBase
{
    //Definimos un endpoint para ejecutar comandos SSH y hacer la autenticacio
    [HttpPost("run")]
    public IActionResult Run([FromBody] SSHConnectionDto dto) //dto es el objeto que contiene los datos de la conexion, estos los recoge del Body de la peticion
    {
        // Comando que vamos a ejecutar en el servidor
        var cmdText = $"powershell -NoProfile -Command Get-EventLog -LogName System |Select-Object TimeGenerated, EntryType, Source, EventID, Message |Export-Csv -Path 'system_log.csv' -NoTypeInformation -Encoding UTF8";

        // Esto será algo como: C:\Users\pepito
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string rutaCSV = Path.Combine(userProfile, "archivo.csv");

        //Obteniendo ruta relativa del proyecto
        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        
        // Crea una carpeta data dentro del proyecto
        string localDir = Path.Combine(projectRoot, "data");

        string usuarioActual = Environment.UserName;

        Directory.CreateDirectory(localDir);

        string localPath = Path.Combine(localDir, "archivo.csv");
        //podemos colocar el comando de get event de una vez

        // Creamos el objeto de conexion con los datos del DTO, este objeto es el que espera Renci.SshNet.SshClient para crear la conexion
        var info = new Renci.SshNet.PasswordConnectionInfo(dto.Host, dto.Port, dto.User, dto.Password)
        {
            //hacemos un timeout para no tumbar la peticion actual, en el caso de que esto duarara mas de 30 segundos
            Timeout = TimeSpan.FromSeconds(30)
        };

        //creamos el cliente ssh para conectarnos al servidor
        using var client = new Renci.SshNet.SshClient(info);
        // configuramos un intervalo de 15 para mantener la conexion activa
        client.KeepAliveInterval = TimeSpan.FromSeconds(15);
        // nos conectamos al servidor ssh
        client.Connect();

        //ejecutamos el comando en el servidor directamente
        var cmd = client.RunCommand(cmdText);

        var scp = client.RunCommand($"scp {usuarioActual}@localhost:\"{localDir}\" ./\r\n");
        
        //obtenemos el resultado del comando que se ejecuto
                var result = new
        {
            exitStatus = cmd.ExitStatus ?? -1,        // -1 si el server no devolvió código
            stdout = cmd.Result ?? string.Empty,
            stderr = cmd.Error ?? string.Empty
        };
        //cerramos la conexion al servidor 
        client.Disconnect();


        // Crear servidor FTP o SFTP para guardar el LOG en el proyecto o usar directamente scp


        // Abrir el archivo con la ruta relativa del LOG generado dentro del proyecto

        // Hacer analisis paralelo/secuencial de los logs generados del servidor

       
        //retoramos el resultado de la respuesta
        return Ok(result);
    }
}
