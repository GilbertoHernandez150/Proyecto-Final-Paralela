namespace FinalParalela.Models;

//Definimos un DTO para la representacion de los datos de autenticacion SSH 
public class SSHConnectionDto
{
    //IP o Hostname al que nos queremos conectar
    public string Host { get; set; } = default!;
    //Puerto al que nos queremos conectar por ssh, por defecto se usa el 22
    public int Port { get; set; } = 22;
    //Usuario para la conexion SSH
    public string User { get; set; } = default!;
    //Password para el user de la conexion SSH
    public string Password { get; set; } = default!;
    //Comando a ejecutar en el servidor, por defecto, haremos un dir 
    // TO DO: Validar si sera necesario o solo utilizaremos el comando por defecto de obtener los logs del systema
    public string Command { get; set; } = "dir";
}