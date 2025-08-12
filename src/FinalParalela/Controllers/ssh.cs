using FinalParalela.Models;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using System.IO;
using FinalParalela.Services;
namespace FinalParalela.Controllers;

[ApiController]
[Route("api/ssh")]
public class SshController : ControllerBase
{
    [HttpPost("run")]
    public IActionResult Run([FromBody] SSHConnectionDto dto) //parametros que esperamos en el body
    {
        //Generamos el csv en el usuario que se haya accedido remotamente
        var cmdText =
            "powershell -NoProfile -Command " +
            "\"$csv = Join-Path $env:USERPROFILE 'system_log.csv'; " +
            "Get-WinEvent -LogName System | " +
            "Select-Object TimeCreated, LevelDisplayName, ProviderName, Id, Message | " +
            "Export-Csv -Path $csv -NoTypeInformation -Encoding UTF8\"";

        // Obtenemos la ruta relativa del proyecto para posteriormente guardar los logs: ./data/system_log.csv
        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string localDir = Path.Combine(projectRoot, "data");
        Directory.CreateDirectory(localDir);//
        string localPath = Path.Combine(localDir, "system_log.csv");

        // creamos el objeto de conexion SSH
        var info = new PasswordConnectionInfo(dto.Host, dto.Port, dto.User, dto.Password)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        //creamos el cliente ssh para hacer la conexion con la informacion obtenida
        using var client = new SshClient(info);
        //mantenemos la conexion activa para evitar que nos de un timeout y se cierre
        client.KeepAliveInterval = TimeSpan.FromSeconds(15);
        //nos conectamos al ciente creado con la informacion
        client.Connect();

        //ejecutamos el comando 
        var cmd = client.RunCommand(cmdText);



        //  Obtenemos la ruta al acceder por ssh, para construir la ruta SFTP 
        var remoteUserProfile = client.RunCommand("powershell -NoProfile -Command \"$env:USERPROFILE\"")
                                      .Result.Trim();
        client.Disconnect(); //cerramos la sesion


        //Ruta remota en formato valido para SFTP en Windows OpenSSH
        var remotePath = $"{remoteUserProfile.Replace('\\', '/')}/system_log.csv";
        if (remotePath.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
            remotePath = "/" + remotePath; // esto devuelve => /C:/Users/usuario/system_log.csv

        // descargamos el archivo por sftp
        using (var sftp = new SftpClient(info)) //pasamos la informacion del cliente
        {
            sftp.Connect();//nos conectamos 
            using var fs = System.IO.File.Create(localPath); //creamos el archivo
            sftp.DownloadFile(remotePath, fs);//comenzamos la descarga, pasandole la instancia del IO del archivo
            sftp.Disconnect(); //nos desconectamos
        }

        // Sanitizamos los logs
        var logsResult = LogsService.SanitizeLogs(localDir);


        //analisi paralelo
        return Ok();
    }
}