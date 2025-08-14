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
    public IActionResult Run([FromBody] SSHConnectionDto dto)
    {
        // Comando remoto que genera un CSV de eventos del sistema en el perfil del usuario
        var cmdText =
            "powershell -NoProfile -Command " +
            "\"$csv = Join-Path $env:USERPROFILE 'system_log.csv'; " +
            "Get-WinEvent -LogName System | " +
            "Select-Object TimeCreated, LevelDisplayName, ProviderName, Id, Message | " +
            "Export-Csv -Path $csv -NoTypeInformation -Encoding UTF8\"";

        // Ruta local donde guardaremos el archivo descargado
        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string localDir = Path.Combine(projectRoot, "data");
        Directory.CreateDirectory(localDir);
        string localPath = Path.Combine(localDir, "system_log.csv");

        // Información de conexión SSH
        var info = new PasswordConnectionInfo(dto.Host, dto.Port, dto.User, dto.Password)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Conexión SSH para ejecutar el comando
        using var client = new SshClient(info);
        client.KeepAliveInterval = TimeSpan.FromSeconds(15);
        client.Connect();
        client.RunCommand(cmdText);

        // Obtener ruta remota del archivo generado
        var remoteUserProfile = client.RunCommand("powershell -NoProfile -Command \"$env:USERPROFILE\"")
                                      .Result.Trim();
        client.Disconnect();

        // Ajustar formato de ruta para SFTP
        var remotePath = $"{remoteUserProfile.Replace('\\', '/')}/system_log.csv";
        if (remotePath.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
            remotePath = "/" + remotePath;

        // Descarga del archivo vía SFTP
        using (var sftp = new SftpClient(info))
        {
            sftp.Connect();
            using var fs = System.IO.File.Create(localPath);
            sftp.DownloadFile(remotePath, fs);
            sftp.Disconnect();
        }

        // Sanitización de logs
        var logsResult = LogsService.SanitizeLogs(localPath);

        // Análisis paralelo aplicando descomposición de datos
        var analisis = LogsService.AnalizarLogsParalelo(logsResult);

        // Retornar resultados
        return Ok(new
        {
            Mensaje = "Análisis paralelo completado con éxito.",
            Resultado = analisis
        });
    }
}