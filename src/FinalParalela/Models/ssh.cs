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
    //cores utilizados para hacer el analisis, por defecto tomara todos los del sistema
    public int Cores { get; set; } = Environment.ProcessorCount;
}