using FinalParalela.Models;
using FinalParalela.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;

namespace FinalParalela.Controllers;

[ApiController]
[Route("api/ssh")]
public class SshController : ControllerBase
{
    [HttpPost("run")]
    public IActionResult Run([FromBody] SSHConnectionDto dto)
    {
     
        // Ruta local donde guardaremos el archivo descargado
        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string localDir = Path.Combine(projectRoot, "data");
        Directory.CreateDirectory(localDir);
        string localPath = Path.Combine(localDir, "system_log.csv");

        try
        {
            // realizamos la conexion ssh generando el archivo y posteriomente lo descargamos para hacer el analisis
            GenerateAndDownloadCsv(dto, localPath);

            // retornamos el analisis genereado por los logs
            var analisis = AnalyzeLogs(localPath, dto.Cores);

            // respuesta del analisis
            return Ok(new
            {
                Mensaje = "Análisis paralelo completado con éxito.",
                Resultado = analisis
            });
        }
        catch (SshAuthenticationException authEx)
        {
            // manejo de error al momento de autenticar, esto puede ser por credenciales incorrectas
            return BadRequest($"Error de autenticación: {authEx.Message}");
        }
        catch (SshConnectionException connEx)
        {
            // problemas al conectar, puede ser xq no reconozca el host, puerto incorrecto etc etc
            return BadRequest($"Error de conexion SSH: {connEx.Message}");
        }
        catch (Exception ex)
        {
            // manejamos cualquier otra excepcion inesperada
            return BadRequest($"Error inesperado: {ex.Message}");
        }




    }


    //declaramos los metodos protected virtual para que al heredar la clase se puedan sobreescribir en los tests que se realizaran
    protected virtual void GenerateAndDownloadCsv(SSHConnectionDto dto, string localPath)
    {
        // comando que utilizamos para generar el csv de los logs del sistema
        var cmdText =
            "powershell -NoProfile -Command " +
            "\"$csv = Join-Path $env:USERPROFILE 'system_log.csv'; " +
            "Get-WinEvent -LogName System | " +
            "Select-Object TimeCreated, LevelDisplayName, ProviderName, Id, Message | " +
            "Export-Csv -Path $csv -NoTypeInformation -Encoding UTF8\"";

        //creamos el objeto de conexion con las credenciales recibidas dentro del dto
        var info = new PasswordConnectionInfo(dto.Host, dto.Port, dto.User, dto.Password)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        

        //inicializamos el cliente SSH con la informacion de conexion
        using var client = new SshClient(info);
        client.KeepAliveInterval = TimeSpan.FromSeconds(15);
        client.Connect();
        //nos conecntamos al servidor por ssh, retornamos un error si no pudimos conectarnos

        if (!client.IsConnected)
            throw new Exception("No se pudo establecer la conexión al servidor.");

        //ejecutamos el comando para que se genere el csv
        client.RunCommand(cmdText); 

        
        //obtenemos la ruta del usuario donde se genero el csv de los logs
        var remoteUserProfile = client.RunCommand("powershell -NoProfile -Command \"$env:USERPROFILE\"")
                                      .Result.Trim();
        //nos desconectamos al ya obtener la ruta
        client.Disconnect();


        // terminamos de construir la ruta del archivo
        var remotePath = $"{remoteUserProfile.Replace('\\', '/')}/system_log.csv";
        if (remotePath.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
            remotePath = "/" + remotePath;


        // hacemos la descarga por sftp, con la misma info de conexion que hicimos con ssh
        using var sftp = new SftpClient(info);
        sftp.Connect();
        //creamos el archivo donde se ira guardando la descarga
        using var fs = System.IO.File.Create(localPath);
        //descargamos el archivo remoto a nuestro sistema local
        sftp.DownloadFile(remotePath, fs);
        sftp.Disconnect();
    }

    // metodo que va a retornar el resultado del analisis de los logs
    protected virtual object AnalyzeLogs(string localPath, int nWorkers)
    { 
        //retornamos el resultado del analisis de los logs
        return LogsService.ProcesarLogsSecuencialVsParalelo(localPath, nWorkers);
    }
}